using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using Unity.VisualScripting;
using UnityEngine.UI;
using System.Threading.Tasks;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Matchmaker;
using Unity.Services.Matchmaker.Models;
using System;
using UnityEngine.SceneManagement;
using UnityEngine.AddressableAssets;


public class UIscript : MonoBehaviour
{
    [SerializeField] TMP_Text pointsdisplay, Movesdisplay;
    [SerializeField] GameObject filledgrid, gridimage;
    [SerializeField] CanvasGroup maingameUI, missionpageUI, duringmissionUI, mainmodeUI, missioninfopageUI, missionlosepageUI, missionchangepopupUI, missionwonUI, multiplayerpageUI, MatchmakingUI, matchfoundUI;
    [SerializeField] GameObject[] menubar;
    Vector3[] initposmenuitems, initsizemenuitems;

    [Header("MissionInfo")]
    [SerializeField] TMP_Text missiondifficulty;
    [SerializeField] TMP_Text missiontarget;
    [SerializeField] TMP_Text missionreward;
    [SerializeField] TMP_Text missionmoves;
    [SerializeField] Image missionrewardimage, closebutton, missionmainimage;

    [SerializeField] Sprite easy, medium, hard;
    [SerializeField] Sprite yellowclose, blueclose;
    [SerializeField] Sprite Coins, Piniata;

    [SerializeField] NetworkConfig networkconfig;

    int currentbaritem = -1;

    //Vector3 currentitempos, currentitemsize;

    MissionManager mm;
    Movescounter mvc;
    BarsFill bfill;
    MissionsCreater mc;

    string matchticketid;
    bool gotAssignment;

    string temp;
    private async void Start()
    {
#if UNITY_SERVER
return;
#endif

        mm = FindAnyObjectByType<MissionManager>();
        mvc = FindAnyObjectByType<Movescounter>();
        bfill = FindAnyObjectByType<BarsFill>();
        mc = FindAnyObjectByType<MissionsCreater>();

        initposmenuitems = new Vector3[menubar.Length];
        initsizemenuitems = new Vector3[menubar.Length];

        for (int i = 0; i < menubar.Length; i++)
        {
            initposmenuitems[i] = menubar[i].GetComponent<RectTransform>().anchoredPosition;
            initsizemenuitems[i] = menubar[i].GetComponent<RectTransform>().localScale;
        }


       
        //loadclient();
        loadmissionUI();
    }

    public void loadmissionUI()
    {
        Transform temp;
        string temp2;
        for (int i = 0; i < missionpageUI.transform.Find("Missions").childCount; i++)
        {
            temp = missionpageUI.transform.Find("Missions").GetChild(i);
            switch (mc.Missions[i].difficulty)
            {
                case 0:
                    temp2 = "EASY";
                    break;
                case 1:
                    temp2 = "MEDIUM";
                    break;
                case 2:
                    temp2 = "HARD";
                    break;
                default:
                    temp2 = "";
                    break;
            }
            temp.Find("Title").GetComponent<TMP_Text>().text = "MISSION " + temp2;

            bool tobreak = false;
            List<string> temp3 = new List<string>();
            if (mc.Missions[i].type.Contains(0))
            {
                temp3.Add(mc.Missions[i].target[0].ToString() + " Points");
            }
            if(mc.Missions[i].type.Contains(1))
            {
                temp3.Add(mc.Missions[i].target[1].ToString() + " Rows");
                tobreak = true;
            }
            if(mc.Missions[i].type.Contains(2))
            {
                temp3.Add(mc.Missions[i].target[2].ToString() + " Columns");
                tobreak = true;
            }
            if (mc.Missions[i].type.Contains(3))
            {
                temp3.Add(mc.Missions[i].target[3].ToString() + " Boxes");
                tobreak = true;
            }

            if (tobreak == false)
                temp2 = temp3[0];

            else
            {
                temp2 = "Break ";
                for (int j = 0; j < temp3.Count; j++)
                {
                    temp2 += temp3[j];
                    if (j < temp3.Count - 2)
                        temp2 += " ,";
                    else if (j == temp3.Count - 2)
                        temp2 += " and ";
                    else
                        temp2 += "";
    
                } 
            }

            temp2 += " in " + mc.Missions[i].givenmoves.ToString() + " Moves";
            temp.Find("Shorttitle").GetComponent<TMP_Text>().text = temp2;
        }
    }

    void Update()
    {
        if (mm.mode == true)
        {
            temp = "";
            foreach(int i in mm.activemission.type)
            {
                temp += mm.points[i].ToString() + "/" + mm.activemission.target[i].ToString();
                switch (i)
                {
                    case 0:
                        temp += " Points";
                        break;
                    case 1:
                        temp += " Rows";
                        break;
                    case 2:
                        temp += " Columns";
                        break;
                    case 3:
                        temp += " Boxes";
                        break;
                }

                temp += "\n";
            }
            
            pointsdisplay.text = temp;
            Movesdisplay.text = (mm.activemission.givenmoves - mvc.movescounter).ToString() + "/" + mm.activemission.givenmoves.ToString();
        }

    }

    public void changebaritem(int newitem)
    {
        if (currentbaritem == newitem)
            return;

        if (currentbaritem == 1 && mm.mode == true)
        {
            openmissionchangepopup();
            mm.cachedmenuchoice = newitem;
            return;
        }

        if (currentbaritem != -1)
        {
            menubar[currentbaritem].GetComponent<RectTransform>().DOAnchorPos(initposmenuitems[currentbaritem], 1.0f);
            menubar[currentbaritem].transform.DOScale(initsizemenuitems[currentbaritem], 1.0f);
        }
        switch (currentbaritem)
        {
            case 0:
                break;
            case 1:
                closemissioninfopage();
                closemissionlosepage();
                closemissionpage();
                break;
            case 2:
                closemainpage();
                break;

            case 3:
                closemultiplayerpage();
                break;

            default:
                break;
        }
        currentbaritem = newitem;

        //if (currentbaritem != -1)
        //{
        //    currentitempos = menubar[currentbaritem].transform.position;
        //    currentitemsize = menubar[currentbaritem].transform.localScale;
        //}
        menubar[currentbaritem].GetComponent<RectTransform>().DOAnchorPos(initposmenuitems[currentbaritem] + new Vector3(0.0f, 80.0f, 0.0f), 1.0f);
        menubar[currentbaritem].transform.DOScale(initsizemenuitems[currentbaritem] * 1.4f, 1.0f);

        switch (currentbaritem)
        {
            case 0:
                break;
            case 1:
                openmissionpage();
                break;
            case 2:
                openmainpage();
                break;

            case 3:
                openmultiplayerpage();
                break;
            default:
                break;
        }

    }

    public void closemainpage()
    {
        filledgrid.SetActive(false);
        gridimage.SetActive(false);
        // maingameUI.DOFade(0.0f, 0.5f).onComplete += () => { maingameUI.gameObject.SetActive(false); };
        maingameUI.gameObject.SetActive(false);
        mainmodeUI.gameObject.SetActive(false);
        bfill.enabled = false;
    }

    public void openmissionpage()
    {
        missionpageUI.gameObject.SetActive(true);
        missionpageUI.alpha = 0;
        missionpageUI.transform.position -= new Vector3(0.0f, 1.0f, 0.0f);
        missionpageUI.transform.DOMoveY(missionpageUI.transform.position.y + 1.0f, 1.0f);
        missionpageUI.DOFade(1.0f, 1.0f);
    }

    public void closemissionpage()
    {
        missionpageUI.gameObject.SetActive(false);
    }

    public void closemissionmode()
    {
        filledgrid.SetActive(false);
        gridimage.SetActive(false);
        missionpageUI.gameObject.SetActive(false);
        duringmissionUI.gameObject.SetActive(false);

        maingameUI.gameObject.SetActive(false);
    }

    public void openmissionmode()
    {
        duringmissionUI.gameObject.SetActive(true);
        filledgrid.SetActive(true);
        gridimage.SetActive(true);
        maingameUI.gameObject.SetActive(true);
        maingameUI.alpha = 0;
        maingameUI.DOFade(1.0f, 1.0f);
    }

    public void openmainpage()
    {
        filledgrid.SetActive(true);
        gridimage.SetActive(true);
        bfill.enabled = true;

        maingameUI.gameObject.SetActive(true);
        maingameUI.alpha = 0;
        maingameUI.DOFade(1.0f, 1.0f);

        mainmodeUI.gameObject.SetActive(true);
        mainmodeUI.alpha = 0;
        mainmodeUI.DOFade(1.0f, 1.0f);
    }

    public void closemissioninfopage()
    {
        missioninfopageUI.gameObject.SetActive(false);
    }

    public void openmissioninfopage(MissionType mt)
    {

        //switch (mt.type)
        //{
        //    case 0:
        //        missiontarget.text = "Earn " + mt.target.ToString() + " Points";
        //        break;
        //    case 1:
        //        missiontarget.text = "Break" + mt.target.ToString() + " Rows";
        //        break;
        //    case 2:
        //        missiontarget.text = "Break" + mt.target.ToString() + " columns";
        //        break;
        //    case 3:
        //        missiontarget.text = "Break" + mt.target.ToString() + " Boxes";
        //        break;
        //}

        switch (mt.difficulty)
        {
            case 0:
                missionmainimage.sprite = easy;
                closebutton.sprite = yellowclose;
                missiondifficulty.text = "Easy";
                break;

            case 1:
                missionmainimage.sprite = medium;
                closebutton.sprite = blueclose;
                missiondifficulty.text = "Medium";
                break;

            case 2:
                missionmainimage.sprite = hard;
                closebutton.sprite = yellowclose;
                missiondifficulty.text = "Hard";
                break;
        }

        switch (mt.rewardtype)
        {
            case 0:
                missionreward.text = mt.rewardamount.ToString();
                missionrewardimage.sprite = Coins;
                break;

            case 1:
                missionreward.text = "Piniata";
                missionrewardimage.sprite = Piniata;
                break;
        }

        missionmoves.text = mt.givenmoves.ToString();

        missioninfopageUI.gameObject.SetActive(true);
        missioninfopageUI.alpha = 0;
        missioninfopageUI.DOFade(1.0f, 1.0f);

    }

    public void openmissionlosepage()
    {
        missionlosepageUI.gameObject.SetActive(true);
        missionlosepageUI.alpha = 0;
        missionlosepageUI.DOFade(1.0f, 1.0f);
    }

    public void closemissionlosepage()
    {
        missionlosepageUI.gameObject.SetActive(false);
    }

    public void openmissionchangepopup()
    {
        missionchangepopupUI.gameObject.SetActive(true);
        missionchangepopupUI.alpha = 0.0f;
        missionchangepopupUI.DOFade(1.0f, 1.0f);
    }

    public void closemissionchangepopup()
    {
        missionchangepopupUI.gameObject.SetActive(false);
    }

    public void openmissionwonpage()
    {
        missionwonUI.gameObject.SetActive(true);
        missionwonUI.alpha = 0;
        missionwonUI.DOFade(1.0f, 1.0f);
    }

    public void closemissionwonpage()
    {
        missionwonUI.gameObject.SetActive(false);
    }

    public void openmultiplayerpage()
    {
        multiplayerpageUI.gameObject.SetActive(true);
    }

    public void closemultiplayerpage()
    {
        multiplayerpageUI.gameObject.SetActive(false);
    }

    public void openmatchmakingpage()
    {
        MatchmakingUI.gameObject.SetActive(true);
    }

    public void closematchmakingpage()
    {
        MatchmakingUI.gameObject.SetActive(false);
    }

    public void openmatchfoundpage()
    {
        matchfoundUI.gameObject.SetActive(true);
    }

    public void closematchfoundpage()
    {
        matchfoundUI.gameObject.SetActive(false);
    }

   
    public async void matchmake()
    {
        string ticketid = await startmatchmake();
        matchticketid = ticketid;
        if (ticketid == null)
            return;

        closemultiplayerpage();
        openmatchmakingpage();

        MultiplayAssignment assignment = await pollmatchmake(ticketid);

        if (assignment.Status!= MultiplayAssignment.StatusOptions.Found)
        {
            openmultiplayerpage();
            return;
        }    

        string serverIp = assignment.Ip;
        ushort serverPort = Convert.ToUInt16(assignment.Port);

        networkconfig.serverIP = serverIp;
        networkconfig.serverport = serverPort;

        closematchmakingpage();
        openmatchfoundpage();

        SceneManager.LoadSceneAsync("Multiplayer Level");
    }

    async Task<string> startmatchmake()
    {
        var players = new List<Player>
        {
        new Player(AuthenticationService.Instance.PlayerId, new Dictionary<string, object>())
        };


        // Set options for matchmaking
        var options = new CreateTicketOptions(
          "queue2", // The name of the queue defined in the previous step,
          new Dictionary<string, object>());

        // Create ticket
        var ticketResponse = await MatchmakerService.Instance.CreateTicketAsync(players, options);

        // Print the created ticket id
        Debug.Log(ticketResponse.Id);
        return ticketResponse.Id;
    }

    async Task<MultiplayAssignment> pollmatchmake(string ticketid)
    {
        MultiplayAssignment assignment = null;
        gotAssignment  = false;
        int i = 0;
        do
        {
            //Rate limit delay
            i++;

            await Task.Delay(TimeSpan.FromSeconds(3f));

            // Poll ticket
            var ticketStatus = await MatchmakerService.Instance.GetTicketAsync(ticketid);
            if (ticketStatus == null)
            {
                continue;
            }

            //Convert to platform assignment data (IOneOf conversion)
            if (ticketStatus.Type == typeof(MultiplayAssignment))
            {
                assignment = ticketStatus.Value as MultiplayAssignment;
            }

            switch (assignment?.Status)
            {
                case MultiplayAssignment.StatusOptions.Found:
                    gotAssignment = true;
                    break;
                case MultiplayAssignment.StatusOptions.InProgress:
                    print("polling");
                    //matchmaking.text = "polling" + i.ToString();
                    break;
                case MultiplayAssignment.StatusOptions.Failed:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Error: " + assignment.Message);
                    break;
                case MultiplayAssignment.StatusOptions.Timeout:
                    gotAssignment = true;
                    Debug.LogError("Failed to get ticket status. Ticket timed out.");
                    break;
                default:
                    throw new InvalidOperationException();
            }

        } while (!gotAssignment);
        return assignment;
    }

    //private void OnApplicationQuit()
    //{
    //    MatchmakerService.Instance.DeleteTicketAsync(matchticketid);
    //}

//private void OnApplicationPause(bool pause)
//{
//    if (pause)
//    {
//        StartCoroutine(DeleteTicketCoroutine());
//    }
//}

//private void OnApplicationPause(bool pause)
//{
//    if (pause)
//    {
//        Task t = Task.Run(async () => await MatchmakerService.Instance.DeleteTicketAsync(matchticketid));
//        while (!t.IsCompleted)
//        {
//            print(t.Status);
//        }
//        print("task completed");
//    }
//}
#if UNITY_ANDROID
    private void OnApplicationPause(bool pause)
    {
        if (pause && matchticketid!=null)
        {
            ForegroundServiceController.StartForegroundService(AuthenticationService.Instance.AccessToken, matchticketid);
            matchticketid = null;
            gotAssignment = true;
            closematchmakingpage();
            openmultiplayerpage();
        }
    }
#endif
    //private IEnumerator DeleteTicketCoroutine()
    //{
    //    if (matchticketid != null)
    //    {
    //        print("pause Started");
    //        yield return DeleteTicketAsync(matchticketid);
    //        print("paused");
    //    }
    //}

    //private async IEnumerator DeleteTicketAsync(string ticketId)
    //{
    //    try
    //    {
    //        await MatchmakerService.Instance.DeleteTicketAsync(ticketId);
    //    }
    //    catch (Exception e)
    //    {
    //        Debug.LogError($"Error deleting ticket: {e.Message}");
    //    }
    //}
    //    private async void OnApplicationFocus(bool hasFocus)
    //    {
    //        if (!hasFocus)
    //        {
    //            if (matchticketid != null)
    //            {
    //                print("Lost Focus");
    //                await MatchmakerService.Instance.DeleteTicketAsync(matchticketid);
    //                print("lost focus completed");
    //            }
    //        }
    //        else
    //            print("Gained Focus");
    //    }
    //}
}