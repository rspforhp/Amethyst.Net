using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct CharArrayRule : ILexRule, IProvideClone<CharArrayRule>, IReversible<CharArrayRule>
{
    public string DebuggerDisplay()
    {
        return $"array(\"{string.Join("",CharArray)}\")";
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
            result = reader.Exists(CharArray, true,out  c);
        else result = reader.ExistsReverse(CharArray, true,out  c);
        readText = c.ToString();
        return result;
    }

    public readonly char[] CharArray;
    public CharArrayRule(params char[] charRange)
    {
        CharArray = charRange;
        CloningProp = () => new CharArrayRule(charRange);
    }
  


    public bool Validate()
    {
        return true;
    }

    public Func<Object> CloningProp { get; set; }
    public bool Reverse { get; set; }
}

