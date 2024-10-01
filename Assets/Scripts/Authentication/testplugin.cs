using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

#if UNITY_ANDROID
using static UnityEngine.AndroidJavaClass;
using static UnityEngine.AndroidJavaObject;


public class ForegroundServiceController : MonoBehaviour
{
    public static void StartForegroundService(string authorization, string id)
    {
        AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
        AndroidJavaClass serviceStarterClass = new AndroidJavaClass("com.example.myplugin.MyForegroundServiceStarter");
        serviceStarterClass.CallStatic("startService", activity, authorization, id);
    }
}
#endif