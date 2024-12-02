using System;
using System.ComponentModel;
using System.Diagnostics;
using Sirenix.OdinInspector;

#nullable disable

/// <summary>
/// <para>
///        OnValueChanged works on properties and fields, and calls the specified function
///        whenever the value has been changed via the inspector.
/// </para>
/// </summary>
/// <remarks>
/// <note type="note">Note that this attribute only works in the editor! Properties changed by script will not call the function.</note>
/// </remarks>
/// <example>
/// <para>The following example shows how OnValueChanged is used to provide a callback for a property.</para>
/// <code>
/// public class MyComponent : MonoBehaviour
/// {
/// 	[OnValueChanged("MyCallback")]
/// 	public int MyInt;
/// 
/// 	private void MyCallback()
/// 	{
/// 		// ..
/// 	}
/// }
/// </code>
/// </example>
/// <example>
/// <para>The following example show how OnValueChanged can be used to get a component from a prefab property.</para>
/// <code>
/// public class MyComponent : MonoBehaviour
/// {
/// 	[OnValueChanged("OnPrefabChange")]
/// 	public GameObject MyPrefab;
/// 
/// 	// RigidBody component of MyPrefab.
/// 	[SerializeField, HideInInspector]
/// 	private RigidBody myPrefabRigidbody;
/// 
/// 	private void OnPrefabChange()
/// 	{
/// 		if(MyPrefab != null)
/// 		{
/// 			myPrefabRigidbody = MyPrefab.GetComponent&lt;Rigidbody&gt;();
/// 		}
/// 		else
/// 		{
/// 			myPrefabRigidbody = null;
/// 		}
/// 	}
/// }
/// </code>
/// </example>
[DontApplyToListElements]
[AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = true)]
[Conditional("UNITY_EDITOR")]
public sealed class LSOnValueChangedAttribute : Attribute
{
    /// <summary>
    /// A resolved string that defines the action to perform when the value is changed, such as an expression or method invocation.
    /// </summary>
    public string Action;

    /// <summary>
    /// Whether to perform the action when a child value of the property is changed.
    /// </summary>
    public bool IncludeChildren;
    
    public string[] Parameters;

    /// <summary>
    /// Whether to perform the action when an undo or redo event occurs via UnityEditor.Undo.undoRedoPerformed. True by default.
    /// </summary>
    public bool InvokeOnUndoRedo = true;

    /// <summary>
    /// Whether to perform the action when the property is initialized. This will generally happen when the property is first viewed/queried (IE when the inspector is first opened, or when its containing foldout is first expanded, etc), and whenever its type or a parent type changes, or it is otherwise forced to rebuild.
    /// </summary>
    public bool InvokeOnInitialize;

    /// <summary>
    /// Name of callback member function. Obsolete; use the Action member instead.
    /// </summary>
    [Obsolete("Use the Action member instead.", false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public string MethodName
    {
        get => Action;
        set => Action = value;
    }

    /// <summary>
    /// Adds a callback for when the property's value is changed.
    /// </summary>
    /// <param name="action">A resolved string that defines the action to perform when the value is changed, such as an expression or method invocation.</param>
    /// <param name="includeChildren">Whether to perform the action when a child value of the property is changed.</param>
    public LSOnValueChangedAttribute(string action, bool includeChildren = false, params string[] parameters)
    {
        Action = action;
        IncludeChildren = includeChildren;
        Parameters = parameters;
    }
}