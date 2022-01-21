using System.IO;
using UnityEngine;

namespace Raindrop.Tests
{
    public class Helper
    {
        
        //easily write to a file
        //filePath = fully-specified file path
        public static void WriteToFile(byte[] outbytes, string filePath)
        {
            //create parent subfolders
            var parentDir = Path.GetDirectoryName(filePath);
            System.IO.Directory.CreateDirectory(parentDir);
                
            //write file
            System.IO.File.WriteAllBytes(filePath, outbytes);
            Debug.Log($"write: {filePath} ");
        }
    }
}