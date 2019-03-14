using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DetachableObject : MonoBehaviour {
    public bool IsThrusterDamageObject;
    public ThrusterScript.direction ThrusterToDamage;
    public float SpinRate = 3.0f;
    public bool OneAtATime = false;
    Collider myColl;        
    public Timer recollide = new Timer();
    public Vector3 startPos;
    public Vector3 startRot;
    public Vector3 startSca;
    Rigidbody myBody;
    // Use this for initialization
    void Awake () {
        //keeps the objects from colliding with a variety of other objects including edges, player constraints, and the player themself
        Physics.IgnoreLayerCollision(0, 11);
        Physics.IgnoreLayerCollision(9, 11);
        Physics.IgnoreLayerCollision(11, 11);
        //make sure the object can be returned to default position, rotation, and scale
        startPos = transform.localPosition;
        startRot = transform.localEulerAngles;
        startSca = transform.localScale;
        myColl = GetComponent<Collider>();
        myBody = GetComponent<Rigidbody>();
        recollide.SetTimer(2.0f);

    }
    void Update() {
        //ensures only one piece can be detached per collision, this avoids a collision knocking off all the pieces allowing a more gradual destruction.
        if (OneAtATime)
        {
            if (recollide.CountDown())
            {
                OneAtATime = false;
                s_Sailer.Singleton.ReActivateCollHotRods(this);
            }
        }

        

    }

    public Collider GetMyColl() {
        return myColl;
    }
    public Rigidbody GetBody() {
        return myBody;
    }
	
    public void OnCollisionEnter(Collision c)
    {
        if (!OneAtATime && !s_Sailer.Singleton.IsShieldOn())//makes sure the shield is off and another part has not recently been knocked off
        {
            
            if (c.gameObject.GetComponent<ObstacleScript>() != null)
            {
                myBody.isKinematic = false;
                if (transform.parent != null)
                {

                    //adds spin to the piece knocked off so flies off in space spinning away
                    myBody.AddTorque(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)) * SpinRate, ForceMode.VelocityChange);
                    myBody.AddForce(new Vector3(Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f), Random.Range(-1.0f, 1.0f)), ForceMode.VelocityChange);
                    AudioControlerScript.Singleton.PlayCrash_snd(transform.position);
                    OneAtATime = true;
                    //enforces no other things should be knocked off beyond the one
                    s_Sailer.Singleton.DeActivateCollHotRods(this);
                    Physics.IgnoreCollision(GetComponent<Collider>(), c.collider);
                    Vector3 startCenter = transform.parent.transform.position;
                    transform.SetParent(null);
                    transform.position = startCenter + transform.localPosition;
                    //if the piece knocked off is flagged as a thruster damage thrusters
                    if (IsThrusterDamageObject)
                    {
                        damageTruster();
                    }
                }

            }
        }
        
    }

    public void OnCollisionExit(Collision c)
    {
        if (c.gameObject.GetComponent<ObstacleScript>() != null)
        {

            if (transform.parent != null)
            {
                //OneAtATime = false;
                Physics.IgnoreCollision(GetComponent<Collider>(), c.collider, false);
            }

        }
    }

    void damageTruster() {
        s_Sailer.Singleton.GetThrusters().AddDamagedThruster(ThrusterToDamage);
    }
}
