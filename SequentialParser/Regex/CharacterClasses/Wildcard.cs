using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct Wildcard : IClassCharacter<Wildcard>
{
    public static Wildcard? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();
        if (sequence.ReadChar() == ".")
        {
            sequence.DontPopPosition();
            return new Wildcard();
        }
        sequence.PopPosition();
        return null;
    }

    

    public bool Matches(Rune r)
    {
        return r.ToString() != "\n";
    }

   

    public Quantifier? Quantifier { get; set; }
}