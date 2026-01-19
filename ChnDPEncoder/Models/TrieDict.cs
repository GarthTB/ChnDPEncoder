namespace ChnDPEncoder.Models;

using System.Collections.Frozen;
using System.Globalization;

/// <summary> 前缀树词库 </summary>
internal sealed class TrieDict
{
    /// <summary> 根节点 </summary>
    private readonly FrozenDictionary<char, Node> _root;

    /// <summary> 将词库文件解析为前缀树 </summary>
    public TrieDict(string path, CostMap costMap, out FrozenSet<string> usedCodes) {
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
            var code = parts[1];
            for (var (prefix, i) = (code, 2); !codes.Add(code);)
                (code, i) = i < 10
                    ? (prefix + i, i + 1) // 2-9：数字选重
                    : (prefix += '=', 2); // 10：等号翻页
            var cost = costMap[code];
            var node = parts[0]
                .Aggregate(
                    root,
                    static (n, c) => (n.Children ??= []).TryGetValue(c, out var child)
                        ? child
                        : n.Children[c] = new());
            if (node.Min is null || node.Min?.Cost > cost)
                node.Min = (code, cost);
        }
        _root = root.Children.ToFrozenDictionary();
        usedCodes = codes.ToFrozenSet();
    }

    /// <summary> 查找文本所有起始词的编码及其开销 </summary>
    /// <param name="text"> 文本 </param>
    /// <param name="prefixes"> 起始词的长度、编码、开销（复用列表） </param>
    /// <returns> 是否有词到达文本末端 </returns>
    /// <remarks> 会先清空List </remarks>
    public bool FindPrefixes(
        ReadOnlySpan<char> text,
        List<(int WordLen, string Code, double Cost)> prefixes) {
        prefixes.Clear();

        if (text.IsEmpty || !_root.TryGetValue(text[0], out var first))
            return false;
        if (first.Min is {} firstMin)
            prefixes.Add((1, firstMin.Code, firstMin.Cost));

        for (var (i, node) = (1, first); i < text.Length; i++)
            if (node.Children is null || !node.Children.TryGetValue(text[i], out node))
                return false;
            else if (node.Min is {} min)
                prefixes.Add((i + 1, min.Code, min.Cost));
        return true;
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
