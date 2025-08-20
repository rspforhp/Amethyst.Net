using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct Character : IClassCharacter<Character>
{
    public readonly Rune Match;

    public Character(Rune match)
    {
        Match = match;
    }

    public static Character? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();
        if (sequence.ReadChar() == null)
        {
            sequence.PopPosition();
            return null;
        }
        sequence.PopPosition();
        
        
        sequence.PushPosition();
        if (sequence.ReadChar() == "(")
        {
            sequence.PopPosition();
            return null;
        }
        sequence.PopPosition();
        
        
        
        sequence.PushPosition();
        if (sequence.ReadChar() == ")")
        {
            sequence.PopPosition();
            return null;
        }
        sequence.PopPosition();
        
        
        return new Character(CharacterEscape.HandleEscape(ref sequence));
    }

    public bool Matches(Rune r)
    {
        return Match == r;
    }

    public Quantifier? Quantifier { get; set; }
}