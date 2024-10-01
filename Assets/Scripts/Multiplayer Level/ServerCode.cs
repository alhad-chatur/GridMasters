using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;


public struct mousepos : INetworkSerializable
{
    public float x;
    public float y;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        if (serializer.IsReader)
        {
            var reader = serializer.GetFastBufferReader();
            reader.ReadValueSafe(out x);
            reader.ReadValueSafe(out y);
        }
        else
        {
            var writer = serializer.GetFastBufferWriter();
            writer.WriteValueSafe(x);
            writer.WriteValueSafe(y);
        }
    }

    public mousepos(float x1,float y1)
        {
        x = x1;
        y = y1;
        }
}


public class ServerCode : NetworkBehaviour
{
    bool isready1 = false, isready2 = false;
    bool tostart = false;
    public bool isallconnected = false;
    bool disconnectcallback = false;
    int[] savedgrid =  new int[81];
    int currblockno;
    bool currhasrotateused;

    UserSpecific us1, us2;

    public NetworkVariable<bool> wholeft = new NetworkVariable<bool>(false);
    public NetworkVariable<int> us1no = new NetworkVariable<int>();
    public NetworkVariable<int> us2no = new NetworkVariable<int>();


    [SerializeField] float maxdisconnectwaittime = 30.0f;
    [SerializeField] float timeouttime = 10;
    int maxmoves = 10;
    [SerializeField] int rotationmoves = 3;
    int movesp1, movesp2;
    int rotmovep1, rotmovep2;
    int points1, points2;
    public NetworkVariable<float>  diswaittime1 = new NetworkVariable<float>(),diswaittime2 = new NetworkVariable<float>();
    public bool isrunning = false;
    public NetworkVariable<int> currentchance = new NetworkVariable<int>(0);
    public NetworkVariable<mousepos> mp = new NetworkVariable<mousepos>();
    public NetworkVariable<float> time = new NetworkVariable<float>();
    //public NetworkVariable<bool> isdragging;

    //private void Start()
    //{
    //    NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnectCallback; 
    //}

    private void OnClientDisconnectCallback(ulong obj)
    {
        if (tostart == false)
            return;
        
        if (obj == 0)
        {
            us1.ondisconnectedthis();
            us2.ondisconnectedthis();
        }
        else
        {
            int disconnectedno = (int)obj;
            if (disconnectedno == us1no.Value)
            {
                
                wholeft.Value = false;
                us2.ondisconnectotherClientRpc();
                us1no.Value = Mathf.Max(us1no.Value, us2no.Value) + 1;
                isready1 = false;
            }
            else
            {
                wholeft.Value = true;
                us1.ondisconnectotherClientRpc();
                us2no.Value = Mathf.Max(us2no.Value, us1no.Value) + 1;
                isready2 = false;
            }
        }
    }

    void Update()
    {
        
        if (!IsServer)
            return;
        
        //executes when the game is starting out
        if (tostart == false && NetworkManager.Singleton.ConnectedClientsList.Count==2 )
        {
            tostart = true;
            isallconnected = true;
            movesp1 = maxmoves;
            movesp2 = maxmoves;
            rotmovep1 = rotationmoves;
            rotmovep2 = rotationmoves;
            points1 = 0;
            points2 = 0;
            diswaittime1.Value = maxdisconnectwaittime;
            diswaittime2.Value = maxdisconnectwaittime;

            foreach (var v in NetworkManager.Singleton.ConnectedClients)
            {
                print(v.Key.ToString() + " " + v.Value.ToString());
            }

            us1no.Value = 1;
            us2no.Value = 2;
            us1 = NetworkManager.Singleton.ConnectedClients[(ulong)us1no.Value].PlayerObject.GetComponent<UserSpecific>();
            us2 = NetworkManager.Singleton.ConnectedClients[(ulong)us2no.Value].PlayerObject.GetComponent<UserSpecific>();
            us1.StartClientRpc(movesp1, rotmovep1,0,0);
            us2.StartClientRpc(movesp2,rotmovep2,0,0);
            us1.ColorsetClientRpc(0, true);
            us2.ColorsetClientRpc(1, true);
        }

        //when disconnection is there
        if (NetworkManager.Singleton.ConnectedClients.Count < 2 && tostart == true)
        {
            //just when disconnection has occured
            if(disconnectcallback ==false)
            {
                disconnectcallback = true;
                if (!NetworkManager.Singleton.ConnectedClients.ContainsKey((ulong)us1no.Value))
                {
                    OnClientDisconnectCallback((ulong)us1no.Value);
                }
                else if(!NetworkManager.Singleton.ConnectedClients.ContainsKey((ulong)us2no.Value))
                {
                    OnClientDisconnectCallback((ulong)us2no.Value);
                }
            }
            
            if(isallconnected)
            isallconnected = false;

            decrementdistimer();
            if(diswaittime1.Value < 0 || diswaittime2.Value <0)
            {
                if (NetworkManager.Singleton.ConnectedClients.Count == 0)
                    endgame(0);
                else
                {
                    if (wholeft.Value == false)
                        endgame(2);
                    else
                        endgame(1);
                }
            }
        }

        //when the disconnedcted player returns
        if(isallconnected == false && NetworkManager.Singleton.ConnectedClients.Count ==2 && tostart == true)
        {
            print(points1.ToString() + points2.ToString());
            if (wholeft.Value == false)
            {
                us1 = NetworkManager.Singleton.ConnectedClients[(ulong)us1no.Value].PlayerObject.GetComponent<UserSpecific>();
                
                us1.StartClientRpc(movesp1, rotmovep1, points1, points2);
                //us1.ColorsetClientRpc(0, true);
                if(currentchance.Value == 1)
                    us1.restoreconfigClientRpc(savedgrid, currblockno, 0, true,true,currhasrotateused);
                else
                    us1.restoreconfigClientRpc(savedgrid, currblockno, 0, true, false,currhasrotateused);

                us2.onreconnectotherClientRpc();
            }
            else
            {
                us2 = NetworkManager.Singleton.ConnectedClients[(ulong)us2no.Value].PlayerObject.GetComponent<UserSpecific>();
                us2.StartClientRpc(movesp2, rotmovep2, points1, points2);
                //us2.ColorsetClientRpc(1, true);
                
                if(currentchance.Value == 1)
                    us2.restoreconfigClientRpc(savedgrid, currblockno, 1, true,false,currhasrotateused);
                else
                    us2.restoreconfigClientRpc(savedgrid, currblockno, 1, true, true,currhasrotateused);

                us1.onreconnectotherClientRpc();
            }
            disconnectcallback = false;

            isallconnected = true;
        }
        
        //when game is running
        if (tostart == true && isallconnected == true && isready1 == true && isready2 == true)
        {
            //Timeout Condition
            time.Value+=Time.deltaTime;
            if(time.Value>timeouttime)
            {
                us1.ontimeoutClientRpc();
                us2.ontimeoutClientRpc();
                if (currentchance.Value == 1)
                    movesp1--;
                else
                    movesp2--;
                
                isrunning = false;
            }

            //this occurs after every turn change
            if(isrunning == false)
            {
                //ending the game based on points
                if(movesp1 == 0 && movesp2 == 0)
                {
                    if (points1 > points2)
                        endgame(1);
                    else if (points1 < points2)
                        endgame(2);
                    else
                        endgame(0);
                }
                time.Value = 0;
                isrunning = true;
                if(currentchance.Value == 0)
                {
                    currentchance.Value = 1;
                    us1.setchanceClientRpc();
                }
                else
                {
                    currentchance.Value = 0;
                    us2.setchanceClientRpc();
                }
            }
        }
    }
    ///<summary>
    ///enter 1 if us1 won, 2 if us2 won or 0 if draw
    /// </summary>
    void endgame(int result)
    {
        if(result==1)
        {
            us1.ongameendClientRpc(1);
            us2.ongameendClientRpc(2);
        }
        else if(result == 2)
        {
            us1.ongameendClientRpc(2);
            us2.ongameendClientRpc(1);
        }
        else
        {
            us1.ongameendClientRpc(0);
            us2.ongameendClientRpc(0);
        }
        tostart = false;
        isrunning = false;
        isallconnected = false;
        NetworkManager.Singleton.Shutdown();
        Application.Quit();
    }

    public void decrementdistimer()
    {
        if (wholeft.Value == false)
            diswaittime1.Value -= Time.deltaTime;

        else
            diswaittime2.Value -= Time.deltaTime;
    }

    [ServerRpc(RequireOwnership =false)]
    public void copystartinfoServerRpc(int color, int blockno)
    {
        if (!IsServer)
            return;

        currblockno = blockno;

        if (currentchance.Value == 1)
            us2.startcopystateClientRpc(blockno,color);
        else
            us1.startcopystateClientRpc(blockno,color);
    }
    [ServerRpc(RequireOwnership = false)]
    public void setmouseposServerRpc(mousepos mpos)
    {
        if (!IsServer)
            return;

        mp.Value = mpos;
    }

    [ServerRpc(RequireOwnership =false)]
    public void onmousedownServerRpc(int ipoints1,int ipoints2)
    {
        if (!IsServer)
            return;

        points1 = ipoints1;
        points2 = ipoints2;

        print(points1.ToString() + points2.ToString());

        if (currentchance.Value == 1)
            us2.onmousedownClientRpc();
        else
            us1.onmousedownClientRpc();

    }

    [ServerRpc(RequireOwnership =false)]
    public void onmouseupServerRpc(float posx, float posy)
    {
        if (!IsServer)
            return;

        if (currentchance.Value == 1)
            us2.onmouseupClientRpc(posx,posy);
        else
            us1.onmouseupClientRpc(posx,posy);

    }

    [ServerRpc(RequireOwnership = false)]
    public void onrotateServerRpc(bool hasrotateused)
    {
        if (!IsServer)
            return;

        if (currentchance.Value == 1)
        {
            if (!hasrotateused)
            {
                rotmovep1--;
                if (rotmovep1 == 0)
                    us1.onrotatedisableClientRpc();
            }
            us2.onrotateClientRpc();
        }
        else
        {
            if (!hasrotateused)
            {
                rotmovep2--;
                if (rotmovep2 == 0)
                    us2.onrotatedisableClientRpc();
            }
            us1.onrotateClientRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void onskipServerRpc()
    {
        if (!IsServer)
            return;

        if (currentchance.Value == 1)
            us2.onskipClientRpc();
        else
            us1.onskipClientRpc();
    }

    [ServerRpc(RequireOwnership =false)]
    public void isrunningoffServerRpc()
    {
        if (!IsServer)
            return;

        isrunning = false;

        if (currentchance.Value == 1)
        {
            movesp1--;
        }
        else
        {
            movesp2--;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void makereadyServerRpc(int id)
    {

        if (!IsServer)
            return;

        if (id == us1no.Value)
            isready1 = true;
        else if (id == us2no.Value)
            isready2 = true;

        if(isready1 == true && isready2 == true)
        {
            us1.startgameClientRpc();
            us2.startgameClientRpc();
        }
    }

    [ServerRpc(RequireOwnership =false)]
    public void copygridServerRpc(int[] grid,bool hasrotateused)
    {
        if (!IsServer)
            return;

        for (int i = 0; i < 81; i++)
            savedgrid[i] = grid[i];


        //for (int i = 0; i < 9; i++)
        //    for (int j = 0; j <9; j++)
        //        savedgrid[i, j] = grid[i, j];

        currhasrotateused = hasrotateused;
    }

    [ServerRpc(RequireOwnership =false)]
    public void reconnectcompleteserverRpc(int id)
    {
        if (!IsServer)
            return;

        if (id == us1no.Value)
            isready1 = true;
        else if (id == us2no.Value)
            isready2 = true;
    }
}
