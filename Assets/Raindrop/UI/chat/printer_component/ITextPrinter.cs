using UnityEngine;

namespace Raindrop
{
    // classes implement this interface to print content to screen.
    public interface ITextPrinter
    {
        void InsertLink(string text, string hyperlink);
        void PrintText(string text, Color color);
        void PrintTextLine(string text);
        void PrintTextLine(string text, Color color);
        void ClearText();

        string Content { get; set; }
        
    }
}
