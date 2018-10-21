using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyMovementScript : MonoBehaviour {
    EnemyScript myEnemy;
    float moveSpeed = 0.5f;
    public bool LeftFacing = true;

	// Use this for initialization
	void Awake () {
        myEnemy = GetComponent<EnemyScript>();
	}
    void Start() {

        moveSpeed = myEnemy.GetMoveSpeed();
    }
	
	// Update is called once per frame
	void Update () {
        if (GameManagerScript.G.GetPause())
        {
            return;
        }

        if (Vector2.Distance(transform.position, PlayerData.Player.transform.position) < 12)
        {
            myEnemy.SetActiveStat(true);
            if (!myEnemy.GetStunned())
            {
                move();
            }
        }
        else
        {
            myEnemy.SetActiveStat(false);
        }
        
	}

    void move()
    {
        Vector3 pPos = PlayerData.Player.transform.position;
        bool shouldMoveTowards = true;
        if (!Camera.main.orthographic)
        {
            shouldMoveTowards = Vector3.Distance(transform.position, pPos) >= myEnemy.GetAttackRange();
        }
        else
        {
            shouldMoveTowards = Vector2.Distance(transform.position, pPos) >= myEnemy.GetAttackRange();
        }

        if (shouldMoveTowards)
        {
            if (pPos.x < transform.position.x)
            {
                myEnemy.SetDriection(true);
                if (LeftFacing)
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
            }
            else
            {
                myEnemy.SetDriection(false);
                if (LeftFacing)
                {
                    transform.eulerAngles = new Vector3(0, 180, 0);
                }
                else
                {
                    transform.eulerAngles = new Vector3(0, 0, 0);
                }
            }

            if (!Camera.main.orthographic)
            {
                transform.position = Vector3.MoveTowards(transform.position, pPos, moveSpeed * Time.deltaTime);
            }
            else
            {
                transform.position = Vector2.MoveTowards(transform.position, pPos, moveSpeed * Time.deltaTime);
            }
            
            myEnemy.SetInRange(false);
        }
        else {
            myEnemy.SetInRange(true);
        }
    }

    public void Move() {
        move();
    }
}
