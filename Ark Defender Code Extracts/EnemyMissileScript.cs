using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMissileScript : HazardScript {
    
    protected bool targetLocked = false;
    protected GameObject targetedShip = null;
    protected Vector2 targetedPosition = new Vector2(0, 0);
    bool smart = false;
    ProjectileScript smartAvoider;
    FlakScript smartAvoider1;
    int smartAvoiderChoice = 0;

    void Start() {
        moveSpeed = 1.75f;
        health = 5;
        damage = 15;
        GameManagerScript.MyGameManager.AddEMs(this);
        GameManagerScript.MyGameManager.AddHazard(this);
    }

    public virtual void OnSpawn(Vector2 myTarget, Vector2 startPos)
    {
        targetedPosition = myTarget;
        OnSpawnAll(startPos);
    }

    public virtual void OnSpawn(GameObject myTarget, Vector2 startPos)
    {
        targetLocked = true;
        targetedShip = myTarget;
        OnSpawnAll(startPos);


    }

    void OnSpawnAll(Vector2 spawnPos)
    {
        smart = false;
        damage = 15;
        transform.position = spawnPos;
        gameObject.SetActive(true);
        
        if (WaveScript.Singleton.GetCurrentWave() > 7)
        {
            if (Random.Range(0, WaveScript.Singleton.GetCurrentWave()) > 4 + Mathf.RoundToInt(WaveScript.Singleton.GetCurrentWave() * 0.25f))
            {
                if (Random.Range(0, 2) == 0)
                {
                    smart = true;
                    damage = 20;
                }
            }
        }


    }

    Vector2 AroundDangerPoint(ProjectileScript threat) {
        float offset = 3.0f;
        if (smartAvoider == null)
        {
            smartAvoider = threat;
            
            if (Mathf.Abs(transform.position.y - threat.gameObject.transform.position.y) >
                Mathf.Abs(transform.position.x - threat.gameObject.transform.position.x))
            {
                if (Random.Range(0, 2) == 0)
                {
                    smartAvoiderChoice = 0;
                    return new Vector2(threat.GetTargetedPosition().x + offset, threat.GetTargetedPosition().y);
                }
                else
                {
                    smartAvoiderChoice = 1;
                    return new Vector2(threat.GetTargetedPosition().x - offset, threat.GetTargetedPosition().y);
                }
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    smartAvoiderChoice = 2;
                    return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y + offset);
                }
                else
                {
                    smartAvoiderChoice = 3;
                    return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y - offset);
                }
                    
            }

        }
        else
        {
            if (smartAvoiderChoice == 0)
            {
                 
                return new Vector2(threat.GetTargetedPosition().x + offset, threat.GetTargetedPosition().y);
            }
            else if (smartAvoiderChoice == 1)
            {

                return new Vector2(threat.GetTargetedPosition().x - offset, threat.GetTargetedPosition().y);
            }
            else if (smartAvoiderChoice == 2)
            {
                    
                return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y + offset);
            }
            else 
            {
                    
                return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y - offset);
            }
            
        }
        
        
        

        
    }
    Vector2 AroundDangerPoint(FlakScript threat)
    {
        float offset = 4.0f;
        if (smartAvoider1 == null)
        {
            smartAvoider1 = threat;

            if (Mathf.Abs(transform.position.y - threat.gameObject.transform.position.y) >
                Mathf.Abs(transform.position.x - threat.gameObject.transform.position.x))
            {
                smartAvoiderChoice = 0;
                return new Vector2(threat.GetTargetedPosition().x - offset, threat.GetTargetedPosition().y);
                   
            }
            else
            {
                if (Random.Range(0, 2) == 0)
                {
                    smartAvoiderChoice = 2;
                    return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y + offset);
                }
                else
                {
                    smartAvoiderChoice = 3;
                    return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y - offset);
                }

            }

        }
        else
        {
            if (smartAvoiderChoice == 0)
            {

                return new Vector2(threat.GetTargetedPosition().x - offset, threat.GetTargetedPosition().y);
            }
            else if (smartAvoiderChoice == 2)
            {

                return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y + offset);
            }
            else
            {

                return new Vector2(threat.GetTargetedPosition().x, threat.GetTargetedPosition().y - offset);
            }

        }





    }

    protected virtual void moveTowardsTarget()
    {
        if (smart)
        {
            //GetFlakList();
            ProjectileScript threat = findProjectileInRange();
            FlakScript threat2 = findFlakInRange();
            if (threat != null || threat2 != null)
            {

                smartAvoidance(threat, threat2);
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, targetedPosition, moveSpeed * Time.deltaTime * GetLevAndEngineAceel());
                pointTowardsTarget(targetedPosition);
            }
        }
        else
        {
            if (targetLocked)
            {
                transform.position = Vector2.MoveTowards(transform.position, targetedShip.transform.position, moveSpeed * Time.deltaTime * GetLevAndEngineAceel());
                pointTowardsTarget(targetedShip.transform.position);

            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, targetedPosition, moveSpeed * Time.deltaTime * GetLevAndEngineAceel());
                pointTowardsTarget(targetedPosition);

            }
        }
        

    }

    void  smartAvoidance(ProjectileScript threat, FlakScript threat2)
    {

        if (threat2 == null)
        {
            aroundMissile(threat);
        }
        else
        {
            if (threat != null)
            {
                if (Vector2.Distance(threat.transform.position, transform.position) < Vector2.Distance(threat2.transform.position, transform.position))
                {
                    
                    aroundMissile(threat);
                }
                else
                {
                    aroundFlak(threat2);
                }

            }
            else
            {
                aroundFlak(threat2);
            }

        }

    }

    void aroundMissile(ProjectileScript threat) {
        Vector2 aroundThreat = AroundDangerPoint(threat);
        Vector2 avoidenceVector = targetedPosition * 0.3f + aroundThreat * 0.7f;
        transform.position = Vector2.MoveTowards(transform.position, avoidenceVector, moveSpeed * Time.deltaTime * GetLevAndEngineAceel() * 1.25f);
        pointTowardsTarget(avoidenceVector);

        if (!smartAvoider.gameObject.activeSelf)
        {
            smartAvoider = null;
        }
    }

    void aroundFlak(FlakScript threat2) {
        Vector2 aroundThreat = AroundDangerPoint(threat2);
        Vector2 avoidenceVector = targetedPosition * 0.3f + aroundThreat * 0.7f;
        transform.position = Vector2.MoveTowards(transform.position, avoidenceVector, moveSpeed * Time.deltaTime * GetLevAndEngineAceel() * 1.25f);
        pointTowardsTarget(avoidenceVector);

        if (!smartAvoider1.gameObject.activeSelf)// || Vector2.Distance(smartAvoider1.gameObject.transform.position, transform.position) > 2)
        {
            smartAvoider1 = null;
        }
    }

    ProjectileScript findProjectileInRange()
    {
        float minDist = 4;
        for (int i = 0; i < GameManagerScript.MyGameManager.GetProjectileList().Count; i++)
        {
            if (GameManagerScript.MyGameManager.GetProjectileList()[i].gameObject.activeSelf)
            {
                if (Vector2.Distance(transform.position, GameManagerScript.MyGameManager.GetProjectileList()[i].transform.position) < minDist)
                {
                    return GameManagerScript.MyGameManager.GetProjectileList()[i];
                }
            }
        }
        return null;
    }


    FlakScript findFlakInRange()
    {
        float minDist = 4;
        for (int i = 0; i < GameManagerScript.MyGameManager.GetFlakList().Count; i++)
        {
            if (GameManagerScript.MyGameManager.GetFlakList()[i].gameObject.activeSelf)
            {
                if (Vector2.Distance(transform.position, GameManagerScript.MyGameManager.GetFlakList()[i].transform.position) < minDist)
                {
                    return GameManagerScript.MyGameManager.GetFlakList()[i];
                }
            }
        }
        return null;
    }

    protected override void die()
    {
        detonate();
        gameObject.SetActive(false);
        resetValues();
    }

    void resetValues()
    {
        targetLocked = false;
        targetedShip = null;
        targetedPosition = new Vector2(0, 5);
        smart = false;
    }

   
    protected void functionsInUpdate()
    {
        invulCountDown();
        
        if (!GameManagerScript.MyGameManager.GetManufacturing())
        {
            moveTowardsTarget();
            
        }
        else
        {
            flyAroundArk(true);
            
        }
        testDistanceFromArk();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.MyGameManager.GetPause())
        {
            return;
        }
        if (!ArkScript.Singleton.gameObject.activeSelf)
        {
            die();
        }
        functionsInUpdate();
    }


    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (other.tag == "Ark")
        {
            collideWithArk();
           
        }
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

   

    void detonate()
    {

        GameManagerScript.MyGameManager.GetEME().OnSpawn(transform.position);
        AudioControlerScript.Singleton.PlayFighterExplosionSnd(false, 0.65f);
    }

   
}
