using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using UnityEngine;

public class DownloadManifest
{
    private HashSet<string> m_fileSet = new HashSet<string>();
    private Map<string, string> m_hashNames = new Map<string, string>();
    private const string MANIFEST_DIVIDER = "<END OF HASHES>";
    private static DownloadManifest s_downloadManifest;
    private static string s_downloadManifestFilePath = FileUtils.GetAssetPath("manifest-downloads.csv");
    private static List<string> s_filesToWrite;
    private static Map<string, string> s_hashesToWrite;

    private DownloadManifest()
    {
    }

    public static void AddFileToWrite(string filePath)
    {
        if (s_filesToWrite == null)
        {
            s_filesToWrite = new List<string>();
        }
        List<string> list = s_filesToWrite;
        lock (list)
        {
            if (!s_filesToWrite.Contains(filePath))
            {
                s_filesToWrite.Add(filePath);
            }
        }
    }

    public static void AddHashNameForBundle(string name, string hash)
    {
        if (s_hashesToWrite == null)
        {
            s_hashesToWrite = new Map<string, string>();
        }
        Map<string, string> map = s_hashesToWrite;
        Monitor.Enter(map);
        try
        {
            s_hashesToWrite.Add(name, hash);
        }
        catch (ArgumentException exception)
        {
            Debug.LogError(string.Format("Exception adding key {0} with value {1} to hashesToWrite dict. Exception: {2}", name, hash, exception.Message));
        }
        finally
        {
            Monitor.Exit(map);
        }
    }

    public static void ClearDataToWrite()
    {
        if (s_filesToWrite != null)
        {
            s_filesToWrite.Clear();
        }
        if (s_hashesToWrite != null)
        {
            s_hashesToWrite.Clear();
        }
    }

    public bool ContainsFile(string filePath)
    {
        return this.m_fileSet.Contains(filePath);
    }

    public string DownloadableBundleFileName(string bundleName)
    {
        if (this.HashForBundle(bundleName) == null)
        {
            return null;
        }
        return bundleName;
    }

    public static DownloadManifest Get()
    {
        if (s_downloadManifest == null)
        {
            s_downloadManifest = new DownloadManifest();
            s_downloadManifest.Load();
        }
        return s_downloadManifest;
    }

    public string HashForBundle(string bundleName)
    {
        string str;
        this.m_hashNames.TryGetValue(bundleName, out str);
        return str;
    }

    private void Load()
    {
        string path = s_downloadManifestFilePath;
        int num = 0;
        try
        {
            using (StreamReader reader = new StreamReader(path))
            {
                string str2;
                bool flag = true;
                while ((str2 = reader.ReadLine()) != null)
                {
                    num++;
                    if (!string.IsNullOrEmpty(str2))
                    {
                        if (flag)
                        {
                            if (str2.Equals("<END OF HASHES>"))
                            {
                                flag = false;
                            }
                            else
                            {
                                this.ParseAndAddHashName(str2);
                            }
                        }
                        else
                        {
                            this.m_fileSet.Add(str2);
                        }
                    }
                }
            }
        }
        catch (FileNotFoundException exception)
        {
            Error.AddDevFatal(string.Format("Failed to find download manifest at '{0}': {1}", path, exception.Message), new object[0]);
        }
        catch (IOException exception2)
        {
            Error.AddDevFatal(string.Format("Failed to read download manifest at '{0}': {1}", path, exception2.Message), new object[0]);
        }
        catch (NullReferenceException exception3)
        {
            Error.AddDevFatal(string.Format("Failed to read from download manifest '{0}' line {1}: {2}", path, num, exception3.Message), new object[0]);
        }
        catch (Exception exception4)
        {
            Error.AddDevFatal(string.Format("An unknown error occurred loading download manifest '{0}' line {1}: {2}", path, num, exception4.Message), new object[0]);
        }
    }

    private bool ParseAndAddHashName(string line)
    {
        char[] separator = new char[] { ';' };
        string[] strArray = line.Split(separator);
        if (strArray.Length != 2)
        {
            return false;
        }
        string str = strArray[0];
        string key = strArray[1];
        this.m_hashNames.Add(key, str);
        return true;
    }

    public static void WriteToFile(string path)
    {
        if (s_filesToWrite != null)
        {
            string directoryName = System.IO.Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(directoryName) && !Directory.Exists(directoryName))
            {
                Directory.CreateDirectory(directoryName);
            }
            using (StreamWriter writer = new StreamWriter(path))
            {
                if (s_hashesToWrite != null)
                {
                    Map<string, string> map = s_hashesToWrite;
                    lock (map)
                    {
                        foreach (KeyValuePair<string, string> pair in s_hashesToWrite)
                        {
                            writer.Write(pair.Value);
                            writer.Write(";");
                            writer.WriteLine(pair.Key);
                        }
                    }
                }
                writer.WriteLine("<END OF HASHES>");
                List<string> list = s_filesToWrite;
                lock (list)
                {
                    foreach (string str2 in s_filesToWrite)
                    {
                        writer.WriteLine(str2);
                    }
                }
            }
        }
    }
}

