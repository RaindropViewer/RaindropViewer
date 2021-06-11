

using System;
/**
* Raindrop Metaverse Client
* Copyright(c) 2009-2014, Raindrop Development Team
* Copyright(c) 2016-2020, Sjofn, LLC
* All rights reserved.
*  
* Raindrop is free software: you can redistribute it and/or modify
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
namespace Catnip.Drawing
{
    public struct Color
    {
        //UnityEngine.Color color;

        //
        // Summary:
        //     Red component of the color.
        public int r;
        //
        // Summary:
        //     Green component of the color.
        public float g;
        //
        // Summary:
        //     Blue component of the color.
        public float b;
        //
        // Summary:
        //     Alpha component of the color (0 is transparent, 1 is opaque).
        public float a;



        public static Color FromArgb(int a, int r, int g, int b)
        {
            var cl = new Color
            {
                a = a,
                r = r,
                g = g,
                b = b
            };

            return cl;

            //throw new NotImplementedException();
        }

        //convert catnip.color -> ue.color
        public static implicit operator UnityEngine.Color(Color v)
        {
            return new UnityEngine.Color(v.r,v.g,v.b,v.a);

            //throw new NotImplementedException();
        }
    }
}