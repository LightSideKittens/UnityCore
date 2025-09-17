#if UNITY_EDITOR
using System;
using System.IO;
using DG.Tweening.Plugins.Core.PathCore;
using UnityEditor;
using UnityEditor.AssetImporters;
using UnityEngine;
using Path = System.IO.Path;

[CustomEditor(typeof(LottieScriptedImporter), true)]
public sealed class LottieScriptedImporterEditor : ScriptedImporterEditor
{
    private float speed = 1;
    private bool loop = true;
    private int rotateId;
    private (bool x, bool y) flip;

    private LottieAnimation anim;
    private bool playing;
    private double lastUpdateTime;
    private int lastW, lastH;
    private int lastRotationIndex = -1;

    private BaseLottieAsset asset;
    private string assetPath;
    private string cachedJson;

    public override void OnEnable()
    {
        base.OnEnable();
        
        assetPath   = ((AssetImporter)target).assetPath;
        asset = AssetDatabase.LoadAssetAtPath<BaseLottieAsset>(assetPath);
        cachedJson  = asset.Json;
        rotateId = (int)asset.Rotation;
        flip = asset.Flip;
        playing = true;
        lastUpdateTime = EditorApplication.timeSinceStartup;
    }

    public override void OnDisable()
    {
        DisposeAnim();
        base.OnDisable();
    }

    protected override bool needsApplyRevert => false;
    protected override bool useAssetDrawPreview => false;
    public override bool showImportedObject  => false;
    public override bool HasPreviewGUI() => true;
    public override GUIContent GetPreviewTitle() => GUIContent.none;
    public override bool RequiresConstantRepaint() => playing;
    
    public override void OnInspectorGUI()
    {
        DrawFlip();
        DrawRotateButton();
        if (asset.IsCompressed)
        {
            if (GUILayout.Button("Decompress"))
            {
                var json = LottieCompressor.Decompress(File.ReadAllBytes(assetPath));
                var newPath = Path.ChangeExtension(assetPath, asset.DecompressedExtension);
                File.WriteAllText(newPath, json);
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }
        else
        {
            if (GUILayout.Button("Compress"))
            {
                var bytes = LottieCompressor.Compress(File.ReadAllText(assetPath));
                var newPath = Path.ChangeExtension(assetPath, asset.CompressedExtension);
                File.WriteAllBytes(newPath, bytes);
                AssetDatabase.DeleteAsset(assetPath);
                AssetDatabase.Refresh();
            }
        }
    }
    
    private void DrawFlip()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Flip", GUILayout.ExpandWidth(true));
        GUI.color = new Color(1, 1, 1, flip.x ? 1f : 0.5f);
        if (GUILayout.Button(flip.x ? "X ❤️" : "X", GUILayout.Height(30)))
        {
            flip.x = !flip.x;
        }
        GUI.color = new Color(1, 1, 1, flip.y ? 1f : 0.5f);
        
        if (GUILayout.Button(flip.y ? "Y ❤️" : "Y", GUILayout.Height(30)))
        {
            flip.y = !flip.y;
        }
        GUI.color = new Color(1, 1, 1, 1);
        GUILayout.EndHorizontal();
    }
    
    private void DrawRotateButton()
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 4; i++)
        {
            var targetAngle = i * 90;
            var text = rotateId == i ? $"{targetAngle}° ❤️" : $"{targetAngle}°";
            if (GUILayout.Button(text, GUILayout.Height(30)) && rotateId != i)
            {
                rotateId = i;
            }
        }

        GUILayout.EndHorizontal();
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if(Event.current.type != EventType.Repaint) return;
        
        EnsureAnimationForPreviewRect(r);
        EditorUpdate();
        if (anim?.Texture == null)
        {
            EditorGUI.DropShadowLabel(r, "No texture from LottieAnimation.");
            return;
        }

        int angleDeg = rotateId * 90;
        bool swap = angleDeg is 90 or 270;

        Rect drawRect = r;
        if (swap)
        {
            drawRect = new Rect(0, 0, r.height, r.width);
            drawRect.center = r.center;
        }

        var scale = new Vector2(flip.x ? -1 : 1, flip.y ? -1 : 1);
        var prev = GUI.matrix;
        GUIUtility.RotateAroundPivot(angleDeg, r.center);
        GUIUtility.ScaleAroundPivot(scale, r.center);
        EditorGUI.DrawPreviewTexture(drawRect, anim.Texture, null, ScaleMode.ScaleToFit);
        GUI.matrix = prev;
    }

    public override void OnPreviewSettings()
    {
        if (GUILayout.Button(playing ? "Pause" : "Play", EditorStyles.miniButton))
        {
            if (anim == null) EnsureAnimationForPreviewRect(GUILayoutUtility.GetLastRect());
            playing = !playing;
            lastUpdateTime = EditorApplication.timeSinceStartup;
        }

        if (GUILayout.Button("Stop", EditorStyles.miniButton))
        {
            playing = false;
            anim?.DrawOneFrame(0);
        }
        
        loop = GUILayout.Toggle(loop, "Loop", EditorStyles.miniButton);
        
        GUILayout.Label("Speed", GUILayout.Width(40));
        speed = GUILayout.HorizontalSlider(speed, 0f, 4f, GUILayout.Width(100));
        
        if (anim != null)
        {
            anim.loop = loop;
            int total = Mathf.Max(1, (int)anim.TotalFramesCount);
            int cur = Mathf.Clamp((int)anim.currentFrame, 0, total - 1);

            GUILayout.Space(10);
            GUILayout.Label($"Frame {cur+1}/{total}", GUILayout.Width(110));
            int newFrame = Mathf.RoundToInt(GUILayout.HorizontalSlider(cur, 0, total - 1, GUILayout.Width(150)));
            if (newFrame != cur)
            {
                playing = false;
                anim.DrawOneFrame(newFrame);
                anim.currentFrame = newFrame;
            }
        }
    }

    private void EditorUpdate()
    {
        if (!playing || anim == null) return;

        double t = EditorApplication.timeSinceStartup;
        float delta = (float)(t - lastUpdateTime);
        lastUpdateTime = t;
        
        anim.UpdateDelta(delta * speed);

        if (!loop && anim.currentFrame >= anim.TotalFramesCount - 1)
            playing = false;

        Repaint();
    }

    private void EnsureAnimationForPreviewRect(Rect r)
    {
        int angleIdx = rotateId;
        bool swap = angleIdx == 1 || angleIdx == 3;
        int w = Mathf.Max(1, Mathf.RoundToInt(swap ? r.height : r.width));
        int h = Mathf.Max(1, Mathf.RoundToInt(swap ? r.width  : r.height));

        bool needRecreate = anim == null || w != lastW || h != lastH || angleIdx != lastRotationIndex;
        if (!needRecreate) return;
        
        var lastTime = anim?.timeSinceLastRenderCall ?? 0;
        var frame = anim?.currentFrame ?? 0;
        DisposeAnim();

        anim = new LottieAnimation(
            cachedJson,
            string.Empty,
            (uint)Mathf.Min(w, h));

        anim.timeSinceLastRenderCall = lastTime;
        double t = EditorApplication.timeSinceStartup;
        float delta = (float)(t - lastUpdateTime);
        lastUpdateTime = t;
        anim.DrawOneFrame(frame);
        anim.currentFrame = frame;
        anim.UpdateDelta(delta * speed);

        lastW = w; lastH = h; lastRotationIndex = angleIdx;
        lastUpdateTime = EditorApplication.timeSinceStartup;
    }

    private void DisposeAnim()
    {
        if (anim != null) { anim.Destroy(); anim = null; }
    }
}
#endif
