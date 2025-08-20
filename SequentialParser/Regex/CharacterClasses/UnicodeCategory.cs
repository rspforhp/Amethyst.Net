using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct UnicodeCategory : IClassCharacter<UnicodeCategory>
{
    public struct FuckNetUnicodeCategory
    {
        public List<System.Globalization.UnicodeCategory> Categories;

        public FuckNetUnicodeCategory(params List<System.Globalization.UnicodeCategory> c)
        {
            Categories = c;
        }

        public static implicit operator FuckNetUnicodeCategory(System.Globalization.UnicodeCategory u) =>
            new (u);


        public static FuckNetUnicodeCategory AllLetter = new FuckNetUnicodeCategory(System.Globalization.UnicodeCategory.LowercaseLetter,System.Globalization.UnicodeCategory.UppercaseLetter,System.Globalization.UnicodeCategory.TitlecaseLetter,System.Globalization.UnicodeCategory.ModifierLetter,System.Globalization.UnicodeCategory.OtherLetter);
        public static FuckNetUnicodeCategory AllCombiningMarks = new(System.Globalization.UnicodeCategory.SpacingCombiningMark,System.Globalization.UnicodeCategory.NonSpacingMark,System.Globalization.UnicodeCategory.EnclosingMark);

        public static FuckNetUnicodeCategory AllNumber = new(System.Globalization.UnicodeCategory.DecimalDigitNumber,
            System.Globalization.UnicodeCategory.LetterNumber, System.Globalization.UnicodeCategory.OtherNumber);

        public static FuckNetUnicodeCategory AllPunctuation = new(System.Globalization.UnicodeCategory.ConnectorPunctuation,System.Globalization.UnicodeCategory.DashPunctuation,System.Globalization.UnicodeCategory.OpenPunctuation,System.Globalization.UnicodeCategory.ClosePunctuation,System.Globalization.UnicodeCategory.InitialQuotePunctuation,System.Globalization.UnicodeCategory.FinalQuotePunctuation,System.Globalization.UnicodeCategory.OtherPunctuation);

        public static FuckNetUnicodeCategory AllSymbols = new FuckNetUnicodeCategory(
            System.Globalization.UnicodeCategory.CurrencySymbol, System.Globalization.UnicodeCategory.MathSymbol,
            System.Globalization.UnicodeCategory.ModifierSymbol, System.Globalization.UnicodeCategory.OtherSymbol);

        public static FuckNetUnicodeCategory AllSeparators = new(System.Globalization.UnicodeCategory.LineSeparator,System.Globalization.UnicodeCategory.ParagraphSeparator,System.Globalization.UnicodeCategory.SpaceSeparator);
        public static FuckNetUnicodeCategory AllOther = new(System.Globalization.UnicodeCategory.Control,System.Globalization.UnicodeCategory.Format,System.Globalization.UnicodeCategory.Surrogate,System.Globalization.UnicodeCategory.PrivateUse,System.Globalization.UnicodeCategory.OtherNotAssigned);
        public static FuckNetUnicodeCategory WordCharacter = new(System.Globalization.UnicodeCategory.LowercaseLetter,System.Globalization.UnicodeCategory.UppercaseLetter,System.Globalization.UnicodeCategory.TitlecaseLetter,System.Globalization.UnicodeCategory.OtherLetter,System.Globalization.UnicodeCategory.ModifierLetter,System.Globalization.UnicodeCategory.NonSpacingMark,System.Globalization.UnicodeCategory.DecimalDigitNumber,System.Globalization.UnicodeCategory.ConnectorPunctuation);

        public static FuckNetUnicodeCategory Whitespace = new();

        public readonly bool Matches(Rune r)
        {
            if (Categories==null||Categories.Count == 0)
            {
                return r.ToString() == "\f" || r.ToString() == "\n" || r.ToString() == "\n" || r.ToString() == "\v" || r.ToString() == "\t" || r.ToString() == "\x85" ||
                       Rune.GetUnicodeCategory(r) is System.Globalization.UnicodeCategory.LineSeparator
                           or System.Globalization.UnicodeCategory.ParagraphSeparator
                           or System.Globalization.UnicodeCategory.SpaceSeparator;
            }
            var unicodeCategory = Rune.GetUnicodeCategory(r);
            return this.Categories.Contains(unicodeCategory);
        }
    }
    
    
    public readonly FuckNetUnicodeCategory Category;
    public readonly bool Negate;

    public static FuckNetUnicodeCategory ParseUniCategory(string name)
    {
        return name switch
        {
            
            
            
            
            "Lu" => System.Globalization.UnicodeCategory.UppercaseLetter,
            "Ll" => System.Globalization.UnicodeCategory.LowercaseLetter,
            "Lt" => System.Globalization.UnicodeCategory.TitlecaseLetter,
            "Lm" => System.Globalization.UnicodeCategory.ModifierLetter,
            "Lo" => System.Globalization.UnicodeCategory.OtherLetter,
            "L" => FuckNetUnicodeCategory.AllLetter,
            "Mn" => System.Globalization.UnicodeCategory.NonSpacingMark,
            "Mc" => System.Globalization.UnicodeCategory.SpacingCombiningMark,
            "Me" => System.Globalization.UnicodeCategory.EnclosingMark,
            "M" => FuckNetUnicodeCategory.AllCombiningMarks,
            "Nd" => System.Globalization.UnicodeCategory.DecimalDigitNumber,
            "Nl" => System.Globalization.UnicodeCategory.LetterNumber,
            "No" => System.Globalization.UnicodeCategory.OtherNumber,
            "N" => FuckNetUnicodeCategory.AllNumber,
            "Pc" => System.Globalization.UnicodeCategory.ConnectorPunctuation,
            "Pd" => System.Globalization.UnicodeCategory.DashPunctuation,
            "Ps" => System.Globalization.UnicodeCategory.OpenPunctuation,
            "Pe" => System.Globalization.UnicodeCategory.ClosePunctuation,
            "Pi" => System.Globalization.UnicodeCategory.InitialQuotePunctuation,
            "Pf" => System.Globalization.UnicodeCategory.FinalQuotePunctuation,
            "Po" => System.Globalization.UnicodeCategory.OtherPunctuation,
            "P" => FuckNetUnicodeCategory.AllPunctuation,
            "Sm" => System.Globalization.UnicodeCategory.MathSymbol,
            "Sc" => System.Globalization.UnicodeCategory.CurrencySymbol,
            "Sk" => System.Globalization.UnicodeCategory.ModifierSymbol,
            "So" => System.Globalization.UnicodeCategory.OtherSymbol,
            "S" => FuckNetUnicodeCategory.AllSymbols,
            "Zs" => System.Globalization.UnicodeCategory.SpaceSeparator,
            "Zl" => System.Globalization.UnicodeCategory.LineSeparator,
            "Zp" => System.Globalization.UnicodeCategory.ParagraphSeparator,
            "Z" => FuckNetUnicodeCategory.AllSeparators,
            "Cc" => System.Globalization.UnicodeCategory.Control,
            "Cs" => System.Globalization.UnicodeCategory.Surrogate,
            "Co" => System.Globalization.UnicodeCategory.PrivateUse,
            "Cn" => System.Globalization.UnicodeCategory.OtherNotAssigned,
            "Cf" => FuckNetUnicodeCategory.AllOther,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
    public UnicodeCategory(FuckNetUnicodeCategory unicodeCategory, bool negate=false)
    {
        Category = unicodeCategory;
        Negate = negate;
    }
    public UnicodeCategory(string unicodeCategory, bool negate=false):this(ParseUniCategory(unicodeCategory),negate)
    {
      
    }

    public static UnicodeCategory? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();

        var first = sequence.ReadChar();
        if (first == "\\")
        {
            var second = sequence.ReadChar();
            if(second is "p" or "P")goto exit;
            switch (second)
            {
                case "W":
                    sequence.DontPopPosition();               
                    return new UnicodeCategory(FuckNetUnicodeCategory.WordCharacter, true); 
                    break;
                case "w":                                                                               
                    sequence.DontPopPosition();                                                         
                    return new UnicodeCategory(FuckNetUnicodeCategory.WordCharacter, false);             
                    break;                                                                       
                case "D":                                                                               
                    sequence.DontPopPosition();                                                         
                    return new UnicodeCategory(new FuckNetUnicodeCategory(System.Globalization.UnicodeCategory.DecimalDigitNumber), true);             
                    break;                                                                              
                case "d":                                                                               
                    sequence.DontPopPosition();                                                         
                    return new UnicodeCategory(new FuckNetUnicodeCategory(System.Globalization.UnicodeCategory.DecimalDigitNumber), false);            
                    break;         
                case "S":                                                                               
                    sequence.DontPopPosition();                                                         
                    return new UnicodeCategory(FuckNetUnicodeCategory.Whitespace, true);             
                    break;                                                                              
                case "s":                                                                               
                    sequence.DontPopPosition();                                                         
                    return new UnicodeCategory(FuckNetUnicodeCategory.Whitespace, false);            
                    break;      
            }
        }
        sequence.PopPosition();
        exit:
        
        sequence.PushPosition();
        string start = sequence.Read(3);
        bool s = start==(@"\p{");
        bool negate = start==(@"\P{");
        if (negate) s = true;
        if (s )
        {
            string name = "";
            while (sequence.TryReadChar(out var sc))
            {
                if (sc == "}")
                    break;
                name += sc;
            }
            sequence.DontPopPosition();
            return new UnicodeCategory(name, negate);
        }
        sequence.PopPosition();
        return null;
    }

    public bool Matches(Rune r)
    {
        return this.Category.Matches(r);
    }

    public Quantifier? Quantifier { get; set; }
}