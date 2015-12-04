using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Blizzard
{
    public class Crypto
    {
        public class SHA1
        {
            public const int Length = 40;
            public const string Null = "0000000000000000000000000000000000000000";

            public static string Calc(byte[] bytes)
            {
                return Calc(bytes, 0, bytes.Length);
            }

            public static string Calc(FileInfo path)
            {
                return Calc(System.IO.File.ReadAllBytes(path.FullName));
            }

            public static string Calc(string message)
            {
                byte[] dst = new byte[message.Length * 2];
                Buffer.BlockCopy(message.ToCharArray(), 0, dst, 0, dst.Length);
                return Calc(dst);
            }

            [DebuggerHidden]
            public static IEnumerator Calc(byte[] bytes, int inputCount, Action<string> hash)
            {
                return new <Calc>c__Iterator1F5 { bytes = bytes, inputCount = inputCount, hash = hash, <$>bytes = bytes, <$>inputCount = inputCount, <$>hash = hash };
            }

            public static string Calc(byte[] bytes, int start, int count)
            {
                byte[] buffer = System.Security.Cryptography.SHA1.Create().ComputeHash(bytes, start, count);
                StringBuilder builder = new StringBuilder();
                foreach (byte num in buffer)
                {
                    builder.Append(num.ToString("x2"));
                }
                return builder.ToString();
            }

            [CompilerGenerated]
            private sealed class <Calc>c__Iterator1F5 : IDisposable, IEnumerator, IEnumerator<object>
            {
                internal object $current;
                internal int $PC;
                internal byte[] <$>bytes;
                internal Action<string> <$>hash;
                internal int <$>inputCount;
                internal byte[] <$s_1161>__4;
                internal int <$s_1162>__5;
                internal byte <b>__6;
                internal byte[] <hashBytes>__3;
                internal System.Security.Cryptography.SHA1 <hasher>__0;
                internal int <offset>__1;
                internal StringBuilder <sb>__2;
                internal byte[] bytes;
                internal Action<string> hash;
                internal int inputCount;

                [DebuggerHidden]
                public void Dispose()
                {
                    this.$PC = -1;
                }

                public bool MoveNext()
                {
                    uint num = (uint) this.$PC;
                    this.$PC = -1;
                    switch (num)
                    {
                        case 0:
                            this.<hasher>__0 = System.Security.Cryptography.SHA1.Create();
                            this.<offset>__1 = 0;
                            break;

                        case 1:
                            break;

                        default:
                            goto Label_0168;
                    }
                    if ((this.bytes.Length - this.<offset>__1) >= this.inputCount)
                    {
                        this.<offset>__1 += this.<hasher>__0.TransformBlock(this.bytes, this.<offset>__1, this.inputCount, this.bytes, this.<offset>__1);
                        this.$current = null;
                        this.$PC = 1;
                        return true;
                    }
                    this.<hasher>__0.TransformFinalBlock(this.bytes, this.<offset>__1, this.bytes.Length - this.<offset>__1);
                    this.<sb>__2 = new StringBuilder();
                    this.<hashBytes>__3 = this.<hasher>__0.Hash;
                    this.<$s_1161>__4 = this.<hashBytes>__3;
                    this.<$s_1162>__5 = 0;
                    while (this.<$s_1162>__5 < this.<$s_1161>__4.Length)
                    {
                        this.<b>__6 = this.<$s_1161>__4[this.<$s_1162>__5];
                        this.<sb>__2.Append(this.<b>__6.ToString("x2"));
                        this.<$s_1162>__5++;
                    }
                    this.hash(this.<sb>__2.ToString());
                    goto Label_0168;
                    this.$PC = -1;
                Label_0168:
                    return false;
                }

                [DebuggerHidden]
                public void Reset()
                {
                    throw new NotSupportedException();
                }

                object IEnumerator<object>.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.$current;
                    }
                }

                object IEnumerator.Current
                {
                    [DebuggerHidden]
                    get
                    {
                        return this.$current;
                    }
                }
            }
        }
    }

    public class File
    {
        public static bool createFolder(string folder)
        {
            if (!string.IsNullOrEmpty(folder))
            {
                try
                {
                    Directory.CreateDirectory(folder);
                    return true;
                }
                catch (UnauthorizedAccessException exception)
                {
                    object[] args = new object[] { folder, exception.Message };
                    Blizzard.Log.Error("*** UnauthorizedAccessException writing {0}\n*** Exception was: {1}", args);
                }
                catch (ArgumentNullException exception2)
                {
                    object[] objArray2 = new object[] { folder, exception2.Message };
                    Blizzard.Log.Error("*** ArgumentNullException writing {0}\n*** Exception was: {1}", objArray2);
                }
                catch (ArgumentException exception3)
                {
                    object[] objArray3 = new object[] { folder, exception3.Message };
                    Blizzard.Log.Error("*** ArgumentException writing {0}\n*** Exception was: {1}", objArray3);
                }
                catch (PathTooLongException exception4)
                {
                    object[] objArray4 = new object[] { folder, exception4.Message };
                    Blizzard.Log.Error("*** PathTooLongException writing {0}\n*** Exception was: {1}", objArray4);
                }
                catch (DirectoryNotFoundException exception5)
                {
                    object[] objArray5 = new object[] { folder, exception5.Message };
                    Blizzard.Log.Error("*** DirectoryNotFoundException writing {0}\n*** Exception was: {1}", objArray5);
                }
                catch (IOException exception6)
                {
                    object[] objArray6 = new object[] { folder, exception6.Message };
                    Blizzard.Log.Error("*** IOException creating folder '{0}'\n*** Exception was: {1}", objArray6);
                    throw new ApplicationException(string.Format("Unable to create folder '{0}': {1}", folder, exception6.Message));
                }
                catch (NotSupportedException exception7)
                {
                    object[] objArray7 = new object[] { folder, exception7.Message };
                    Blizzard.Log.Error("*** NotSupportedException writing {0}\n*** Exception was: {1}", objArray7);
                }
            }
            return false;
        }

        public static void SearchFoldersForExtension(DirectoryInfo dir, string extension, ref List<FileInfo> results)
        {
            foreach (FileInfo info in dir.GetFiles(string.Format("*.{0}", extension), SearchOption.AllDirectories))
            {
                results.Add(info);
            }
        }

        public static void SearchFoldersForExtension(string dirPath, string extension, ref List<FileInfo> results)
        {
            if (Directory.Exists(dirPath))
            {
                SearchFoldersForExtension(new DirectoryInfo(dirPath), extension, ref results);
            }
        }

        public static void SearchFoldersForExtensions(DirectoryInfo dir, string[] extensions, ref List<FileInfo> results)
        {
            foreach (string str in extensions)
            {
                SearchFoldersForExtension(dir, str, ref results);
            }
        }

        public static void SearchFoldersForExtensions(string dirPath, string[] extensions, ref List<FileInfo> results)
        {
            if (Directory.Exists(dirPath))
            {
                SearchFoldersForExtensions(new DirectoryInfo(dirPath), extensions, ref results);
            }
        }

        public class CopyTask
        {
            public bool overwrite = true;
            public bool sourceIsFolder;
            public bool targetIsFolder;

            private CopyTask()
            {
            }

            public Result copy()
            {
                if (!this.sourceExists)
                {
                    return Result.MissingSource;
                }
                if (this.targetExists)
                {
                    if (!this.overwrite)
                    {
                        return Result.CantOverwriteTarget;
                    }
                    if ((this.targetIsFolder && !FileUtils.SetFolderWritableFlag(this.targetPath, true)) || (!this.targetIsFolder && !FileUtils.SetFileWritableFlag(this.targetPath, true)))
                    {
                        return Result.CantOverwriteTarget;
                    }
                }
                try
                {
                    if (this.sourceIsFolder)
                    {
                        copyRecursive(new DirectoryInfo(this.sourceFolder), this.targetFolder, this.overwrite);
                    }
                    else
                    {
                        Blizzard.File.createFolder(this.targetFolder);
                        System.IO.File.Copy(this.sourcePath, this.targetPath, this.overwrite);
                    }
                    return Result.Success;
                }
                catch (UnauthorizedAccessException exception)
                {
                    object[] args = new object[] { this.sourcePath, this.targetPath, exception.Message };
                    Blizzard.Log.Warning("*** Unauthorized access writing {0} to {1}\n*** Exception was: {2}", args);
                    return Result.UnauthorizedAccess;
                }
                catch (ArgumentNullException)
                {
                    return Result.ArgumentNull;
                }
                catch (ArgumentException)
                {
                    return Result.Argument;
                }
                catch (PathTooLongException)
                {
                    return Result.PathTooLong;
                }
                catch (DirectoryNotFoundException)
                {
                    return Result.DirectoryNotFound;
                }
                catch (FileNotFoundException)
                {
                    return Result.FileNotFound;
                }
                catch (IOException exception2)
                {
                    object[] objArray2 = new object[] { this.sourcePath, this.targetPath, exception2.Message };
                    Blizzard.Log.Warning("*** IO error writing {0} to {1}\n*** Exception was: {2}", objArray2);
                    return Result.IO;
                }
                catch (NotSupportedException)
                {
                    return Result.NotSupported;
                }
                catch (Exception exception3)
                {
                    object[] objArray3 = new object[] { this.sourcePath, this.targetPath, exception3.Message };
                    Blizzard.Log.Warning("*** Unknown error writing {0} to {1}: {2}", objArray3);
                    return Result.Unknown;
                }
            }

            private static void copyRecursive(DirectoryInfo source, string target, bool overwrite)
            {
                if (Blizzard.File.createFolder(target))
                {
                    foreach (FileInfo info in source.GetFiles())
                    {
                        object[] args = new object[] { target, info.Name };
                        System.IO.File.Copy(info.FullName, Blizzard.Path.combine(args), overwrite);
                    }
                    foreach (DirectoryInfo info2 in source.GetDirectories())
                    {
                        object[] objArray2 = new object[] { target, info2.Name };
                        copyRecursive(info2, Blizzard.Path.combine(objArray2) + "/", overwrite);
                    }
                }
            }

            public static Blizzard.File.CopyTask FileToFile(string s, string t)
            {
                return new Blizzard.File.CopyTask { sourcePath = s, targetPath = t };
            }

            public static Blizzard.File.CopyTask FileToFolder(string s, string t)
            {
                return new Blizzard.File.CopyTask { sourcePath = s, targetFolder = t, targetFilename = System.IO.Path.GetFileName(s) };
            }

            public static Blizzard.File.CopyTask FolderToFolder(string s, string t)
            {
                return new Blizzard.File.CopyTask { sourceFolder = s, targetFolder = t, targetIsFolder = true, sourceIsFolder = true };
            }

            public override string ToString()
            {
                return string.Format("{0} => {1}", this.sourcePath, this.targetPath);
            }

            private static DateTime writeTime(bool isFolder, string path)
            {
                return (!isFolder ? System.IO.File.GetLastWriteTime(path) : Directory.GetLastWriteTime(path));
            }

            public bool sourceExists
            {
                get
                {
                    return (!this.sourceIsFolder ? System.IO.File.Exists(this.sourcePath) : Directory.Exists(this.sourceFolder));
                }
            }

            public string sourceFilename { get; private set; }

            public string sourceFolder { get; set; }

            public FileSystemInfo sourceInfo
            {
                get
                {
                    return (!this.targetIsFolder ? ((FileSystemInfo) new FileInfo(this.targetPath)) : ((FileSystemInfo) new DirectoryInfo(this.targetPath)));
                }
            }

            public string sourcePath
            {
                get
                {
                    if (this.sourceFilename == null)
                    {
                    }
                    return System.IO.Path.Combine(this.sourceFolder, string.Empty);
                }
                set
                {
                    this.sourceFolder = System.IO.Path.GetDirectoryName(value);
                    this.sourceFilename = System.IO.Path.GetFileName(value);
                }
            }

            public DateTime sourceTime
            {
                get
                {
                    return writeTime(this.sourceIsFolder, this.sourcePath);
                }
            }

            public bool targetExists
            {
                get
                {
                    return (!this.targetIsFolder ? System.IO.File.Exists(this.targetPath) : Directory.Exists(this.targetFolder));
                }
            }

            public string targetFilename { get; private set; }

            public string targetFolder { get; set; }

            public FileSystemInfo targetInfo
            {
                get
                {
                    return (!this.targetIsFolder ? ((FileSystemInfo) new FileInfo(this.targetPath)) : ((FileSystemInfo) new DirectoryInfo(this.targetPath)));
                }
            }

            public string targetPath
            {
                get
                {
                    if (this.targetIsFolder)
                    {
                        return (!this.sourceIsFolder ? System.IO.Path.Combine(this.targetFolder, this.sourceFilename) : this.targetFolder);
                    }
                    return System.IO.Path.Combine(this.targetFolder, this.targetFilename);
                }
                set
                {
                    this.targetFolder = System.IO.Path.GetDirectoryName(value);
                    this.targetFilename = System.IO.Path.GetFileName(value);
                }
            }

            public DateTime targetTime
            {
                get
                {
                    return writeTime(this.targetIsFolder, this.targetPath);
                }
            }

            public enum Result
            {
                Unknown,
                Success,
                MissingSource,
                CantOverwriteTarget,
                UnauthorizedAccess,
                ArgumentNull,
                Argument,
                PathTooLong,
                DirectoryNotFound,
                FileNotFound,
                IO,
                NotSupported
            }
        }
    }

    public class Log
    {
        public static void Error(string message)
        {
            try
            {
                UnityEngine.Debug.LogError(string.Format("{0}: Error: {1}", Blizzard.Time.Stamp(), message));
            }
            catch (Exception)
            {
            }
        }

        public static void Error(string format, params object[] args)
        {
            Error(string.Format(format, args));
        }

        public static void SayToFile(StreamWriter logFile, string format, params object[] args)
        {
            try
            {
                string str = Blizzard.Time.Stamp() + ": " + string.Format(format, args);
                if (logFile != null)
                {
                    logFile.WriteLine(str);
                    logFile.Flush();
                }
            }
            catch (Exception)
            {
            }
        }

        public static void Warning(string message)
        {
            try
            {
                UnityEngine.Debug.LogWarning(string.Format("{0}: Warning: {1}", Blizzard.Time.Stamp(), message));
            }
            catch (Exception)
            {
            }
        }

        public static void Warning(string format, params object[] args)
        {
            Warning(string.Format(format, args));
        }

        public static void Write(string message)
        {
            try
            {
                UnityEngine.Debug.Log(string.Format("{0}: {1}", Blizzard.Time.Stamp(), message));
            }
            catch (Exception)
            {
            }
        }

        public static void Write(string format, params object[] args)
        {
            Write(string.Format(format, args));
        }
    }

    public static class Path
    {
        public static string combine(params object[] args)
        {
            string str = string.Empty;
            foreach (object obj2 in args)
            {
                if ((obj2 != null) && !string.IsNullOrEmpty(obj2.ToString()))
                {
                    str = System.IO.Path.Combine(str, obj2.ToString());
                }
            }
            return str;
        }
    }

    public class Time
    {
        public static string FormatElapsedTime(TimeSpan elapsedTime)
        {
            StringBuilder builder = new StringBuilder();
            if (elapsedTime.TotalHours >= 1.0)
            {
                builder.Append(string.Format("{0}h", Convert.ToInt32(elapsedTime.TotalHours)));
            }
            if (elapsedTime.Minutes >= 1.0)
            {
                builder.Append(string.Format("{0}m", Convert.ToInt32(elapsedTime.Minutes)));
            }
            builder.Append(elapsedTime.Seconds);
            builder.Append(".");
            builder.Append(elapsedTime.Milliseconds);
            builder.Append("s");
            return builder.ToString();
        }

        public static string Stamp()
        {
            return Stamp(DateTime.Now);
        }

        public static string Stamp(DateTime then)
        {
            return then.ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        public class ScopedTimer : IDisposable
        {
            private readonly string message_;
            private readonly DateTime start_ = DateTime.Now;

            private ScopedTimer(string message)
            {
                this.message_ = message;
            }

            public static Blizzard.Time.ScopedTimer Create(string postMessage)
            {
                return new Blizzard.Time.ScopedTimer(postMessage);
            }

            public static Blizzard.Time.ScopedTimer Create(string preMessage, string postMessage)
            {
                Blizzard.Log.Write(preMessage);
                return Create(postMessage);
            }

            public void Dispose()
            {
                Blizzard.Log.Write(string.Format("{0} ({1})", this.message_, Blizzard.Time.FormatElapsedTime(DateTime.Now.Subtract(this.start_))));
            }
        }
    }
}

