using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BarsFill : MonoBehaviour
{
    [SerializeField] ObjectSpawner spawner;
    [SerializeField] Image RedBar, GreenBar;
    [SerializeField] TMP_Text redtext, greentext;
    [SerializeField] int maxcount = 100;
    [SerializeField] GameObject bars;

    //private void Start()
    //{
    //    MissionManager.missionstart.AddListener(onmissionstart);
    //    MissionManager.missionend.AddListener(onmissionend);
    //}

    void Update()
    {
        if (spawner.destroyedtiles[0] > maxcount)
            spawner.destroyedtiles[0] = 0;
        if (spawner.destroyedtiles[1] > maxcount)
            spawner.destroyedtiles[1] = 0;
        
        RedBar.fillAmount = (float)spawner.destroyedtiles[0] / maxcount;
        GreenBar.fillAmount = (float)spawner.destroyedtiles[1] / maxcount;
        redtext.text = RedBar.fillAmount * 100 + "%";
        greentext.text = GreenBar.fillAmount * 100 + "%";
    }

    //void onmissionstart(MissionType mt)
    //{
    //    bars.SetActive(false);
    //}

    //void onmissionend(MissionType mt, bool result)
    //{
    //    bars.SetActive(true);
    //}

}
