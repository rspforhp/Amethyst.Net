using RuleLexer;
using RuleLexer.LexRules;

namespace AmethystLexer.Literal;

public struct  BooleanLiteral : ILexRule<BooleanLiteral>.WithValue<bool>
{
    public static readonly StringRule True = "true";
    public static readonly StringRule False = "false";
    public static readonly OrRule<StringRule, StringRule> Boolean = True.Or(False);
    

    public BooleanLiteral()
    {
        
    }

    public UnmanagedString AfterRead { get; set; }

    public OrRule<StringRule, StringRule> Rule=Boolean;
    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Rule, out read);
    }

    public void AfterReadMethod()
    {
        switch (Rule.AfterRead)
        {
            case "true":
                Value = true;
                break;
            case "false":
                Value = false;
                break;
        }
    }

    public bool? Value { get; set; }
}