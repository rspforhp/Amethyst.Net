using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
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
    public static bool TryGetEnum<T>(this string text, out T en) where T : struct, Enum
    {
        return Enum.TryParse<T>(text, out en);
    }

    public static readonly List<char> AllowedCharsInIdentifiersBesidesLetterAndDigits = new()
    {
        '@', '_'
    };

    public static bool CompileElement(this ICompileable toCompile, Compiler c)
    {
        return toCompile.HiddenCompileElement(c);
    }


    private static bool HandleCompileElement<T>(T element, Compiler c) where T : ICompileable
    {
        if (element.CompileElement(c))
            return true;
        return false;
    }

    public static bool CompileElement<T1, T2, T3, T4, T5>(this StructureElementControl<T1, T2, T3, T4, T5> control,
        Compiler c)
        where T1 : StructureElement, ICompileable, new()
        where T2 : StructureElement, ICompileable, new()
        where T3 : StructureElement, ICompileable, new()
        where T4 : StructureElement, ICompileable, new()
        where T5 : StructureElement, ICompileable, new()
    {
        if (control.Element is ICompileable comp)
        {
            return HandleCompileElement(comp, c);
        }

        return false;
    }

    public static bool CompileElement<T1, T2, T3, T4>(this StructureElementControl<T1, T2, T3, T4> control, Compiler c)
        where T1 : StructureElement, ICompileable, new()
        where T2 : StructureElement, ICompileable, new()
        where T3 : StructureElement, ICompileable, new()
        where T4 : StructureElement, ICompileable, new()
    {
        if (control.Element is ICompileable comp)
        {
            return HandleCompileElement(comp, c);
        }

        return false;
    }

    public static bool CompileElement<T1, T2, T3>(this StructureElementControl<T1, T2, T3> control, Compiler c)
        where T1 : StructureElement, ICompileable, new()
        where T2 : StructureElement, ICompileable, new()
        where T3 : StructureElement, ICompileable, new()
    {
        if (control.Element is ICompileable comp)
        {
            return HandleCompileElement(comp, c);
        }

        return false;
    }

    public static bool CompileElement<T1, T2>(this StructureElementControl<T1, T2> control, Compiler c)
        where T1 : StructureElement, ICompileable, new() where T2 : StructureElement, ICompileable, new()
    {
        if (control.Element is ICompileable comp)
        {
            return HandleCompileElement(comp, c);
        }

        return false;
    }

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
    private static bool SomeBool;

    private static void ILPreview()
    {
        if (SomeBool)
        {
            Console.WriteLine(1);
        }
        else
        {
            Console.WriteLine(2);
        }
    }

    public static void Main(string[] args)
    {
        Console.WriteLine("Compiling shard!");
        FileReader.OpenFile(@"../../../hello_world.shard");
        var sh = new ShardStructure();
        var f = sh.ParseElement(FileReader);
        if (!f) throw new Exception("???");
        //string asmName = "AssemblyShard";
        var ilc = new ILCompiler("AssemblyShard");
        ilc.ReferencedAssemblies.AddRange(AppDomain.CurrentDomain.GetAssemblies());

        sh.CompileElement(ilc);

        ilc.CompileToFile();


        //OLD
        /*
        MethodInfo entryPoint = null;
        foreach (var classStructure in sh.ElementList)
        {
            //TODO: abstract away the IL gen
            TypeAttributes typeAtr = TypeAttributes.Class;

            var typeBuilder = mainModule.DefineType(classStructure.MyIdentifier.Identifier, typeAtr);

            foreach (var member in classStructure.Members.ElementList)
            {
                switch (member.Element)
                {
                    case FuncStructure func:
                        var m = typeBuilder.DefineMethod(func.MyIdentifier.Identifier,
                            MethodAttributes.Static | MethodAttributes.Public, CallingConventions.Standard,
                            Type.GetType(func.MyTypeReference.FullIdentifier),
                            func.Parameters.ElementList.Select(a => a.MyTypeReference.FullIdentifier)
                                .Select(Type.GetType).ToArray());
                        var il = m.GetILGenerator();
                        il.BeginScope();

                        foreach (var expr in func.FuncBody.ElementList)
                        {
                            switch (expr.Element)
                            {
                                case FunctionInvokationExpressionStructure invok:

                                    Debugger.Break();
                                    break;
                            }
                        }


                        il.Emit(OpCodes.Ret);
                        il.EndScope();

                        if (func.MyModifiers.ElementList.Any(a => a.Value == MethodModifiers.entry))
                        {
                            if (entryPoint != null) throw new Exception("Multiple entry points are not supported!");
                            entryPoint = m;
                        }

                        break;
                    case FieldStructure field:
                        break;
                }
            }

            typeBuilder.CreateType();
        }*/
    }

    public class ILCompiler : Compiler
    {
        public MethodInfo EntryPoint;
        public string AsmName;

        public PersistedAssemblyBuilder AsmBuilder;
        public ModuleBuilder MainModule;
        public TypeBuilder TypeBuilder;
        public MethodBuilder MethodBuilder;
        public ILGenerator IL;

        public List<Assembly> ReferencedAssemblies = new();
        public List<TypeBuilder> AddedTypes = new();
        private Stack<Type> TypeStack = new();

        public void Push(Type t)
        {
            if(t!=typeof(void))
                TypeStack.Push(t);
        }

        public Type Pop()
        {
            if (TypeStack.Count == 0) return typeof(void);
           return TypeStack.Pop();
        }
        public Type[] Pop(int amount,bool reverse)
        {
            if (TypeStack.Count == 0) return [];
            Type[] types = new Type[amount];
            for (int i = 0; i < amount; i++)
            {
                types[i] = TypeStack.Pop();
            }
            if(reverse)
                types = types.Reverse().ToArray();
            return types;
        }

        public Type GetType(string name, bool throwIfFalse)
        {
            foreach (var typeBuilder in AddedTypes)
            {
                if (typeBuilder.FullName == name) return typeBuilder;
                if (typeBuilder.Name == name) return typeBuilder;
            }

            foreach (var asm in ReferencedAssemblies)
            {
                foreach (var typeBuilder in asm.GetTypes())
                {
                    if (typeBuilder.FullName == name) return typeBuilder;
                    if (typeBuilder.Name == name) return typeBuilder;
                }
            }

            if (throwIfFalse)
                throw new TypeLoadException();
            return null;
        }

        public Type GetType(String typename)
        {
            //var a = AccessTools.TypeByName(typename);
            //if (a != null) return a;
            var localType = this.GetType(typename, false);
            if (localType is not null)
                return localType;
            return null;
        }


        public ILCompiler(string asmName)
        {
            AsmName = asmName;
            AsmBuilder = new PersistedAssemblyBuilder(new AssemblyName(AsmName), typeof(object).Assembly);
            MainModule = AsmBuilder.DefineDynamicModule(AsmName);
        }

        public override bool CompileToFile()
        {
            MetadataBuilder metadataBuilder =
                AsmBuilder.GenerateMetadata(out BlobBuilder ilStream, out BlobBuilder fieldData);
            ManagedPEBuilder peBuilder = new(
                header: PEHeaderBuilder.CreateExecutableHeader(),
                metadataRootBuilder: new MetadataRootBuilder(metadataBuilder),
                ilStream: ilStream,
                mappedFieldData: fieldData,
                entryPoint: EntryPoint == null
                    ? default
                    : MetadataTokens.MethodDefinitionHandle(EntryPoint.MetadataToken));

            BlobBuilder peBlob = new();
            peBuilder.Serialize(peBlob);
            // Create the executable:
            using (FileStream fileStream = new(AsmName + ".dll", FileMode.Create, FileAccess.Write))
            {
                peBlob.WriteContentTo(fileStream);
            }


            BundleManifest manifest = new BundleManifest(6);
            manifest.Files.Add(new BundleFile($"{AsmName}.dll", BundleFileType.Assembly,
                contents: System.IO.File.ReadAllBytes($"{AsmName}.dll")));
            manifest.WriteUsingTemplate(
                $"{AsmName}.exe",
                BundlerParameters.FromTemplate(
                    appHostTemplatePath:
                    @"C:\Program Files\dotnet\packs\Microsoft.NETCore.App.Host.win-x64\10.0.0-preview.4.25258.110\runtimes\win-x64\native\apphost.exe",
                    appBinaryPath: $"{AsmName}.dll"
                    //imagePathToCopyHeadersFrom: @"C:\Path\To\Original\HelloWorld.exe"
                ));

            File.WriteAllText($"{AsmName}.runtimeconfig.json",
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

            File.WriteAllText($"{AsmName}.deps.json",
                $$"""
                  {
                    "runtimeTarget": {
                      "name": ".NETCoreApp,Version=v10.0",
                      "signature": ""
                    },
                    "compilationOptions": {},
                    "targets": {
                      ".NETCoreApp,Version=v10.0": {
                        "{{AsmName}}/1.0.0": {
                          "runtime": {
                            "{{AsmName}}.dll": {}
                          }
                        }
                      }
                    },
                    "libraries": {
                      "{{AsmName}}/1.0.0": {
                        "type": "project",
                        "serviceable": false,
                        "sha512": ""
                      }
                    }
                  }
                  """);
            return true;
        }
    }

    public class ClassStructure : StructureElement, IHaveAccessibility, IHaveClassModifiers, IHaveIdentifier,
        ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        public bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;

            TypeAttributes atr = TypeAttributes.Class;
            foreach (var e in MyClassModifiers.ElementList.Select(a => a.Value))
            {
                switch (e)
                {
                    case ClassModifiers.@static:
                        atr |= TypeAttributes.Sealed | TypeAttributes.Abstract;
                        break;
                    case ClassModifiers.@abstract:
                        atr |= TypeAttributes.Abstract;
                        break;
                    case ClassModifiers.@sealed:
                        atr |= TypeAttributes.Sealed;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            switch (MyAccessibility.Value)
            {
                case Accessibility.@private:
                    atr |= TypeAttributes.NestedPrivate;
                    break;
                case Accessibility.@public:
                    atr |= TypeAttributes.Public;
                    break;
                case Accessibility.@internal:
                    atr |= TypeAttributes.NestedPrivate;
                    break;
                case Accessibility.@protected:
                    atr |= TypeAttributes.NestedPrivate;
                    break;
                case null:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            //TODO: parent
            ilc.TypeBuilder = ilc.MainModule.DefineType(this.MyIdentifier.Identifier, atr, typeof(Object));

            foreach (var member in this.Members.ElementList)
            {
                member.CompileElement(ilc);
            }

            ilc.TypeBuilder.CreateType();


            ilc.TypeBuilder = null;
            return true;
        }


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
                if (reader.Peek() == '}')
                {
                    reader.Advance();
                    reader.SkipAllWhiteSpace();
                    return true;
                }
            }

            return false;
        }

        public EnumStructureElement<Accessibility> MyAccessibility { get; } = new();
        public StructureElementArray<EnumStructureElement<ClassModifiers>> MyClassModifiers { get; } = new();
        public SingleIdentifierStructure MyIdentifier { get; } = new();
    }

    public class ShardStructure : StructureElementArray<ClassStructure>, ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        public bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            foreach (var classStructure in this.ElementList)
            {
                classStructure.CompileElement(c);
            }

            return true;
        }
    }

    public enum Accessibility
    {
        _,
        @private,
        @public,
        @internal,
        @protected,
    }

    public interface IHaveAccessibility
    {
        public EnumStructureElement<Accessibility> MyAccessibility { get; }
    }

    public enum MethodModifiers
    {
        _ = 0,
        @entry,
        @static,
        @override,
        @virtual,
        @new,
        @abstract,
    }

    public interface IHaveModifiers<T> where T : struct, Enum
    {
        public ModifiersList<T> MyModifiers { get; }
    }

    public enum ClassModifiers
    {
        _ = 0,
        @static,
        @abstract,
        @sealed,
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
        public SingleIdentifierStructure this[Index index]
        {
            get { return ElementList[index]; }
        }

        public IdentifierStructure this[Range index]
        {
            get
            {
                var identifierStructure = new IdentifierStructure();
                identifierStructure.Position = this.Position;
                identifierStructure.ElementList = ElementList[index];
                return identifierStructure;
            }
        }

        public override char Separator => '.';

        public string FullIdentifier
        {
            get => string.Join('.', this.ElementList.Select(a => a.Identifier));
            set
            {
                ElementList.Clear();
                var split = value.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                foreach (var s in split)
                {
                    ElementList.Add(new SingleIdentifierStructure() { Identifier = s, Position = this.Position });
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

    public class ModifiersList<T> : StructureElementArray<EnumStructureElement<T>> where T : struct, Enum
    {
    }

    public abstract class MemberStructure<T> : StructureElement, IHaveAccessibility, IHaveModifiers<T>,
        IHaveTypeReference, IHaveIdentifier where T : struct, Enum
    {
        public EnumStructureElement<Accessibility> MyAccessibility { get; } = new();
        public ModifiersList<T> MyModifiers { get; } = new();
        public TypeReferenceStructure MyTypeReference { get; } = new();
        public SingleIdentifierStructure MyIdentifier { get; } = new();

        public override string ToString()
        {
            return $"{MyAccessibility} {MyModifiers} {MyTypeReference} {MyIdentifier}";
        }

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (!MyAccessibility.ParseElement(reader)) MyAccessibility.Value = Accessibility.@private;
            reader.SkipAllWhiteSpace();
            MyModifiers.ParseElement(reader);
            reader.SkipAllWhiteSpace();
            if (!MyTypeReference.ParseElement(reader)) return false;
            reader.SkipAllWhiteSpace();
            if (!MyIdentifier.ParseElement(reader)) return false;
            return true;
        }
    }

    public abstract class ExpressionStructure : StructureElement, ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        //? idk what i could put here besides just making it empty yet
        public abstract bool DoCompileElement(Compiler c);
    }

    public abstract class AbstractLiteralStructure : StructureElement,ICompileable
    {
        public abstract Type[] LiteralTypeOptions { get; }
        public abstract Type ResolvedType { get; }
        protected string ReadWord;


        public override string ToString()
        {
            return ReadWord;
        }

        public List<Type> SupportedCompilers { get; } = [typeof(ILCompiler)];
        public abstract bool DoCompileElement(Compiler c);
    }

    public abstract class AbstractIntegerLiteral : AbstractLiteralStructure
    {
        public override bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            var il = ilc.IL;
            if (IntValue.HasValue)
            {
                ilc.Push(typeof(int));
                il.Emit(OpCodes.Ldc_I4,IntValue.Value);
            }
            else if (UIntValue.HasValue)
            {
                ilc.Push(typeof(uint));
                il.Emit(OpCodes.Ldc_I4,UIntValue.Value);
            }
            else if (LongValue.HasValue)
            {
                ilc.Push(typeof(long));
                il.Emit(OpCodes.Ldc_I8,LongValue.Value);
            }
            else if (ULongValue.HasValue)
            {
                ilc.Push(typeof(ulong));
                il.Emit(OpCodes.Ldc_I8,ULongValue.Value);
            }
            else throw new NotImplementedException();
            return true;
        }

        public override Type[] LiteralTypeOptions =>
        [
            typeof(int), typeof(uint), typeof(long), typeof(ulong), typeof(Int128), typeof(UInt128)
        ];

        public char[] AllowedChars =
        [
            '_', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9',
            'U', 'u', 'L', 'l', 'x', 'X',
            'A', 'B', 'C', 'D', 'E', 'F',
            'a', 'b', 'c', 'd', 'e', 'f',
            'b', 'B'
        ];

        public bool Unsigned = false;
        public bool Long = false;

        public int? IntValue;
        public uint? UIntValue;
        public long? LongValue;
        public ulong? ULongValue;

        public override Type ResolvedType
        {
            get
            {
                if (IntValue.HasValue) return IntValue.Value.GetType();
                if (UIntValue.HasValue) return UIntValue.Value.GetType();
                if (LongValue.HasValue) return LongValue.Value.GetType();
                if (ULongValue.HasValue) return ULongValue.Value.GetType();
                return null;
            }
        }

        protected bool StartedWritingDigits = false;
        protected bool EndedWritingDigits = false;
        protected StringBuilder DigitsWord = new();
    }

    public class DecimalIntegerLiteral : AbstractIntegerLiteral
    {
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            ReadWord = reader.PeekWord;
            reader.Advance(ReadWord);
            for (int i = 0; i < ReadWord.Length; i++)
            {
                char cur = ReadWord[i];
                if (!AllowedChars.Contains(cur)) return false;
                if (i == 0 && cur == '_') return false;
                if (i == ReadWord.Length - 1 && cur == '_') return false;
                if (i == 1 && cur == 'x' || cur == 'X' && ReadWord[0] == '0') return false;
                if (i == 1 && cur == 'b' || cur == 'B' && ReadWord[0] == '0') return false;
                if (cur!='_' && !StartedWritingDigits)
                    StartedWritingDigits = true;
                if (cur == 'U' || cur == 'u')
                {
                    EndedWritingDigits = true;
                    if (!Unsigned) Unsigned = true;
                    else return false;
                }

                else if (cur == 'L' || cur == 'l')
                {
                    EndedWritingDigits = true;
                    if (!Long) Long = true;
                    else return false;
                }else if (EndedWritingDigits)
                {
                    Debugger.Break();
                    return false;
                }

                if (cur!='_' && StartedWritingDigits && !EndedWritingDigits)
                {
                    DigitsWord.Append(cur);
                }
            }

            if (DigitsWord.Length > 0)
            {
                if (Long && Unsigned)
                {
                    if (ulong.TryParse(DigitsWord.ToString(), out ulong res))
                    {
                        this.ULongValue = res;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Long)
                {
                    if (long.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out long res1))
                    {
                        this.LongValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Unsigned)
                {
                    if (uint.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out uint res1))
                    {
                        this.UIntValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (int.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out int res1))
                    {
                        this.IntValue = res1;
                        return true;
                    }
                    else if (uint.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out uint res2))
                    {
                        this.UIntValue = res2;
                        return true;
                    }
                    else if (long.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out long res3))
                    {
                        this.LongValue = res3;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.Integer, null, out ulong res4))
                    {
                        this.ULongValue = res4;
                        return true;
                    }
                    else
                        return false;
                }

                return false;
            }

            return false;
        }
    }

    public class HexadecimalIntegerLiteral : AbstractIntegerLiteral
    {
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            ReadWord = reader.PeekWord;
            reader.Advance(ReadWord);
            for (int i = 0; i < ReadWord.Length; i++)
            {
                char cur = ReadWord[i];
                if (!AllowedChars.Contains(cur)) return false;
                if (i == 0 && cur == '_') return false;
                if (i == ReadWord.Length - 1 && cur == '_') return false;
                if (i == 1 && cur == 'x' || cur == 'X' && ReadWord[0] == '0')
                {
                    StartedWritingDigits = true;
                    continue;
                }

                if (i == 1 && cur == 'b' || cur == 'B' && ReadWord[0] == '0') return false;
                //if (cur!='_' && !StartedWritingDigits)
                //    StartedWritingDigits = true;
                if (cur == 'U' || cur == 'u')
                {
                    EndedWritingDigits = true;
                    if (!Unsigned) Unsigned = true;
                    else return false;
                }
                else if (cur == 'L' || cur == 'l')
                {
                    EndedWritingDigits = true;
                    if (!Long) Long = true;
                    else return false;
                }else if (EndedWritingDigits)
                {
                    Debugger.Break();
                    return false;
                }

                if (cur!='_'  && StartedWritingDigits && !EndedWritingDigits)
                {
                    DigitsWord.Append(cur);
                }
            }

            if (DigitsWord.Length > 0)
            {
                if (Long && Unsigned)
                {
                    if (ulong.TryParse(DigitsWord.ToString(), out ulong res))
                    {
                        this.ULongValue = res;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Long)
                {
                    if (long.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out long res1))
                    {
                        this.LongValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Unsigned)
                {
                    if (uint.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out uint res1))
                    {
                        this.UIntValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (int.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out int res1))
                    {
                        this.IntValue = res1;
                        return true;
                    }
                    else if (uint.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out uint res2))
                    {
                        this.UIntValue = res2;
                        return true;
                    }
                    else if (long.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out long res3))
                    {
                        this.LongValue = res3;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.HexNumber, null, out ulong res4))
                    {
                        this.ULongValue = res4;
                        return true;
                    }
                    else
                        return false;
                }

                return false;
            }

            return false;
        }
    }

    public class BinaryIntegerLiteral : AbstractIntegerLiteral
    {
        protected override bool DoParseElement(SimpleFileReader reader)
        {
            ReadWord = reader.PeekWord;
            reader.Advance(ReadWord);
            for (int i = 0; i < ReadWord.Length; i++)
            {
                char cur = ReadWord[i];
                if (!AllowedChars.Contains(cur)) return false;
                if (i == 0 && cur == '_') return false;
                if (i == ReadWord.Length - 1 && cur == '_') return false;
                if (i == 1 && cur == 'x' || cur == 'X' && ReadWord[0] == '0') return false;
                if (i == 1 && cur == 'b' || cur == 'B' && ReadWord[0] == '0')
                {
                    StartedWritingDigits = true;
                    continue;
                }
                

                //if (cur!='_' && !StartedWritingDigits)
                //    StartedWritingDigits = true;
                if (cur == 'U' || cur == 'u')
                {
                    EndedWritingDigits = true;
                    if (!Unsigned) Unsigned = true;
                    else return false;
                }
                else if (cur == 'L' || cur == 'l')
                {
                    EndedWritingDigits = true;
                    if (!Long) Long = true;
                    else return false;
                }else if (EndedWritingDigits)
                {
                    Debugger.Break();
                    return false;
                }

                if (cur!='_' && StartedWritingDigits && !EndedWritingDigits)
                {
                    DigitsWord.Append(cur);
                }
            }

            if (DigitsWord.Length > 0)
            {
                if (Long && Unsigned)
                {
                    if (ulong.TryParse(DigitsWord.ToString(), out ulong res))
                    {
                        this.ULongValue = res;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Long)
                {
                    if (long.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out long res1))
                    {
                        this.LongValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else if (Unsigned)
                {
                    if (uint.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out uint res1))
                    {
                        this.UIntValue = res1;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out ulong res2))
                    {
                        this.ULongValue = res2;
                        return true;
                    }
                    else
                        return false;
                }
                else
                {
                    if (int.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out int res1))
                    {
                        this.IntValue = res1;
                        return true;
                    }
                    else if (uint.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out uint res2))
                    {
                        this.UIntValue = res2;
                        return true;
                    }
                    else if (long.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out long res3))
                    {
                        this.LongValue = res3;
                        return true;
                    }
                    else if (ulong.TryParse(DigitsWord.ToString(), NumberStyles.BinaryNumber, null, out ulong res4))
                    {
                        this.ULongValue = res4;
                        return true;
                    }
                    else
                        return false;
                }

                return false;
            }

            return false;
        }
    }

    public class IntegerLiteralStructure : StructureElementControl<DecimalIntegerLiteral, HexadecimalIntegerLiteral,
        BinaryIntegerLiteral>,ICompileable
    {
        public Type ResolvedType
        {
            get
            {
                if (this.Element is AbstractIntegerLiteral integer)
                    return integer.ResolvedType;
                return null;
            }
        }

        //i think its done?


        public List<Type> SupportedCompilers => ((ICompileable)Element).SupportedCompilers;
        public bool DoCompileElement(Compiler c)
        {
            return ((ICompileable)Element).CompileElement(c);
        }
    }

    public class StringLiteralStructure : AbstractLiteralStructure
    {
   

        public override string ToString()
        {
            return $"\"{StringInterpretation}\"";
        }

        public override bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            var il = ilc.IL;
            il.Emit(OpCodes.Ldstr,StringInterpretation);
            ilc.Push(typeof(string));
            return true;
        }

        public System.String StringInterpretation = "";

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

                if (reader.PeekString("\\\"", true))
                    StringInterpretation += "\"";
                goto again;
            }
            else return false;
        }

        public override Type[] LiteralTypeOptions => [typeof(string)];
        public override Type ResolvedType => typeof(string);
    }

    public class BooleanLiteralStructure : AbstractLiteralStructure
    {
     

        public override Type[] LiteralTypeOptions => [typeof(bool)];
        public override Type ResolvedType => typeof(bool);

        public override string ToString()
        {
            return Value.ToString();
        }

        public override bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            var il = ilc.IL;
            if (!this.Value.HasValue) throw new NotImplementedException();
            if(this.Value.Value)
                il.Emit(OpCodes.Ldc_I4_1);
            else il.Emit(OpCodes.Ldc_I4_0);
            ilc.Push(typeof(bool));
            return true;
        }

        public bool? Value;

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            var word = reader.PeekWord;
            if (bool.TryParse(word, out var b))
            {
                Value = b;
                reader.Advance(word);
                return true;
            }

            return false;
        }
    }

    public class LiteralStructure : StructureElementControl<BooleanLiteralStructure, IntegerLiteralStructure,
        StringLiteralStructure>, ICompileable
    {
        public List<Type> SupportedCompilers => ((ICompileable)Element).SupportedCompilers;
        public bool DoCompileElement(Compiler c)
        {
            return ((ICompileable)Element).CompileElement(c);
        }
    }

    public class InvokationParameterStructure : StructureElementControl<LiteralStructure,FunctionInvokationExpressionStructure,
        IdentifierStructure>
    {
        public List<Type> SupportedCompilers => ((ICompileable)Element).SupportedCompilers;
        public bool DoCompileElement(Compiler c)
        {
            return ((ICompileable)Element).CompileElement(c);
        }
    }

    public class FunctionInvokationExpressionStructure : ExpressionStructure
    {
        public override bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            var il = ilc.IL;

            //TODO: left off here!      

            var methodName = this.MethodIdentifier[^1];
            var typeName = this.MethodIdentifier[..^1];
            if (string.IsNullOrEmpty(typeName.FullIdentifier))
            {
                //TODO: either inside the this type, or was got via using's im yet to implement
                Debugger.Break();
            }

            //TODO: get method with params
            foreach (var para in this.Parameters.ElementList)
            {
                para.CompileElement(c);
            }
            var pCount = Parameters.ElementList.Count;
            var paramTypes = ilc.Pop(pCount,true);
            

            Type theType = ilc.GetType(typeName.FullIdentifier);
            if (theType == null) throw new TypeLoadException("Type not found!");

            var gotMethod = theType.GetMethod(methodName.Identifier,
                paramTypes.ToArray());

            

            il.EmitCall(OpCodes.Call, gotMethod, null);


            return true;
        }

        public override string ToString()
        {
            return $"{MethodIdentifier}({Parameters})";
        }

        public StructureElementList<InvokationParameterStructure> Parameters = new();

        public IdentifierStructure MethodIdentifier = new();

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
                }
                else reader.Advance();
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

    public class IfExpression : ExpressionStructure
    {
        public override bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            
            if (IfInsides.Element is ICompileable compileable)
            {
                compileable.CompileElement(c);
            }
           
            
            //TODO: type conversion
            if (ilc.Pop() != typeof(bool)) throw new TypeLoadException($"Not convertible to bool");
            
            //TODO: some stack validation i need to make
            
            
            var l =ilc.IL.DefineLabel(); ;
            var skipElse = ilc.IL.DefineLabel();
            
            ilc.IL.Emit(OpCodes.Brfalse, l);

            FuncBody.CompileElement(c);

            if (Else.Element != null)
            {
                ilc.IL.Emit(OpCodes.Br,skipElse);
            }
            
            ilc.IL.MarkLabel(l);
            
            if (Else.Element != null)
            {
                Else.CompileElement(ilc);
            
                ilc.IL.MarkLabel(skipElse);
            }
            
            
            return true;
        }

        public override string ToString()
        {
            return $"if({IfInsides}){{{FuncBody}}}";
        }

        public InvokationParameterStructure IfInsides = new();
        public FuncBodyStructure FuncBody = new();
        public StructureElementControl<IfExpression, FuncBodyStructure> Else;

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            reader.SkipAllWhiteSpace();
            if (!reader.PeekString("if", true)) return false;
            reader.SkipAllWhiteSpace();
            if (reader.Peek() != '(') return false;
            reader.Advance();
            reader.SkipAllWhiteSpace();
            if (reader.Peek() != ')')
            {
                reader.SkipAllWhiteSpace();
                if (!IfInsides.ParseElement(reader)) return false;
                reader.SkipAllWhiteSpace();
                if (reader.Peek() == ')')
                {
                    reader.Advance();
                }
                else
                {
                    Debugger.Break();
                    return false;
                }
            }
            else reader.Advance();

            reader.SkipAllWhiteSpace();
            FuncBody.ParseElement(reader);

            if (reader.PeekString("else", true))
            {
                Else = new();
                Else.ParseElement(reader);
            }
            
            //Debugger.Break();
            //TODO: else

            return true;
        }
    }

    public class FuncBodyStructure : StructureElementList<
        StructureElementControl<FuncBodyStructure, IfExpression, FunctionInvokationExpressionStructure>>, ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        public bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            ilc.IL.BeginScope();

            foreach (var expr in this.ElementList)
            {
                expr.CompileElement(ilc);
            }

            ilc.IL.EndScope();
            return true;
        }

        public override char Separator => ';';

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            reader.SkipAllWhiteSpace();
            if (reader.Peek() == '{')
            {
                reader.Advance();
                //Do method body
                if (!base.DoParseElement(reader))
                {
                    //empty body
                }

                reader.SkipAllWhiteSpace();
                if (reader.Peek() == '}')
                {
                    reader.Advance();
                    reader.SkipAllWhiteSpace();
                    return true;
                }
            }
            else
            {
                //TODO: single line
                return false;
            }

            Debugger.Break();
            return false;
        }
    }

    public class FuncStructure : MemberStructure<MethodModifiers>, ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        public bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;
            MethodAttributes atr = MethodAttributes.PrivateScope;
            switch (MyAccessibility.Value)
            {
                case Accessibility.@private:
                    atr |= MethodAttributes.Private;
                    break;
                case Accessibility.@public:
                    atr |= MethodAttributes.Public;
                    break;
                case Accessibility.@internal:
                    atr |= MethodAttributes.Private;
                    break;
                case Accessibility.@protected:
                    atr |= MethodAttributes.Private;
                    break;
                case null:
                default:
                    throw new ArgumentOutOfRangeException();
            }

            bool setEntry = false;
            foreach (var v in MyModifiers.ElementList.Select(a => a.Value))
            {
                switch (v)
                {
                    case MethodModifiers.entry:
                        if (ilc.EntryPoint == null)
                        {
                            setEntry = true;
                        }
                        else
                        {
                            throw new Exception("???");
                            //TODO: this is an error.
                            return false;
                        }

                        break;
                    case MethodModifiers.@static:
                        atr |= MethodAttributes.Static;
                        break;
                    case MethodModifiers.@override:
                        atr |= MethodAttributes.ReuseSlot;
                        break;
                    case MethodModifiers.@virtual:
                        atr |= MethodAttributes.Virtual;
                        break;
                    case MethodModifiers.@new:
                        atr |= MethodAttributes.NewSlot;
                        break;
                    case MethodModifiers.@abstract:
                        atr |= MethodAttributes.Abstract;
                        break;
                    case null:
                    case MethodModifiers._:
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }

            ilc.MethodBuilder = ilc.TypeBuilder.DefineMethod(this.MyIdentifier.Identifier, atr,
                ilc.GetType(this.MyTypeReference.FullIdentifier),
                this.Parameters.ElementList.Select(a => ilc.GetType(a.MyTypeReference.FullIdentifier)).ToArray()
            );
            if (setEntry)
            {
                            ilc.EntryPoint = ilc.MethodBuilder;
            }
            for (int i = 0; i < Parameters.ElementList.Count; i++)
            {
                var p = Parameters.ElementList[i];
                ilc.MethodBuilder.DefineParameter(i, ParameterAttributes.None, p.MyIdentifier.Identifier);
            }

            ilc.IL = ilc.MethodBuilder.GetILGenerator();
            this.FuncBody.CompileElement(ilc);
            ilc.IL.Emit(OpCodes.Ret);

            return true;
        }

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
                }
                else reader.Advance();
            }
            else reader.Advance();

            reader.SkipAllWhiteSpace();

            if (reader.Peek() == ';')
            {
                //Done with the method, maybe has no body somehow
                reader.Advance();
                return true;
            }
            else
            {
                if (FuncBody.ParseElement(reader)) return true;
            }

            //Debugger.Break();
            return false;
        }
    }

    public enum FieldModifiers
    {
        _ = 0,
        @static,
    }

    public class FieldStructure : MemberStructure<FieldModifiers>, ICompileable
    {
        public List<Type> SupportedCompilers => [typeof(ILCompiler)];

        public bool DoCompileElement(Compiler c)
        {
            if (c is not ILCompiler ilc) return false;

            //TODO:
            return false;
        }


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
        get { return _pos; }
        set
        {
            //Console.WriteLine(System.Environment.StackTrace);
            _pos = value;
        }
    }

    public char PeekChar => Peek();

    //[] for type's and <> for generics, idk for other stuff lol
    public readonly List<char> WordEndingChars = new()
        { ';', '(', ')', '{', '}', ',', '+', '/', ':', '-', '=', '*', '.' };

    public string PeekWord => Position >= this.FileContents.Length
        ? string.Empty
        : PeekUntil(c => char.IsWhiteSpace(c) || WordEndingChars.Contains(c));

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
        if (am == 6 && posFirst == 0 && Position == 5)
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

        Retreat((uint)builder.Length);
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


    public abstract class Compiler
    {
        public abstract bool CompileToFile();
    }

    public interface ICompileable
    {
        public abstract List<Type> SupportedCompilers { get; }

        public sealed bool HiddenCompileElement(Compiler c)
        {
            if (!SupportedCompilers.Contains(c.GetType())) return false;
            return DoCompileElement(c);
        }

        protected abstract bool DoCompileElement(Compiler c);
    }

    public abstract class StructureElement
    {
        public uint Position { get; set; }
        public uint Size { get; protected set; }


        public bool ParseElement(SimpleFileReader reader)
        {
            this.Position = reader.Position;
            var b = DoParseElement(reader);
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
    //make more compile too if i do that
    public class StructureElementControl<T1, T2, T3, T4, T5> : StructureElementControl<T1, T2, T3, T4>
        where T1 : StructureElement, new()
        where T2 : StructureElement, new()
        where T3 : StructureElement, new()
        where T4 : StructureElement, new()
        where T5 : StructureElement, new()
    {
        protected T5 _T5 = new();

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
        protected T4 _T4 = new();

        protected override bool DoParseElement(SimpleFileReader reader)
        {
            if (base.DoParseElement(reader)) return true;
            return HandleElement(ref _T4, reader);
        }
    }

    public class StructureElementControl<T1, T2, T3> : StructureElementControl<T1, T2>
        where T1 : StructureElement, new() where T2 : StructureElement, new() where T3 : StructureElement, new()
    {
        protected T3 _T3 = new();

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

        protected T1 _T1 = new();
        protected T2 _T2 = new();
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

            return ElementList.Count > 0;
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
                reader.SkipAllWhiteSpace();
                var structureElement = new T();
                if (structureElement.ParseElement(reader)) ElementList.Add(structureElement);

                else break;
            }

            return ElementList.Count > 0;
        }
    }
}
//File extension names to be used:
//amethyst, shard, geode
//owo ?
// amethyst - project file
// shard - code file
// geode - solution file