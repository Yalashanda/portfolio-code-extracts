using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_PlayerMovement : MonoBehaviour
{
    public bool ShowDebug = false;
    Rigidbody myBody;
    public GameObject rotationObj;
    public Vector3 direction = new Vector3();
    public Vector3 aimDirection = new Vector3();
    
    s_Player myPlayer;

    //rotation variable
    public float angle = 0.0f;
    float angle2 = 0.0f;
    float angleChangeRate = 2.0f;


  

    void Start()
    {
        myPlayer = GetComponent<s_Player>();
        myBody = GetComponent<Rigidbody>();
        if (myPlayer.GetTeam() == 0)
        {
            setDirection(transform.right);
            aimDirection = transform.right;
        }
        else
        {
            setDirection(transform.right * -1);
            aimDirection = transform.right * -1;
        }
        
    }

    private void Update()
    {
        if (s_GameManager.GetPaused())
        {
            return;
        }
        if (myPlayer.GetCanMove() && !myPlayer.GetAI())
        {
            transform.position += ((direction.normalized) * myPlayer.GetMoveSpeed() * myPlayer.GetSlowedAmount() * Time.deltaTime);

        }

        if (ShowDebug || s_GameManager.ShowDebug())
        {
            Debug.Log("Direction is " + direction + " normailzed Direction is " + direction.normalized);
        }

    }

    //Returns the aim direction if player is standing still
    public Vector3 GetDirection() {
        Vector3 d = direction.normalized;
        return  d == Vector3.zero ? aimDirection.normalized : d;
    }
    public Vector3 GetDirectionRaw(bool _unNormalized = true)
    {
        if (_unNormalized)
        {
            return direction;
        }
        return direction.normalized;
    }
    public void MoveForward() {
        if (myPlayer.GetCanMove())
            transform.position += (myPlayer.GetForwardVector() * myPlayer.GetMoveSpeed() * Time.deltaTime);
    }
    public void MoveBackwards()
    {
        if (myPlayer.GetCanMove())
            transform.position -= (myPlayer.GetForwardVector() * myPlayer.GetMoveSpeed() * Time.deltaTime);
    }
    public void RotateRight() {
        if (myPlayer.GetCanMove())
            rotateLeft();
    }
    public void RotateLeft()
    {
        if (myPlayer.GetCanMove())
            rotateRight();
    }

    public void MoveNorth()
    {

        setDirection((direction + new Vector3(0, 0, 1)).normalized);
    }
    public void MoveSouth()
    {

        setDirection((direction + new Vector3(0, 0, -1)).normalized);
    }
    public void MoveEast()
    {
        setDirection((direction + new Vector3(1, 0, 0)).normalized);

    }
    public void MoveWest()
    {
        setDirection((direction + new Vector3(-1, 0, 0)).normalized);
    }

    public void MoveJoy(float dir, bool horizontal)
    {
        if (horizontal)
        {
            setDirection(new Vector3(dir, direction.y, direction.z).normalized);
        }
        else
        {
            setDirection(new Vector3(direction.x, direction.y, dir).normalized);
        }

    }

    public void AimJoy(float dir, bool horizontal)
    {
        if (myPlayer.GetCurrentState() == s_Player.States.Stunned)
        {
            return;
        }
        Vector3 currentDirection = aimDirection;
        if (horizontal)
        {
            aimDirection = new Vector3(dir, aimDirection.y, aimDirection.z).normalized;
        }
        else
        {
            aimDirection = new Vector3(aimDirection.x, aimDirection.y, dir).normalized;
        }


        if (currentDirection != aimDirection)
        {
            float a = Vector3.Dot(currentDirection, aimDirection) / currentDirection.sqrMagnitude * aimDirection.sqrMagnitude;
            a = Mathf.Acos(a);
            myPlayer.SetForwardVector(aimDirection);
            float tempVal = Mathf.Rad2Deg * a;
        }

    }
    public void AimMouse(Vector3 dir)
    {
        if (myPlayer.GetCurrentState() == s_Player.States.Stunned)
        {
            return;
        }
        Vector3 currentDirection = aimDirection;
        aimDirection = dir;


        if (currentDirection != aimDirection)
        {
            float a = Vector3.Dot(currentDirection, aimDirection) / currentDirection.sqrMagnitude * aimDirection.sqrMagnitude;
            a = Mathf.Acos(a);
            myPlayer.SetForwardVector(aimDirection);
            //float tempVal = Mathf.Rad2Deg * a;
           
        }

    }

    public Vector3 GetAimDirection() {
        return aimDirection;
    }

    public void MoveNorthUp()
    {
        setDirection(new Vector3(direction.x, direction.y, 0).normalized);
    }
    public void MoveSouthUp()
    {
        setDirection(new Vector3(direction.x, direction.y, 0).normalized);
    }
    public void MoveEastUp()
    {
        
        setDirection(new Vector3(0, direction.y, direction.z).normalized);

    }
    public void MoveWestUp()
    {
        setDirection(new Vector3(0, direction.y, direction.z).normalized);

    }


    void setForwardVector()
    {
     
            float x = (myPlayer.GetForwardVector().x * Mathf.Cos(angle)) + (myPlayer.GetForwardVector().z * Mathf.Sin(angle));
            float y = myPlayer.GetForwardVector().y;
            float z = (-myPlayer.GetForwardVector().x * Mathf.Sin(angle)) + (myPlayer.GetForwardVector().z * Mathf.Cos(angle));
            myPlayer.SetForwardVector(new Vector3(x, y, z));
            aimDirection = new Vector3(x, y, z);
            rotateImage();


    }
    void setUpVector()
    {
        float x = (myPlayer.GetUpVector().x * Mathf.Cos(angle2)) + (myPlayer.GetUpVector().z * Mathf.Sin(angle2));
        float y = (-myPlayer.GetUpVector().x * Mathf.Sin(angle2)) + (myPlayer.GetUpVector().z * Mathf.Cos(angle2));
        float z = myPlayer.GetUpVector().y;
        myPlayer.SetUpVector(new Vector3(x, y, z));
        rotateImage();

    }
    void rotateImage()
    {
            //transform.Rotate(new Vector3(0, 1, 0), Mathf.Rad2Deg * angle);
    }

    void rotateLeft()
    {
        angle = angleChangeRate * Time.deltaTime;
        setForwardVector();
    }
    void rotateUpLeft()
    {
        angle2 = -angleChangeRate * Time.deltaTime;
        setUpVector();
    }
    void rotateRight()
    {
        angle = -angleChangeRate * Time.deltaTime;
        setForwardVector();
    }
    public float GetAngle()
    {
        return angle;
    }

    void setDirection(Vector3 _dir)
    {        
        direction = _dir;
    }

    public Vector3 GetUpVector()
    {
        return myPlayer.GetUpVector();
    }
    public Vector3 GetForwardVector()
    {
        return myPlayer.GetForwardVector();
    }
   
    public float GetMoveSpeed() {
        return myPlayer.GetMoveSpeed();
    }
    public Rigidbody GetRigidBody() {
        return myBody;
    }
   
    void setPositionElement(float elementValue, int vectorIndexToChange) {

        switch (vectorIndexToChange)
        {
            case 0:
                transform.position = new Vector3(elementValue, transform.position.y, transform.position.z);
                break;
            case 1:
                transform.position = new Vector3(transform.position.x, elementValue, transform.position.z);
                break;
            case 2:
                transform.position = new Vector3(transform.position.x, transform.position.y, elementValue);
                break;
            default:
                break;
        }
    }
  
}

