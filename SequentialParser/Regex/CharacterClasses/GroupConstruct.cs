using System.Text;

namespace SequentialParser.Regex.CharacterClasses;

public struct GroupConstruct : IClassCharacter<GroupConstruct>
{
    public readonly List<object> ToMatch;

    public readonly HandlerReference? Reference;
    public GroupConstruct(HandlerReference? reference, params List<object> toMatch)
    {
        Reference = reference;
        ToMatch = toMatch;
    }
   
    public static GroupConstruct? Parse(ref AdvancedStringReader sequence)
    {
       sequence.PushPosition();//+1 before ret
       if (sequence.ReadChar() != "(")
       {
           sequence.PopPosition();//-1 on ret
           return null;
       }

       List<object> classes = new();
       IClassCharacter c = null;
       do
       {
           sequence.PushPosition();//+1
           c = ClassCharacter.ParseWithQuantifier(ref sequence);
           if (c == null)
           {
               sequence.PopPosition();//-1
               break;
           }
           sequence.DontPopPosition();//-1
           classes.Add(c);
           
       } while (c!=null);
       
       if (sequence.ReadChar() == ")")
       {
           sequence.DontPopPosition();//-1 on ret
           HandlerReference? refe = HandlerReference.Parse(ref sequence);
           return new GroupConstruct(refe,classes);
       }
       sequence.PopPosition();//-1 on ret
       return null;
    }

    public bool Matches(Rune r)
    {
        throw new Exception("Do not call this please.");
    }

    public string ReadWithoutQuantifier(ref AdvancedStringReader r)
    {
        uint initialPos = r.CurrentPosition;
        r.PushPosition();
        StringBuilder b = new();
        foreach (object o in ToMatch)
        {
            IClassCharacter co =(IClassCharacter)o;
            r.PushPosition();
            var read=co.ReadWithQuantifier(ref r);
            if (read != null)
            {
                b.Append(read);
                r.DontPopPosition();
            }
            else
            {
                r.PopPosition();
                r.PopPosition();
                return null;
            }
        }
        

        if (Reference is { } v)
        {
            bool f=r.Handlers[v.Reference].Handle(ref r,b, initialPos,r.CurrentPosition);
            if (!f)
            {
                r.PopPosition();
                return null;
            }
        }
        
        r.DontPopPosition();
        return b.ToString();
    }

    public Quantifier? Quantifier { get; set; }
}