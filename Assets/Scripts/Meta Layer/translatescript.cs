using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class translatescript : MonoBehaviour
{
    bool isdragging = false;
    Vector3 startmousepos;
    Vector3 spritestartpos,spritestartscale;
    public GameObject currobject;
    SpriteRenderer sr;
    [SerializeField] Transform p1_1, p1_2, p2_1, p2_2;
    float multiplier;
    float l1, l2,h,tano,objl2;
    Transform center;
    objectscript objscript;

    private void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        l1 = Vector3.Distance(p1_1.position, p1_2.position);
        l2 = Vector3.Distance(p2_1.position, p2_2.position);
        h = Mathf.Abs(p1_1.position.y - p2_1.position.y);
        tano = (l2 - l1)/(2*h);
        print(tano);
        center = currobject.transform.Find("Center");
        objscript = currobject.GetComponent<objectscript>();
    }
    private void OnMouseDown()
    {
        if(Input.GetMouseButtonDown(0))
        {
            isdragging = true;
            startmousepos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            spritestartpos = center.position;
            spritestartscale = currobject.transform.localScale;
            sr.color = new Color(1.0f, 1.0f, 1.0f, 0.4f);
            objl2 = l2 - 2 * (center.position.y - p2_2.position.y) * tano;
        }
    }
    private void Update()
    {
        if(isdragging == true)
        {
            this.transform.localPosition = Vector3.zero;
            Vector3 change = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z)) - startmousepos;
            currobject.transform.position += new Vector3(change.x, change.y, 0.0f);
            float h = center.position.y - spritestartpos.y;
            currobject.transform.localScale = spritestartscale * (objl2 - 2 * h * tano) / objl2;
            startmousepos += change;
        }
    }

    private void OnMouseUp()
    {
        isdragging = false;
        sr.color = new Color(1.0f, 1.0f, 1.0f, 0.7f);
    }

}
