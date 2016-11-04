using System;
using System.IO;

namespace HearthstoneInjectionDll
{
    /// <summary>
    ///     private
    /// </summary>
    public class Debug
    {
        public static void AppendDesktopLog(string destFileName, string content = "", bool append = true)
        {
            using (var sw = new StreamWriter(@"C:\Users\" + Environment.UserName + @"\Desktop\" +
                                             destFileName + ".txt", append))
            {
                sw.WriteLine(content);
                sw.Close();
            }
        }

        public static void ShowIngameMessage(string information)
        {
            DialogManager.Get().ShowMessageOfTheDay(information);
        }
    }
}