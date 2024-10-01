using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class objectscript : MonoBehaviour
{
    public bool iscolliding = false;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        iscolliding = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        iscolliding = false;
    }
}
