//using Packer.Model;
//using System;
//using System.Collections.Generic;
//using System.IO.Compression;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml.Linq;

//namespace Packer.TMP
//{
//    public class ArchiveModelStore : IModelStore<RepositoryModel>, IDisposable
//    {
//        private readonly string pbiFilePath;
//        private readonly string tempFolderPath;


//        public ArchiveModelStore(string pbiFilePath)
//        {
//            this.pbiFilePath = pbiFilePath;
//            tempFolderPath  = Path.Combine(Path.GetTempPath(), "Packer");
            
//            Directory.Delete(tempFolderPath, true);
//        }

//        public void Dispose()
//        {
//            Directory.Delete(tempFolderPath, true);
//        }

//        public RepositoryModel Read()
//        {
//            ZipFile.ExtractToDirectory(pbiFilePath, tempFolderPath);

//        }

//        public void Save(RepositoryModel model)
//        {
//            throw new NotImplementedException();
//        }
//    }
//}
