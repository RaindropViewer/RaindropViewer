using System;
using System.Text.RegularExpressions;
using OpenMetaverse;
using TMPro;
using UnityEngine;

namespace Raindrop
{
    //this class allows you to print into a TMP Text
    //consider refactoring ITextPrinter into an adaptor class.
    public class TMPTextFieldPrinter: MonoBehaviour, ITextPrinter
    {
        private TMPro.TMP_Text rtb;
        private static readonly string urlRegexString = @"(https?://[^ \r\n]+)|(\[secondlife://[^ \]\r\n]* ?(?:[^\]\r\n]*)])|(secondlife://[^ \r\n]*)";
        Regex urlRegex;
        private SlUriParser uriParser;

        private void Awake()
        {
            rtb = this.GetComponent<TMP_Text>();
            if (!rtb)
            {
                OpenMetaverse.Logger.Log(
                    "you added the TMPTEXTBOXprinter to an gameobject without a tmptextbox!!!",
                    Helpers.LogLevel.Error);
            } 

            uriParser = new SlUriParser();
            urlRegex = new Regex(urlRegexString, RegexOptions.Compiled | RegexOptions.IgnoreCase);

        }

        

        public void InsertLink(string text, string hyperlink)
        {
            rtb.text = rtb.text + "<link=\"" + hyperlink + "\"> <color=blue>" + text + "</color></link>";
            //rtb.InsertLink(text, hyperlink);
        }

        private void FindURLs(string text)
        {
            string[] lineParts = urlRegex.Split(text);
            int linePartIndex;

            // 'text' will be split into 1 + NumLinks*2 parts...
            // If 'text' has no links in it:
            //    lineParts[0] = text
            // If 'text' has one link in it:
            //    lineParts[0] = <Text before first link>
            //    lineParts[1] = <first link>
            //    lineParts[2] = <text after first link>
            // If 'text' has two links in it:
            //    lineParts[0] = <Text before first link>
            //    lineParts[1] = <first link>
            //    lineParts[2] = <text after first link>
            //    lineParts[3] = <second link>
            //    lineParts[4] = <text after second link>
            // ...
            for (linePartIndex = 0; linePartIndex < lineParts.Length - 1; linePartIndex += 2)
            {
                AppendText(lineParts[linePartIndex]);
                //Color c = ForeColor;
                InsertLink(uriParser.GetLinkName(lineParts[linePartIndex + 1]), lineParts[linePartIndex + 1]);
                //ForeColor = c;
            }
            if (linePartIndex != lineParts.Length)
            {
                AppendText(lineParts[linePartIndex]);
            }
        }

        private void AppendText(string v)
        {
            rtb.text = rtb.text + v;
            //throw new NotImplementedException();
        }

        #region ITextPrinter Members

        public void PrintText(string text, Color color)
        {
            PrintText(text, 
                (byte) (color.r * 255),
                (byte) (color.g * 255),
                (byte) (color.b * 255));
        }

        // public void PrintText(string text, TextColor color)
        // {
        //     switch (color)
        //     {
        //         case TextColor.black :
        //             PrintText(text, (byte) 0, (byte) 0, (byte) 0);
        //             break;
        //
        //         case TextColor.blue :
        //             PrintText(text, (byte) 0, (byte) 0, (byte) 255);
        //             break;
        //
        //         case TextColor.green :
        //             PrintText(text, (byte) 0, (byte) 255, (byte) 0);
        //             break;
        //
        //         case TextColor.orange :
        //             PrintText(text, (byte) 255, (byte) 127, (byte) 20);
        //             break;
        //         //
        //         // case TextColor.purple :
        //         //     PrintText(text, (byte) 0, (byte) 0, (byte) 0);
        //         //     break;
        //
        //         case TextColor.red :
        //             PrintText(text, (byte) 255, (byte) 0, (byte) 0);
        //             break;
        //
        //         case TextColor.white :
        //             PrintText(text, (byte) 255, (byte) 255, (byte) 255);
        //             break;
        //         //
        //         // case TextColor.yellow :
        //         //     PrintText(text, (byte) 0, (byte) 0, (byte) 0);
        //         //     break;
        //
        //     }
        //     
        //     
        //
        //     //else
        //     //{
        //         // FindURLs(text);
        //     //}
        // }
        
        public void PrintText(string text, byte r, byte g, byte b)
        {
            string hex_r = BitConverter.ToString(new[]{r});
            string hex_g = BitConverter.ToString(new[]{g});
            string hex_b = BitConverter.ToString(new[]{b});
            
            rtb.text = rtb.text 
                       + "<color=" + hex_r + hex_g + hex_r + ">" + text;

            //else
            //{
            // FindURLs(text);
            //}
        }

        public void PrintTextLine(string text)
        {
            PrintText(text + Environment.NewLine,
                Color.black);
        }

        public void PrintTextLine(string text, Color color)
        {
            //if (rtb.InvokeRequired)
            //{
            //    rtb.Invoke(new MethodInvoker(() => PrintTextLine(text, color)));
            //    return;
            //}
             
            PrintTextLine(text); 
        }

        public void ClearText()
        {
            rtb.text = "";
        }

        public string Content
        {
            get => rtb.text;
            set => rtb.text = value;
        }

        #endregion
    }
}
