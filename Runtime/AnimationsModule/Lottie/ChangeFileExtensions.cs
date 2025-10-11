using UnityEngine;
using UnityEditor;
using System.IO;

public class ChangeFileExtensions : MonoBehaviour
{
    [MenuItem("Tools/Change File Extensions")]
    private static void ChangeExtensions()
    {
        // Путь к папке — можно заменить или задать через окно
        string folderPath = EditorUtility.OpenFolderPanel("Выберите папку", Application.dataPath, "");

        if (string.IsNullOrEmpty(folderPath))
        {
            Debug.LogWarning("Папка не выбрана.");
            return;
        }
        
        string newExtension = ".ziplottie";

        if (string.IsNullOrEmpty(newExtension))
        {
            Debug.LogWarning("Расширение не задано.");
            return;
        }

        // Добавляем точку, если пользователь её не ввёл
        if (!newExtension.StartsWith("."))
            newExtension = "." + newExtension;

        // Получаем все файлы
        string[] files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);

        int changedCount = 0;

        foreach (string filePath in files)
        {
            var ext = Path.GetExtension(filePath);
            string newPath = Path.ChangeExtension(filePath, newExtension);
            if (ext == ".meta")
            {
                newPath = Path.ChangeExtension(filePath[..^5], newExtension) + ".meta";
            }
            File.Move(filePath, newPath);
            changedCount++;
        }

        Debug.Log($"✅ Изменено расширение у {changedCount} файлов на {newExtension}");
    }
}