using System.Runtime.CompilerServices;
using PLGSGen;

namespace PLSGen;

public abstract class LexRule
{
    public abstract string Name {  get;}
    public string ReadValue{ get; protected set; }
    //System.Range for char ranges
    //char[] for char array
    //string for string
    // | for or
    // & for and
    //* for list of rule (0 allowed)
    //+ for list of rule (0 NOT allowed)
    //? for optional
    //anything else is a rule name
    public RuleList Elements;
    protected LexRule()
    {
    }

    public bool Read(ref SimpleStringReader reader)
    {
        bool result = false;
        //TODO:

        if (result) result = Validate();
        return result;
    }

    public bool Read(SimpleStringReader reader)
    {
        return Read(ref reader);
    }
    
    public virtual bool Validate() => true;
}

public class RuleList
{
    public RuleElement? Rule;
    public List<RuleList> List=new();

    public override string ToString()
    {
        if (Rule.HasValue) return $"{Rule.Value.ToString()}";
        return $"new RuleList({string.Join(",", List.Select(a=>a.ToString()))})";
    }

    public RuleList(params RuleElement[] elements)
    {
        this.AddRange(elements);
    }


    public void Add(RuleElement elem)
    {
        List.Add(new(){Rule=elem});
    }

    public void AddRange(RuleElement[] elems)
    {
        foreach (var ruleElement in elems)
        {
            Add(ruleElement);
        }
    }
    public void Add(RuleElement[] elem)
    {
        RuleList scope = new();
        scope.AddRange(elem);
        List.Add(scope);
    }
}
public struct RuleElement
{
    public override string ToString()
    {
        return $"new RuleElement({Rule},RuleElement.RuleRelationType.{Type})";
    }

    public enum RuleRelationType
    {
        _,
        Or,
        And,
    }
    public static string StrType(RuleElement.RuleRelationType t)
    {
        switch (t)
        {
            case RuleElement.RuleRelationType.Or:
                return "|";
            case RuleElement.RuleRelationType.And:
                return "&";
            default:
            case RuleElement.RuleRelationType._:
                return "";
        }
    }

    public readonly SimpleRule Rule;
    public readonly RuleRelationType Type;

    public RuleElement(SimpleRule rule, RuleRelationType type)
    {
        Rule = rule;
        Type = type;
    }
}

public struct SimpleRule
{
    public string ReadValue { get; internal set; }
    public readonly List<SimpleRule> ListValues;
    public enum RuleType
    {
        _,
        RuleName,
        Range,
        Array,
        String,
        RuleList,
    }

    public bool Optional { get; internal set; }
    public readonly RuleType Type;

    public readonly string RuleName;
    public readonly (char from,char toInc) CharRange;
    public readonly char[] CharArray;
    public readonly string String;
    public readonly StrongBox<SimpleRule> ListElement;

    public bool Lex(ref SimpleStringReader reader)
    {
        bool result = false;
        //Save state
        var p = reader.Position;
        LexRule lexRule = null;

        //TODO:
        
        

        //Return state back if we failed to read anything
        if (!result) reader.Position = p;
        return result || Optional;
    }

    public SimpleRule MakeOptional()
    {
        SimpleRule r = this;
        r.Optional = true;
        return r;
    }

    public override string ToString()
    {
        switch (this.Type)
        {
            case RuleType.RuleName:
                return $"new SimpleRule({RuleName},true)";
                break;
            case RuleType.Range:
                return $"new SimpleRule('{this.CharRange.from}'..'{this.CharRange.toInc}')";
                break;
            case RuleType.Array:
                return $"new SimpleRule([\"{string.Join(",", CharArray.Select(a=>$"'{a}'"))}\"])";
                break;
            case RuleType.String:
                return $"new SimpleRule(\"{this.String}\")";
                break;
            case RuleType.RuleList:
                return $"new SimpleRule({this.ListElement.Value})";
                break;
            default:
            case RuleType._:
                return null;
        }
        return null;
    }

    
    public SimpleRule(SimpleRule element) : this(RuleType.RuleList)
    {
        this.ListElement = new StrongBox<SimpleRule>(element);
        ListValues = new();
    }
    public SimpleRule(string str) : this(RuleType.String)
    {
        this.String = str;
    }
    public SimpleRule(char[] ar) : this(RuleType.Array)
    {
        this.CharArray = ar;
    }
    public SimpleRule(System.Range range) : this(RuleType.Range)
    {
        this.CharRange = ((char)range.Start.Value,(char)range.End.Value);
    }
    public SimpleRule(char from, char toInc) : this(RuleType.Range)
    {
        this.CharRange = (from,toInc);
    }
    public SimpleRule(string ruleName, bool nothing) : this(RuleType.RuleName)
    {
        this.RuleName = ruleName;
    }
    private SimpleRule(RuleType type)
    {
        this.Type = type;
    }

    [Obsolete("Do not use this",true)]
    public SimpleRule()
    {
    }
}