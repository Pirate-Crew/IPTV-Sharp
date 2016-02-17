using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace IPTV_Sharp
{
    public class DictionaryManager
    {
        private string folder_path;
        private List<string> _entries;

        public List<string> entries
        {
            get
            {
                return _entries;
            }
        }

        public DictionaryManager(string folder_path)
        {
            this.folder_path = folder_path;
            _entries = new List<string>();
        }

        public bool LoadDictionaries()
        {
            bool success = false;

            if (Directory.Exists(folder_path))
            {
                string[] files = Directory.GetFiles(folder_path);
                foreach(string file in files)
                {
                    StreamReader reader = new StreamReader(file);
                    string line = string.Empty;
                    while((line=reader.ReadLine())!=null)
                    {
                        _entries.Add(line);
                    }
                }
            }               
                              
            return success;
        }
    }
}
