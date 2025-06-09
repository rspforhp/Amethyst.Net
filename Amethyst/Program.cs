using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using static Amethyst.IDefineStructure;
using System.Reflection;
using System.Reflection.Emit;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using AsmResolver.DotNet.Bundles;

namespace Amethyst;
public static class Extensions
{
    public static bool TryGetEnum<T>(this string text,out T en) where T : struct, Enum
    {
        return Enum.TryParse<T>(text, out en);
    }

    public static readonly List<char> AllowedCharsInIdentifiersBesidesLetterAndDigits = new()
    {
        '@','_'
    };

    public static bool IsValidIdentifier(this string text)
    {
        if (string.IsNullOrEmpty(text) || string.IsNullOrWhiteSpace(text)) return false;
        if (char.IsDigit(text[0])) return false;
        var res = !text.Any(c =>
            !char.IsLetterOrDigit(c) && !AllowedCharsInIdentifiersBesidesLetterAndDigits.Contains(c));
        
        return res;
    }

  
}
public static class Program
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Compiling shard!");
        FileReader.OpenFile(@"../../../hello_world.shard");
        var sh = new ShardStructure();
        var f = sh.ParseElement(FileReader);
        if (!f) throw new Exception("???");
        string asmName = "AssemblyShard";
        var cor = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "System.Private.CoreLib.dll");
        PersistedAssemblyBuilder asm = new PersistedAssemblyBuilder(new AssemblyName(asmName), typeof(object).Assembly);
        var mainModule = asm.DefineDynamicModule(asmName);

        foreach (var classStructure in sh.ElementList)
        {
            var typeBuilder = mainModule.DefineType(classStructure.MyIdentifier.Identifier, TypeAttributes.Class | TypeAttributes.Public);
            foreach (var member in classStructure.Members.ElementList)
            {
                switch (member.Element)
                {
                    case FuncStructure func:
                       var m=typeBuilder.DefineMethod(func.MyIdentifier.Identifier,
                            MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard,
                            Type.GetType(func.MyTypeReference.FullIdentifier),
                            func.Parameters.ElementList.Select(a => a.MyTypeReference.FullIdentifier)
                                .Select(Type.GetType).ToArray());
                       var il = m.GetILGenerator();
                       il.BeginScope();
                       
                       il.Emit(OpCodes.Ret);
                       il.EndScope();
                       break;
                    case FieldStructure field:
                        break;
                }
            }

            typeBuilder.CreateType();
        }
        
        var program = mainModule.DefineType("Program", TypeAttributes.Class | TypeAttributes.Public);
        var main=program.DefineMethod("Main", MethodAttributes.Static | MethodAttributes.Public | MethodAttributes.HideBySig);
        ILGenerator il2 = main.GetILGenerator();
        il2.BeginScope();
        il2.EmitWriteLine("THIS ISN'T REAL!");
        il2.Emit(OpCodes.Ret);
        il2.EndScope();
        program.CreateType();
        //asm.Save(asmName+".dll");
        MetadataBuilder metadataBuilder = asm.GenerateMetadata(out BlobBuilder ilStream, out BlobBuilder fieldData);
        
        ManagedPEBuilder peBuilder = new(
            header: PEHeaderBuilder.CreateExecutableHeader(),
            metadataRootBuilder: new MetadataRootBuilder(metadataBuilder),
            ilStream: ilStream,
            mappedFieldData: fieldData,
            entryPoint: MetadataTokens.MethodDefinitionHandle(main.MetadataToken));

        BlobBuilder peBlob = new();
        peBuilder.Serialize(peBlob);
        // Create the executable:
        using (FileStream fileStream = new(asmName + ".dll", FileMode.Create, FileAccess.Write))
        {        peBlob.WriteContentTo(fileStream);

        }


        BundleManifest manifest = new BundleManifest(6);
        manifest.Files.Add(new BundleFile($"{asmName}.dll", BundleFileType.Assembly, contents:System.IO.File.ReadAllBytes($"{asmName}.dll")));
        manifest.WriteUsingTemplate(
            $"{asmName}.exe",
            BundlerParameters.FromTemplate(
                appHostTemplatePath: @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\10.0.0-preview.4.25258.110\runtimes\win-x64\native\apphost.exe",
                appBinaryPath: $"{asmName}.dll"
                //imagePathToCopyHeadersFrom: @"C:\Path\To\Original\HelloWorld.exe"
                ));    
        
        File.WriteAllText($"{asmName}.runtimeconfig.json",
"""
{
  "runtimeOptions": {
    "tfm": "net10.0",
    "framework": {
      "name": "Microsoft.NETCore.App",
      "version": "10.0.0-preview.4.25258.110"
    },
    "configProperties": {
      "System.Runtime.Serialization.EnableUnsafeBinaryFormatterSerialization": false
    }
  }
}
""");
        
        File.WriteAllText($"{asmName}.deps.json",
            $$"""
            {
              "runtimeTarget": {
                "name": ".NETCoreApp,Version=v10.0",
                "signature": ""
              },
              "compilationOptions": {},
              "targets": {
                ".NETCoreApp,Version=v10.0": {
                  "{{asmName}}/1.0.0": {
                    "runtime": {
                      "{{asmName}}.dll": {}
                    }
                  }
                }
              },
              "libraries": {
                "{{asmName}}/1.0.0": {
                  "type": "project",
                  "serviceable": false,
                  "sha512": ""
                }
              }
            }
            """);
        //asm.Save(asmName+".exe");

        //Debugger.Break();
        
    }

    public class ClassStructure : StructureElement, IHaveAccessibility,IHaveClassModifiers,IHaveIdentifier
    {
        public override string ToString()
        {
            return $"{MyAccessibility} {MyClassModifiers} class {MyIdentifier} {{{Members}}}";
        }

        public StructureElementArray<StructureElementControl<FuncStructure, FieldStructure>> Members = new();

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            reader.SkipAllWhiteSpace();
            if (!MyAccessibility.ParseElement(reader)) MyAccessibility.Value = Accessibility.@private;
            reader.SkipAllWhiteSpace();
            MyClassModifiers.ParseElement(reader);
            reader.SkipAllWhiteSpace();
            if (!reader.PeekString("class", true)) return false;
            reader.SkipAllWhiteSpace();
            if (!MyIdentifier.ParseElement(reader)) return false;
           
            
            reader.SkipAllWhiteSpace();
            if (reader.Peek() == '{')
            {
                reader.Advance();
                reader.SkipAllWhiteSpace();
                Members.ParseElement(reader);
                reader.SkipAllWhiteSpace();
                if (reader.Peek()=='}')
                {
                    reader.Advance();reader.SkipAllWhiteSpace();
                    return true;
                }
            }

            return false;


    }

        public EnumStructureElement<Accessibility> MyAccessibility { get; } = new();
        public StructureElementArray<EnumStructureElement<ClassModifiers>> MyClassModifiers { get; }= new();
        public SingleIdentifierStructure MyIdentifier { get; }= new();
    }
    public class ShardStructure : StructureElementArray<ClassStructure>
    {
       
    }

    public enum Accessibility
    {
        @private,
        @public,
        @internal,
        @protected,
    }

    public interface IHaveAccessibility
    {
        public EnumStructureElement<Accessibility> MyAccessibility { get; }
    }

    public enum MemberModifiers
    {
        _ = 0,
        @static,
        @override,
        @virtual,
        @new,
        @abstract,
    }

    public interface IHaveMemberModifiers
    {
        public StructureElementArray<EnumStructureElement<MemberModifiers>> MyMemberModifiers { get; }
    }

    public enum ClassModifiers
    {
        _ = 0,
        @static,
        @abstract,
    }

    public interface IHaveClassModifiers
    {
        public StructureElementArray<EnumStructureElement<ClassModifiers>> MyClassModifiers { get; }
    }
    public enum TypeKeywords
    {
        _ = 0,
        @void,
        @string,
        @float,
        @double,
        @byte,
        @short,
        @int,
        @long,
        @bool,
        @sbyte,
        @char,
        @decimal,
        @uint,
        @nint,
        @nuint,
        @ulong,
        @ushort,
        @object,
        //@delegate,
        //@dynamic,
    }

    public interface IHaveTypeReference
    {
        public TypeReferenceStructure MyTypeReference { get; }
    }

    public interface IHaveIdentifier
    {
        public SingleIdentifierStructure MyIdentifier { get; }
    }

    public class IdentifierStructure : StructureElementList<SingleIdentifierStructure>
    {
        public override char Separator => '.';

        public string FullIdentifier
        {
            get => string.Join('.', this.ElementList.Select(a=>a.Identifier));
            set
            {
                ElementList.Clear();
                var split = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var s in split)
                {
                    ElementList.Add(new SingleIdentifierStructure(){Identifier = s,Position =this.Position });
                }
            }
        }
    }

    public class SingleIdentifierStructure : StructureElement
    {
        public override string ToString()
        {
            return Identifier;
        }

        public string Identifier;

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            var w = reader.PeekWord;
            {
                if (w.IsValidIdentifier())
                {
                    this.Identifier = w;
                    reader.Advance(w);
                    return true;
                }
                //TODO: allow weird identifiers, maybe make smth for that idk?
                return false;
            }
        }
    }

    public class TypeReferenceStructure : IdentifierStructure
    {
       
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            var b = base.DoParseElement(reader);
            if (FullIdentifier.TryGetEnum<TypeKeywords>(out var tk))
            {
                switch (tk)
                {
                    case TypeKeywords.@string:
                        FullIdentifier = "System.String";
                        break;
                    case TypeKeywords.@float:
                        FullIdentifier = "System.Single";
                        break;
                    case TypeKeywords.@double:
                        FullIdentifier = "System.Double";
                        break;
                    case TypeKeywords.@byte:
                        FullIdentifier = "System.Byte";
                        break;
                    case TypeKeywords.@short:
                        FullIdentifier = "System.Int16";
                        break;
                    case TypeKeywords.@int:
                        FullIdentifier = "System.Int32";
                        break;
                    case TypeKeywords.@void:
                        FullIdentifier = "System.Void";
                        break;
                    case TypeKeywords.@long:
                        FullIdentifier = "System.Int64";
                        break;
                    case TypeKeywords.@bool:
                        FullIdentifier = "System.Boolean";
                        break;
                    case TypeKeywords.@sbyte:
                        FullIdentifier = "System.SByte";
                        break;
                    case TypeKeywords.@char:
                        FullIdentifier = "System.Char";
                        break;
                    case TypeKeywords.@decimal:
                        FullIdentifier = "System.Decimal";
                        break;
                    case TypeKeywords.@uint:
                        FullIdentifier = "System.UInt32";
                        break;
                    case TypeKeywords.nint:
                        FullIdentifier = "System.IntPtr";
                        break;
                    case TypeKeywords.nuint:
                        FullIdentifier = "System.UIntPtr";
                        break;
                    case TypeKeywords.@ulong:
                        FullIdentifier = "System.UInt64";
                        break;
                    case TypeKeywords.@ushort:
                        FullIdentifier = "System.UInt16";
                        break;
                    case TypeKeywords.@object:
                        FullIdentifier = "System.Object";
                        break;
                    default:
                    case TypeKeywords._:
                        throw new ArgumentOutOfRangeException();
                }
            }
            return b;
        }
    }

    public class FuncParameterStructure : StructureElement, IHaveIdentifier, IHaveTypeReference
    {
        public override string ToString()
        {
            return $"{MyTypeReference} {MyIdentifier}";
        }

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (!MyTypeReference.ParseElement(reader)) return false;
            reader.SkipAllWhiteSpace();
            if (!MyIdentifier.ParseElement(reader)) return false;
            return true;
        }

        public SingleIdentifierStructure MyIdentifier { get; } = new();
        public TypeReferenceStructure MyTypeReference { get; } = new();
    }
    public abstract class MemberStructure : StructureElement, IHaveAccessibility, IHaveMemberModifiers,
        IHaveTypeReference,IHaveIdentifier
    {
        public EnumStructureElement<Accessibility> MyAccessibility { get; } = new();
        public StructureElementArray<EnumStructureElement<MemberModifiers>> MyMemberModifiers { get; } = new();
        public TypeReferenceStructure MyTypeReference { get; } = new();
        public SingleIdentifierStructure MyIdentifier { get; } = new();

        public override string ToString()
        {
            return $"{MyAccessibility} {MyMemberModifiers} {MyTypeReference} {MyIdentifier}";
        }

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (!MyAccessibility.ParseElement(reader)) MyAccessibility.Value = Accessibility.@private;
            reader.SkipAllWhiteSpace();
            MyMemberModifiers.ParseElement(reader);
            reader.SkipAllWhiteSpace();
            if (!MyTypeReference.ParseElement(reader)) return false;
            reader.SkipAllWhiteSpace();
            if (!MyIdentifier.ParseElement(reader)) return false;
            return true;
        }   

    }

    public abstract class ExpressionStructure : StructureElement
    {
        //? idk what i could put here besides just making it empty yet
    }

    public abstract class AbstractLiteralStructure : StructureElement
    {
    }

    public class NumberLiteralStructure : AbstractLiteralStructure
    {
        public override string ToString()
        {
            if (IsInteger) return BiggestInteger.ToString();
            if (IsFloat) return BiggestFloat.ToString();
            return "NAN";
        }

        public System.Int128 BiggestInteger;
        public bool IsInteger;
        public System.Decimal BiggestFloat;
        public bool IsFloat;
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            var word = reader.PeekWord;
            if (Int128.TryParse(word, out BiggestInteger))
            {
                reader.Advance(word);
                IsInteger = true;
                return true;
            }
            if (Decimal.TryParse(word, out BiggestFloat))
            {
                reader.Advance(word);
                IsFloat = true;
                return true;
            }
            return false;
        }
    }

    public class StringLiteralStructure : AbstractLiteralStructure
    {
        public override string ToString()
        {
            return $"\"{StringInterpretation}\"";
        }

        public System.String StringInterpretation="";
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (reader.Peek() == '\"')
            {
                reader.Advance();
                again:
                while (!reader.PeekString("\\\"", false))
                {
                    if (reader.Peek() == '\"')
                    {
                        reader.Advance();
                       //Debugger.Break();
                        return true;
                    }
                    StringInterpretation += reader.Read();
                }
                if(reader.PeekString("\\\"", true))
                  StringInterpretation += "\\\"";
                goto again;
            }
            else return false;
        }
    }
    public class LiteralStructure : StructureElementControl<NumberLiteralStructure,StringLiteralStructure>
    {
        
    }
    public class InvokationParameterStructure : StructureElementControl<IdentifierStructure, LiteralStructure>
    {
      
    }
    public class FunctionInvokationExpressionStructure : ExpressionStructure
    {
        public override string ToString()
        {
            return $"{MethodIdentifier}({Parameters})";
        }

        public StructureElementList<InvokationParameterStructure> Parameters = new();
        
        public IdentifierStructure MethodIdentifier=new();
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            reader.SkipAllWhiteSpace();
            if (!MethodIdentifier.ParseElement(reader)) return false;
            reader.SkipAllWhiteSpace();
            if (reader.Peek() != '(') return false;
            reader.Advance();

            reader.SkipAllWhiteSpace();

            if (reader.Peek() != ')')
            {
                //handle parameters
                Parameters.ParseElement(reader);
                if (reader.Peek() != ')')
                {
                    ///???
                    Debugger.Break();
                    return false;
                }else reader.Advance();
            }
            else reader.Advance();
            
            reader.SkipAllWhiteSpace();

            /*
            if (reader.Peek() == ';')
            {
                //Done with the method
                reader.Advance();
                return true;
            }
            return false;*/
            return true;
        }
    }
    public class FuncBodyStructure : StructureElementList<StructureElementControl<FunctionInvokationExpressionStructure,FunctionInvokationExpressionStructure>>
    {
        public override char Separator => ';';
    }

    public class FuncStructure : MemberStructure
    {
        public override string ToString()
        {
            return $"{base.ToString()}({Parameters}){{{FuncBody.ToString()}}}";
        }

        public StructureElementList<FuncParameterStructure> Parameters = new();
        public FuncBodyStructure FuncBody = new();
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            base.DoParseElement(reader);
            //reader.SkipAllWhiteSpace();
            if (reader.Peek() != '(') return false;
            reader.Advance();

            reader.SkipAllWhiteSpace();

            if (reader.Peek() != ')')
            {
                //handle parameters
                Parameters.ParseElement(reader);
                if (reader.Peek() != ')')
                {
                    ///???
                    Debugger.Break();
                    return false;
                }else reader.Advance();
            }
            else reader.Advance();
            
            reader.SkipAllWhiteSpace();

            if (reader.Peek() == ';')
            {
                //Done with the method, maybe has no body somehow
                reader.Advance();
                return true;
            }
            else if(reader.Peek()=='{')
            {
                reader.Advance();
                //Do method body
                FuncBody.ParseElement(reader);
                reader.SkipAllWhiteSpace();
                if (reader.Peek() == '}')
                {
                    reader.Advance();
                    reader.SkipAllWhiteSpace();
                    return true;
                }
                else //????what
                Debugger.Break();
                return false;
            }
            else
            {
                Debugger.Break();
                return false;
            }
            

            
            return false;
        }
    }

    public class FieldStructure : MemberStructure
    {
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            base.DoParseElement(reader);
            reader.SkipAllWhiteSpace();
            if (reader.Peek() == ';')
            {
                reader.Advance();
                reader.SkipAllWhiteSpace();
                return true;
            }
            return false;
        }

        public EnumStructureElement<Accessibility> MyAccessibility { get; }
    }


    public static SimpleFileReader FileReader = new();
}

public class SimpleFileReader
{
    public string FileContents = string.Empty;
    private uint _pos;
    public uint Position
    {
        get
        {
            return _pos;
        }
        set
        {
            //Console.WriteLine(System.Environment.StackTrace);
            _pos = value;
        }
    }
    public char PeekChar => Peek();
    //[] for type's and <> for generics, idk for other stuff lol
    public readonly List<char> WordEndingChars = new() { ';','(',')','{','}',',','+','/',':','-','=','*','.'};
    public string PeekWord => Position>=this.FileContents.Length? string.Empty :  PeekUntil(c => char.IsWhiteSpace(c) || WordEndingChars.Contains(c) );

    public override string ToString()
    {
        var word = PeekWord;
        return PeekWord;
    }



    public char Peek()
    {
        if (Position >= FileContents.Length) return '\0';
        return FileContents[(int)Position];
    }

    public void Advance(string str)
    {
        Advance((uint)str.Length);
    }

    public void Advance(uint am = 1)
    {
        var posFirst = Position;
        Position += am;
        if (am==6 && posFirst==0 && Position==5)
            throw new Exception("Math issue");

    }

    public void Retreat(uint am = 1)
    {
        Position -= am;
    }

    public char Read()
    {
        char c = Peek();
        Advance();
        return c;
    }

    public bool PeekString(string checkFor, bool advanceOnTrue)
    {
        var b = Peek((uint)checkFor.Length) == checkFor;
        if (b && advanceOnTrue) Advance(checkFor);
        return b;
    }

    public void SkipAllWhiteSpace()
    {
        while (char.IsWhiteSpace(Peek()))
        {
            Advance();
        }
    }

    public string Peek(uint length)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < length; i++)
        {
            builder.Append(Read());
        }

        Retreat(length);
        return builder.ToString();
    }

    public string Read(uint length)
    {
        var s = Peek(length);
        Advance(length);
        return s;
    }

    public string Peek(Func<char, bool> rule)
    {
        StringBuilder builder = new StringBuilder();
        var c = Peek();
        while (rule(c))
        {
            Advance();
            builder.Append(c);
            c = Peek();
        }

        Retreat((uint)builder.Length );
        return builder.ToString();
    }

    public string Read(Func<char, bool> rule)
    {
        var peek = Peek(rule);
        Advance((uint)peek.Length);
        return peek;
    }
    
    
    public string PeekUntil(Func<char, bool> rule)
    {
        StringBuilder builder = new StringBuilder();
        var c = Peek();
        while (!rule(c))
        {
            Advance();
            builder.Append(c);
            c = Peek();
        }

        Retreat((uint)builder.Length);
        return builder.ToString();
    }

    public string ReadUntil(Func<char, bool> rule)
    {
        var peek = PeekUntil(rule);
        Advance((uint)peek.Length);
        return peek;
    }


    public void OpenFile(string path)
    {
        FileContents = File.ReadAllText(path);
        Position = 0;
    }

    public void CloseFile()
    {
        FileContents = string.Empty;
        Position = unchecked((uint)-1);
    }
}

public static class IDefineStructure
{


    public class EnumStructureElement<T> : StructureElement where T : struct, Enum
    {
        public override string ToString()
        {
            return Value.ToString();
        }

        public T? Value;

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            T v;
            var w = reader.PeekWord;
            if (w.TryGetEnum(out v))
            {
                Value = v;
                reader.Advance(w);
                return true;
            }

            return false;
        }
    }


    public abstract class StructureElement
    {
        public uint Position { get; set; }
        public uint Size { get; protected set; }

        public bool ParseElement(SimpleFileReader reader)
        {
            this.Position = reader.Position;
            var b=DoParseElement(reader);
            if (b)
            {
                Size = reader.Position - this.Position;
                return true;
            }

            return false;
        }

        /// <summary>
        /// Tries to parse an element, and sets its position in the file
        /// </summary>
        /// <returns>whether parsing failed at any point</returns>
        protected abstract bool DoParseElement(SimpleFileReader reader);
    }

    //TODO: make more if i need to lol
    public class StructureElementControl<T1, T2, T3, T4, T5> : StructureElementControl<T1, T2, T3, T4>
        where T1 : StructureElement, new()
        where T2 : StructureElement, new()
        where T3 : StructureElement, new()
        where T4 : StructureElement, new()
        where T5 : StructureElement, new()
    {
        private T5 _T5 = new();

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (base.DoParseElement(reader)) return true;
            return HandleElement(ref _T5, reader);
        }
    }

    public class StructureElementControl<T1, T2, T3, T4> : StructureElementControl<T1, T2, T3>
        where T1 : StructureElement, new()
        where T2 : StructureElement, new()
        where T3 : StructureElement, new()
        where T4 : StructureElement, new()
    {
        private T4 _T4 = new();

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (base.DoParseElement(reader)) return true;
            return HandleElement(ref _T4, reader);
        }
    }

    public class StructureElementControl<T1, T2, T3> : StructureElementControl<T1, T2>
        where T1 : StructureElement, new() where T2 : StructureElement, new() where T3 : StructureElement, new()
    {
        private T3 _T3 = new();

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (base.DoParseElement(reader)) return true;
            return HandleElement(ref _T3, reader);
        }
    }

    public class StructureElementControl<T1, T2> : StructureElement where T1 : StructureElement, new()
        where T2 : StructureElement, new()
    {
        public override string ToString()
        {
            return Element.ToString();
        }

        private T1 _T1 = new();
        private T2 _T2 = new();
        public StructureElement Element;

        protected bool HandleElement<T>(ref T element, SimpleFileReader reader) where T : StructureElement
        {
            var before = reader.Position;
            if (element.ParseElement(reader))
            {
                Element = element;
                return true;
            }

            element = null;
            reader.Position = before;
            return false;
        }

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (HandleElement(ref _T1, reader)) return true;
            if (HandleElement(ref _T2, reader)) return true;
            return false;
        }
    }

    public class StructureElementList<T> : StructureElementArray<T> where T : StructureElement, new()
    {
        public override string ToString()
        {
            return $"[{string.Join(Separator, ElementList)}]";
        }

        public virtual char Separator => ',';
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            while (true)
            {
                var structureElement = new T();
                if (structureElement.ParseElement(reader)) ElementList.Add(structureElement);
                else break;
                
                reader.SkipAllWhiteSpace();
                if (reader.Peek() == Separator)
                {
                    reader.Advance();
                    reader.SkipAllWhiteSpace();
                    continue;
                }
                else break;
                
            }

            return ElementList.Count>0;
        }
    }

    public class StructureElementArray<T> : StructureElement where T : StructureElement, new()
    {
        public List<T> ElementList = new();

        public override string ToString()
        {
            return $"[{string.Join(", ", ElementList)}]";
        }

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            while (true)
            {
                var structureElement = new T();
                if (structureElement.ParseElement(reader)) ElementList.Add(structureElement);
                
                else break;
            }

            return ElementList.Count>0;
        }
    }
}
//File extension names to be used:
//amethyst, shard, geode
//owo ?
// amethyst - project file
// shard - code file
// geode - solution file