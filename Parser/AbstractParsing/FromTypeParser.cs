using System.Diagnostics;
using System.Reflection;
using Parser.Attributes;
using Parser.Rules;

namespace Parser;

public class FromTypeParser<X> : FromTypeParser
{
    public FromTypeParser(AbstractFileParser parent) : base(typeof(X),parent)
    {
    }

    public X ParsedValueX => (X)ParsedValue;

}


public class FromTypeParser : AbstractSubParser
{
    public string PeekWordSpecial()
    {
       return Parent.FileReader.Peek(new ExRule(a=>
           char.IsLetterOrDigit(a) || a=='_' || a=='@'
           ));
    }
        public string PeekWord()
        {
            return Parent.FileReader.Peek(new ExRule(a=>
                !char.IsWhiteSpace(a)
            ));
        }
    public override void Parse()
    {
        var word = PeekWordSpecial();
        while(string.IsNullOrWhiteSpace(word))word = PeekWord();
        
        
        if (hasType.Type.HasAttribute<GenExRuleAttribute>() || hasType.ActualType.HasAttribute<GenExRuleAttribute>())
        {
            this.ParsedValue = Activator.CreateInstance(this.hasType.ActualType);
            
            var fieldList = hasType.Type.GetFields().Select(a => new ExtraOfField() { Field = a }).ToArray();
            foreach (var curField in fieldList)
            {
                var fieldPosition = Parent.FileReader.Position;
            
                var fieldParser = curField.MakeAParser(Parent);
                fieldParser.Parse();
            
                var fieldValue = fieldParser.ParsedValue;
            
                var fieldEndPosition = Parent.FileReader.Position;
            
            
                curField.Field.SetValue(this.ParsedValue, Activator.CreateInstance(curField.Field.FieldType, fieldValue,fieldPosition,fieldEndPosition));
            
                //curField.Field.SetValue();
                Console.WriteLine(curField);

            }
        }
        else if (hasType.IsEnum)
        {
            if (hasType.TryGetEnum(word,out var v))
            {
                //good!
                this.ParsedValue = v;
                Parent.FileReader.Advance(word.Length+1);
                return;
            }
            else
            {
                throw new Exception($"Unknown element {word} at {Parent.FileReader.Position}!");
            }
        }
        else if (hasType.ActualType == typeof(string))
        {
            ParsedValue = word;
            Parent.FileReader.Advance(word.Length+1);
        }



        if (ParsedValue == null) throw new NotImplementedException();   

    }


    public Object ParsedValue;

    public AbstractFileParser Parent;
    public FromTypeParser(Type t,AbstractFileParser parent)
    {
      
        //TODO: control
        hasType = new ExtraOfField(){Type = t};
        Parent = parent;
    }

    private ExtraOfField hasType;

    private struct ExtraOfField
    {
        public FromTypeParser MakeAParser(AbstractFileParser parent) => new FromTypeParser(ActualType, parent);
        public Type ActualType =>Field==null?  (Type.GetGenericArguments().Length>0 ? Type.GetGenericArguments()[0] : Type)  :Field.FieldType.GetGenericArguments()[0];
        public bool IsEnum => ActualType.IsEnum;
        public List<string> EnumNames => Enum.GetNames(ActualType).ToList();
        public List<object> EnumValues => Enum.GetValues(ActualType).Cast<object>().ToList();
        public List<(string name, object value)> FullEnumValues
        {
            get
            {
                var field = this; return field.EnumNames.Select( (a, i) => (a, field.EnumValues[i])).ToList();
            }
        }

        public bool TryGetEnum(string name,out object value)
        {
            value = null;
            var index = EnumNames.IndexOf(name);
            if (index == -1) return false;
            value = EnumValues[index];
            return true;
        }

        public Type Type;
        public FieldInfo Field;
        public bool EndsWithWhiteSpace => Field.HasAttribute<EndsWithWhiteSpaceAttribute>();
        public bool EndsWithSemiColon => Field.HasAttribute<EndsWithSemiColonAttribute>();
        public bool RoundScope => Field.HasAttribute<RoundScopeAttribute>();
        public bool SquareScope => Field.HasAttribute<SquareScopeAttribute>();
        public bool CurveScope => Field.HasAttribute<CurveScopeAttribute>();
        public bool BirdScope => Field.HasAttribute<BirdScopeAttribute>();
        

     
    }
}