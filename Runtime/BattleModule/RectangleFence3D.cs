using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class RectangleFence3D : MonoBehaviour
{
    public float width = 10f;
    public float height = 10f;
    public float thickness = 2f;
    public float thicknessY = 2f;
    
    private readonly List<BoxCollider> colliders = new();
    
    private void Init()
    {
        CreateBorder("Top");
        CreateBorder("Bottom");
        CreateBorder("Left");
        CreateBorder("Right");
    }

    private void UpdateBorders()
    {
        var halfWidth = width / 2;
        var halfHeight = height / 2;
        var halfThickness = thickness / 2;
        
        AdjustBorder(colliders[0], new Vector2(-halfThickness, halfHeight + halfThickness), new Vector3(width + thickness, thickness, thicknessY));
        AdjustBorder(colliders[1], new Vector2(halfThickness, -halfHeight - halfThickness), new Vector3(width + thickness, thickness, thicknessY));
        AdjustBorder(colliders[2], new Vector2(-halfWidth - halfThickness, -halfThickness), new Vector3(thickness, height + thickness, thicknessY));
        AdjustBorder(colliders[3], new Vector2(halfWidth + halfThickness, halfThickness), new Vector3(thickness, height + thickness, thicknessY));
    }

    private void OnEnable()
    {
        colliders.AddRange(GetComponentsInChildren<BoxCollider>());

        if (colliders.Count < 4)
        {
            Init();
            UpdateBorders();
        }
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        try { UpdateBorders(); }
        catch { }
    }
#endif
    

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = new Color(0.49f, 1f, 0.29f, 0.67f);

        for (var i = 0; i < colliders.Count ; i++)
        {
            var bounds = colliders[i].bounds;
            Gizmos.DrawCube(bounds.center, bounds.size);
        }
    }

    private void CreateBorder(string name)
    {
        var side = new GameObject(name);
        side.transform.SetParent(transform);
        side.transform.localPosition = Vector3.zero; 
        colliders.Add(side.AddComponent<BoxCollider>());
    }
    
    private void AdjustBorder(BoxCollider collider, Vector3 position, Vector3 size)
    {
        collider.size = size;
        collider.center = position;
    }
}