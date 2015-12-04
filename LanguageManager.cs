using System;
using System.IO;
using System.Reflection;

namespace HearthstoneInjectionDll
{
    internal class LanguageManager
    {
        private static string GetGameLanguageString()
        {
            var dirsStrings = Directory.GetDirectories(
                Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86) + @"\Hearthstone\Strings");
            var languageString = dirsStrings[0];

            return languageString.Substring((languageString.LastIndexOf("\\", StringComparison.Ordinal) + 1),
                languageString.Length - (languageString.LastIndexOf("\\", StringComparison.Ordinal) + 1));
        }

        /// <summary>
        ///     return translation file of all cards depending on string language
        /// </summary>
        /// <returns></returns>
        public static string GetLanguageJsonContent()
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "HearthstoneInjectionDll.LangFiles.AllSets." + GetGameLanguageString() + ".json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
                // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }

        public static string GetLanguageJsonContent(string languageString)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = "HearthstoneInjectionDll.LangFiles.AllSets." + languageString + ".json";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
                // ReSharper disable once AssignNullToNotNullAttribute
            using (var reader = new StreamReader(stream))
            {
                return reader.ReadToEnd();
            }
        }
    }
}