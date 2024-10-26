using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using Unity.VisualScripting;

public class GameSceneScript : MonoBehaviour
{
    [SerializeField] GameObject prefabTile;
    [SerializeField] GameObject prefabCursor;    
    [SerializeField] List<GameObject> whiteUnits;
    [SerializeField] List<GameObject> blackUnits;
    [SerializeField] Material transparentMaterial;
    [SerializeField] Material cursorMaterial;
    [SerializeField] Button tukeBtn;
    [SerializeField] Button toruBtn;
    [SerializeField] Button cancelBtn;
    [SerializeField] TextMeshProUGUI turnText;

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
    const int maxStage = 2;
    const int boardWidth = 9;
    const int boardHeight = 9;

    CursorController[,] cursors;
    UnitController[,] units;

    private float tileWidth;
    private float tileHeight;

    private GameObject selectedUnit = null;
    private GameObject selectedCursor = null;
    private GameObject clickedObject = null;
    private List<Vector2Int> selectedUnitMovable = null;

    private bool clickedBtnFlag = false;
    private int checkBtn = -1;
    private int nowPlayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        tukeBtn.onClick.AddListener(() => onClickedBtn(tukeBtn));
        toruBtn.onClick.AddListener(() => onClickedBtn(toruBtn));
        cancelBtn.onClick.AddListener(() => onClickedBtn(cancelBtn));

        cursors = new CursorController[boardWidth, boardHeight];
        units = new UnitController[boardWidth, boardHeight];

        Vector3 tileScale = prefabTile.transform.localScale;
        tileWidth = tileScale.x;
        tileHeight = tileScale.z;

        turnText.color = Color.black;
        turnText.text = "黒のターンです";

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

                if (type == 0)
                {
                    cursorctrl.InitCursor(i, j, pos, 0);
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
                cursorctrl.InitCursor(i, j, pos, 1);

                GameObject unit = Instantiate(prefabUnit, pos, Quaternion.Euler(0, 0, 0));                

                Vector3 newScale = unit.transform.localScale;
                newScale.z *= 2;
                unit.transform.localScale = newScale;

                Rigidbody rigidbody = unit.AddComponent<Rigidbody>();
                rigidbody.mass = 10f;
                rigidbody.drag = 10f;
                rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationY | RigidbodyConstraints.FreezeRotationZ;

                BoxCollider collider = unit.AddComponent<BoxCollider>();
                collider.size = new Vector3(0.03f, 0.03f, 0.003f);
                collider.center = new Vector3(0f, 0f, 0.002f);

                UnitController unitctrl = unit.AddComponent<UnitController>();
                unitctrl.InitUnit(player, type-1, new Vector2Int(i, j), boardWidth, boardHeight, maxStage);

                units[i, j] = unitctrl;
                cursors[i, j] = cursorctrl;
            }
        }        
    }

    // Update is called once per frame
    void Update()
    {
        if (clickedBtnFlag)
        {
            if (checkBtn == 0)
            {                
                UnitController bottomStageUnit = clickedObject.GetComponent<UnitController>();
                UnitController upStageUnit = selectedUnit.GetComponent<UnitController>();

                Rigidbody firstRigid = bottomStageUnit.GetComponent<Rigidbody>();
                firstRigid.isKinematic = false;
                upStageUnit.setStatus(upStageUnit.getFieldStatus() + 1);

                Vector2Int destinationIndex = bottomStageUnit.getIndex();
                Vector2Int departureIndex = upStageUnit.getIndex();
                upStageUnit.MoveUnit(getPosition(destinationIndex.x, destinationIndex.y), destinationIndex);
                if (changePlayer())
                    Debug.Log("Player" + nowPlayer + " win");

                firstRigid.isKinematic = true;

                bottomStageUnit.SetOnUnit(true);
                cursors[departureIndex.x, departureIndex.y].setOnUnitCount(cursors[departureIndex.x, departureIndex.y].getOnUnitCount() - 1);
                cursors[destinationIndex.x, destinationIndex.y].setOnUnitCount(cursors[destinationIndex.x, destinationIndex.y].getOnUnitCount() + 1);

                selectedUnit = null;
                selectedUnitMovable = null;
                setTransparent();
            }
            else if (checkBtn == 1)
            {
                UnitController targetUnitctrl = clickedObject.GetComponent<UnitController>();
                UnitController attackUnitctrl = selectedUnit.GetComponent<UnitController>();
                UnitController targetBottomUnit = null;
                if (targetUnitctrl.getFieldStatus() >= 2)
                {
                    for (int i = 0; i < boardWidth; i++)
                    {
                        for (int j = 0; j < boardHeight; j++)
                        {
                            UnitController _unit = units[i, j];
                            if(_unit != null)
                            {
                                if((_unit.getIndex()  == targetUnitctrl.getIndex()) && _unit != targetUnitctrl)
                                    targetBottomUnit = _unit;
                            }
                        }
                    }
                }

                Vector2Int targetIndex = targetUnitctrl.getIndex();
                Vector2Int attackIndex = attackUnitctrl.getIndex();
                Destroy(targetUnitctrl.gameObject);

                if (targetBottomUnit != null)
                {
                    if(targetUnitctrl.getPlayer() == targetBottomUnit.getPlayer())
                    {
                        attackUnitctrl.MoveUnit(getPosition(targetIndex.x, targetIndex.y), targetIndex);
                        if (changePlayer())
                            Debug.Log("Player" + nowPlayer + " win");

                        Destroy(targetBottomUnit.gameObject);
                        attackUnitctrl.setStatus(1);

                        cursors[attackIndex.x, attackIndex.y].setOnUnitCount(cursors[attackIndex.x, attackIndex.y].getOnUnitCount() - 1);
                        cursors[targetIndex.x, targetIndex.y].setOnUnitCount(1);
                    }
                    else
                    {
                        Rigidbody bottomRigid = targetBottomUnit.GetComponent<Rigidbody>();
                        bottomRigid.isKinematic = false;                        

                        attackUnitctrl.MoveUnit(getPosition(targetIndex.x, targetIndex.y), targetIndex);
                        if (changePlayer())
                            Debug.Log("Player" + nowPlayer + " win");

                        bottomRigid.isKinematic = true;

                        targetBottomUnit.SetOnUnit(true);
                        cursors[attackIndex.x, attackIndex.y].setOnUnitCount(cursors[attackIndex.x, attackIndex.y].getOnUnitCount() - 1);                        
                    }
                }
                else
                {
                    attackUnitctrl.MoveUnit(getPosition(targetIndex.x, targetIndex.y), targetIndex);
                    if (changePlayer())
                        Debug.Log("Player" + nowPlayer + " win");

                    attackUnitctrl.setStatus(1);
                    cursors[attackIndex.x, attackIndex.y].setOnUnitCount(cursors[attackIndex.x, attackIndex.y].getOnUnitCount() - 1);
                    cursors[targetIndex.x, targetIndex.y].setOnUnitCount(1);
                }
                
                selectedUnit = null;
                selectedUnitMovable = null;
                setTransparent();                               
            }
            
            tukeBtn.gameObject.SetActive(false);
            toruBtn.gameObject.SetActive(false);
            cancelBtn.gameObject.SetActive(false);

            checkBtn = -1;
            clickedBtnFlag = false;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit) && !EventSystem.current.IsPointerOverGameObject())
            {
                clickedObject = hit.collider.gameObject;                

                if (selectedUnit == null)
                {                    
                    UnitController unitctrl = clickedObject.GetComponent<UnitController>();
                    if (unitctrl != null)
                    {
                        if (unitctrl.getPlayer() == nowPlayer)
                        {
                            selectedUnit = clickedObject;
                            unitctrl.LiftUnit();
                            selectedUnitMovable = unitctrl.getMovableTiles(units, cursors);
                            foreach (var index in selectedUnitMovable)
                            {
                                Renderer renderer = cursors[index.x, index.y].GetComponent<Renderer>();
                                renderer.material = cursorMaterial;
                            }
                        }
                        else                        
                            selectedUnit = null;                        
                    }                                      
                }
                else
                {
                    Debug.Log(selectedUnit.name);
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
                                Vector2Int unitIndex = _unitctrl.getIndex();
                                Debug.Log(unitIndex.x + ", " + unitIndex.y);
                                Debug.Log(selectedUnitMovable.Count);
                                if (selectedUnitMovable.Contains(unitIndex))
                                {
                                    Debug.Log("Btn");
                                    showBtn(_unitctrl, unitctrl.getPlayer());
                                }
                            }
                        }
                        else
                        {
                            CursorController cursorctrl = clickedObject.GetComponent<CursorController>();
                            if (cursorctrl != null && selectedUnitMovable != null)
                            {
                                Debug.Log(cursorctrl.getOnUnitCount());
                                Vector2Int cursorIndex = cursorctrl.getIndex();                                

                                if (selectedUnitMovable.Contains(cursorIndex))
                                {
                                    selectedUnit = null;
                                    selectedUnitMovable = null;
                                    setTransparent();

                                    Vector2Int unitIndex = unitctrl.getIndex();
                                    cursors[unitIndex.x, unitIndex.y].setOnUnitCount(cursors[unitIndex.x, unitIndex.y].getOnUnitCount()-1);
                                    cursors[cursorIndex.x, cursorIndex.y].setOnUnitCount(cursors[cursorIndex.x, cursorIndex.y].getOnUnitCount() + 1);

                                    unitctrl.MoveUnit(getPosition(cursorIndex.x, cursorIndex.y), cursorIndex);
                                    if (changePlayer())
                                        Debug.Log("Player" + nowPlayer + " win");
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

    private void onClickedBtn(Button button)
    {
        clickedBtnFlag = true;

        if (button == tukeBtn)        
            checkBtn = 0;        
        else if (button == toruBtn)                    
            checkBtn = 1;          
        
    }
    private void showBtn(UnitController clickedUnit, int selectedPlayer)
    {   
        int clickedPlayer = clickedUnit.getPlayer();        

        bool tukeFlag = clickedUnit.getFieldStatus() < maxStage;
        bool toruFlag = clickedPlayer != selectedPlayer;

        if (tukeFlag || toruFlag)
        {
            cancelBtn.gameObject.SetActive(true);

            if (tukeFlag)
                tukeBtn.gameObject.SetActive(true);

            if(toruFlag)
                toruBtn.gameObject.SetActive(true);
        }        
    }

    private bool changePlayer()
    {
        Vector2Int kingIndex = new Vector2Int(-1, -1);
        for(int i = 0; i < boardWidth; i++)
        {
            for(int j = 0; j < boardHeight; j++)
            {
                UnitController unitctrl = units[i, j];
                if (unitctrl == null) continue;
                if (unitctrl.getPlayer() != nowPlayer) continue;

                if (unitctrl.getUnitType() == UnitType.Sui)
                {
                    kingIndex = unitctrl.getIndex();
                    break;
                }
            }
        }
        
        nowPlayer = (nowPlayer + 1) % 2;
        if (nowPlayer == 0)
        {
            turnText.color = Color.black;
            turnText.text = "黒のターンです";
        }
        else if (nowPlayer == 1)
        {
            turnText.color = Color.white;
            turnText.text = "白のターンです";
        }

        for (int i = 0; i < boardWidth; i++)
        {
            for(int j = 0; j < boardHeight; j++)
            {
                UnitController unitctrl = units[i, j];
                if (unitctrl == null) continue;
                if (unitctrl.getPlayer() != nowPlayer) continue;

                List<Vector2Int> area = unitctrl.getMovableTiles(units, cursors);
                if (area.Contains(kingIndex))
                    return true;
            }
        }

        return false;
    }

}
