using System.Diagnostics;

namespace PLGSGen.Rework;

[DebuggerDisplay("{DebuggerDisplay()}")]
public struct RuleNameRule : ILexRule, IProvideClone<RuleNameRule>, IReversible<RuleNameRule>
{
    public Func<object> CloningProp { get; set; }
    public string DebuggerDisplay()
    {
        return RuleName;
    }
    public override int GetHashCode()
    {
        GetRule();
        return FoundRule!=null?FoundRule.GetHashCode():0;
    }
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }
    public bool _Lex(ref SimpleStringReader reader, out string readText)
    {
        GetRule();
        return FoundRule.Lex(ref reader, out readText);
    }

    public readonly string RuleName;
    public ILexRule FoundRule;
    public static readonly Dictionary<string, ILexRule> RegisteredRules = new();

    public void GetRule()
    {
        FoundRule = RegisteredRules[RuleName];
    }

    public RuleNameRule(string ruleName)
    {
        RuleName = ruleName;
        CloningProp = () => new RuleNameRule(ruleName);
    }

    public bool Validate()
    {
        return true;
    }

    public bool Reverse { get; set; }
}