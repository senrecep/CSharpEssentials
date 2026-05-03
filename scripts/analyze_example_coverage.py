#!/usr/bin/env python3
"""
CSharpEssentials - Public API Example Coverage Analyzer
Her kütüphanedeki public API'leri (tip, metod, property) çıkarıp
ilgili example projesinde kullanılıp kullanılmadığını kontrol eder.
"""

import os
import re
from pathlib import Path
from collections import defaultdict

PROJECT_ROOT = Path("/Users/recepsen/Documents/projects/recep/CSharpEssentials")

# Kütüphane -> Example klasörü eşleştirmesi
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
    "CSharpEssentials": "Examples.Main",
}


def get_cs_files(directory):
    """Dizindeki tüm .cs dosyalarını döndürür (bin/obj hariç)."""
    files = []
    for root, dirs, filenames in os.walk(directory):
        # bin/obj klasörlerini atla
        dirs[:] = [d for d in dirs if d not in ('bin', 'obj')]
        for f in filenames:
            if f.endswith('.cs'):
                files.append(os.path.join(root, f))
    return files


def extract_types_and_members(file_path):
    """
    Bir C# dosyasından public tip ve üye isimlerini çıkarır.
    Dönüş: [(kategori, isim, dosya_ismi), ...]
    kategori: 'type' | 'method' | 'property' | 'field' | 'enum_value'
    """
    try:
        with open(file_path, 'r', encoding='utf-8') as f:
            content = f.read()
    except Exception:
        return []

    results = []
    lines = content.split('\n')

    # Dosyadaki tip isimlerini bul (constructor tespiti için)
    type_names = set()
    type_pattern = re.compile(
        r'^\s*public\s+(?:static\s+|abstract\s+|sealed\s+|readonly\s+|partial\s+)*'
        r'(?:class|struct|interface|enum|record(?:\s+struct)?)\s+(\w+)'
    )
    for line in lines:
        m = type_pattern.match(line)
        if m:
            type_names.add(m.group(1))

    # Satır satır parse et
    for line in lines:
        stripped = line.strip()
        if not stripped.startswith('public '):
            continue
        if stripped.startswith('public override string ToString'):
            continue

        # --- Enum değerleri ---
        # public const X veya sadece public enum member'ları
        # Basitçe: public ile başlayan ve = veya , içeren satırlar
        enum_val_match = re.match(r'^\s*public\s+const\s+\w+\s+(\w+)\s*[=;]', stripped)
        if enum_val_match:
            results.append(('field', enum_val_match.group(1), os.path.basename(file_path)))
            continue

        # --- Tip tanımları ---
        type_match = type_pattern.match(stripped)
        if type_match:
            results.append(('type', type_match.group(1), os.path.basename(file_path)))
            continue

        # --- Metodlar ---
        # public [static|async|override|virtual|readonly|partial|implicit|explicit] <return_type> Name(<params>)
        # Ama constructor hariç: Name == tip_ismi ve parantez varsa
        if ' operator ' in stripped:
            continue  # implicit/explicit operator'leri atla
        method_match = re.match(
            r'^\s*public\s+(?:static\s+|async\s+|override\s+|virtual\s+|abstract\s+|readonly\s+|partial\s+)*'
            r'[\w<>,\[\]\s>?]+?\s+(\w+)(?:<[^>]+>)?\s*\(',
            stripped
        )
        if method_match:
            name = method_match.group(1)
            # Constructor filtrele
            if name in type_names:
                continue
            # Object metodlarını atla
            if name in ('ToString', 'Equals', 'GetHashCode', 'CompareTo', 'Clone'):
                continue
            # Genel C# keyword'lerini atla
            if name in ('if', 'while', 'for', 'switch', 'using', 'return', 'new', 'await'):
                continue
            results.append(('method', name, os.path.basename(file_path)))
            continue

        # --- Property'ler ---
        # public [static|override|...] <type> Name { get; set; }
        prop_match = re.match(
            r'^\s*public\s+(?:static\s+|override\s+|virtual\s+|abstract\s+|readonly\s+)*'
            r'[\w<>,\[\]\s>?]+?\s+(\w+)\s*\{',
            stripped
        )
        if prop_match:
            name = prop_match.group(1)
            # Indexer atla
            if name == 'this':
                continue
            results.append(('property', name, os.path.basename(file_path)))
            continue

        # --- Field'ler ---
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
    """Example klasöründeki tüm .cs dosyalarının içeriğini birleştirir."""
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
    API isminin example içeriğinde kullanılıp kullanılmadığını kontrol eder.
    Tam kelime eşleşmesi arar.
    """
    pattern = r'\b' + re.escape(api_name) + r'\b'
    return bool(re.search(pattern, example_content))


def analyze_library(lib_name, example_name):
    """Bir kütüphane için analiz yapar."""
    lib_dir = PROJECT_ROOT / lib_name
    example_dir = PROJECT_ROOT / "examples" / example_name

    if not lib_dir.exists():
        return None

    # Tüm public API'leri topla
    all_apis = []  # [(kategori, isim, dosya), ...]
    for cs_file in get_cs_files(lib_dir):
        apis = extract_types_and_members(cs_file)
        all_apis.extend(apis)

    # Tekilleştir (aynı isim farklı dosyalarda olabilir)
    seen = set()
    unique_apis = []
    for cat, name, fname in all_apis:
        key = (cat, name)
        if key not in seen:
            seen.add(key)
            unique_apis.append((cat, name, fname))

    # Example içeriğini oku
    example_content = read_example_content(example_dir)
    has_example = example_content != ""

    # Kullanım kontrolü
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
    """Analiz sonuçlarından Markdown raporu üretir."""
    lines = []
    lines.append("# CSharpEssentials - Detaylı Example Coverage Raporu")
    lines.append("")
    lines.append("> Bu rapor, **her bir public API öğesinin** (tip, metod, property, field)")
    lines.append("> ilgili example projesinde kullanılıp kullanılmadığını gösterir.")
    lines.append(">")
    lines.append("> ✅ = Example'da kullanılıyor")
    lines.append("> ❌ = Example'da kullanılmıyor")
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
            lines.append("⚠️ **Example projesi bulunamadı!**")
            lines.append("")
            continue

        lines.append(f"**Kapsam:** {used_count}/{total_count} ({used_count*100//total_count if total_count else 0}%)")
        lines.append("")
        lines.append("| Kategori | API İsmi | Example'da Kullanım |")
        lines.append("|----------|----------|---------------------|")

        # Alfabetik sırala
        for api in sorted(apis, key=lambda x: (x['category'], x['name'])):
            icon = "✅" if api['used'] else "❌"
            cat = api['category']
            name = api['name']
            lines.append(f"| {cat} | `{name}` | {icon} |")

        lines.append("")

    # Genel özet
    lines.append("---")
    lines.append("")
    lines.append("## 📊 Genel Example Coverage Özeti")
    lines.append("")
    lines.append(f"- **Toplam Public API:** {total_apis}")
    lines.append(f"- **Example'da Kullanılan:** {total_used}")
    lines.append(f"- **Kapsam:** {total_used*100//total_apis if total_apis else 0}%")
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

    print(f"\nRapor oluşturuldu: {output_path}")


if __name__ == "__main__":
    main()
