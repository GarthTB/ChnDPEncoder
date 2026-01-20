namespace ChnDPEncoder.Core;

using System.Collections.Concurrent;
using Models;

/// <summary> 编码统计数据 </summary>
internal static class CodeStats
{
    /// <summary> 分析编码统计数据 </summary>
    public static IReadOnlyList<string> Analyze(
        int textLen,
        string code,
        double cost,
        LayoutMap layout) {
        if (textLen == 0)
            throw new ArgumentException("文本为空", nameof(textLen));
        if (code.Length < 1)
            throw new ArgumentException("编码过短", nameof(code));

        List<string> report = new(39) {
            "------概况------",
            $"总字数\t{textLen}",
            $"总码数\t{code.Length}",
            $"总开销\t{cost:0.######}",
            $"字均码长\t{1d * code.Length / textLen:0.######}",
            $"字均开销\t{cost / textLen:0.######}",
            $"码均开销\t{cost / (code.Length - 1):0.######}"
        };
        if (code.Length < 5) {
            report.AddRange(["编码过短，不详细分析", "------编码------", code]);
            return report;
        }

        var fingerCnt = new int[9]; // 各手指计数
        var rowCnt = new int[5]; // 各排计数
        var repeatCnt = new int[4]; // 2-5+连击计数
        var leapCnt = new int[3]; // 同指跨1-3排计数
        var switchCnt = 0; // 互击计数

        _ = Parallel.ForEach(
            Partitioner.Create(0, code.Length),
            range => {
                for (var (i, end) = range; i < end; i++) {
                    var (finger, row) = layout[code[i]];
                    if (finger is {} f)
                        _ = Interlocked.Increment(ref fingerCnt[f]);
                    if (row is {} r)
                        _ = Interlocked.Increment(ref rowCnt[r]);
                    if (i > 0)
                        if (code[i] == code[i - 1])
                            _ = Interlocked.Increment(ref repeatCnt[0]);
                        else if (layout[code[i - 1]] is ({} f1, {} r1) and not (8, 4)
                              && finger is {} f2 and not 8
                              && row is {} r2 and not 4)
                            if (f1 == f2 && Math.Abs(r1 - r2) is var rowDist and > 0)
                                _ = Interlocked.Increment(ref leapCnt[rowDist - 1]);
                            else if ((f1, f2) is (< 4, > 3) or (> 3, < 4))
                                _ = Interlocked.Increment(ref switchCnt);
                    if (i > 1 && code[i] == code[i - 1] && code[i] == code[i - 2])
                        _ = Interlocked.Increment(ref repeatCnt[1]);
                    if (i > 2
                     && code[i] == code[i - 1]
                     && code[i] == code[i - 2]
                     && code[i] == code[i - 3])
                        _ = Interlocked.Increment(ref repeatCnt[2]);
                    if (i > 3
                     && code[i] == code[i - 1]
                     && code[i] == code[i - 2]
                     && code[i] == code[i - 3]
                     && code[i] == code[i - 4])
                        _ = Interlocked.Increment(ref repeatCnt[3]);
                }
            });

        var leftSum = fingerCnt[..4].Sum();
        var rightSum = fingerCnt[4..8].Sum();
        var bias = leftSum + rightSum > 0
            ? 100d * (leftSum - rightSum) / (leftSum + rightSum)
            : 0;
        var repeat2 = repeatCnt[0] - repeatCnt[1];
        var repeat3 = repeatCnt[1] - repeatCnt[2];
        var repeat4 = repeatCnt[2] - repeatCnt[3];

        report.AddRange(
        [
            "互击\t" + FormatResult(switchCnt, 2),
            "拇指\t" + FormatResult(fingerCnt[8], 1),
            $"偏倚\t{bias:0.######} %",
            "------左手------",
            "总计\t" + FormatResult(leftSum, 1),
            "小指\t" + FormatResult(fingerCnt[0], 1),
            "无名\t" + FormatResult(fingerCnt[1], 1),
            "中指\t" + FormatResult(fingerCnt[2], 1),
            "食指\t" + FormatResult(fingerCnt[3], 1),
            "------右手------",
            "总计\t" + FormatResult(rightSum, 1),
            "食指\t" + FormatResult(fingerCnt[4], 1),
            "中指\t" + FormatResult(fingerCnt[5], 1),
            "无名\t" + FormatResult(fingerCnt[6], 1),
            "小指\t" + FormatResult(fingerCnt[7], 1),
            "-------排-------",
            "数字\t" + FormatResult(rowCnt[0], 1),
            "上排\t" + FormatResult(rowCnt[1], 1),
            "中排\t" + FormatResult(rowCnt[2], 1),
            "下排\t" + FormatResult(rowCnt[3], 1),
            "空格\t" + FormatResult(rowCnt[4], 1),
            "----同键连击----",
            "2连击\t" + FormatResult(repeat2, 2),
            "3连击\t" + FormatResult(repeat3, 3),
            "4连击\t" + FormatResult(repeat4, 4),
            $"5+连击\t{repeatCnt[3]}",
            "----同指跨排----",
            "1排\t" + FormatResult(leapCnt[0], 2),
            "2排\t" + FormatResult(leapCnt[1], 2),
            "3排\t" + FormatResult(leapCnt[2], 2),
            "------编码------",
            code
        ]);
        return report;

        string FormatResult(int cnt, int nGramLen) {
            var total = code.Length - nGramLen + 1;
            var ratio = 100d * cnt / total;
            return $"{cnt}\t{ratio:0.######} %";
        }
    }
}
