namespace ChnDPEncoder.Models;

using System.Collections.Frozen;
using System.Globalization;

/// <summary> 前缀树词库 </summary>
internal sealed class TrieDict
{
    /// <summary> 根节点 </summary>
    private readonly FrozenDictionary<char, Node> _root;

    /// <summary> 将词库文件解析为前缀树 </summary>
    public TrieDict(string path, CostMap costMap, out FrozenSet<(string, char)> spaceCodes) {
        var entries = File.ReadLines(path)
            .Select(static (line, idx) => (idx, Parts: line.Split('\t', 4)))
            .Where(static tup => tup.Parts is [{ Length: > 0 } word, { Length: > 0 }, ..]
                              && word[0] != '#')
            .OrderByDescending(static tup => tup.Parts.Length > 2
                ? double.Parse(tup.Parts[2], NumberStyles.Any)
                : 0) // 权重降序
            .ThenBy(static tup => tup.idx); // 行号升序

        Node root = new() { Children = new(4096) }; // 预分配常用字
        HashSet<string> codes = new(4096);
        foreach (var (_, parts) in entries) {
            var node = parts[0]
                .Aggregate(
                    root,
                    static (n, c) => (n.Children ??= []).TryGetValue(c, out var child)
                        ? child
                        : n.Children[c] = new());
            var code = parts[1];
            for (var (i, prefix) = (2, code); !codes.Add(code);)
                (i, code) = i < 10
                    ? (i + 1, prefix + i) // 2-9：数字选重
                    : (2, prefix += '='); // 10：等号翻页
            var cost = costMap[code];
            if (node.Min is null || node.Min?.Cost > cost)
                node.Min = (code, cost);
        }
        _root = root.Children.ToFrozenDictionary();
        spaceCodes = codes.Where(static code => code.Length > 1)
            .Select(static code => (code[..^1], code[^1]))
            .ToFrozenSet();
    }

    /// <summary> 查找文本所有起始词的编码及其开销 </summary>
    /// <param name="text"> 文本 </param>
    /// <param name="wordInfos"> 起始词的长度、编码、开销（复用列表） </param>
    /// <returns> 是否在到达文本末端前查找殆尽 </returns>
    /// <remarks> 会先清空List </remarks>
    public bool FindPrefixes(
        ReadOnlySpan<char> text,
        List<(int Len, string Code, double Cost)> wordInfos) {
        wordInfos.Clear();
        if (text.IsEmpty)
            return false;

        if (!_root.TryGetValue(text[0], out var first))
            return true;
        if (first.Min is {} firstMin)
            wordInfos.Add((1, firstMin.Code, firstMin.Cost));

        for (var (i, node) = (1, first); i < text.Length; i++)
            if (node.Children is null || !node.Children.TryGetValue(text[i], out node))
                return true;
            else if (node.Min is {} min)
                wordInfos.Add((i + 1, min.Code, min.Cost));
        return false;
    }

    /// <summary> 前缀树节点 </summary>
    private sealed class Node
    {
        /// <summary> 子节点 </summary>
        public Dictionary<char, Node>? Children;

        /// <summary> 最小开销编码 </summary>
        public (string Code, double Cost)? Min;
    }
}
