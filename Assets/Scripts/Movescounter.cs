using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Movescounter : MonoBehaviour
{
    [SerializeField] int imoves, irotation;
    public int movescounter, rotationcounter;
    [SerializeField] TMP_Text movestext, rotationtext;
    [SerializeField] Button rotationbutton;
    int movescountstored,rotationcountstored;
    
    void Start()
    {
        movescounter = imoves;
        rotationcounter = irotation;
        MissionManager.missionstart.AddListener(onmissionenable);
        MissionManager.missionend.AddListener(onmissionend);
    }

    void Update()
    {
        if(movescounter >= 0)
        movestext.text = movescounter.ToString();
        else
            movestext.text = "-";

        if (rotationcounter >= 0)
        {
            rotationtext.text = rotationcounter.ToString();
            if (rotationbutton.interactable == false)
                rotationbutton.interactable = true;
        }
        else
        {
            if (rotationbutton.interactable == true)
                rotationbutton.interactable = false;
            rotationtext.text = "-";
        }
    }

    public void onmissionenable(MissionType mt)
    {
        movescountstored = movescounter;
        rotationcountstored = rotationcounter;
        movescounter = mt.givenmoves;
        rotationcounter = mt.rotationmoves;
    }

    public void onmissionend(MissionType mt,int result)
    {
        movescounter = movescountstored;
        rotationcounter = rotationcountstored;
    }

}
