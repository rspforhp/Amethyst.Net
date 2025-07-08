namespace RuleLexer.LexRules;

public struct StringRule : ILexRule<StringRule>
{
    public readonly UnmanagedString Value;

    public override string ToString()
    {
        return Value;
    }

    public StringRule(string value)
    {
        Value = value;
    }

    public static implicit operator StringRule(string str) => new StringRule(str);

    public UnmanagedString AfterRead { get; set; }

    public bool Read(ref StringLexer reader, out string read)
    {
        if (reader.Exists(Value, true))
        {
            read = Value;
            return true;
        }
        read = "";
        return false;
    }
}