namespace SwU.HttpFramework.Mvc;

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class ControllerAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public abstract class HttpMethodAttribute : Attribute
{
    public string HttpMethod { get; }
    public string Route { get; }

    protected HttpMethodAttribute(string httpMethod, string route)
    {
        HttpMethod = httpMethod;
        Route = route ?? "/";
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpGetAttribute : HttpMethodAttribute
{
    public HttpGetAttribute(string route) : base("GET", route)
    {
    }
}

[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
public class HttpPostAttribute : HttpMethodAttribute
{
    public HttpPostAttribute(string route) : base("POST", route)
    {
    }
}
