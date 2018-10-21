using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyScript : MonoBehaviour {
    public int health = 50;
    protected int healthMax = 5;
    protected float attackRange = 2.0f;
    protected bool stunned = false;
    protected bool canBeStunned = true;
    protected bool inRange = false;
    protected bool onFire = false;
    protected bool poisoned = false;
    protected int poisonDamagePS = 1;
    protected int fireDamagePS = 2;
    protected float fireTime = 1.0f;
    protected float poisonedTime = 1.0f;
    protected int lootDropChance = 25;
    protected int lootDropMin = 5;
    protected int lootDropMax = 10;
    protected int damgeToInflict;
    float stunnedTime = 1.0f;
    StunVFXScript stunVFX;
    OnFireScript fireVFX;
    bool started = false;
    bool headingLeft = true;
    protected Animator myAnimator;
    protected bool ranged = false;
    protected bool dieing = false;
    float perSecondTime = 1.0f;
    bool active = false;
    

    protected timer myAttackCD = new timer();
    protected timer myWalkCD = new timer();
    protected float dieingTime = 1.0f;
    protected float dieingTimeReset = 1.0f;
    protected float attackTime = 1.0f;
    protected float attackTimeReset = 1.0f;
    protected float moveSpeed = 0.5f;
    Rigidbody2D myBody;

    float hurlTime = 1.0f;
    float hurlTimeR = 1.0f;

    // Use this for initialization
    void Start() {


    }

    protected void onStart() {
        if (!started)
        {
            myAnimator = GetComponent<Animator>();
            GameManagerScript.G.AddEnemy(this);
            UpdateAnimClipTimes();
            attackTime = attackTimeReset;
            started = false;
            myBody = GetComponent<Rigidbody2D>();
        }

    }

    // Update is called once per frame
    void Update() {
        onUpdate();

    }


    public int GetDamgeToInflict() {
        return damgeToInflict;
    }
    protected void onUpdate() {

        onStart();
        reduceStun();
        hurlTimer();
        incrmentalDamage();
        toxic();
        burn();
        dieCodeInUpdate();


    }

    protected virtual void dieCodeInUpdate() {
        if (dieing)
        {
            if (dieingTime <= 0)
            {
                if (myAnimator != null)
                {
                    myAnimator.SetBool("Dieing", false);
                }
                //gameObject.SetActive(false);
                dieingTime = dieingTimeReset;
                dieing = false;
                GameManagerScript.G.RemoveEnemy(this);
                Destroy(gameObject);



            }
            dieingTime -= Time.deltaTime;
        }
    }

    void hurlTimer() {
        if (hurlTime > 0)
        {
            hurlTime -= Time.deltaTime;
            if (hurlTime <= 0)
            {
                myBody.bodyType = RigidbodyType2D.Kinematic;
                myBody.velocity = Vector3.zero;
            }


        }
    }

    void incrmentalDamage()
    {
        if (perSecondTime <= 0)
        {

            if (onFire)
            {
                TakeDamage(fireDamagePS, 0, 0);
            }
            if (poisoned)
            {
                TakeDamage(poisonDamagePS, 0, 0);
            }
            perSecondTime = 1.0f;
        }

        perSecondTime -= Time.deltaTime;
    }

    public void SetAlight(float burnLength = 2.0f) {
        onFire = true;
        fireTime = burnLength;
        fireVFX = GameManagerScript.G.GetFireVFX();
        fireVFX.OnSpawn(gameObject);
    }
    public void SetPoisoned(float poiLen = 2.0f)
    {
        poisoned = true;
        poisonedTime = poiLen;
    }
    void burn() {
        if (onFire)
        {
            if (fireTime <= 0)
            {
                onFire = false;
                fireVFX.Die();
                fireVFX = null;
            }
            fireTime -= Time.deltaTime;

        }
    }
    void toxic() {

        if (poisoned)
        {
            if (poisonedTime <= 0)
            {
                onFire = false;
            }
            poisonedTime -= Time.deltaTime;

        }
    }

    public virtual void AttackEnd() {
        if (myAnimator != null)
        {
            myAnimator.SetBool("Attacking", false);
            attackTime = attackTimeReset;
        }
        else
        {

            attackTime = 2.0f;
        }
        

        if (!ranged && inRange)
        {
            if (!PlayerData.Player.GetElectrified())
            {
                PlayerData.Player.TakeDamage(damgeToInflict);
            }
            else
            {
                PlayerData.Player.TakeDamage(Mathf.FloorToInt(damgeToInflict * 0.5f));
                TakeDamage(Mathf.FloorToInt(damgeToInflict * 0.5f), 25, 3);
            }

            
        }
    }

    protected void reduceStun() {
        if (stunned)
        {
            if (stunVFX != null)
            {
                stunVFX.gameObject.transform.position = transform.position;
            }
            if (stunnedTime <= 0)
            {
                stunned = false;
                if (stunVFX != null)
                {
                    stunVFX.Die();
                    stunVFX = null;
                }
            }
            stunnedTime -= Time.deltaTime;
        }
    }

    public bool GetInRange() { return inRange; }
    public void SetInRange(bool val) { inRange = val; }

    public bool GetStunned()
    {
        return stunned;
    }
    public float GetAttackRange() {
        return attackRange;
    }

    public int GetHealth() {
        return health;
    }
    public bool GetDieing() { return dieing; }

    public virtual void OnSpawn(Vector2 startPos) {
        transform.position = startPos;
        health = healthMax;
        gameObject.SetActive(true);
    }

    protected virtual void die() {
        if (myAnimator != null)
        { 
            myAnimator.SetBool("Dieing", true);
        }
        dieing = true;
        if (stunVFX != null)
        {
            stunVFX.Die();
            stunVFX = null;
        }
        if (fireVFX != null)
        {
            fireVFX.Die();
            fireVFX = null;
            
        }
        stunned = false;
        onFire = false;
        float chance = Random.Range(0, 100);
        if (chance < lootDropChance)
        {
            //Debug.Log("Loot Dropped with " + chance.ToString() + "at position " + transform.position.ToString());
            PlayerData.Player.AddGold(Random.Range(lootDropMin, lootDropMax + 1));
            AudioControlScript.mAC.PlayGetCoin();
        }

        checkIfRelatedQuest();


    }

    public void SetDriection(bool val) {
        headingLeft = val;
    }
    public void TakeDamageArea(int damageAmountArea, int stunAreaChance, float stunAreaLength) {
        if (active)
        {
            health -= damageAmountArea;
            if (Random.Range(0, 5) == 0)
            {
                PlayPainSoundArea();
            }
            if (health <= 0)
            {
                die();
            }
            if (gameObject.activeSelf)
            {
                checkStunChance(stunAreaChance, stunAreaLength);
            }
        }


    }
    public void TakeDamage(int damageAmount, int stunChance, float stunLength) {
        if (active)
        {
            health -= damageAmount;
            repulseOnHit();
            if (Random.Range(0, 5) == 0)
            {
                PlayPainSound();
            }

            if (health <= 0)
            {
                if (Random.Range(0, 5) < 1)
                {
                    PlayDieSound();
                }
                die();
            }
            if (gameObject.activeSelf)
            {
                checkStunChance(stunChance, stunLength);
            }
        }


    }

    void repulseOnHit() {
        myBody.bodyType = RigidbodyType2D.Dynamic;
        myBody.gravityScale = 0;
        float offset = 1.0f;
        if (headingLeft)
        {
            myBody.AddForce(new Vector2(offset, 0), ForceMode2D.Impulse);
        }
        else
        {
            myBody.AddForce(new Vector2(-offset, 0), ForceMode2D.Impulse);
        }
        hurlTime = hurlTimeR;
    }
    protected void checkStunChance(int valToRollUnder, float howLong) {
        if (canBeStunned)
        {
            if (Random.Range(0, 100) < valToRollUnder)
            {
                Debug.Log("Stunned!");
                AudioControlScript.mAC.PlayStunned();
                stunVFX = GameManagerScript.G.GetStunVFX();
                stunVFX.OnSpawn(transform.position);
                stunned = true;
                stunnedTime = howLong;
            }
        }


    }

    public virtual void Attack(bool isRanged = false) {

        ranged = isRanged;
        if (myAnimator != null) {
            myAnimator.SetBool("Attacking", true);
        }
        




    }

    public float GetAttackTime() {
        if (myAnimator != null)
        {
            return attackTimeReset;
        }
        else
        {
            return 2.0f;
        }
    }

    public void UpdateAnimClipTimes()
    {
        if (myAnimator != null)
        {
            AnimationClip[] clips = myAnimator.runtimeAnimatorController.animationClips;
            foreach (AnimationClip clip in clips)
            {
                switch (clip.name)
                {
                    case "Attack":
                        attackTimeReset = clip.length;
                        break;
                    case "Walk":
                        myWalkCD.SetTimeR(clip.length);
                        break;
                    case "Die":
                        dieingTimeReset = clip.length;
                        break;

                }
            }
        }

        
    }

    public virtual void ProduceProjectile() {

    }

    protected virtual void checkIfRelatedQuest() {

        for (int i = 0; i < QuestStorageScript.Singleton.MyQuests.Count; i++)
        {
            if (QuestStorageScript.Singleton.MyQuests[i].MyQuest == QuestStorageScript.QuestType.KILLATROLL) {
                if (GetType() == typeof(TrollScript) && !QuestStorageScript.Singleton.MyQuests[i].QuestState)
                {
                    QuestStorageScript.Singleton.MyQuests[i].IncreaseHave();
                }

            }
            if (QuestStorageScript.Singleton.MyQuests[i].MyQuest == QuestStorageScript.QuestType.KILLFIVESKELETONS)
            {
                if (GetType() == typeof(SkeletonScript) && !QuestStorageScript.Singleton.MyQuests[i].QuestState)
                {
                    QuestStorageScript.Singleton.MyQuests[i].IncreaseHave();
                }
            }


        }

    }

    protected virtual void PlayPainSound() {

    }
    protected virtual void PlayPainSoundArea(){
        PlayPainSound();
    }

    protected virtual void PlayDieSound() { }
    public bool GetActiveStat() { return active; }
    public void SetActiveStat(bool val) { active = val; }

    public float GetMoveSpeed() {
        return moveSpeed;
    }
}

