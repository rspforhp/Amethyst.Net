using System.Diagnostics;

namespace PLGSGen.Rework;

public static class StringRule_Ext
{
    public static StringRule Rule(this string str) => new(str);
    public static StringRule StrRule(this object str) => new(str.ToString());
}
[DebuggerDisplay("{DebuggerDisplay()}")]
public struct StringRule : ILexRule, IProvideClone<StringRule>
{
    public override int GetHashCode()
    {
        return ToRead.GetHashCode();
    }

    public string DebuggerDisplay()
    {
        return $"\"{ToRead}\"";
    }
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }
    public bool _Lex(ref SimpleStringReader reader, out string readText)
    {
        bool result = false;
        result = reader.Exists(ToRead, true);
        readText = ToRead;
        return result;
    }

    public readonly string ToRead;

    public StringRule(string toRead)
    {
        ToRead = toRead;
        CloningProp = ()=>new StringRule(toRead);
    }

    public bool Validate()
    {
        return true;
    }

    public  Func<Object> CloningProp { get; set; }
}