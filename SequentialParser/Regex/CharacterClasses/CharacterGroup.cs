using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct CharacterGroup : IClassCharacter<CharacterGroup>
{
    public readonly RuneRange[] Group;
    public readonly bool Negated;

    public CharacterGroup(RuneRange[] group, bool negated = false)
    {
        Group = group;
        Negated = negated;
    }

    public static CharacterGroup? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();
            if (sequence.ReadChar() != "[")
            {
                sequence.PopPosition();
                return null;
            }
            sequence.PushPosition();
            bool negated = sequence.ReadChar() == "^";
            if(!negated)sequence.PopPosition();
            else sequence.DontPopPosition();
            
            string insides = "";

           
            while (true)
            {
                var sc = CharacterEscape.HandleEscape(ref sequence);
                if (sc.ToString() == "]")
                    break;
                insides += sc;
            }
            AdvancedStringReader innerReader = new AdvancedStringReader(insides).CopyHandlers(ref sequence);
            Stack<RuneRange> runes = new();
            while (innerReader.TryReadChar(out var sc))
            {
                if (sc == "-")
                {
                    var from = runes.Pop();
                    if (from.From != from.To) throw new Exception();
                    if (innerReader.LeftString().Length == 0)
                    {
                        break;
                    }
                    var to = innerReader.ReadChar().ToRune();
                    runes.Push(new RuneRange(from.From, to));
                }
                else runes.Push(sc.ToRune());
            }

            sequence.DontPopPosition();
            if (runes.Count == 0) throw new Exception();
            return new CharacterGroup(runes.Reverse().ToArray(), negated);
    }


    public bool Matches(Rune r)
    {
        return Negated ? !Group.Contains(r) : Group.Contains(r);
    }

    public Quantifier? Quantifier { get; set; }
}