using System.Net;
using System.Text;
using System.Text.Json;
using Samostoyatelnaya_31_10.Models;

string[] technologies = { "csharp", "javascript", "python" };
string[] statuses = { "active", "archived" };

var projects = new List<Project>();
var locker = new object();
int nextId = 1;

var jsonOptions = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

var listener = new HttpListener();
listener.Prefixes.Add("http://localhost:5000/");
listener.Start();
Console.WriteLine("Server started: http://localhost:5000/");

while (true)
{
    var context = listener.GetContext();
    try
    {
        Handle(context);
    }
    catch (Exception)
    {
        Send(context, 500, new { error = "Internal server error" });
    }
}

void Handle(HttpListenerContext context)
{
    string method = context.Request.HttpMethod;
    string path = context.Request.Url!.AbsolutePath.TrimEnd('/');
    if (path == "") path = "/";

    if (path == "/projects")
    {
        if (method == "GET") GetProjects(context);
        else Send(context, 405, new { error = "Method not allowed" });
    }
    else if (path == "/projects/details")
    {
        if (method == "GET") GetDetails(context);
        else Send(context, 405, new { error = "Method not allowed" });
    }
    else if (path == "/projects/create")
    {
        if (method == "POST") CreateProject(context);
        else Send(context, 405, new { error = "Method not allowed" });
    }
    else if (path == "/projects/update")
    {
        if (method == "PUT") UpdateProject(context);
        else Send(context, 405, new { error = "Method not allowed" });
    }
    else
    {
        Send(context, 404, new { error = "Route not found" });
    }
}

void GetProjects(HttpListenerContext context)
{
    string? technology = context.Request.QueryString["technology"];

    if (technology != null && !technologies.Contains(technology))
    {
        Send(context, 400, new { error = "technology must be: csharp, javascript or python" });
        return;
    }

    lock (locker)
    {
        var result = projects
            .Where(p => technology == null || p.Technology == technology)
            .ToList();
        Send(context, 200, result);
    }
}

void GetDetails(HttpListenerContext context)
{
    if (!int.TryParse(context.Request.QueryString["id"], out int id))
    {
        Send(context, 400, new { error = "id is required and must be a number" });
        return;
    }

    lock (locker)
    {
        var project = projects.FirstOrDefault(p => p.Id == id);
        if (project == null)
            Send(context, 404, new { error = "Project not found" });
        else
            Send(context, 200, project);
    }
}

void CreateProject(HttpListenerContext context)
{
    var request = ReadBody(context);
    if (request == null)
    {
        Send(context, 400, new { error = "Invalid JSON body" });
        return;
    }

    string? error = ValidateTitle(request.Title) ?? ValidateTechnology(request.Technology);
    if (error != null)
    {
        Send(context, 400, new { error });
        return;
    }

    lock (locker)
    {
        var project = new Project
        {
            Id = nextId++,
            Title = request.Title!.Trim(),
            Technology = request.Technology!,
            Status = "active"
        };
        projects.Add(project);
        Send(context, 201, project);
    }
}

void UpdateProject(HttpListenerContext context)
{
    if (!int.TryParse(context.Request.QueryString["id"], out int id))
    {
        Send(context, 400, new { error = "id is required and must be a number" });
        return;
    }

    var request = ReadBody(context);
    if (request == null)
    {
        Send(context, 400, new { error = "Invalid JSON body" });
        return;
    }

    string? error = ValidateTitle(request.Title)
                    ?? ValidateTechnology(request.Technology)
                    ?? ValidateStatus(request.Status);
    if (error != null)
    {
        Send(context, 400, new { error });
        return;
    }

    lock (locker)
    {
        var project = projects.FirstOrDefault(p => p.Id == id);
        if (project == null)
        {
            Send(context, 404, new { error = "Project not found" });
            return;
        }

        project.Title = request.Title!.Trim();
        project.Technology = request.Technology!;
        project.Status = request.Status!;
        Send(context, 200, project);
    }
}

string? ValidateTitle(string? title)
{
    if (title == null) return "title is required";
    int length = title.Trim().Length;
    if (length < 5 || length > 100) return "title must be from 5 to 100 characters";
    return null;
}

string? ValidateTechnology(string? technology)
{
    if (technology == null || !technologies.Contains(technology))
        return "technology must be: csharp, javascript or python";
    return null;
}

string? ValidateStatus(string? status)
{
    if (status == null || !statuses.Contains(status))
        return "status must be: active or archived";
    return null;
}

ProjectRequest? ReadBody(HttpListenerContext context)
{
    try
    {
        using var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8);
        string text = reader.ReadToEnd();
        return JsonSerializer.Deserialize<ProjectRequest>(text, jsonOptions);
    }
    catch (JsonException)
    {
        return null;
    }
}

void Send(HttpListenerContext context, int statusCode, object data)
{
    string json = JsonSerializer.Serialize(data, jsonOptions);
    byte[] bytes = Encoding.UTF8.GetBytes(json);

    context.Response.StatusCode = statusCode;
    context.Response.ContentType = "application/json; charset=utf-8";
    context.Response.ContentLength64 = bytes.Length;
    context.Response.OutputStream.Write(bytes, 0, bytes.Length);
    context.Response.Close();
}