using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NetworkConfig", menuName = "ScriptableObjects/NetworkConfig", order = 1)]
public class NetworkConfig : ScriptableObject
{
    public string serverIP;
    public ushort serverport;

}
