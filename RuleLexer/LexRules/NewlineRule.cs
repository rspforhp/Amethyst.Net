namespace RuleLexer.LexRules;

public struct NewlineRule : ILexRule<NewlineRule>
{
    public override string ToString()
    {
        return $"\\n";
    }
    public static OrRule<OrRule<OrRule<OrRule<StringRule, StringRule>, StringRule>, StringRule>, StringRule> Newline = "\r".Convert<StringRule>()
            .Or((StringRule)"\n")
            .Or((StringRule)"\u0085")
            .Or((StringRule)"\u2028")
            .Or((StringRule)"\u2029")
       ;

    public NewlineRule() 
    {
    }

    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Newline, out read);
    }
}