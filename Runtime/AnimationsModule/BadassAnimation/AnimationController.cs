using System.Collections.Generic;
using System.Diagnostics;
using UnityEditor;
using UnityEngine;

public interface IAnimatable
{
    void BeforeEvaluate();
    IEnumerable<IEvaluator> Evaluators { get; }
    void AfterEvaluate();
}


public interface IEvaluator
{
    void Evaluate();
}

public class AnimationController : MonoBehaviour, IAnimatable
{
    public int count;
    public BadassAnimation.EvaluateData data;

    private void OnEnable()
    {
        BadassAnimation.Register(this);
    }

    private void OnDisable()
    {
        BadassAnimation.Unregister(this);
    }

    Stopwatch sw;
    
    public void BeforeEvaluate()
    {
        data.x += Time.deltaTime;   
    }

    public IEnumerable<IEvaluator> Evaluators
    {
        get
        {
            yield return data;
        }
    }
    
    public void AfterEvaluate()
    {
        var pos = transform.localPosition;
        pos.x = data.y;
        transform.localPosition = pos;
    }
}