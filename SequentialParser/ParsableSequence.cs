using System.Text;

namespace SequentialParser;

public abstract class AbstractParsableSequence : ParsableSequence
{
    
    [Obsolete("Not defined", true)]
    public override ParsableSequence Add(string sequenceString)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Not defined", true)]
    public override ParsableSequence Add(params List<SequenceReference> references)
    {
        throw new NotImplementedException();
    }

    [Obsolete("Not defined", true)]
    public override ParsableSequence Define(char theId, ParsableSequence subSequence)
    {
        throw new NotImplementedException();
    }
}
public sealed class FunctionParsableSequence : AbstractParsableSequence
{
    public Func<char, bool> Read;
 
    
    public static FunctionParsableSequence Digit = (FunctionParsableSequence)Make(char.IsDigit).Verify();
    public static FunctionParsableSequence Letter = (FunctionParsableSequence)Make(char.IsLetter).Verify();
    public static FunctionParsableSequence LetterOrDigit = (FunctionParsableSequence)Make(char.IsLetterOrDigit).Verify();
    public static FunctionParsableSequence WhiteSpace = (FunctionParsableSequence)Make(char.IsWhiteSpace).Verify();

    public static FunctionParsableSequence HexDigit = (FunctionParsableSequence)Make(char.IsAsciiHexDigit).Verify();
    public static FunctionParsableSequence BinDigit = (FunctionParsableSequence)Make(c=>c is '0' or '1').Verify();
    
    
    
    public static FunctionParsableSequence Make(Func<char, bool> toRead)
    {
        var t = new FunctionParsableSequence();
        t.Read = toRead;
        return t;
    }
    public override ParsableSequence Verify()
    {
        if (Read == null) throw new Exception("Reading function cant be null!");
        return this;
    }
    public override Dictionary<string, object> Parse(ref SimpleStringReader reader)
    {
        var rs = reader.Peek(1);
        char s = '\0';
        if (rs.Length > 0) s = rs[0];
        if (!Read(s))
        {
            return null;
        }

        reader.Position++;
        var ss = s.ToString();
        var d = new Dictionary<string, object>
        {
            [ReadStringKey] = ss
        };
        if (!DoValidatation(d)) d = null;
        return d;
    }
}

public sealed class StringSequence : AbstractParsableSequence
{
    public string[] Sequences;
    public static StringSequence Make(params string[] sequences)
    {
        var t = new StringSequence();
        t.Sequences = sequences;
        return t;
    }

    public Type EnumType;
    public static StringSequence Make<T>() where T : struct, Enum
    {
        var s = Make(Enum.GetNames<T>());
        s.EnumType = typeof(T);
        return s;
    }
    public override ParsableSequence Verify()
    {
        if (Sequences.Length == 0) throw new Exception("Sequences length cant be 0!");
        return this;
    }
    public override Dictionary<string, object> Parse(ref SimpleStringReader reader)
    {
        foreach (var seq in Sequences)
        {
            if (reader.Exists(seq,true))
            {
                var d=new Dictionary<string, object>()
                {
                    [ReadStringKey]=seq
                };
                if (EnumType != null)
                {
                    foreach (var enumValue in EnumType.GetEnumValues())
                    {
                        if (enumValue.ToString() == seq)
                        {
                            d[ExtraParsedValue] = enumValue;
                            break;
                        }
                    }
                }
                if (!DoValidatation(d)) d = null;
                return d;
            }
        }
        return null;
    }
}


public sealed class SwitchSequence : AbstractParsableSequence
{
    public List<ParsableSequence> Sequences;
    public static SwitchSequence Make(params List<ParsableSequence> sequences)
    {
        var t = new SwitchSequence();
        t.Sequences = sequences;
        return t;
    }
    public override ParsableSequence Verify()
    {
        if (Sequences.Count == 0) throw new Exception("Sequences length cant be 0!");
        return this;
    }
    public override Dictionary<string, object> Parse(ref SimpleStringReader reader)
    {
        ulong i = 0;
        foreach (var seq in Sequences)
        {
            var p = reader.Position;
            var d=seq.Parse(ref reader);
            if (d == null) reader.Position = p;
            if (d != null)
            {
                d[SwitchIndex] = i;
                if(!DoValidatation(d))
                    d = null;
            }
            if (d != null)
                return d;
            i++;
        }
        return null;
    }
}




public class ParsableSequence
{
    public event Func<string, Dictionary<string, object>, bool> Validate;

    public ParsableSequence AddValidation(Func<string, Dictionary<string, object>, bool> v)
    {
        Validate += v;
        return this;
    }
    public bool DoValidatation(Dictionary<string,object> d)
    {
        if (Validate == null) return true;
        else
        {
            bool r = true;
            foreach (var v in Validate.GetInvocationList())
            {
                var rb=v.DynamicInvoke((String)d[ReadStringKey], d);
                if ((bool)rb == false)
                {
                    r = false;
                    break;
                }
            }

            return r;
        }
    }
    
    public const string ReadStringKey = "READ_STRING";
    public const string SequenceIdKey = "SEQUENCE_ID_";
    public const string ExtraParsedValue = "PARSED_VALUE";
    public const string SwitchIndex = "SWITCH_INDEX";

    public virtual Dictionary<string, object> Parse(ref SimpleStringReader reader)
    {
        StringBuilder b = new();
        var d = new Dictionary<string, object>();
        for (int i = 0; i < References.Count; i++)
        {
            var refe = References[i];
            var prefe = GetFromReference(refe);
            ulong j = 0;
            for (; j < refe.MaxSize; j++)
            {
                //TODO: check if failed somehow
                var rd = prefe.Parse(ref reader);
                if (rd != null)
                {
                    d[SequenceIdKey + refe.Id + $"_{j}_@" + i] = rd;
                    b.Append(rd[ReadStringKey]);
                }
                else break;
            }

            if (j < refe.MinSize)
            {
                //throw new Exception($"Expected at least {refe.MinSize} elements, got {j}");
                return null;
            }
        }
        d[ReadStringKey] = b.ToString();   
        if (!DoValidatation(d)) d = null;
        return d;
    }

    public Dictionary<string, object> ParseOnce(SimpleStringReader reader) => Parse(ref reader);


    protected ParsableSequence()
    {
    }

    public record SequenceReference(char Id, ulong MinSize,ulong MaxSize);

    public ParsableSequence GetFromReference(SequenceReference refe)
    {
        return SequenceMap[refe.Id];
    }

    public readonly Dictionary<char, ParsableSequence> SequenceMap = new();
    public readonly List<SequenceReference> References = new();
    public static ParsableSequence Make() => new();

    public static bool IsId(char cur)
    {
        return !(char.IsDigit(cur) || cur == '[' || cur == ']');
    }

    public static (ulong min,ulong max) ReadSize(string sequenceString, ref int index)
    {
        index += 2;
        StringBuilder b = new();
        ulong min = 0;
        ulong max = 0;

        b.Clear();
        for (; index < sequenceString.Length; index++)
        {
            char cur = sequenceString[index];
            if (cur == ']')
            {
                if (b.Length == 0) return (0,ulong.MaxValue);
            }

            if (cur == '.')
            {
                var s = b.ToString();
                min = s == "inf" ? ulong.MaxValue : ulong.Parse(s);
                index++;
                break;
            }
            b.Append(cur);
        }
        
        b.Clear();
        for (; index < sequenceString.Length; index++)
        {
            char cur = sequenceString[index];
            if (cur == ']')
            {
                if (b.Length == 0)
                {
                    max = ulong.MaxValue;
                    break;
                }
                else
                {
                    var s = b.ToString();
                    max = s == "inf" ? ulong.MaxValue : ulong.Parse(s);
                    break;
                }
                
              
            }
            b.Append(cur);
        }
        if(min==0&&max==0)
            throw new Exception("Failed to parse size of sequence");
        return (min, max);
    }

    public static List<SequenceReference> ParseSequence(string sequenceString)
    {
        List<SequenceReference> l = new();
        for (int i = 0; i < sequenceString.Length; i++)
        {
            char cur = sequenceString[i];
            if (!IsId(cur)) continue;
            char? prev = null;
            char? next = null;
            if (i > 0) prev = sequenceString[i - 1];
            if (i < sequenceString.Length - 1) next = sequenceString[i + 1];

            char theId = cur;

            if (next == null || IsId(next.Value))
            {
                l.Add(new(theId, 1,1));
                continue;
            }

            if (next == '[')
            {
                var arraySize = ReadSize(sequenceString, ref i);
                l.Add(new(theId, arraySize.min,arraySize.max));
                continue;
            }


            else throw new Exception($"Sequence Parse exception. Unexpected char \'{cur}\' in \"{sequenceString}\"");
        }


        return l;
    }

    public virtual ParsableSequence Add(string sequenceString)
    {
        return this.Add(ParseSequence(sequenceString));
    }

    public virtual ParsableSequence Add(params List<SequenceReference> references)
    {
        this.References.AddRange(references);
        return this;
    }

    public virtual ParsableSequence Define(char theId, ParsableSequence subSequence)
    {
        SequenceMap.Add(theId, subSequence);
        return this;
    }

    //TODO: derive from dictionary to add some helpers regarding accessing the stuff via ids and what not

    public virtual ParsableSequence Verify()
    {
        foreach (var sequenceReference in References)
        {
            if (!SequenceMap.ContainsKey(sequenceReference.Id))
                throw new Exception($"Id \'{sequenceReference.Id}\' doesn't have an assigned sequence");
        }
        return this;
    }
}