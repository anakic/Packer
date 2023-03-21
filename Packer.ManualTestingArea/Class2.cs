using Packer2.Library;
using Packer2.Library.DataModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Packer.ManualTestingArea
{
    public class Class2
    {
        public static void Main()
        {
            // Read - TabularModel.\sadf | Write - TabularModel.\with_root_ignores2 - Customization wlt1

            FolderDatabaseStore store = new FolderDatabaseStore(@"C:\Users\AntonioNakic_lbehpwm\OneDrive - RealWorld.Health\Desktop\Data Model");
            var model = store.Read();

            FolderDatabaseStore store2 = new FolderDatabaseStore("C:\\models\\test234\\with_cust", "WLT");
            store2.Save(model);
        }
    }
}
