using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct ListRule : ILexRule, IProvideClone<ListRule>, IReversible<ListRule>
{
    public string DebuggerDisplay()
    {
        return $"array({Element.DebuggerDisplay()})";
    }
    public override int GetHashCode()
    {
        return $"array {Element.GetHashCode()}".GetHashCode();
    }
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }
    public bool _Lex(ref SimpleStringReader reader, out string readText)
    {
        ILexRule cl = Element.CloneRule();
        readText = "";
        while (cl.Lex(ref reader, out var s))
        {
            readText += s;
            Rules.Add(cl);
            cl = Element.CloneRule();
        }
        return true;
    }

    public readonly ILexRule Element;
    public readonly List<ILexRule> Rules;
    public ListRule(ILexRule rule)
    {
        Rules = new();
        Element = rule;
        CloningProp = () => new ListRule(rule);
    }


    public bool Validate()
    {
        return Rules.Count > 0;
    }

    public Func<Object> CloningProp { get; set; }
    public bool Reverse { get; set; }
}