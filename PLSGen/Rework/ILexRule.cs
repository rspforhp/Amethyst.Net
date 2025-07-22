namespace PLGSGen.Rework;

public interface ILexRule : IProvideClone
{
    public string DebuggerDisplay();
    public bool Optional { get; set; }
    public string Label { get; set; }
    public string LexedText { get; set; }
    public uint LexedPosition { get; set; }
    public bool HasLexed { get; set; }
    abstract bool _Lex(ref SimpleStringReader reader,out string readText);
    public bool Validate();

 
}
public struct ILexRuleConvertible
{
    private ILexRule Rule;
    private string StringRule;

    public static implicit operator ILexRuleConvertible(string str) => new() { StringRule = str };
    public static implicit operator ILexRuleConvertible(StringRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(CharArrayRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(EachRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(ListRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(RangeRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(RuleNameRule str) => new() { Rule = str };
    public static implicit operator ILexRuleConvertible(SwitchRule str) => new() { Rule = str };
    public ILexRule GetRule()
    {
        if (StringRule != null) return new StringRule(StringRule);
        return Rule;
    }
}

public static class ILexRuleExtension
{
    public static ILexRule CloneRule(this IProvideClone c)
    {
        return (ILexRule)c.CloningProp();
    }

    public static string GenerateRuleTree(this ILexRule r)
    {
        
        switch (r)
        {
            case StringRule s:
                return $"new StringRule(\"{new SimpleStringReader(s.ToRead).WriteQuotedString()}\")";
                break;
            case SwitchRule sw:
                return $"new SwitchRule({string.Join(", ",sw.Rules.Select(a=>a.GenerateRuleTree()))})";
                break;
        }
        return "";
    }
 
    
  
    //For structs
    public static bool Lex<T>(ref this T rule,ref SimpleStringReader reader,out string readText) where T : struct,ILexRule
    {
        bool result = false;
        rule.LexedPosition = reader.Position;
        result = rule._Lex(ref reader,out readText);
        rule.LexedText = readText;
        if (result) result = rule.Validate();
        if (!result)
        {
            reader.Position = rule.LexedPosition;
            rule.LexedText = null;
        }
        else rule.HasLexed = true;

        result |= rule.Optional;
        if (!result) readText = null;
        return result;
    }
    public static bool Lex<T>(ref this SimpleStringReader reader,ref T rule,out string readText) where T : struct, ILexRule
    {
        return rule.Lex(ref reader,out readText);
    }
    public static bool SingleLex<T>(ref this T rule, SimpleStringReader reader,out string readText) where T : struct, ILexRule
    {
        return rule.Lex(ref reader,out readText);
    }
    //
    
    
    //For classes
    public static bool Lex<T>( this T rule,ref SimpleStringReader reader,out string readText) where T : class,ILexRule
    {
        bool result = false;
        rule.LexedPosition = reader.Position;
        result = rule._Lex(ref reader,out readText);
        rule.LexedText = readText;
        if (result) result = rule.Validate();
        if (!result)
        {
            reader.Position = rule.LexedPosition;
            rule.LexedText = null;
        }
        else rule.HasLexed = true;
        return result || rule.Optional;
    }
    public static bool Lex<T>(ref this SimpleStringReader reader, T rule,out string readText) where T : class, ILexRule
    {
        return rule.Lex(ref reader,out readText);
    }
    public static bool SingleLex<T>( this T rule, SimpleStringReader reader,out string readText) where T : class, ILexRule
    {
        return rule.Lex(ref reader,out readText);
    }
    //
}