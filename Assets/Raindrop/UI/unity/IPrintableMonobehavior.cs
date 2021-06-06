using SixLabors.ImageSharp;

namespace Raindrop
{
    public interface IPrintableMonobehavior
    {
        void InsertLink(string text);
        void InsertLink(string text, string hyperlink);
        void PrintText(string text);
        void PrintTextLine(string text);
        void PrintTextLine(string text, Color color);
        void ClearText();

        string Content { get; set; }
        Color ForeColor { get; set; }
        Color BackColor { get; set; }
        string Text { get; set; }

        void AppendText(string v);
        void Clear();
    }
}