using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GameSceneScript : MonoBehaviour
{
    [SerializeField] GameObject prefabTile;
    [SerializeField] GameObject prefabCursor;
    [SerializeField] List<GameObject> whiteUnits;
    [SerializeField] List<GameObject> blackUnits;
    [SerializeField] Material transparentMaterial;
    [SerializeField] Material cursorMaterial;

    //Default set;
    int[,] boardSetting =
    {
        {0, 0, 10, 0, 0, 0, -10, 0, 0},
        {0, 8, 0, 0, 0, 0, 0, -7, 0},
        {0, 12, 9, 0, 0, 0, -9, -12, 0},
        {2, 0, 5, 0, 0, 0, -5, 0, -3},
        {1, 6, 10, 0, 0, 0, -10, -6, -1},
        {3, 0, 5, 0, 0, 0, -5, 0, -2},
        {0, 12, 9, 0, 0, 0, -9, -12, 0},
        {0, 7, 0, 0, 0, 0, 0, -8, 0},
        {0, 0, 10, 0, 0, 0, -10, 0, 0},
    };

    const int playerMax = 2;
    const int boardWidth = 9;
    const int boardHeight = 9;

    CursorController[,] cursors;
    UnitController[,] units;

    private float tileWidth;
    private float tileHeight;

    private GameObject selectedUnit = null;
    private GameObject selectedCursor = null;
    private List<Vector2Int> selectedUnitMovable = null;

    // Start is called before the first frame update
    void Start()
    {
        cursors = new CursorController[boardWidth, boardHeight];
        units = new UnitController[boardWidth, boardHeight];

        Vector3 tileScale = prefabTile.transform.localScale;
        tileWidth = tileScale.x;
        tileHeight = tileScale.z;

        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                int type = boardSetting[i, j];                
                GameObject prefabUnit;
                int player = -1;

                Vector3 pos = getPosition(i, j);
                GameObject cursor = Instantiate(prefabCursor, pos, Quaternion.identity);                
                CursorController cursorctrl = cursor.AddComponent<CursorController>();
                cursorctrl.InitCursor(i, j, pos);

                if (type == 0)
                {
                    cursors[i, j] = cursorctrl;
                    continue;
                }
                else if (type > 0)
                {
                    prefabUnit = blackUnits[type-1];
                    player = 0;
                }
                else
                {
                    type *= -1;
                    prefabUnit = whiteUnits[type-1];
                    player = 1;
                }
                
                GameObject unit = Instantiate(prefabUnit, pos, Quaternion.Euler(0, 0, 0));                

                Vector3 newScale = unit.transform.localScale;
                newScale.z *= 2;
                unit.transform.localScale = newScale;

                Rigidbody rigidbody = unit.AddComponent<Rigidbody>();
                rigidbody.mass = 1e3f;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

                BoxCollider collider = unit.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.03f, 0.03f, 0.003f);
                collider.center = new Vector3(0f, 0f, 0.002f);

                UnitController unitctrl = unit.AddComponent<UnitController>();
                unitctrl.InitUnit(player, type-1, pos, new Vector2Int(i, j), boardWidth, boardHeight);

                units[i, j] = unitctrl;
                cursors[i, j] = cursorctrl;
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit))
            {
                GameObject clickedObject = hit.collider.gameObject;                

                if (selectedUnit == null)
                {
                    selectedUnit = clickedObject;
                    UnitController unitctrl = clickedObject.GetComponent<UnitController>();
                    if (unitctrl != null)
                    {                        
                        unitctrl.LiftUnit();
                        selectedUnitMovable = unitctrl.getMovableTiles(getPositions(), unitctrl.getUnitType());
                        foreach (var index in selectedUnitMovable)
                        {
                            Renderer renderer = cursors[index.x, index.y].GetComponent<Renderer>();
                            renderer.material = cursorMaterial;
                        }
                    }                                      
                }
                else
                {
                    UnitController unitctrl = selectedUnit.GetComponent<UnitController>();                                                            

                    if (selectedUnit == clickedObject)
                    {
                        if (unitctrl != null)
                        {
                            setTransparent();
                            unitctrl.LiftOffUnit();
                        }
                        selectedUnit = null;
                        selectedUnitMovable = null;
                    }
                    else
                    {
                        UnitController _unitctrl = clickedObject.GetComponent<UnitController>();
                        if (_unitctrl != null)
                        {
                            if (unitctrl != null)
                            {
                                setTransparent();
                                unitctrl.LiftOffUnit();
                            }

                            _unitctrl.LiftUnit();
                            selectedUnit = clickedObject;
                            selectedUnitMovable = _unitctrl.getMovableTiles(getPositions(), _unitctrl.getUnitType());
                            foreach (var index in selectedUnitMovable)
                            {
                                Renderer renderer = cursors[index.x, index.y].GetComponent<Renderer>();
                                renderer.material = cursorMaterial;
                            }
                        }
                        else
                        {
                            CursorController cursorctrl = clickedObject.GetComponent<CursorController>();                            
                            if (cursorctrl != null)
                            {
                                Vector2Int index = cursorctrl.getIndex();
                                if (selectedUnitMovable.Contains(index))
                                {
                                    selectedUnit = null;
                                    selectedUnitMovable = null;
                                    setTransparent();

                                    unitctrl.MoveUnit(getPosition(index.x, index.y), index);
                                }                                
                            }
                        }
                    }
                }
            }
        }
    }

    private Vector3 getPosition(int row, int file)
    {
        float x = (row - boardWidth / 2) * tileWidth;
        float z = (file - boardHeight / 2) * tileHeight;
        return new Vector3(x, 0, z);
    }
    private void setTransparent()
    {
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                Renderer renderer = cursors[i, j].GetComponent<Renderer>();
                renderer.material = transparentMaterial;
            }
        }
    }

    private List<Vector2Int> getPositions()
    {
        List<Vector2Int> list = new List<Vector2Int>();
        for(int i = 0; i < boardWidth; i++)
        {
            for(int j = 0; j < boardHeight; j++)
            {
                UnitController _unit = units[i, j];
                if (_unit != null)
                    list.Add(units[i, j].getIndex());
            }
        }

        return list;
    }
}
