using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements.Experimental;

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
    None = 0,
    OnBoard,
    SecondStage,
    ThirdStage,
}

public class UnitController : MonoBehaviour
{
    private int _player;
    private UnitType _unitType;
    private FieldStatus _fieldStatus;

    private int tile_row;
    private int tile_file;
    private int maxStage;

    private const float liftHeight = 0.7f;
    private float unitHeight;
    private bool onUnit;

    private Vector2Int _position;
    private Vector2Int _oldPosition;

    public void InitUnit(int player, int unittype, Vector2Int tile_index, int row, int file, int max)
    {
        this._player = player;
        this._unitType = (UnitType)unittype;
        this._fieldStatus = FieldStatus.OnBoard;
        this._position = tile_index;
        this.tile_row = row;
        this.tile_file = file;
        this.unitHeight = GetComponent<Renderer>().bounds.size.z;
        this.maxStage = max;
        this.onUnit = false;
        transform.eulerAngles = getAngles(player);
    }

    public Vector3 getAngles(int player) { return new Vector3(270, (player + 1) * 180, 0); }
    public bool isOnUnit() { return onUnit; }
    public void SetOnUnit(bool onUnit) { this.onUnit = onUnit; }
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

        transform.position = target;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
    }
    public void MoveUnit(Vector3 target, Vector2Int index)
    {
        _oldPosition = _position;
        _position = index;


        if (_fieldStatus == FieldStatus.SecondStage)
        {
            target.y = unitHeight * 1.5f;
        }
        else if (_fieldStatus == FieldStatus.ThirdStage)
        {
            target.y = unitHeight * 3.0f;
        }
        else
        {
            target.y = 0;
        }

        transform.position = target;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (!rigidbody.useGravity)
            rigidbody.useGravity = true;
    }

    public List<Vector2Int> getMovableTiles(UnitController[,] units, CursorController[,] cursors)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        Vector2Int target;

        int player_adjust = (_player == 0) ? 1 : -1;

        switch (_unitType)
        {
            case UnitType.Sui:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, -1) * player_adjust, cursors);
                    break;
                }
            case UnitType.Taishou:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, -1) * player_adjust, cursors);

                    for (int i = 1; i < tile_row; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if(addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))                       
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;                        
                    }
                    for (int i = -1; i > -tile_row; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < tile_file; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -tile_file; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Chuujou:
                {
                    addAreaInside(list, new Vector2Int(-1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);

                    int max = (tile_row < tile_file) ? tile_row : tile_file;
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, -i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, -i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(-i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(-i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    break;
                }
            case UnitType.Shoushou:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    break;
                }
            case UnitType.Samurai:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    break;
                }
            case UnitType.Yari:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);

                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Kiba:
                {
                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -3; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -3; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Shinobi:
                {
                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(i, -i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, -i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < 3; i++)
                    {
                        target = _position + new Vector2Int(-i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(-i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -3; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Toride:
                {
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, -1) * player_adjust, cursors);
                    break;
                }
            case UnitType.Hyou:
                {
                    addAreaInside(list, new Vector2Int(0, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                }
                break;
            case UnitType.Oodzutsu:
                {
                    addAreaInside(list, new Vector2Int(1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, 0) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 3) * player_adjust, cursors);
                    break;
                }
            case UnitType.Yumi:
                {
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(-1, 2) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 2) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 2) * player_adjust, cursors);
                    break;
                }
            case UnitType.Tsutsu:
                {
                    addAreaInside(list, new Vector2Int(-1, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, -1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, 2) * player_adjust, cursors);
                    break;
                }
            case UnitType.Boushou:
                {
                    addAreaInside(list, new Vector2Int(-1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(1, 1) * player_adjust, cursors);
                    addAreaInside(list, new Vector2Int(0, -1) * player_adjust, cursors);
                    break;
                }
        }

        return list;
    }
    public UnitType getUnitType() { return this._unitType; }
    public Vector2Int getIndex() { return this._position; }

    private bool addAreaInside(List<Vector2Int> list, Vector2Int index, CursorController[,] cursors)
    {
        Vector2Int target = _position + index;
        if (target.x >= 0 && target.x < tile_row && target.y >= 0 && target.y < tile_file)
        {            
            if (cursors[target.x, target.y].getOnUnitCount() <= (int)_fieldStatus)
            {
                list.Add(target);                
            }
            return true;
        }

        return false;
    }

    public void setStatus(int status) { this._fieldStatus = (FieldStatus)status; }
    public int getFieldStatus() { return (int)this._fieldStatus; }
    public int getPlayer() { return this._player; }
}