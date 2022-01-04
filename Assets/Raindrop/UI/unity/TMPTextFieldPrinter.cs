/**
 * Radegast Metaverse Client
 * Copyright(c) 2009-2014, Radegast Development Team
 * Copyright(c) 2016-2020, Sjofn, LLC
 * All rights reserved.
 *  
 * Radegast is free software: you can redistribute it and/or modify
 * it under the terms of the GNU Lesser General Public License as published
 * by the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU Lesser General Public License
 * along with this program.If not, see<https://www.gnu.org/licenses/>.
 */

using System;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;

namespace Raindrop
{
    //this class wraps a TMPtextbox and implements Itextprinter interface, so that you can print into the TMOPtextbox.
    //consider refactoring ITextPrinter into an adaptor class.
    public class TMPTextFieldPrinter:MonoBehaviour, ITextPrinter
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
                Debug.LogError("you added the TMPTEXTBOXprinter to an gameobject without a tmptextbox!!!");
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

        public void PrintText(string text)
        {
            rtb.text = rtb.text + text;

            //else
            //{
                FindURLs(text);
            //}
        }

        public void PrintTextLine(string text)
        {
            PrintText(text + Environment.NewLine);
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
