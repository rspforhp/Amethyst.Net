using UnmanageUtility;

namespace RuleLexer.LexRules;

public struct CharListRule : ILexRule<CharListRule>
{
    public UnmanagedString AfterRead { get; set; }
    public static CharListRule operator ~(in CharListRule r)
    {
        CharListRule copy = r;
        copy.Reverse = !copy.Reverse;
        return copy;
    }
    public readonly UnmanagedArray<char> CharArray;
    public bool Reverse;

    public CharListRule(params char[] ar)
    {
        CharArray= new UnmanagedArray<char>(ar);
        Reverse = false;
    }
    public override string ToString()
    {
        List<char> charList = new();
        for (int i = 0; i < CharArray.Length; i++)
        {
            char c = CharArray[i];
            charList.Add(c);
        }
        return $"{(Reverse?"~":"")}[{string.Join(", ",charList)}]";
    }
    public bool Read(ref StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        if (b.Length == 0)
        {
            read = "";
            return false;
        }
        char c = b[0];
        if (CharArray.Contains(c) ^ Reverse)
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}