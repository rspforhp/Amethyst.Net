using System.Text;

namespace SequentialParser.Regex;

public static class CharacterEscape
{
    public static Rune ToRune(this string str)
    {
        switch (str.Length)
        {
            case 1:
            {
                char t = str[0];
                return new Rune(t);
            }
            case 2:
            {
                char t1 = str[0];
                char t2 = str[1];
                return new Rune(t1, t2);
            }
            default:
                throw new Exception();
        }
    }
    public static Rune ToRune(this char str)
    {
        return new Rune(str);
    }
    public static Rune HandleEscape(ref AdvancedStringReader reader)
    {
        var _1 = reader.ReadChar();
        if (_1 == "("||_1 == ")") throw new Exception("NO");
        if (_1 != "\\") return _1.ToRune();
        var _2 = reader.ReadChar();
        var _12 = _1 + _2;
        if (_12 == @"\a")
            return new Rune('\a');
        if (_12 == @"\b")
            return new Rune('\b');
        if (_12 == @"\t")
            return new Rune('\t');
        if (_12 == @"\r")
            return new Rune('\r');
        if (_12 == @"\v")
            return new Rune('\v');
        if (_12 == @"\f")
            return new Rune('\f');
        if (_12 == @"\n")
            return new Rune('\n');
        if (_12 == @"\e")
            return new Rune('\e');
        
        reader.PushPosition();

        var _3 = reader.ReadChar();
        var _123 = _12 + _3;
        if (_123 is ['\\', 'c', _] && ParseControl(_123[2..]) is { } r3)
            return r3.ToRune();
        var _4 = reader.ReadChar();
        var _1234 = _123 + _4;
        if (_1234 is ['\\', _, _, _] && ParseOctal(_1234[1..]) is { } r1)
            return r1.ToRune();
        if (_1234 is ['\\', 'x', _, _] && ParseHex(_1234[2..]) is { } r2)
            return r2.ToRune();
        var _123456 = _1234 + reader.Read(2);
        
        if (_123456 is ['\\', 'u', _, _, _, _] && ParseHex(_123456[2..]) is { } r4)
            return r4.ToRune();
        
        reader.PopPosition();
        
        return _2.ToRune();
    }
    private static string ParseOctal(string seq)
    {
        if (seq.Any(a =>! char.IsDigit(a))) return null;
        char c = ((char)Convert.ToInt32(seq, 8));
        return c==0?null:c.ToString();
    }
    private static string ParseHex(string seq)
    {
        if (seq.Any(a =>!char.IsAsciiHexDigit(a))) return null;
        string c = ((Rune)Convert.ToInt32(seq, 16)).ToString();
        return c;
    }
    private static string ParseControl(string seq)
    {
        //Not implemented
        return seq;
    }
   
}