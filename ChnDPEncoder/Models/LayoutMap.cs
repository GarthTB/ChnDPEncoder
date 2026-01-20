namespace ChnDPEncoder.Models;

using System.Collections.Frozen;

/// <summary> 键盘布局表 </summary>
internal sealed class LayoutMap
{
    /// <summary> 键盘布局表 </summary>
    private readonly FrozenDictionary<char, (byte? Finger, byte? Row)> _layoutMap;

    /// <summary> 解析键盘布局表 </summary>
    public LayoutMap(ReadOnlySpan<string> layout) {
        Dictionary<char, (byte? Finger, byte? Row)> layoutMap = new(46); // 默认46键
        for (byte i = 0; i < 9; i++)
            foreach (var c in layout[i])
                layoutMap[c] = (i, null);
        for (byte i = 0; i < 5; i++)
            foreach (var c in layout[i + 9])
                layoutMap[c] = (layoutMap.TryGetValue(c, out var prev)
                    ? prev.Finger
                    : null, i);
        _layoutMap = layoutMap.ToFrozenDictionary();
    }

    /// <summary> 获取键的手指和排号：Finger 0-7为左小指-右小指，8为拇指，Row 0-4为数字排-空格排，null为缺失 </summary>
    public (byte? Finger, byte? Row) this[char key] =>
        _layoutMap.TryGetValue(key, out var result)
            ? result
            : (null, null);
}
