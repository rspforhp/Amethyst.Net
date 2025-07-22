using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct EachRule : ILexRule,IProvideClone<EachRule>
{
    public override int GetHashCode()
    {
        return string.Join("",Rules.Select(a=>a.GetHashCode())).GetHashCode();
    }
    public string DebuggerDisplay()
    {
        var t = string.Join(", ", this.Rules.Select(a => a.DebuggerDisplay()));
        return $"each({t})";
    }
    
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }

    public bool _Lex(ref SimpleStringReader reader, out string readText)
    {
        //I think returning true makes sense if we have nothing in it
        if (Rules.Count == 0)
        {
            readText = null;
            return true;
        }
        readText = "";
        foreach (var lexRule in Rules)
        {
            var r=lexRule.Lex(ref reader,out var s);
            if (!r)
            {
                readText = null;
                return false;
            }
            readText += s;
        }
        return true;
    }

    public readonly IReadOnlyList<ILexRule> Rules;

    public EachRule(params IReadOnlyList<ILexRule> rules)
    {
        Rules = rules;
        CloningProp = () => new EachRule(rules);
    }
    public EachRule(params IReadOnlyList<ILexRuleConvertible> rules)
    {
        Rules = rules.Select(a=>a.GetRule()).ToList();
        var rule = Rules;
        CloningProp = () => new EachRule(rule);
    }
    public bool Validate()
    {
        return true;
    }

    public Func<Object> CloningProp { get; set; }
}