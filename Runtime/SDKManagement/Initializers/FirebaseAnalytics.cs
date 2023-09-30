#if FIREBASE_ANALYTICS
using System;
using UnityEngine;
using static Firebase.Analytics.FirebaseAnalytics;
using static UnityEngine.SystemInfo;

namespace LSCore.SDKManagement
{
    public partial class SDKInitializer
    {
        [Serializable]
        public class FirebaseAnalytics : Base
        {
            protected override void Internal_Init(Action<string> onComplete)
            {
                SetAnalyticsCollectionEnabled(true);
                SetDeviceInfo();
                
                onComplete(string.Empty);
            }

            private static void SetDeviceInfo()
            {
                SetUserProperty("OS", operatingSystem);
                SetUserProperty("Device Name", deviceName);
                SetUserProperty("Device Model", deviceModel);
                SetUserProperty("CPU Type", processorType);
                SetUserProperty("CPU Count", processorCount.ToString());
                SetUserProperty("System Memory", GetBytesReadable((long)systemMemorySize * 1024 * 1024));
                
                SetUserProperty("System Language", Application.systemLanguage.ToString());
                SetUserProperty("Platform", Application.platform.ToString());
                SetUserProperty("Application Version", Application.version);
                
                SetUserProperty("Resolution", $"{Screen.width}x{Screen.height}");
                SetUserProperty("DPI", ((int) Screen.dpi).ToString());
                
                SetUserProperty("GPU Name", graphicsDeviceName);
                SetUserProperty("GPU Vendor", graphicsDeviceVendor);
                SetUserProperty("GPU Version", graphicsDeviceVersion);
                SetUserProperty("Max Tex Size", maxTextureSize.ToString());
                
                
                string GetBytesReadable(long i)
                {
                    var sign = i < 0 ? "-" : "";
                    double readable;
                    string suffix;
                    
                    switch (i)
                    {
                        case >= 0x40000000:
                            suffix = "GB";
                            readable = i >> 20;

                            break;
                        case >= 0x100000:
                            suffix = "MB";
                            readable = i >> 10;

                            break;
                        case >= 0x400:
                            suffix = "KB";
                            readable = i;

                            break;
                        default:
                            return i.ToString(sign + "0 B");
                    }
                    
                    readable /= 1024;

                    return sign + readable.ToString("0.### ") + suffix;
                }
            }
        }
    }
}
#endif