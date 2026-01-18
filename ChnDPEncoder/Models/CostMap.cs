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

    /// <summary> 解析键对开销表文件 </summary>
    public CostMap(string path) {
        var data = File.ReadLines(path)
            .Where(static line => line is [_, _, '\t', _, ..])
            .Select(static line =>
                (Key: (line[0] << 16) | line[1], Cost: double.Parse(line.AsSpan(3))))
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
    public double this[ReadOnlySpan<char> code] {
        get {
            if (code.Length < 2)
                return 0;

            var sum = 0d;
            for (var (i, key) = (1, (int)code[0]); i < code.Length; i++) {
                key = (key << 16) | code[i];
                if (_costMap.TryGetValue(key, out var cost))
                    sum += cost;
                else {
                    _ = _missingPairs.Add(key);
                    sum += _meanCost;
                }
            }
            return sum;
        }
    }
}
