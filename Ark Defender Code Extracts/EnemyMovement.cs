using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovement : HazardScript {
    public bool EnginesEngaged = true;
    public enum States
    {
        FLYINGTOWARDARK,
        VEERINGOFFFROMARK,
    }
    States currentState = States.FLYINGTOWARDARK;

    Vector2 secondFlightPath = Vector2.zero;
    public Vector2 flightTarget = Vector2.zero;
    public void OnSpawn(float speedOfMovement)
    {
        secondFlightPath = GetFlightOffset();
        currentState = States.FLYINGTOWARDARK;
        moveSpeed = speedOfMovement;
    }

    
    float weightFactor = 1;

    float[] minimumYWeight = { 0.25f, 0.3f, 0.4f };
    public int currentFlightPlan = 0;
    EnemyScript myEnemy;

    // Use this for initialization
    void Start () {
        myEnemy = GetComponent<EnemyScript>();
	}
	
	// Update is called once per frame
	void Update () {
        if (EnginesEngaged)
        {
            FlyPattern();

        }
    }


    void FlyPattern()
    {
        
        switch (myEnemy.GetVesselType())
        {
            case EnemyScript.ShipType.BOMBER:
            case EnemyScript.ShipType.FiGHTER:
                if (!GameManagerScript.MyGameManager.GetManufacturing() && ArkScript.Singleton.gameObject.activeSelf)
                {
                    bomberFlyPattern();
                }
                else
                {
                    flyAroundArk(true);
                }
                break;
            case EnemyScript.ShipType.CAPITAL:
                capitalFlyPattern();
                break;

            default:
                break;
        }
        pointTowardsTarget(flightTarget);
    }

    Vector2 GetFlightOffset()
    {

        float[] val = { GameManagerScript.MyGameManager.GetSpawnDist(), -GameManagerScript.MyGameManager.GetSpawnDist() };
        return new Vector2(0, val[Random.Range(0, val.Length)]);


    }



    void bomberFlyPattern()
    {
        if (currentState == States.FLYINGTOWARDARK)
        {
            veeringFlightPathTowards();

        }
        else
        {
            veeringFlightPathAway();

        }

    }
    void capitalFlyPattern()
    {
        capitalShipFlightPath();
    }


    void veeringFlightPathTowards()
    {
        Vector2 ArkPos = ArkScript.Singleton.gameObject.transform.position;
        float distanceFromShip = Vector2.Distance(transform.position, ArkPos);


        if (distanceFromShip > 1)
        {
            weightFactor = 1 / distanceFromShip;
        }
        else
        {
            weightFactor = 1;

        }

        flightTarget = secondFlightPath * weightFactor + ArkPos * (1 - weightFactor);
        transform.position = Vector2.MoveTowards(transform.position, flightTarget, moveSpeed * Time.deltaTime * GetLevAndEngineAceel());

        if (Vector2.Distance(flightTarget, transform.position) < 0.1f)
        {
            myEnemy.OnVeer();
            currentState = States.VEERINGOFFFROMARK;
            currentFlightPlan = Random.Range(0, minimumYWeight.Length);
        }
    }

    void veeringFlightPathAway()
    {


        Vector2 EndPoint1 = new Vector2(0, GameManagerScript.MyGameManager.GetSpawnDist() * 1.5f * myEnemy.GetDirectionValVert());
        Vector2 EndPoint2 = new Vector2(GameManagerScript.MyGameManager.GetSpawnDist() * 1.5f, 0) * -1 * (myEnemy.GetDirectionVal() / Mathf.Abs(myEnemy.GetDirectionVal()));
        float distanceFromShip = Vector2.Distance(transform.position, EndPoint2);

        if (distanceFromShip > 1)
        {
            weightFactor = 1 / distanceFromShip;

            if (weightFactor < minimumYWeight[currentFlightPlan])
            {
                weightFactor = minimumYWeight[currentFlightPlan];
            }
        }
        else
        {
            weightFactor = 1;

        }

        Vector2 val = EndPoint1 * (weightFactor);
        Vector2 val1 = EndPoint2 * (1 - weightFactor);

        if (float.IsNaN(val.x) || float.IsNaN(val.y) || float.IsNaN(val1.x) || float.IsNaN(val1.y))
        {

        }
        else
        {
            flightTarget = EndPoint1 * (weightFactor) + EndPoint2 * (1 - weightFactor);
        }

        if (float.IsNaN(flightTarget.x))
        {
            Debug.Log("X screwed");
            if (float.IsNaN(flightTarget.y))
            {
                Debug.Log("Y screwed");
            }
        }
        else if (float.IsNaN(flightTarget.y))
        {
            Debug.Log("Y screwed");
        }
        else
        {
            transform.position = Vector2.MoveTowards(transform.position, flightTarget, moveSpeed * Time.deltaTime * GetLevAndEngineAceel());
        }



        if (Vector2.Distance(transform.position, flightTarget) < 0.1f)
        {
            gameObject.SetActive(false);
        }


    }

    void capitalShipFlightPath()
    {


        if (myEnemy.GetSpawnedTopOrBottom())
        {
            flightTarget = new Vector2(transform.position.x, (GameManagerScript.MyGameManager.GetDeathDist() + 2) * myEnemy.GetDirectionValVert());
        }
        else
        {
            flightTarget = new Vector2((GameManagerScript.MyGameManager.GetDeathDist() + 2) * myEnemy.GetDirectionVal(), transform.position.y);
        }
        transform.position = Vector2.MoveTowards(transform.position, (flightTarget * 0.9f + Vector2.zero * 0.1f), moveSpeed * Time.deltaTime);

    }

    void directPath()
    {


        flightTarget = new Vector2(GameManagerScript.MyGameManager.GetDeathDist() + 2, transform.position.y);



        if (transform.position.y > 0)
        {

            transform.position = Vector2.MoveTowards(transform.position, flightTarget, moveSpeed * Time.deltaTime);

        }
        else
        {

            transform.position = Vector2.MoveTowards(transform.position, flightTarget, moveSpeed * Time.deltaTime);

        }
    }
}
