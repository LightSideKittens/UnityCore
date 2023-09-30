using System.Diagnostics;
using UnityEngine;

[Conditional("UNITY_EDITOR")]
public partial class ReferenceFromAttribute : PropertyAttribute
{
     public ReferenceFromAttribute(string prefabName)
     {
          Constructor(prefabName);
     }
     
     partial void Constructor(string prefabName);
}
