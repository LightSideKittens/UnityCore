using System;
using System.Collections.Generic;
using System.Linq;
using LSCore;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class Branch : MonoBehaviour
{
    [Serializable]
    public class PreloadBranch : DoIt
    {
        public ComponentAssRef<Branch> branch;
        
        public override void Do()
        {
            
        }
    }
    
    [Serializable]
    public class SetCurrent : DoIt
    {
        public string branch;
        public DefaultLoader loader;
        [MinValue(0)] public int commit;
        [SerializeReference] public List<DoIt> onSuccess = new();
        [SerializeReference] public List<DoIt> onError = new();
        
        public bool HasBranch => SceneGit.HasBranch(branch);
        
        public override void Do()
        {
            var lastBranch = SceneGit.CurrentBranch;
            var lastCommit = SceneGit.CurrentCommit;
            var branchName = branch;
            
            AsyncOperationHandle handle = default;

            if (lastCommit != -1 && lastBranch != branchName)
            {
                var path = SceneGit.FindPath(lastBranch, branchName);
                path[0].from = lastCommit;
                path[^1].to = commit;
                
                List<ComponentAssRef<Branch>> refs = new ();
                for (int i = 0; i < path.Count; i++)
                {
                    var name = path[i].branch;
                    
                    if (SceneGit.NeedLoadBranchAsset(name))
                    {
                        refs.Add(new ComponentAssRef<Branch>(name));
                    }
                }

                if (refs.Count > 0)
                {
                    var handle2 = Addressables.LoadAssetsAsync<Branch>(refs, null, Addressables.MergeMode.Union);
                    handle = handle2;
                    handle2.OnSuccess(branches =>
                    {
                        foreach (var b in branches)
                        {
                            SceneGit.SaveBranchToConfig(b);
                        }
                        
                        DoChanges(path);
                    });
                }
                else
                {
                    DoChanges(path);
                }
            }
            else if (lastCommit != commit)
            {
                if (SceneGit.NeedLoadBranchAsset(branchName))
                {
                    var handle2 = new ComponentAssRef<Branch>(branch).LoadAsync();
                    handle = handle2;
                    handle2.OnSuccess(b =>
                    {
                        SceneGit.SaveBranchToConfig(b);
                        DoChanges(new[]{(branchName, lastCommit, commit)});
                    });
                }
                else
                {
                    DoChanges(new[]{(branchName, lastCommit, commit)});
                }
            }

            if (handle.IsValid())
            {
                handle.OnSuccess(OnCheckout);
                Load(handle);
            }
            else
            {
                OnCheckout();
            }

            void OnCheckout()
            {
                SceneGit.CurrentBranch = branchName;
                SceneGit.CurrentCommit = commit;
            }

            void DoChanges(IList<(string branch, int from, int to)> path)
            {
                var commits = new List<IList<Commit>>();
                    
                for (int i = 0; i < path.Count; i++)
                {
                    var name = path[i].branch;
                    commits.Add(SceneGit.GetCommits(name));
                }
                
                var changes = new Dictionary<string, (Action<Action> change, int order)>();
                    
                for (var i = 0; i < commits.Count; i++)
                {
                    var b = commits[i];
                    var part = path[i];

                    var sign = Math.Sign(part.to - part.from);
                    bool useDo = sign == 1;
                    for (int j = Math.Max(0, part.from); j <= part.to; j += sign)
                    {
                        var c = b[j];
                        for (int k = 0; k < c.changes.Count; k++)
                        {
                            var change = c.changes[k];
                            changes[change.Key] = (useDo ? change.Do : change.Undo, changes.Count);
                        }
                    }
                }

                Commit.Do(changes.OrderBy(kvp => kvp.Value.order).Select(kvp => kvp.Value.change));
            }
        }
        
        public void Load(AsyncOperationHandle handle)
        {
            Load();
            void Load()
            {
                handle.OnSuccess(onSuccess.Do);
                handle.OnError(onError.Do);
                loader.Show(handle, Load);
            }
        }

        public SetCurrent(){}
        
        public SetCurrent(string branch, int commit)
        {
            this.branch = branch;
            this.commit = commit;
        }
    }

    public List<Commit> commits;
    
    private void Awake()
    {
        name = name.Replace("(Clone)", string.Empty);
        var branchName = name;
        
        for (var i = 0; i < commits.Count; i++)
        {
            var commit = commits[i];
            commit.Branch = branchName;
            commit.Index = i;
            commit.Load();
        }
    }

    private void OnDestroy()
    {
        foreach (var step in commits)
        {
            step.Unload();
        }
    }
}