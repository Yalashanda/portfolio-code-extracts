using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//base clas for melee and ranged weapon classes
public class s_Weapon : MonoBehaviour
{
    int tier = 0;// weapon tier used for so the AI can determine the best weapons
    TossStruct tossed = null; //the struct defining the current state of the arc of a tossed weapon
    protected Outline m_myOutline = null; // class used for outline effect on weapon
    [SerializeField]
    Sprite WeaponSillouette = null; //the sprite for the weapon to render in the hud
    public class TossStruct {
        enum State {
            towardZenith,
            towardEnd
        }
        State currentState = State.towardZenith;
        public Vector3 currentPos;
        public Vector3 endPoint;
        public Vector3 target;
        public float speed;
        bool m_ignoreArc = false;
        public TossStruct(Vector3 _startPos, Vector3 _target, float _speed, bool _ignorearc = false) {
            currentPos = _startPos;
            endPoint = _target;
            target = (endPoint - currentPos);
            target = _startPos + (target.normalized * target.magnitude * 0.5f) + new Vector3(0, 5, 0);            
            speed = _speed;
            currentState = State.towardZenith;
            m_ignoreArc = _ignorearc;


        }
        public Vector3 Move(out bool reachedPoint) {
            reachedPoint = false;

            if (m_ignoreArc)
            {
                reachedPoint = false;
                currentPos = Vector3.MoveTowards(currentPos, endPoint, speed * Time.deltaTime);
                if (s_Calculator.GetDistanceLessThan(currentPos, endPoint, 0.5f))
                {
                    reachedPoint = true;
                }
                return currentPos;
            }
            else
            {
                currentPos = Vector3.MoveTowards(currentPos, target, speed * Time.deltaTime);
                if (s_Calculator.GetDistanceLessThan(currentPos, target, 0.5f))
                {
                    if (currentState == State.towardZenith)
                    {
                        target = endPoint;
                        currentState = State.towardEnd;
                    }
                    else
                    {
                        reachedPoint = true;
                    }
                }
                return currentPos;
            }
            
        }
        
    }

    protected enum WeaponDistance{
        Melee,
        Ranged
    }
    protected WeaponDistance myWeaponRange;
    //So when a weapon is picked up a name is spawned above the player
    public enum WeaponName {
        NULL,
        SMG,
        FryingPan,
        Shotgun,
        Boomerang,
        Revolver,
        MegaKatana,
        Flamethrower,
        GrenadeLauncher,
        PlasmaGun,
        CrossBow,
        MissileLauncher,
        DuckGrenade,
        BlackHoleGun,
        AutoCrossbow,
        RayGun,
        Rifle,
        LightningGun,
        Shrukien,
        SpikeGun
           
    }

    //used to position the physical weapon when it is picked up
    s_WeaponMesh m_myMesh = null;
   
    //indentifier for the animation pose to use for the weapon
    public enum WeaponHoldingType{
        OneHandedMelee,
        TwoHandedMelee,
        OneHandedRanged,
        TwoHandedRanged,
        OneHandedThrown

    }
    Color holderColor = Color.white;
    [SerializeField]
    WeaponName NameOfWeapon = WeaponName.NULL;
    [SerializeField]
    WeaponHoldingType HowToHoldWeapon = WeaponHoldingType.TwoHandedRanged;    
    s_Player holder = null;
    protected Timer canFireTimer = new Timer(0.1f); //the timer counting down until the weapon fires again
    protected bool canFire = true;

    [SerializeField]
    int AmmoAmount = -999; //-999 = unlimited ammo
    protected s_WeaponSpawnInPoint mySpawnPoint;
    protected SetOnce<int> AMMOMAX = new SetOnce<int>(); // value that can only be set once, i.e. when the weapon spawns
    Timer DespawnTimer = new Timer(40.0f); //How long an unheld weapon should remain before despawning, resets after a weapon is picked up
    public void PositionMesh(int _rotationToUse)
    {
        if (m_myMesh == null)
            m_myMesh = GetComponentInChildren<s_WeaponMesh>();
        if (m_myMesh != null)
        {
            transform.localEulerAngles = Vector3.zero;
            m_myMesh.SetMeshRotation(_rotationToUse);
        }
    }

    protected void revertMesh()
    {
        if (m_myMesh == null)
            m_myMesh = GetComponentInChildren<s_WeaponMesh>();
        if (m_myMesh != null)
        {
            
            m_myMesh.RevertMeshRotation();
        }
            
    }

    protected void checkCanFire()
    {
        
        if (!canFire)
        {
            if (canFireTimer.CountDown())
            {
                canFire = true;
            }
        }
    }
    
    protected virtual void stayToHolder()
    {

        if (holder != null)
        {
            if (transform.parent == null)
            {
                Vector3 cross = (s_Calculator.cross(holder.GetForwardVector(), holder.GetUpVector()).normalized);
                rotateToHolder(cross);
                transform.position = holder.transform.position + (cross * 1.5f);
            }
            else
            {
                transform.position = transform.parent.transform.position;
            }

           
                DespawnTimer.SetTimer(DespawnTimer.timeReset);
        }
        else
        {
            if (!s_GameManager.Singleton.GetPause())
            {
                if (mySpawnPoint == null)
                {
                    if (DespawnTimer.CountDown())
                    {
                        Destroy(gameObject);
                    }
                }
            }
 
           
        }
    }


    public virtual void FireWeapon()
    {

        

    }
    public virtual void FireWeaponSecondary()
    {

    }

    protected virtual void fireWeapon() {

    }
    protected bool checkAmmoCount(bool reduceAmmo)
    {
        if (AmmoAmount > 0 || AmmoAmount == -999)
        {
            if (AmmoAmount != -999 && reduceAmmo)
            {
                AmmoAmount--;
                if (AmmoAmount < 0)
                {
                    AmmoAmount = 0;                    
                }
            }
            return true;
        }
        if (holder != null)
        {
            
            Debug.Log("Tossing weapon because it is empty");
            holder.DropWeapon(true);
        }
        return false;
    }

    protected virtual void onUpdate() {
        if (s_GameManager.GetPaused())
        {
            return;
        }

        
        if (holder != null && holder.GetEquippedWeapon() != this)
        {
            Dropped(false);
        }
        
        

        if (tossed != null)
        {
            bool hitLocation;
            transform.position = tossed.Move(out hitLocation);
            if (hitLocation)
            {
                tossed = null;
                
                RaycastHit hit;
                if (Physics.Raycast(transform.position, Vector3.down, out hit, 30, Physics.AllLayers, QueryTriggerInteraction.Ignore))
                {
                    if (hit.distance > 1.5f)
                    {
                        tossed = new TossStruct(transform.position, hit.point + new Vector3(0, 1, 0), 14, true);
                    }
                }
                
                checkIfEmpty();
            }
        }
    }
    
    public void Dropped(bool died) {
        if (died == false )
        {
            Vector3 targetPos = transform.position + (holder.GetAimDirection() * -4);
            RaycastHit hit;
            if (Physics.Raycast(targetPos, Vector3.down, out hit, Physics.AllLayers))
            {
                targetPos.y = hit.point.y + 0.1f;
            }
            tossed = new TossStruct(transform.position, targetPos, 14);
        }
        holderColor = Color.white;
        SetHolder(null);
        transform.SetParent(null);
        transform.eulerAngles = Vector3.zero;
        revertMesh();
    }
    public void SetHolder(s_Player toHold)
    {
        holder = toHold;
        if (toHold == null)
        {
            holderColor = Color.white;
            if (m_myOutline != null)
                m_myOutline.enabled = true;
        }
        else
        {
            holderColor = toHold.PlayerColor;
            if (m_myOutline != null)
                m_myOutline.enabled = false;
        }
        if (tossed == null)
        {
            checkIfEmpty();
        }
    }

    
    void checkIfEmpty() {
        if (holder == null)
        {
            if (!checkAmmoCount(false))
            {
                weaponFadesOnEmpty();
            }
        }
    }
    void weaponFadesOnEmpty() {
        if (s_GameManager.ShowCreateDestroy())
            Debug.Log("Game object " + gameObject.name + " with id of " + gameObject.GetInstanceID() + " is being destroyed");
        Destroy(gameObject);
    }

    public void SetHoldingSpawnPoint(s_WeaponSpawnInPoint val)
    {
        
        mySpawnPoint = val;
    }

    protected virtual void OnEnter(Collider c)
    {
        s_Player p = c.gameObject.GetComponent<s_Player>();
        if (p != null)
        {
            p.SetOverlapped(this);
        }
        if (mySpawnPoint != null && p != null && p.GetEquippedWeapon() == null)
        {
            mySpawnPoint.SetCurrentWeapon(null);
            mySpawnPoint = null;
            transform.SetParent(null);
                
        }
        if (p != null && p.GetEquippedWeapon() == null && holder == null && !p.GetAI() && p.GetCurrentState() != s_Player.States.Dead)
        {
            onPickUp(p);
        }
        
    }


    public void onPickUp(s_Player p) {
        if (p.SetEquippedWeapon(this)) // checks to make the weapon is not one just dropped if it is not then it equips it and continues
        {
            AudioControlerScript.PlaySound(AudioControlerScript.Clips.pickup_gun_001);
            SetHolder(p);
            p.AddWeaponToHand(this);
            holderColor = p.PlayerColor;
            tossed = null;
            p.PlayPickupSound();
            doPickUpEffect();
        }
        
    }
    private void OnCollisionEnter(Collision collision)
    {

        OnEnter(collision.collider);
    }

    private void OnTriggerEnter(Collider collision)
    {

        OnEnter(collision);
    }

    private void OnTriggerExit(Collider other)
    {
        onExit(other);
    }

    private void OnCollisionExit(Collision collision)
    {
        onExit(collision.collider);
    }

    void onExit(Collider c) {
        s_Player p = c.gameObject.GetComponent<s_Player>();
        if (p != null)
        {
            p.SetOverlapped(this);
        }
    }
    public void PickUp(s_Player p) {
        if (p != null && p.GetEquippedWeapon() == null && holder == null)
        {
            onPickUp(p);
        }
    }

    protected void doPickUpEffect() {
        string input = "";
        switch (NameOfWeapon)
        {
            case WeaponName.NULL:
                break;
            case WeaponName.SMG:
                input = "SMG";
                break;
            case WeaponName.Revolver:
                input = "Revolver";
                break;
            case WeaponName.MegaKatana:
                input = "Mega Sword";
                break;
            case WeaponName.Flamethrower:
                input = "Flamethrower";
                break;
            case WeaponName.GrenadeLauncher:
                input = "Grenade Launcher";
                break;
            case WeaponName.FryingPan:
                input = "Cast Iron Doom";
                break;
            case WeaponName.Shotgun:
                input = "Shotgun";
                break;
            case WeaponName.Boomerang:
                input = "AI Boomerang";
                break;
            case WeaponName.PlasmaGun:
                input = "XN-PC Plasma Gun";
                break;
            case WeaponName.CrossBow:
                input = "Crossbow";
                break;
            case WeaponName.MissileLauncher:
                input = "Missile Launcher";
                break;
            case WeaponName.DuckGrenade:
                input = "Duck Grenade";
                break;
            case WeaponName.BlackHoleGun:
                input = "Hawking Cannon";
                break;
            case WeaponName.AutoCrossbow:
                input = "Heh heh he HAHAHHAHAHAHAHAH!";
                break;
            case WeaponName.RayGun:
                input = "Retro death!";
                break;
            case WeaponName.Rifle:
                input = "Bolt Action Rifle";
                break;
            case WeaponName.LightningGun:
                input = "Quake in fear!";
                break;
            case WeaponName.Shrukien:
                input = "AI Shuriken.";
                break;
            case WeaponName.SpikeGun:
                input = "Spike Gun";
                break;
            default:
                break;
        }
        s_GameManager.Singleton.GetFloatingText().OnSpawn(transform.position, input, holder.PlayerColor);
        s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(WarboticsEnums.VFX.PickUpEffect).OnSpawn(transform.position); //returns a class that is a child of s_VFXEffect and sets its position
    }

    protected void playFireSoundEffect() {
        switch (NameOfWeapon)
        {
            case WeaponName.NULL:
                break;
            case WeaponName.SMG:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.machine_gun_shot_002);
                break;
            case WeaponName.FryingPan:
                break;
            case WeaponName.Shotgun:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.shotgun_blast_002);
                break;
            case WeaponName.Boomerang:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.boomerang_001);
                break;
            case WeaponName.Revolver:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.pistol_shot_002);
                break;
            case WeaponName.MegaKatana:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.katana_slash_002);
                break;
            case WeaponName.Flamethrower:
                break;
            case WeaponName.GrenadeLauncher:
                break;
            case WeaponName.PlasmaGun:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.plasma_cannon_Alexander);
                break;
            case WeaponName.CrossBow:
            case WeaponName.AutoCrossbow:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.crossbow_shot_001);
                break;
            case WeaponName.MissileLauncher:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.missile_001);
                break;
            case WeaponName.DuckGrenade:
                break;
            case WeaponName.RayGun:
                break;
            case WeaponName.BlackHoleGun:
                break;
            case WeaponName.Rifle:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.sniper_rifle_Alexander);
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.pickup_gun_001);
                break;
            case WeaponName.LightningGun:
                break;
            case WeaponName.Shrukien:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.shruiken_Alexander);
                break;
            case WeaponName.SpikeGun:
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.spike_gun_Alexander);
                break;
            default:
                Debug.Log("No sound effect set for " + gameObject.name + " attack sound");
                break;
        }

    }

    public void SetTier(int val) {
        tier = val;
    }
    public int GetTier()
    {
        return tier;
    }
    public virtual float GetFireRate()
    {
        return canFireTimer.timeReset;
    }
    public int GetWeaponRange() {
        return (int)myWeaponRange;
    }
    public bool IsWeaponRanged()
    {
        return myWeaponRange == WeaponDistance.Ranged;
    }
    public bool GetCanFire()
    {
        return canFire;
    }
    public virtual bool GetWeaponIsAbleToFire() {
        return canFire;
    }
    public float GetAmmoAsPercent() {
        return (AmmoAmount / (float)AMMOMAX.Value);
    }

    public Sprite GetWeaponSillouette() {
        return WeaponSillouette;
    }

    public virtual float GetReloadTimeAsPercent()
    {
        return canFireTimer.time / canFireTimer.timeReset;
    }
}
