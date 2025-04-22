using System;
using System.Collections.Generic;
using LSCore.AnimationsModule;
using Sirenix.OdinInspector.Editor;
using UnityEngine;

public partial class MoveItWindow
{
    private static readonly
        Dictionary<Type, Action<string, OdinMenuTree, MoveItClip, MoveIt.Handler, CurvesEditor>>
        addItemActionsByType = new()
        {
            {
                typeof(Color), (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "r", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "g", green, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "b", blue, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "a", alpha, clip, handler, editor));
                }
            },
            {
                typeof(Vector3), (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", green, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "z", blue, clip, handler, editor));
                }
            },
            {
                typeof(Vector2), (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", green, clip, handler, editor));
                }
            },
            {
                typeof(float),
                (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "value", red, clip, handler, editor));
                }
            },
            /*{
                typeof(RotateAroundPoint.Data), (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", green, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "z", blue, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "offset_X", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "offset_Y", green, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "offset_Z", blue, clip, handler, editor));
                }
            },
            {
                typeof(TransformRotation.Data), (path, tree, clip, handler, editor) =>
                {
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "x", red, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "y", green, clip, handler, editor));
                    tree.AddMenuItemAtPath(path, new CurveItem(tree, "z", blue, clip, handler, editor));
                }
            },*/
        };
}