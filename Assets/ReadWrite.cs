using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace Disk
{
    class ReadWrite
    {
        public bool saveCredentials(Raindrop.Types.Credential cred, String path)
        {
            var dirPath = path + "/SaveCredentials.data";

            if (dirPath != null)
                SaveSerialisable(ref cred, dirPath);
                //File.WriteAllText(dirPath, User + Pass);

            return true; //success
        }

        //writes the obj/data to the filepath.
        public void SaveSerialisable<T>(ref T data , String filePath)
        {
            FileStream dataStream = new FileStream(filePath, FileMode.Create);

            BinaryFormatter converter = new BinaryFormatter();
            converter.Serialize(dataStream, data);

            dataStream.Close();
        }

        public List<Raindrop.Types.Credential> readCredentials(String path)
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
        public object ReadSerialisable(String filePath)
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
