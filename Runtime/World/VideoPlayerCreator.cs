using System;
using System.IO;
using UnityEngine;
using UnityEngine.Video;

namespace LSCore
{
    public static class VideoPlayerCreator
    {
        /*public static VideoPlayer Create(string filePath, Action<VideoPlayer> onComplete = null)
        {
            if (!filePath.StartsWith("file://"))
            {
                filePath = "file:///" + filePath;
            }
            
            GameObject videoPlayerObj = new GameObject(Path.GetFileName(filePath));
            
            VideoPlayer videoPlayer = videoPlayerObj.AddComponent<VideoPlayer>();
            
            videoPlayer.source = VideoSource.Url;
            videoPlayer.url = filePath;
            videoPlayer.playOnAwake = false;
            videoPlayer.isLooping = true;
            videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            videoPlayer.Stop();
            videoPlayer.prepareCompleted += OnVideoPrepared;
            videoPlayer.Prepare();
            
            return videoPlayer;
            
            void OnVideoPrepared(VideoPlayer vp)
            {
                videoPlayer.prepareCompleted -= OnVideoPrepared;
                int videoWidth = (int)vp.width;
                int videoHeight = (int)vp.height;
                
                if (videoWidth <= 0 || videoHeight <= 0)
                {
                    videoWidth = 512;
                    videoHeight = 512;
                }
                
                var renderTexture = new RenderTexture(videoWidth, videoHeight, 0);
                renderTexture.Create();
                vp.targetTexture = renderTexture;
                onComplete?.Invoke(vp);
            }
        }*/
    }
}