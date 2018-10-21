using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : HazardScript {
    public bool WeaponsArmed = true;
    float attackRateReset = 3.0f;
    public Sprite[] ShipImages;
    SpriteRenderer myRenderer = null;
    LootScript myLoot;
    public enum ShipType {
        BOMBER,
        CAPITAL,
        FiGHTER
    }




    

    bool pursued = false;
    FighterScript pursuer = null;
    int[] negPos = { -1, 1 };


    float directionVal = 1;
    float directionValVert = 1;

    ShipType vesselType = ShipType.BOMBER;

    bool SpawnedTopOrBottom = false;

    EnemyMovement myMovement;
    void Awake() {
        myMovement = GetComponent<EnemyMovement>();
    }

    // Use this for initialization
    void Start() {
        GameManagerScript.MyGameManager.AddHazard(this);
        GameManagerScript.MyGameManager.AddEnemy(this);
        myRenderer = GetComponent<SpriteRenderer>();
        OnSpawn(transform.position, vesselType);
        myLoot = GetComponent<LootScript>();

    }

    // Update is called once per frame
    void Update() {
        if (GameManagerScript.MyGameManager.GetPause())
        {
            return;
        }
        onUpdate();

       

    }



    public void OnSpawn(Vector2 pos, ShipType myType)
    {

        base.OnSpawn(pos);
        if (myRenderer == null)
        {
            myRenderer = GetComponent<SpriteRenderer>();
        }
        vesselType = myType;
        setStatsPerType();
        myMovement.OnSpawn(moveSpeed);
        WeaponsArmed = true;
        pursued = false;
    }


    void setStatsPerType() {
        myRenderer.sprite = ShipImages[(int)vesselType];
        switch (vesselType)
        {
            case ShipType.BOMBER:
                //Description: Releases multiple missiles at once and also has a chance of firing a smart missile
                health = 12;
                rawValue = 12;
                SetMoveSpeed(1.0f);
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                attackRateReset = 3.0f;
                break;
            case ShipType.CAPITAL:
                //Description: They fly from one side of the screen to the other, spitting fighters, bombers, and the occasional missile at the Ark.
                health = 100;
                rawValue = 200;
                transform.localScale = new Vector3(1f, 1f, 1f);
                SetMoveSpeed(0.5f);
                attackRateReset = 2.0f;
                capitalOnSpawn();
                break;
            case ShipType.FiGHTER:
                //Description: Mostly they circle your ship and launch missiles at it, occasionally they may also fire energy weapons
                health = 10;
                rawValue = 10;
                SetMoveSpeed(1.5f);
                transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
                attackRateReset = 2.5f;
                break;
            default:
                break;
        }
    }

    void capitalOnSpawn() {
        SpawnedTopOrBottom = false;
        if (transform.position.x > 0)
        {
            directionVal = -1;
        }
        else
        {
            directionVal = 1;
        }
        if (Mathf.Abs(transform.position.y) > GameManagerScript.MyGameManager.GetSpawnDist() - 1)
        {
            SpawnedTopOrBottom = true;
            if (transform.position.y > 0)
            {
                directionValVert = -1;
            }
            else
            {
                directionValVert = 1;
            }
        }
    }



    protected override void die()
    {

        ArkScript.Singleton.AddRawMaterials(rawValue);
        if (vesselType == ShipType.CAPITAL)
        {
            GameManagerScript.MyGameManager.GetCEX().OnSpawn(transform.position);
            AudioControlerScript.Singleton.PlayCapitalExlposionSnd();
            myLoot.GrantLoot(30);
        }
        else
        {
            GameManagerScript.MyGameManager.GetFE().OnSpawn(transform.position);
            AudioControlerScript.Singleton.PlayFighterExplosionSnd(false, 0.25f);
            if (vesselType == ShipType.BOMBER)
            {
                myLoot.GrantLoot(10);
            }
            else
            {
                myLoot.GrantLoot(5);
            }
            AudioControlerScript.Singleton.PlayFighterExplosionSnd(false, 0.25f);
        }
        base.die();
    }

   

    public ShipType GetVesselType() {
        return vesselType;
    }

    public bool GetSpawnedTopOrBottom() {
        return SpawnedTopOrBottom;
    }

    public float GetDirectionValVert()
    {
        return directionValVert;
    }

    public float GetDirectionVal()
    {
        return directionVal;
    }

    public void OnVeer() {

        WeaponsArmed = false;
        directionVal = transform.position.x;
        directionValVert = negPos[Random.Range(0, negPos.Length)];
    }

    public bool GetPursued() {
        return pursued;
    }

    public FighterScript GetPursuer()
    {
        return pursuer;
    }

    public void SetPursued(bool pursu)
    {
        pursued = pursu;
    }

    public bool GetWeaponsArmed() {
        return WeaponsArmed;
    }

    public float GetAttackRateReset() {
        return attackRateReset;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ark")
        {
            collideWithArk();

        }
        if (vesselType != ShipType.CAPITAL)
        {
            if (other.gameObject.GetComponent<FlakScript>() != null)
            {
                FlakScript thisFlak = other.gameObject.GetComponent<FlakScript>();
                if (Random.Range(0, 10) < thisFlak.Effecitveness())
                {
                    die();
                    thisFlak.Impacted();
                }


            }
        }
        

    }

    public void BeingPursued(FighterScript p) {
        pursued = true;
        pursuer = p;
        
    }

    public void PeelOff() {
        pursued = false;
    }
}
