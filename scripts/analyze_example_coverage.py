#!/usr/bin/env python3
"""
CSharpEssentials - Public API Example Coverage Analyzer

Extracts public APIs (types, methods, properties) from each library
and checks whether they are used in the corresponding example project.
"""

import os
import re
from pathlib import Path
from collections import defaultdict

PROJECT_ROOT = Path(__file__).parent.parent

# Library -> Example directory mapping
LIB_EXAMPLE_MAP = {
    "CSharpEssentials.Any": "Examples.Any",
    "CSharpEssentials.AspNetCore": "Examples.AspNetCore",
    "CSharpEssentials.Clone": "Examples.Clone",
    "CSharpEssentials.Core": "Examples.Core",
    "CSharpEssentials.Entity": "Examples.Entity",
    "CSharpEssentials.EntityFrameworkCore": "Examples.EntityFrameworkCore",
    "CSharpEssentials.Enums": "Examples.Enums",
    "CSharpEssentials.Errors": "Examples.Errors",
    "CSharpEssentials.GcpSecretManager": "Examples.GcpSecretManager",
    "CSharpEssentials.Json": "Examples.Json",
    "CSharpEssentials.Maybe": "Examples.Maybe",
    "CSharpEssentials.RequestResponseLogging": "Examples.RequestResponseLogging",
    "CSharpEssentials.Results": "Examples.Results",
    "CSharpEssentials.Rules": "Examples.Rules",
    "CSharpEssentials.Time": "Examples.Time",
    "CSharpEssentials.Validation": "Examples.Validation",
    "CSharpEssentials": "Examples.Main",
}


def get_cs_files(directory):
    """Returns all .cs files in the directory (excluding bin/obj)."""
    files = []
    for root, dirs, filenames in os.walk(directory):
        # Skip bin/obj directories
        dirs[:] = [d for d in dirs if d not in ('bin', 'obj')]
        for f in filenames:
            if f.endswith('.cs'):
                files.append(os.path.join(root, f))
    return files


def extract_types_and_members(file_path):
    """
    Extracts public type and member names from a C# file.
    Returns: [(category, name, filename), ...]
    category: 'type' | 'method' | 'property' | 'field' | 'enum_value'
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception:
        return []

    results = []
    lines = content.split('\n')

    # Find type names in file (for constructor detection)
    type_names = set()
    type_pattern = re.compile(
        r'^\s*public\s+(?:static\s+|abstract\s+|sealed\s+|readonly\s+|partial\s+)*'
        r'(?:class|struct|interface|enum|record(?:\s+struct)?)\s+(\w+)'
    )
    for line in lines:
        m = type_pattern.match(line)
        if m:
            type_names.add(m.group(1))

    # Parse line by line
    for line in lines:
        stripped = line.strip()
        if not stripped.startswith('public '):
            continue
        if stripped.startswith('public override string ToString'):
            continue

        # --- Enum values ---
        # public const X or plain public enum members
        # Simplified: lines starting with public containing = or ,
        enum_val_match = re.match(r'^\s*public\s+const\s+\w+\s+(\w+)\s*[=;]', stripped)
        if enum_val_match:
            results.append(('field', enum_val_match.group(1), os.path.basename(file_path)))
            continue

        # --- Type definitions ---
        type_match = type_pattern.match(stripped)
        if type_match:
            results.append(('type', type_match.group(1), os.path.basename(file_path)))
            continue

        # --- Methods ---
        # public [static|async|override|virtual|readonly|partial|implicit|explicit] <return_type> Name(<params>)
        # Exclude constructors: Name == type_name with parentheses
        if ' operator ' in stripped:
            continue  # Skip implicit/explicit operators
        method_match = re.match(
            r'^\s*public\s+(?:static\s+|async\s+|override\s+|virtual\s+|abstract\s+|readonly\s+|partial\s+)*'
            r'[\w<>,\[\]\s>?]+?\s+(\w+)(?:<[^>]+>)?\s*\(',
            stripped
        )
        if method_match:
            name = method_match.group(1)
            # Filter out constructors
            if name in type_names:
                continue
            # Skip Object base methods
            if name in ('ToString', 'Equals', 'GetHashCode', 'CompareTo', 'Clone'):
                continue
            # Skip C# keywords
            if name in ('if', 'while', 'for', 'switch', 'using', 'return', 'new', 'await'):
                continue
            results.append(('method', name, os.path.basename(file_path)))
            continue

        # --- Properties ---
        # public [static|override|...] <type> Name { get; set; }
        prop_match = re.match(
            r'^\s*public\s+(?:static\s+|override\s+|virtual\s+|abstract\s+|readonly\s+)*'
            r'[\w<>,\[\]\s>?]+?\s+(\w+)\s*\{',
            stripped
        )
        if prop_match:
            name = prop_match.group(1)
            # Skip indexers
            if name == 'this':
                continue
            results.append(('property', name, os.path.basename(file_path)))
            continue

        # --- Fields ---
        # public [static|readonly] <type> Name = ...;
        field_match = re.match(
            r'^\s*public\s+(?:static\s+|readonly\s+)*[\w<>,\[\]\s>?]+?\s+(\w+)\s*[=;]',
            stripped
        )
        if field_match:
            name = field_match.group(1)
            results.append(('field', name, os.path.basename(file_path)))
            continue

    return results


def read_example_content(example_dir):
    """Concatenates the contents of all .cs files in the example directory."""
    if not os.path.exists(example_dir):
        return ""
    content_parts = []
    for fpath in get_cs_files(example_dir):
        try:
            with open(fpath, 'r', encoding='utf-8') as f:
                content_parts.append(f.read())
        except Exception:
            pass
    return "\n".join(content_parts)


def check_usage(api_name, example_content):
    """
    Checks if the API name is used in the example content.
    Searches for whole-word matches.
    """
    pattern = r'\b' + re.escape(api_name) + r'\b'
    return bool(re.search(pattern, example_content))


def analyze_library(lib_name, example_name):
    """Performs analysis for a library."""
    lib_dir = PROJECT_ROOT / lib_name
    example_dir = PROJECT_ROOT / "examples" / example_name

    if not lib_dir.exists():
        return None

    # Collect all public APIs
    all_apis = []  # [(category, name, file), ...]
    for cs_file in get_cs_files(lib_dir):
        apis = extract_types_and_members(cs_file)
        all_apis.extend(apis)

    # Deduplicate (same name may appear in multiple files)
    seen = set()
    unique_apis = []
    for cat, name, fname in all_apis:
        key = (cat, name)
        if key not in seen:
            seen.add(key)
            unique_apis.append((cat, name, fname))

    # Read example content
    example_content = read_example_content(example_dir)
    has_example = example_content != ""

    # Usage check
    results = []
    for cat, name, fname in unique_apis:
        used = check_usage(name, example_content) if has_example else False
        results.append({
            'category': cat,
            'name': name,
            'file': fname,
            'used': used
        })

    return {
        'library': lib_name,
        'example': example_name,
        'has_example': has_example,
        'apis': results
    }


def generate_markdown_report(analyses):
    """Generates a Markdown report from analysis results."""
    lines = []
    lines.append("# CSharpEssentials - Detailed Example Coverage Report")
    lines.append("")
    lines.append("> This report shows whether each **public API element** (type, method, property, field)")
    lines.append("> is used in the corresponding example project.")
    lines.append(">")
    lines.append("> ✅ = Used in example")
    lines.append("> ❌ = Not used in example")
    lines.append("")

    total_apis = 0
    total_used = 0

    for analysis in analyses:
        if analysis is None:
            continue

        lib = analysis['library']
        apis = analysis['apis']
        has_ex = analysis['has_example']

        used_count = sum(1 for a in apis if a['used'])
        total_count = len(apis)
        total_apis += total_count
        total_used += used_count

        lines.append(f"## {lib}")
        lines.append("")
        if not has_ex:
            lines.append("⚠️ **Example project not found!**")
            lines.append("")
            continue

        lines.append(f"**Coverage:** {used_count}/{total_count} ({used_count*100//total_count if total_count else 0}%)")
        lines.append("")
        lines.append("| Category | API Name | Used in Example |")
        lines.append("|----------|----------|-----------------|")

        # Sort alphabetically
        for api in sorted(apis, key=lambda x: (x['category'], x['name'])):
            icon = "✅" if api['used'] else "❌"
            cat = api['category']
            name = api['name']
            lines.append(f"| {cat} | `{name}` | {icon} |")

        lines.append("")

    # Overall summary
    lines.append("---")
    lines.append("")
    lines.append("## 📊 Overall Example Coverage Summary")
    lines.append("")
    lines.append(f"- **Total Public APIs:** {total_apis}")
    lines.append(f"- **Used in Examples:** {total_used}")
    lines.append(f"- **Coverage:** {total_used*100//total_apis if total_apis else 0}%")
    lines.append("")

    return "\n".join(lines)


def main():
    analyses = []
    for lib_name, example_name in LIB_EXAMPLE_MAP.items():
        print(f"Analyzing {lib_name} ...")
        analysis = analyze_library(lib_name, example_name)
        analyses.append(analysis)

    report = generate_markdown_report(analyses)
    output_path = PROJECT_ROOT / "EXAMPLE_COVERAGE_DETAIL.md"
    with open(output_path, 'w', encoding='utf-8') as f:
        f.write(report)

    print(f"Report generated: {output_path}")


if __name__ == "__main__":
    main()
