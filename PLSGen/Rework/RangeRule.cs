using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct RangeRule : ILexRule, IProvideClone<RangeRule>, IReversible<RangeRule>
{
    public string DebuggerDisplay()
    {
        char f =(char) CharRange.Start.Value;
        char e = (Char)CharRange.End.Value;
        return $"(\'{f}\'..\'{e}\')";
    }
    public override int GetHashCode()
    {
        char f =(char) CharRange.Start.Value;
        char e = (Char)CharRange.End.Value;
        return $"{f}..{e}".GetHashCode();
    }
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }
    public bool _Lex(ref SimpleStringReader reader, out string readText)
    {
        bool result = false;
        char c = '\0';
        if(!Reverse)
            result = reader.Exists(CharRange, true,out  c);
        else result = reader.ExistsReverse(CharRange, true,out  c);
        readText = c.ToString();
        return result;
    }

    public readonly Range CharRange;
    public RangeRule(Range charRange)
    {
        CharRange = charRange;
        CloningProp = () => new RangeRule(charRange);
    }
    public RangeRule(char f,char e)
    {
        CharRange=(f..e);
        CloningProp = () => new RangeRule(f,e);
    }


    public bool Validate()
    {
        return true;
    }

    public Func<Object> CloningProp { get; set; }
    public bool Reverse { get; set; }
}

public static class RangeExtension
{
    public static bool InRangeInclusive(this Range r,int val)
    {
        var f = r.Start.Value;
        var e = r.End.Value;
        return val >= f && val <= e;
    }
    public static bool InRangeExclusive(this Range r,int val)
    {
        var f = r.Start.Value;
        var e = r.End.Value;
        return val >= f && val < e;
    }
}