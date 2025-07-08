using System.Text;
using AmethystParser.AmethystRules.Literal;

namespace AmethystParser.Lexing;

public sealed class ReverseCharRangeRule : LexRule
{
    public readonly CharRangeRule Rule;
    public static CharRangeRule operator ~(ReverseCharRangeRule r)
    {
        return r.Rule;
    }

    public override string ToString()
    {
        return $"~{Rule}";
    }

    public ReverseCharRangeRule(CharRangeRule rule):base(null)
    {
        Rule = rule;
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        char c = b[0];
        var From = Rule.From;
        var To = Rule.To;
        if (!(c>=From&&c<=To))
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}

public sealed class EnumRule<T> : LexRule, IHasValue<T> where T : struct, Enum
{
    public static string[] Names = Enum.GetNames<T>();
    public static T[] Values = Enum.GetValues<T>();
    public T Value { get; set; }
    public EnumRule() : base(null)
    {
    }

    public override string ToString()
    {
        return $"Enum<{typeof(T).Name}>";
    }

    public static EnumRule<T> Make() => new EnumRule<T>();

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        foreach (var @enum in Values)
        {
            if (!reader.Exists(@enum.ToString(), true)) continue;
            read = @enum.ToString();
            Value = @enum;
            return true;
        }

        read = "";
        return false;
    }

   
}

public sealed class ReverseCharListRule : LexRule
{
    public static CharListRule operator ~(ReverseCharListRule r)
    {
        return r.Rule;
    }
    public readonly CharListRule Rule;
    public ReverseCharListRule(CharListRule r):base(null)
    {
        Rule = r;
    }

    public override string ToString()
    {
        return $"~{Rule}";
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        if (b.Length == 0)
        {
            read = "";
            return false;
        }
        char c = b[0];
        if (!Rule.List.Contains(c))
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}
public sealed class CharListRule : LexRule
{
    public static ReverseCharListRule operator ~(CharListRule r)
    {
        return new ReverseCharListRule(r);
    }
    public readonly List<char> List;
    public override string ToString()
    {
        return $"[{string.Join(", ",List)}]";
    }

    public CharListRule(params char[] l):base(null)
    {
        List = l.ToList();
    }
    public static implicit operator CharListRule( char[] l)
    {
        return new CharListRule(l);
    }
    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        if (b.Length == 0)
        {
            read = "";
            return false;
        }
        char c = b[0];
        if (List.Contains(c))
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}

public sealed class CharRangeRule : LexRule
{
    public static ReverseCharRangeRule operator ~(CharRangeRule r)
    {
        return new ReverseCharRangeRule(r);
    }

    public override string ToString()
    {
        return $"({From}..{To})";
    }

    public readonly  char From;
    public readonly char To;


    public CharRangeRule(char from, char to):base(null)
    {
        From = from;
        To = to;
    }
    public CharRangeRule(Range range):this((char)range.Start.Value,(char)range.End.Value)
    {
    }
    public static implicit operator CharRangeRule(Range range)
    {
        return new CharRangeRule(range);
    }
 

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var b = reader.Peek(1);
        if (b.Length == 0)
        {
            read = "";
            return false;
        }
        char c = b[0];
        if (c>=From&&c<=To)
        {
            read = c.ToString();
            reader.Position++;
            return true;
        }
        read = "";
        return false;
    }
}

public sealed class AlwaysTrueRule : LexRule
{
    public override string ToString()
    {
        return $"True";
    }

    public AlwaysTrueRule() : base(null)
    {
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        read = "";
        return true;
    }
}
public sealed class StringRule : LexRule
{
    private string StrValue;

    public override string ToString()
    {
        return StrValue;
    }

    public static implicit operator StringRule(string s)
    {
       return new StringRule(s);
    }
    public StringRule(string str) : base(null)
    {
        StrValue = str;
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var b = reader.Exists(StrValue, true);
        if (b)
        {
            read = StrValue;
            return true;
        }

        read = "";
        return false;
    }
}

public sealed class OldAndRule : LexRule
{
    public readonly LexRule Left;
    public readonly LexRule Right;
    
    public OldAndRule(LexRule l,LexRule r) : base(null)
    {
        Left = l;
        Right = r;
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var lb = Left.Read(reader, out var ls);
        if (!lb)
        {
            read="";
            return false;
        }
        var rb = Right.Read(reader, out var rs);
        if (rb)
        {
            read = ls+rs;
            return true;
        }

        read = "";
        return false;
    }
}
public sealed class OldOrRule : LexRule
{
    public readonly LexRule Left;
    public readonly LexRule Right;
    
    public OldOrRule(LexRule l,LexRule r) : base(null)
    {
        Left = l;
        Right = r;
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var lb = Left.Read(reader, out var ls);
        if (lb)
        {
            read = ls;
            return true;
        }
        var rb = Right.Read(reader, out var rs);
        if (rb)
        {
            read = rs;
            return true;
        }

        read = "";
        return false;
    }
}

public sealed class ListRule : LexRule
{
    public override string ToString()
    {
        char a = '*';
        if (!CanHaveZero) a = '+';
        return $"{ElementRule}{a}";
    }

    public readonly bool CanHaveZero;
    public LexRule ElementRule { get; private set; }
    public LexRule KillRule { get; private set; }
    public ListRule(LexRule rule,bool canHaveZero) : base(null)
    {
        CanHaveZero = canHaveZero;
        ElementRule = rule;
    }

    public struct ElementPart
    {
        public readonly LexRule OriginalElement;
        public readonly string MyString;
        public readonly object MyValue;

        public ElementPart(LexRule originalElement, string myString, object myValue)
        {
            OriginalElement = originalElement;
            MyString = myString;
            MyValue = myValue;
        }
    }

    public readonly List<ElementPart> Elements = new();

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        var e = ElementRule;
        StringBuilder b = new();
        bool hasElements = false;
        string sOut;
        if (KillRule!=null&&KillRule.Read(reader, out var k))
        {
            read = k;
            return CanHaveZero;
        }
        while (e.Read(reader,out sOut))
        {

            object val = null;
            if (e is IHasValue ve) val = ve.Value;
            Elements.Add(new(e,sOut,val));
            
            hasElements = true;
            b.Append(sOut);
            if (KillRule!=null&&KillRule.Read(reader, out var killed))
            {
                b.Append(killed);
                break;
            }
            e=ElementRule;
        }
        read= b.ToString();
        return hasElements  || CanHaveZero;
    }
    public LexRule ElementAction(Action<LexRule> a)
    {
        ElementRule.Action(a);
        return this;
    }

    public LexRule Nongreedy(LexRule toKillOn)
    {
        KillRule = toKillOn;
        return this;
    }
}

public sealed class RuleCombo : LexRule
{
    public enum ComboType
    {
        Start,
        And,
        Or,
    }
    public struct ComboElement
    {
        public readonly ComboType ComboType;
        public readonly LexRule TheRule;

        public static string ComboToString(ComboType t)
        {
            switch (t)
            {
                case ComboType.Start:
                    return "";
                case ComboType.And:
                    return "& ";
                case ComboType.Or:
                    return "| ";
                default:
                    throw new ArgumentOutOfRangeException(nameof(t), t, null);
            }
        }
        public ComboElement(ComboType comboType, LexRule theRule)
        {
            ComboType = comboType;
            TheRule = theRule;
        }

        public override string ToString()
        {
            return $"{ComboToString(ComboType)}{TheRule}";
        }
    }


    public override string ToString()
    {
        return $"[{string.Join(" ",Elements)}]";
    }

    public List<ComboElement> Elements = new();

    public RuleCombo() :base(null)
    {
        
    }

    protected override bool ReadThis(StringLexer reader, out string read)
    {
        bool result = false;
        string outRead = "";

        ComboType lastType = ComboType.Start;
        string lastRead="";
        foreach (var comboElement in Elements)
        {
            lastType = comboElement.ComboType;
                switch (lastType)
            {
                case ComboType.Start:
                {
                    result = comboElement.TheRule.Read(reader, out lastRead);
                    if(result)
                      outRead += lastRead;
                }
                    break;
                case ComboType.And:
                {
                    if (!result) continue;
                    var curRes = comboElement.TheRule.Read(reader, out lastRead);
                    result = curRes;
                    if(result)
                     outRead += lastRead;
                }
                    break;
                case ComboType.Or:
                {
                    if (result) continue;
                    var curRes = comboElement.TheRule.Read(reader, out lastRead);
                    result = curRes;
                    if(result)
                     outRead += lastRead;
                }
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        read = outRead;
        return result;
    }
}

public interface IHasValue
{
    public object Value { get; set; }
}

public interface IHasValue<T> : IHasValue
{
    object IHasValue.Value
    {
        get => Value;
        set => Value = (T)value;
    }
    public T? Value { get; set; }
}
//TODO: rework rules to be STRUCTS, and IMMUTABLE
public class LexRule
{
    
    

    public T GetByLabel<T>(string label) where T : LexRule
    {
        if (MyLabel == label && this is T tr) return tr;
        var type = this.GetType();
        foreach (var r   in type.GetFields().ToList().FindAll(a=>a.FieldType.IsAssignableTo(typeof(LexRule))).Select(a=>a.GetValue(this)))
        {
            if (r is not LexRule rul)
            {
                continue;
            }
            if (rul.MyLabel==label&&rul.GetType().IsAssignableTo(typeof(T)))
            {
                return (T)rul;
            }
            var f = rul.GetByLabel<T>(label);
            if (f != null) return f;
        }
        foreach (var r   in type.GetProperties().ToList().FindAll(a=>a.PropertyType.IsAssignableTo(typeof(LexRule))).Select(a=>a.GetValue(this)))
        {
            if (r is not LexRule rul)
            {
                continue;
            }
            if (rul.MyLabel==label&&rul.GetType().IsAssignableTo(typeof(T)))
            {
                return (T)rul;
            }
            var f = rul.GetByLabel<T>(label);
            if (f != null) return f;
        }
        return null;
    }
    public static LexRule AnythingUntill(string till)
    {
        return CharRangeRule.Wildcard.List().Nongreedy(till.Rule());
    }

    public LexRule Optional()
    {
        RuleCombo c = new RuleCombo();
        c.Elements.Add(new(RuleCombo.ComboType.Start,this));
        c.Elements.Add(new(RuleCombo.ComboType.Or,True));
        return c;
    }
    public LexRule NotOptional()
    {
        RuleCombo c = new RuleCombo();
        c.Elements.Add(new(RuleCombo.ComboType.Start,this));
        return c;
    }
    public static AlwaysTrueRule True = new AlwaysTrueRule();

    public sealed class WildcardRule : LexRule
    {
        public override string ToString()
        {
            return $"\\*";
        }

        public WildcardRule() : base(null)
        {
            ComplexRule=new CharRangeRule(char.MinValue, char.MaxValue);
        }
    }

    public static WildcardRule Wildcard = new ();
    public static CharRangeRule Digits = new CharRangeRule('0'..'9');
    public static CharRangeRule Lowercase = new CharRangeRule('a'..'z');
    public static CharRangeRule Uppercase = 'A'..'Z';
    public static LexRule Letters = Uppercase | Lowercase;
    public static LexRule LettersOrDigits = Letters | Digits;

    public sealed class WhiteSpaceRule : LexRule
    {
        public override string ToString()
        {
            return $"\\t";
        }

        public WhiteSpaceRule() : base(null)
        {
            ComplexRule= "\t".Rule() | "\v".Rule()|"\f".Rule()
                         |"\u0020".Rule()|"\u00a0".Rule()|"\u1680".Rule()|"\u2000".Rule()|"\u2001".Rule()
                         |"\u2002".Rule()|"\u2003".Rule()|"\u2004".Rule()|"\u2005".Rule()|"\u2006".Rule()
                         |"\u2007".Rule()|"\u2008".Rule()|"\u2009".Rule()|"\u200A".Rule()|"\u202F".Rule()
                         |"\u205f".Rule()|"\u3000".Rule();
        }
    }

    public static WhiteSpaceRule Whitespace = new WhiteSpaceRule();
    public static LexRule WhitespaceOrNot = Whitespace | new StringLiteral();
    public static LexRule VariableWhitespace = Whitespace.List(true);
    public static LexRule OptionalVariableWhitespace = Whitespace.List();
    public static LexRule Newline = "\r".Rule()|"\n".Rule()|"\u0085".Rule()|"\u2028".Rule()|"\u2029".Rule();
    public string MyLabel { get; private set; }
    public Action<LexRule> MyAction { get; private set; }

  
  
    public LexRule Label(string label)
    {
        MyLabel = label;
        return this;
    }
 
    public LexRule Action(Action<LexRule> a)
    {
        MyAction = a;
        return this;
    }
   
    public ListRule List(bool oneOrMore=false)
    {
        var l = new ListRule(this,!oneOrMore);
        return l;
    }

    public sealed class SeparatedListRule : LexRule
    {
        public override string ToString()
        {
            return $"{FirstElement}[{Separator}]";
        }

        public LexRule FirstElement;
        public string Separator;
        public readonly List<ListRule.ElementPart> Elements = new();
        public LexRule ElementRule;
        public SeparatedListRule(LexRule r,string sep) : base(null)
        {
            FirstElement = r;
            Separator = sep;
            LexRule t = null;
            if (string.IsNullOrWhiteSpace(sep)) t = sep.Rule();
            else  t = OptionalVariableWhitespace & sep & OptionalVariableWhitespace;
            ElementRule = (t & r);
            ComplexRule =null;
        }

        protected override bool ReadThis(StringLexer reader, out string read)
        {
            var e = ElementRule;
            StringBuilder b = new();
            if (!FirstElement.Read(reader, out var s))
            {
                read = "";
                return false;
            }
            b.Append(s);
            string sOut;
            while (e.Read(reader,out sOut))
            {
                object val = null;
                if (e is IHasValue ve) val = ve.Value;
                Elements.Add(new(e,sOut,val));
                b.Append(sOut);
                e=ElementRule;
            }
            read= b.ToString();
            return true;
        }
 
    }
    public SeparatedListRule ListWithSeparator(string sep)
    {
        var l = new SeparatedListRule(this, sep);
        return l;
    }

    public RuleCombo Or(LexRule r)
    {
        var l = this;
        RuleCombo c = null;
        if (l is RuleCombo lc) c = lc;
        else
        {
            c = new();
            c.Elements.Add(new(RuleCombo.ComboType.Start,l));
        }
        c.Elements.Add(new(RuleCombo.ComboType.Or,r));
        return c;
    }
    public RuleCombo And( LexRule r)
    {
        var l = this;
        RuleCombo c = null;
        if (l is RuleCombo lc) c = lc;
        else
        {
            c = new();
            c.Elements.Add(new(RuleCombo.ComboType.Start,l));
        }
        c.Elements.Add(new(RuleCombo.ComboType.And,r));
        return c;
    }
 
    public static RuleCombo operator |(string l,LexRule r)
    {
        return l.Rule().Or(r);
    }
    public static RuleCombo operator |(Range l, LexRule r)
    {
        return l.Rule().Or(r);
    }
    public static RuleCombo operator &(string l, LexRule r)
    {
        return l.Rule().And(r);
    }
    public static RuleCombo operator &(Range l, LexRule r)
    {
        return l.Rule().And(r);
    }
    public static RuleCombo operator |(LexRule l, LexRule r)
    {
        return l.Or(r);
    }
    public static RuleCombo operator |(LexRule l, string r)
    {
        return l.Or(r.Rule());
    }
    public static RuleCombo operator |(LexRule l, Range r)
    {
        return l.Or(r.Rule());
    }
    
    //TODO: Change "and" and "or" to use lists instead of being abstraction based for readability
    public static RuleCombo operator &(LexRule l, LexRule r)
    {
        return l.And(r);
    }
    public static RuleCombo operator &(LexRule l, string r)
    {
        return l.And(r.Rule());
    }
    public static RuleCombo operator &(LexRule l, Range r)
    {
        return l.And(r.Rule());
    }
    public LexRule ComplexRule;

    protected virtual bool ReadThis(StringLexer reader, out string read)
    {
        read = "";
        return false;
    }

    public string MyString { get; private set; }
    
    public bool Read(StringLexer reader, out string read)
    {
        BeforeParse();
        if (ComplexRule == null)
        {
            var b=ReadThis(reader, out var s);
            if (!b)
            {
                read = "";
                return false;
            }
            read = s;
            MyString = s;
        }
        else
        {
            var b=ComplexRule.Read(reader, out var s);
            if (!b)
            {
                read = "";
                return false;
            }
            read = s;
            MyString = s;
        }

        AfterParse();
        if (MyAction != null) MyAction(this);
        return true;
    }
    public virtual void BeforeParse()
    {
        
    }
    public virtual void AfterParse()
    {
        
    }
    public LexRule(LexRule rules)
    {
        ComplexRule = rules;
    }
    public LexRule()
    {
    }


  
}