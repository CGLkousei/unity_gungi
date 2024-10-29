using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UIElements.Experimental;
using System.Linq;

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
    SuiSecond,
    TaishouSecond,
    ChuujouSecond,
    ShoushouSecond,
    SamuraiSecond,
    YariSecond,
    KibaSecond,
    ShinobiSecond,
    TorideSecond,
    HyouSecond,
    OodzutsuSecond,
    YumiSecond,
    TsutsuSecond,
    BoushouSecond,
}

public enum FieldStatus
{
    None = -1,
    arata,
    OnBoard,
    SecondStage,
    ThirdStage,
}


public class UnitController : MonoBehaviour
{
    public static readonly Dictionary<UnitType, UnitType> correspondTable = new Dictionary<UnitType, UnitType>()
    {
        { UnitType.Sui, UnitType.SuiSecond },
        { UnitType.Taishou, UnitType.TaishouSecond },
        { UnitType.Chuujou, UnitType.ChuujouSecond },
        { UnitType.Shoushou, UnitType.ShoushouSecond },
        { UnitType.Samurai, UnitType.SamuraiSecond },
        { UnitType.Yari, UnitType.YariSecond },
        { UnitType.Kiba, UnitType.KibaSecond },
        { UnitType.Shinobi, UnitType.ShinobiSecond },
        { UnitType.Toride, UnitType.TorideSecond },
        { UnitType.Hyou, UnitType.HyouSecond },
        { UnitType.Oodzutsu, UnitType.OodzutsuSecond },
        { UnitType.Yumi, UnitType.YumiSecond },
        { UnitType.Tsutsu, UnitType.TsutsuSecond },
        { UnitType.Boushou, UnitType.BoushouSecond }
    };

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

    public void InitCapturedUnit(int player, int unittype, int row, int file, int max)
    {
        this._player = player;
        this._unitType = (UnitType)unittype;
        this._fieldStatus = FieldStatus.arata;
        this._position = new Vector2Int(-1, -1);
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
    public void MoveUnit(UnitController[,,] units, Vector3 target, Vector2Int index, int destiLayer)
    {
        if ((int)_fieldStatus > 0)
        {
            units[index.x, index.y, destiLayer - 1] = units[_position.x, _position.y, (int)_fieldStatus - 1];
            units[_position.x, _position.y, (int)_fieldStatus - 1] = null;
        }
        else if ((int)_fieldStatus == 0)
        {
            units[index.x, index.y, destiLayer - 1] = this;
        }
 
        _oldPosition = _position;
        _position = index;

        if ((FieldStatus)destiLayer == FieldStatus.SecondStage)
        {
            target.y = unitHeight * 1.5f;
        }
        else if ((FieldStatus)destiLayer == FieldStatus.ThirdStage)
        {
            target.y = unitHeight * 3.0f;
        }
        else
        {
            target.y = 0;
        }        

        _fieldStatus = (FieldStatus)destiLayer;
        transform.position = target;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if (!rigidbody.useGravity)
            rigidbody.useGravity = true;
    }

    public List<Vector2Int> getMovableTiles(CursorController[,] cursors)
    {
        List<Vector2Int> list = new List<Vector2Int>();
        Vector2Int target;

        int player_adjust = (_player == 0) ? 1 : -1;

        switch (_unitType)
        {
            case UnitType.Sui:
            case UnitType.SuiSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Sui)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

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
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Taishou:
            case UnitType.TaishouSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Taishou)
                        max = 2;
                    else
                        max = 3;

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
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

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
            case UnitType.ChuujouSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Chuujou)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    max = (tile_row < tile_file) ? tile_row : tile_file;
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
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    break;
                }
            case UnitType.Shoushou:
            case UnitType.ShoushouSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Shoushou)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
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
                    break;
                }
            case UnitType.Samurai:
            case UnitType.SamuraiSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Samurai)
                        max = 2;
                    else
                        max = 3;


                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
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
                    break;
                }
            case UnitType.Yari:
            case UnitType.YariSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Yari)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < (max + 1); i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
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
                    break;
                }
            case UnitType.Kiba:
            case UnitType.KibaSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Kiba)
                        max = 3;
                    else
                        max = 4;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Shinobi:
            case UnitType.ShinobiSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Shinobi)
                        max = 3;
                    else
                        max = 4;

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
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Toride:
            case UnitType.TorideSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Toride)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
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
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Hyou:
            case UnitType.HyouSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Hyou)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }                
            case UnitType.Oodzutsu:
            case UnitType.OodzutsuSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Oodzutsu)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(i, 0) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, 0) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 3; i < (max + 2); i++)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Yumi:
            case UnitType.YumiSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Yumi)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, 1) + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, 1) + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, 1) + new Vector2Int(-i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(-i, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Tsutsu:
            case UnitType.TsutsuSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Tsutsu)
                        max = 2;
                    else
                        max = 3;

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(0, 1) + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
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
                        target = _position + new Vector2Int(-i, -i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(-i, -i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }
                    break;
                }
            case UnitType.Boushou:
            case UnitType.BoushouSecond:
                {
                    int max = 0;
                    if (_unitType == UnitType.Boushou)
                        max = 2;
                    else
                        max = 3;

                    for (int i = -1; i > -max; i--)
                    {
                        target = _position + new Vector2Int(0, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(0, i) * player_adjust, cursors))
                            if (cursors[target.x, target.y].getOnUnitCount() > 0)
                                break;
                    }

                    for (int i = 1; i < max; i++)
                    {
                        target = _position + new Vector2Int(i, i) * player_adjust;
                        if (addAreaInside(list, new Vector2Int(i, i) * player_adjust, cursors))
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
            if(_unitType == UnitType.Sui)
            {
                if (cursors[target.x, target.y].getOnUnitCount() < 1)
                {
                    list.Add(target);
                    return true;
                }
            }
            else
            {
                if (cursors[target.x, target.y].getOnUnitCount() <= (int)_fieldStatus)
                {
                    list.Add(target);
                    return true;
                }                
            }            
        }

        return false;
    }

    public void setStatus(int status) { this._fieldStatus = (FieldStatus)status; }
    public int getFieldStatus() { return (int)this._fieldStatus; }
    public int getPlayer() { return this._player; }

    public void changeUnitType()
    {
        if (correspondTable.ContainsKey(_unitType))
        {
            _unitType = correspondTable[_unitType];
        }
        else if(correspondTable.ContainsValue(_unitType))
        {
            _unitType = correspondTable.FirstOrDefault(pair => pair.Value == _unitType).Key;
        }
    }
}