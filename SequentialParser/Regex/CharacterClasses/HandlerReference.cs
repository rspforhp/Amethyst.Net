using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct HandlerReference : IClassCharacter<HandlerReference>
{
    public readonly string Reference;

    public HandlerReference(string reference)
    {
        Reference = reference;
    }

    public static HandlerReference? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();

        var s = sequence.ReadChar();
        if (s != "$")
        {
            sequence.PopPosition();
            return null;
        }

        string name = "";
        while (sequence.TryReadChar(out var sc))
        {
            if (sc == "$") break;
            name += sc;
        }

        if (name.Length > 0)
        {
            sequence.DontPopPosition();
            return new HandlerReference(name);
        }
        
        
        sequence.PopPosition();
        return null;
    }

    public bool Matches(Rune r)
    {
        return false;
    }

    public string ReadWithoutQuantifier(ref AdvancedStringReader r)
    {
        return "";
    }

    public Quantifier? Quantifier { get; set; }
}