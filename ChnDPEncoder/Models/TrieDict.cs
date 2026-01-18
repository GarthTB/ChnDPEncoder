namespace ChnDPEncoder.Models;

/// <summary> 前缀树词库 </summary>
internal sealed class TrieDict
{
    /// <summary> 根节点 </summary>
    private readonly Node _root = new() { Children = new(8192) }; // 假设覆盖通规

    /// <summary> 将词库文件解析为前缀树 </summary>
    public TrieDict(string path, CostMap costMap, out HashSet<string> usedCodes) {
        var entries = File.ReadLines(path)
            .Select(static (line, idx) => (idx, Parts: line.Split('\t', 4)))
            .Where(static tup => tup.Parts is [{ Length: > 0 } word, { Length: > 0 }, ..]
                              && word[0] != '#')
            .OrderByDescending(static tup => tup.Parts.Length > 2
                ? double.Parse(tup.Parts[2])
                : 0) // 权重降序
            .ThenBy(static tup => tup.idx); // 行号升序

        usedCodes = new(8192);
        foreach (var (_, parts) in entries) {
            var code = parts[1];
            for (var (root, i) = (code, 2); !usedCodes.Add(code);)
                (code, i) = i < 10
                    ? (root + i, i + 1) // 不到10：数字选重上屏
                    : ((root += '=') + ' ', 2); // 到10：等号翻页，空格首选上屏
            var node = parts[0]
                .Aggregate(
                    _root,
                    static (n, c) => (n.Children ??= new()).TryGetValue(c, out var child)
                        ? child
                        : n.Children[c] = new());
            var cost = costMap[code];
            if (node.MinCc is null || cost < node.MinCc?.Cost)
                node.MinCc = new(code, cost);
        }
    }

    /// <summary> 尝试刷新文本所有起始词的编码及其开销 </summary>
    /// <param name="text"> 文本 </param>
    /// <param name="codes"> 编码及其开销：索引=词长-1 </param>
    /// <returns> 文本是否耗尽 </returns>
    public bool TryUpdateCodes(ReadOnlySpan<char> text, List<CodeCost?> codes) {
        codes.Clear();
        var node = _root;
        foreach (var c in text)
            if (node.Children is {} children && children.TryGetValue(c, out node))
                codes.Add(node.MinCc);
            else
                return false;
        return true;
    }

    /// <summary> 前缀树节点 </summary>
    private sealed class Node
    {
        /// <summary> 子节点 </summary>
        public Dictionary<char, Node>? Children;

        /// <summary> 最小开销编码 </summary>
        public CodeCost? MinCc;
    }
}
