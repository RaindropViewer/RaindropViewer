//using System;
//using System.Collections.Generic;
//using System.IO;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;
//using SixLabors.ImageSharp;
//using UnityEngine; 


////fake image class that actually just calls unity image internally. //modified 5/6/2021
//namespace Catnip.Drawing
//{
//    public abstract class Image : MarshalByRefObject, IDisposable, ICloneable, ISerializable
//    {

//        private Texture2D tex;
//        public abstract int Width { get; internal set; }
//        public abstract int Height { get; internal set; }

//        public static Image FromFile(string fname)
//        {
//            return FromFile(filename, false);
//        }

//        public static Image FromFile(string filename, bool useEmbeddedColorManagement)
//        {
//            if (!File.Exists(filename))
//                throw new FileNotFoundException(filename);

//            var fileData = File.ReadAllBytes(filename);
//            //var tex = new Texture2D(2, 2);
//            //tex.LoadImage(fileData);

//            var image = SixLabors.ImageSharp.Image.Load(filename);

//            return image;
//        }

//        public object Clone()
//        {
//            throw new NotImplementedException();
//        }

//        public void Dispose()
//        {
//            throw new NotImplementedException();
//        }

//        public void GetObjectData(SerializationInfo info, StreamingContext context)
//        {
//            throw new NotImplementedException();
//        }
         
//    }
//}
