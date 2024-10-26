using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{    
    private Vector2Int index;
    private float cursorHight = 0.05f;
    private int onUnitCount;
    private bool isTuke;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitCursor(int row, int file, Vector3 position, int onUnitCount)
    {
        Vector3 pos = position;
        pos.y += cursorHight;
        transform.position = pos;
        index = new Vector2Int(row, file);
        this.onUnitCount = onUnitCount;
    }
    public Vector2Int getIndex()
    {
        return index;
    }
    public void setOnUnitCount(int onunit) {
        if (onUnitCount < 0)
            this.onUnitCount = 0;
        else
            this.onUnitCount = onunit;
    }
    public int getOnUnitCount() { return onUnitCount; }
}
