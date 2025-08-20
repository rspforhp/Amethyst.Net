using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using SequentialParser.Regex;
using SequentialParser.Regex.CharacterClasses;

namespace SequentialParser;

/// <summary>
/// Mutable struct for reading strings easily
/// Supports unicode, support it too if using this
/// </summary>
[DebuggerDisplay("{LeftString()}")]
[ImmutableObject(false)]
public struct AdvancedStringReader : IDisposable
{
    private readonly Stack<(uint, string,int)> PositionStack = new();

    public void PushPosition()
    {
        var t = new StackTrace(1);
        var f = t.GetFrame(0);
        var n = f.GetMethod().DeclaringType.Name;
        PositionStack.Push((CurrentPosition,n,f.GetILOffset()));
    }
    public void PopPosition()
    {
        var t = new StackTrace(1);
        var f = t.GetFrame(0);
        var n = f.GetMethod().DeclaringType.Name;
        var pn = PositionStack.Peek().Item2;
        if (pn == n) PopPositionUnsafe();
        else throw new Exception("Not ur stack.");
    }
    public void PopPositionUnsafe()
    {
       CurrentPosition= PositionStack.Pop().Item1;
    }
    public void DontPopPosition()
    {
       PositionStack.Pop();
    }
    public void Dispose()
    {
        PopPosition();
    }

    public readonly string UnderlyingString;
    public uint CurrentPosition;
    public int Position
    {
        get => (int)CurrentPosition;
        set => CurrentPosition = (uint)value;
    }
    public string LeftString() => UnderlyingString[Position..];

    public string ReadChar()
    {
        if (Position >= UnderlyingString.Length) return null;
        Rune r = Rune.GetRuneAt(UnderlyingString, Position);
        Position += r.Utf16SequenceLength;
        return r.ToString();
    }

    public bool TryReadChar(out string sc)
    {
        sc = ReadChar();
        return sc != null;
    }
   
    public string Read(IClassCharacter c)
    {
        return c.ReadWithQuantifier(ref this);
    }

    public string Read(uint charAmount)
    {
        StringBuilder builder = new();
        for (int i = 0; i < charAmount  ; i++)
            builder.Append(ReadChar());
        return builder.ToString();
    }



    public Dictionary<string, GroupConstructHandler> Handlers = new();
    public Dictionary<string, GroupConstruct?> NamedGroups = new();
    
    public AdvancedStringReader(string toRead)
    {
        UnderlyingString = toRead;
    }

    public AdvancedStringReader AddHandler(string key, GroupConstructHandler handler)
    {
        Handlers.Add(key,handler);
        return this;
    }
    public AdvancedStringReader CopyHandlers(Dictionary<string, GroupConstructHandler> handlers)
    {
        this.Handlers = handlers;
        return this;
    }
    public AdvancedStringReader CopyHandlers(ref AdvancedStringReader reader)
    {
        this.Handlers = reader.Handlers;
        return this;
    }
    
    
    public AdvancedStringReader AddGroup(string key,  GroupConstruct handler)
    {
        NamedGroups.Add(key,handler);
        return this;
    }
    public AdvancedStringReader CopyGroups(Dictionary<string,  GroupConstruct?> handlers)
    {
        this.NamedGroups = handlers;
        return this;
    }
    public AdvancedStringReader CopyGroups(ref AdvancedStringReader reader)
    {
        this.NamedGroups = reader.NamedGroups;
        return this;
    }
    public AdvancedStringReader(Stream toRead, bool disposeAfter = true)
    {
        using StreamReader sr = new StreamReader(toRead, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
        string r = sr.ReadToEnd();
        if(disposeAfter)toRead.Dispose();
        this=new AdvancedStringReader(r);
    }
}