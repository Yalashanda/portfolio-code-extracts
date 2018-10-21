using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Path1Script : MonoBehaviour {

    public float ShipSpeed = 2.0f;
    public int nodeIndex;
    public Vector3[] arrayOfNodes = {new Vector3 (0,0,0), new Vector3(0, 0, 0), new Vector3(0, 0, 0) };
    Vector3[] arrayOfTargets;
    public Vector3 newDir;
    
    // Use this for initialization
    void Start () {
        nodeIndex = 0;

        arrayOfTargets = arrayOfNodes;
        for (uint i = 0; i < arrayOfNodes.Length; i++)
        {
            if (i == 0)
            {
                arrayOfNodes[i] = transform.position;
            }
            else
            {
                arrayOfNodes[i].x = arrayOfNodes[i].x + arrayOfNodes[i - 1].x;// + offset;
                arrayOfNodes[i].y = arrayOfNodes[i].y + arrayOfNodes[i - 1].y;
            }

            if (i == arrayOfNodes.Length - 1)
            {
                arrayOfNodes[i] = transform.position;
            }
        }

        




    }

    // Update is called once per frame
    void Update() {
        

            if (nodeIndex < arrayOfTargets.Length)
            {
                if (Vector3.MoveTowards(transform.position, arrayOfTargets[nodeIndex], ShipSpeed * Time.deltaTime) != arrayOfTargets[nodeIndex])
                {
                
                transform.position = Vector3.MoveTowards(transform.position, arrayOfTargets[nodeIndex], ShipSpeed * Time.deltaTime);
                
                
                Vector2 moveDirection = new Vector2 (transform.position.x - arrayOfTargets[nodeIndex].x, transform.position.y - arrayOfTargets[nodeIndex].y);
                if (moveDirection != Vector2.zero)
                {
                    float angle = Mathf.Atan2(moveDirection.x, moveDirection.y) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.AngleAxis(angle, Vector3.back);
                }


            }
            else
                {
                    nodeIndex++;
                    if (nodeIndex >= arrayOfTargets.Length)
                    {
                        nodeIndex = 0;

                        for (uint i = 0; i < arrayOfNodes.Length; i++)
                        {
                            arrayOfNodes[i].y -= 1.5f;

                        }
                    }
                }
            }






        if (transform.position.y < -7)
        {
            Destroy(gameObject);
        }





    }


    float FindDegree(float x, float y)
    {
        float value = (float)((System.Math.Atan2(x, y) / System.Math.PI) * 180f);
        if (value < 0)
        {
            value += 360f;
        }
        return value;
    }


    



}
