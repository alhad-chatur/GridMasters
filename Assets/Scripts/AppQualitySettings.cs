using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class AppQualitySettings : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = (int)math.min(60.0, Screen.currentResolution.refreshRateRatio.value);
    }
}
