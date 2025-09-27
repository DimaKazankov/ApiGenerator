#!/usr/bin/env python3
"""
Script to filter OpenAPI/Swagger spec by tag, keeping only operations with the specified tag.
Usage: python filter_swagger.py <input_file> <output_dir> <tag>
"""

import json
import sys
import os
import copy

def operation_has_tag(operation, target_tag):
    """Check if an operation has the specified tag."""
    tags = operation.get("tags", [])
    return target_tag in tags

def find_schema_refs(schema, used_refs):
    """Recursively find all schema references in the given schema object."""
    if not isinstance(schema, dict):
        return
    
    if "$ref" in schema:
        ref = schema["$ref"]
        if ref.startswith("#/components/schemas/"):
            schema_name = ref.rsplit("/", 1)[1]
            used_refs.add(schema_name)
    
    for value in schema.values():
        if isinstance(value, dict):
            find_schema_refs(value, used_refs)
        elif isinstance(value, list):
            for item in value:
                find_schema_refs(item, used_refs)

def filter_swagger_by_tag(input_file, output_dir, tag):
    """Filter swagger file by tag and save the result."""
    try:
        with open(input_file, 'r', encoding='utf-8') as f:
            doc = json.load(f)
    except Exception as e:
        print(f"Error reading {input_file}: {e}")
        return None
    
    # Create a deep copy of the document
    filtered_doc = copy.deepcopy(doc)
    filtered_doc["paths"] = {}
    
    # Filter paths by tag
    for path, methods in doc.get("paths", {}).items():
        kept_methods = {}
        for method, operation in methods.items():
            if isinstance(operation, dict) and operation_has_tag(operation, tag):
                kept_methods[method] = operation
        
        if kept_methods:
            filtered_doc["paths"][path] = kept_methods
    
    # Find all referenced schemas
    used_schema_refs = set()
    
    for path, methods in filtered_doc.get("paths", {}).items():
        for method, operation in methods.items():
            # Check request body
            request_body = operation.get("requestBody")
            if request_body:
                for media_type in (request_body.get("content") or {}).values():
                    find_schema_refs(media_type.get("schema", {}), used_schema_refs)
            
            # Check responses
            responses = operation.get("responses", {})
            for response in responses.values():
                for media_type in (response.get("content") or {}).values():
                    find_schema_refs(media_type.get("schema", {}), used_schema_refs)
            
            # Check parameters
            parameters = operation.get("parameters", [])
            for param in parameters:
                find_schema_refs(param.get("schema", {}), used_schema_refs)
    
    # Recursively find referenced schemas
    schemas = (doc.get("components") or {}).get("schemas") or {}
    all_used_schemas = set()
    
    def collect_dependent_schemas(schema_name):
        if schema_name in all_used_schemas or schema_name not in schemas:
            return
        all_used_schemas.add(schema_name)
        find_schema_refs(schemas[schema_name], all_used_schemas)
    
    for ref in used_schema_refs:
        collect_dependent_schemas(ref)
    
    # Filter schemas to only include used ones
    if all_used_schemas:
        filtered_doc.setdefault("components", {}).setdefault("schemas", {})
        filtered_doc["components"]["schemas"] = {
            k: v for k, v in schemas.items() if k in all_used_schemas
        }
    
    # Write the filtered document
    output_file = os.path.join(output_dir, f"swagger_{tag}.json")
    try:
        with open(output_file, 'w', encoding='utf-8') as f:
            json.dump(filtered_doc, f, separators=(',', ':'))
        print(output_file)
        return output_file
    except Exception as e:
        print(f"Error writing {output_file}: {e}")
        return None

if __name__ == "__main__":
    if len(sys.argv) != 4:
        print("Usage: python filter_swagger.py <input_file> <output_dir> <tag>")
        sys.exit(1)
    
    input_file, output_dir, tag = sys.argv[1], sys.argv[2], sys.argv[3]
    
    if not os.path.exists(input_file):
        print(f"Input file does not exist: {input_file}")
        sys.exit(1)
    
    if not os.path.exists(output_dir):
        os.makedirs(output_dir)
    
    result = filter_swagger_by_tag(input_file, output_dir, tag)
    if not result:
        sys.exit(1)
