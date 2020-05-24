using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class s_WeaponRanged : s_Weapon
{

    public bool ShowDebug = false;
    [Tooltip("Is projectile deflectable?  Used in conjuntion with a melee weapon's deflection settings.")]
    public bool Deflectable = true;
    [Tooltip("Does the projectile lock onto the first target along its trajectory?")]
    public bool TrackingShot = false;
    public enum EffectType {
        Damage,
        KnockBack,
        Slow,
        Immobilize
    }

    [System.Serializable]
    class ConstantJitters
    {
        public float[] Jitters;
        int m_index = -1;
        public float GetJitter()
        {
            return Jitters[increment()];
        }
        int increment()
        {
            m_index++;
            if (m_index > Jitters.Length - 1)
                m_index = 0;
            return m_index;
        }
    }

    [SerializeField]
    ConstantJitters m_ConsistantJitters = null;

    public enum CollideType
    {
        Everything,
        Players,
        Obstacles,
        Walls,
        Hazards
    }
    public class RadiusEffectClass
    {
        public RadiusEffectClass(float d, List<EffectType> a, float force, float stunLength, float knockBackJitter, float slowLength, float slowPercent)
        {
            EffectDistance = d;
            Effects = a;
            Force = force;
            StunLength = stunLength;
            KnockBackJitter = knockBackJitter;
            SlowLength = slowLength;
            SlowPercent = slowPercent;
        }
        public void RemoveExcess()
        {

            if (Effects.Count > 1 && !ran)
            {
                Debug.Log("Remove called");
                ran = true;
                List<EffectType> tempList = new List<EffectType>();
                for (int i = 0; i < Effects.Count; i++)
                {

                    EffectType toCheck = Effects[i];
                    if (!tempList.Contains(toCheck))
                    {
                        Debug.Log("Adding " + toCheck);
                        tempList.Add(toCheck);
                    }

                }

                Effects.Clear();
                foreach (EffectType e in tempList)
                {
                    Effects.Add(e);
                }


            }
        }
        public List<s_Player> nonTeamPlayers = new List<s_Player>();
        public float EffectDistance = 0;
        public List<EffectType> Effects = new List<EffectType>();
        public float RadiansKnockBack = 0;
        public float Force = 0;
        bool ran = false;
        public float StunLength, KnockBakcForce, KnockBackJitter, SlowLength, SlowPercent;
    }

    public class BounceStruct
    {

        public bool B = false;
        public int Minbt = 1;
        public int Maxbt = 1;
        public float Minbd = 1.0f;
        public float Maxbd = 1.0f;
        public BounceStruct(bool b, int mibt, int mabt, float mibd, float mabd)
        {
            B = b;
            Minbt = mibt;
            Maxbt = mabt;
            Minbd = mibd;
            Maxbd = mabd;
        }
    }

    public BounceStruct CreateBounce()
    {
        return new BounceStruct(Bounce, MinimumBounceTimes, MaximumBounceTimes, MinimumBounceDistance, MaximumBounceDistance);
    }
    public RadiusEffectClass CreateDetonate()
    {
        RadiusEffectClass d = new RadiusEffectClass(DetonateDistance, DetonateEffect, DetonateKnockBackForce, DetonateLengthOfStun, DetonateKnockBackJitter, DetonateLengthOfSlow, DetonateSlowPercentage);
        if (d.Effects.Count < 1)
        {
            foreach (EffectType e in TypeOfEffect)
            {
                d.Effects.Add(e);
            }
        }
        return d;

    }

    public RadiusEffectClass CreateAura()
    {
        RadiusEffectClass a = new RadiusEffectClass(AuraRadius, AuraEffect, AuraKnockBackForce, AuraLengthOfStun, AuraKnockBackJitter, AuraLengthOfSlow, AuraSlowPercentage);
        if (a.Effects.Count < 1)
        {
            foreach (EffectType e in TypeOfEffect)
            {
                a.Effects.Add(e);
            }
        }
        return a;

    }

    public RadiusEffectClass CreateMine()
    {
        RadiusEffectClass m = new RadiusEffectClass(MineDetonateRange, MineEffect, MineKnockBackForce, MineLengthOfStun, MineKnockBackJitter, MineLengthOfSlow, MineSlowPercentage);
        if (m.Effects.Count < 1)
        {
            foreach (EffectType e in TypeOfEffect)
            {
                m.Effects.Add(e);
            }
            
        }
        return m;

    }
    [Tooltip("The amount of damage a weapon inflicts (currently players have one hitpoint).")]
    public int damage = 1;
    public s_BulletSpawner.bulletType projectileType;
    [HideInInspector]
    public float LengthOfStun = 1.0f;
    [HideInInspector]
    public float KnockBackForce = 0;
    [HideInInspector]
    public float KnockBackJitter = 0;
    [HideInInspector]
    public float LengthOfSlow = 0;
    [HideInInspector]
    public float SlowPercentage = 0;
    public bool Bounce = false;
    [HideInInspector]
    public int MinimumBounceTimes = 1;
    [HideInInspector]
    public int MaximumBounceTimes = 1;
    [HideInInspector]
    public float MinimumBounceDistance = 1.0f;
    [HideInInspector]
    public float MaximumBounceDistance = 1.0f;

    [HideInInspector]
    public float DetonateDistance = 0f;
    [HideInInspector]
    public float AuraRadius = 0f;
    [HideInInspector]
    public float AuraLengthOfStun = 1.0f;
    [HideInInspector]
    public float AuraKnockBackForce = 0;
    [HideInInspector]
    public float AuraKnockBackJitter = 0;
    [HideInInspector]
    public float AuraLengthOfSlow = 0;
    [HideInInspector]
    public float AuraSlowPercentage = 0;
    [HideInInspector]
    public float DetonateRadius = 0f;
    [HideInInspector]
    public float DetonateLengthOfStun = 1.0f;
    [HideInInspector]
    public float DetonateKnockBackForce = 0;
    [HideInInspector]
    public float DetonateKnockBackJitter = 0;
    [HideInInspector]
    public float DetonateLengthOfSlow = 0;
    [HideInInspector]
    public float DetonateSlowPercentage = 0;
    [HideInInspector]
    public float MineLengthOfStun = 1.0f;
    [HideInInspector]
    public float MineKnockBackForce = 0;
    [HideInInspector]
    public float MineKnockBackJitter = 0;
    [HideInInspector]
    public float MineLengthOfSlow = 0;
    [HideInInspector]
    public float MineSlowPercentage = 0;
    [HideInInspector]
    public bool MineSticksToFloor = true;
    [HideInInspector]
    public float MineDetonateRange = 0.5f;
    [HideInInspector]
    public bool DestroyedByProjectiles = false;

    [Tooltip("On impact with obstacle mine will stop and stick to location.  Effects unique to projectile type. Mines will always collide with obstacles.")]
    public bool IsMine = false;
    [Tooltip("Whenever an enemy gets within a certain distance of this projectile it will detonate")]
    public bool DetonateOnProximity = false;
    [Tooltip("An effect will occur to players or items in radius of this weapon's projectiles.  (Unique effects coded on a per projectile basis)")]
    public bool Aura = false;





    [Tooltip("The type of effect when a projectile hits an enemy.")]
    public EffectType[] TypeOfEffect;
    [Tooltip("How frequently a projectile will be created (in seconds) when holding down the fire button.  Doubles as charge time.")]
    public float fireRate = 0.1f;
    [Tooltip("Does weapon support burst fire.")]
    public bool BurstFire = false;
    [Tooltip("Cool down between bursts of fire.")]
    public float burstFireRate = 3.0f;
    [Tooltip("Time between individual shots")]
    public float TimeBetweenBurstShots = 0.25f;
    [VectorLabels("Min shots per burst", "   Max shots per burst")]
    public Vector2 BurstFireRange = new Vector2();
    [Tooltip("Will not holding down the fire key reset the number of shots for your next burst.")]
    public bool ResetBurstCountOnFireUp = true;
    Timer burstFireDelayBetweenSoundCallsSec = new Timer();
    
    [Tooltip("How many targets the projectile can penetrate.  -1 represents and infinite number.")]
    public int Penetrates = 0;

    [Tooltip("Distance from point of impact that the projectile's effects will take place.  In Unity Units.")]
    public float AreaOfEffect = 0;
    [Tooltip("If the area of effect is greater than 0 how much force does it impart at its point of impact.  Force falls off with distance.")]
    public float AreaOfEffectForce = 0;
    [Tooltip("Unity units per second projectile moves.  In Unity Units.")]
    public float ProjectileMoveRate = 15;
    [Tooltip("Spread of projectiles fire.  In Radians (2pi radians = 360 degrees).")]
    public float FireSpread = 0;
    [Tooltip("Strength of recoil from weapon fire.  A value of 0 has no recoil.")]
    public float Recoil = 0;
    

    //[Tooltip("Barring a valid collision how long will a projectile last (in seconds).")]
    [VectorLabels("Min", "   Max")]
    public Vector2 LifeTime = new Vector2(5.0f, 5.0f);

    //[Tooltip("Barring a valid collision longest a projectile will last (in seconds).")]
    protected float lifeTimeOfProjectileMax = 5.0f;
    //[Tooltip("Barring a valid collision shortest a projectile will last (in seconds).")]
    protected float lifeTimeOfProjectileMin = 5.0f;
    protected bool burstCanFire = true;
    int burstFireCount = 0;
    int burstFireCurrentMax = 0;
    
    protected Timer burstFireTimer = new Timer(3.0f); //the timer counting down until the weapon's burst fires again     
    protected Timer betweenShotsTimer = new Timer(0.25f); //the time between indivudal shots of a single burst

    [Tooltip("What the Projectile will collide")]
    public CollideType[] WillCollideWith = { CollideType.Everything };

    public List<EffectType> MineEffect = new List<EffectType>();
    public List<EffectType> DetonateEffect = new List<EffectType>();
    public List<EffectType> AuraEffect = new List<EffectType>();

    [SerializeField]
    bool m_enableMuzzleFlash = true;
    //END PUBLIC VARIABLES

    public s_FirePoint m_pointForBullets = null;
    bool started = false;
    
    Vector3 currentDirection = new Vector3(1, 0, 0);

    public void RemoveExcess(ref List<EffectType> Effects)
    {

        if (Effects.Count > 1)
        {
            List<EffectType> tempList = new List<EffectType>();
            for (int i = 0; i < Effects.Count; i++)
            {

                EffectType toCheck = Effects[i];
                if (!tempList.Contains(toCheck))
                {
                    tempList.Add(toCheck);
                }

            }

            Effects.Clear();
            foreach (EffectType e in tempList)
            {
                Effects.Add(e);
            }


        }
    }



    // Start is called before the first frame update
    void Start()
    {
        myWeaponRange = WeaponDistance.Ranged;
        
        OnSpawn();
    }

    

    //things that occur for weapons that are not variable, they must be done before the spawn proper and should only occur once per weapon
    protected void onStart() {
        if (started)
        {
            return;
        }
        m_myOutline = GetComponent<Outline>();
        m_pointForBullets = GetComponentInChildren<s_FirePoint>();
        started = true;
        RemoveExcess(ref AuraEffect);
        RemoveExcess(ref DetonateEffect);
        lifeTimeOfProjectileMin = (LifeTime.x > 0 ? LifeTime.x : 5);
        lifeTimeOfProjectileMax = (LifeTime.y > 0 ? LifeTime.y : 5);
        if (LifeTime.x <= 0)
        {
            Debug.LogError("Lifetime min for " + gameObject.name + " was zero.  Set to default");
        }
        if (LifeTime.y <= 0)
        {
            Debug.LogError("Lifetime max for " + gameObject.name + " was zero.  Set to default");
        }
        setBurstFire();
    }

    //called when the item is spawned, sets values and may rely on values being set in onStart()
    public virtual void OnSpawn()
    {
        AMMOMAX.Value = AmmoAmount;
        onStart();
        canFireTimer.SetTimer(fireRate);
        burstFireTimer.SetTimer(burstFireRate);
        burstFireDelayBetweenSoundCallsSec.SetTimer(burstFireRate);
        canFire = true;
        burstCanFire = true;
       
    }

    void setBurstFire()
    {
        burstFireCount = 0;
        burstFireTimer.SetTimer(burstFireRate);
        burstFireCurrentMax = Random.Range((int)BurstFireRange.x, (int)BurstFireRange.y + 1);
    }

    // Update is called once per frame
    void Update()
    {
        
        onUpdate();
    }
    protected override void onUpdate() {
        
        base.onUpdate();
        if (s_GameManager.GetPaused())
        {
            return;
        }
        if ( IsMine && Input.GetKeyUp(KeyCode.K))
        {
            Debug.Log(MineDetonateRange);
        }
        checkCanFire();
        stayToHolder();
        checkBurstFire();


    }

    void checkBurstFire() {
        if (ResetBurstCountOnFireUp)
        {
            if (burstFireCount != 0)
            {
                if (betweenShotsTimer.CountDown())
                {
                    burstFireCount = 0;
                    burstCanFire = true;
                }
            }
        }

        betweenShotsTimer.CountDownAutoCheckBool();
        burstFireDelayBetweenSoundCallsSec.CountDownAutoCheckBool();
        if (!burstCanFire)
        {
            if (burstFireTimer.CountDown())
            {
                burstCanFire = true;
            }
        }


    }


    //public function, if can fire is true calls the weapon fire function which may have a variety of effects
    public override void FireWeapon()
    {

        if (BurstFire)
        {            
            if (burstCanFire)
            {
                if (!burstFireDelayBetweenSoundCallsSec.ShouldCountDown())
                {
                    burstFireDelayBetweenSoundCallsSec.SetTimerShouldCountDown(true);
                    playFireSoundEffect();
                }
                    
                if (!betweenShotsTimer.ShouldCountDown() && checkAmmoCount(true))
                {
                    
                    betweenShotsTimer.SetTimer(TimeBetweenBurstShots);
                    betweenShotsTimer.SetTimerShouldCountDown(true);
                    burstFireCount++;
                    fireWeapon();
                    if (TimeBetweenBurstShots == 0)
                    {
                        while (burstFireCount < burstFireCurrentMax)
                        {
                            burstFireCount++;
                            fireWeapon();
                        }
                    }
                }

                if (burstFireCount >= burstFireCurrentMax)
                {
                    burstFireEndEffect();
                    burstCanFire = false;
                    setBurstFire();
                    
                }
            }
        }
        else
        {
            if (canFire && checkAmmoCount(true))
            {
                canFire = false;
                playFireSoundEffect();
                fireWeapon();
            }
        }
    }
    public override void FireWeaponSecondary()
    {

    }

    protected float speedModifer() {

        if (holder.GetMovementScript() != null)
        {
            Vector3 l = GetFireDirectionAndRecoil();
            Vector3 r = holder.GetMovementScript().GetDirection();
            float dot = Vector3.Dot(l, r);
            float speedModifer = 0;
            if (s_Calculator.AreNear(dot, 1, 0.1f))
            {
                speedModifer = holder.GetMovementScript().GetMoveSpeed();
            }
            //Debug.Log(speedModifer + " is current speed due to dot of " + dot + " from vectors l: " + l + " r: " + r);
            return speedModifer;
        }
        return 1;
    }
    //the acutal fire function, this is customized to the spesific weapon and is what produces the actual projectile
    protected override void fireWeapon() {
        if (m_enableMuzzleFlash)
        {
            switch (HowToHoldWeapon)
            {
                case WeaponHoldingType.OneHandedMelee:
                    break;
                case WeaponHoldingType.TwoHandedMelee:
                    break;
                case WeaponHoldingType.OneHandedRanged:
                    s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(WarboticsEnums.VFX.MuzzleFlashOneHanded).OnSpawn(m_pointForBullets.transform.position);
                    break;
                case WeaponHoldingType.TwoHandedRanged:
                    s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(WarboticsEnums.VFX.MuzzleFlashTwoHanded).OnSpawn(m_pointForBullets.transform.position);
                    break;
                case WeaponHoldingType.OneHandedThrown:
                    break;
                default:
                    break;
            }
        }
        s_Projectile p = GetProjectileForGun();   
        p.ShowDebug = ShowDebug;
        p.OnSpawn(holder,
            GetFireDirectionAndRecoil(),
            ProjectileMoveRate + speedModifer(),
            damage,
            new float[] { lifeTimeOfProjectileMin, lifeTimeOfProjectileMax },
            WillCollideWith,
            TrackingShot,
            AreaOfEffect,
            AreaOfEffectForce,
            TypeOfEffect,
            KnockBackForce,
            KnockBackJitter,
            LengthOfStun,
            LengthOfSlow,
            SlowPercentage,
            Deflectable,
            CreateBounce(),
            (DetonateOnProximity ? CreateDetonate() : null),
            (Aura ? CreateAura() : null),
            Penetrates,
            IsMine,
            DestroyedByProjectiles,
            MineSticksToFloor,
            (IsMine ? CreateMine() : null),
            (m_pointForBullets == null ? Vector3.zero : m_pointForBullets.transform.position)
            );
        
    }

    protected virtual Vector3 GetFireDirectionAndRecoil() {
        Vector3 dir = spread(holder.GetAimDirection(), FireSpread);
        if (!holder.GetAI())
        {
            holder.RecoilEffect(-dir, Recoil * (holder.moveSpeed / 5));
        }
        return dir;
    }



    public float GetProjectileSpeed()
    {
        return ProjectileMoveRate + speedModifer();
    }
  

    protected virtual s_Projectile GetProjectileForGun()
    {
        return s_GameManager.Singleton.GetBulletSpawner().GetProjectileOfType(projectileType);
    }
    protected virtual void burstFireEndEffect()
    {

    }

    //the spread in radians should be a value from 0 (no spread) to 1 (90 degrees to left or right)
    protected Vector3 spread(Vector3 direction, float spreadInRadians)
    {
        
        float spreadAmount = m_ConsistantJitters.Jitters.Length > 0 ? m_ConsistantJitters.GetJitter() : Random.Range(-spreadInRadians, spreadInRadians);
        float x = (direction.x * Mathf.Cos(spreadAmount)) + (direction.z * Mathf.Sin(spreadAmount));
        float y = direction.y;
        float z = (-direction.x * Mathf.Sin(spreadAmount)) + (direction.z * Mathf.Cos(spreadAmount));
        return new Vector3(x, y, z);
    }


    public override bool GetWeaponIsAbleToFire()
    {
        if (BurstFire)
        {
            return canFire && burstCanFire;
        }
        return canFire;
    }

    public override float GetFireRate()
    {
        if (BurstFire)
            return burstFireTimer.timeReset;
        return canFireTimer.timeReset;
    }
    public float GetFireRateForIndividualBurstFireShots()
    {
        return TimeBetweenBurstShots;
    }
    public override float GetReloadTimeAsPercent()
    {
        if (BurstFire)
        {
            return burstFireTimer.time / burstFireTimer.timeReset;
        }
        else
        {
            return base.GetReloadTimeAsPercent();
        }
    }
}
