namespace ChnDPEncoder.Core;

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
        if (code.Length < 2)
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

        var fingerCnt = new int[9]; // 各手指计数
        var rowCnt = new int[5]; // 各排计数
        var repeatLen = 1; // 连击长度
        var repeatCnt = new int[4]; // 2-5+连击计数
        var leapCnt = new int[3]; // 同指跨1-3排计数
        var switchCnt = 0; // 互击计数

        var prev = code[0];
        var (finger0, row0) = layout[prev];
        if (finger0 is {} f0)
            fingerCnt[f0]++;
        if (row0 is {} r0)
            rowCnt[r0]++;

        foreach (var cur in code.AsSpan(1)) {
            var (fCur, rCur) = layout[cur];
            if (fCur is {} f)
                fingerCnt[f]++;
            if (rCur is {} r)
                rowCnt[r]++;
            if (prev == cur)
                repeatLen++;
            else {
                if (repeatLen > 1) {
                    repeatCnt[Math.Min(repeatLen - 2, 3)]++;
                    repeatLen = 1;
                }
                if (layout[prev] is not (({} fp, {} rp) and (< 8, < 4))
                 || fCur is not ({} fc and < 8)
                 || rCur is not ({} rc and < 4))
                    continue;
                if (fp == fc && Math.Abs(rp - rc) is var rowDist and > 0)
                    leapCnt[rowDist - 1]++;
                else if ((fp < 4 && fc > 3) || (fp > 3 && fc < 4))
                    switchCnt++;
            }
            prev = cur;
        }

        if (repeatLen > 1)
            repeatCnt[Math.Min(repeatLen - 2, 3)]++;
        var leftSum = fingerCnt[..4].Sum();
        var rightSum = fingerCnt[4..8].Sum();
        var bias = leftSum + rightSum > 0
            ? 100d * (leftSum - rightSum) / (leftSum + rightSum)
            : 0;

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
            "2连击\t" + FormatResult(repeatCnt[0], 2),
            "3连击\t" + FormatResult(repeatCnt[1], 3),
            "4连击\t" + FormatResult(repeatCnt[2], 4),
            $"5+连击\t{repeatCnt[3]}",
            "----同指跨排----",
            "1排\t" + FormatResult(leapCnt[0], 2),
            "2排\t" + FormatResult(leapCnt[1], 2),
            "3排\t" + FormatResult(leapCnt[2], 2),
            "------编码------",
            code
        ]);
        return report;

        string FormatResult(int cnt, int windowSize) {
            var total = code.Length - windowSize + 1;
            var ratio = 100d * cnt / total;
            return $"{cnt}\t{ratio:0.######} %";
        }
    }
}
