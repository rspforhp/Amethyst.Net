namespace AmethystParser.Lexing;

public struct StringLexer
{
    private string UnderlyingString;
    public uint Position;

    public string Peek(uint length)
    {
        var l=uint.Min(Position+length, (uint)UnderlyingString.Length);
        return UnderlyingString[(int)Position..((int)l)];
    }
    public string Read(LexRule rule,out bool succeed)
    {
        var b=rule.Read(this, out var s);
        succeed = b;
        return s;
    }

    public bool Exists(string toRead, bool adjustIfTrue)
    {
        var l = toRead.Length;
        var s = Peek((uint)l);
        if (!string.Equals(s, toRead)) return false;
        if (adjustIfTrue)
            Position += (uint)s.Length;
        return true;
    }

    public StringLexer()
    {
    }

    public StringLexer(string underlyingString)
    {
        UnderlyingString = underlyingString;
    }
}