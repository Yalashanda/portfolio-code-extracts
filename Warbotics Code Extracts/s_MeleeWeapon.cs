using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_MeleeWeapon : s_Weapon
{
    public enum DeflectionType{
        NoDeflection, 
        ExactReflection,
        DefaultDeflection,
        OverrideProjectileDeflection,
        DeflectForward
    }


    [Tooltip("How far the weapon reaches out from the player")]
    public float Range = 3.0f;
    [Tooltip("Amount of damage the weapon will inflict on a hit")]
    public int Damage = 1;
    [Tooltip("-999 is infinite durability.  Durability is reduced when a melee weapon hits")]
    public int Durability = -999;
    [Tooltip("Arc infront of player the weapon will hit.")]
    public int SwingArc = 180;
    [Tooltip("How much knockback is applied to a player hit with this weapon. 0 is no knockback.")]
    public float SwingForce = 0.0f;
    [Tooltip("How frequently the weapon is attacked with (i.e. fire rate)")]
    public float AttackRate = 1.0f;
    [Tooltip("Determine type of deflection\n\nNoDeflection: no effect\n\nExactReflection: The Projectile will move in reverse following its own trajectory back\n\nDefaultDeflection:  The projectile will use its own jitter value, unless said value  equals -999 then it will use the melee weapon's jitter value\n\nOverrideProjectileDeflection: The projectile will use the melee weapon's jitter value\n\nDeflectForward: The projectile will reflect and move in the direction the player is facing")]
    public DeflectionType TypeOfDeflection = DeflectionType.NoDeflection;
    [Tooltip("Projectiles have penetration \"hp\" each time they go through an obstacle they lose one.  projectiles with a penetration value higher than this will go through the melee weapon and cannot be deflected.  Suggested minimum of 3")]
    public int CanDeflectPentrationsUpTo = 3;
    [Tooltip("How long the Deflection lasts")]
    public float DeflectionLength = 1.0f;
    [Tooltip("How large an arc infront of the player the weapon will deflect projectiles (reccomend values of 180-360).  If zero it uses the SwingArc")]
    public float DeflectionArc = 0;
    [Range(0, 1)]
    public float slowPercent = 0;
    

    bool canDeflect = true;
    Timer canDeflectTimer = new Timer(1.0f);
    //[Tooltip("Amount of jitter to add to deflected projectile (in radians)")]
    [Range(0, 2)]
    public float  DeflectionJitter = 0.25f;

     

    ArcClass deflectionArc = new ArcClass(180);
    ArcClass swingArc = new ArcClass(180);
    Timer swingTimer = new Timer(1.0f);
    bool canSwing = true;

    // Start is called before the first frame update
    void Start()
    {
        //resetswingArc();
        myWeaponRange = WeaponDistance.Melee;
        m_myOutline = GetComponent<Outline>();
        resetArcs();
        swingTimer.SetTimer(AttackRate);
        canDeflectTimer.SetTimer(DeflectionLength);
    }

    private void Update()
    {
        onUpdate();
        if (s_GameManager.Singleton.GetPause())
        {
            return;
        }


        timers();
        stayToHolder();
    }

    void timers() {

        if (!canSwing)
        {
            if (swingTimer.CountDownAutoCheckBool())
            {
                canSwing = true;
            }
        }
        if (!canDeflect)
        {
            if (canDeflectTimer.CountDownAutoCheckBool())
            {
                canDeflect = true;
                canDeflectTimer.SetTimer(DeflectionLength);
            }
        }
    }
    void resetArcs() {
        swingArc.ResetArc(SwingArc);
        deflectionArc.ResetArc(DeflectionArc);
        
    }
    public override void FireWeapon()
    {
        if (canSwing && checkAmmoCount(true) && checkDurabilityCount())
        {
            canSwing = false;
            swing();
        }
    }
    public override void FireWeaponSecondary()
    {
        deflect(false);
    }
    protected bool checkDurabilityCount()
    {
        if (Durability > 0 || Durability == -999)
        {
            
            return true;
        }
        return false;
    }

    void reduceDurability() {
        if (Durability != -999)
        {
            Durability--;
            if (Durability < 0)
            {
                Durability = 0;
                breakWeapon();
            }
        }
    }
    void breakWeapon() {
        holder.DropWeapon();
        die();
    }

    void deflect(bool orVal = false) {
        //an orVal of true will result in the player always deflecting
        if (canDeflect || orVal)
        {
            if (slowPercent > 0)
            {
                if (orVal)
                {
                    holder.TakeSlowEffect(0.1f, slowPercent);
                }
                else
                {
                    holder.TakeSlowEffect(canDeflectTimer.timeReset, slowPercent);
                }
            }
            
            canDeflect = false;
            holder.ActivateStatusEffect(s_Player.StatusEffects.Deflecting, DeflectionLength);
            canDeflectTimer.SetTimerShouldCountDown(true);
        }
        
    }

    void die() {        
        Destroy(gameObject);
    }
    protected void onImpact() {
        switch (NameOfWeapon)
        {
            case WeaponName.NULL:
                break;
            case WeaponName.SMG:
                break;
            case WeaponName.FryingPan:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.pan_hit_002);
                break;
            case WeaponName.Shotgun:
                break;
            case WeaponName.Boomerang:
                break;
            case WeaponName.Revolver:
                break;
            case WeaponName.MegaKatana:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.katana_slash_002);
                break;
            case WeaponName.Flamethrower:
                break;
            case WeaponName.GrenadeLauncher:
                break;
            case WeaponName.PlasmaGun:
                break;
            case WeaponName.CrossBow:
                break;
            case WeaponName.MissileLauncher:
                break;
            case WeaponName.DuckGrenade:
                break;
            default:
                Debug.LogError("No case for impact sound effect for weapon " + NameOfWeapon.ToString() + " on gameobject " + gameObject.name);
                break;
        }
    }
    public void PlayDeflectionSound() 
    { 
    switch (NameOfWeapon)
        {
            case WeaponName.NULL:
                break;
            case WeaponName.SMG:
                break;
            case WeaponName.FryingPan:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.pan_hit_002);
                break;
            case WeaponName.Shotgun:
                break;
            case WeaponName.Boomerang:
                break;
            case WeaponName.Revolver:
                break;
            case WeaponName.MegaKatana:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.katana_slash_002);
                break;
            case WeaponName.Flamethrower:
                break;
            case WeaponName.GrenadeLauncher:
                break;
            case WeaponName.PlasmaGun:
                break;
            case WeaponName.CrossBow:
                break;
            case WeaponName.MissileLauncher:
                break;
            case WeaponName.DuckGrenade:
                break;
            default:
                Debug.LogError("No case for deflection sound effect for weapon " + NameOfWeapon.ToString() + " on gameobject " + gameObject.name);
                break;
        }
    }
    void swing()
    {
        deflect();
        swingTimer.SetTimerShouldCountDown(true);
        Vector3 forward = holder.GetAimDirection();
        List<s_Player> swingables = s_GameManager.Singleton.GetPlayersOnTeam(holder.GetTeam(), true);
        resetArcs();
        for (int i = 0; i < swingables.Count; i++)
        {
            if (s_Calculator.GetDistanceLessThan(holder.transform.position, swingables[i].transform.position, Range))
            {
                Vector3 dirToTarget = holder.transform.position- swingables[i].transform.position;
                float dotResult = Vector3.Dot(dirToTarget.normalized, forward.normalized);
                s_Player swingy = swingables[i];
                if (dotResult < 0  && swingy.GetCurrentState() != s_Player.States.Dead)
                {
                    if (dotResult * -1 > swingArc.arc)
                    {
                        swingy.TakeKnockBack(-dirToTarget.normalized, SwingForce, 0, true);
                        onImpact();
                        swingy.TakeDamage(Damage);
                        reduceDurability();
                    }
                }
            }
        }

    }


    Vector3 deflectionDirection(Vector3 direction, float DeflectionSpread) {

            if (DeflectionSpread == -999)
            {
                DeflectionSpread = DeflectionJitter;
            }
        
            float spreadAmount = Random.Range(-DeflectionJitter, DeflectionJitter);
            float x = (direction.x * Mathf.Cos(spreadAmount)) + (direction.z * Mathf.Sin(spreadAmount));
            float y = direction.y;
            float z = (-direction.x * Mathf.Sin(spreadAmount)) + (direction.z * Mathf.Cos(spreadAmount));            
            return new Vector3(x, y, z);

        

    }

    public Vector3 Deflect(s_Projectile p) {
        if (p.GetPenetrationsLeft() < CanDeflectPentrationsUpTo)
        {
            switch (TypeOfDeflection)
            {
                case DeflectionType.NoDeflection:
                    return p.GetMoveDirection();
                case DeflectionType.DefaultDeflection:
                    return deflectionDirection(-p.GetMoveDirection(), p.GetDeflectionSpread());
                case DeflectionType.OverrideProjectileDeflection:
                    return deflectionDirection(-p.GetMoveDirection(), DeflectionJitter);
                case DeflectionType.DeflectForward:
                    return holder.GetAimDirection();
                case DeflectionType.ExactReflection:
                    return -p.GetMoveDirection();
                default:
                    return p.GetMoveDirection();
            }
        }
        return p.GetMoveDirection();
    }
    public float GetDeflectionArc() {
        resetArcs();
        return (DeflectionArc == 0 ? swingArc.arc : deflectionArc.arc) ;
    }


}
