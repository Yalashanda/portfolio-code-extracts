using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_Sailer : MonoBehaviour {
    public Animator MyAnimator;
    public GameObject Board;
    public bool DifferentDamage = false;
    float cheatDam = 0;
    float health = 10;
    float healthMax = 10;
    Collider[] myColliders;
    enum states {
        alive,
        dead
    }
    states currentState = states.alive;
    Timer invulTimer = new Timer();
    Timer hitEffectTimer = new Timer();
    bool hit = false;
    ThrusterScript myThruster;
    public s_SegementStatBar myHealthBar;
    public s_SegementStatBar mylaserBar;
    public s_StatBarLinear myWatchLaserBar;

    public static s_Sailer Singleton;
    bool shield = false;
    public GameObject ShieldObj;
    public GameObject LaserObj;
    int shieldStr = 3;
    int shieldStrMax = 3;
    


    bool autoStop = false;
    Timer autoStopTime = new Timer();

    bool radar = false;
    Timer radarTime = new Timer();

    bool firingLaser = false;
    float laser = 0;
    float laserMax = 100;
    Timer laserTime = new Timer();
    

    DetachableObject[] myHotRods;

    void Awake() {
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }
    }
    void Start() {
        
        radarTime.SetTimer(25.0f);
        autoStopTime.SetTimer(15.0f);
        laserTime.SetTimer(10.0f);
        myThruster = GetComponent<ThrusterScript>();
        myHealthBar.OnSpawn(health, healthMax);
        mylaserBar.OnSpawn(laser, laserMax);
        if (myWatchLaserBar != null)
        {
            myWatchLaserBar.SetLinearBarExtremes();
            myWatchLaserBar.SetBar(laser, laserMax);
        }
        myHotRods = GetComponentsInChildren<DetachableObject>();
        invulTimer.SetTimer(1.0f);
        hitEffectTimer.SetTimer(0.5f);
        myColliders = GetComponentsInChildren<Collider>();
    }

    void Update() {
        if (s_GameManager.Singleton.GetPaused())
        {
            return;
        }

        if (hitEffectTimer.ShouldCountDown())
        {
            GetComponent<s_OnHitEffects>().HitEnd();
        }

        if (hit)
        {
            if (invulTimer.CountDown())
            {
                hit = false;
            }
        }
        //if laser in charged and laser buttons pressed fire the laser
        if (laser >= laserMax)
        {
            
            if (
                (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_LR) && SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_LL)) ||
                (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_UR) && SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_UL))
                )
            {
                fireLaser();
            }
        }


        powerUpTimers();
    }

    public void TakeDamage(float val) {
        if (!hit)
        {
            hit = true;
            if (shield)
            {
                ShieldTakeDamage((int)val);

                
            }
            else
            {
                if (currentState == states.alive)
                {
                    health -= val;
                    AudioControlerScript.Singleton.PlayCrash_snd(transform.position);
                    GetComponent<s_OnHitEffects>().OnHit();
                    hitEffectTimer.SetShouldCountDown(true);

                    if (health < 1)
                    {
                        die();
                    }
                    myHealthBar.SetBar(health, healthMax);

                    if (laser < laserMax)
                    {
                        laser -= laser - (val * 5) > 0 ? (val * 5) : laser;
                        mylaserBar.SetBar(laser, laserMax);
                        if (myWatchLaserBar != null)
                        {
                            myWatchLaserBar.SetBar(laser, laserMax);
                        }
                    }
                }

            }
        }
        
    }

    public void Repair(float val) {
        health += val;
        if (health > healthMax)
        {
            health = healthMax;
        }
        myHealthBar.SetBar(health, healthMax);
    }

    public float GetHealth() {
        return health;
    }
    public void SetHealth(float val) {
        health = val;
        myHealthBar.SetBar(health, healthMax);
    }

    public void ActivateShield()
    {
        shieldStr = shieldStrMax;
        shield = true;
        ShieldObj.SetActive(true);
    }

    public void DeactivateShield()
    {
        AudioControlerScript.Singleton.PlayShieldCollapse_snd(transform.position);
        shield = false;
        ShieldObj.SetActive(false);
    }
    //if the shield is active it will take damage before the player
    public void ShieldTakeDamage(int val) {
        shieldStr -= val;
        AudioControlerScript.Singleton.PlayShieldHit_snd(transform.position);
        if (shieldStr < 1)
        {
            DeactivateShield();
        }
    }


    public void SetRadar(bool val)
    {
        radar = val;
    }

    public bool GetRadar()
    {
        return radar;
    }


    public void SetAutoStop(bool val)
    {
        autoStop = val;
    }

    public bool GetAutoStop()
    {
        return autoStop;
    }

    //increases laser charge
    public void AddLaser(float val) {
        laser += val;
        if (laser > laserMax)
        {            
            laser = laserMax;
        }
        mylaserBar.SetBar(laser, laserMax);

        //if the theoretical optional watch is attached to the device show the laser charge on the watch
        if (myWatchLaserBar != null)
        {
            myWatchLaserBar.SetBar(laser, laserMax);
            if (laser == laserMax)
            {
                myWatchLaserBar.gameObject.GetComponent<s_Emission_Flash>().Flash = true;
            }
        }
       
    }

    public ThrusterScript GetThrusters() {
        return myThruster;
    }

    void fireLaser() {
        LaserObj.SetActive(true);
        firingLaser = true;
        laser = 0;
        ShootLaserAniamation();
        if (myWatchLaserBar != null)
        {
            myWatchLaserBar.gameObject.GetComponent<s_Emission_Flash>().Flash = false;
            myWatchLaserBar.gameObject.GetComponent<s_Emission_Flash>().ZeroOut();
        }
    }
    void deactivateLaser() {
        LaserObj.SetActive(false);
        firingLaser = false;
        ShootLaserAniamation();


    }
    //count down for active power ups
    void powerUpTimers() {
        if (radar)
        {
            if (radarTime.time < 2.0f)
            {
                AudioControlerScript.Singleton.PlayWhooshing_snd(transform.position);
            }

            if (radarTime.CountDown())
            {
                radar = false;
            }


        }

        if (autoStop)
        {

            if (autoStopTime.time < 2.0f)
            {
                AudioControlerScript.Singleton.PlayWhooshing_snd(transform.position);
            }
            if (autoStopTime.CountDown())
            {
                autoStop = false;
            }
        }


        if (firingLaser)
        {

            if (laserTime.time < 2.0f)
            {
                AudioControlerScript.Singleton.PlayWhooshing_snd(transform.position);
            }
            if (laserTime.CountDown())
            {
                deactivateLaser();
            }
        }
    }
    //turns off colliders for detectable pieces
    public void DeActivateCollHotRods(DetachableObject oneHit)
    {
        for (int i = 0; i < myHotRods.Length; i++)
        {
            if (oneHit != myHotRods[i])
            {
                myHotRods[i].GetMyColl().enabled = false;
            }
        }
    }
    //turns on colliders for detectable pieces
    public void ReActivateCollHotRods(DetachableObject oneHit)
    {
        for (int i = 0; i < myHotRods.Length; i++)
        {
            if (oneHit != myHotRods[i])
            {
                myHotRods[i].GetMyColl().enabled = true;
            }
        }
    }
    public bool IsShieldOn()
    {
        return shield;
    }
    void die() {
        AudioControlerScript.Singleton.PlayExplosion_snd(transform.position);
        currentState = states.dead;
        DieAniamation();
        Board.SetActive(false);
        s_GameManager.Singleton.GetBoardExplosion(transform.position);
        transform.position = Vector3.zero;
        s_GameManager.Singleton.LoseGame();
    }
  

    public void DieAniamation() {
        MyAnimator.SetTrigger("dieing");

    }
    public void ShootLaserAniamation()
    {
        MyAnimator.SetBool("lasering", !MyAnimator.GetBool("lasering"));
    }
    public void HitAniamation()
    {
        MyAnimator.SetTrigger("beinghit");       
    }
    public Collider[] GetColliders() {
        return myColliders;
    }
}
