using System;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace LSCore
{
    [Serializable]
    public class DefaultLoader
    {
        public enum LoadType
        {
            Loop,
            Percent,
            Value
        }
        
        public LoadType loadType = LoadType.Loop;
        [Required]
        [LabelText("🔃 Loader")] [SerializeReference] public Loader loader;
        [LabelText("ℹ️⛔ Error")] [SerializeReference] public LoaderError error;
        
        public void Show(AsyncOperationHandle handle, Action retry)
        {
            var downloadStatus = handle.GetDownloadStatus();
            handle.OnSuccess(loader.Hide);
            handle.OnError(() => error?.Show(retry));
            
            switch (loadType)
            {
                case LoadType.Loop:
                    loader.ShowLoop();
                    break;
                case LoadType.Percent:
                    if (!downloadStatus.IsDone)
                    {
                        loader.ShowPercentProgress(out var progress);
                        World.Updated += OnProgress;
                        handle.OnComplete(() => World.Updated -= OnProgress);
                        
                        void OnProgress()
                        {
                            downloadStatus = handle.GetDownloadStatus();
                            progress(downloadStatus.Percent);
                        }
                    }
                    else
                    {
                        loader.ShowLoop();
                    }
                    break;
                case LoadType.Value:
                    if (!downloadStatus.IsDone)
                    {
                        loader.ShowValueProgress(downloadStatus.TotalBytes, out var progress);
                        World.Updated += OnProgress;
                        handle.OnComplete(() => World.Updated -= OnProgress);
                        
                        void OnProgress()
                        {
                            downloadStatus = handle.GetDownloadStatus();
                            progress(downloadStatus.DownloadedBytes);
                        }
                    }
                    else
                    {
                        loader.ShowLoop();
                    }
                    break;
            }   
        }
        
        public void Show(AsyncOperation handle)
        {
            handle.completed += x => loader.Hide();
            
            switch (loadType)
            {
                case LoadType.Loop:
                    loader.ShowLoop();
                    break;
                case LoadType.Percent:
                    if (!handle.isDone)
                    {
                        loader.ShowPercentProgress(out var progress);
                        World.Updated += OnProgress;
                        handle.completed += x => World.Updated -= OnProgress;
                        
                        void OnProgress()
                        {
                            progress(handle.progress);
                        }
                    }
                    break;
                case LoadType.Value:
                    if (!handle.isDone)
                    {
                        loader.ShowValueProgress(1, out var progress);
                        World.Updated += OnProgress;
                        handle.completed += x=> World.Updated -= OnProgress;
                        
                        void OnProgress()
                        {
                            progress(handle.progress);
                        }
                    }
                    break;
            }   
        }

        public void OnResult(bool success)
        {
            loader.Hide();
            
            if (!success)
            {
                error?.Show();
            }
        }
    }
}