namespace ChnDPEncoder.Models;

/// <summary> 键对开销表 </summary>
internal sealed class CostMap
{
    /// <summary> 键对开销表 </summary>
    private readonly Dictionary<int, double> _costMap;

    /// <summary> 平均开销 </summary>
    private readonly double _meanCost;

    /// <summary> 缺失的键对 </summary>
    private readonly HashSet<int> _missingPairs = [];

    /// <summary> 从文件解析键对开销表 </summary>
    public CostMap(string path) {
        var data = File.ReadLines(path)
            .Select(static line => line.Split('\t', 3))
            .Where(static parts => parts is [{ Length: 2 }, _])
            .Select(static parts =>
                (Key: (parts[0][0] << 16) | parts[0][1], Cost: double.Parse(parts[1])))
            .ToArray();
        _costMap = data.ToDictionary(static kvp => kvp.Key, static kvp => kvp.Cost);
        _meanCost = data.Average(static kvp => kvp.Cost);
    }

    /// <summary> 缺失的键对 </summary>
    public IReadOnlySet<(char A, char B)> MissingPairs =>
        _missingPairs.Select(static key => (A: (char)(key >> 16), B: (char)(key & 0xFFFF)))
            .Where(static pair => !char.IsWhiteSpace(pair.A) && !char.IsWhiteSpace(pair.B))
            .ToHashSet();

    /// <summary> 获取编码的总开销 </summary>
    public double GetCost(ReadOnlySpan<char> code) {
        if (code.Length < 2)
            return 0;
        var sum = 0d;
        for (var (i, key) = (1, (int)code[0]); i < code.Length; i++) {
            key = (key << 16) | code[i];
            if (_costMap.TryGetValue(key, out var cost))
                sum += cost;
            else {
                _missingPairs.Add(key);
                sum += _meanCost;
            }
        }
        return sum;
    }
}
