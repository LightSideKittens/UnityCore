#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using System.IO;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Threading;
using JetBrains.Annotations;
using Newtonsoft.Json.Linq;

public class LightSideProjectSetuper : OdinEditorWindow
{
    [ToggleLeft]
    [LabelText("Add Existing (skip cloning)")]
    [InfoBox("Если включено, скрипт НЕ будет вызывать 'git clone', а просто найдёт уже существующую папку 'Package' и прочитает её package.json.")]
    public bool addExisting = false;
    
    [HideIf("addExisting")]
    [Title("Git Repository Settings")]
    [InfoBox("При addExisting = false:\n" +
             " - Скрипт клонирует репозиторий в папку localClonePath + /Package.\n" +
             "При addExisting = true:\n" +
             " - Клонирование пропускается, скрипт ищет уже существующую папку \"Package\" и package.json внутри неё.")]
    [LabelText("Repository URL"), TextArea(1, 2)]
    public string gitRepositoryUrl = "https://github.com/LightSideKittens/BaseProject.git";

    [HideIf("addExisting")]
    [LabelText("Branch (optional)")]
    [Tooltip("Если не задано, будет клонирована основная ветка (например, 'main' или 'master').")]
    public string branchName = "";

    [FolderPath(RequireExistingPath = false, AbsolutePath = true)]
    [LabelText("Local Clone Path")]
    [InfoBox("При addExisting=false, результат будет localClonePath + '/Package'.\n" +
             "При addExisting=true, нужно либо:\n" +
             " - чтобы в localClonePath уже была подпапка 'Package' с package.json\n" +
             " - либо чтобы localClonePath само называлось 'Package' и в нём лежал package.json")]
    public string localClonePath = "";

    private Process cloneProcess;
    private StringBuilder logBuilder = new StringBuilder();
    [UsedImplicitly] private bool isCloning;
    private bool isCancelRequested;

    private float overallProgress;

    private float receivingProgress;
    private float resolvingProgress;

    private static readonly Regex advancedProgressRegex =
        new Regex(@"(\w+) objects:\s+(\d+)% \((\d+)\/(\d+)\)", RegexOptions.Compiled);

    private static readonly Regex simplePercentRegex =
        new Regex(@"(\d+)%", RegexOptions.Compiled);

    [MenuItem("Tools/Light Side Core Installer")]
    private static void OpenWindow()
    {
        var window = GetWindow<LightSideProjectSetuper>();
        window.titleContent = new GUIContent("Light Side Project Setuper");
        window.Show();
    }

    [Button("Clone (or Add) & Link", ButtonSizes.Large), GUIColor(0.3f, 0.8f, 0.3f)]
    [EnableIf("@!isCloning")]
    public void CloneOrAddAndLink()
    {
        lock (logBuilder)
        {
            logBuilder.Clear();
        }

        if (!CheckGitAvailable())
        {
            LogError("Git не найден или не доступен в PATH.");
            return;
        }

        if (addExisting)
        {
            isCancelRequested = false;
            isCloning = false;
            string packageFolder = FindPackageFolder(localClonePath);
            if (string.IsNullOrEmpty(packageFolder))
            {
                LogError("Не удалось найти директорию 'Package' и файл package.json в выбранном пути.");
                return;
            }

            UpdateManifestJsonFromPackage(packageFolder);
        }
        else
        {
            StartCloneAsync();
        }
    }
    
    [Button(ButtonSizes.Medium), GUIColor(0.9f, 0.3f, 0.3f)]
    [InfoBox("Удаляет зависимость из manifest.json на основе поля 'name' из package.json в папке 'Package'.")]
    public void RemoveDependency()
    {
        string packageFolder = FindPackageFolder(localClonePath);
        if (string.IsNullOrEmpty(packageFolder))
        {
            LogError("Не удалось найти директорию 'Package' или package.json для удаления зависимости.");
            return;
        }

        string packageJsonPath = Path.Combine(packageFolder, "package.json");
        if (!File.Exists(packageJsonPath))
        {
            LogError($"Не найден package.json по пути: {packageJsonPath}");
            return;
        }

        string packageName;
        try
        {
            var jsonText = File.ReadAllText(packageJsonPath);
            var json = JObject.Parse(jsonText);
            var nameToken = json["name"];
            if (nameToken == null)
            {
                LogError("В package.json отсутствует поле 'name'.");
                return;
            }

            packageName = nameToken.ToString();
        }
        catch (Exception e)
        {
            LogError("Ошибка при чтении package.json: " + e.Message);
            return;
        }

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
        if (!File.Exists(manifestPath))
        {
            LogError("Не найден manifest.json: " + manifestPath);
            return;
        }

        try
        {
            string manifestText = File.ReadAllText(manifestPath);
            JObject manifestJson = JObject.Parse(manifestText);

            var deps = manifestJson["dependencies"] as JObject;
            if (deps == null)
            {
                LogError("В manifest.json не найдена секция 'dependencies'.");
                return;
            }

            if (deps[packageName] != null)
            {
                deps.Remove(packageName);
                File.WriteAllText(manifestPath, manifestJson.ToString());
                Log($"Удалена зависимость '{packageName}' из manifest.json");
            }
            else
            {
                LogWarning($"В manifest.json нет зависимости с ключом '{packageName}'.");
            }

            AssetDatabase.Refresh();
        }
        catch (Exception e)
        {
            LogError("Ошибка при попытке удалить зависимость: " + e.Message);
        }
    }


    [Button(ButtonSizes.Medium), GUIColor(1f, 0.5f, 0.5f)]
    [EnableIf("@isCloning")]
    [LabelText("Cancel Clone")]
    public void CancelClone()
    {
        isCancelRequested = true;
        if (cloneProcess != null && !cloneProcess.HasExited)
        {
            try
            {
                cloneProcess.Kill();
                LogWarning("Операция клонирования отменена пользователем.");
            }
            catch (Exception e)
            {
                LogError("Не удалось убить процесс: " + e.Message);
            }
        }
    }

    [ShowInInspector, ProgressBar(0, 100, 0)]
    [HideLabel, LabelText("Progress")]
    [DisableIf("@!isCloning")]
    public float CloneProgress => overallProgress;

    [ShowInInspector, MultiLineProperty(10), HideLabel, ReadOnly]
    public string CloneLog
    {
        get
        {
            lock (logBuilder)
            {
                return logBuilder.ToString();
            }
        }
    }

    private void StartCloneAsync()
    {
        isCancelRequested = false;
        isCloning = true;
        overallProgress = 0f;
        receivingProgress = 0f;
        resolvingProgress = 0f;
        
        try
        {
            Directory.CreateDirectory(localClonePath);
        }
        catch (Exception e)
        {
            LogError("Не удалось создать папку: " + e.Message);
            isCloning = false;
            return;
        }
        
        if (Directory.Exists(localClonePath) && Directory.GetFiles(localClonePath).Length > 0)
        {
            LogError($"В папке '{localClonePath}' уже есть какие-то файлы. " +
                     $"git clone может выдать ошибку, если там уже есть репозиторий или другие файлы.");
            isCloning = false;
            return;
        }

        string gitArgs = $"clone \"{gitRepositoryUrl}\" \"{localClonePath}\"";
        if (!string.IsNullOrWhiteSpace(branchName))
        {
            gitArgs = $"clone --single-branch --branch \"{branchName}\" \"{gitRepositoryUrl}\" \"{localClonePath}\"";
        }

        var processInfo = new ProcessStartInfo("git", gitArgs)
        {
            CreateNoWindow = true,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        try
        {
            cloneProcess = new Process();
            cloneProcess.StartInfo = processInfo;
            cloneProcess.EnableRaisingEvents = true;
            cloneProcess.OutputDataReceived += OnOutputDataReceived;
            cloneProcess.ErrorDataReceived += OnErrorDataReceived;
            cloneProcess.Exited += OnProcessExited;

            cloneProcess.Start();
            cloneProcess.BeginOutputReadLine();
            cloneProcess.BeginErrorReadLine();

            EditorApplication.update += ForceRepaint;
        }
        catch (Exception e)
        {
            LogError("Ошибка при запуске git-процесса: " + e.Message);
            isCloning = false;
        }
    }
    
    private void OnOutputDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            lock (logBuilder)
            {
                logBuilder.AppendLine(e.Data);
            }

            var matchAdv = advancedProgressRegex.Match(e.Data);
            if (matchAdv.Success)
            {
                string stage = matchAdv.Groups[1].Value;
                int percent = int.Parse(matchAdv.Groups[2].Value);
                int current = int.Parse(matchAdv.Groups[3].Value);
                int total = int.Parse(matchAdv.Groups[4].Value);

                float fraction = (float)current / Mathf.Max(1, total);
                if (stage == "Receiving")
                    receivingProgress = fraction;
                else if (stage == "Resolving")
                    resolvingProgress = fraction;

                overallProgress = receivingProgress * 80f + resolvingProgress * 20f;
            }
            else
            {
                var matchSimple = simplePercentRegex.Match(e.Data);
                if (matchSimple.Success)
                {
                    if (int.TryParse(matchSimple.Groups[1].Value, out int val))
                    {
                        overallProgress = val;
                    }
                }
            }
        }
    }

    private void OnErrorDataReceived(object sender, DataReceivedEventArgs e)
    {
        if (!string.IsNullOrEmpty(e.Data))
        {
            lock (logBuilder)
            {
                logBuilder.AppendLine("[ERR] " + e.Data);
            }
        }
    }

    private void OnProcessExited(object sender, EventArgs e)
    {
        bool success = (cloneProcess.ExitCode == 0 && !isCancelRequested);

        lock (logBuilder)
        {
            logBuilder.AppendLine($"Процесс клонирования завершён. ExitCode: {cloneProcess.ExitCode}");
        }

        cloneProcess.OutputDataReceived -= OnOutputDataReceived;
        cloneProcess.ErrorDataReceived -= OnErrorDataReceived;
        cloneProcess.Exited -= OnProcessExited;
        cloneProcess = null;

        EditorApplication.update -= ForceRepaint;
        isCloning = false;

        if (success)
        {
            string finalPackagePath = Path.Combine(localClonePath, "Package");
            UpdateManifestJsonFromPackage(finalPackagePath);
        }
        else
        {
            LogError("Git clone завершился ошибкой или был отменён.");
        }
    }
    
    private string FindPackageFolder(string basePath)
    {
        if (!Directory.Exists(basePath))
        {
            LogError($"Указанная папка не найдена: {basePath}");
            return null;
        }

        string lastName = new DirectoryInfo(basePath).Name;
        if (string.Equals(lastName, "Package", StringComparison.OrdinalIgnoreCase))
        {
            string packageJsonPath = Path.Combine(basePath, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                LogError($"В {basePath} не найден package.json");
                return null;
            }
            return basePath;
        }
        else
        {
            string subPkg = Path.Combine(basePath, "Package");
            if (!Directory.Exists(subPkg))
            {
                LogError($"В папке {basePath} не найдена поддиректория 'Package'");
                return null;
            }

            string packageJsonPath = Path.Combine(subPkg, "package.json");
            if (!File.Exists(packageJsonPath))
            {
                LogError($"В {subPkg} не найден package.json");
                return null;
            }
            return subPkg;
        }
    }

    private void UpdateManifestJsonFromPackage(string packageFolder)
    {
        Remover.Remove();
        Log($"{Thread.CurrentThread.ManagedThreadId}");
        
        string packageJsonPath = Path.Combine(packageFolder, "package.json");
        if (!File.Exists(packageJsonPath))
        {
            LogError($"Не найден файл package.json в {packageFolder}");
            return;
        }

        string nameValue;
        try
        {
            string jsonText = File.ReadAllText(packageJsonPath);
            JObject json = JObject.Parse(jsonText);
            JToken nameToken = json["name"];
            if (nameToken == null)
            {
                LogError("В package.json нет поля 'name'");
                return;
            }
            nameValue = nameToken.ToString();
        }
        catch (Exception e)
        {
            LogError("Ошибка при чтении package.json: " + e.Message);
            return;
        }

        string projectRoot = Path.GetFullPath(Path.Combine(Application.dataPath, ".."));
        string manifestPath = Path.Combine(projectRoot, "Packages", "manifest.json");
        if (!File.Exists(manifestPath))
        {
            LogError("Не найден manifest.json по пути: " + manifestPath);
            return;
        }

        string manifestText = File.ReadAllText(manifestPath);
        JObject manifestJson = JObject.Parse(manifestText);

        var deps = manifestJson["dependencies"] as JObject;
        if (deps == null)
        {
            LogError("В manifest.json нет секции 'dependencies'");
            return;
        }

        string normalizedPath = packageFolder.Replace('\\', '/');
        if (!normalizedPath.StartsWith("file:", StringComparison.OrdinalIgnoreCase))
        {
            normalizedPath = "file:" + normalizedPath;
        }

        deps[nameValue] = normalizedPath;

        File.WriteAllText(manifestPath, manifestJson.ToString());
        Log($"manifest.json обновлён: \"{nameValue}\": \"{normalizedPath}\"");

        AssetDatabase.Refresh();
    }

    private bool CheckGitAvailable()
    {
        try
        {
            var processInfo = new ProcessStartInfo("git", "--version")
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true
            };
            using (var p = Process.Start(processInfo))
            {
                if (p == null)
                    return false;

                p.WaitForExit(2000);
                return (p.ExitCode == 0);
            }
        }
        catch
        {
            return false;
        }
    }

    private void Log(string msg)
    {
        lock (logBuilder)
        {
            logBuilder.AppendLine("[INFO] " + msg);
        }
    }

    private void LogWarning(string msg)
    {
        lock (logBuilder)
        {
            logBuilder.AppendLine("[WARN] " + msg);
        }
    }

    private void LogError(string msg)
    {
        lock (logBuilder)
        {
            logBuilder.AppendLine("[ERROR] " + msg);
        }
    }

    private void ForceRepaint()
    {
        Repaint();
    }
    
    private static class Remover
    {
        public static void Remove()
        {
            var assemblyPath = GetAsmdefPathByAssemblyName("LightSideCoreInstaller");

            if (!string.IsNullOrEmpty(assemblyPath))
            {
                var path = Path.GetDirectoryName(assemblyPath);
                if (Directory.Exists(path))
                {
                    Directory.Delete(path, true);
                    File.Delete($"{path}.meta");
                }
            }
        }
    
        private static string GetAsmdefPathByAssemblyName(string assemblyName)
        {
            string[] guids = AssetDatabase.FindAssets("t:AssemblyDefinitionAsset");
            foreach (var guid in guids)
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                string json = File.ReadAllText(path);
            
                AsmdefData data = JsonUtility.FromJson<AsmdefData>(json);
                if (data != null && data.name == assemblyName)
                {
                    return path;
                }
            }
        
            return null;
        }
    
        [Serializable]
        private class AsmdefData
        {
            public string name;
        }
    }
}
#endif
