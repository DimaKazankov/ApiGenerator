# ---- config ----
OPENAPI_FILE := swagger.json
# Dynamically extract tags from the swagger file
TAGS := $(shell python3 extract_tags.py $(OPENAPI_FILE) 2>/dev/null || echo "")

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
.PHONY: all clean og-by-tag nswag-by-tag show-tags help
all: og-by-tag nswag-by-tag

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
	@echo "  show-tags   - Show tags detected in $(OPENAPI_FILE)"
	@echo "  clean       - Remove all generated files"
	@echo "  help        - Show this help"
	@echo ""
	@echo "Configuration:"
	@echo "  OPENAPI_FILE = $(OPENAPI_FILE)"
	@echo "  Detected tags: $(TAGS)"

# ---- Approach A: OpenAPI Generator - Generate separate projects organized by tag namespace  
# Each tag gets its own complete project with all APIs, organized by tag namespace
og-by-tag: $(TAGS:%=og-%)

og-%: $(OPENAPI_FILE)
	@tag=$*; \
	mkdir -p "$(OUT_OG)/$$tag"; \
	$(CGEN) generate \
	  -i "$(OPENAPI_FILE)" \
	  -g csharp \
	  -o "$(OUT_OG)/$$tag" \
	  --global-property "$(CGEN_GLOBAL)" \
	  --additional-properties "$(CGEN_PROPS),packageName=ShopApi.$$tag"; \
	$(PY) remove_comments.py "$(OUT_OG)/$$tag"; \
	# Remove unwanted project files while keeping folder structure \
	rm -f "$(OUT_OG)/$$tag"/*.sln; \
	rm -f "$(OUT_OG)/$$tag"/src/*/ShopApi.*.csproj; \
	rm -rf "$(OUT_OG)/$$tag"/src/*/ShopApi.*.Test; \
	rm -f "$(OUT_OG)/$$tag"/appveyor.yml; \
	rm -rf "$(OUT_OG)/$$tag"/api; \
	rm -rf "$(OUT_OG)/$$tag"/.openapi-generator; \
	rm -rf "$(OUT_OG)/$$tag"/docs

# ---- Approach B: NSwag - Generate separate client files organized by tags
# Each tag gets its own client file with all APIs, but organized by tag namespace
nswag-by-tag: $(TAGS:%=nswag-%)

nswag-%: $(OPENAPI_FILE)
	@tag=$*; \
	mkdir -p "$(OUT_NSWAG)"; \
	PATH="$$PATH:~/.dotnet/tools" $(NSWAG) openapi2csclient \
	  /input:"$(OPENAPI_FILE)" \
	  /output:"$(OUT_NSWAG)/$${tag}Client.cs" \
	  /namespace:ShopApi.$$tag \
	  /operationGenerationMode:MultipleClientsFromOperationId \
	  /GenerateClientInterfaces:true \
	  /GenerateDtoTypes:true \
	  /UseBaseUrl:true \
	  /GenerateOptionalParameters:true \
	  /GenerateContractsOutput:false; \
	$(PY) remove_comments.py "$(OUT_NSWAG)"

