using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Raindrop.Disk
{
    //contains functions that directly write to disk.
    static class ReadWrite
    {
        private static string filenameCredentials = "/SaveCredentials.data";

        public static bool saveCredentials(Raindrop.Types.Credential cred, String path)
        {
            var dirPath = path + filenameCredentials;

            if (dirPath != null)
                SaveSerialisable(ref cred, dirPath);
                //File.WriteAllText(dirPath, User + Pass);

            return true; //success
        }

        //writes the obj/data to the filepath.
        public static void SaveSerialisable<T>(ref T data , String filePath)
        {
            FileStream dataStream = new FileStream(filePath, FileMode.Create);

            BinaryFormatter converter = new BinaryFormatter();
            converter.Serialize(dataStream, data);

            dataStream.Close();
        }

        public static List<Raindrop.Types.Credential> ReadCredentials(String path)
        {
            String dirPath = path + "/SaveCredentials.data";

            if (File.Exists(dirPath))
            {
                // File exists  

                return (List<Raindrop.Types.Credential>)ReadSerialisable(dirPath);
            }
            else
            {
                // File does not exist
                return new List<Raindrop.Types.Credential>();
            }


        }
        //reads filepath, returns the object of specified type
        public static object ReadSerialisable(String filePath)
        {
            FileStream dataStream = new FileStream(filePath, FileMode.Open);

            BinaryFormatter converter = new BinaryFormatter();
            object cred = converter.Deserialize(dataStream); //the metadata of binaryformatter already knows the type of the data on disk.

            dataStream.Close();

            //T emps = (T)cred;
            //fs.Flush();
            //fs.Close();
            //fs.Dispose();

            //return (T) Convert.ChangeType(cred,typeof(T));

            return cred;
        }




    }
}
