using System.Reflection;

namespace SequentialParser.AutoParser;

public class ArrayParser: ClassParser
{
    public static MethodInfo OfType = typeof(Enumerable).GetMethod("OfType");
    public static MethodInfo ToArray = typeof(Enumerable).GetMethod("ToArray");
    
    public readonly Type ElementType;
    public readonly MethodInfo OfTypeElement;
    public readonly MethodInfo ToArrayElement;

    public ArrayParser(Type arrayType)
    {
        this.ElementType = arrayType.GetElementType();
        OfTypeElement = OfType.MakeGenericMethod(ElementType);
        ToArrayElement = ToArray.MakeGenericMethod(ElementType);
    }

  
    public override object Parse(ref SimpleStringReader reader,FieldInfo optionalField)
    {
        List<object> list = new List<object>();
        object o =ParsableClasses.Parse(ref reader,ElementType,optionalField);
        while (o != null)
        {
            list.Add(o);
            o =ParsableClasses.Parse(ref reader,ElementType,optionalField);
        }

        //TODO: attribute for size requiredness
       var ofList= OfTypeElement.Invoke(null, [list]);
       var ofAr = ToArrayElement.Invoke(null, [ofList]);
         return ofAr;
    }
}