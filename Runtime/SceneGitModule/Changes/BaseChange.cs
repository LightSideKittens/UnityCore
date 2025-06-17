using System;

[Serializable]
public abstract class BaseChange
{
    public string Branch { get; set; }
    public int Commit { get; set; }
    public int Index { get; set; }
    public virtual string Key => GetType().FullName;
    
    public virtual void Do(Action onComplete)
    {
        onComplete();
        SceneGit.SetChange(this);
    }
    
    public virtual void Undo(Action onComplete)
    {
        
    }
    
    public virtual void Preload()
    {
        
    }
    
    public virtual void Unload()
    {
        
    }
}