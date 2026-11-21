using SwU.HttpFramework.Templating;

namespace SwU.Tests;

[TestClass]
public class TemplateEngineTests
{
    private readonly SimpleTemplateEngine _engine = new("Views");

    [TestMethod]
    public void Render_SimpleAndNestedVariables_ReplacesValues()
    {
        var model = new
        {
            Name = "Luke Skywalker",
            Card = new { Name = "Darth Vader" }
        };

        string result = _engine.Render("{{Name}} / {{Card.Name}}", model);

        Assert.AreEqual("Luke Skywalker / Darth Vader", result);
    }

    [TestMethod]
    public void Render_EncodedVariable_HtmlEncodesScriptTags()
    {
        string result = _engine.Render("{{Text}}", new { Text = "<script>alert(1)</script>" });

        StringAssert.Contains(result, "&lt;script&gt;");
        StringAssert.Contains(result, "&lt;/script&gt;");
        Assert.IsFalse(result.Contains("<script>", StringComparison.Ordinal));
    }

    [TestMethod]
    public void Render_RawVariable_DoesNotEncodeHtml()
    {
        string result = _engine.Render("{{{Raw}}}", new { Raw = "<b>Bold</b>" });

        Assert.AreEqual("<b>Bold</b>", result);
    }

    [TestMethod]
    public void Render_ValueThatLooksLikeTemplate_IsNotRenderedTwice()
    {
        Dictionary<string, object?> model = new()
        {
            ["Name"] = "{{Other}}",
            ["Other"] = "hacked"
        };

        string result = _engine.Render("{{Name}}", model);

        Assert.AreEqual("{{Other}}", result);
    }

    [TestMethod]
    public void Render_RawValueThatLooksLikeTemplate_IsNotRenderedTwice()
    {
        Dictionary<string, object?> model = new()
        {
            ["Raw"] = "{{#if Other}}yes{{/if}}",
            ["Other"] = true
        };

        string result = _engine.Render("{{{Raw}}}", model);

        Assert.AreEqual("{{#if Other}}yes{{/if}}", result);
    }

    [TestMethod]
    public void Render_IfBlock_ChoosesCorrectBranch()
    {
        Assert.AreEqual("Yes", _engine.Render("{{#if Show}}Yes{{/if}}", new { Show = true }));
        Assert.AreEqual(string.Empty, _engine.Render("{{#if Show}}Yes{{/if}}", new { Show = false }));
        Assert.AreEqual("No", _engine.Render("{{#if Show}}Yes{{else}}No{{/if}}", new { Show = false }));
        Assert.AreEqual("Yes", _engine.Render("{{#if Show}}Yes{{else}}No{{/if}}", new { Show = true }));
    }

    [TestMethod]
    public void Render_EachBlock_RepeatsTemplateForEveryItem()
    {
        var model = new
        {
            Items = new[]
            {
                new { Name = "Luke" },
                new { Name = "Leia" }
            }
        };

        string result = _engine.Render("<ul>{{#each Items}}<li>{{Name}}</li>{{/each}}</ul>", model);

        Assert.AreEqual("<ul><li>Luke</li><li>Leia</li></ul>", result);
    }

    [TestMethod]
    public void Render_EachBlockWithEmptyCollection_ProducesNothing()
    {
        var model = new { Items = Array.Empty<object>() };

        string result = _engine.Render("<ul>{{#each Items}}<li>{{Name}}</li>{{/each}}</ul>", model);

        Assert.AreEqual("<ul></ul>", result);
    }

    [TestMethod]
    public void Render_MissingOrNullValues_ProduceEmptyStringWithoutException()
    {
        Dictionary<string, object?> model = new()
        {
            ["NullValue"] = null
        };

        string result = _engine.Render("[{{Missing}}][{{NullValue}}]", model);

        Assert.AreEqual("[][]", result);
    }

    [TestMethod]
    public void Render_IfWithMissingProperty_ProducesEmptyString()
    {
        string result = _engine.Render("[{{#if Missing}}x{{/if}}]", new { });

        Assert.AreEqual("[]", result);
    }

    [TestMethod]
    public void Render_NullModel_ProducesEmptyVariable()
    {
        string result = _engine.Render("[{{Name}}]", null);

        Assert.AreEqual("[]", result);
    }
}
