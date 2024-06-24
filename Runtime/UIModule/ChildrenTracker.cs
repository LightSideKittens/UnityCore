using System;
using System.Collections.Generic;
using UnityEngine;

public class ChildrenTracker : MonoBehaviour
{
    private class ChildData
    {
        public Transform child;
        public int index;

        public ChildData(Transform child, int index)
        {
            this.child = child;
            this.index = index;
        }
    }
    public event Action<Transform> Enabled;
    public event Action<Transform> Disabled;
    public event Action<Transform> Added;
    public event Action<Transform> Removed;
    public event Action<Transform, int, int> SiblingIndexChanged;
    public event Action<Transform, int, int> SiblingIndexSet;
    public event Action<Transform, Transform> Swapped;
    public event Action ChildrenCountChanged;
    public event Action RectTransformDimensionsChanged;
    public event Action TransformChildrenChanged;
    public event Action ChildrenOrderChanged;
    private Dictionary<Transform, bool> activeSet = new();
    private LinkedList<ChildData> children = new();
    private int lastChildCount;

    void Awake()
    {
        UpdateChildrenSet();
        Added += OnAdded;
        Removed += OnRemoved;
        RectTransform.reapplyDrivenProperties += UpdateActiveSet;
    }

    private void OnDestroy()
    {
        RectTransform.reapplyDrivenProperties -= UpdateActiveSet;
    }

    private void OnRemoved(Transform child)
    {
        activeSet.Remove(child);
    }

    private void OnAdded(Transform child)
    {
        activeSet[child] = child.gameObject.activeSelf;
    }

    private void OnRectTransformDimensionsChange()
    {
        RectTransformDimensionsChanged?.Invoke();
    }

    private void UpdateActiveSet(RectTransform rect)
    {
        if (activeSet.TryGetValue(rect, out var activeSelf))
        {
            bool currentActive = rect.gameObject.activeSelf;
            activeSet[rect] = currentActive;
                
            if (activeSelf != currentActive)
            {
                if (currentActive)
                {
                    Enabled?.Invoke(rect);
                }
                else
                {
                    Disabled?.Invoke(rect);
                }
            }
        }
    }

    private Transform[] _swapped = new Transform[2];
    private void OnChildrenOrderChanged()
    {
        ChildrenOrderChanged?.Invoke();
        int countOfChanges = 0;
        var node = children.Last;

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            var data = node!.Value;
            
            if (child != data.child)
            {
                int index = data.child.GetSiblingIndex();
                int index2 = child.GetSiblingIndex();
                
                if (Math.Abs(index2 - index) == 1)
                {
                    do
                    {
                        node = node.Previous;
                        data = node!.Value;
                    } while (data.child != child);
                    
                    index = data.child.GetSiblingIndex();
                }
                
                children.Remove(node);
                if (index >= children.Count)
                {
                    children.AddLast(node);
                }
                else
                {
                    children.Insert(index, node);
                }
                
                break;
            }

            node = node.Previous;
        }

        node = children.First;
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            var data = node!.Value;
            int prevIndex = data.index;
            int abs = Math.Abs(prevIndex - i);
            
            if (abs > 1)
            {
                SiblingIndexSet?.Invoke(child, prevIndex, i);
            }
            
            if(abs != 0)
            {
                _swapped[countOfChanges % 2] = child;
                countOfChanges++;
                data.index = i;
                SiblingIndexChanged?.Invoke(child, prevIndex, i);
            }

            node = node.Next;
        }
        
        if (countOfChanges == 2)
        {
            Swapped?.Invoke(_swapped[0], _swapped[1]);
        }
    }

    private void OnTransformChildrenChanged()
    {
        TransformChildrenChanged?.Invoke();
        int childCount = transform.childCount;
        int childCountDiff = childCount - lastChildCount;
        
        if (lastChildCount != childCount)
        {
            lastChildCount = childCount;
            ChildrenCountChanged?.Invoke();
        }
        else
        {
            OnChildrenOrderChanged();
            return;
        }
        
        if (childCountDiff > 0)
        {
            int index = childCount - 1;
            var child = transform.GetChild(index);
            children.AddLast(new ChildData(child, index));
            Added?.Invoke(child);
            return;
        }
        
        var node = children.First;

        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            
            if (child == node!.Value.child)
            {
                node = node!.Next;
                continue;
            }
            
            break;
        }

        var nextNode = node!.Next;
        children.Remove(node!);

        while (nextNode != null)
        {
            nextNode!.Value.index--;
            nextNode = nextNode.Next;
        } 
        
        Removed?.Invoke(node.Value.child);
    }

    private void UpdateChildrenSet()
    {
        children.Clear();
        lastChildCount = transform.childCount;

        for (int i = 0; i < lastChildCount; i++)
        {
            Transform child = transform.GetChild(i);
            children.AddLast(new ChildData(child, i));
            activeSet.Add(child, child.gameObject.activeSelf);
        }
    }
}