using Microsoft.VisualBasic;
using Microsoft.VisualBasic.CompilerServices;
using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Security;
using static Examples.IO.NativeMethods;

namespace Examples.IO;

internal static class FileSystem
{
    private enum UIOptionInternal
    {
        OnlyErrorDialogs = UIOption.OnlyErrorDialogs,
        AllDialogs = UIOption.AllDialogs,
        NoUI
    }

    private enum FileOrDirectory
    {
        File,
        Directory
    }

    public static void DeleteDirectory(string directory, DeleteDirectoryOption onDirectoryNotEmpty)
    {
        DeleteDirectoryInternal(directory, onDirectoryNotEmpty, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
    }

    public static void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle)
    {
        DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException);
    }

    public static void DeleteDirectory(string directory, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
    {
        DeleteDirectoryInternal(directory, DeleteDirectoryOption.DeleteAllContents, ToUIOptionInternal(showUI), recycle, onUserCancel);
    }

    public static void DeleteFile(string file)
    {
        DeleteFileInternal(file, UIOptionInternal.NoUI, RecycleOption.DeletePermanently, UICancelOption.ThrowException);
    }

    public static void DeleteFile(string file, UIOption showUI, RecycleOption recycle)
    {
        DeleteFileInternal(file, ToUIOptionInternal(showUI), recycle, UICancelOption.ThrowException);
    }

    public static void DeleteFile(string file, UIOption showUI, RecycleOption recycle, UICancelOption onUserCancel)
    {
        DeleteFileInternal(file, ToUIOptionInternal(showUI), recycle, onUserCancel);
    }

    private static void DeleteDirectoryInternal(string directory, DeleteDirectoryOption onDirectoryNotEmpty, UIOptionInternal showUI, RecycleOption recycle, UICancelOption onUserCancel)
    {
        VerifyDeleteDirectoryOption(nameof(onDirectoryNotEmpty), onDirectoryNotEmpty);
        VerifyRecycleOption(nameof(recycle), recycle);
        VerifyUICancelOption(nameof(onUserCancel), onUserCancel);
        string fullPath = Path.GetFullPath(directory);
        ThrowIfDevicePath(fullPath);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException($"Could not find directory '{directory}'.");
        }

        if (IsRoot(fullPath))
        {
            throw new IOException($"Could not complete operation since directory is a root directory: '{directory}'.");
        }

        if (showUI != UIOptionInternal.NoUI && Environment.UserInteractive)
        {
            ShellDelete(fullPath, showUI, recycle, onUserCancel, FileOrDirectory.Directory);
        }
        else
        {
            Directory.Delete(fullPath, onDirectoryNotEmpty == DeleteDirectoryOption.DeleteAllContents);
        }
    }

    private static void VerifyDeleteDirectoryOption(string argName, DeleteDirectoryOption argValue)
    {
        if (argValue != DeleteDirectoryOption.DeleteAllContents && argValue != DeleteDirectoryOption.ThrowIfDirectoryNonEmpty)
        {
            throw new InvalidEnumArgumentException(argName, (int)argValue, typeof(DeleteDirectoryOption));
        }
    }

    private static void DeleteFileInternal(string file, UIOptionInternal showUI, RecycleOption recycle, UICancelOption onUserCancel)
    {
        VerifyRecycleOption(nameof(recycle), recycle);
        VerifyUICancelOption(nameof(onUserCancel), onUserCancel);
        string fileFullPath = NormalizeFilePath(file, nameof(file));
        ThrowIfDevicePath(fileFullPath);
        if (!File.Exists(fileFullPath))
        {
            throw new FileNotFoundException($"Could not find file {fileFullPath}", fileFullPath);
        }
        if (showUI != UIOptionInternal.NoUI && Environment.UserInteractive)
        {
            ShellDelete(fileFullPath, showUI, recycle, onUserCancel, FileOrDirectory.File);
        }
        else
        {
            File.Delete(fileFullPath);
        }
    }

    private static void VerifyRecycleOption(string argName, RecycleOption argValue)
    {
        if (argValue != RecycleOption.DeletePermanently && argValue != RecycleOption.SendToRecycleBin)
        {
            throw new InvalidEnumArgumentException(argName, (int)argValue, typeof(RecycleOption));
        }
    }

    private static void VerifyUICancelOption(string argName, UICancelOption argValue)
    {
        if (argValue != UICancelOption.DoNothing && argValue != UICancelOption.ThrowException)
        {
            throw new InvalidEnumArgumentException(argName, (int)argValue, typeof(UICancelOption));
        }
    }

    private static void ThrowWinIOError(int errorCode) =>
        throw (errorCode switch
        {
            NativeTypes.ErrorFileNotFound => new FileNotFoundException(),
            NativeTypes.ErrorPathNotFound => new DirectoryNotFoundException(),
            NativeTypes.ErrorAccessDenied => new UnauthorizedAccessException(),
            NativeTypes.ErrorFilenameExcedRange => new PathTooLongException(),
            NativeTypes.ErrorInvalidDrive => new DriveNotFoundException(),
            NativeTypes.ErrorOperationAborted => new OperationCanceledException(),
            NativeTypes.ErrorCancelled => new OperationCanceledException(),
            _ => new IOException(new Win32Exception(errorCode).Message, Marshal.GetHRForLastWin32Error())
        });


    internal static string NormalizeFilePath(string path, string paramName)
    {
        CheckFilePathTrailingSeparator(path, paramName);
        return NormalizePath(path);
    }

    internal static void CheckFilePathTrailingSeparator(string path, string paramName)
    {
        if (Operators.CompareString(path, "", TextCompare: false) == 0)
        {
            throw new ArgumentNullException(paramName);
        }

        if (path.EndsWith(Conversions.ToString(Path.DirectorySeparatorChar), StringComparison.Ordinal) | path.EndsWith(Conversions.ToString(Path.AltDirectorySeparatorChar), StringComparison.Ordinal))
        {
            throw new ArgumentException("The given file path ends with a directory separator character.", paramName);
        }
    }

    internal static string NormalizePath(string path) => GetLongPath(RemoveEndingSeparator(Path.GetFullPath(path)));

    private static void ThrowIfDevicePath(string path)
    {
        if (path.StartsWith(@"\\.\", StringComparison.Ordinal))
        {
            throw new ArgumentException(@"The given path is a Win32 device path. Don't use paths starting with '\\.\'.", nameof(path));
        }
    }

    private static string GetLongPath(string fullPath)
    {
        Debug.Assert(fullPath != "" && Path.IsPathRooted(fullPath), "Must be full path!!!");

        try
        {
            if (IsRoot(fullPath))
            {
                return fullPath;
            }

            DirectoryInfo directoryInfo = new(GetParentPath(fullPath));
            if (File.Exists(fullPath))
            {
                Debug.Assert(directoryInfo.GetFiles(Path.GetFileName(fullPath)).Length == 1, "Must found exactly 1!!!");
                return directoryInfo.GetFiles(Path.GetFileName(fullPath))[0].FullName;
            }

            if (Directory.Exists(fullPath))
            {
                Debug.Assert(directoryInfo.GetDirectories(Path.GetFileName(fullPath)).Length == 1, "Must found exactly 1!!!");
                return directoryInfo.GetDirectories(Path.GetFileName(fullPath))[0].FullName;
            }

            return fullPath;
        }
        catch (Exception ex)
        {
            if (ex is ArgumentException or ArgumentNullException or PathTooLongException or NotSupportedException or DirectoryNotFoundException or SecurityException or UnauthorizedAccessException)
            {
                return fullPath;
            }

            throw;
        }
    }

    private static bool IsRoot(string path)
    {
        if (!Path.IsPathRooted(path))
        {
            return false;
        }
        path = path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return string.Equals(path, Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase);
    }

    private static string RemoveEndingSeparator(string path)
    {
        if (Path.IsPathRooted(path) && path.Equals(Path.GetPathRoot(path), StringComparison.OrdinalIgnoreCase))
        {
            return path;
        }
        return path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }

    public static string GetParentPath(string path)
    {
        Path.GetFullPath(path);
        if (IsRoot(path))
        {
            throw new ArgumentException($"Could not get parent path since the given path is a root directory: '{path}'.", nameof(path));
        }
        return Path.GetDirectoryName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar));
    }

    private static UIOptionInternal ToUIOptionInternal(UIOption showUI)
    {
        return showUI switch
        {
            UIOption.AllDialogs => UIOptionInternal.AllDialogs,
            UIOption.OnlyErrorDialogs => UIOptionInternal.OnlyErrorDialogs,
            _ => throw new InvalidEnumArgumentException(nameof(showUI), (int)showUI, typeof(UIOption)),
        };
    }

    private static void ShellDelete(string fullPath, UIOptionInternal showUI, RecycleOption recycle, UICancelOption onUserCancel, FileOrDirectory fileOrDirectory)
    {
        Debug.Assert(fullPath != "" && Path.IsPathRooted(fullPath), "FullPath must be a full path!!!");
        Debug.Assert(showUI != UIOptionInternal.NoUI, "Why call ShellDelete if ShowUI is NoUI???");

        ShFileOperationFlags shFileOperationFlags = GetOperationFlags(showUI);
        if (recycle == RecycleOption.SendToRecycleBin)
        {
            shFileOperationFlags |= ShFileOperationFlags.FofAllowundo;
        }

        ShellFileOperation(ShFileOperationType.FoDelete, shFileOperationFlags, fullPath, null, onUserCancel, fileOrDirectory);
    }

    private static ShFileOperationFlags GetOperationFlags(UIOptionInternal showUI)
    {
        ShFileOperationFlags shFileOperationFlags = ShFileOperationFlags.FofNoconfirmmkdir | ShFileOperationFlags.FofNoConnectedElements;
        if (showUI == UIOptionInternal.OnlyErrorDialogs)
        {
            shFileOperationFlags |= ShFileOperationFlags.FofSilent | ShFileOperationFlags.FofNoconfirmation;
        }

        return shFileOperationFlags;
    }

    private static void ShellFileOperation(ShFileOperationType operationType, ShFileOperationFlags operationFlags, string fullSource, string? fullTarget, UICancelOption onUserCancel, FileOrDirectory fileOrDirectory)
    {
        Debug.Assert(Enum.IsDefined(typeof(ShFileOperationType), operationType));
        Debug.Assert(operationType != ShFileOperationType.FoRename, "Don't call Shell to rename!!!");
        Debug.Assert(fullSource != "" && Path.IsPathRooted(fullSource), "Invalid FullSource path!!!");
        Debug.Assert(operationType == ShFileOperationType.FoDelete || (fullTarget != "" && Path.IsPathRooted(fullTarget)), "Invalid FullTarget path!!!");

        Shfileopstruct lpFileOp = GetShellOperationInfo(operationType, operationFlags, fullSource, fullTarget);
        int result;
        try
        {
            result = SHFileOperation(ref lpFileOp);
            SHChangeNotify(145439u, 3u, IntPtr.Zero, IntPtr.Zero);
        }
        catch (Exception)
        {
            throw;
        }

        if (lpFileOp.fAnyOperationsAborted)
        {
            if (onUserCancel == UICancelOption.ThrowException)
            {
                throw new OperationCanceledException();
            }
        }
        else if (result != 0)
        {
            ThrowWinIOError(result);
        }
    }

    private static Shfileopstruct GetShellOperationInfo(ShFileOperationType operationType, ShFileOperationFlags operationFlags, string sourcePath, string? targetPath = null)
    {
        Debug.Assert(sourcePath != "" && Path.IsPathRooted(sourcePath), "Invalid SourcePath!!!");

        return GetShellOperationInfo(operationType, operationFlags, new string[] { sourcePath }, targetPath);
    }

    private static Shfileopstruct GetShellOperationInfo(ShFileOperationType operationType, ShFileOperationFlags operationFlags, string[] sourcePaths, string? targetPath = null)
    {
        Debug.Assert(Enum.IsDefined(typeof(ShFileOperationType), operationType), "Invalid OperationType!!!");
        Debug.Assert(string.IsNullOrEmpty(targetPath) || Path.IsPathRooted(targetPath), "Invalid TargetPath!!!");
        Debug.Assert(sourcePaths is not null && sourcePaths.Length > 0, "Invalid SourcePaths!!!");

        Shfileopstruct operationInfo = new()
        {
            wFunc = (uint)operationType,
            fFlags = (ushort)operationFlags,
            pFrom = GetShellPath(sourcePaths),
            pTo = targetPath is null ? null : GetShellPath(targetPath),
            hNameMappings = IntPtr.Zero
        };

        try
        {
            operationInfo.hwnd = Process.GetCurrentProcess().MainWindowHandle;
        }
        catch (Exception ex)
        {
            if (ex is SecurityException or InvalidOperationException or NotSupportedException)
            {
                operationInfo.hwnd = IntPtr.Zero;
            }
            else
            {
                throw;
            }
        }
        operationInfo.lpszProgressTitle = string.Empty;
        return operationInfo;
    }

    private static string GetShellPath(string fullPath)
    {
        Debug.Assert(fullPath != "" && Path.IsPathRooted(fullPath), "Must be full path!!!");

        return GetShellPath(new string[] { fullPath });
    }

    private static string GetShellPath(string[] fullPaths)
    {
        StringBuilder multiString = new();
        foreach (string fullPath in fullPaths)
        {
            multiString.Append(fullPath + ControlChars.NullChar);
        }

        Debug.Assert(multiString.ToString().EndsWith(Conversions.ToString(ControlChars.NullChar), StringComparison.Ordinal));

        return multiString.ToString();
    }
}

internal static class NativeMethods
{
    [Flags]
    internal enum ShFileOperationFlags : ushort
    {
        FofMultidestfiles = 0x1,
        FofConfirmmouse = 0x2,
        FofSilent = 0x4,
        FofRenameoncollision = 0x8,
        FofNoconfirmation = 0x10,
        FofWantmappinghandle = 0x20,
        FofAllowundo = 0x40,
        FofFilesonly = 0x80,
        FofSimpleprogress = 0x100,
        FofNoconfirmmkdir = 0x200,
        FofNoerrorui = 0x400,
        FofNocopysecurityattribs = 0x800,
        FofNorecursion = 0x1000,
        FofNoConnectedElements = 0x2000,
        FofWantnukewarning = 0x4000,
        FofNorecursereparse = 0x8000
    }

    internal enum ShFileOperationType : uint
    {
        FoMove = 1u,
        FoCopy = 0x2,
        FoDelete = 0x3,
        FoRename = 0x4
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto, Pack = 1)]
    internal struct Shfileopstruct
    {
        internal IntPtr hwnd;

        internal uint wFunc;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string pFrom;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string? pTo;

        internal ushort fFlags;

        internal bool fAnyOperationsAborted;

        internal IntPtr hNameMappings;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string lpszProgressTitle;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct Shfileopstruct64
    {
        internal IntPtr hwnd;

        internal uint wFunc;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string pFrom;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string? pTo;

        internal ushort fFlags;

        internal bool fAnyOperationsAborted;

        internal IntPtr hNameMappings;

        [MarshalAs(UnmanagedType.LPTStr)]
        internal string lpszProgressTitle;
    }

    internal static int SHFileOperation(ref Shfileopstruct lpFileOp)
    {
        if (IntPtr.Size == 4)
        {
            return SHFileOperation32(ref lpFileOp);
        }

        Shfileopstruct64 lpFileOp64 = new()
        {
            hwnd = lpFileOp.hwnd,
            wFunc = lpFileOp.wFunc,
            pFrom = lpFileOp.pFrom,
            pTo = lpFileOp.pTo,
            fFlags = lpFileOp.fFlags,
            fAnyOperationsAborted = lpFileOp.fAnyOperationsAborted,
            hNameMappings = lpFileOp.hNameMappings,
            lpszProgressTitle = lpFileOp.lpszProgressTitle
        };

        int result = SHFileOperation64(ref lpFileOp64);
        lpFileOp.fAnyOperationsAborted = lpFileOp64.fAnyOperationsAborted;
        return result;
    }

    [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = nameof(SHFileOperation), SetLastError = true, ThrowOnUnmappableChar = true)]
    private static extern int SHFileOperation32(ref Shfileopstruct lpFileOp);

    [DllImport("shell32.dll", CharSet = CharSet.Auto, EntryPoint = nameof(SHFileOperation), SetLastError = true, ThrowOnUnmappableChar = true)]
    private static extern int SHFileOperation64(ref Shfileopstruct64 lpFileOp);

    [DllImport("shell32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    internal static extern void SHChangeNotify(uint wEventId, uint uFlags, IntPtr dwItem1, IntPtr dwItem2);
}

internal static class NativeTypes
{
    internal const int ErrorFileNotFound = 2;

    internal const int ErrorPathNotFound = 3;

    internal const int ErrorAccessDenied = 5;

    internal const int ErrorAlreadyExists = 183;

    internal const int ErrorFilenameExcedRange = 206;

    internal const int ErrorInvalidDrive = 15;

    internal const int ErrorInvalidParameter = 87;

    internal const int ErrorSharingViolation = 32;

    internal const int ErrorFileExists = 80;

    internal const int ErrorOperationAborted = 995;

    internal const int ErrorCancelled = 1223;
}
