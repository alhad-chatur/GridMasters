using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class thisuserUI : MonoBehaviour
{
    [SerializeField] TMP_Text timer;
    [SerializeField] TMP_Text user1points,user2points;
    [SerializeField] TMP_Text rotationcounter, movescounter;
    [SerializeField] GameObject rotationviel;
    [SerializeField] CanvasGroup ondisconnectmeUI,ondisconnectotherUI;
    [SerializeField] TMP_Text disconnecttimerme, disconnecttimerother;
    [SerializeField] CanvasGroup winscreen, losescreen,drawscreen;
    [SerializeField] Transform startmatchUI,gridimage;
    [SerializeField] CanvasGroup mainmatchUI;

    [SerializeField] Sprite yellowprofsprite, redprofsprite;
    [SerializeField] Color yellowbg, redbg;
    public UserSpecific us;
    ServerCode sc;
    Mulit_ObjectSpawner mos;
    PersistantClientCode pcc;
    int disconnected = 0;
    float localdisconnecttimer;
    public bool hasreallystarted = false;
    public int thisuserclientid;
    // Update is called once per frame
    private void Start()
    {
        sc = FindFirstObjectByType<ServerCode>();
        mos = FindFirstObjectByType<Mulit_ObjectSpawner>();
        pcc = FindFirstObjectByType<PersistantClientCode>();
        rotationviel.SetActive(false);
        disconnected = 0;
    }

    void Update()
    {
        if (disconnected == 1)
        {
            localdisconnecttimer -= Time.deltaTime;
            if(localdisconnecttimer<0)
            {
                NetworkManager.Singleton.Shutdown();
                ondisconnectmeUI.gameObject.SetActive(false);
                ongamelose();
            }
            if (sc.wholeft.Value == false)
                disconnecttimerme.text = localdisconnecttimer.ToString();
            else
                disconnecttimerme.text = localdisconnecttimer.ToString();
        }
        else if (disconnected == 2)
        {
            if (sc.wholeft.Value == false)
                disconnecttimerother.text = sc.diswaittime1.Value.ToString();
            else
                disconnecttimerother.text = sc.diswaittime2.Value.ToString();
        }
        if (us == null)
            return;

        timer.text = sc.time.Value.ToString();
        if (us.color == 0)
        {
            user1points.text = mos.points1.ToString();
            user2points.text = mos.points2.ToString();
        }
        else
        {
            user1points.text = mos.points2.ToString();
            user2points.text = mos.points1.ToString();
        }
        rotationcounter.text = us.rotationmoves.ToString();
        movescounter.text = us.movescounter.ToString();
    }

    public void disablerotation()
    {
        rotationviel.SetActive(true);
    }

    public void ondisconnectme()
    {
        if (us.hasstarted == false)
            return;

        pcc.isconnected = false;
        ondisconnectmeUI.gameObject.SetActive(true);
        disconnected = 1;
        hasreallystarted = false;
        if (thisuserclientid == sc.us1no.Value)
            localdisconnecttimer = sc.diswaittime1.Value;
        else
            localdisconnecttimer = sc.diswaittime2.Value;
    }

    public void onreconnectme()
    {
        pcc.isconnected = true;
        ondisconnectmeUI.gameObject.SetActive(false);
        disconnected = 0;
    }

    public void ondisconnectother()
    {
        if (us.hasstarted == false)
            return;

        ondisconnectotherUI.gameObject.SetActive(true);
        disconnected = 2;
    }

    public void onreconnectother()
    {
        ondisconnectotherUI.gameObject.SetActive(false);
        disconnected = 0;
    }

    public void ongamewin()
    {
        winscreen.gameObject.SetActive(true);
    }
    
    public void ongamelose()
    {
        losescreen.gameObject.SetActive(true);
    }

    public void ongamedraw()
    {
        drawscreen.gameObject.SetActive(true);
    }

    public IEnumerator gamestartscreen(int color,float waittime)
    {
        //mainmatchUI.gameObject.SetActive(false);
        mainmatchUI.gameObject.SetActive(true);
        mainmatchUI.interactable = false;
        mainmatchUI.alpha = 0;
        startmatchUI.gameObject.SetActive(true);
        gridimage.gameObject.SetActive(false);
        //hasreallystarted = true;

        if (color == 0)
        {
            startmatchUI.Find("P1").Find("Image").GetComponent<Image>().sprite = redprofsprite;
            startmatchUI.Find("P1").Find("BG").GetComponent<Image>().color = redbg;
            startmatchUI.Find("P2").Find("Image").GetComponent<Image>().sprite = yellowprofsprite;
            startmatchUI.Find("P2").Find("BG").GetComponent<Image>().color = yellowbg;

            mainmatchUI.transform.Find("P1").Find("Image").GetComponent<Image>().sprite = redprofsprite;
            mainmatchUI.transform.Find("P2").Find("Image").GetComponent<Image>().sprite = yellowprofsprite;

        }
        else if(color == 1)
        {
            startmatchUI.Find("P2").Find("Image").GetComponent<Image>().sprite = redprofsprite;
            startmatchUI.Find("P2").Find("BG").GetComponent<Image>().color = redbg;
            startmatchUI.Find("P1").Find("Image").GetComponent<Image>().sprite = yellowprofsprite;
            startmatchUI.Find("P1").Find("BG").GetComponent<Image>().color = yellowbg;

            mainmatchUI.transform.Find("P2").Find("Image").GetComponent<Image>().sprite = redprofsprite;
            mainmatchUI.transform.Find("P1").Find("Image").GetComponent<Image>().sprite = yellowprofsprite;
        }

        yield return new WaitForSeconds(waittime);
 
        us.givesignalready();
    }

    public void closestartscreen()
    {
        mainmatchUI.gameObject.SetActive(true);
        mainmatchUI.interactable = true;
        mainmatchUI.alpha = 1.0f;
        startmatchUI.gameObject.SetActive(false);
        gridimage.gameObject.SetActive(true);
    }
}
