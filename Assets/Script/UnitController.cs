using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public enum UnitType
{
    None = -1,
    Sui,
    Taishou,
    Chuujou,
    Shoushou,
    Samurai,
    Yari,
    Kiba,
    Shinobi,
    Toride,
    Hyou,
    Oodzutsu,
    Yumi,
    Tsutsu,
    Boushou,
}

public enum FieldStatus
{
    None = -1,
    OnBoard,
    SecondStage,
    ThirdStage,
    Dead,
}

public class UnitController : MonoBehaviour
{
    private int _player;
    private UnitType _unitType;
    private FieldStatus _fieldStatus;

    private const float liftHeight = 0.7f;

    private Vector2Int _position;
    private Vector2Int _oldPosition;

    public void InitUnit(int player, int unittype, Vector3 pos, Vector2Int tile_index)
    {
        this._player = player;
        this._unitType = (UnitType)unittype;
        this._fieldStatus = FieldStatus.OnBoard;
        this._position = tile_index;

        transform.eulerAngles = getAngles(player);
    }

    public Vector3 getAngles(int player)
    {
        return new Vector3(270, player * 180, 0);
    }

    public void LiftUnit()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;

        Vector3 target = transform.position;
        target.y += liftHeight;

        transform.position = target;
    }
    public void LiftOffUnit()
    {
        Vector3 target = transform.position;
        target.y -= liftHeight;

        transform.position= target;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
    }
    public void MoveUnit(Vector3 target, Vector2Int index)
    {
        _oldPosition = _position;
        _position = index;
        transform.position = target;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if(!rigidbody.useGravity)
            rigidbody.useGravity = true;
    }

    public List<Vector2Int> getMovableTiles(CursorController[,] cursors, UnitType unittype)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        int rows = cursors.GetLength(0) / 2;
        int files = cursors.GetLength(1) / 2;

        switch (unittype)
        {
            case UnitType.Sui:
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            list.Add(new Vector2Int(i, j));
                        }
                    }
                    break;
                }
            case UnitType.Taishou:
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (Math.Abs(i) != Math.Abs(j)) continue;
                            list.Add(new Vector2Int(i, j));
                        }
                    }

                    for (int i = -rows*2; i <= rows*2; i++)
                    {
                        if(i == 0) continue;
                        list.Add(new Vector2Int(i, 0));
                    }                        

                    for (int j = -files*2; j <= files*2; j++)
                    {
                        if(j == 0) continue;
                        list.Add(new Vector2Int(0, j));
                    }                        
                    break;
                }                
            case UnitType.Chuujou:
                {
                    for (int i = -1; i <= 1; i++)
                    {
                        for (int j = -1; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (i == j) continue;
                            list.Add(new Vector2Int(i, j));
                        }
                    }

                    for (int i = -rows*2; i <= rows*2; i++)
                        for(int j = -files*2; j <= files*2; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (Math.Abs(i) == Math.Abs(j))
                                list.Add(new Vector2Int(i, j));
                        }

                    break;
                }
            case UnitType.Shoushou:
                {
                    for(int i = -1; i <= 1;  i++)
                    {
                        for(int j = 0; j <= 1; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            list.Add(new Vector2Int(i, j));
                        }
                    }
                    list.Add(new Vector2Int(0, -1));
                    break;
                }
            case UnitType.Samurai:
                {
                    list.Add(new Vector2Int(-1, 1));
                    list.Add(new Vector2Int(0, 1));
                    list.Add(new Vector2Int(1, 1));
                    list.Add(new Vector2Int(0, -1));
                    break;
                }
            case UnitType.Yari:
                {
                    list.Add(new Vector2Int(-1, 1));
                    list.Add(new Vector2Int(0, 1));
                    list.Add(new Vector2Int(1, 1));
                    list.Add(new Vector2Int(0, -1));
                    list.Add(new Vector2Int(0, 2));
                    break;
                }
            case UnitType.Kiba:
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        if (i == 0) continue;
                        list.Add(new Vector2Int(i, 0));
                    }

                    for (int j = -2; j <= 2; j++)
                    {
                        if (j == 0) continue;
                        list.Add(new Vector2Int(0, j));
                    }
                    break;
                }
            case UnitType.Shinobi:
                {
                    for (int i = -2; i <= 2; i++)
                    {
                        for (int j = -2; j <= 2; j++)
                        {
                            if (i == 0 && j == 0) continue;
                            if (Math.Abs(i) != Math.Abs(j)) continue;
                            list.Add(new Vector2Int(i, j));
                        }
                    }
                    break;
                }
            case UnitType.Toride:
                {
                    list.Add(new Vector2Int(0, 1));
                    list.Add(new Vector2Int(-1, 0));
                    list.Add(new Vector2Int(1, 0));
                    list.Add(new Vector2Int(-1, -1));
                    list.Add(new Vector2Int(1, -1));
                    break;
                }
            case UnitType.Hyou:
                {
                    list.Add(new Vector2Int(0, 1));
                    list.Add(new Vector2Int(0, -1));
                }
                break;
            case UnitType.Oodzutsu:
                {
                    list.Add(new Vector2Int(-1, 0));
                    list.Add(new Vector2Int(1, 0));
                    list.Add(new Vector2Int(0, -1));
                    list.Add(new Vector2Int(0, 3));
                    break;
                }
            case UnitType.Yumi:
                {
                    list.Add(new Vector2Int(0, -1));
                    list.Add(new Vector2Int(-1, 2));
                    list.Add(new Vector2Int(0, 2));
                    list.Add(new Vector2Int(1, 2));
                    break;
                }
            case UnitType.Tsutsu:
                {
                    list.Add(new Vector2Int(-1, -1));
                    list.Add(new Vector2Int(1, -1));
                    list.Add(new Vector2Int(0, 2));
                    break;
                }
            case UnitType.Boushou:
                {
                    list.Add(new Vector2Int(-1, 1));
                    list.Add(new Vector2Int(1, 1));
                    list.Add(new Vector2Int(0, -1));
                    break;
                }
        }

        return list;
    }
    public UnitType getUnitType()
    {
        return this._unitType;
    }
    public Vector2Int getIndex()
    {
        return this._position;
    }
}
