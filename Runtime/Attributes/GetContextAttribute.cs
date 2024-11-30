using System;
using System.Diagnostics;
using Sirenix.OdinInspector;

[AttributeUsage(AttributeTargets.Field)]
[Conditional("UNITY_EDITOR")]
public class GetContextAttribute : ShowInInspectorAttribute { }