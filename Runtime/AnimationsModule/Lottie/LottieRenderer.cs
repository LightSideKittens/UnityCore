using System;
using System.Linq;
using LSCore;
using LSCore.Extensions;
using Sirenix.OdinInspector;
#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using UnityEngine.UI;
using Sirenix.Utilities;
using UnityEngine.Rendering;

[ExecuteAlways]
[DisallowMultipleComponent]
[RequireComponent(typeof(MeshRenderer), typeof(MeshFilter))]
public sealed class LottieRenderer : MonoBehaviour
{
    private Mesh quad;
    private static Material unlitMat;
    private static readonly int mainTexId = Shader.PropertyToID("_MainTex");
    private static readonly LSVertexHelper vertexHelper = new();
    
    static LottieRenderer()
    {
        vertexHelper.Init();
#if UNITY_EDITOR
        Selection.selectionChanged += OnSelectionChanged;
        EditorApplication.update += OnEditorUpdate;

        void OnSelectionChanged()
        {
            EditorWorld.Updated -= OnUpdate;
            if (Selection.gameObjects.Any(x => x && x.GetComponent<LottieRenderer>()))
                EditorWorld.Updated += OnUpdate;
        }

        void OnEditorUpdate()
        {
            EditorApplication.update -= OnEditorUpdate;
            OnSelectionChanged();
        }
#endif
        World.Updated += OnUpdate;
    }

    private static Action updated;
    private static void OnUpdate() => updated?.Invoke();

    private const int MinSize = 64;
    private const int MaxSize = 2048;

    private LottieAnimation lottie;
    private MeshRenderer mr;
    private MeshFilter mf;
    private MaterialPropertyBlock mpb;

    private uint lastSize;
    private bool isVisible;
    private Vector3 lastScale;

    [SerializeField] [HideInInspector] private int rotateId = 0;
    [SerializeField] [HideInInspector] private int pixelsPerUnit = 128;
    [SerializeField] [HideInInspector] private Vector2Int flip;
    private bool verticesIsDirty;

    [HideInInspector] public BaseLottieAsset asset;
    [ShowInInspector]
    public BaseLottieAsset Asset
    {
        get => asset;
        set
        {
            if (value == asset) return;
            asset = value;
            DestroyLottie();
            CreateLottie();
            if (asset != null)
            {
                asset.SetupRenderer(this);
            }
            UpdatePlayState();
        }
    }
    
    [SerializeField] [HideInInspector] private Material material;
    [ShowInInspector]
    public Material Material
    {
        get => material;
        set
        {
            if(material == value) return;
            material = value;
            mr.sharedMaterial = value;
        }
    }

    [ShowInInspector]
    public int PixelsPerUnit
    {
        get => pixelsPerUnit;
        set
        {
            if(value == pixelsPerUnit) return;
            pixelsPerUnit = Mathf.Clamp(value, MinSize, MaxSize);
            RecreateIfNeeded();
        }
    }

    [ShowInInspector]
    [CustomValueDrawer("DrawFlip")]
    public (bool x, bool y) Flip
    {
        get => (flip.x.ToBool(), flip.y.ToBool());
        set
        {
            flip = new Vector2Int(value.x.ToInt(), value.y.ToInt());
            verticesIsDirty = true;
        }
    }

    [ShowInInspector]
    [CustomValueDrawer("DrawRotateButton")]
    public LSImage.RotationMode Rotation
    {
        get => (LSImage.RotationMode)rotateId;
        set
        {
            rotateId = (int)value;
            verticesIsDirty = true;
        }
    }

#if UNITY_EDITOR
    private (bool x, bool y) DrawFlip((bool x, bool y) value, GUIContent label)
    {
        EditorGUILayout.BeginHorizontal();

        var lbl = new GUIContent(label);
        Rect totalRect = EditorGUILayout.GetControlRect();
        Rect fieldRect = EditorGUI.PrefixLabel(totalRect, lbl);
        
        GUI.Label(fieldRect.TakeFromLeft(18), "X");
        var xFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), value.x);
        GUI.Label(fieldRect.TakeFromLeft(18), "Y");
        var yFlipValue = EditorGUI.Toggle(fieldRect.TakeFromLeft(25), value.y);
        Flip = (xFlipValue, yFlipValue);
        EditorGUILayout.EndHorizontal();
        return Flip;
    }

    private LSImage.RotationMode DrawRotateButton(LSImage.RotationMode value, GUIContent label)
    {
        GUILayout.Space(10);
        GUILayout.BeginHorizontal();

        for (int i = 0; i < 4; i++)
        {
            var targetAngle = i * 90;
            var text = rotateId == i ? $"{targetAngle}° ❤️" : $"{targetAngle}°";
            if (GUILayout.Button(text, GUILayout.Height(30)) && rotateId != i)
            {
                Rotation = (LSImage.RotationMode)i;
            }
        }

        GUILayout.EndHorizontal();
        return Rotation;
    }
#endif
    

    [SerializeField] [HideInInspector] private bool isEnabled = true;
    [ShowInInspector]
    public bool Enabled
    {
        get => isEnabled;
        set
        {
            if (isEnabled == value) return;
            isEnabled = value;
            UpdatePlayState();
        }
    }

    [SerializeField] [HideInInspector] private bool loop = true;
    [ShowInInspector]
    public bool Loop
    {
        get => loop;
        set
        {
            if(loop == value) return;
            if(lottie != null) lottie.loop = value;
            loop = value;
            if (value)
            {
                UpdatePlayState();
            }
        }
    }

    [SerializeField] [HideInInspector] private float speed = 1f;
    [ShowInInspector]
    public float Speed
    {
        get => speed;
        set
        {
            if(Mathf.Abs(speed - value) < 0.01f) return;
            speed = Mathf.Max(0f, value);
            UpdatePlayState();
        }
    }
    
    [ShowInInspector]
    private uint Size
    {
        get
        {
            var s = transform.localScale;
            var maxAxis = Mathf.Max(Mathf.Abs(s.x), Mathf.Abs(s.y));
            var px = Mathf.RoundToInt(maxAxis * pixelsPerUnit);
            var clamped = Mathf.Clamp(px, MinSize, MaxSize);
            var newSize = (uint)Mathf.ClosestPowerOfTwo(clamped);
            lastSize = newSize;
            return newSize;
        }
    }

    public bool IsEnded => !loop && (lottie.currentFrame >= lottie.TotalFramesCount - 1);
    public bool IsVisible
    {
        get { return enabled && gameObject.activeInHierarchy && isVisible; }
        set
        {
            if(isVisible == value) return;
            isVisible = value;
            UpdatePlayState();
        }
    }

    public bool IsPlaying { get; private set; }
    

    private void Awake()
    {
        Init();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        Init();
        mr.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
        mf.hideFlags = HideFlags.HideAndDontSave | HideFlags.HideInInspector;
    }
#endif

    private void Init()
    {
        mr ??= GetComp<MeshRenderer>();
        mr.allowOcclusionWhenDynamic = false;
        mr.lightProbeUsage = LightProbeUsage.Off;
        mr.reflectionProbeUsage = ReflectionProbeUsage.Off;
        mr.shadowCastingMode = ShadowCastingMode.Off;
        mr.receiveShadows = false;
        mf ??= GetComp<MeshFilter>();
        mpb ??= new MaterialPropertyBlock();

        if (quad == null) BuildUnitQuad();
        if (unlitMat == null) unlitMat = new Material(Shader.Find("Sprites/Default"));
        if (!Material) Material = unlitMat;
        if(mr.sharedMaterial == null) mr.sharedMaterial = Material;

        T GetComp<T>() where T : Component
        {
            T ret;
#if UNITY_EDITOR
            ret = GetComponent<T>();
            if (ret == null) ret = gameObject.AddComponent<T>();
#else
            ret = gameObject.AddComponent<T>();
#endif
            return ret;
        }
    }

    private void OnEnable()
    {
        IsPlaying = false;
        UpdatePlayState();
    }

    private void OnDisable() => UpdatePlayState();
    private void OnDestroy() => DestroyLottie();
    private void OnBecameVisible() => IsVisible = true;
    private void OnBecameInvisible() => IsVisible = false;

    private void UpdatePlayState()
    {
        var lastIsPlaying = IsPlaying;
        IsPlaying = IsVisible && isEnabled && asset != null && !IsEnded && speed > 0;
        if (IsPlaying == lastIsPlaying) return;

        if (IsPlaying)
        {
            if (lottie == null)
            {
                CreateLottie();
                lottie!.DrawOneFrame(0);
            }

            updated += Tick;
        }
        else
        {
            updated -= Tick;
        }
    }

    
    private void Tick()
    {
        if (verticesIsDirty)
        {
            BuildUnitQuad();
            verticesIsDirty = false;
        }

        var scale = transform.lossyScale;
        if (scale != lastScale)
        {
            if (lastSize != Size)
            {
                Recreate();
            }
        }

        lastScale = scale;

        lottie.LateUpdateFetch();
        lottie.UpdateAsync(speed);
        if (IsEnded) UpdatePlayState();
    }

    private void RecreateIfNeeded()
    {
        if (lastSize == Size) return; 
        Recreate();
    }
    
    private void Recreate()
    {
        if (lottie == null || !IsVisible) return;
        
        var lastFrame = lottie.currentFrame;
        DestroyLottie();
        CreateLottie();
        lottie.currentFrame = lastFrame;
    }

    private void OnTextureSwapped(Texture2D tex)
    {
        SetTextureOnRenderer(tex);
    }

    private void SetTextureOnRenderer(Texture tex)
    {
#if UNITY_EDITOR
        if (tex == null) return;
#endif
        mr.GetPropertyBlock(mpb);
        mpb.SetTexture(mainTexId, tex);
        mr.SetPropertyBlock(mpb);
    }

    private void CreateLottie()
    {
        lottie = new LottieAnimation(asset.Json, string.Empty, Size);
        lottie.OnTextureSwapped += OnTextureSwapped;
        lottie.loop = Loop;
    }

    private void DestroyLottie()
    {
        if (lottie == null) return;
        lottie.Destroy();
        lottie = null;
    }
    
    private void BuildUnitQuad()
    {
        quad ??= new Mesh { name = "LottieWorld_UnitQuad" };
        
        var vh = vertexHelper;
        var v = UIVertex.simpleVert;
        v.color = Color.white;

        v.position = new Vector3(-0.5f, -0.5f, 0f);
        v.uv0 = new Vector2(0f, 0f);
        vh.AddVert(v);

        v.position = new Vector3(-0.5f,  0.5f, 0f);
        v.uv0 = new Vector2(0f, 1f);
        vh.AddVert(v);

        v.position = new Vector3( 0.5f,  0.5f, 0f);
        v.uv0 = new Vector2(1f, 1f);
        vh.AddVert(v);

        v.position = new Vector3( 0.5f, -0.5f, 0f);
        v.uv0 = new Vector2(1f, 0f);
        vh.AddVert(v);

        vh.AddTriangle(0, 1, 2);
        vh.AddTriangle(0, 2, 3);

        RotateMesh(vh);
        vh.FillMesh(quad);
        vh.Clear();
        quad.RecalculateBounds();
        mf.sharedMesh = quad;
    }

    public delegate void RotateAction(ref Vector3 value, in Vector2 center);

    private RotateAction rotateAction;
    
    private void RotateMesh(LSVertexHelper vh)
    {
        if (rotateId == 0 && flip is { x: 0, y: 0 }) return;

        UIVertex vert = new UIVertex();
        var count = vh.currentVertCount;
        var center = new Vector2(0, 0);
        
        rotateAction = rotateId switch
        {
            1 => Rotate90,
            2 => Rotate180,
            3 => Rotate270,
            _ => null
        };

        rotateAction += Invert;

        for (int i = 0; i < count; i++)
        {
            vh.PopulateUIVertex(ref vert, i);
            var pos = vert.position;
            rotateAction(ref pos, center);
            vert.position = pos;
            vh.SetUIVertex(vert, i);
        }

        count = vh.currentIndexCount / 6 * 4;
        vh.ClearTriangles();

        if (rotateId % 2 == 1)
        {
            for (int i = 0; i < count; i += 4)
            {
                vh.AddTriangle(i + 1, i + 2, i + 3);
                vh.AddTriangle(i + 3, i, i + 1);
            }
        }
        else
        {
            for (int i = 0; i < count; i += 4)
            {
                vh.AddTriangle(i, i + 1, i + 2);
                vh.AddTriangle(i + 2, i + 3, i);
            }
        }

    }

    private void Invert(ref Vector3 pos, in Vector2 center)
    {
        float xOffset = pos.x - center.x;
        float yOffset = pos.y - center.y;

        if (flip.x == 1)
        {
            pos.x = -xOffset;
        }

        if (flip.y == 1)
        {
            pos.y = -yOffset;
        }
    }

    private void Rotate90(ref Vector3 pos, in Vector2 center)
    {
        var x = pos.x;
        pos.x = -pos.y + center.x;
        pos.y = x;
    }

    private void Rotate180(ref Vector3 pos, in Vector2 center)
    {
        pos.x = -pos.x + center.x;
        pos.y = -pos.y + center.y;
    }

    private void Rotate270(ref Vector3 pos, in Vector2 center)
    {
        var x = pos.x;
        pos.x = pos.y;
        pos.y = -x + center.y;
    }
}
