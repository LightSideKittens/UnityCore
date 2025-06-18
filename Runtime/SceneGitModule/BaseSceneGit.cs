using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LSCore;
using LSCore.ConfigModule;
using LSCore.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;

public class BaseSceneGit<T> : SingleService<T> where T : BaseSceneGit<T>
{
    public const string Branches = "branches";
    public const string OnCommit = "onCommit";
    public const string Commits = "commits";
    public const string Changes = "changes";
    public const string ChangesForUndo = "changesForUndo";
    public const string CurrentBranchKey = "currentBranch";
    public const string CurrentCommitKey = "currentCommit";
    public const string PathKey = "path";
    public const string PathSeparator = "/";
    public const string RootFolder = "SceneGit";
    
    public Branch.SetCurrent rootBranch;
    
    private string configName;
    public static JToken Config => Manager.Config.data;
    public static JTokenGameConfigManager GetManager(string path) => 
        JTokenGameConfig.GetManager(Path.Combine(RootFolder, Instance.configName, path));

    public static JTokenGameConfigManager GetBranchManager(string path) => GetManager(Path.Combine("Branches", path));
    public static JTokenGameConfigManager Manager => GetManager(string.Empty);
    public static JToken GetBranchConfig(string branchName) => GetBranchManager(branchName).Config.data;
    public static bool IsBranchConfigFileExist(string branchName) => GetBranchManager(branchName).IsFileExists;
    public static bool IsBranchAssetChanged(string branchName) => Addressables.GetDownloadSizeAsync(branchName).WaitForCompletion() > 0;
    public static bool NeedLoadBranchAsset(string branchName) => !IsBranchConfigFileExist(branchName) || IsBranchAssetChanged(branchName);

    public static void SaveBranchToConfig(Branch branch)
    {
        branch = Instantiate(branch);
        var config = GetBranchConfig(branch.name);
        config[Commits] = JArray.FromObject(branch.commits, Serializer);
        GetBranchManager(branch.name).Save();
    }
    
    public static IList<Commit> GetCommits(string branch)
    {
        var config = GetBranchConfig(branch);
        return config[Commits].ToObject<List<Commit>>(Serializer);
    }
    
    public static string CurrentBranch
    {
        get => Config[CurrentBranchKey]?.ToString();
        set => Config[CurrentBranchKey] = value;
    }

    public static int CurrentCommit
    {
        get => Config[CurrentCommitKey].As(-1);
        set => Config[CurrentCommitKey] = value;
    }

    private static JsonSerializer serializer;
    private static JsonSerializer Serializer
    {
        get
        {
            if (serializer == null)
            {
                var settings = ConfigSerializationSettings.CreateDefault();
                settings.TypeNameHandling = TypeNameHandling.Auto;
                serializer = JsonSerializer.Create(settings);
            }

            return serializer;
        }
    }
    
    private static IEnumerable<Action<Action>> ChangesActions => 
        Config.AsJ<JObject>(Changes).Properties().Values()
            .Select(DeserializeChange)
            .Select(change => (Action<Action>)change.Do);

    public static void Initialize()
    {
        var instance = Instance;
    }

    protected override void Init()
    {
        base.Init();
        configName = gameObject.scene.name;
        
        if (!rootBranch.HasBranch)
        {
            GetBranch(rootBranch.branch);
            rootBranch.Do();
            rootBranch.onSuccess.Add((DelegateDoIt)(() =>
            {
                
            }));
        }
        else
        {
            SetupScene();
        }
    }

    public static void SetupScene() => Commit.Do(ChangesActions);

    [Button]
    private void Init1()
    {
        configName = gameObject.scene.name;
        CreateBranch("branch-A", "root", 0);
        CreateBranch("branch-B", "root", 2);
        CreateBranch("branch-C", "root", 3);
        CreateBranch("branch-D", "branch-C", 4);
        CreateBranch("branch-E", "branch-D", 1);
        CreateBranch("branch-F", "branch-E", 4);
        CreateBranch("branch-G", "branch-B", 7);
        CreateBranch("branch-H", "branch-G", 4);
        Debug.Log(Config.ToString(Formatting.Indented));
        
        PrintPath("branch-H", "branch-F");
        PrintPath("branch-F", "branch-H");
        PrintPath("branch-C", "branch-F");
        PrintPath("branch-F", "branch-C");
        PrintPath("branch-F", "branch-E");
        PrintPath("branch-E", "branch-F");
    }

    private static void PrintPath(string from, string to)
    {
        var list = FindPath(from, to);

        Debug.Log(string.Join(" -> ", list));
    }
    
    public static bool HasBranch(string branchName)
    {
        return Config.AsJ<JObject>(Branches).ContainsKey(branchName);
    }
    
    public static void CreateBranch(string newBranchName, string ontoBranchName, int ontoCommit)
    {
        var ontoBranch = GetBranch(ontoBranchName);
        var newBranch = GetBranch(newBranchName);

        var path = ontoBranch.TryGetValue(PathKey, out var pathToken) 
            ? string.Concat(pathToken.ToString(), PathSeparator, ontoBranchName) 
            : ontoBranchName;

        newBranch[PathKey] = path;
        newBranch[OnCommit] = ontoCommit;
    }

    public static void PushCommit(string branch, Commit commit)
    {
        var commitJToken = JObject.FromObject(commit, Serializer);
        var branchConfig = GetBranchConfig(branch);
        branchConfig.AsJ<JArray>(Commits).Add(commitJToken);
    }

    public static Commit DeserializeCommit(JToken commit)
    {
        return commit.ToObject<Commit>(Serializer);
    }
    
    public static BaseChange DeserializeChange(JToken change)
    {
        return change.ToObject<BaseChange>(Serializer);
    }

    public static void SetChange(BaseChange change)
    {
        var changes = Config.AsJ<JObject>(Changes);
        var hash = change.Key;
        changes.Remove(hash);
        changes[hash] = change.FromObject(Serializer);
    }
    
    public static JToken GetChange(string key)
    {
        return Config.AsJ<JObject>(Changes)[key];
    }

    public static void SetLastChangeForUndo(BaseChange change)
    {
        var config = GetBranchConfig(change.Branch);
        var lastChange = GetChange(change.Key);
        config.AsJ<JObject>(ChangesForUndo)[change.Key] = new JObject
        {
            {"Branch", lastChange["Branch"]},
            {"Commit", lastChange["Commit"]},
            {"Index", lastChange["Index"]},
        };
    }
    
    public static LSList<(string branch, int from, int to)> FindPath(string from, string to)
    {
        var branches = Config.AsJ<JObject>(Branches);

        if (!branches.ContainsKey(from) || !branches.ContainsKey(to))
        {
            return null;
        }

        var result = new LSList<(string branch, int from, int to)>();
        var fromBranch = branches[from];
        var toBranch = branches[to];
        var fromParentsSet = string.Concat(fromBranch[PathKey].ToString(), PathSeparator, from).Split(PathSeparator[0]);
        var toParentsSet = string.Concat(toBranch[PathKey].ToString(), PathSeparator, to).Split(PathSeparator[0]);

        int i = 1;
        while (true)
        {
            if (i < fromParentsSet.Length && i < toParentsSet.Length)
            {
                var target = fromParentsSet[i];
                if (target != toParentsSet[i])
                {
                    break;
                }
                
                i++;
                continue;
            }
            
            break;
        }
        
        var fromIndex = -1;
        int toIndex;
        int j = fromParentsSet.Length - 1;
        for (; j >= i; j--)
        {
            var branch = fromParentsSet[j];
            toIndex = branches[branch][OnCommit].ToInt();
            result.Add((branch, fromIndex, toIndex));
            fromIndex = toIndex;
        }
        
        int k = i-1;
        
        for (; k < toParentsSet.Length - 1; k++)
        {
            var nextBranch = toParentsSet[k + 1];
            toIndex = branches[nextBranch][OnCommit].ToInt();
            result.Add((toParentsSet[k], fromIndex, toIndex));
            fromIndex = 0;
        }
        
        toIndex = -1;
        result.Add((toParentsSet[k], fromIndex, toIndex));
        
        return result;
    }

    
    public static JObject GetJObject(string propertyName) => Config.AsJ<JObject>(propertyName);
    
    private static JObject GetBranch(string branchName)
    {
        return Config.AsJ<JObject>(Branches).AsJ<JObject>(branchName);
    }
}