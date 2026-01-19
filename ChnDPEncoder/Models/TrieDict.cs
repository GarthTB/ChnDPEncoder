namespace ChnDPEncoder.Models;

using System.Globalization;

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
                ? double.Parse(tup.Parts[2], NumberStyles.Any)
                : 0) // 权重降序
            .ThenBy(static tup => tup.idx); // 行号升序

        usedCodes = new(8192);
        foreach (var (_, parts) in entries) {
            var code = parts[1];
            for (var (root, i) = (code, 2); !usedCodes.Add(code);)
                (code, i) = i < 10
                    ? (root + i, i + 1) // 2-9：数字选重
                    : (root += '=', 2); // 10：等号翻页
            var cost = costMap[code];
            var node = parts[0]
                .Aggregate(
                    _root,
                    static (n, c) => (n.Children ??= []).TryGetValue(c, out var child)
                        ? child
                        : n.Children[c] = new());
            if (node.Cost is null || cost < node.Cost)
                (node.Code, node.Cost) = (code, cost);
        }
    }

    /// <summary> 尝试刷新文本所有起始词的编码及其开销 </summary>
    /// <param name="text"> 文本 </param>
    /// <param name="prefixes"> 起始词的字数、编码、开销 </param>
    /// <returns> 是否有词到达文本末端 </returns>
    public bool FindPrefixes(
        ReadOnlySpan<char> text,
        List<(int WordLen, string Code, double Cost)> prefixes) {
        prefixes.Clear();
        for (var (i, node) = (0, _root); i < text.Length; i++)
            if (node.Children is null || !node.Children.TryGetValue(text[i], out node))
                return false;
            else if (node is { Code: {} code, Cost: {} cost })
                prefixes.Add((i + 1, code, cost));
        return true;
    }

    /// <summary> 前缀树节点 </summary>
    private sealed class Node
    {
        /// <summary> 子节点 </summary>
        public Dictionary<char, Node>? Children;

        /// <summary> 最优编码 </summary>
        public string? Code;

        /// <summary> 最小开销 </summary>
        public double? Cost;
    }
}
