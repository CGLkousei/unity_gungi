using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

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
    [SerializeField] Canvas FinishCanvas;
    [SerializeField] GameObject BlackKomadai;
    [SerializeField] GameObject WhiteKomadai;

    //Default set;
    static readonly int[,] boardSetting =
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

    static readonly int[] blackCapturedUnit = { 6, 6, 4, 4, 8, 10, 7 };
    static readonly int[] whiteCapturedUnit = { 6, 6, 4, 4, 8, 10, 7 };

    const int playerMax = 2;
    const int maxStage = 2;
    const int boardWidth = 9;
    const int boardHeight = 9;

    CursorController[,] cursors;
    UnitController[,,] units;

    private float tileWidth;
    private float tileHeight;

    private GameObject selectedUnit = null;
    private GameObject selectedCursor = null;
    private GameObject clickedObject = null;
    private List<Vector2Int> selectedUnitMovable = null;

    private bool clickedBtnFlag;
    private bool finishFlag;
    private int checkBtn = -1;
    private int nowPlayer = 0;

    // Start is called before the first frame update
    void Start()
    {
        tukeBtn.onClick.AddListener(() => onClickedBtn(tukeBtn));
        toruBtn.onClick.AddListener(() => onClickedBtn(toruBtn));
        cancelBtn.onClick.AddListener(() => onClickedBtn(cancelBtn));


        cursors = new CursorController[boardWidth, boardHeight];
        units = new UnitController[boardWidth, boardHeight, 2];

        //Vector3 tileScale = prefabTile.transform.localScale;
        Vector3 tileScale = prefabTile.GetComponent<Renderer>().bounds.size;
        tileWidth = tileScale.x;
        tileHeight = tileScale.z;

        turnText.color = Color.black;
        turnText.text = "黒のターンです";

        clickedBtnFlag = false;
        finishFlag = false;

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

                units[i, j, 0] = unitctrl;
                cursors[i, j] = cursorctrl;
            }
        }
        
        Renderer blackRenderer = BlackKomadai.GetComponent<Renderer>();
        Vector3 blackCenter = blackRenderer.bounds.center;
        Vector3 blackSize = blackRenderer.bounds.size;
        float komadaiWidth = blackSize.x;
        float komadaiHeight = blackSize.z;
        int xNum = (int)(komadaiWidth / tileWidth);
        int yNum = (int)(komadaiHeight / tileHeight);

        float xSpace = (komadaiWidth - tileWidth * xNum) / (xNum + 1);
        float ySpace = (komadaiHeight - tileHeight * yNum) / (yNum + 1);

        float xFirst = 0;
        if(xNum % 2 == 0)        
            xFirst = blackCenter.x - (0.5f * tileWidth) - (xSpace * 0.5f) - (xNum * 0.5f - 1) * (tileWidth + xSpace);                    
        else        
            xFirst = blackCenter.x - (int)(xNum / 2) * (tileWidth + xSpace);                    

        float yFirst = 0;
        if(yNum % 2 == 0)        
            yFirst = blackCenter.z + (0.5f * tileHeight) + (ySpace * 0.5f) + (yNum * 0.5f -1) * (tileHeight + ySpace);                    
        else        
            yFirst = blackCenter.z + (int)(yNum / 2) * (tileHeight + ySpace);                   

        for(int i = 0; i < blackCapturedUnit.Length; i++)
        {
            float x = xFirst + (int)(i % xNum) * (tileWidth + xSpace);
            float z = yFirst - (int)(i / xNum) * (tileHeight + ySpace);

            GameObject prefabUnit = blackUnits[blackCapturedUnit[i] - 1];
            GameObject unit = Instantiate(prefabUnit, new Vector3(x, 0, z), Quaternion.Euler(0, 0, 0));

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
            unitctrl.InitCapturedUnit(0, blackCapturedUnit[i]-1, boardWidth, boardHeight, maxStage);
        }

        Renderer whiteRenderer = WhiteKomadai.GetComponent<Renderer>();
        Vector3 whiteCenter = whiteRenderer.bounds.center;
        Vector3 whiteSize = whiteRenderer.bounds.size;
        komadaiWidth = whiteSize.x;
        komadaiHeight = whiteSize.z;
        xNum = (int)(komadaiWidth / tileWidth);
        yNum = (int)(komadaiHeight / tileHeight);

        xSpace = (komadaiWidth - tileWidth * xNum) / (xNum + 1);
        ySpace = (komadaiHeight - tileHeight * yNum) / (yNum + 1);

        xFirst = 0;
        if (xNum % 2 == 0)
            xFirst = whiteCenter.x - (0.5f * tileWidth) - (xSpace * 0.5f) - (xNum * 0.5f - 1) * (tileWidth + xSpace);
        else
            xFirst = whiteCenter.x - (int)(xNum / 2) * (tileWidth + xSpace);

        yFirst = 0;
        if (yNum % 2 == 0)
            yFirst = whiteCenter.z + (0.5f * tileHeight) + (ySpace * 0.5f) + (yNum * 0.5f - 1) * (tileHeight + ySpace);
        else
            yFirst = whiteCenter.z + (int)(yNum / 2) * (tileHeight + ySpace);

        for (int i = 0; i < whiteCapturedUnit.Length; i++)
        {
            float x = xFirst + (int)(i % xNum) * (tileWidth + xSpace);
            float z = yFirst - (int)(i / xNum) * (tileHeight + ySpace);

            GameObject prefabUnit = whiteUnits[whiteCapturedUnit[i] - 1];
            GameObject unit = Instantiate(prefabUnit, new Vector3(x, 0, z), Quaternion.Euler(0, 0, 0));

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
            unitctrl.InitCapturedUnit(1, whiteCapturedUnit[i] - 1, boardWidth, boardHeight, maxStage);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!finishFlag) 
        { 
            if (clickedBtnFlag)
            {
                if (checkBtn == 0)
                {
                    UnitController bottomStageUnit = clickedObject.GetComponent<UnitController>();
                    UnitController upStageUnit = selectedUnit.GetComponent<UnitController>();

                    if (upStageUnit.getFieldStatus() > 1)
                    {
                        UnitController attackBottomUnit = getUnitSameTile(units, upStageUnit);
                        if (attackBottomUnit != null)
                        {
                            attackBottomUnit.SetOnUnit(false);
                            upStageUnit.changeUnitType();
                        }
                    }                    

                    Rigidbody firstRigid = bottomStageUnit.GetComponent<Rigidbody>();
                    firstRigid.isKinematic = false;                    

                    Vector2Int destinationIndex = bottomStageUnit.getIndex();
                    Vector2Int departureIndex = upStageUnit.getIndex();

                    if (upStageUnit.getFieldStatus() > 0)
                    {
                        cursors[departureIndex.x, departureIndex.y].setOnUnitCount(cursors[departureIndex.x, departureIndex.y].getOnUnitCount() - 1);
                        cursors[destinationIndex.x, destinationIndex.y].setOnUnitCount(cursors[destinationIndex.x, destinationIndex.y].getOnUnitCount() + 1);
                    }
                    else if (upStageUnit.getFieldStatus() == 0)
                    {
                        cursors[destinationIndex.x, destinationIndex.y].setOnUnitCount(cursors[destinationIndex.x, destinationIndex.y].getOnUnitCount() + 1);
                    }
                    upStageUnit.MoveUnit(units, getPosition(destinationIndex.x, destinationIndex.y), destinationIndex, 2);
                    if (changePlayer())
                        finishFlag = true;

                    firstRigid.isKinematic = true;

                    upStageUnit.changeUnitType();
                    bottomStageUnit.SetOnUnit(true);                    

                    selectedUnit = null;
                    selectedUnitMovable = null;
                    setTransparent();
                }
                else if (checkBtn == 1)
                {
                    UnitController targetUnitctrl = clickedObject.GetComponent<UnitController>();
                    UnitController attackUnitctrl = selectedUnit.GetComponent<UnitController>();
                    UnitController targetBottomUnit = getUnitSameTile(units, targetUnitctrl);

                    Vector2Int targetIndex = targetUnitctrl.getIndex();
                    Vector2Int attackIndex = attackUnitctrl.getIndex();
                    Destroy(targetUnitctrl.gameObject);

                    if (attackUnitctrl.getFieldStatus() > 1)
                    {
                        UnitController attackBottomUnit = getUnitSameTile(units, attackUnitctrl);
                        if (attackBottomUnit != null)
                        {
                            attackBottomUnit.SetOnUnit(false);
                            attackUnitctrl.changeUnitType();
                        }
                    }

                    if (targetBottomUnit != null)
                    {
                        if (targetUnitctrl.getPlayer() == targetBottomUnit.getPlayer())
                        {
                            attackUnitctrl.MoveUnit(units, getPosition(targetIndex.x, targetIndex.y), targetIndex, 1);
                            if (changePlayer())
                                finishFlag = true;

                            Destroy(targetBottomUnit.gameObject);                            

                            cursors[attackIndex.x, attackIndex.y].setOnUnitCount(cursors[attackIndex.x, attackIndex.y].getOnUnitCount() - 1);
                            cursors[targetIndex.x, targetIndex.y].setOnUnitCount(1);
                        }
                        else
                        {
                            Rigidbody bottomRigid = targetBottomUnit.GetComponent<Rigidbody>();
                            bottomRigid.isKinematic = false;

                            attackUnitctrl.MoveUnit(units, getPosition(targetIndex.x, targetIndex.y), targetIndex, 2);
                            if (changePlayer())
                                finishFlag = true;

                            bottomRigid.isKinematic = true;

                            attackUnitctrl.changeUnitType();
                            targetBottomUnit.SetOnUnit(true);
                            cursors[attackIndex.x, attackIndex.y].setOnUnitCount(cursors[attackIndex.x, attackIndex.y].getOnUnitCount() - 1);
                        }
                    }
                    else
                    {
                        attackUnitctrl.MoveUnit(units, getPosition(targetIndex.x, targetIndex.y), targetIndex, 1);
                        if (changePlayer())
                            finishFlag = true;

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
                            if (!unitctrl.isOnUnit())
                            {
                                if (unitctrl.getPlayer() == nowPlayer)
                                {
                                    selectedUnit = clickedObject;
                                    unitctrl.LiftUnit();

                                    if(unitctrl.getFieldStatus() == 0)
                                    {
                                        selectedUnitMovable = getMovableCapturedUnit(units, unitctrl);
                                    }
                                    else
                                    {
                                        selectedUnitMovable = unitctrl.getMovableTiles(cursors);                                        
                                    }
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
                                    if (!_unitctrl.isOnUnit())
                                    {
                                        Vector2Int unitIndex = _unitctrl.getIndex();
                                        if (selectedUnitMovable.Contains(unitIndex))
                                        {
                                            showBtn(_unitctrl, unitctrl.getPlayer());
                                        }
                                    }
                                }
                            }
                            else
                            {
                                CursorController cursorctrl = clickedObject.GetComponent<CursorController>();
                                if (cursorctrl != null && selectedUnitMovable != null)
                                {
                                    Vector2Int cursorIndex = cursorctrl.getIndex();

                                    if (selectedUnitMovable.Contains(cursorIndex))
                                    {
                                        selectedUnit = null;
                                        selectedUnitMovable = null;
                                        setTransparent();

                                        if (unitctrl.getFieldStatus() > 1)
                                        {
                                            UnitController attackBottomUnit = getUnitSameTile(units, unitctrl);
                                            if (attackBottomUnit != null)
                                            {
                                                attackBottomUnit.SetOnUnit(false);
                                                unitctrl.changeUnitType();
                                            }
                                        }

                                        Vector2Int unitIndex = unitctrl.getIndex();
                                        if(unitctrl.getFieldStatus() > 0)
                                        {
                                            cursors[unitIndex.x, unitIndex.y].setOnUnitCount(cursors[unitIndex.x, unitIndex.y].getOnUnitCount() - 1);
                                            cursors[cursorIndex.x, cursorIndex.y].setOnUnitCount(cursors[cursorIndex.x, cursorIndex.y].getOnUnitCount() + 1);
                                        }
                                        else if(unitctrl.getFieldStatus() == 0)
                                        {
                                            cursors[cursorIndex.x, cursorIndex.y].setOnUnitCount(cursors[cursorIndex.x, cursorIndex.y].getOnUnitCount() + 1);
                                        }

                                        unitctrl.MoveUnit(units, getPosition(cursorIndex.x, cursorIndex.y), cursorIndex, 1);
                                        if (changePlayer())
                                            finishFlag = true;
                                    }
                                    else if (cursorIndex == unitctrl.getIndex())
                                    {
                                        if(unitctrl != null) {                             
                                            setTransparent();
                                            unitctrl.LiftOffUnit();
                                        }
                                        selectedUnit = null;
                                        selectedUnitMovable = null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
        else
        {
            FinishCanvas.gameObject.SetActive(true);
            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = 0; j < boardHeight; j++)
                {
                    for(int k = 0; k < 2; k++)
                    {
                        if (units[i, j, k] != null)
                            Destroy(units[i, j, k]);                        
                    }

                    if (cursors[i, j] != null)
                        Destroy(cursors[i, j]);
                }
            }

            TextMeshProUGUI winnerTxt = FinishCanvas.transform.Find("WinnerText").GetComponent<TextMeshProUGUI>();
            if (nowPlayer == 0)
            {
                turnText.color = Color.black;
                turnText.text = "黒の勝利です";
            }
            else if (nowPlayer == 1)
            {
                turnText.color = Color.white;
                turnText.text = "白の勝利です";
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
                UnitController unitctrl = units[i, j, 0];
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
                for(int k = 0; k < 2; k++)
                {
                    UnitController unitctrl = units[i, j, k];
                    if (unitctrl == null) continue;
                    if (unitctrl.getPlayer() != nowPlayer) continue;
                    if (unitctrl.isOnUnit()) continue;

                    List<Vector2Int> area = unitctrl.getMovableTiles(cursors);
                    if (area.Contains(kingIndex))
                        return true;
                }                
            }
        }

        return false;
    }
    UnitController getUnitSameTile(UnitController[,,] units, UnitController u)
    {
        int height = u.getFieldStatus() % 2;
        UnitController _unit = null;
        for (int i = 0; i < boardWidth; i++)
        {
            for (int j = 0; j < boardHeight; j++)
            {
                if (units[i, j, height] != null)
                {
                    if (units[i, j, height].getIndex() == u.getIndex() && units[i, j, height] != u)
                    {
                        _unit = units[i, j, height];
                    }
                }                
            }
        }

        return _unit;
    }
    public List<Vector2Int> getMovableCapturedUnit(UnitController[,,] units, UnitController u)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        int row = -1;
        if(u.getPlayer() == 0)
        {
            row = 0;
            bool flag = false;
            for(int j = boardHeight-1; j >= 0; j--)
            {
                for(int i = 0; i < boardWidth; i++)
                {
                    if (units[i, j, 0] != null)
                    {
                        if (units[i, j, 0].getPlayer() == 0)
                        {                            
                            row = j;
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                    break;
            }

            for(int i = 0; i < boardWidth; i++)
            {
                for(int j = 0; j <= row; j++)
                {
                    if (units[i, j, 1] != null)
                        continue;

                    if (units[i, j, 0] != null)
                    {
                        if (units[i, j, 0].getUnitType() == UnitType.Sui)
                            continue;
                    }                    

                    list.Add(new Vector2Int(i, j));
                }
            }
        }
        else
        {
            row = boardHeight - 1;
            bool flag = false;
            for (int j = 0; j < boardHeight; j++)
            {
                for (int i = 0; i < boardWidth; i++)
                {
                    if (units[i, j, 0] != null)
                    {
                        if (units[i, j, 0].getPlayer() == 1)
                        {
                            row = j;
                            flag = true;
                            break;
                        }
                    }
                }
                if (flag)
                    break;
            }

            for (int i = 0; i < boardWidth; i++)
            {
                for (int j = boardHeight-1; j >= row; j--)
                {
                    if (units[i, j, 1] != null)
                        continue;

                    if (units[i, j, 0] != null)
                    {
                        if (units[i, j, 0].getUnitType() == UnitType.Sui)
                            continue;
                    }

                    list.Add(new Vector2Int(i, j));
                }
            }
        }

        return list;
    }
}
