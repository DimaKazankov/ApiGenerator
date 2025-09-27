#!/usr/bin/env python3
"""
Derive a C# base namespace from OpenAPI spec's info.title.
Falls back to 'Api' if title is missing.

Rules:
- Split on non-alphanumeric characters
- PascalCase each token
- If the result doesn't end with 'Api', append 'Api'
"""

import json
import re
import sys


def to_pascal_case(text: str) -> str:
    tokens = re.split(r"[^A-Za-z0-9]+", text or "")
    parts = [t.capitalize() for t in tokens if t]
    return "".join(parts) or "Api"


def derive_base_namespace(swagger_file: str) -> str:
    try:
        with open(swagger_file, "r", encoding="utf-8") as f:
            doc = json.load(f)
    except Exception:
        return "Api"

    title = ((doc.get("info") or {}).get("title") or "").strip()
    base = to_pascal_case(title) if title else "Api"
    if not base.endswith("Api"):
        base = f"{base}Api"
    return base


if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python derive_namespace.py <swagger_file>", file=sys.stderr)
        sys.exit(1)
    print(derive_base_namespace(sys.argv[1]))


