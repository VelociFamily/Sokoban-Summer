import json
import glob
import os
from collections import defaultdict, deque

root = os.path.join(os.path.dirname(__file__), '..')
search = [os.path.join(root, 'Assets', '**', '*.asmdef')]

files = []
for pattern in search:
    files.extend(glob.glob(pattern, recursive=True))

name_to_path = {}
refs = defaultdict(list)

for f in files:
    try:
        with open(f, 'r', encoding='utf-8') as fh:
            data = json.load(fh)
            name = data.get('name')
            if name:
                name_to_path[name] = f
                for r in data.get('references', []):
                    refs[name].append(r)
    except Exception as e:
        print(f'Failed to read {f}: {e}')

# build graph for only known assemblies
graph = {n: [r for r in refs[n] if r in name_to_path] for n in name_to_path}

print('Found assemblies:')
for n in sorted(graph.keys()):
    print(f' - {n}')

# detect cycles using DFS
visited = {}
stack = []
cycles = []

def dfs(node):
    visited[node] = 1
    stack.append(node)
    for nbr in graph.get(node, []):
        if visited.get(nbr, 0) == 0:
            dfs(nbr)
        elif visited.get(nbr) == 1:
            # cycle found
            idx = stack.index(nbr)
            cycles.append(stack[idx:] + [nbr])
    visited[node] = 2
    stack.pop()

for n in graph:
    if visited.get(n, 0) == 0:
        dfs(n)

if cycles:
    print('\nCycles detected:')
    for c in cycles:
        print(' -> '.join(c))
else:
    print('\nNo cycles detected.')

# print adjacency
print('\nDependency graph:')
for n, outs in graph.items():
    print(f'{n} -> {outs}')
