using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    private int row;
    private int file;
    public bool onUnit;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitCursor(int row, int file)
    {
        this.row = row;
        this.file = file;
        this.onUnit = false;
    }

    public void setUnit()
    {
        this.onUnit = true;
    }
}
