using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

public class testpause : MonoBehaviour
{
    // Start is called before the first frame update

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Task t = Task.Run(async () => await wait());
            while (!t.IsCompleted)
            {
                print(t.Status);
            }
            print("task completed");
        }
    }

    async Task wait()
    {
        print("task started");
        
        UnityWebRequest www = UnityWebRequest.Get("https://jsonplaceholder.typicode.com/posts");

        await Task.Delay(2000);
        www.SendWebRequest();

        if (www.result != UnityWebRequest.Result.Success)
        {
            Debug.Log(www.error);
        }
        else
        {
            Debug.Log("Received: " + www.downloadHandler.text);
        }
    }
}
