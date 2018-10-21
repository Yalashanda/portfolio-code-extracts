using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComboScript : MonoBehaviour {
        
    float timeReset = 0.45f;
    float time = 0.5f;
    float bufferTimeReset = 6.0f;
    float bufferTime = 0.5f;
    string pastPressesString = "";
    //vlad combos
    string fireballCombo = "WSWSLMB";
    bool canFireball = true;
    string thunderStrikeCombo = "SSSARMB";
    bool canThunderStrike = true;
    string roundhouseFireCombo = "AADWWSLMB";
    bool canRoundHouse = true;
    //Dalv combos
    string lightningStormCombo = "AAAWSLMB";
    bool canlightningStorm = true;
    string flameWallCombo = "SSWSSRMB";
    bool canFlameWall = true;
    string shockingBodyCombo = "AADSWSRMB";
    bool canShockingBody = true;
    //both
    string electricFlameCombo = "ADADDAALMB";
    string electricFlameCombo2 = "ADAAADDLMB";
    bool canElectricFlame = true;





    timer roundhouseFireTimer = new timer();
    timer electricFlameTimer = new timer();
    timer thunderStrikeTimer = new timer();
    timer fireballTimer = new timer();
    timer lightningStormTimer = new timer();
    timer flameWallTimer = new timer();
    timer shockingBodyTimer = new timer();


    void Start() {

        roundhouseFireTimer.SetTimeR(10.0f);
        electricFlameTimer.SetTimeR(5.0f);
        thunderStrikeTimer.SetTimeR(7.0f);
        fireballTimer.SetTimeR(5.0f);
        lightningStormTimer.SetTimeR(5.0f);
        flameWallTimer.SetTimeR(12.0f);
        shockingBodyTimer.SetTimeR(20.0f);
    }
    // Update is called once per frame
    void Update() {
        
        timers();
        keyChecks();
        countDown();

    }

    void timers() {
        if (!canRoundHouse)
        {
            if (roundhouseFireTimer.countDown())
            {
                canRoundHouse = true;
            }
        }


        if (!canElectricFlame)
        {
            if (electricFlameTimer.countDown())
            {
                canElectricFlame = true;
            }
        }


        if (!canFireball)
        {
            if (fireballTimer.countDown())
            {

                canFireball = true;
            }
        }

        if (!canThunderStrike)
        {
            if (thunderStrikeTimer.countDown())
            {
                canThunderStrike = true;
            }
        }

//Dalv spells
        if (!canShockingBody)
        {
            if (shockingBodyTimer.countDown())
            {
                canShockingBody = true;
            }
        }


        if (!canlightningStorm)
        {
            if (lightningStormTimer.countDown())
            {
                canlightningStorm = true;
            }
        }



        if (!canFlameWall)
        {
            if (flameWallTimer.countDown())
            {
                canFlameWall = true;
            }
        }




    }

    void countDown() {

        if (bufferTime <= 0)
        {
            pastPressesString = "";
            bufferTime = bufferTimeReset;
        }
        bufferTime -= Time.deltaTime;
    }

    void compareKeyPresses() {
        if (checkAgainstInputCombo2(fireballCombo) && canFireball)
        {
            canFireball = false;
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.FIREBALL);
            
        }

        else if (checkAgainstInputCombo2(thunderStrikeCombo) && canThunderStrike)
        {
            canThunderStrike = false;
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.THUNDERSTRIKE);
        }
        else if ((checkAgainstInputCombo2(electricFlameCombo) || checkAgainstInputCombo2(electricFlameCombo2)) && canElectricFlame)
        {
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.ELECTRICFLAME);
            canElectricFlame = false;
        }
        else if (checkAgainstInputCombo2(roundhouseFireCombo) && canRoundHouse)
        {
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.ROUNDHOUSEFIREBOLT);
            canRoundHouse = false;
        }
        else if (checkAgainstInputCombo2(lightningStormCombo) && canlightningStorm)
        {
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.LIGHTNINGSTORM);
            canlightningStorm = false;
        }
        else if (checkAgainstInputCombo2(flameWallCombo) && canFlameWall)
        {
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.FLAMEWALL);
            canFlameWall = false;
        }
        else if (checkAgainstInputCombo2(shockingBodyCombo) && canShockingBody)
        {
            PlayerData.Player.SetQueuedCombo(PlayerData.ComboSpell.SHOCKINGBODY);
            canShockingBody = false;
        }


    }
    bool checkAgainstInputCombo2(string combo) {
        if (pastPressesString.Contains(combo))
        {
            return true;
        }
        return false;

    }

    public void ForceComparePress() {
        
        compareKeyPresses();
        
    }
    
    void keyChecks() {
        if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
        {
            pastPressesString += "D";
            time = timeReset;
        }

        if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
        {
            pastPressesString += "A";
            time = timeReset;
        }

        if (Input.GetKeyUp(KeyCode.W) || Input.GetKeyUp(KeyCode.UpArrow))
        {
            pastPressesString += "W";
            time = timeReset;
        }

        if (Input.GetKeyUp(KeyCode.S) || Input.GetKeyUp(KeyCode.DownArrow))
        {
            pastPressesString += "S";
            time = timeReset;
        }


        if (Input.GetMouseButtonUp(0))
        {
            pastPressesString += "LMB";
            time = timeReset;
        }


        if (Input.GetMouseButtonUp(1))
        {
            pastPressesString += "RMB";
            time = timeReset;
        }

        
    }


  
}
