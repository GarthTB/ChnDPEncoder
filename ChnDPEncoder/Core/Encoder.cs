namespace ChnDPEncoder.Core;

using System.Collections.Frozen;
using Models;

/// <summary> 最小开销编码求解器 </summary>
/// <param name="costs"> 键对开销表 </param>
/// <param name="dict"> 前缀树词库 </param>
/// <param name="usedCodes"> 词库中存在的编码 </param>
internal sealed class Encoder(CostMap costs, TrieDict dict, FrozenSet<string> usedCodes)
{
    /// <summary> 求解最小开销编码 </summary>
    /// <param name="path"> 待编码文本路径 </param>
    public (int TextLen, string Code, double Cost) Encode(string path) =>
        throw new NotImplementedException();
}
