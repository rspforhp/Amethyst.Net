
using System.Diagnostics;
using System.Text;

public static class RangeExtension
{
    public static bool InRangeInclusive(this Range r,int val)
    {
        var f = r.Start.Value;
        var e = r.End.Value;
        return val >= f && val <= e;
    }
    public static bool InRangeExclusive(this Range r,int val)
    {
        var f = r.Start.Value;
        var e = r.End.Value;
        return val >= f && val < e;
    }
}

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
        if (Position > l) return string.Empty;
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
                var t = Peek(1);
                if (t == string.Empty) break;
                curRead += t;
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

    public string WriteQuotedString()
    {
        StringBuilder builder = new();

        while (true)
        {
            var s = this.Read(1);
            if (s == string.Empty) break;
            char read = s[0];
            switch (read)
            {
                case '\'':
                    builder.Append("\\\'");
                    break;
                case '\"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\0':
                    builder.Append("\\0");
                    break;
                case '\a':
                    builder.Append("\\a");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '\v':
                    builder.Append("\\v");
                    break;
                default:
                    builder.Append(read);
                    break;
            }
        }

        return builder.ToString();
    }
    public string ReadQuotedString()
    {
        if (!Exists("\"", true)) return null;
        StringBuilder builder = new();

        while (true)
        {
            char read = this.Read(1)[0];
            if (read == '\\')
            {
                char escapedChar = Read(1)[0];
                switch (escapedChar)
                {
                    case 'u':
                        break;
                    case 'U':
                        break;
                    case 'x':
                        break;
                    case '\'':
                        builder.Append('\'');
                        break;
                    case '\"':
                        builder.Append('\"');
                        break;
                    case '\\':
                        builder.Append('\\');
                        break;
                    case '0':
                        builder.Append('\0');
                        break;
                    case 'a':
                        builder.Append('\a');
                        break;
                    case 'b':
                        builder.Append('\b');
                        break;
                    case 'f':
                        builder.Append('\f');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    case 'v':
                        builder.Append('\v');
                        break;
                    default:
                        throw new Exception("What are you trying to escape?");
                }
            }
            else
            {
                if (read == '\"') break;
                builder.Append(read);
            }
        }

        return builder.ToString();
    }
}