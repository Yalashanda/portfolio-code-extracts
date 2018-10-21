using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttackScript : MonoBehaviour {

    EnemyScript myEnemy;
    bool canAttack = true;
    float time;
    float timeR;
    public float aF = 1.0f; //extra time between attack animations
    bool animEnded = false;
    public bool ranged;
    
    // Use this for initialization
    void Awake()
    {
        myEnemy = GetComponent<EnemyScript>();
        
            timeR = myEnemy.GetAttackTime() + aF;
        
    }

    // Update is called once per frame
    void Update()
    {
        if (GameManagerScript.G.GetPause())
        {
            return;
        }
        if (!myEnemy.GetStunned() && myEnemy.GetInRange() && canAttack && !myEnemy.GetDieing())
        {
            myEnemy.Attack(ranged);
            canAttack = false;
        }

        
            if (!canAttack)
            {
                
                   
                if (time <= 0)
                {
                    myEnemy.AttackEnd();
                    canAttack = true;
                    animEnded = false;
                    time = timeR;
                }

                if (time <= timeR - myEnemy.GetAttackTime() && !animEnded)
            {
                    myEnemy.AttackEnd();
                    if (ranged)
                    {
                        myEnemy.ProduceProjectile();
                    }
                
                    animEnded = true;
                }

            time -= Time.deltaTime;
            }
        
       
             
        
    }

   


}
