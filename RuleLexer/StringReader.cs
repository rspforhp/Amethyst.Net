using RuleLexer.LexRules;

namespace RuleLexer;

public struct StringLexer
{
    private string UnderlyingString;
    public uint Position;

    public string Peek(uint length)
    {
        var l=uint.Min(Position+length, (uint)UnderlyingString.Length);
        return UnderlyingString[(int)Position..((int)l)];
    }
    public bool Read<T>(ref T rule,out string read) where T : unmanaged, ILexRule<T>
    {
        rule.BeforeRead();
        var r= rule.Read(ref this, out read);
        rule.AfterReadMethod();
            rule.AfterRead = read;
        return r;
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