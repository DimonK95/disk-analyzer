using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security;
using System.Security.Cryptography;

namespace Analizatior
{
    class EntityFile
    {
        public string FilePath, Hash;
        public int Size;
        public DateTime ModificationDate;
        
        public EntityFile(string path)
        {
            FilePath = path;
            byte[] fileBytes = File.ReadAllBytes(FilePath);

            MD5CryptoServiceProvider md5provider = new MD5CryptoServiceProvider();
            Size = fileBytes.Length;
            ModificationDate = File.GetLastWriteTime(FilePath);
            Hash = BitConverter.ToString(md5provider.ComputeHash(fileBytes));
        } 
    }
}
