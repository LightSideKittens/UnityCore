using System.IO;
using System.IO.Compression;

public static class TgsHelper
{
    /// <summary>
    /// Распаковывает .tgs (gzip) в обычный JSON-файл.
    /// </summary>
    /// <param name="inputPath">Путь к исходному .tgs-файлу</param>
    /// <param name="outputPath">Путь, куда сохранить JSON</param>
    public static void DecompressTgsFile(string inputPath, string outputPath)
    {
        using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            gzipStream.CopyTo(outputStream);
        }
    }

    /// <summary>
    /// Возвращает содержимое .tgs (gzip) как строку JSON.
    /// </summary>
    public static string TgsToJsonString(string inputPath)
    {
        using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzipStream))
        {
            return reader.ReadToEnd();
        }
    }
}