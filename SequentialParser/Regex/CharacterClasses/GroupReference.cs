using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct GroupReference : IClassCharacter<GroupReference>
{
    public readonly string Reference;

    public GroupReference(string reference)
    {
        Reference = reference;
    }

    public static GroupReference? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();

        var s = sequence.ReadChar();
        if (s != "@")
        {
            sequence.PopPosition();
            return null;
        }

        string name = "";
        while (sequence.TryReadChar(out var sc))
        {
            if (sc == "@") break;
            name += sc;
        }

        if (name.Length > 0)
        {
            sequence.DontPopPosition();
            return new GroupReference(name);
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
        var gr = r.NamedGroups[this.Reference];
        if (gr == null) throw new Exception();
        r.PushPosition();
        var rea = r.Read(gr.Value);
        if (rea == null)
        {
            r.PopPosition();
            return null;
        }
        
        r.DontPopPosition();
        return rea;
    }

    public Quantifier? Quantifier { get; set; }
}