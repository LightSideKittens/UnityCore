using System.IO;
using System.IO.Compression;

public static class TgsHelper
{
    public static void DecompressTgsFile(string inputPath, string outputPath)
    {
        using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var outputStream = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
        {
            gzipStream.CopyTo(outputStream);
        }
    }
    
    public static string TgsToJsonString(string inputPath)
    {
        using (var inputStream = new FileStream(inputPath, FileMode.Open, FileAccess.Read))
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzipStream))
        {
            return reader.ReadToEnd();
        }
    }
    
    public static string TgsToJsonString(byte[] rawData)
    {
        using (var inputStream = new MemoryStream(rawData))
        using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
        using (var reader = new StreamReader(gzipStream))
        {
            return reader.ReadToEnd();
        }
    }
}