using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrusterScript : MonoBehaviour {
    public bool ThrustersDamagable;
    public s_ThrusterBoost myBoostNoise;
    public enum direction {
        NULL,
        UL,
        LL,
        UR,
        LR
    }
    List<direction> damagedThrusters = new List<direction>();
    public s_SegementStatBar myFuelBar;
    public bool cumlative = false;
    [Tooltip("Undamaged speed multiplier")]
    public float vFactor = 5f;
    [Tooltip("Damaged speed multiplier")]
    public float vFactorDamaged = 2.5f;
    [Tooltip("Amount by which fuel is increased at intervals")]
    public float FuelRechargeAmount = 0.5f; //amount by which fuel is increased at intervals
    [Tooltip("Interval at which fuel is increased")]
    public float FuelRechargeRate = 0.05f; //interval at which fuel is increased
    [Tooltip("Amount of fuel is consumed per fixed update (Rate of fuel consumption)")]
    public float FuelConsumptionAmount = 0.2f; //amount by which fuel is consumed

    direction thrustDirection = direction.NULL;

    Rigidbody myBody;
    public GameObject LRThruster;
    public GameObject LLThruster;
    public GameObject URThruster;
    public GameObject ULThruster;
    float maxSpeed = 5.0f;


    Vector3 LRThrusterV;
    Vector3 LLThrusterV;
    Vector3 URThrusterV;
    Vector3 ULThrusterV;
    Vector3 thrustPointVector = Vector3.zero;
   
    bool canThrust = true;
    
    Timer FuelChargeTimer = new Timer();
    float maxFuel = 100;
    float fuel = 100;
    public float initalThrustFuelRequirement = 5;


    // Use this for initialization
    void Start() {
        fuel = maxFuel;
      
        LRThrusterV = LRThruster.transform.localPosition;
        LLThrusterV = LLThruster.transform.localPosition;
        URThrusterV = URThruster.transform.localPosition;
        ULThrusterV = ULThruster.transform.localPosition;
        myBody = GetComponent<Rigidbody>();
        FuelChargeTimer.SetTimer(FuelRechargeRate);
        myFuelBar.OnSpawn(fuel, maxFuel);



    }

    void Update() {
        if (s_GameManager.Singleton.GetPaused())
        {
            return;
        }
        if (FuelChargeTimer.CountDown())
        {
            AddFuel(FuelRechargeAmount);
            s_Sailer.Singleton.AddLaser(0.2f);
        }
    }

    void FixedUpdate() {
        if (s_GameManager.Singleton.GetPaused())
        {
            return;
        }
        if (fuel < initalThrustFuelRequirement)
        {
            canThrust = false;
        }
        else
        {
            canThrust = true;
        }

        //when any key is released the thrust direction is set to null
        if ((SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_LR) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_UR) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_LL) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_UL))  && (!cumlative))
        {
            myBoostNoise.gameObject.SetActive(false);
            thrustDirection = direction.NULL;
            
        }
        if (canThrust)
        {
            
            if (SimonXInterface.GetButtonDown(SimonXInterface.SimonButtonType.Button_LR) ||
                SimonXInterface.GetButtonDown(SimonXInterface.SimonButtonType.Button_UR)||
                SimonXInterface.GetButtonDown(SimonXInterface.SimonButtonType.Button_LL)||
                SimonXInterface.GetButtonDown(SimonXInterface.SimonButtonType.Button_UL))
            {
                ReduceFuel(initalThrustFuelRequirement);

            }

            thrustPointVector = Vector3.zero;
            
            //sets the direction of the thrust
            if (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_LR) && (thrustDirection == direction.NULL || cumlative || thrustDirection == direction.LR))
            {
                effectThrustPointVector(LRThrusterV);
                thrustDirection = direction.LR;
            }

            if (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_LL) && (thrustDirection == direction.NULL || cumlative || thrustDirection == direction.LL))
            {
                effectThrustPointVector(LLThrusterV);
                thrustDirection = direction.LL;
            }


            if (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_UL) && (thrustDirection == direction.NULL || cumlative || thrustDirection == direction.UL))
            {
                effectThrustPointVector(ULThrusterV);
                thrustDirection = direction.UL;
            }


            if (SimonXInterface.GetButton(SimonXInterface.SimonButtonType.Button_UR) && (thrustDirection == direction.NULL || cumlative || thrustDirection == direction.UR))
            {

                effectThrustPointVector(URThrusterV);
                thrustDirection = direction.UR;

            }


            
            
            //if the thrust direction has been set
            if (thrustPointVector != Vector3.zero)
            {
                if (fuel > GetMinimumFuel() * 1.5f)
                {
                    
                    //add force in the direction dependent on the thrust direction
                    if (!damagedThrusters.Contains(thrustDirection))
                    {
                        myBody.AddForce((thrustPointVector).normalized * vFactor, ForceMode.Impulse);
                    }
                    else
                    {
                        myBody.AddForce((thrustPointVector).normalized * vFactorDamaged, ForceMode.Impulse);
                    }
                    myBoostNoise.gameObject.SetActive(true);
                }
                ReduceFuel(FuelConsumptionAmount);
            }
            
            //caps speed;
            if (myBody.velocity.magnitude > maxSpeed)
            {
                myBody.velocity = myBody.velocity.normalized * maxSpeed;
            }
            //if the auto stop power up active then stop as soon as key released.
            if (s_Sailer.Singleton.GetAutoStop())
            {
                if (SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_LR) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_UR) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_LL) ||
                SimonXInterface.GetButtonUp(SimonXInterface.SimonButtonType.Button_UL))
                {
                    myBody.velocity = Vector3.zero;
                }
            }
        }

    }

    void effectThrustPointVector(Vector3 inputVector) {
        //determines if the thrust from multiple thrusters can work together, or only one thruster at a time can influence movement
        if (cumlative)
        {
            thrustPointVector += inputVector;
        }
        else
        {
            thrustPointVector = inputVector;
        }
    }

    public float GetFuel() {
        return fuel;
    }
    public void ReduceFuel(float amountUsed) {
        fuel -= amountUsed;
        if (fuel < 0)
        {
            fuel = 0;
        }
        myFuelBar.SetBar(fuel, maxFuel);


    }
    public void AddFuel(float val) {
        fuel += val;
        if (fuel > maxFuel)
        {
            fuel = maxFuel;
        }
        myFuelBar.SetBar(fuel, maxFuel);

    }
    public float GetMinimumFuel() {
        return initalThrustFuelRequirement;
    }
    public float GetMaxFuel() {
        return maxFuel;
    }

    public void AddDamagedThruster(direction damagePoint) {
        if (ThrustersDamagable)
        {
            if (!damagedThrusters.Contains(damagePoint))
            {
                damagedThrusters.Add(damagePoint);
            }
        }
    }
    
}
