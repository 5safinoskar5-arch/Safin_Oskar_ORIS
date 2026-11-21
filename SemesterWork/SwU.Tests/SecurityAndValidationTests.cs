using SwU.WebApp.Services;

namespace SwU.Tests;

[TestClass]
public class SecurityAndValidationTests
{
    private readonly ValidationService _validation = new();
    private readonly HtmlSanitizer _sanitizer = new();

    [TestMethod]
    public void IsValidEmail_AcceptsRealisticAddresses()
    {
        Assert.IsTrue(_validation.IsValidEmail("user@example.com"));
        Assert.IsTrue(_validation.IsValidEmail("admin@swudb.local"));
        Assert.IsTrue(_validation.IsValidEmail("  player@swudb.local  "));
    }

    [TestMethod]
    public void IsValidEmail_RejectsBrokenAddresses()
    {
        Assert.IsFalse(_validation.IsValidEmail("user@"));
        Assert.IsFalse(_validation.IsValidEmail("@example.com"));
        Assert.IsFalse(_validation.IsValidEmail("user example.com"));
        Assert.IsFalse(_validation.IsValidEmail(""));
        Assert.IsFalse(_validation.IsValidEmail(null));
    }

    [TestMethod]
    public void IsValidCardNumber_AcceptsSeedFormats()
    {
        Assert.IsTrue(_validation.IsValidCardNumber("IC27-061"));
        Assert.IsTrue(_validation.IsValidCardNumber("SOR-001"));
    }

    [TestMethod]
    public void IsValidCardNumber_RejectsWrongFormats()
    {
        Assert.IsFalse(_validation.IsValidCardNumber("123-abc"));
        Assert.IsFalse(_validation.IsValidCardNumber("SOR-1"));
        Assert.IsFalse(_validation.IsValidCardNumber("sor-001"));
        Assert.IsFalse(_validation.IsValidCardNumber("SOR_001"));
        Assert.IsFalse(_validation.IsValidCardNumber(null));
    }

    [TestMethod]
    public void IsValidUsername_ChecksLengthAndAllowedCharacters()
    {
        Assert.IsTrue(_validation.IsValidUsername("admin"));
        Assert.IsTrue(_validation.IsValidUsername("player_1"));

        Assert.IsFalse(_validation.IsValidUsername("ab"));
        Assert.IsFalse(_validation.IsValidUsername("user name"));
        Assert.IsFalse(_validation.IsValidUsername("user!"));
        Assert.IsFalse(_validation.IsValidUsername(null));
    }

    [TestMethod]
    public void IsValidComment_ChecksLengthLimits()
    {
        Assert.IsTrue(_validation.IsValidComment("Great card"));
        Assert.IsTrue(_validation.IsValidComment(new string('a', 500)));

        Assert.IsFalse(_validation.IsValidComment("Hi"));
        Assert.IsFalse(_validation.IsValidComment(new string('a', 501)));
        Assert.IsFalse(_validation.IsValidComment(null));
    }

    [TestMethod]
    public void IsValidCommentTitle_ChecksLengthLimits()
    {
        Assert.IsTrue(_validation.IsValidCommentTitle("Nice card"));

        Assert.IsFalse(_validation.IsValidCommentTitle("Hi"));
        Assert.IsFalse(_validation.IsValidCommentTitle(new string('a', 121)));
    }

    [TestMethod]
    public void IsValidRating_AndCost_CheckRanges()
    {
        Assert.IsTrue(_validation.IsValidRating(1));
        Assert.IsTrue(_validation.IsValidRating(5));
        Assert.IsFalse(_validation.IsValidRating(0));
        Assert.IsFalse(_validation.IsValidRating(6));

        Assert.IsTrue(_validation.IsValidCost(0));
        Assert.IsTrue(_validation.IsValidCost(20));
        Assert.IsFalse(_validation.IsValidCost(21));
        Assert.IsFalse(_validation.IsValidCost(-1));
    }

    [TestMethod]
    public void IsValidPower_AndHp_AllowNullAndCheckRanges()
    {
        Assert.IsTrue(_validation.IsValidPower(null));
        Assert.IsTrue(_validation.IsValidPower(0));
        Assert.IsTrue(_validation.IsValidPower(30));
        Assert.IsFalse(_validation.IsValidPower(31));
        Assert.IsFalse(_validation.IsValidPower(-1));

        Assert.IsTrue(_validation.IsValidHp(null));
        Assert.IsTrue(_validation.IsValidHp(0));
        Assert.IsTrue(_validation.IsValidHp(50));
        Assert.IsFalse(_validation.IsValidHp(51));
        Assert.IsFalse(_validation.IsValidHp(-1));
    }

    [TestMethod]
    public void IsValidSetCode_AcceptsSeedCodesOnly()
    {
        Assert.IsTrue(_validation.IsValidSetCode("IC27"));
        Assert.IsTrue(_validation.IsValidSetCode("SOR"));

        Assert.IsFalse(_validation.IsValidSetCode("sor"));
        Assert.IsFalse(_validation.IsValidSetCode("I"));
        Assert.IsFalse(_validation.IsValidSetCode(null));
    }

    [TestMethod]
    public void Sanitize_RemovesScriptBlock()
    {
        string result = _sanitizer.Sanitize("<script>alert(1)</script>");

        Assert.AreEqual(string.Empty, result);
    }

    [TestMethod]
    public void Sanitize_RemovesEventHandlerFromImage()
    {
        string result = _sanitizer.Sanitize("<img src=x onerror=alert(1)>");

        Assert.IsFalse(result.Contains("onerror", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(result.Contains("<img", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Sanitize_RemovesJavascriptUrl()
    {
        string result = _sanitizer.Sanitize("javascript:alert(1)");

        Assert.IsFalse(result.Contains("javascript", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void Sanitize_KeepsBasicFormattingTags()
    {
        string result = _sanitizer.Sanitize("<b>Bold</b> <p>Text</p>");

        Assert.AreEqual("<b>Bold</b> <p>Text</p>", result);
    }

    [TestMethod]
    public void Sanitize_StripsAttributesFromAllowedTags()
    {
        string result = _sanitizer.Sanitize("<span style=\"color:red\" onclick=\"evil()\">Hi</span>");

        Assert.AreEqual("<span>Hi</span>", result);
    }

    [TestMethod]
    public void Sanitize_RemovesScriptButKeepsSafeContent()
    {
        string result = _sanitizer.Sanitize("<p>Safe</p><script>alert(1)</script>");

        Assert.AreEqual("<p>Safe</p>", result);
    }

    [TestMethod]
    public void Sanitize_RemovesNestedScriptAndSplitJavascript()
    {
        string nestedScript = _sanitizer.Sanitize("<scr<script></script>ipt>alert(1)</scr<script></script>ipt>");
        string splitJs = _sanitizer.Sanitize("<p>javas<!--x-->cript:alert(1)</p>");

        Assert.IsFalse(nestedScript.Contains("<script", StringComparison.OrdinalIgnoreCase));
        Assert.IsFalse(splitJs.Contains("javascript:", StringComparison.OrdinalIgnoreCase));
    }

    [TestMethod]
    public void CardImageUrlDisplay_RejectsDangerousSchemes()
    {
        SwU.WebApp.Models.Card card = new() { ImageUrl = "javascript:alert(1)" };
        SwU.WebApp.Models.Card protocolRelative = new() { ImageUrl = "//evil.example/a.png" };
        SwU.WebApp.Models.Card safeLocal = new() { ImageUrl = "/images/cards/card.webp" };

        Assert.AreEqual("/images/cards/placeholder.svg", card.ImageUrlDisplay);
        Assert.AreEqual("/images/cards/placeholder.svg", protocolRelative.ImageUrlDisplay);
        Assert.AreEqual("/images/cards/card.webp", safeLocal.ImageUrlDisplay);
    }

    [TestMethod]
    public void HashPassword_MatchesAdminHashFromSeed()
    {
        string hash = AuthService.HashPassword("Admin123!");

        Assert.AreEqual("14961e846cdd166b0304b865d290fccbc2e734b9a597cd14ab1c7854de807dfb", hash);
    }

    [TestMethod]
    public void HashPassword_MatchesPlayerHashFromSeed()
    {
        string hash = AuthService.HashPassword("Player123!");

        Assert.AreEqual("1e9ca1cfbdd4f0f62a5e67a1f1cce19475e5c7d8de6fe92590f6a1440c4101ce", hash);
    }

    [TestMethod]
    public void HashPassword_IsLowercaseHexSha256()
    {
        string hash = AuthService.HashPassword("SomePassword");

        Assert.AreEqual(64, hash.Length);
        Assert.AreEqual(hash.ToLowerInvariant(), hash);
    }

    [TestMethod]
    public void HashPassword_DifferentPasswordsProduceDifferentHashes()
    {
        Assert.AreNotEqual(
            AuthService.HashPassword("Admin123!"),
            AuthService.HashPassword("Player123!"));
    }
}
