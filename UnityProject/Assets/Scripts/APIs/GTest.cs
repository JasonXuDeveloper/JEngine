public class GTest<T>
{
    public int a;
    public T b;

    public override string ToString()
    {
        return $"a={a},b={b}";
    }
}

public class JSONTest
{
    public int a;
    public float b;
    public double c;
    public bool d;
    public string e;
}