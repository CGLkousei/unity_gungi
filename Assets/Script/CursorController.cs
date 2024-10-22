using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{    
    private Vector2Int index;
    private float cursorHight = 0.05f;
    private bool movable;
    

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitCursor(int row, int file, Vector3 position)
    {
        Vector3 pos = position;
        pos.y += cursorHight;
        transform.position = pos;
        index = new Vector2Int(row, file);
        this.movable = false;
    }

    public void setMovable()
    {
        this.movable = true;
    }
    public void clearUnit()
    {
        this.movable = false;
    }

    public Vector2Int getIndex()
    {
        return index;
    }
}
