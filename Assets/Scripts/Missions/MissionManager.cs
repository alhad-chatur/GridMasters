using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
public struct MissionType
{
    public HashSet<int> type;
    //0 -> get x points in y moves
    //1 -> clear x rows in y moves
    //2 -> clear x columns in y moves
    //3 -> clear x boxes in y moves
    public int givenmoves;
    public Dictionary<int,int> target;
    public int rotationmoves;
    public int difficulty;
    public int rewardtype;
    public int rewardamount;

    public MissionType(HashSet<int> itype, int igivenmoves, Dictionary<int, int> itarget,int irotationmoves,int idifficulty,int irewardtype,int irewardamount)
    {
        type = itype;
        givenmoves = igivenmoves;
        target = itarget;
        rotationmoves = irotationmoves;
        difficulty = idifficulty;
        rewardtype = irewardtype;
        rewardamount = irewardamount;
    }
}

public class MissionManager : MonoBehaviour
{
    public bool mode;
    public Dictionary<int,int> points;
    public int pointstogive = 3,combopoints = 1,streakpoints=1;
    public MissionType activemission,infomission;
    public static UnityEvent<MissionType> missionstart ;
    public static UnityEvent<MissionType,int> missionend ;

    public int cachedmenuchoice;

    MissionsCreater mc;
    UIscript ui;
    Movescounter mvc;
    void Start()
    {
        points = new Dictionary<int, int>();
       
        missionstart = new UnityEvent<MissionType>();
        missionend = new UnityEvent<MissionType, int>();
        missionstart.RemoveAllListeners();
        missionend.RemoveAllListeners();
        missionend.AddListener(onmissionend);
        //missionstart.AddListener(onmissionstart);
        mvc = FindAnyObjectByType<Movescounter>();
        mc = FindAnyObjectByType<MissionsCreater>();
        ui = FindAnyObjectByType<UIscript>();
        mode = false;
        
    }
    void Update()
    {
        if (mode == true)
        {
            //pointsdisplay.text = points.ToString() + "/" + activemission.target.ToString();
            //Movesdisplay.text = (activemission.givenmoves - mvc.movescounter).ToString() +"/" + activemission.givenmoves.ToString();
            
            checkmission();
        }
    }
    void checkmission()
    {
        bool check = false;
        foreach(int i in activemission.type)
        {
            if (points[i] < activemission.target[i])
            {
                check = true;
                break;
            }
        }
        
        if (check == false)
        {
            missionend.Invoke(activemission, 1);
        }
        else
        {
            if (mvc.movescounter <= 0)
            {
                missionend.Invoke(activemission, 0);
            }
        }

    }
    public void onmissioninfo(int index)
    {
        MissionType mt = mc.Missions[index];
        infomission = mt;
        ui.closemissionpage();
        ui.openmissioninfopage(mt);
    }

    public void onclosemissioninfo()
    {
        
        ui.closemissioninfopage();
        ui.openmissionpage();
    }

    public void onclosemissionlose()
    {
        ui.closemissionlosepage();
        ui.openmissionpage();
    }

    public void onmissionstart()
    {
        ui.closemissioninfopage();
        ui.openmissionmode();

        activemission = infomission;
        mode = true;
        missionstart.Invoke(infomission);
        points = new Dictionary<int, int>(activemission.type.Count);

        foreach (int i in activemission.type)
        {
            points[i] = 0;
        }
    }

    void onmissionend(MissionType mt,int result)
    {
        mode = false;
        ui.closemissionmode();
        if (result == 0)
            ui.openmissionlosepage();
        else if (result == 1)
            ui.openmissionwonpage();
    }

    public void onmissionretry()
    {
        ui.closemissionlosepage();
        ui.openmissionmode();
        mode = true;
        missionstart.Invoke(activemission);
        points = new Dictionary<int, int>(activemission.type.Count);

        foreach (int i in activemission.type)
        {
            points[i] = 0;
        }
    }

    public void onconfirmabortmission()
    {
        ui.closemissionchangepopup();
        ui.closemissionmode();
        missionend.Invoke(activemission, 2);
        ui.changebaritem(cachedmenuchoice);
    }

    public void ondenyabortmission()
    {
        ui.closemissionchangepopup();
    }

    public void onwonpageclick()
    {
        ui.closemissionwonpage();
        ui.openmissionpage();
    }

}
