using Npgsql;
using SwU.HttpFramework.Mvc;
using SwU.HttpFramework.Mvc.Results;
using SwU.HttpFramework.Templating;
using SwU.WebApp.Models;
using SwU.WebApp.Repositories;
using SwU.WebApp.Services;

namespace SwU.WebApp.Controllers;

[Controller]
public class AuthController : AppController
{
    private const int SessionMaxAgeSeconds = 604800;

    private readonly UserRepository _users;
    private readonly ValidationService _validation;

    public AuthController(
        UserRepository users,
        AuthService authService,
        ITemplateEngine templateEngine,
        ValidationService validation)
        : base(templateEngine, authService)
    {
        _users = users;
        _validation = validation;
    }

    [HttpGet("/login")]
    public async Task<IActionResult> LoginPage()
    {
        User? user = await GetCurrentUserAsync();
        if (user != null)
        {
            return Redirect("/");
        }

        string error = GetLoginErrorMessage(GetQuery("error"));
        return RenderLogin(error, string.Empty, null, 200);
    }

    [HttpPost("/login")]
    public async Task<IActionResult> Login()
    {
        User? currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            return Redirect("/");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        string login = FormValue(form, "login").Trim();
        string password = FormValue(form, "password");

        if (login.Length == 0 || password.Length == 0)
        {
            return RenderLogin("Please enter your login and password.", login, null, 400);
        }

        string? sessionId = await AuthService.LoginAsync(login, password);
        if (sessionId == null)
        {
            return RenderLogin("Invalid login or password.", login, null, 400);
        }

        SetSessionCookie(sessionId);
        return Redirect("/");
    }

    [HttpGet("/register")]
    public async Task<IActionResult> RegisterPage()
    {
        User? user = await GetCurrentUserAsync();
        if (user != null)
        {
            return Redirect("/");
        }

        return RenderRegister(string.Empty, string.Empty, string.Empty, null, 200);
    }

    [HttpPost("/register")]
    public async Task<IActionResult> Register()
    {
        User? currentUser = await GetCurrentUserAsync();
        if (currentUser != null)
        {
            return Redirect("/");
        }

        Dictionary<string, string> form = await ReadFormAsync();
        string username = FormValue(form, "username").Trim();
        string email = FormValue(form, "email").Trim();
        string password = FormValue(form, "password");
        string confirmPassword = FormValue(form, "confirmPassword");

        string error = ValidateRegistrationInput(username, email, password, confirmPassword);
        if (error.Length > 0)
        {
            return RenderRegister(error, username, email, null, 400);
        }

        error = await FindRegistrationConflictAsync(username, email);
        if (error.Length > 0)
        {
            return RenderRegister(error, username, email, null, 400);
        }

        try
        {
            string? sessionId = await AuthService.RegisterAsync(username, email, password);
            if (sessionId == null)
            {
                return RenderRegister("Could not create a session. Please try again.", username, email, null, 500);
            }

            SetSessionCookie(sessionId);
            return Redirect("/");
        }
        catch (PostgresException exception)
        {
            string message = DbErrorHelper.GetMessage(
                exception,
                "A user with this login or email already exists.");
            return RenderRegister(message, username, email, null, 400);
        }
    }

    [HttpPost("/logout")]
    public IActionResult Logout()
    {
        string? sessionId = AuthService.GetSessionId(Context);
        AuthService.Logout(sessionId);
        DeleteCookie(AuthService.SessionCookieName);
        return Redirect("/");
    }

    private IActionResult RenderLogin(string error, string login, User? user, int statusCode)
    {
        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = "Log in",
            ["Error"] = error,
            ["Login"] = login
        };

        return RenderView("login.html", model, user, statusCode);
    }

    private IActionResult RenderRegister(
        string error,
        string username,
        string email,
        User? user,
        int statusCode)
    {
        Dictionary<string, object?> model = new()
        {
            ["PageTitle"] = "Register",
            ["Error"] = error,
            ["Username"] = username,
            ["Email"] = email
        };

        return RenderView("register.html", model, user, statusCode);
    }

    private void SetSessionCookie(string sessionId)
    {
        SetCookie(AuthService.SessionCookieName, sessionId, httpOnly: true, maxAgeSeconds: SessionMaxAgeSeconds);
    }

    private string ValidateRegistrationInput(
        string username,
        string email,
        string password,
        string confirmPassword)
    {
        if (!_validation.IsValidUsername(username))
        {
            return "Login: 3–20 characters, Latin letters, digits and underscore only.";
        }

        if (!_validation.IsValidEmail(email))
        {
            return "Please enter a valid email address.";
        }

        if (!_validation.IsValidPassword(password))
        {
            return "Password must be 6 to 100 characters long.";
        }

        if (!string.Equals(password, confirmPassword, StringComparison.Ordinal))
        {
            return "Passwords do not match.";
        }

        return string.Empty;
    }

    private async Task<string> FindRegistrationConflictAsync(string username, string email)
    {
        User? existingUsername = await _users.GetByUsernameAsync(username);
        if (existingUsername != null)
        {
            return "This login is already taken.";
        }

        User? existingEmail = await _users.GetByEmailAsync(email);
        if (existingEmail != null)
        {
            return "This email is already registered.";
        }

        return string.Empty;
    }

    private static string GetLoginErrorMessage(string? errorCode)
    {
        switch (errorCode)
        {
            case "access_denied":
                return "You do not have permission to access this section.";
            case "login_required":
                return "You need to log in to perform this action.";
            default:
                return string.Empty;
        }
    }
}
