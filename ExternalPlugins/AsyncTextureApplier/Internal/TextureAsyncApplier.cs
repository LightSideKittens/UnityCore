using System;
using System.Collections.Generic;
using LSCore;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.Rendering;

#if UNITY_EDITOR
using UnityEditor;
[InitializeOnLoad]
#endif
public static class TextureAsyncApplier
{
    private static Dictionary<Texture, uint> idByTexture = new();
    private static HashSet<uint> applyingIds = new();
    
    public static uint RegisterOrGetId(this Texture2D texture)
    {
        if (!idByTexture.TryGetValue(texture, out var id))
        {
            unsafe
            {
                idByTexture[texture] = NativeBridge.RegisterHandle((IntPtr) texture.GetRawTextureData<byte>().GetUnsafeReadOnlyPtr());
            }
        }
        
        return id;
    }

    public static void Unregister(this Texture2D texture)
    {
        if (idByTexture.TryGetValue(texture, out var id))
        {
            NativeBridge.UnregisterHandle(id);
        }
    }
    
    public static void ApplyAsync(this Texture2D texture)
    {
        var id = texture.RegisterOrGetId();
        if(!applyingIds.Add(id)) return;
        commandBuffer.IssuePluginCustomTextureUpdateV2(NativeBridge.EventHandler_ptr, texture, id);
    }
    
    private static CommandBuffer commandBuffer;
    private static Camera registeredCamera;
    private static CameraEvent registeredEvent;
    private static int lastFrame = -1;
    
    private static bool IsUsingScriptableRenderPipeline => GraphicsSettings.currentRenderPipeline != null;

#if UNITY_EDITOR
    static TextureAsyncApplier()
    {
        World.Creating += () =>
        {
            commandBuffer?.Clear();
            applyingIds.Clear();
            registeredCamera = null;
            lastFrame = -1;
        };
        
        InitializeOnLoad();
    }
#endif
    
#if !UNITY_EDITOR
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
#endif
    private static void InitializeOnLoad()
    {
        if(commandBuffer != null) return;
        
        commandBuffer = new CommandBuffer
        {
            name = nameof(TextureAsyncApplier),
        };
        
        if (IsUsingScriptableRenderPipeline)
        {
            //RenderPipelineManager.beginContextRendering += CachedOnBeginContextRendering;
        }
        else
        {
            Camera.onPreRender += OnPreRender;
            Camera.onPostRender += cam =>
            {
                if (applyingIds.Count > 0)
                {
                    commandBuffer.Clear();
                    applyingIds.Clear();
                }
            };
        }
    }
    
    private static void OnPreRender(Camera camera)
    {
        int currentFrame = Time.frameCount;
        if (currentFrame != lastFrame)
        {
            lastFrame = currentFrame;
            OnPreRenderFirstCamera(camera);
        }
    }

    private static void OnPreRenderFirstCamera(Camera camera)
    {
        if (registeredCamera == null)
        {
            Setup();
        }
        else if(registeredCamera != camera)
        {
            registeredCamera.RemoveCommandBuffer(registeredEvent, commandBuffer);
            Setup();
        }

        void Setup()
        {
            registeredCamera = camera;
            registeredEvent = camera.GetFirstCameraEvent();
            registeredCamera.AddCommandBuffer(registeredEvent, commandBuffer);
        }
    }

    private static CameraEvent GetFirstCameraEvent(this Camera camera)
    {
        return camera.actualRenderingPath switch
        {
            RenderingPath.DeferredShading => CameraEvent.BeforeGBuffer,
            _ => CameraEvent.BeforeForwardOpaque
        };
    }
}
