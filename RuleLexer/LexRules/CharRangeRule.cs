namespace RuleLexer.LexRules;

public struct CharRangeRule : ILexRule<CharRangeRule>
{
    public static CharRangeRule operator ~(in CharRangeRule r)
    {
        //TODO: is this immutable? should be
        CharRangeRule copy = r;
        copy.Reverse = !copy.Reverse;
        return copy;
    }

    public static implicit operator CharRangeRule(System.Range r) => new CharRangeRule(r);
    public CharRangeRule(char from, char to)
    {
        From = from;
        To = to;
        Reverse = false;
    }
    public override string ToString()
    {
        return $"{(Reverse?"~":"")}({From}..{To})";
    }
    public readonly char From;
    public readonly char To;
    public bool Reverse;
    public CharRangeRule(System.Range range):this((char)range.Start.Value,(char)range.End.Value)
    {
        
    }

    public UnmanagedString AfterRead { get; set; }

    public bool Read(ref StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        if (b.Length == 0)
        {
            read = "";
            return false;
        }
        char c = b[0];
        bool aa = !Reverse && c >= From && c <= To;
        bool bb = Reverse && !(c >= From && c <= To);
        if (aa||bb)
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}