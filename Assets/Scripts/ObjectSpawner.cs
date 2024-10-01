using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class ObjectSpawner : MonoBehaviour
{
    List<List<(int, int)>> Dimensions = new List<List<(int, int)>>
    {
            new List<(int, int)> { (0,0), (1,0) , (2,0), (3,0) },
            new List<(int, int)> { (0,0), (0,1) , (0,2), (0,3) },
            new List<(int, int)> { (0,0),(0,1) ,(1,0) },
            new List<(int, int)> { (0,0),(-1,0),(1,0),(0,1)},
            new List<(int, int)> { (0,0),(0,1),(1,0),(2,0)},
            new List<(int, int)> { (0,0),(0,1),(-1,1),(1,0)},
            new List<(int, int)> { (0,0),(0,1),(1,0),(1,1)}
    };
    [SerializeField] GameObject[] colortile;
    [SerializeField] GameObject[] SpecialTiles; // 0-> left     1->leftright       2->diagonal
    [SerializeField] GameObject filledgrid;
    List<GameObject> currentlistobjects;
    [SerializeField] GameObject currparent,grandparent;
    
    [SerializeField] GameObject shadowprefab;
    [SerializeField] Color redshadow, greenshadow;
    GameObject shadow;
    List<SpriteRenderer> shadowsrs;


    [SerializeField] GameObject destroycubeprefab;
    GameObject[,] destroycubes;
    Rigidbody[,] destroycubesrb;
    GameObject destroycubesparent;
    WaitForSeconds destroycubesttlwfs;
    float destroycubesttl = 3.0f;

    int rand;
    List<(int,int)> currlist;
    List<int> currlistcol;
    float spprob = 0.2f;
    bool isDragging;
    int color;                       // 0->red,  1->green   , 2->blue

    //public int gdestroyed, rdestroyed, bdestroyed;    
    public int[] destroyedtiles;      // 0->red, 1->green , 2->blue
    List<int> row2destroy, col2destroy;
    List<(int, int)> box2destroy;
    Queue<(int, (int, int))> spdestroys;
    
    float[] scaler;
    float shadowscaler;

    [SerializeField] GridManager GridManager;
    [SerializeField] Movescounter mvc;
    
    bool hasrotateused = false;
    [SerializeField] float ttodestroy = 0.1f;
    WaitForSeconds timetodestroy;

    //related to missionmode
    [SerializeField]MissionManager missionManager;
    bool isstreak;
    void Start()
    {
        destroyedtiles = new int[3];
        currentlistobjects = new List<GameObject>();
        row2destroy = new List<int>();
        col2destroy = new List<int>();
        box2destroy = new List<(int,int)>();
        spdestroys = new Queue<(int, (int, int))>();
        currlist = new List<(int, int)>();
        currlistcol = new List<int>();
        timetodestroy = new WaitForSeconds(ttodestroy);
        destroycubes = new GameObject[9, 9];
        destroycubesrb = new Rigidbody[9, 9];
        destroycubesparent = new GameObject("DestroyCubesParent");

        MissionManager.missionstart.AddListener(onmissionstart);
        MissionManager.missionend.AddListener(onmissionend);

        for(int i=0;i<GridManager.gridSizeX;i++)
            for(int j=0;j<GridManager.gridSizeY;j++)
            {
                destroycubes[i, j] = Instantiate(destroycubeprefab,destroycubesparent.transform);
                destroycubes[i, j].layer = 6;
                destroycubesrb[i, j] = destroycubes[i, j].GetComponent<Rigidbody>();
                destroycubes[i, j].SetActive(false);
            }

        destroycubesttlwfs = new WaitForSeconds(destroycubesttl);

        shadow = new GameObject("Shadow");
        shadowsrs = new List<SpriteRenderer>();
        //currparent = new GameObject("Parent");
        scaler = new float[2] { (GridManager.cellSize - GridManager.padding) / colortile[0].GetComponent<SpriteRenderer>().sprite.bounds.size.x, (GridManager.cellSize - GridManager.padding) / colortile[1].GetComponent<SpriteRenderer>().sprite.bounds.size.x };
        shadowscaler = (GridManager.cellSize - GridManager.padding) /shadowprefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x;
        spprob = 0.2f;
        hasrotateused = false;
        
       generatenewobject();
    }
    void generatenewobject()
    {
        rand = Random.Range(0, Dimensions.Count);
        currlist.Clear();
        currlist.AddRange(Dimensions[rand]);
        currentlistobjects.Clear();
        currlistcol.Clear();

        shadowsrs.Clear();

        //currparent = new GameObject("Parent");
        //currparent.transform.parent = this.transform;
        grandparent.transform.localPosition = Vector3.zero;
        currparent.transform.localPosition = Vector3.zero;
        GameObject temp;

        Vector3 parentcenter = Vector3.zero;

        color = Random.Range(0, 2);
        
        for (int i=0;i< currlist.Count;i++)
        {
            temp = Instantiate(colortile[color], currparent.transform);
            temp.transform.localScale *= scaler[color];
            temp.transform.localPosition = new Vector3(currlist[i].Item1 * GridManager.cellSize, currlist[i].Item2 * GridManager.cellSize, 0);
            currentlistobjects.Add(temp);
            currlistcol.Add(color);
            parentcenter += temp.transform.localPosition;

            //instantiating the shadow
            temp = Instantiate(shadowprefab, shadow.transform);
            temp.transform.localScale *= shadowscaler;
            temp.transform.localPosition = new Vector3(currlist[i].Item1 * GridManager.cellSize, currlist[i].Item2 * GridManager.cellSize, 0);
            shadowsrs.Add(temp.GetComponent < SpriteRenderer >());
        }

        shadow.SetActive(false);

        parentcenter = parentcenter / currlist.Count;

        currparent.transform.localPosition = -parentcenter;

        if (Random.Range(0, 1.0f) < spprob)
        {
            int sptile = Random.Range(0, (int)SpecialTiles.Length);
            GameObject tempgo = Instantiate(SpecialTiles[sptile],currparent.transform);
            //tempgo.transform.parent = currparent.transform;
            tempgo.transform.localScale *= (GridManager.cellSize - GridManager.padding) / SpecialTiles[sptile].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
            int pos = Random.Range(0, (int)currentlistobjects.Count);
            tempgo.transform.localPosition = currentlistobjects[pos].transform.localPosition; 
            Destroy(currentlistobjects[pos]);
            currentlistobjects[pos] = tempgo;
            currlistcol[pos] = sptile + 3;
        }

        hasrotateused = false;
    }

    IEnumerator checkpattern()
    {
        bool iscombo = false;

    checkpatternstart:        
        row2destroy.Clear();
        col2destroy.Clear();
        box2destroy.Clear();
        spdestroys.Clear();

        //checking filled rows
        for(int i=0;i<GridManager.gridSizeY;i++)
        {
            bool isfilled = true;
            for(int j=0;j<GridManager.gridSizeX;j++)
            {
                if (GridManager.grid[j, i] == -1)
                {
                    isfilled = false;
                    break;
                }
            }
            if (isfilled == true)
            {
                row2destroy.Add(i);

            }
        }

        //checking filled cols
        for (int i = 0; i < GridManager.gridSizeX; i++)
        {
            bool isfilled = true;
            for (int j = 0; j < GridManager.gridSizeY; j++)
            {
                if (GridManager.grid[i, j] == -1)
                {
                    isfilled = false;
                    break;
                }
            }
            if (isfilled == true)
            {
                col2destroy.Add(i);
            }    
        }

        //checking filled boxes
        for (int i = 0; i < GridManager.gridSizeX; i += 3)
        {
            for (int j = 0; j < GridManager.gridSizeY; j += 3)
            {
                bool filled = true;
                for (int x = i; x < i + 3; x++)
                {
                    for (int y = j; y < j + 3; y++)
                    {
                        if (GridManager.grid[x, y] == -1)
                        {
                            filled = false;
                            break;
                        }
                    }
                    if (!filled)
                      break;
                }
                if (filled)
                {
                    box2destroy.Add((i, j));
                }
            }
        }

        foreach(int i in row2destroy)
        {
            if (missionManager.mode == true)
            {
                if (missionManager.activemission.type.Contains(0))
                {
                    missionManager.points[0] += missionManager.pointstogive;
                    if (iscombo == false)
                        iscombo = true;
                    else
                        missionManager.points[0] += missionManager.combopoints;
                }
                else if(missionManager.activemission.type.Contains(1))
                {
                    missionManager.points[1] += 1;
                }
            }
            for(int j=0;j<GridManager.gridSizeX;j++)
            {
                destroycolortile(j, i);
                //yield return timetodestroy;
            }
        }

        foreach(int i in col2destroy)
        {
            if (missionManager.mode == true)
            {
                if (missionManager.activemission.type.Contains(0))
                {
                    missionManager.points[0] += missionManager.pointstogive;
                    if (iscombo == false)
                        iscombo = true;
                    else
                        missionManager.points[0] += missionManager.combopoints;
                }
                else if(missionManager.activemission.type.Contains(2))
                {
                    missionManager.points[2] += 1;
                }
            }
            for (int j = GridManager.gridSizeY-1; j >=0; j--)
            {
                destroycolortile(i, j);
                //yield return timetodestroy;
            }
        }

        foreach((int,int) v in box2destroy)
        {
            if (missionManager.mode == true)
            {
                if (missionManager.activemission.type.Contains(0))
                {
                    missionManager.points[0] += missionManager.pointstogive;
                    if (iscombo == false)
                        iscombo = true;
                    else
                        missionManager.points[0] += missionManager.combopoints;
                }
                else if(missionManager.activemission.type.Contains(3))
                {
                    missionManager.points[3] += 1;
                }
            }
            for (int x = v.Item1; x < v.Item1 + 3; x++)
                for (int y = v.Item2; y < v.Item2 + 3; y++)
                {
                    destroycolortile(x, y);
                   // yield return timetodestroy;
                }
        }

        if (spdestroys.Count > 0)
        {
            while (spdestroys.Count > 0)
            {
                //if (missionManager.mode == true)
                //    if (missionManager.activemission.type.Contains(0))
                //    {
                //        missionManager.points[0] += missionManager.pointstogive;
                //        if (iscombo == false)
                //            iscombo = true;
                //        else
                //            missionManager.points[0] += missionManager.combopoints;
                //    }

                yield return StartCoroutine(destroysptiles(spdestroys.Peek().Item1, spdestroys.Peek().Item2.Item1, spdestroys.Peek().Item2.Item2));
                spdestroys.Dequeue();
            }
            goto checkpatternstart;
        }

        if (missionManager.mode == true)
        {
            if (isstreak)
            {
                if (iscombo)
                {
                    if(missionManager.activemission.type.Contains(0))
                        missionManager.points[0] += missionManager.streakpoints;
                    //istreakUI
                }
                else
                    isstreak = false;
            }
            else
            {
                if (iscombo)
                    isstreak = true;
            }
        }

        mvc.movescounter--;
        if(mvc.movescounter >=0)
        generatenewobject();

        yield return null;

    }

    IEnumerator destroysptiles(int type,int x,int y)
    {

        switch(type)
        {
            case 3:
                for(int i=0;i<=x;i++)
                {
                    if (GridManager.gridobject[i, y] == null)
                    {
                        //destroycolortile(i, y);
                        makecolortile(i, y);
                        yield return timetodestroy;
                    }
                }
                break;

            case 4:
                for (int i = 0; i < GridManager.gridSizeX; i++)
                {
                    if (GridManager.gridobject[i, y] == null)
                    {
                        //destroycolortil+e(i, y);
                        makecolortile(i, y);
                        yield return timetodestroy;
                    }
                }
                break;

            case 5:
                int i1, j1;
                i1 = x - 1;
                j1 = y + 1;
                makecolortile(x, y);

                while (i1 >= 0 && i1 < GridManager.gridSizeX && j1 >= 0 && j1 < GridManager.gridSizeY)
                {
                    if (GridManager.gridobject[i1, j1] == null)
                    {
                        //destroycolortile(i1, j1);
                        makecolortile(i1, j1);
                        yield return timetodestroy;
                    }
                    i1--;
                    j1++;
                }
                i1 = x + 1;
                j1 = y - 1;
                while (i1 >= 0 && i1 < GridManager.gridSizeX && j1 >= 0 && j1 < GridManager.gridSizeY)
                {
                    if (GridManager.gridobject[i1, j1] == null)
                    {
                        //destroycolortile(i1, j1);
                        makecolortile(i1, j1);
                        yield return timetodestroy;
                    }
                    i1++;
                    j1--;
                }
                break;
        }

        yield return null;

    }

    void destroycolortile(int x,int y)
    {
        if (GridManager.gridobject[x, y] == null)
            return;

        //GameObject temp = Instantiate(destroycube, GridManager.gridobject[x, y].transform.position, Quaternion.identity);
        // temp.GetComponent<Rigidbody>()
        destroycubes[x, y].SetActive(true);
        destroycubesrb[x, y].position = GridManager.gridobject[x, y].transform.position;
        Vector3 dir = (Camera.main.transform.position - destroycubesrb[x, y].position).normalized;
        Vector3 deflection = new Vector3(Random.Range(-0.3f, 0.3f), Random.Range(-0.3f, 0.3f), 0.0f);
        destroycubesrb[x, y].velocity = Random.Range(5f, 15f) * (dir+deflection).normalized;
        //destroycubesrb[x, y].angularVelocity = new Vector3(Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f), Random.Range(-1.5f, 1.5f));
        StartCoroutine(destroycubesanim(destroycubes[x,y]));

        Destroy(GridManager.gridobject[x, y]);
        GridManager.gridobject[x, y] = null;
        
        switch (GridManager.grid[x, y])
        {
            case 0:
                if(missionManager.mode == false)
                    destroyedtiles[0]++;
                break;
            case 1:
                if(missionManager.mode == false)
                    destroyedtiles[1]++;
                break;
            case 2:
                
                if (missionManager.mode == false)
                    destroyedtiles[2]++;
                break;
            case 3:
                spdestroys.Enqueue((3, (x, y)));
                break;
            case 4:
                spdestroys.Enqueue((4, (x, y)));
                break;
            case 5:
                spdestroys.Enqueue((5, (x, y)));
                break;
        }
        GridManager.grid[x, y] = -1;
    }

    void makecolortile(int x,int y)
    {
        if (GridManager.gridobject[x,y] !=null)
           return;

        int rand = Random.Range(0, 2);

        GridManager.gridobject[x,y] = Instantiate(colortile[rand], new Vector3(x*GridManager.cellSize,y*GridManager.cellSize,0.0f),Quaternion.identity,filledgrid.transform);
        GridManager.gridobject[x, y].transform.localScale *= scaler[rand];
        GridManager.gridobject[x, y].name = "new";
        GridManager.gridobject[x, y].GetComponent<SpriteRenderer>().sortingOrder = 1;
        GridManager.grid[x, y] = rand;
    }

    private void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            isDragging = true;
            grandparent.transform.parent = null;
            grandparent.transform.localScale = Vector3.one;
        }
    }

    private void OnMouseUp()
    {
        bool toreturn = false;
        //Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        //adding the gridmanager.cellsize/2 because the tiles are not pivoted at the left corner but at the center
        int posx = (int)((currparent.transform.position.x +GridManager.cellSize/2) / GridManager.cellSize);
        int posy = (int)((currparent.transform.position.y +GridManager.cellSize/2)/ GridManager.cellSize);
        for (int i=0;i<currlist.Count;i++)
        {
            if(posx + currlist[i].Item1 <GridManager.gridSizeX && posx + currlist[i].Item1 >=0 
                && posy + currlist[i].Item2 < GridManager.gridSizeY && posy + currlist[i].Item2 >= 0
                && GridManager.grid[currlist[i].Item1 + posx, currlist[i].Item2 + posy] ==-1)
            {
                continue;
            }
            else
            {
                toreturn = true;
                break;
            }
        }
        
        if(toreturn)
            grandparent.transform.localPosition = Vector3.zero;
        
        else
        {
            currparent.transform.position = new Vector3(posx*GridManager.cellSize, posy*GridManager.cellSize, 0);
            
            for(int i=0;i<currlist.Count;i++)
            {
                currentlistobjects[i].GetComponent<SpriteRenderer>().sortingOrder = 1;
                currentlistobjects[i].transform.parent = filledgrid.transform;
                GridManager.gridobject[currlist[i].Item1 + posx, currlist[i].Item2 + posy] = currentlistobjects[i];
                GridManager.grid[currlist[i].Item1 +posx, currlist[i].Item2 +posy] = currlistcol[i];
            }

            foreach (Transform t in shadow.transform)
                Destroy(t.gameObject);

            StartCoroutine(checkpattern());
           // generatenewobject();
        }

        isDragging = false;
        grandparent.transform.parent = this.transform;
        grandparent.transform.localScale = Vector3.one;
        grandparent.transform.localPosition = Vector3.zero;
    }

    private void Update()
    {
        if (mvc.movescounter < 0)
            grandparent.SetActive(false);
        else
            grandparent.SetActive(true);

        if (isDragging)
        {
            Vector3 cursorPosition = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, -Camera.main.transform.position.z));
            grandparent.transform.position = new Vector3(cursorPosition.x, cursorPosition.y, transform.position.z);
            
            //setting up the shadow
            int posx = (int)((currparent.transform.position.x + GridManager.cellSize / 2) / GridManager.cellSize);
            int posy = (int)((currparent.transform.position.y + GridManager.cellSize / 2) / GridManager.cellSize);
            float color = 1;        //1 -> green   0->red
            bool toshow = false;
            for (int i = 0; i < currlist.Count; i++)
                if (posx + currlist[i].Item1 < GridManager.gridSizeX && posx + currlist[i].Item1 >= 0
                && posy + currlist[i].Item2 < GridManager.gridSizeY && posy + currlist[i].Item2 >= 0)
                {
                    toshow = true;
                    if(GridManager.grid[currlist[i].Item1 + posx, currlist[i].Item2 + posy] != -1)
                    {
                        color *= 0;
                    }
                }
                else
                {
                    toshow = false;
                    break;
                }

            if (toshow)
            {

                foreach (SpriteRenderer sr in shadowsrs)
                {
                    if (color == 0)
                        sr.color = redshadow;
                    else
                        sr.color = greenshadow;
                }

                shadow.gameObject.SetActive(true);
                shadow.transform.position = new Vector3(posx * GridManager.cellSize, posy * GridManager.cellSize, 0);
            }
            else
                shadow.gameObject.SetActive(false);
        }
        else
        {
            shadow.gameObject.SetActive(false);
        }    
    }

    public void rotate()
    {
        if(hasrotateused == false)
        {
            if (mvc.rotationcounter < 0)
                return;
            hasrotateused = true;
            mvc.rotationcounter--;
        }
        
        int x, y;
        Vector3 parentcenter = Vector3.zero;

        for (int i=0;i<currlist.Count;i++)
        {
            x = currlist[i].Item1;
            y = currlist[i].Item2;
            currlist[i] = (-y, x);
            currentlistobjects[i].transform.localPosition = new Vector3(currlist[i].Item1 * GridManager.cellSize, currlist[i].Item2 * GridManager.cellSize, 0);
            parentcenter+=currentlistobjects[i].transform.localPosition;
        }
        parentcenter = parentcenter / currlist.Count;
        currparent.transform.localPosition = -parentcenter;

        //rotating the shadow
        foreach(Transform t in shadow.transform)
        {
            t.transform.localPosition = new Vector3(-t.transform.localPosition.y, t.transform.localPosition.x, 0);
        }

    }

    public void Skip()
    {
        if (mvc.movescounter > 0)
        {
            mvc.movescounter--;
            foreach (Transform t in currparent.transform)
            {
                Destroy(t.gameObject);
            }
            foreach (Transform t in shadow.transform)
                Destroy(t.gameObject);

            generatenewobject();
        }
        else if(mvc.movescounter ==0)
        {
            mvc.movescounter--;
            foreach (Transform t in currparent.transform)
            {
                Destroy(t.gameObject);
            }
            foreach (Transform t in shadow.transform)
                Destroy(t.gameObject);
        }
    }

    public void onmissionstart(MissionType mt)
    {
        //MissionManager.missionstart.Invoke();

        //missionManager.mode = true;
        //missionManager.points = 0;
        //performing a deep copy of grid array


        for(int i =0;i<GridManager.gridSizeX;i++)
        {
            for(int j = 0; j < GridManager.gridSizeY;j++)
            {
                if (GridManager.grid[i, j] != -1)
                    Destroy(GridManager.gridobject[i, j]);
                GridManager.savedgrid[i, j] = GridManager.grid[i, j];
                GridManager.grid[i, j] = -1;
            }
        }
        for(int i=0;i<currlist.Count;i++)
        {
            Destroy(currentlistobjects[i]);
        }
        currentlistobjects.Clear();
        currlist.Clear();
        currlistcol.Clear();
        hasrotateused = false;

        //destroying the shadows
        foreach (Transform t in shadow.transform)
            Destroy(t.gameObject);
        generatenewobject();
        isstreak = false;
    }

    void onmissionend(MissionType mt,int result)
    {
        for (int i = 0; i < GridManager.gridSizeX; i++)
        {
            for (int j = 0; j < GridManager.gridSizeY; j++)
            {
                if (GridManager.grid[i, j] != -1)
                    Destroy(GridManager.gridobject[i, j]);
                GridManager.grid[i, j] = -1;
                GridManager.grid[i, j] = GridManager.savedgrid[i, j];
            }
        }
        for (int i = 0; i < currlist.Count; i++)
        {
            Destroy(currentlistobjects[i]);
        }
        currentlistobjects.Clear();
        currlist.Clear();
        currlistcol.Clear();
        hasrotateused = false;

        //destroying the shadows
        foreach (Transform t in shadow.transform)
            Destroy(t.gameObject);

        print("yaha se to thik hai");

        GameObject temp;
        for (int i = 0; i < GridManager.gridSizeX; i++)
        {
            for (int j = 0; j < GridManager.gridSizeY; j++)
            {
                if (GridManager.grid[i, j] != -1)
                {
                    if (GridManager.grid[i, j] <= 2)
                    {
                        temp = Instantiate(colortile[GridManager.grid[i, j]], filledgrid.transform);
                        temp.transform.localScale *= scaler[color];
                    }
                    else
                    {
                        temp = Instantiate(SpecialTiles[GridManager.grid[i, j] - 3], filledgrid.transform);
                        temp.transform.localScale *= (GridManager.cellSize - GridManager.padding) / SpecialTiles[GridManager.grid[i,j]].GetComponent<SpriteRenderer>().sprite.bounds.size.x;
                    }
                    temp.transform.localPosition = new Vector3(GridManager.cellSize * i, GridManager.cellSize * j, 0.0f);
                    temp.GetComponent<SpriteRenderer>().sortingOrder = 1;
                    GridManager.gridobject[i, j] = temp;
                }
            }
        }

        generatenewobject();

    }

    IEnumerator destroycubesanim(GameObject go)
    {
        yield return destroycubesttlwfs;
        go.SetActive(false);
    }
}
