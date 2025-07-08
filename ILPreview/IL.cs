public static class AProgram
{
    public static  void main()
    {
        for (int i = 0; i < 10; i++)
        {
            System.Console.WriteLine(Fib(i));
        }
    }
    public static int Fib(int n) {
        if (n < 2)
        {
            return n;
        }
        return Fib(n - 1) + Fib(n - 2);
    }
}