using System;
using System.IO;

namespace HearthstoneInjectionDll
{
    /// <summary>
    ///     private
    /// </summary>
    public class Debug
    {
        public static void AppendDesktopLog(string destFileName, string content)
        {
            using (var sw = new StreamWriter(@"C:\Users\" + Environment.UserName + @"\Desktop\" +
                                             destFileName + ".txt", true))
            {
                sw.WriteLine(content);
                sw.Close();
            }
        }
    }
}