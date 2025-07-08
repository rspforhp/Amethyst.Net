namespace RuleLexer.LexRules;

public struct AlwaysFalse : ILexRule<AlwaysFalse>
{
    public UnmanagedString AfterRead { get; set; }
    public override string ToString()
    {
        return $"False";
    }

    public bool Read(ref StringLexer reader, out string read)
    {
        read = "";
        return false;
    }
}