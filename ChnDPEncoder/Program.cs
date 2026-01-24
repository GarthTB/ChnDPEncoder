using ChnDPEncoder.Core;
using ChnDPEncoder.Models;
using static System.Console;
using static System.IO.Path;

try {
    WriteLine(
        "ChnDPEncoder 动态规划赛码器 1.0.0 (20260122)\n"
      + "作者：Garth TB | 天卜 <g-art-h@outlook.com>\n"
      + "仓库：https://github.com/GarthTB/ChnDPEncoder\n"
      + "加载配置...");
    var config = ConfigModel.FromToml("Config.toml");
    Encoder encoder = new(config.Costs, config.Dict, config.SpaceCodes);
    WriteLine("配置就绪！");
    foreach (var inPath in config.Texts) {
        var outPath = GenOutPath(inPath);
        WriteLine($"{inPath}编码中...");
        var (textLen, codeLen, costSum) = encoder.Encode(inPath, outPath, 4096);
        WriteLine("编码完成，分析中...");
        CodeStats.Analyze(textLen, codeLen, costSum, outPath, config.Layout, 4096);
        WriteLine($"分析完成，结果在{outPath}");
    }
} catch (Exception ex) {
    ForegroundColor = ConsoleColor.Red;
    WriteLine($"异常中断：{ex.Message}\n栈追踪：\n{ex.StackTrace}");
    ResetColor();
} finally {
    WriteLine("程序结束，已退出");
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
