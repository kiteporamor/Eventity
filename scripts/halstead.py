#!/usr/bin/env python3
import os
import re
import math
import sys

OPERATOR_PATTERN = re.compile(r"\+\+|--|==|!=|<=|>=|<<|>>|&&|\|\||\+|\-|\*|/|%|=|<|>|\!|\^|&|\||~|\?|:|\.|,|;")
IDENTIFIER_PATTERN = re.compile(r"\b[_A-Za-z][_A-Za-z0-9]*\b")

def halstead_metrics(text):
    operators = OPERATOR_PATTERN.findall(text)
    identifiers = IDENTIFIER_PATTERN.findall(text)

    # Filter out C# keywords from identifiers to approximate operands
    keywords = set(["if","else","for","while","return","class","struct","namespace","using","public","private","protected","internal","static","void","int","string","bool","new","var","await","async","switch","case","break","continue","try","catch","finally"])
    operands = [i for i in identifiers if i not in keywords]

    N1 = len(operators)
    N2 = len(operands)
    unique_ops = set(operators)
    unique_ops_count = len(unique_ops)
    unique_operands = set(operands)
    unique_operands_count = len(unique_operands)

    n = unique_ops_count + unique_operands_count
    N = N1 + N2
    if n == 0:
        volume = 0.0
    else:
        volume = N * math.log2(n)
    return {
        'N1': N1, 'N2': N2,
        'n1': unique_ops_count, 'n2': unique_operands_count,
        'N': N, 'n': n, 'volume': volume
    }

def scan_repo(root):
    results = {}
    for dirpath, dirnames, filenames in os.walk(root):
        # skip build artifacts
        if 'obj' in dirpath.split(os.sep) or 'bin' in dirpath.split(os.sep):
            continue
        for f in filenames:
            if f.endswith('.cs'):
                path = os.path.join(dirpath, f)
                try:
                    with open(path, 'r', encoding='utf-8') as fh:
                        txt = fh.read()
                except Exception:
                    continue
                metrics = halstead_metrics(txt)
                results[path] = metrics
    return results

def main():
    root = os.getcwd()
    results = scan_repo(root)
    worst = None
    for path, m in results.items():
        vol = m['volume']
        print(f"{path}: volume={vol:.1f} N={m['N']} n={m['n']} (n1={m['n1']} n2={m['n2']})")
        if worst is None or vol > worst[0]:
            worst = (vol, path)

    if worst:
        print(f"\nWorst file: {worst[1]} with volume={worst[0]:.1f}")
        # if volume huge, fail to force attention (threshold is generous)
        if worst[0] > 50000:
            print("Halstead volume exceeds threshold (50000). Failing.")
            sys.exit(1)

if __name__ == '__main__':
    main()
