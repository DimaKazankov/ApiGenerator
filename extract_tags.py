#!/usr/bin/env python3
"""
Script to extract tags from OpenAPI/Swagger spec file.
Usage: python extract_tags.py <swagger_file>
Outputs: space-separated list of tag names
"""

import json
import sys

def extract_tags(swagger_file):
    """Extract all tags from the swagger file."""
    try:
        with open(swagger_file, 'r', encoding='utf-8') as f:
            doc = json.load(f)
        
        # Get tags from the tags section
        tags_section = doc.get("tags", [])
        tag_names = [tag.get("name") for tag in tags_section if tag.get("name")]
        
        # Also find tags used in operations (in case some aren't in the tags section)
        paths = doc.get("paths", {})
        operation_tags = set()
        
        for path, methods in paths.items():
            for method, operation in methods.items():
                if isinstance(operation, dict) and "tags" in operation:
                    operation_tags.update(operation["tags"])
        
        # Combine both sources and remove duplicates
        all_tags = list(set(tag_names + list(operation_tags)))
        all_tags.sort()  # Sort for consistent output
        
        return all_tags
        
    except Exception as e:
        print(f"Error reading {swagger_file}: {e}", file=sys.stderr)
        return []

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python extract_tags.py <swagger_file>", file=sys.stderr)
        sys.exit(1)
    
    swagger_file = sys.argv[1]
    tags = extract_tags(swagger_file)
    
    if tags:
        print(" ".join(tags))
    else:
        print("No tags found", file=sys.stderr)
        sys.exit(1)
