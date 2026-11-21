using System.Collections.Concurrent;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using SwU.WebApp.Models;
using SwU.WebApp.Repositories;

namespace SwU.WebApp.Services;

public class AuthService
{
    public const string SessionCookieName = "swu_session";

    private readonly UserRepository _users;

    // В памяти храним только идентификатор сессии и id пользователя
    private readonly ConcurrentDictionary<string, int> _sessions = new();

    public AuthService(UserRepository users)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
    }

    public static string HashPassword(string password)
    {
        byte[] input = Encoding.UTF8.GetBytes("swu_salt_2026:" + password);
        byte[] hash = SHA256.HashData(input);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public async Task<string?> LoginAsync(string login, string password)
    {
        string normalizedLogin = login.Trim();
        User? user = await _users.GetByUsernameAsync(normalizedLogin);
        if (user == null)
        {
            user = await _users.GetByEmailAsync(normalizedLogin);
        }

        if (user == null)
        {
            return null;
        }

        string passwordHash = HashPassword(password);
        if (!string.Equals(passwordHash, user.PasswordHash, StringComparison.Ordinal))
        {
            return null;
        }

        return CreateSession(user.Id);
    }

    public async Task<string?> RegisterAsync(string username, string email, string password)
    {
        User user = new()
        {
            Username = username.Trim(),
            Email = email.Trim(),
            PasswordHash = HashPassword(password),
            Role = "User",
            CreatedAt = DateTime.Now
        };

        await _users.CreateAsync(user);
        return user.Id > 0 ? CreateSession(user.Id) : null;
    }

    public async Task<User?> GetCurrentUserAsync(HttpListenerContext context)
    {
        string? sessionId = GetSessionId(context);
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return null;
        }

        if (!_sessions.TryGetValue(sessionId, out int userId))
        {
            return null;
        }

        // Всегда читаем свежую строку из БД: удаление или смена роли применяются сразу
        User? user = await _users.GetByIdAsync(userId);
        if (user == null)
        {
            _sessions.TryRemove(sessionId, out _);
        }

        return user;
    }

    public static string? GetSessionId(HttpListenerContext context)
    {
        Cookie? cookie = context.Request.Cookies[SessionCookieName];
        return cookie?.Value;
    }

    public void Logout(string? sessionId)
    {
        if (!string.IsNullOrWhiteSpace(sessionId))
        {
            _sessions.TryRemove(sessionId, out _);
        }
    }

    private string CreateSession(int userId)
    {
        string sessionId = Guid.NewGuid().ToString("N");
        _sessions[sessionId] = userId;
        return sessionId;
    }
}
