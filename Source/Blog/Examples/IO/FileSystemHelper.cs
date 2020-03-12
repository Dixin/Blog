namespace Examples.IO
{
    using System;
    using System.IO;

    public static class FileSystemHelper
    {
        public static void Move(string source, string destination, bool overwrite = false)
        {
            if (File.Exists(source))
            {
                FileHelper.Move(source, destination, overwrite);
            }
            else if(Directory.Exists(source))
            {
                DirectoryHelper.Move(source, destination, overwrite);
            }
            else
            {
                throw new ArgumentException("The provided path does not exist.", nameof(source));
            }
        }
    }
}
