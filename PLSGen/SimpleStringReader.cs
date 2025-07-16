
using System.Diagnostics;
using System.Text;
using PLGSGen.Rework;

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
    public bool Exists(char toRead, bool adjustIfTrue)
    {
        var l = 1;
        var s = Peek((uint)l)[0];
        if (s!=toRead) return false;
        if (adjustIfTrue)
            Position += 1;
        return true;
    }
    public bool Exists(char[] toRead, bool adjustIfTrue,out char ReadChar)
    {
        var l = 1;
        var s = Peek((uint)l)[0];
        ReadChar = s;
        if (!toRead.Contains(s)) return false;
        if (adjustIfTrue)
            Position += 1;
        return true;
    }
    public bool ExistsReverse(char[] toRead, bool adjustIfTrue,out char ReadChar)
    {
        var l = 1;
        var s = Peek((uint)l)[0];
        ReadChar = s;
        if (toRead.Contains(s)) return false;
        if (adjustIfTrue)
            Position += 1;
        return true;
    }
    public bool Exists(Range toRead, bool adjustIfTrue,out char ReadChar)
    {
        var l = 1;
        var s = Peek((uint)l)[0];
        ReadChar = s;
        if (!toRead.InRangeInclusive(s)) return false;
        if (adjustIfTrue)
            Position += 1;
        return true;
    }
    public bool ExistsReverse(Range toRead, bool adjustIfTrue,out char ReadChar)
    {
        var l = 1;
        var s = Peek((uint)l)[0];
        ReadChar = s;
        if (toRead.InRangeInclusive(s)) return false;
        if (adjustIfTrue)
            Position += 1;
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