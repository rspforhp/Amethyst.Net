using System.Text;
using SequentialParser.Regex.CharacterClasses;

namespace SequentialParser.Regex;

public enum QuantifierEnum
{
    ZeroOrMore,
    OneOrMore,
    ZeroOrOne,
    Optional=ZeroOrOne,
    ExactlyNTimes,
    AtleastNTimes,
    AtleastNTimesNoMoreThanMTimes,
}

public struct Quantifier
{
    public readonly QuantifierEnum Enum;
    public readonly uint N;
    public readonly uint M;

   
    public Quantifier(QuantifierEnum e, uint n = 0, uint m = 0)
    {
        Enum = e;
        N = n;
        M = m;
    }

    public static Quantifier? Parse(ref AdvancedStringReader sequence)
    {
        sequence.PushPosition();

        string first = sequence.ReadChar();

        if (first == "*")
        {
            sequence.DontPopPosition();
            return new Quantifier(QuantifierEnum.ZeroOrMore);
        }

        if (first == "+")
        {
            sequence.DontPopPosition();
            return new Quantifier(QuantifierEnum.OneOrMore);
        }

        if (first == "?")
        {
            sequence.DontPopPosition();
            return new Quantifier(QuantifierEnum.ZeroOrOne);
        }
        if (first == "{")
        {
            uint? n = null;
            uint? m = null;
            string ns = "";
            while (sequence.TryReadChar(out var sc))
            {
                if (sc == ",") break;

                if (sc == "}")
                {
                    sequence.DontPopPosition();
                    return new Quantifier(QuantifierEnum.ExactlyNTimes, uint.Parse(ns));
                }
                ns += sc;
            }
            n = uint.Parse(ns);
            sequence.PushPosition();
            if (sequence.ReadChar() == "}")
            {
                sequence.DontPopPosition();
                return new Quantifier(QuantifierEnum.AtleastNTimes, n.Value);
            }
            else sequence.PopPosition();
            
            string ms = "";
            while (sequence.TryReadChar(out var sc))
            {
                if (sc == "}")
                {
                     sequence.DontPopPosition();
                    return new Quantifier(QuantifierEnum.AtleastNTimesNoMoreThanMTimes, uint.Parse(ns),uint.Parse(ms));
                }
                ms += sc;
            }

        }
        
        
        sequence.PopPosition();
        return null;
    }

    public string Read(ref AdvancedStringReader sequence, IClassCharacter c)
    {
        StringBuilder b = new();
        uint amountDid = 0;
        switch (Enum)
        {
            case QuantifierEnum.ZeroOrMore:
            {
                for (; amountDid < uint.MaxValue; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return b.ToString();
            }
            case QuantifierEnum.OneOrMore:
            {
                for (; amountDid < uint.MaxValue; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return amountDid>0? b.ToString():null;
            }
            case QuantifierEnum.ZeroOrOne:
            {
                for (; amountDid < 1; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return b.ToString();
            }
            case QuantifierEnum.ExactlyNTimes:
            {
                for (; amountDid < N; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return amountDid==N? b.ToString():null;
            }
            case QuantifierEnum.AtleastNTimes:
            {
                for (; amountDid < uint.MaxValue; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return amountDid>=N? b.ToString():null;
            }
            case QuantifierEnum.AtleastNTimesNoMoreThanMTimes:
            {
                for (; amountDid < M; amountDid++)
                {
                    var read = c.ReadWithoutQuantifier(ref sequence);
                    if (read == null) break;
                    b.Append(read);
                }
                return amountDid>=N? b.ToString():null;
            }
        }
        throw new ArgumentOutOfRangeException();
    }
}