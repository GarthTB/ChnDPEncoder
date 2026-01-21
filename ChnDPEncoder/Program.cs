using ChnDPEncoder.Core;
using ChnDPEncoder.Models;
using static System.Console;

try {
    WriteLine(
        "ChnDPEncoder 动态规划赛码器 1.0.0 (20260122)\n"
      + "作者：Garth TB | 天卜 <g-art-h@outlook.com>\n"
      + "仓库：https://github.com/GarthTB/ChnDPEncoder\n"
      + "加载配置...");
    var config = ConfigModel.FromToml("Config.toml");
    Encoder encoder = new(config.Costs, config.Dict, config.NeedSpace);
    WriteLine("配置就绪，开始编码...");
    foreach (var inPath in config.Texts) {
        var (textLen, code, cost) = encoder.Encode(inPath);
        var report = CodeStats.Analyze(textLen, code, cost, config.Layout);
        var outPath = GenOutPath(inPath);
        File.WriteAllLines(outPath, report);
        WriteLine($"已完成：{inPath} -> {outPath}");
    }
} catch (Exception ex) {
    ForegroundColor = ConsoleColor.Red;
    WriteLine($"异常中断：{ex.Message}\n栈追踪：\n{ex.StackTrace}");
    ResetColor();
} finally {
    WriteLine("程序结束");
}

static string GenOutPath(string inPath) {
    var dir = Path.GetDirectoryName(inPath) ?? throw new InvalidOperationException("无法获取待编码文本的目录");
    var name = Path.GetFileNameWithoutExtension(inPath);
    var ext = Path.GetExtension(inPath);
    var outPath = Path.Combine(dir, $"{name}_Code{ext}");
    for (var i = 2; File.Exists(outPath); i++)
        outPath = Path.Combine(dir, $"{name}_Code_{i}{ext}");
    return outPath;
}
