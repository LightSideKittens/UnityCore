using System.Collections;
using System.Reflection;
using UnityEditor;
using UnityEngine;

namespace LSCore.Sirenix.Utilities.Editor
{
  /// <summary>
  /// Collection of extension methods for <see cref="T:UnityEditor.GenericMenu" />.
  /// </summary>
  public static class LSGenericMenuExtensions
  {
    private static readonly FieldInfo GenericMenu_MenuItems = typeof (GenericMenu).GetField("m_MenuItems", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
    private static readonly FieldInfo MenuItem_Content;
    private static readonly FieldInfo MenuItem_Func;
    private static readonly FieldInfo MenuItem_Func2;
    private static readonly FieldInfo MenuItem_On;

    static LSGenericMenuExtensions()
    {
      System.Type nestedType = typeof (GenericMenu).GetNestedType("MenuItem", BindingFlags.Public | BindingFlags.NonPublic);
      if (nestedType != null)
      {
        MenuItem_Content = nestedType.GetField("content", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MenuItem_Func = nestedType.GetField("func", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MenuItem_Func2 = nestedType.GetField("func2", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        MenuItem_On = nestedType.GetField("on", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
      }
      if (GenericMenu_MenuItems == null)
        Debug.LogError("Could not find private Unity member GenericMenu.menuItems in this version of Unity. Some Odin functionality may be disabled.");
      if (MenuItem_Content == null)
        Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.content in this version of Unity. Some Odin functionality may be disabled.");
      if (MenuItem_Func == null)
        Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.func in this version of Unity. Some Odin functionality may be disabled.");
      if (MenuItem_Func2 == null)
        Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.func2 in this version of Unity. Some Odin functionality may be disabled.");
      if (!(MenuItem_On == null))
        return;
      Debug.LogError("Could not find private Unity member GenericMenu.MenuItem.on in this version of Unity. Some Odin functionality may be disabled.");
    }

    /// <summary>
    /// Removes all menu items with a given name from the GenericMenu.
    /// </summary>
    /// <param name="menu">The GenericMenu to remove items from.</param>
    /// <param name="name">The name of the items to remove.</param>
    /// <returns>True if any items were removed, otherwise false.</returns>
    public static bool RemoveItems(this GenericMenu menu, string name)
    {
      if (GenericMenu_MenuItems == null || MenuItem_Content == null)
      {
        Debug.LogWarning("Cannot remove menu item from GenericMenu, as private Unity members were missing.");
        return false;
      }
      IList arrayList = (IList) GenericMenu_MenuItems.GetValue(menu);
      bool flag = false;
      for (int index = 0; index < arrayList.Count; ++index)
      {
        object obj = arrayList[index];
        if (((GUIContent) MenuItem_Content.GetValue(obj)).text == name)
        {
          arrayList.RemoveAt(index--);
          flag = true;
        }
      }
      return flag;
    }

    /// <summary>
    /// Replaces the first found menu item with a given name with a new menu item, or if no such element is found, adds a new one.
    /// </summary>
    /// <param name="menu">The GenericMenu to replace items in.</param>
    /// <param name="name">The name of the items to remove.</param>
    /// <param name="func">The func to replace or add.</param>
    /// <param name="on">The on value to set the new menu item with.</param>
    /// <returns>True if an item was replaced, otherwise false.</returns>
    public static bool ReplaceOrAdd(
      this GenericMenu menu,
      string name,
      bool on,
      GenericMenu.MenuFunction func)
    {
      if (GenericMenu_MenuItems == null || MenuItem_Content == null || MenuItem_Func == null || MenuItem_Func2 == null || MenuItem_On == null)
      {
        Debug.LogWarning("Cannot replace menu items in GenericMenu, as private Unity members were missing.");
        return false;
      }
      ArrayList arrayList = (ArrayList) GenericMenu_MenuItems.GetValue(menu);
      bool flag = false;
      for (int index = 0; index < arrayList.Count; ++index)
      {
        object obj = arrayList[index];
        if (((GUIContent) MenuItem_Content.GetValue(obj)).text == name)
        {
          MenuItem_Func.SetValue(obj, func);
          MenuItem_Func2.SetValue(obj, null);
          MenuItem_On.SetValue(obj, on);
          flag = true;
          break;
        }
      }
      if (!flag)
        menu.AddItem(new GUIContent(name), on, func);
      return flag;
    }

    /// <summary>
    /// Replaces the first found menu item with a given name with a new menu item, or if no such element is found, adds a new one.
    /// </summary>
    /// <param name="menu">The GenericMenu to replace items in.</param>
    /// <param name="name">The name of the items to remove.</param>
    /// <param name="on">The on value to set the new menu item with.</param>
    /// <param name="func2">The func to replace or add.</param>
    /// <param name="userData">The user data.</param>
    /// <returns>True if an item was replaced, otherwise false.</returns>
    public static bool ReplaceOrAdd(
      this GenericMenu menu,
      string name,
      bool on,
      GenericMenu.MenuFunction2 func2,
      object userData)
    {
      if (GenericMenu_MenuItems == null || MenuItem_Content == null || MenuItem_Func == null || MenuItem_Func2 == null || MenuItem_On == null)
      {
        Debug.LogWarning("Cannot replace menu items in GenericMenu, as private Unity members were missing.");
        return false;
      }
      ArrayList arrayList = (ArrayList) GenericMenu_MenuItems.GetValue(menu);
      bool flag = false;
      for (int index = 0; index < arrayList.Count; ++index)
      {
        object obj = arrayList[index];
        if (((GUIContent) MenuItem_Content.GetValue(obj)).text == name)
        {
          MenuItem_Func.SetValue(obj, null);
          MenuItem_Func2.SetValue(obj, func2);
          MenuItem_On.SetValue(obj, on);
          flag = true;
          break;
        }
      }
      if (!flag)
        menu.AddItem(new GUIContent(name), on, func2, userData);
      return flag;
    }
  }
}
