using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class MissionsCreater : MonoBehaviour
{
    List<MissionType> missions;
 public List<MissionType> Missions
    {
        get { return missions; }
    }

    void Start()
    {
        missions = new List<MissionType>();
        missions.Add(new MissionType(new HashSet<int> { 0 }, 20, new Dictionary<int, int> { { 0, 30 } }, 5,0,0,50));
        missions.Add(new MissionType(new HashSet<int> { 1 }, 20, new Dictionary<int, int> { { 1, 5 } }, 5,1,0,100));
        missions.Add(new MissionType(new HashSet<int> { 2,3 }, 20, new Dictionary<int, int> { { 2, 3 },{ 3,3} }, 3, 2, 1, 1));
    }

}
