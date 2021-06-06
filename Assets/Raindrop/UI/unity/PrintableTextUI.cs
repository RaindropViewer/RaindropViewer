

using SixLabors.ImageSharp;
using System;
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
namespace Raindrop
{
    public class PrintableTextUI : UnityEngine.MonoBehaviour, IPrintableMonobehavior
    {
        public string Text => throw new NotImplementedException();

        string IPrintableMonobehavior.Content { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Color IPrintableMonobehavior.ForeColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        Color IPrintableMonobehavior.BackColor { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        string IPrintableMonobehavior.Text { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void AppendText(string v)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        internal void InsertLink(string text)
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.ClearText()
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.InsertLink(string text)
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.InsertLink(string text, string hyperlink)
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.PrintText(string text)
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.PrintTextLine(string text)
        {
            throw new NotImplementedException();
        }

        void IPrintableMonobehavior.PrintTextLine(string text, Color color)
        {
            throw new NotImplementedException();
        }
    }
}