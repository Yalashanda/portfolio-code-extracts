using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrcScript : EnemyScript {
    void Awake()
    {
        moveSpeed = 1.0f;
    }

    // Use this for initialization
    void Start () {
        attackRange = 5;
        lootDropChance = 60;
        lootDropMin = 5;
        lootDropMax = 15;
        health = 6;
        healthMax = 6;
        damgeToInflict = 2;
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
        AudioControlScript.mAC.PlayOrcHit();
    }
    protected override void PlayDieSound()
    {
        AudioControlScript.mAC.PlayOrcDie();
    }

}
