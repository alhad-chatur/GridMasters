using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int gridSizeX = 9;
    public int gridSizeY = 9;
    [SerializeField] GameObject TilePrefab;
    public float cellSize;
    public float padding = 0.1f;
    public int[,] grid;
    public int[,] savedgrid;
    public GameObject[,] gridobject;
    [SerializeField] GameObject gridimage;
    SpriteRenderer sr;
    void Start()
    {
        cellSize = TilePrefab.GetComponent<SpriteRenderer>().sprite.bounds.size.x + padding;
        grid = new int[9, 9];
        gridobject = new GameObject[9, 9];
        savedgrid = new int[9, 9];
        GenerateGrid();
        sr = gridimage.GetComponent<SpriteRenderer>();
        gridimage.transform.position = new Vector3(-(cellSize-padding) / 2, -(cellSize-padding) / 2, 0.0f);
        gridimage.transform.localScale = cellSize * 9 / (sr.sprite.bounds.size.x + padding) * Vector3.one;
    }

    void GenerateGrid()
    {
        for (int x = 0; x < gridSizeX; x++)
        {
            for (int y = 0; y < gridSizeY; y++)
            {
                grid[x, y] = -1;
            }
        }
    }

}
