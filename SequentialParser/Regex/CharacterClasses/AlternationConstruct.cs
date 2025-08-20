using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct AlternationConstruct : IClassCharacter<AlternationConstruct>
{
    public readonly IClassCharacter First;
    public readonly IClassCharacter Second;

    private AlternationConstruct(IClassCharacter first, IClassCharacter second)
    {
        First = first;
        Second = second;
    }
    

    public static AlternationConstruct? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();

       var first = ClassCharacter.ParseWithQuantifierNoAlternation(ref sequence);
        if (first == null)
        {
            sequence.PopPosition();
            return null;
        }

        if (sequence.ReadChar() != "|")
        {
            sequence.PopPosition();
            return null;
        }

        var second= ClassCharacter.ParseWithQuantifier(ref sequence);
        if (second == null)
        {
            sequence.PopPosition();
            return null;
        }
        

        sequence.DontPopPosition();
        return new AlternationConstruct(first,second);
    }

    public bool Matches(Rune r)
    {
        return First.Matches(r) || Second.Matches(r);
    }

    public Quantifier? Quantifier { get; set; }
}