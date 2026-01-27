namespace ChnDPEncoder.Core;

using System.Collections.Frozen;
using System.Text;
using Models;

/// <summary> 最小开销编码求解器 </summary>
/// <param name="costs"> 键对开销表 </param>
/// <param name="dict"> 前缀树词库 </param>
/// <param name="spaceCodes"> 需要空格分隔的编码 </param>
internal sealed class Encoder(CostMap costs, TrieDict dict, FrozenSet<(string, char)> spaceCodes)
{
    /// <summary> 求解最小开销编码 </summary>
    public (long TextLen, long CodeLen, double CostSum) Encode(
        string inPath,
        string outPath,
        int chunkSize) {
        long textLen = 0, codeLen = 0; // 总字数、总码数
        (string Prev, string Code, double Cost)?[] dp = [("", "", 0)]; // 到达各索引的最小开销编码
        List<(int Len, string Code, double Cost)> wordInfos = new(8); // 当前索引处的起始词的长度、编码、开销
        StringBuilder frozenCode = new(2 * chunkSize); // 已固化的编码前部

        var chunk = ""; // 当前文本块
        int curIdx = 0, maxIdx = 0, charsRead; // 当前索引、已抵达的最远索引、新块字数
        var buffer = new char[chunkSize];
        using StreamWriter writer = new(outPath, true, Encoding.UTF8, 2 * chunkSize);
        using (StreamReader reader = new(inPath, Encoding.UTF8, true, 3 * chunkSize))
            while ((charsRead = reader.Read(buffer, 0, chunkSize)) > 0)
                for (PrepareChunk(); dict.FindPrefixes(chunk.AsSpan(curIdx), wordInfos); curIdx++)
                    ProcCurIdx();
        for (; curIdx < chunk.Length; curIdx++) {
            _ = dict.FindPrefixes(chunk.AsSpan(curIdx), wordInfos);
            ProcCurIdx();
        }

        var (endCode, endPart, costSum) = dp[^1]!.Value; // 一定可达
        if (endCode.Length == 0)
            throw new ArgumentException("文本为空", nameof(inPath));
        var endBlock = frozenCode.Append(endPart);
        if (char.IsAsciiLetter(endCode[^1])) {
            _ = endBlock.Append(' '); // 空格上屏末词
            costSum += costs[[endCode[^1], ' ']];
        }
        writer.WriteLine(endBlock); // 换行分隔统计数据
        codeLen += endBlock.Length;
        return (textLen, codeLen, costSum);

        // 将新块接在剩余部分之后
        void PrepareChunk() {
            Console.Write($"\r第 {textLen} - {textLen += charsRead} 字...");
            chunk = string.Concat(chunk.AsSpan(curIdx), buffer.AsSpan(0, charsRead));
            var rest = dp.AsSpan(curIdx);
            var newDp = new (string, string, double)?[rest.Length + charsRead];
            rest.CopyTo(newDp);
            dp = newDp;
            maxIdx -= curIdx;
            curIdx = 0;
        }

        // 处理当前索引
        void ProcCurIdx() {
            var (prev, code1, cost1) = dp[curIdx]!.Value; // 一定可达
            if (curIdx == maxIdx) { // 确定最优，固化前部
                if (frozenCode.Append(code1).Length > chunkSize) { // 写入固化部分
                    writer.Write(frozenCode);
                    codeLen += frozenCode.Length;
                    frozenCode.Clear();
                }
                code1 = "";
            }
            foreach (var (len, code2, cost2) in wordInfos)
                UpdateMin(curIdx + len, code2, cost2);
            if (dp[curIdx + 1] is null) // 填入原字符兜底
                UpdateMin(curIdx + 1, new(chunk[curIdx], 1), 0);

            // 更新到达指定索引的最小开销编码
            void UpdateMin(int tgtIdx, string code2, double cost2) {
                var needSpace = spaceCodes.Contains((prev, code2[0]));
                var cost = prev.Length == 0
                    ? cost2
                    : needSpace
                        ? cost1 + costs[[prev[^1], ' ']] + costs[[' ', code2[0]]] + cost2
                        : cost1 + costs[[prev[^1], code2[0]]] + cost2;
                if (dp[tgtIdx]?.Cost <= cost)
                    return;
                var code = needSpace
                    ? code1 + ' ' + code2
                    : code1 + code2;
                dp[tgtIdx] = (code2, code, cost);
                if (tgtIdx > maxIdx)
                    maxIdx = tgtIdx;
            }
        }
    }
}
