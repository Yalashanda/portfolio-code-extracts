using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinScript : EnemyScript {
    void Awake()
    {
        moveSpeed = 1.5f;
    }
    // Use this for initialization
    void Start () {
        attackRange = 1;
        lootDropChance = 50;
        lootDropMin = 5;
        lootDropMax = 15;
        health = 3;
        healthMax = 3;
        damgeToInflict = 1;
        onStart();
       
    }

    void Update() {
        onUpdate();
   
    }

    public override void ProduceProjectile()
    {
        Vector3 pPos = PlayerData.Player.transform.position;
        float offset = 0.5f;
        if (pPos.x > transform.position.x)
        {
            GameManagerScript.G.GetArrows().OnSpawn(transform.position + new Vector3(offset, transform.position.z * 0.3f, 0), pPos);
        }
        else
        {
            GameManagerScript.G.GetArrows().OnSpawn(transform.position + new Vector3(-offset, transform.position.z * 0.3f, 0), pPos);
        }
    }


    protected override void PlayPainSound()
    {
        AudioControlScript.mAC.PlayGoblinHit();
    }
    protected override void PlayDieSound()
    {
        AudioControlScript.mAC.PlayGoblinDie();
    }
}
