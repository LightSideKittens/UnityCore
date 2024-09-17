using System.IO;

public class FileExt
{
    public static void DeletePath(string path)
    {
        if (Directory.Exists(path))
        {
            Directory.Delete(path, true);
        }
        else
        {
            string directory = Path.GetDirectoryName(path);
            string fileNameWithoutExtension = Path.GetFileName(path);

            if (directory == null)
            {
                directory = Directory.GetCurrentDirectory();
            }

            string[] files = Directory.GetFiles(directory, fileNameWithoutExtension + ".*");

            foreach (string file in files)
            {
                File.Delete(file);
            }
        }
    }
}