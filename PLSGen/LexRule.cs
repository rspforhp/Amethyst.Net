using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using PLGSGen;

namespace PLSGen;
[DebuggerDisplay("{DebugView()}")]
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
    public SimpleRule Elements;
    protected LexRule()
    {
    }

    public virtual string DebugView()
    {
        return Elements.DebugView();
    }

    public bool? SucceedOnRead;
    public bool Read(ref SimpleStringReader reader)
    {
        bool result = false;
        result = Elements.Read(ref  reader);
        ReadValue = Elements.ReadValue;
        if (result) result = Validate();
        SucceedOnRead = result;
        return result;
    }

    public bool Read(SimpleStringReader reader)
    {
        return Read(ref reader);
    }
    
    public virtual bool Validate() => true;
}
[DebuggerDisplay("{DebugView()}")]
public struct   RuleElement
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

    public SimpleRule Rule;
    public readonly RuleRelationType Type;

    public RuleElement(SimpleRule rule, RuleRelationType type)
    {
        Rule = rule;
        Type = type;
    }

    public readonly string DebugView()
    {
        return $"{Rule.DebugView()}{StrType(Type)}";
    }

    public bool Read(ref SimpleStringReader reader)
    {
        return this.Rule.Read(ref reader);
    }
}
[DebuggerDisplay("{DebugView()}")]
public struct SimpleRule
{
    public string ReadValue { get; internal set; }
    public enum RuleType
    {
        _,
        RuleName,
        Range,
        Array,
        String,
        RuleList,
        RuleScope,
    }

    public bool Optional;
    public bool Negate;
    public readonly RuleType Type;

    public readonly string RuleName;
    public  StrongBox<SimpleRule> RuleNameRule;
    public readonly (char from,char toInc) CharRange;
    public readonly char[] CharArray;
    public readonly string String;
    public readonly StrongBox<SimpleRule> ListElement;
    public List<SimpleRule> ListValues;

    public IReadOnlyList<StrongBox<RuleElement>> ScopeList;

    public SimpleRule NegateMe()
    {
        SimpleRule r = this;
        r.Negate = !r.Negate;
        return r;
    }

    public SimpleRule MakeOptional()
    {
        SimpleRule r = this;
        r.Optional = true;
        return r;
    }

    public override string ToString()
    {
        string result = "";
        switch (this.Type)
        {
            case RuleType.RuleName:
                result= $"new SimpleRule(nameof({RuleName}),true)";
                break;
            case RuleType.Range:
                result= $"new SimpleRule('{this.CharRange.from}','{this.CharRange.toInc}')";
                break;
            case RuleType.Array:
                result= $"new SimpleRule(\"{PLSGenerator.Escape(string.Join("", CharArray))}\".ToCharArray())";
                break;
            case RuleType.String:
                result= $"new SimpleRule(\"{PLSGenerator.Escape(this.String)}\")";
                break;
            case RuleType.RuleList:
                result= $"new SimpleRule({this.ListElement.Value})";
                break;
            case RuleType.RuleScope:
                result= $"new SimpleRule({string.Join(",", ScopeList.Select(a=>a.Value.ToString()))})";
                break;
            default:
            case RuleType._:
                return null;
        }

        if (Optional) result = $"{result}.MakeOptional()";
        if (Negate) result = $"{result}.NegateMe()";
        return result;
    }

    public SimpleRule(params List<RuleElement> elements) : this(RuleType.RuleScope)
    {
        this.ScopeList = elements.Select(a=>new StrongBox<RuleElement>(a)).ToList();
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

    public readonly string DebugView()
    {
        string result = "";
        switch (this.Type)
        {
            case RuleType.RuleName:
                result= $"{RuleName}";
                break;
            case RuleType.Range:
                result= $"'{this.CharRange.from}'..'{this.CharRange.toInc}'";
                break;
            case RuleType.Array:
                result= $"[\"{string.Join(",", CharArray.Select(a=>$"'{a}'"))}\"]";
                break;
            case RuleType.String:
                result= $"\"{this.String}\"";
                break;
            case RuleType.RuleList:
                result= $"{this.ListElement.Value.DebugView()}*";
                break;
            case RuleType.RuleScope:
                result= $"({string.Join("", ScopeList.Select(a=>$"{a.Value.DebugView()}"))})";
                break;
            default:
            case RuleType._:
                return null;
        }


        if (Optional) result += "?";
        if (Negate) result = $"~{result}";
        return result;
    }

    public bool Read(ref SimpleStringReader reader)
    {
        var storePos = reader.Position;
        //Lists are by-ref so i better copy this before reading
        if(ScopeList!=null)
            ScopeList = ScopeList.Select(a=>new StrongBox<RuleElement>(a.Value)).ToList();
        bool result = false;
        switch (Type)
        {
            case RuleType.RuleName:
            {
                if (RuleNameRule == null)
                {
                    Type t = null;
                    var tempRulename = RuleName;
                    foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                    {
                        var types = assembly.GetTypes();
                        t = types.ToList().Find(a => a.Namespace == "LexRules" && a.Name == tempRulename);
                        if (t != null) break;
                    }
                    if (t == null) throw new TypeLoadException();
                    var r = t
                        .GetField("StaticRule", BindingFlags.Public | BindingFlags.Static);
                    RuleNameRule = new StrongBox<SimpleRule>((SimpleRule)r.GetValue(null));
                }
                result = RuleNameRule.Value.Read(ref reader);
                this.ReadValue = RuleNameRule.Value.ReadValue;
            }
                break;
            case RuleType.Range:
            {
                var r = reader.Peek(1);
                if (r.Length == 0)
                {
                    result = false;
                    break;
                }
                char ch = r[0];
                if ((ch >= CharRange.from && ch <= CharRange.toInc)^Negate)
                {
                    reader.Position++;
                    ReadValue = ch.ToString();
                    result = true;
                }
            }
                break;
            case RuleType.Array:
            {
                var r = reader.Peek(1);
                if (r.Length == 0)
                {
                    result = false;
                    break;
                }
                char ch = r[0];
                if (CharArray.Contains(ch) ^ Negate)
                {
                    reader.Position++;
                    ReadValue = ch.ToString();
                    result = true;
                }
            }
                break;
            case RuleType.String:
            {
                result = reader.Exists(this.String, true);
                if (result) ReadValue = String;
            }
                break;
            case RuleType.RuleList:
            {
                ref var elementRule = ref this.ListElement.Value;
                SimpleRule rule = elementRule;
                ListValues = new();
                while (rule.Read(ref reader))
                {
                    this.ReadValue += rule.ReadValue;
                    this.ListValues.Add(rule);
                    rule = elementRule;
                }

                result = ListValues.Count>0;
            }
                break;
            case RuleType.RuleScope:
            {
                ref var firstRule = ref ScopeList[0].Value;
                result = firstRule.Read(ref reader);
                ReadValue += firstRule.Rule.ReadValue;
                RuleElement.RuleRelationType t = firstRule.Type;
                for (int i = 1; i < ScopeList.Count; i++)
                {
                    ref var curRule=ref  ScopeList[i].Value;
                    switch (t)
                    {
                        case RuleElement.RuleRelationType.Or:
                            if (!result)
                            {
                                result = curRule.Read(ref reader);
                                ReadValue += curRule.Rule.ReadValue;
                            }
                            break;
                        case RuleElement.RuleRelationType.And:
                            if (result)
                            {
                                result = curRule.Read(ref reader);
                                ReadValue += curRule.Rule.ReadValue;
                            }
                            break;
                        default:
                        case RuleElement.RuleRelationType._:
                            throw new ArgumentOutOfRangeException();
                    }
                    t =curRule.Type;
                }
            }
                break;
            default:
            case RuleType._:
                result = false;
                throw new ArgumentOutOfRangeException();
        }

        result|= Optional;
        if (!result)
        {
            reader.Position = storePos;
            this.ReadValue = "";
        }
        return result;
    }
}