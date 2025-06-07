namespace Parser;

public abstract class AbstractSubParser : AbstractParser
{
    public int Position;
    public struct WithPosition<T>
    {
        public T Value;
        public int Position;
        public int EndPosition;

        public WithPosition(T v, int p,int ep)
        {
            Value = v;
            Position = p;
            EndPosition = ep;
        }
    }
}