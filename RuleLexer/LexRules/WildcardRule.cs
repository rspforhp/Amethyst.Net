namespace RuleLexer.LexRules;

public struct WildcardRule : ILexRule<WildcardRule>
{
    public override string ToString()
    {
        return $"\\*";
    }

   public static CharRangeRule Wildcard=new CharRangeRule(char.MinValue, char.MaxValue);
    public UnmanagedString AfterRead { get; set; }
    public bool Read(ref StringLexer reader, out string read)
    {
        return reader.Read(ref Wildcard, out read);
    }
}