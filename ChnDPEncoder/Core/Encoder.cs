namespace ChnDPEncoder.Core;

using System.Collections.Frozen;
using System.Text;
using Models;

/// <summary> 最小开销编码求解器 </summary>
/// <param name="costs"> 键对开销表 </param>
/// <param name="dict"> 前缀树词库 </param>
/// <param name="needSpace"> 需要空格分隔的编码 </param>
internal sealed class Encoder(CostMap costs, TrieDict dict, FrozenSet<(string, char)> needSpace)
{
    /// <summary> 分块字符数 </summary>
    private const int ChunkSize = 1 << 16;

    /// <summary> 求解最小开销编码 </summary>
    public (int TextLen, string Code, double Cost) Encode(string path) {
        var chunk = ""; // 当前文本块
        int curIdx = 0, maxIdx = 0; // 当前索引、已抵达的最远索引
        int textLen = 0, chunkCnt; // 总字数、当前块字数
        (string Last, string Code, double Cost)?[] dp = [("", "", 0)]; // 到达各索引的最小开销编码
        StringBuilder frozenCode = new(ChunkSize); // 已固化的编码前部

        var buffer = new char[ChunkSize];
        List<(int Len, string Code, double Cost)> wordInfos = new(16); // 当前索引处的起始词的长度、编码、开销
        using (StreamReader reader = new(path))
            while ((chunkCnt = reader.Read(buffer, 0, ChunkSize)) > 0) {
                Console.Write($"\r第 {textLen} - {textLen += chunkCnt} 字...");
                PrepareChunk();
                for (; dict.FindPrefixes(chunk.AsSpan(curIdx), wordInfos); curIdx++)
                    ProcCurIdx();
            }
        for (; curIdx < chunk.Length; curIdx++) {
            _ = dict.FindPrefixes(chunk.AsSpan(curIdx), wordInfos);
            ProcCurIdx();
        }

        var (end, finalCode, finalCost) = dp[^1]!.Value; // 一定可达
        finalCode = frozenCode.Append(finalCode).Append(' ').ToString(); // 空格上屏末词
        finalCost += costs[[end[^1], ' ']];
        Console.WriteLine($"\n共 {textLen} 字，{finalCode.Length} 码");
        return (textLen, finalCode, finalCost);

        // 将新块接在剩余部分之后
        void PrepareChunk() {
            chunk = string.Concat(chunk.AsSpan(curIdx), buffer.AsSpan(0, chunkCnt));
            var rest = dp.AsSpan(curIdx);
            var newDp = new (string, string, double)?[rest.Length + chunkCnt];
            rest.CopyTo(newDp);
            dp = newDp;
            maxIdx -= curIdx;
            curIdx = 0;
        }

        // 处理当前索引
        void ProcCurIdx() {
            var (last, code1, cost1) = dp[curIdx]!.Value; // 一定可达
            if (curIdx == maxIdx) { // 没有其他编码时，当前为全局最优，固化前部
                _ = frozenCode.Append(code1);
                code1 = "";
            }

            foreach (var (len, code2, cost2) in wordInfos)
                UpdateMin(curIdx + len, code2, cost2);
            if (dp[curIdx + 1] is null) // 填入原字符兜底
                UpdateMin(curIdx + 1, chunk[curIdx].ToString(), 0);

            // 更新到达指定索引的最小开销编码
            void UpdateMin(int tgtIdx, string code2, double cost2) {
                var ns = needSpace.Contains((last, code2[0]));
                var cost = ns
                    ? cost1 + costs[[code1[^1], ' ']] + costs[[' ', code2[0]]] + cost2
                    : cost1 + costs[[code1[^1], code2[0]]] + cost2;
                if (dp[tgtIdx]?.Cost <= cost)
                    return;
                var code = ns
                    ? code1 + ' ' + code2
                    : code1 + code2;
                dp[tgtIdx] = (code2, code, cost);
                if (tgtIdx > maxIdx)
                    maxIdx = tgtIdx;
            }
        }
    }
}
