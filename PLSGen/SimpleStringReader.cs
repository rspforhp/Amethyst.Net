
using System.Diagnostics;
using System.Text;

namespace PLGSGen;

[DebuggerDisplay("{LeftString()}")]
public struct SimpleStringReader
{
    private string UnderlyingString;
    public uint Position;

    public string LeftString()
    {
        return UnderlyingString[(int)Position..];
    }
    
    public string Peek(uint length)
    {
        var l=Math.Min(Position+length, (uint)UnderlyingString.Length);
        return UnderlyingString[(int)Position..((int)l)];
    }

    public string Read(uint length)
    {
        var s = Peek(length);
        Position += (uint)s.Length;
        return s;
    }

    public string ReadUntill(Func<string, bool> validator)
    {
        string curRead = "";
        string resultingString = "";
        curRead += Peek(1);
        var r = !validator(curRead);
        if(r)
            do
            {
                resultingString = curRead;
                Position++;
                curRead += Peek(1);
            } while (!validator(curRead) || curRead==resultingString);
        return resultingString;
    }

    public void SkipWhitespace()
    {
        string p = Peek(1);
        
        while (p.Length > 0 && string.IsNullOrWhiteSpace(p))
        {
            Position++;
            p = Peek(1);
        }
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

    public SimpleStringReader()
    {
    }

    public SimpleStringReader(string underlyingString)
    {
        UnderlyingString = underlyingString;
    }
}