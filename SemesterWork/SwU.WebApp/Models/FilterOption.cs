namespace SwU.WebApp.Models;

// Пункт выпадающего списка с заранее вычисленным состоянием selected
public class FilterOption
{
    public string Value { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public bool Selected { get; set; }

    public FilterOption()
    {
    }

    public FilterOption(string value, string text, bool selected)
    {
        Value = value;
        Text = text;
        Selected = selected;
    }
}
