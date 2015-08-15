using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace tests.helpers
{
    class TestPaths
    {
        public static string TestFolder
        {
            get
            {
                if (_testfolderpath == null)
                {
                    string dir = //System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().CodeBase.Substring("file:///".Length));
                        AppDomain.CurrentDomain.BaseDirectory;
                    int idx = dir.IndexOf("bin\\Debug");
                    if (idx < 0) idx = dir.IndexOf("bin\\Release");                    
                    _testfolderpath = (idx > 0) ? dir.Substring(0, idx) : dir;
                }
                return _testfolderpath;
            }
        }

        internal static string DataFolder
        {
            get
            {
                if (_testdatafolderpath == null)
                {
                    _testdatafolderpath = Path.Combine(TestFolder, "data");
                }
                return _testdatafolderpath;
            }
        }

        private static string _testfolderpath = null;
        private static string _testdatafolderpath = null;
    }
}
