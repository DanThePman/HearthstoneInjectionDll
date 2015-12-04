using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class FileUtils
{
    public static readonly char[] FOLDER_SEPARATOR_CHARS = new char[] { '/', '\\' };

    public static string GameAssetPathToName(string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return path;
        }
        int num = path.LastIndexOf('/');
        if (num < 0)
        {
            return path;
        }
        return path.Substring(num + 1);
    }

    public static string GameToSourceAssetName(string folder, string name, string dotExtension = ".prefab")
    {
        return string.Format("{0}/{1}{2}", folder, name, dotExtension);
    }

    public static string GameToSourceAssetPath(string path, string dotExtension = ".prefab")
    {
        return string.Format("{0}{1}", path, dotExtension);
    }

    public static string GetAssetPath(string fileName)
    {
        return fileName;
    }

    public static Locale? GetForeignLocaleFromSourcePath(string path)
    {
        Locale? localeFromSourcePath = GetLocaleFromSourcePath(path);
        if (!localeFromSourcePath.HasValue)
        {
            return null;
        }
        if (((Locale) localeFromSourcePath.Value) == Locale.enUS)
        {
            return null;
        }
        return localeFromSourcePath;
    }

    public static bool GetLastFolderAndFileFromPath(string path, out string folderName, out string fileName)
    {
        folderName = null;
        fileName = null;
        if (string.IsNullOrEmpty(path))
        {
            return false;
        }
        int num = path.LastIndexOfAny(FOLDER_SEPARATOR_CHARS);
        if (num > 0)
        {
            int num2 = path.LastIndexOfAny(FOLDER_SEPARATOR_CHARS, num - 1);
            int startIndex = (num2 >= 0) ? (num2 + 1) : 0;
            int length = num - startIndex;
            folderName = path.Substring(startIndex, length);
        }
        if (num < 0)
        {
            fileName = path;
        }
        else if (num < (path.Length - 1))
        {
            fileName = path.Substring(num + 1);
        }
        return ((folderName != null) || (fileName != null));
    }

    public static Locale? GetLocaleFromSourcePath(string path)
    {
        Locale locale;
        string directoryName = System.IO.Path.GetDirectoryName(path);
        int num = directoryName.LastIndexOf("/");
        if (num < 0)
        {
            return null;
        }
        string str = directoryName.Substring(num + 1);
        try
        {
            locale = EnumUtils.Parse<Locale>(str);
        }
        catch (Exception)
        {
            return null;
        }
        return new Locale?(locale);
    }

    public static string GetMD5(string fileName)
    {
        if (!System.IO.File.Exists(fileName))
        {
            return string.Empty;
        }
        using (FileStream stream = System.IO.File.OpenRead(fileName))
        {
            MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
            return BitConverter.ToString(provider.ComputeHash(stream)).Replace("-", string.Empty);
        }
    }

    public static string GetMD5FromString(string buf)
    {
        MD5CryptoServiceProvider provider = new MD5CryptoServiceProvider();
        return BitConverter.ToString(provider.ComputeHash(Encoding.UTF8.GetBytes(buf))).Replace("-", string.Empty);
    }

    public static string GetOnDiskCapitalizationForDir(DirectoryInfo dirInfo)
    {
        DirectoryInfo parent = dirInfo.Parent;
        if (parent == null)
        {
            return dirInfo.Name;
        }
        string name = parent.GetDirectories(dirInfo.Name)[0].Name;
        return System.IO.Path.Combine(GetOnDiskCapitalizationForDir(parent), name);
    }

    public static string GetOnDiskCapitalizationForDir(string dirPath)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        return GetOnDiskCapitalizationForDir(dirInfo);
    }

    public static string GetOnDiskCapitalizationForFile(FileInfo fileInfo)
    {
        DirectoryInfo directory = fileInfo.Directory;
        string name = directory.GetFiles(fileInfo.Name)[0].Name;
        return System.IO.Path.Combine(GetOnDiskCapitalizationForDir(directory), name);
    }

    public static string GetOnDiskCapitalizationForFile(string filePath)
    {
        FileInfo fileInfo = new FileInfo(filePath);
        return GetOnDiskCapitalizationForFile(fileInfo);
    }

    public static bool IsForeignLocaleSourcePath(string path)
    {
        return GetForeignLocaleFromSourcePath(path).HasValue;
    }

    public static IntPtr LoadPlugin(string fileName, bool handleError = true)
    {
        PlatformDependentValue<string> value2 = new PlatformDependentValue<string>(PlatformCategory.OS) {
            PC = "Hearthstone_Data/Plugins/{0}",
            Mac = "Hearthstone.app/Contents/Plugins/{0}.bundle/Contents/MacOS/{0}",
            iOS = string.Empty,
            Android = string.Empty
        };
        try
        {
            string filename = string.Format((string) value2, fileName);
            IntPtr ptr = DLLUtils.LoadLibrary(filename);
            if ((ptr == IntPtr.Zero) && handleError)
            {
                string str2 = Directory.GetCurrentDirectory().Replace(@"\", "/");
                string str3 = string.Format("{0}/{1}", str2, filename);
                object[] messageArgs = new object[] { str3 };
                Error.AddDevFatal("Failed to load plugin from '{0}'", messageArgs);
                object[] objArray2 = new object[] { fileName };
                Error.AddFatalLoc("GLOBAL_ERROR_ASSET_LOAD_FAILED", objArray2);
            }
            return ptr;
        }
        catch (Exception exception)
        {
            object[] objArray3 = new object[] { exception.Message, exception.StackTrace };
            Error.AddDevFatal("FileUtils.LoadPlugin() - Exception occurred. message={0} stackTrace={1}", objArray3);
            return IntPtr.Zero;
        }
    }

    public static string MakeLocalizedPathFromSourcePath(Locale locale, string enUsPath)
    {
        string directoryName = System.IO.Path.GetDirectoryName(enUsPath);
        string fileName = System.IO.Path.GetFileName(enUsPath);
        int startIndex = directoryName.LastIndexOf("/");
        if ((startIndex >= 0) && directoryName.Substring(startIndex + 1).Equals(Localization.DEFAULT_LOCALE_NAME))
        {
            directoryName = directoryName.Remove(startIndex);
        }
        return string.Format("{0}/{1}/{2}", directoryName, locale, fileName);
    }

    public static string MakeMetaPathFromSourcePath(string path)
    {
        return string.Format("{0}.meta", path);
    }

    public static string MakeSourceAssetMetaPath(string path)
    {
        return MakeMetaPathFromSourcePath(MakeSourceAssetPath(path));
    }

    public static string MakeSourceAssetPath(DirectoryInfo folder)
    {
        return MakeSourceAssetPath(folder.FullName);
    }

    public static string MakeSourceAssetPath(FileInfo fileInfo)
    {
        return MakeSourceAssetPath(fileInfo.FullName);
    }

    public static string MakeSourceAssetPath(string path)
    {
        string str = path.Replace(@"\", "/");
        int index = str.IndexOf("/Assets", StringComparison.OrdinalIgnoreCase);
        return str.Remove(0, index + 1);
    }

    public static bool SetFileWritableFlag(string path, bool setWritable)
    {
        if (System.IO.File.Exists(path))
        {
            try
            {
                FileAttributes attributes = System.IO.File.GetAttributes(path);
                FileAttributes fileAttributes = !setWritable ? (attributes | FileAttributes.ReadOnly) : (attributes & ~FileAttributes.ReadOnly);
                if (setWritable && (Environment.OSVersion.Platform == PlatformID.MacOSX))
                {
                    fileAttributes |= FileAttributes.Normal;
                }
                if (fileAttributes != attributes)
                {
                    System.IO.File.SetAttributes(path, fileAttributes);
                    if (System.IO.File.GetAttributes(path) != fileAttributes)
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (DirectoryNotFoundException)
            {
            }
            catch (FileNotFoundException)
            {
            }
            catch (Exception)
            {
            }
        }
        return false;
    }

    public static bool SetFolderWritableFlag(string dirPath, bool writable)
    {
        foreach (string str in Directory.GetFiles(dirPath))
        {
            SetFileWritableFlag(str, writable);
        }
        foreach (string str2 in Directory.GetDirectories(dirPath))
        {
            SetFolderWritableFlag(str2, writable);
        }
        return true;
    }

    public static string SourceToGameAssetName(string path)
    {
        int num = path.LastIndexOf('/');
        if (num < 0)
        {
            return path;
        }
        int length = path.LastIndexOf('.');
        if (length < 0)
        {
            return path;
        }
        return path.Substring(num + 1, length);
    }

    public static string SourceToGameAssetPath(string path)
    {
        int length = path.LastIndexOf('.');
        if (length < 0)
        {
            return path;
        }
        return path.Substring(0, length);
    }

    public static string StripLocaleFromPath(string path)
    {
        string directoryName = System.IO.Path.GetDirectoryName(path);
        string fileName = System.IO.Path.GetFileName(path);
        if (Localization.IsValidLocaleName(System.IO.Path.GetFileName(directoryName)))
        {
            return string.Format("{0}/{1}", System.IO.Path.GetDirectoryName(directoryName), fileName);
        }
        return path;
    }

    public static string BasePersistentDataPath
    {
        get
        {
            string str = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData).Replace('\\', '/');
            return string.Format("{0}/Blizzard/Hearthstone", str);
        }
    }

    public static string CachePath
    {
        get
        {
            string path = string.Format("{0}/Cache", PersistentDataPath);
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception exception)
                {
                    Debug.LogError(string.Format("FileUtils.CachePath - Error creating {0}. Exception={1}", path, exception.Message));
                }
            }
            return path;
        }
    }

    public static string InternalPersistentDataPath
    {
        get
        {
            return string.Format("{0}/Dev", BasePersistentDataPath);
        }
    }

    public static string PersistentDataPath
    {
        get
        {
            string path = null;
            if (ApplicationMgr.IsInternal())
            {
                path = InternalPersistentDataPath;
            }
            else
            {
                path = PublicPersistentDataPath;
            }
            if (!Directory.Exists(path))
            {
                try
                {
                    Directory.CreateDirectory(path);
                }
                catch (Exception exception)
                {
                    Debug.LogError(string.Format("FileUtils.PersistentDataPath - Error creating {0}. Exception={1}", path, exception.Message));
                    Error.AddFatalLoc("GLOBAL_ERROR_ASSET_CREATE_PERSISTENT_DATA_PATH", new object[0]);
                }
            }
            return path;
        }
    }

    public static string PublicPersistentDataPath
    {
        get
        {
            return BasePersistentDataPath;
        }
    }
}

