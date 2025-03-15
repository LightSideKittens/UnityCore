using UnityEngine;
using System.IO;
using UnityEngine.Networking;

public static class StreamingAssetsUtils
{
    public static string GetOrCreateCopiedFile(string filename)
    {
        string persistentPath = Path.Combine(Application.persistentDataPath, filename);
        
        if (!File.Exists(persistentPath))
        {
            string sourcePath = Path.Combine(Application.streamingAssetsPath, filename);

#if UNITY_ANDROID && !UNITY_EDITOR
            using (UnityWebRequest www = UnityWebRequest.Get(sourcePath))
            {
                www.SendWebRequest();
                while (!www.isDone) { }
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    return null;
                }
                File.WriteAllBytes(persistentPath, www.downloadHandler.data);
            }
#else
            if (File.Exists(sourcePath))
            {
                File.Copy(sourcePath, persistentPath);
            }
            else
            {
                return null;
            }
#endif
        }
        
        return persistentPath;
    }
}
