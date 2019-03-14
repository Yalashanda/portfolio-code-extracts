using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_pick_up : MonoBehaviour {
    public enum PowerUpType {
        SHIELD,
        HEALTH,
        FUEL,
        LASER,
        AUTOSTOP,
        RADAR
    }
    public PowerUpType KindOfPowerUp = PowerUpType.HEALTH;
    public Material[] myMaterials;
    public float moveSpeed = 18.5f;
    public TextMesh myText;
    Mesh myMesh;
    protected Rigidbody myBody;
    Timer startFlight = new Timer();

    enum state {
        dead,
        alive,
        radar
    }

    state currentState = state.alive;

    void Awake() {
        Physics.IgnoreLayerCollision(9, 12);
        Physics.IgnoreLayerCollision(10, 12);
        Physics.IgnoreLayerCollision(11, 12);
        
        startFlight.SetTimer(1.0f);
        myMesh = GetComponent<MeshFilter>().sharedMesh;
    }
    

    void Update() {
        if (s_GameManager.Singleton.GetPaused())
        {
            return;
        }

        stateMachineBehaviour();
    }

    public void OnCollisionEnter(Collision c)
    {
        s_Sailer sailer = c.gameObject.GetComponent<s_Sailer>();
        if(sailer != null && currentState == state.alive)
        {
            AudioControlerScript.Singleton.PlayPowerupCollected_snd(s_Sailer.Singleton.transform.position, false, 0.5f);
            powerupEffect(sailer);
            die();
        }
    }

    public void OnSpawn(Vector3 startPos, PowerUpType myType) {
        transform.position = startPos;
        KindOfPowerUp = myType;
        myText.text = KindOfPowerUp.ToString();
        if (s_Sailer.Singleton.GetRadar())
        {
            createRadarGhost();
            currentState = state.radar;
        }
        else
        {
            
            currentState = state.alive;
            
        }
        GetComponent<Renderer>().material = myMaterials[(int)KindOfPowerUp];
        gameObject.SetActive(true);


       
    }

    protected void die()
    {
        currentState = state.dead;
        gameObject.SetActive(false);
        transform.position = new Vector3(250, -250, 250);

    }
    protected void move()
    {
        if (myBody != null)
        {
            myBody.AddForce(Vector3.up * moveSpeed * myBody.mass, ForceMode.Acceleration);
        }
        else
        {
            myBody = GetComponent<Rigidbody>();
            if (myBody == null)
            {
                Debug.LogError("Variable 'myBody' is null!!");
            }
        }
        if (transform.position.y > 25)
        {
            die();
        }

    }

    //implement effect based on kind of power up
    void powerupEffect(s_Sailer mySailer) {

        switch (KindOfPowerUp)
        {
            case PowerUpType.SHIELD:
                mySailer.ActivateShield();
                break;
            case PowerUpType.HEALTH:
                mySailer.Repair(3);
                break;
            case PowerUpType.FUEL:
                mySailer.GetThrusters().AddFuel(mySailer.GetThrusters().GetMaxFuel()*0.1f);
                break;
            case PowerUpType.LASER:
                mySailer.AddLaser(25);
                break;
            case PowerUpType.AUTOSTOP:
                mySailer.SetAutoStop(true);
                break;
            case PowerUpType.RADAR:
                mySailer.SetRadar(true);
                break;
            default:
                break;
        }
    }


    protected void stateMachineBehaviour()
    {
        switch (currentState)
        {
            case state.dead:
                break;
            case state.alive:
                move();
                break;
            case state.radar:
                if (startFlight.CountDown())
                {
                    currentState = state.alive;
                }
                break;
            default:
                break;
        }
    }

    // if the radar power up is active create a ghost in front of this that moves ahead of it and will show when the player is going to collide with the actual object
    protected void createRadarGhost()
    {

        s_GameManager.Singleton.GetRadarGhost().OnSpawn(transform.position, Instantiate(myMesh));
    }
}
