using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;

public class SolveGrid : MonoBehaviour
{

    List<int> row2destroy, col2destroy;
    List<(int, int)> box2destroy;
    Queue<(int, (int, int))> spdestroys;
    void solve(int[,] grid, List<(int,int)> currlist)
    {
        List<((int,int),int)> locations = new List<((int, int), int)>();
        bool isvalid;
        for(int i=0; i<9; i++)
            for(int j=0; j<9; j++)
            {
                isvalid = true;
                foreach(var v in currlist)
                {
                    if (grid[i + v.Item1,j + v.Item2] !=0)
                    {
                        isvalid = false;
                        break;
                    }
                }
                if (isvalid == false)
                    break;

                else
                {
                    
                    locations.Add(((i, j), j));
                }

            }

    }
    /*
    IEnumerator checkpattern(int[,] grid)
    {
    checkpatternstart:
        row2destroy.Clear();
        col2destroy.Clear();
        box2destroy.Clear();
        spdestroys.Clear();
        //checking filled rows
        for (int i = 0; i < 9; i++)
        {
            bool isfilled = true;
            for (int j = 0; j <9; j++)
            {
                if (grid[j, i] == -1)
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
        for (int i = 0; i < 9; i++)
        {
            bool isfilled = true;
            for (int j = 0; j < 9; j++)
            {
                if (grid[i, j] == -1)
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
        for (int i = 0; i < 9; i += 3)
        {
            for (int j = 0; j < 9; j += 3)
            {
                bool filled = true;
                for (int x = i; x < i + 3; x++)
                {
                    for (int y = j; y < j + 3; y++)
                    {
                        if (grid[x, y] == -1)
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

        bool iscombo = false;
        foreach (int i in row2destroy)
        {
            int isall = -2;
            for (int j = 0; j < 9; j++)
            {
                if (isall == -2)
                    isall = grid[j, i];

                else
                    if (grid[j, i] != isall)
                    isall = -1;

                //destroycolortile(j, i);
                //yield return timetodestroy;
            }
            if (isall == 0)
                points1 += samecolorpoints;
            else if (isall == 1)
                points2 += samecolorpoints;

        }

        foreach (int i in col2destroy)
        {
            int isall = -2;
            for (int j = GridManager.gridSizeY - 1; j >= 0; j--)
            {

                if (isall == -2)
                    isall = GridManager.grid[i, j];

                else
                    if (GridManager.grid[i, j] != isall)
                    isall = -1;
                destroycolortile(i, j);
                //yield return timetodestroy;
            }
            if (isall == 0)
                points1 += samecolorpoints;
            else if (isall == 1)
                points2 += samecolorpoints;
        }

        foreach ((int, int) v in box2destroy)
        {
            int isall = -2;
            for (int x = v.Item1; x < v.Item1 + 3; x++)
                for (int y = v.Item2; y < v.Item2 + 3; y++)
                {

                    if (isall == -2)
                        isall = GridManager.grid[x, y];

                    else
                        if (GridManager.grid[x, y] != isall)
                        isall = -1;

                    destroycolortile(x, y);
                    // yield return timetodestroy;
                }
            if (isall == 0)
                points1 += samecolorpoints;
            else if (isall == 1)
                points2 += samecolorpoints;
        }

        if (spdestroys.Count > 0)
        {
            while (spdestroys.Count > 0)
            {
                yield return StartCoroutine(destroysptiles(spdestroys.Peek().Item1, spdestroys.Peek().Item2.Item1, spdestroys.Peek().Item2.Item2));
                spdestroys.Dequeue();
            }
            goto checkpatternstart;
        }

        if (ourturn)
        {
            us.isrunningoff();
            ourturn = false;
            viel.SetActive(true);

        }

        yield return null;

    }

    */
}
