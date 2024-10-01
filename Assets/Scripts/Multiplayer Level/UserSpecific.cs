using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class UserSpecific : NetworkBehaviour
{
    public int color;
    public bool hasstarted = false;

    public Mulit_ObjectSpawner mos;
    public ServerCode sc;
    public thisuserUI UI;
    GridManager gridmanager;
    public int movescounter, rotationmoves;

    [ClientRpc]
    public void StartClientRpc(int imovescounter, int irotationcounter,int points1,int points2)
    {
        mos = FindFirstObjectByType<Mulit_ObjectSpawner>();
        sc = FindFirstObjectByType<ServerCode>();
        UI = FindFirstObjectByType<thisuserUI>();
        gridmanager = FindFirstObjectByType<GridManager>();
        if (!IsOwner)
            return;


        UI.us = this;
        UI.thisuserclientid = (int)OwnerClientId;
        mos.us = this;
        mos.points1 = points1;
        mos.points2 = points2;
        movescounter = imovescounter;
        rotationmoves = irotationcounter;
    }

    void Update()
    {
        if (!IsOwner || !hasstarted)
            return;

        if (((int)OwnerClientId == sc.us1no.Value && sc.currentchance.Value == 1) || ((int)OwnerClientId == sc.us2no.Value && sc.currentchance.Value == 0)) //using XOR gate to simplfiy the logic
        {
            mousepos temp = mos.mp;
            temp.x = temp.x / Screen.width;
            temp.y = temp.y / Screen.height;

            sc.setmouseposServerRpc(temp);
        }
    }

    [ClientRpc]
    public void setchanceClientRpc()
    {
        if (!IsOwner)
            return;

        int rand = Random.Range(0, mos.Dimensions.Count);

        mos.chancestart(color,rand,true);
        sc.copystartinfoServerRpc(color,rand);
    }

    [ClientRpc]
    public void startgameClientRpc ()
    {
        if (!IsOwner)
            return;

        if (UI.hasreallystarted == false)
        {
            UI.hasreallystarted = true;
            UI.closestartscreen();
        }
    }

    [ClientRpc]
    public void ColorsetClientRpc(int col, bool hs)
    {
        color = col;
        hasstarted = hs;
        if (!IsOwner)
            return;

        StartCoroutine(UI.gamestartscreen(color,2.0f));
    }

    [ClientRpc]
    public void restoreconfigClientRpc(int[] grid, int blockno,int col, bool hs,bool isour,bool hasrotateused)
    {
        color = col;
        hasstarted = hs;
        if (!IsOwner)
            return;

        int[,] testgrid = new int[9,9];

        int index = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                testgrid[i, j] = grid[index];
                index++;
            }
        }

        StartCoroutine(UI.gamestartscreen(color, 0.0f));
        mos.restoreconfiguration(testgrid,hasrotateused);
        mos.chancestart(col, blockno, isour);
        sc.reconnectcompleteserverRpc((int)OwnerClientId);
    }

    [ClientRpc]
    public void startcopystateClientRpc(int blockno,int color)
    {
        if (!IsOwner)
            return;
        if (mos == null)
            print("problem");

        mos.chancestart(color, blockno,false);
    }

    [ClientRpc]
    public void onmousedownClientRpc()
    {
        if (!IsOwner)
            return;
        
        mos.onmousedownfunction();
    }

    [ClientRpc]
    public void onmouseupClientRpc(float posx,float posy)
    {
        if (!IsOwner)
            return;

        mos.onmouseupfunction(posx, posy);
    }

    [ClientRpc]
    public void onrotateClientRpc()
    {
        if (!IsOwner)
            return;

        mos.rotatefunction();
    }

    [ClientRpc]
    public void onskipClientRpc()
    {
        if (!IsOwner)
            return;

        mos.skipfunction();
    }

    [ClientRpc]
    public void ontimeoutClientRpc()
    {
        if (!IsOwner)
            return;

        if(mos.ourturn)
        movescounter--;
        mos.timeout();
    }

    [ClientRpc]
    public void onrotatedisableClientRpc()
    {
        if (!IsOwner)
            return;

        UI.disablerotation();
    }
    
    [ClientRpc]
    public void ondisconnectotherClientRpc()
    {
        if (!IsOwner)
            return;

        int[] flattenedGrid = new int[81];
        int index = 0;
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                flattenedGrid[index] = gridmanager.grid[i, j];
                index++;
            }
        }


        sc.copygridServerRpc(flattenedGrid,mos.hasrotateused);
        UI.ondisconnectother();
    }

    [ClientRpc]
    public void onreconnectotherClientRpc()
    {
        if (!IsOwner)
            return;

        UI.onreconnectother();
    }


    /// <summary>
    /// enter 1 if won, 2 if lost, 0 if draw
    /// </summary>
    /// <param name="result"></param>
    [ClientRpc]
    public void ongameendClientRpc(int result)
    {
        if (!IsOwner)
            return;

        if (result == 1)
            UI.ongamewin();
        else if (result == 2)
            UI.ongamelose();
        else
            UI.ongamedraw();

        hasstarted = false;
        NetworkManager.Singleton.Shutdown();
    }

    public void isrunningoff()
    {
        if (!IsOwner)
            return;

        movescounter--;
        sc.isrunningoffServerRpc();   
    }

    public void callonmousedown()
    {
        if (!IsOwner)
            return;

        sc.onmousedownServerRpc(mos.points1,mos.points2);
    }

    public void callonmouseup(float x, float y)
    {
        if (!IsOwner)
            return;

        sc.onmouseupServerRpc(x, y);
    }

    public void callonrotate(bool hasrotated)
    {
        if (!IsOwner)
            return;

        if(!hasrotated)
        rotationmoves--;
        
        sc.onrotateServerRpc(hasrotated);
    }

    public void callonskip()
    {
        if (!IsOwner)
            return;

        sc.onskipServerRpc();
    }

    public void ondisconnectedthis()
    {
        if (!IsOwner)
            return;

        UI.ondisconnectme();
    }

    public void givesignalready()
    {
        sc.makereadyServerRpc((int)OwnerClientId);
    }

}
