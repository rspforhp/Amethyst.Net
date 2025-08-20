using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public interface IClassCharacter
{
    public static abstract IClassCharacter? Parse(ref AdvancedStringReader sequence);
    public bool Matches(Rune r);
    public  virtual string ReadWithoutQuantifier(ref AdvancedStringReader r)
    {
        r.PushPosition();
        var read = r.ReadChar();
        if (Matches(read.ToRune()))
        {
            r.DontPopPosition();
            return read;
        }
        r.PopPosition();
        return null;
    }
    public string ReadWithQuantifier(ref AdvancedStringReader r)
    {
        Quantifier ??= new Quantifier(QuantifierEnum.ExactlyNTimes, 1);
        return Quantifier?.Read(ref r,this);
    }
    public Quantifier? Quantifier { get; set; }
    
}

public interface IClassCharacter<T> : IClassCharacter where T : struct, IClassCharacter<T>
{
    static IClassCharacter? IClassCharacter.Parse(ref AdvancedStringReader sequence)
    {
        return T.Parse(ref sequence);
    }

    public static abstract T? Parse(ref AdvancedStringReader sequence);
  
}

public static class ClassCharacter
{
    public static IClassCharacter ParseWithoutQuantifier(ref AdvancedStringReader sequence)
    {
        if (AlternationConstruct.Parse(ref sequence) is { } a) return a;
        return ParseWithoutQuantifierNoAlternation(ref sequence);
    }
    public static IClassCharacter ParseWithoutQuantifierNoAlternation(ref AdvancedStringReader sequence)
    {
        if (GroupReference.Parse(ref sequence) is { } grf) return grf;
        if (GroupConstruct.Parse(ref sequence) is { } g1) return g1;
        if (CharacterGroup.Parse(ref sequence) is { } cg) return cg;
        if (UnicodeCategory.Parse(ref sequence) is { } u) return u;
        if (Wildcard.Parse(ref sequence) is { } w) return w;
        if (Character.Parse(ref sequence) is { } c) return c;
        return null;
    }
    public static IClassCharacter ParseWithQuantifierNoAlternation(ref AdvancedStringReader sequence)
    {
        var i = ParseWithoutQuantifierNoAlternation(ref sequence);
        if(i!=null)
            i.Quantifier = Quantifier.Parse(ref sequence);
        //TODO: handle quantifiers
        return i;
    }
    public static IClassCharacter ParseWithQuantifier(ref AdvancedStringReader sequence)
    {
        var i = ParseWithoutQuantifier(ref sequence);
        if(i!=null)
            i.Quantifier = Quantifier.Parse(ref sequence);
        //TODO: handle quantifiers
        return i;
    }

    public static IClassCharacter ParseWithQuantifier( AdvancedStringReader sequence)
    {
        return ParseWithQuantifier(ref sequence);
    }
    public static IClassCharacter Get( AdvancedStringReader sequence)
    {
        return ParseWithQuantifier(ref sequence);
    }
    public static IClassCharacter Get( string sequence)
    {
        return Get(new AdvancedStringReader(sequence));
    }
    public static T Get<T>( string sequence) where T : struct,IClassCharacter<T>
    {
        var r = Get(new AdvancedStringReader(sequence));
        if (r is T t) return t;
        throw new Exception();
    }
}