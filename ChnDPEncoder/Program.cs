using ChnDPEncoder.Core;
using ChnDPEncoder.Models;
using static System.Console;
using static System.IO.Path;

try {
    Write(
        "ChnDPEncoder 动态规划赛码器 1.1.1 (20260128)\n"
      + "作者：Garth TB | 天卜 <g-art-h@outlook.com>\n"
      + "仓库：https://github.com/GarthTB/ChnDPEncoder\n"
      + "加载配置...");
    var config = ConfigModel.FromToml("Config.toml");
    Encoder encoder = new(config.Costs, config.Dict, config.SpaceCodes);
    Write("完成！\n");
    foreach (var inPath in config.Texts) {
        var outPath = GenOutPath(inPath);
        Write($"编码{inPath}...\n");
        var (textLen, codeLen, costSum) = encoder.Encode(inPath, outPath, 262144);
        Write($"完成！共{textLen}字，{codeLen}码。\n分析...");
        CodeStats.Analyze(textLen, codeLen, costSum, outPath, config.Layout, 262144);
        Write($"完成！\n结果已存至{outPath}\n");
    }
} catch (Exception ex) {
    ForegroundColor = ConsoleColor.Red;
    Write($"异常：{ex.Message}\n栈追踪：\n{ex.StackTrace}\n");
    ResetColor();
} finally {
    Write("程序已退出\n");
}

static string GenOutPath(string inPath) {
    var dir = GetDirectoryName(inPath) ?? throw new InvalidOperationException("无法获取待编码文本的目录");
    var name = GetFileNameWithoutExtension(inPath);
    var ext = GetExtension(inPath);
    var outPath = Combine(dir, $"{name}_Code{ext}");
    for (var i = 2; File.Exists(outPath); i++)
        outPath = Combine(dir, $"{name}_Code_{i}{ext}");
    return outPath;
}
