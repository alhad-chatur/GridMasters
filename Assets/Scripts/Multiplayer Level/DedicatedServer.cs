#if UNITY_SERVER

using System;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Services.Core;
using Unity.Services.Multiplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DedicatedServer : MonoBehaviour
{

    IMultiplayService m_MultiplayService;
    private MultiplayEventCallbacks m_MultiplayEventCallbacks;
    private IServerEvents m_ServerEvents;


    private void Start()
    {
        onStart();
           //NetworkManager.Singleton.StartServer();
        
            //Scene currentScene = SceneManager.GetActiveScene();
            //GameObject[] rootObjects = currentScene.GetRootGameObjects();
            //foreach (GameObject obj in rootObjects)
            //{
            //    if (!obj.CompareTag("ServerRelated"))
            //        Destroy(obj);
            //}

}


/// <summary>
/// This should be done early in the server's lifecycle, as you'll want to receive events as soon as possible.
/// </summary>
private async void onStart()
    {
        await UnityServices.InitializeAsync();
        await getmultiplayinstance();

        // We must first prepare our callbacks like so:
        m_MultiplayEventCallbacks = new MultiplayEventCallbacks();
        m_MultiplayEventCallbacks.Allocate += OnAllocate;
        //m_MultiplayEventCallbacks.Deallocate += OnDeallocate;
        //m_MultiplayEventCallbacks.Error += OnError;
        //m_MultiplayEventCallbacks.SubscriptionStateChanged += OnSubscriptionStateChanged;

        m_ServerEvents = await MultiplayService.Instance.SubscribeToServerEventsAsync(m_MultiplayEventCallbacks);
    }

    public static void LogServerConfig()
    {
        var serverConfig = MultiplayService.Instance.ServerConfig;
        Debug.Log($"Server ID[{serverConfig.ServerId}]");
        Debug.Log($"AllocationID[{serverConfig.AllocationId}]");
        Debug.Log($"Port[{serverConfig.Port}]");
        Debug.Log($"QueryPort[{serverConfig.QueryPort}");
        Debug.Log($"LogDirectory[{serverConfig.ServerLogDirectory}]");
    }

    private void OnAllocate(MultiplayAllocation allocation)
    {
        LogServerConfig();
        Example_ReadyingServer();
    }

    private async void Example_ReadyingServer()
    {
        // After the server is back to a blank slate and ready to accept new players
        await MultiplayService.Instance.ReadyServerForPlayersAsync();

        NetworkManager.Singleton.StartServer();
    }

    async Task getmultiplayinstance()
    {
        try
        {
            m_MultiplayService = MultiplayService.Instance;
        }
        catch (Exception ex)
        {
            Debug.LogWarning($"Error creating Multiplay allocation service.\n{ex}");
        }
    }

    private async void Example_UnreadyingServer()
    {
        // The match has ended and players are disconnected from the server
        await MultiplayService.Instance.UnreadyServerAsync();
    }

}

#endif
