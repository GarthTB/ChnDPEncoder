namespace ChnDPEncoder.Models;

using System.Collections.Frozen;
using Tomlyn;
using Tomlyn.Model;

/// <summary> 配置模型 </summary>
/// <param name="Costs"> 键对开销表 </param>
/// <param name="Dict"> 前缀树词库 </param>
/// <param name="UsedCodes"> 词库中存在的编码 </param>
/// <param name="Texts"> 待编码文本路径 </param>
/// <param name="Layout"> 键盘布局表 </param>
internal sealed record ConfigModel(
    CostMap Costs,
    TrieDict Dict,
    FrozenSet<string> UsedCodes,
    string[] Texts,
    LayoutMap Layout)
{
    /// <summary> 从TOML文件加载配置 </summary>
    public static ConfigModel FromToml(string path) {
        var toml = Toml.ToModel(File.ReadAllText(path));
        CostMap costs = toml["cost_map"] is string costsPath
            ? new(costsPath)
            : throw new ArgumentException("键对开销表路径缺失");
        TrieDict dict = toml["rime_dict"] is string dictPath
            ? new(dictPath, costs, out var usedCodes)
            : throw new ArgumentException("RIME词库路径缺失");
        var sTexts = toml["texts"] is TomlArray { Count: > 0 } oTexts
            ? oTexts.Select(static obj =>
                obj as string ?? throw new ArgumentException("待编码文本路径格式错误"))
            : throw new ArgumentException("待编码文本路径缺失");
        var sKeys = toml["layout"] is TomlArray { Count: 14 } oKeys
            ? oKeys.Select(static obj => obj as string ?? throw new ArgumentException("键盘布局格式错误"))
            : throw new ArgumentException("键盘布局缺失或格式错误");
        LayoutMap layout = new(sKeys.ToArray());
        return new(costs, dict, usedCodes, sTexts.ToArray(), layout);
    }
}
