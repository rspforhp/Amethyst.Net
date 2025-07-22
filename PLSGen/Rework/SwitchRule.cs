using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct SwitchRule : ILexRule,IProvideClone<SwitchRule>
{
    public string DebuggerDisplay()
    {
        var t = string.Join(", ", this.Rules.Select(a => a.DebuggerDisplay()));
        return $"switch({t})";
    }
    public override int GetHashCode()
    {
        return string.Join("",Rules.Select(a=>a.GetHashCode())).GetHashCode();
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

        foreach (var lexRule in Rules)
        {
            var r=lexRule.Lex(ref reader,out var s);
            if (!r) continue;
            readText = s;
            return true;
        }

        readText = null;
        return false;
    }

    public readonly IReadOnlyList<ILexRule> Rules;

    public SwitchRule(params IReadOnlyList<ILexRule> rules)
    {
        Rules = rules;
        CloningProp = () => new SwitchRule(rules);
    }

  
    public SwitchRule(params IReadOnlyList<ILexRuleConvertible> rules)
    {
        Rules = rules.Select(a=>a.GetRule()).ToList();
        var rule = Rules;
        CloningProp = () => new SwitchRule(rule);
    }
    public bool Validate()
    {
        return true;
    }

    public Func<Object> CloningProp { get; set; }
}