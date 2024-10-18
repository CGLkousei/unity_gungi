using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    private float liftHeight = 0.7f;

    private Vector2Int _position;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void InitUnit(int player, int unittype, Vector3 pos, Vector2Int tile_index)
    {
        this._player = player;
        this._unitType = (UnitType)unittype;
        this._fieldStatus = FieldStatus.OnBoard;

        transform.eulerAngles = getAngles(player);
    }

    public Vector3 getAngles(int player)
    {
        return new Vector3(270, player * 180, 0);
    }

    public void MoveUnit(Vector3 pos, Vector2Int tile_index)
    {
        pos.y = 0;
        transform.position = pos;

        _position = tile_index;
    }

    public void LiftUnit()
    {
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = false;

        Vector3 target = transform.position;
        target.y += liftHeight;

        //transform.position = Vector3.Lerp(transform.position, target, liftSpeed * Time.deltaTime); ;
        transform.position = target;
    }
    public void LiftOffUnit()
    {
        Vector3 target = transform.position;
        target.y -= liftHeight;

        //transform.position = Vector3.Lerp(transform.position, target, liftSpeed * Time.deltaTime);
        transform.position= target;

        Rigidbody rigidbody = GetComponent<Rigidbody>();
        rigidbody.useGravity = true;
    }
    public void MoveUnit(Vector3 target)
    {
        transform.position = target;
        Rigidbody rigidbody = GetComponent<Rigidbody>();
        if(!rigidbody.useGravity)
            rigidbody.useGravity= true;
    }
}
