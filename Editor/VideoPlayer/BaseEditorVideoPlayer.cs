using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
 
[ExecuteInEditMode]
#if UNITY_EDITOR
[InitializeOnLoad]
#endif
public abstract class BaseEditorVideoPlayer<T> : MonoBehaviour where T : BaseEditorVideoPlayer<T>, new()
{
    public VideoPlayer videoPlayer;
    public CanvasGroup canvasGroup;
    public RawImage image;
    private static bool isPlaying;
    private static bool isInited;
    private static Dictionary<string, VideoClip> clips = new Dictionary<string, VideoClip>();
    public static int ClipsCount => clips.Count;
    private float clipLength;
    private float time;
    private readonly float animationSpeed = 2;
    private readonly float animationDuration = 2f;
    protected abstract string Folder { get; }
    protected abstract string Name { get; }

    static BaseEditorVideoPlayer()
    {
#if UNITY_EDITOR
        EditorApplication.update += OnReset;
#endif
    }

    private static void OnReset()
    {
        if (Application.isPlaying)
        {
            return;
        }
        
        var gameObject = FindObjectsOfType<T>();

        for (int i = 0; i < gameObject.Length; i++)
        {
            DestroyImmediate(gameObject[i].gameObject);
            DestroyImmediate(gameObject[i].canvasGroup.gameObject);
        }
        
        var go = new GameObject();
        var editorVideoPlayer = go.AddComponent<T>();
        
        var tempClips = Resources.LoadAll<VideoClip>($"LightGamesEditorVideo/{editorVideoPlayer.Folder}");

        for (int i = 0; i < tempClips.Length; i++)
        {
            var clip = tempClips[i];
            clips.Add(clip.name, clip);
        }

        DestroyImmediate(go);
        isInited = true;
#if UNITY_EDITOR
        EditorApplication.update -= OnReset;
#endif
    }
    
    public static void Play(int clipIndex, bool isLoop = true, float playbackSpeed = 1, float alpha = 1)
    {
        if (isPlaying || Application.isPlaying)
        {
            return;
        }

        if (!isInited)
        {
            OnReset();
        }
        
        var go = new GameObject();
        var editorVideoPlayer = go.AddComponent<T>();
        
        var clipName = $"{editorVideoPlayer.Name}{clipIndex}";
        
        if (clips.TryGetValue(clipName, out var clip))
        {
            Internal_Play(editorVideoPlayer, clip);
            go.name = clip.name;
        }
        else
        {
            Debug.LogError($"[{typeof(T)}] Video clip was not found by path Resources/LightGamesEditorVideo/{editorVideoPlayer.Folder}/{clipName}");
        }
    }

    private static void Internal_Play(T editorVideoPlayer, VideoClip clip, bool isLoop = true, float playbackSpeed = 1, float alpha = 1)
    {
        var go = editorVideoPlayer.gameObject;
        var videoPlayer = go.AddComponent<VideoPlayer>();
        editorVideoPlayer.videoPlayer = videoPlayer;
        videoPlayer.source = VideoSource.VideoClip;
        videoPlayer.playOnAwake = false;
        videoPlayer.waitForFirstFrame = false;
        videoPlayer.isLooping = isLoop;
        videoPlayer.skipOnDrop = true;
        videoPlayer.playbackSpeed = playbackSpeed;
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = Resources.Load<RenderTexture>("EditorVideo");
        videoPlayer.targetCameraAlpha = alpha;
        videoPlayer.targetCamera3DLayout = Video3DLayout.No3D;
        videoPlayer.aspectRatio = VideoAspectRatio.FitInside;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
        videoPlayer.clip = clip;
        
        var canvas = Instantiate(Resources.Load<Canvas>("EditorVideoCanvas"));
        canvas.transform.SetSiblingIndex(0);
        editorVideoPlayer.clipLength = (float)clip.length;
        editorVideoPlayer.image = canvas.transform.GetChild(0).GetChild(0).GetComponent<RawImage>();
        editorVideoPlayer.canvasGroup = canvas.GetComponent<CanvasGroup>();
        editorVideoPlayer.image.texture = videoPlayer.targetTexture;
        editorVideoPlayer.canvasGroup.alpha = 0;
        editorVideoPlayer.StartAnimate();
    }
    
    private void StartAnimate()
    {
        isPlaying = true;
        var color = Color.white; color.a = 0; image.color = color;
#if UNITY_EDITOR
        EditorApplication.update += FadeIn;
        EditorApplication.update += StopAnimate;
#endif
    }

    private void StopAnimate()
    {
        time += Time.deltaTime;
        
        if (time > animationDuration || time > clipLength)
        {
#if UNITY_EDITOR
            EditorApplication.update -= FadeIn;
            EditorApplication.update += FadeOut;
            EditorApplication.update -= StopAnimate;
#endif
        }
    }

    private void FadeIn()
    {
        canvasGroup.alpha += Time.deltaTime * animationSpeed;

        if (canvasGroup.alpha >= 0.99f)
        {
            var color = image.color; color.a += Time.deltaTime * animationSpeed; image.color = color;
        }
        
    }
    
    private void FadeOut()
    {
        var color = image.color; color.a -= Time.deltaTime * animationSpeed; image.color = color;
        
        if (image.color.a <= 0)
        {
            canvasGroup.alpha -= Time.deltaTime * animationSpeed;

            if (canvasGroup.alpha <= 0)
            {
#if UNITY_EDITOR
                EditorApplication.update -= FadeOut;
#endif
                DestroyImmediate(gameObject);
                DestroyImmediate(canvasGroup.gameObject);
                isPlaying = false; 
            }
        }
    }

    private void Awake()
    {
        if (Application.isPlaying)
        {
            Destroy(gameObject);
            Destroy(canvasGroup.gameObject);
        }
    }

    private void Start()
    {
        videoPlayer.Play();
    }
}