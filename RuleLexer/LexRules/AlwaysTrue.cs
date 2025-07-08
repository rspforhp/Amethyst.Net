namespace RuleLexer.LexRules;

public struct AlwaysTrue : ILexRule<AlwaysTrue>
{
    public UnmanagedString AfterRead { get; set; }
    public override string ToString()
    {
        return $"True";
    }

    public bool Read(ref StringLexer reader, out string read)
    {
        read = "";
        return true;
    }
}