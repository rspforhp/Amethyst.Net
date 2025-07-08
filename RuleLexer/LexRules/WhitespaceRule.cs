namespace RuleLexer.LexRules;

public struct WhitespaceRule : ILexRule<WhitespaceRule>
{
    public override string ToString()
    {
        return $"\\t";
    }

    public static OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule>, StringRule> Whitespace = "\t".Convert<StringRule>().Or((StringRule)"\v").Or((StringRule)"\f")
        .Or((StringRule)"\u0020")
        .Or((StringRule)"\u00a0")
        .Or((StringRule)"\u1680")
        .Or((StringRule)"\u2000")
        .Or((StringRule)"\u2001")
        .Or((StringRule)"\u2002")
        .Or((StringRule)"\u2003")
        .Or((StringRule)"\u2004")
        .Or((StringRule)"\u2005")
        .Or((StringRule)"\u2006")
        .Or((StringRule)"\u2007")
        .Or((StringRule)"\u2008")
        .Or((StringRule)"\u2009")
        .Or((StringRule)"\u200A")
        .Or((StringRule)"\u202F")
        .Or((StringRule)"\u205f")
        .Or((StringRule)"\u3000");

    public WhitespaceRule() 
    {
    }

    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Whitespace, out read);
    }
}