#!/usr/bin/env python3
"""
Script to remove XML documentation comments and block comments from C# files.
Usage: python remove_comments.py <directory_path>
"""

import os
import re
import sys

def remove_comments_from_file(file_path):
    """Remove XML doc comments and block comments from a C# file."""
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
        
        # Remove triple-slash XML comments (/// ...)
        lines = content.splitlines()
        filtered_lines = []
        for line in lines:
            stripped = line.lstrip()
            if not stripped.startswith('///'):
                filtered_lines.append(line)
        
        content = '\n'.join(filtered_lines)
        
        # Remove block comments (/* ... */)
        content = re.sub(r'/\*.*?\*/', '', content, flags=re.DOTALL)
        
        # Remove empty lines that were left after comment removal
        lines = content.splitlines()
        cleaned_lines = []
        prev_empty = False
        for line in lines:
            is_empty = not line.strip()
            if not (is_empty and prev_empty):
                cleaned_lines.append(line)
            prev_empty = is_empty
        
        content = '\n'.join(cleaned_lines)
        
        with open(file_path, 'w', encoding='utf-8') as f:
            f.write(content)
            
    except Exception as e:
        print(f"Error processing {file_path}: {e}")

def process_directory(root_dir):
    """Process all .cs files in the directory recursively."""
    for dirpath, dirnames, filenames in os.walk(root_dir):
        for filename in filenames:
            if filename.endswith('.cs'):
                file_path = os.path.join(dirpath, filename)
                remove_comments_from_file(file_path)
                print(f"Processed: {file_path}")

if __name__ == "__main__":
    if len(sys.argv) != 2:
        print("Usage: python remove_comments.py <directory_path>")
        sys.exit(1)
    
    directory = sys.argv[1]
    if not os.path.exists(directory):
        print(f"Directory does not exist: {directory}")
        sys.exit(1)
    
    process_directory(directory)
