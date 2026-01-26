namespace ChnDPEncoder.Models;

using System.Collections.Frozen;
using Tomlyn;
using Tomlyn.Model;

/// <summary> 配置模型 </summary>
/// <param name="Costs"> 键对开销表 </param>
/// <param name="Dict"> 前缀树词库 </param>
/// <param name="SpaceCodes"> 需要空格分隔的编码 </param>
/// <param name="Texts"> 待编码文本路径 </param>
/// <param name="Layout"> （可选）键盘布局表 </param>
internal sealed record ConfigModel(
    CostMap Costs,
    TrieDict Dict,
    FrozenSet<(string, char)> SpaceCodes,
    IEnumerable<string> Texts,
    LayoutMap? Layout)
{
    /// <summary> 从TOML文件加载配置 </summary>
    public static ConfigModel FromToml(string path) {
        var toml = File.ReadAllText(path);
        var model = Toml.ToModel(toml);
        CostMap costs = model["cost_map"] is string costsPath
            ? new(costsPath)
            : throw new ArgumentException("键对开销表路径缺失");
        TrieDict dict = model["rime_dict"] is string dictPath
            ? new(dictPath, costs, out var spaceCodes)
            : throw new ArgumentException("RIME词库路径缺失");
        var sTexts = model["texts"] is TomlArray { Count: > 0 } oTexts
            ? ParseTomlArray(oTexts, "待编码文本路径")
            : throw new ArgumentException("待编码文本路径缺失");
        LayoutMap? layout = model["layout"] is TomlArray { Count: 14 } oKeys
            ? new(ParseTomlArray(oKeys, "键盘布局").ToArray())
            : null;
        return new(costs, dict, spaceCodes, sTexts, layout);

        static IEnumerable<string> ParseTomlArray(TomlArray arr, string name) =>
            arr.Select(obj => obj as string ?? throw new ArgumentException($"{name}数组格式错误"));
    }
}
