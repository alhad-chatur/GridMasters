using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Networking.Transport.Relay;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Matchmaker.Models;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;

public class MultiplayerUI : NetworkBehaviour
{

    [SerializeField] TMP_InputField ip;
    [SerializeField] TMP_Text text;
    [SerializeField] TMP_Text matchmaking;
    [SerializeField] AssetReferenceGameObject clientonlyPrefab;
    [SerializeField] NetworkConfig networkConfig;
    bool isclientloaded = false;
    private void Start()
    {
#if UNITY_SERVER
return;
#endif
        loadclient();
        
    }

    public async Task<bool> StartClientWithRelay(string joinCode)
    {
        print("start method");
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }

        var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode: joinCode);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(joinAllocation, "dtls"));
        return !string.IsNullOrEmpty(joinCode) && NetworkManager.Singleton.StartClient();
    }

   

    public async Task<string> StartHostWithRelay(int maxConnections = 5)
    {
        await UnityServices.InitializeAsync();
        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
        }
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(maxConnections);
        NetworkManager.Singleton.GetComponent<UnityTransport>().SetRelayServerData(new RelayServerData(allocation, "dtls"));
        var joinCode = await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        print(joinCode);
        text.text = joinCode;
        return NetworkManager.Singleton.StartHost() ? joinCode : null;
    }
    // Start is called before the first frame update

    public void Client()
    {
        
        NetworkManager.Singleton.StartClient();
        if (isclientloaded == false)
        {
            loadclient();
            isclientloaded = true;
        }// StartClientWithRelay(ip.text);
    }
    public void Server()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void Host()
    {
        //StartHostWithRelay();
        NetworkManager.Singleton.StartHost();
    }

    void loadclient()
    {
        clientonlyPrefab.Instantiate().Completed +=
            (temp) =>
            {
                Transform temp2;
                for (int i = temp.Result.transform.childCount - 1; i >= 0; --i)
                {
                    temp2 = temp.Result.transform.GetChild(i);
                    temp2.SetParent(null);
                    if (temp2.name == "Canvas")
                    {
                        temp2.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceCamera;
                        temp2.GetComponent<Canvas>().worldCamera = Camera.main;
                    }
                }

                UnityTransport t = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;

                if (t == null)
                    print("issue");

                t.SetConnectionData(networkConfig.serverIP, networkConfig.serverport);

                NetworkManager.Singleton.StartClient();

            };

    }

    //    private async void Start()
    //    {
    //#if UNITY_SERVER
    //return;
    //#endif
    //        await UnityServices.InitializeAsync();
    //        await SignInAnonymouslyAsync();
    //        loadclient();
    //    }

    //    async Task SignInAnonymouslyAsync()
    //    {
    //        try
    //        {
    //            await AuthenticationService.Instance.SignInAnonymouslyAsync();
    //            Debug.Log("Sign in anonymously succeeded!");

    //            // Shows how to get the playerID
    //            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");

    //        }
    //        catch (AuthenticationException ex)
    //        {
    //            // Compare error code to AuthenticationErrorCodes
    //            // Notify the player with the proper error message
    //            Debug.LogException(ex);
    //        }
    //        catch (RequestFailedException ex)
    //        {
    //            // Compare error code to CommonErrorCodes
    //            // Notify the player with the proper error message
    //            Debug.LogException(ex);
    //        }
    //    }

    //    public async void matchmake()
    //    {
    //        string ticketid = await startmatchmake();
    //       MultiplayAssignment assignment = await pollmatchmake(ticketid);
    //        //if (assignment.Status != MultiplayAssignment.StatusOptions.Found)
    //        //    return;

    //        string serverIp = assignment.Ip;
    //        ushort serverPort = Convert.ToUInt16(assignment.Port);

    //        UnityTransport t = (UnityTransport)NetworkManager.Singleton.NetworkConfig.NetworkTransport;
    //        t.SetConnectionData(serverIp, (ushort)serverPort);

    //        NetworkManager.Singleton.StartClient();
    //    }

    //    async Task<string> startmatchmake()
    //    {
    //        var players = new List<Player>
    //        {
    //        new Player(AuthenticationService.Instance.PlayerId, new Dictionary<string, object>())
    //        };


    //        // Set options for matchmaking
    //        var options = new CreateTicketOptions(
    //          "queue2", // The name of the queue defined in the previous step,
    //          new Dictionary<string, object>());

    //        // Create ticket
    //        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

    //        // Print the created ticket id
    //        Debug.Log(ticketResponse.Id);
    //        return ticketResponse.Id;
    //    }

    //    async Task<MultiplayAssignment> pollmatchmake(string ticketid)
    //    {
    //        MultiplayAssignment assignment = null;
    //        bool gotAssignment = false;
    //        int i = 0;
    //        do
    //        {
    //            //Rate limit delay
    //            i++;

    //            await Task.Delay(TimeSpan.FromSeconds(3f));

    //            // Poll ticket
    //            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketid);
    //            if (ticketStatus == null)
    //            {
    //                continue;
    //            }

    //            //Convert to platform assignment data (IOneOf conversion)
    //            if (ticketStatus.Type == typeof(MultiplayAssignment))
    //            {
    //                assignment = ticketStatus.Value as MultiplayAssignment;
    //            }

    //            switch (assignment?.Status)
    //            {
    //                case MultiplayAssignment.StatusOptions.Found:
    //                    gotAssignment = true;
    //                    break;
    //                case MultiplayAssignment.StatusOptions.InProgress:
    //                    print("polling");
    //                    //matchmaking.text = "polling" + i.ToString();
    //                    break;
    //                case MultiplayAssignment.StatusOptions.Failed:
    //                    gotAssignment = true;
    //                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
    //                    break;
    //                case MultiplayAssignment.StatusOptions.Timeout:
    //                    gotAssignment = true;
    //                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
    //                    break;
    //                default:
    //                    throw new InvalidOperationException();
    //            }

    //        } while (!gotAssignment);
    //        return assignment;
    //    }

    public void discconectclient()
    {
        NetworkManager.Singleton.Shutdown();
    }
}
