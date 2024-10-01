using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class PersistantClientCode : NetworkBehaviour 
{
    thisuserUI ui;
    public bool isconnected = false;
    void Start()
    {
        NetworkManager.OnClientConnectedCallback += NetworkManager_OnClientConnectedCallback;
    }
    
    private void NetworkManager_OnClientConnectedCallback(ulong obj)
    {
        if (!IsClient)
            return;
        if(!ui)
        ui = FindAnyObjectByType<thisuserUI>();
        ui.onreconnectme();
        isconnected = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (ui) //this means that we are indeed on a client and not a server
        {
            if (isconnected == true && NetworkManager.IsConnectedClient == false)
            {
                ui.ondisconnectme();
                isconnected = false;
            }
        }
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            NetworkManager.Singleton.Shutdown();
        }
        //else
        //{
        //    NetworkManager.Singleton.StartClient();
        //}
    }
}
