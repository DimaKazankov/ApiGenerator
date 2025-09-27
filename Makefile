# ---- config ----
OPENAPI_FILE := swagger.json
# Dynamically extract tags from the swagger file
TAGS := $(shell python3 extract_tags.py $(OPENAPI_FILE) 2>/dev/null || echo "")
# Optional base namespace prefix (override with `make BASE_NS=MyNs`).
# Default is to use the tag name as the root (e.g., Users.Api / Users.Dto)
BASE_NS ?=

# OpenAPI Generator options (C# client)
CGEN := npx --yes @openapitools/openapi-generator-cli
CGEN_PROPS := nullableReferenceTypes=true,validatable=false,hideGenerationTimestamp=true,nonPublicApi=false
# Disable only documentation files
CGEN_GLOBAL := apiDocs=false,modelDocs=false,apiTests=false,modelTests=false

# NSwag CLI
NSWAG := nswag

# AutoRest CLI
AUTOREST := npx --yes @autorest/autorest

# Output roots
OUT_ROOT := Generated
OUT_OG := $(OUT_ROOT)/openapi-generator
OUT_NSWAG := $(OUT_ROOT)/nswag
OUT_AUTOREST := $(OUT_ROOT)/autorest

# Python used to (a) slice spec by tag, (b) strip XML comments from C#.
PY := python3

# ---- top-level ----
.PHONY: all clean og-by-tag nswag-by-tag show-tags help install-nswag install-openapi-generator
all: og-by-tag nswag-by-tag

# Install NSwag CLI tool if not already installed
install-nswag:
	@export PATH="$$PATH:$$HOME/.dotnet/tools"; \
	if ! command -v nswag >/dev/null 2>&1; then \
		echo "Installing NSwag CLI tool..."; \
		dotnet tool install --global NSwag.ConsoleCore; \
	else \
		echo "NSwag CLI tool is already installed"; \
	fi

# Install/verify OpenAPI Generator CLI tool
install-openapi-generator:
	@echo "Verifying OpenAPI Generator CLI tool..."
	@$(CGEN) version || echo "OpenAPI Generator will be installed automatically on first use"

clean:
	@rm -rf $(OUT_ROOT)

# Show detected tags from the swagger file
show-tags:
	@echo "Detected tags in $(OPENAPI_FILE):"
	@if [ -n "$(TAGS)" ]; then \
		for tag in $(TAGS); do echo "  - $$tag"; done; \
	else \
		echo "  No tags found or error reading file"; \
	fi

# Show help
help:
	@echo "Available targets:"
	@echo "  all         - Generate code using both approaches"
	@echo "  og-by-tag   - Generate code using OpenAPI Generator (one project per tag)"
	@echo "  nswag-by-tag - Generate code using NSwag (one client file per tag)"  
	@echo "  install-nswag - Install NSwag CLI tool if not already installed"
	@echo "  install-openapi-generator - Verify OpenAPI Generator CLI tool"
	@echo "  show-tags   - Show tags detected in $(OPENAPI_FILE)"
	@echo "  clean       - Remove all generated files"
	@echo "  help        - Show this help"
	@echo ""
	@echo "Configuration:"
	@echo "  OPENAPI_FILE = $(OPENAPI_FILE)"
	@echo "  BASE_NS      = $(BASE_NS)  (default: use tag-root like Users.Api/Users.Dto)"
	@echo "  Detected tags: $(TAGS)"

# ---- Approach A: OpenAPI Generator - Generate separate projects organized by tag namespace  
# Each tag gets its own complete project with all APIs, organized by tag namespace
og-by-tag: $(TAGS:%=og-%)

og-%: $(OPENAPI_FILE)
	@tag=$*; \
	mkdir -p "$(OUT_OG)/$$tag"; \
	mkdir -p "$(OUT_ROOT)/filtered"; \
	filtered_spec=$$($(PY) filter_swagger.py "$(OPENAPI_FILE)" "$(OUT_ROOT)/filtered" "$$tag"); \
	if [ -z "$$filtered_spec" ]; then echo "Failed to filter spec for tag $$tag"; exit 1; fi; \
	if [ -n "$(BASE_NS)" ]; then pkg_ns="$(BASE_NS).$$tag"; else pkg_ns="$$tag"; fi; \
	$(CGEN) generate \
	  -i "$$filtered_spec" \
	  -g csharp \
	  -o "$(OUT_OG)/$$tag" \
	  --global-property "$(CGEN_GLOBAL)" \
	  --additional-properties "$(CGEN_PROPS),packageName=$$pkg_ns"; \
	$(PY) remove_comments.py "$(OUT_OG)/$$tag"; \
	# Remove unwanted project files while keeping folder structure \
	rm -f "$(OUT_OG)/$$tag"/*.sln; \
	rm -f "$(OUT_OG)/$$tag"/src/*/*.csproj; \
	rm -rf "$(OUT_OG)/$$tag"/src/*/*Test*; \
	rm -f "$(OUT_OG)/$$tag"/appveyor.yml; \
	rm -rf "$(OUT_OG)/$$tag"/api; \
	rm -rf "$(OUT_OG)/$$tag"/.openapi-generator; \
	rm -rf "$(OUT_OG)/$$tag"/docs

# ---- Approach B: NSwag - Generate separate folders per tag with separated responsibilities
# Each tag gets its own folder with Client/ and Models/ subfolders for better organization
nswag-by-tag: install-nswag $(TAGS:%=nswag-%)

nswag-%: $(OPENAPI_FILE)
	@tag=$*; \
	mkdir -p "$(OUT_NSWAG)/$$tag/Client"; \
	mkdir -p "$(OUT_NSWAG)/$$tag/Models"; \
	mkdir -p "$(OUT_ROOT)/filtered"; \
	filtered_spec=$$($(PY) filter_swagger.py "$(OPENAPI_FILE)" "$(OUT_ROOT)/filtered" "$$tag"); \
	if [ -z "$$filtered_spec" ]; then echo "Failed to filter spec for tag $$tag"; exit 1; fi; \
	if [ -n "$(BASE_NS)" ]; then client_ns="$(BASE_NS).$$tag.Api"; models_ns="$(BASE_NS).$$tag.Dto"; else client_ns="$$tag.Api"; models_ns="$$tag.Dto"; fi; \
	export PATH="$$PATH:$$HOME/.dotnet/tools"; \
	echo "Generating client interfaces and implementations for $$tag..."; \
	$(NSWAG) openapi2csclient \
	  /input:"$$filtered_spec" \
	  /output:"$(OUT_NSWAG)/$$tag/Client/$${tag}Client.cs" \
	  /namespace:$$client_ns \
	  /operationGenerationMode:MultipleClientsFromOperationId \
	  /GenerateClientInterfaces:true \
	  /GenerateDtoTypes:false \
	  /UseBaseUrl:true \
	  /GenerateOptionalParameters:true \
	  /GenerateContractsOutput:false; \
	echo "Generating DTOs/Models for $$tag..."; \
	$(NSWAG) openapi2csclient \
	  /input:"$$filtered_spec" \
	  /output:"$(OUT_NSWAG)/$$tag/Models/$${tag}Models.cs" \
	  /namespace:$$models_ns \
	  /operationGenerationMode:SingleClientFromOperationId \
	  /GenerateClientInterfaces:false \
	  /GenerateClientClasses:false \
	  /GenerateDtoTypes:true \
	  /UseBaseUrl:false \
	  /GenerateOptionalParameters:false \
	  /GenerateContractsOutput:false; \
	$(PY) remove_comments.py "$(OUT_NSWAG)/$$tag"

