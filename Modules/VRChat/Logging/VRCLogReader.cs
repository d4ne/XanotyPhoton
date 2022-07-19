using System;
using System.IO;

namespace XanotyPhoton
{
    public class VRCLogReader
    {
        public static string FetchRoom()
        {
            var directory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData) + @"Low\VRChat\VRChat");
            
            if (directory != null && directory.Exists)
            {
                FileInfo target = null;

                foreach (var info in directory.GetFiles("output_log_*.txt", SearchOption.TopDirectoryOnly))
                {
                    if (target == null || info.LastAccessTime.CompareTo(target.LastAccessTime) >= 0)
                    {
                        target = info;
                    }
                }

                if (target != null)
                {
                    var fs = new FileStream(target.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                    
                    using (var sr = new StreamReader(fs))
                    {
                        string line;

                        while ((line = sr.ReadLine()) != null)
                        {
                            if (line != null)
                            {
                                if (line.Contains(" Joining w"))
                                {
                                    string[] arr = line.Split(new[] { " Joining " }, StringSplitOptions.None);
                                    return arr[1];
                                }
                            }
                        }
                    }
                }
            }

            return null;
        }
    }
}
