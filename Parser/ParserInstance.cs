using System.Diagnostics;

namespace Parser;

public class ParserInstance
{
    public readonly Dictionary<string, AbstractFileParser> FileAssociations = new();

    public T RegisterFileParser<T>() where T : AbstractFileParser, new()
    {
        var n = new T();
        FileAssociations[n.Extension] = n;
        return n;
    }

    private static ReadOnlySpan<char>  GetFileExtension(ReadOnlySpan<char>  path)
    {
        ReadOnlySpan<char> fileName = Path.GetFileName(path);
        int lastPeriod = fileName.LastIndexOf('.');
        return lastPeriod < 0 ?
            throw new Exception("No file extension found") : // No extension was found
            fileName[(lastPeriod+1)..];
    }

    public void ParseFile(string path)
    {
        var ext = GetFileExtension(path);
        if (!FileAssociations.TryGetValue(ext.ToString(), out var parser))
            throw new Exception($"No matching parser for file with extension .{ext}");
        parser.ParseFile(path);
    }
    public void ParseFile(string path)
    {
        FileReader.OpenFile(path);
        Parse();
    }

    public class SimpleFileReader
    {
        public string FileContents=string.Empty;
        public int Position { get; protected set; }

        public char Peek()
        {
            return FileContents[Position];
        }

        public void Advance(int am=1) => Position+=am;
        public void Retreat(int am=1) => Position-=am;
        public char Read()
        {
            char c= Peek();
            Advance();
            return c;
        }

        public string Peek(int length)
        {
            StringBuilder builder = new StringBuilder(length);
            for (int i = 0; i < length; i++)
            {
                builder[i] = Read();
            }
            Retreat(length);
            return builder.ToString();
        }
        public string Read(int length)
        {
            var s = Peek(length);
            Advance(length);
            return s;
        }

        public string Peek(AbstractRule rule)
        {
            StringBuilder builder = new StringBuilder();
            var c = Peek();
            if (rule.IsValidChar(c)) Advance();
            while (rule.IsValidChar(c))
            {
                builder.Append(c);
                c = Read();
            }
            Retreat(builder.Length+1);
            return builder.ToString();
        }
        public string Read(AbstractRule rule)
        {
            var peek = Peek(rule);
            Advance(peek.Length);
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
            Position = -1;
        }
    }

    public SimpleFileReader FileReader=new();
}