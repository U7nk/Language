namespace TestProject1;

public class TestContext
{
    public Type Type { get; set; }
    public string MemberName { get; set; }

    public List<List<int>> ListOfList { get; set; }

    public bool IntEquals(int left, int right)
    {
        return left == right;
    }

    public bool ObjectEquals(object left, object right)
    {
        return left == right;
    }

    public bool StringEquals(object left, object right)
    {
        return String.Equals(left, right);
    }

    public string OverloadedMethod(string str)
    {
        return str;
    }

    public int OverloadedMethod(int number)
    {
        return number;
    }

    public bool FuncTwoParams(int fParam, Func<int, int, bool> funcTwoParams, int sParam)
    {
        return funcTwoParams(fParam, sParam);
    }
}