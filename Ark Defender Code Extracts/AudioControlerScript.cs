using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioControlerScript : MonoBehaviour {
    //This script was set up to serve two main purposes the first was to limit the number of sounds 
    //that could be played at given time and the second was to centralize the audio assets in the game
    public int availableTokens = 10;
    public GameObject speaker1;
    
    AudioSpeakerScript[] mySpeakers;
    AudioSpeakerScript[] myPriortySpeakers;
    public AudioClip GotLootSnd;
    public AudioClip PowerIncreaseSnd;
    public AudioClip PowerDecreaseSnd;
    public AudioClip NotEnoughTime;
    public AudioClip NotEnoughResources;
    public AudioClip RadationBeltSnd;
    public AudioClip RadationBeltWarningSnd;
    public AudioClip AsteroidDestroyedSnd;
    public AudioClip AsteroidBreakUpSnd;
    public AudioClip PointDefFire;
    public AudioClip EnergyWeaponFire;
    public AudioClip FighterExplosionSnd;
    public AudioClip CapitalExlposionSnd;
    public AudioClip MissileExplosionSnd;
    public AudioClip MegaMissileExplosionSnd;
    public AudioClip ArkExplosionSnd;
    public AudioClip ScreamsOfDyingSnd; //remains unused
    public AudioClip ManufacturingSnd;
    public AudioClip ShieldImpactSnd;
    public AudioClip FlakFireSnd;
    public AudioClip FighterFireSnd;
    public AudioClip MissileFireSnd;
    public static AudioControlerScript Singleton;
    

    // Use this for initialization
    void Start () {
        mySpeakers = new AudioSpeakerScript[availableTokens];
        myPriortySpeakers = new AudioSpeakerScript[3];
        if (Singleton != null && Singleton != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Singleton = this;
        }

        for (int i = 0; i < mySpeakers.Length; i++)
        {
            GameObject s = Instantiate(speaker1, transform.position, Quaternion.identity);
            mySpeakers[i] = s.GetComponent<AudioSpeakerScript>();
        }
        for (int i = 0; i < myPriortySpeakers.Length; i++)
        {
            GameObject s = Instantiate(speaker1, transform.position, Quaternion.identity);
            myPriortySpeakers[i] = s.GetComponent<AudioSpeakerScript>();
        }
        


    }

    bool tokenCheck() {
        if (availableTokens > 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }


    void playsnd(AudioClip myClip, bool priorty, float vol, float pitch, float pan, int priortyInt)
    {
        if (!priorty)
        {
            if (tokenCheck())
            {

                for (int i = 0; i < mySpeakers.Length; i++)
                {
                    if (!mySpeakers[i].GetIsPlaying())
                    {
                        if (myClip != null)
                        {
                            mySpeakers[i].PlaySound(myClip, vol);
                            break;
                        }
                        break;


                    }
                }


            }
        }
        else
        {
            bool foundSpeaker = false;
            for (int i = 0; i < mySpeakers.Length; i++)
            {
                if (!mySpeakers[i].GetIsPlaying())
                {
                    if (myClip != null)
                    {
                        mySpeakers[i].PlaySound(myClip, vol);
                        foundSpeaker = true;
                        break;
                    }
                    break;
                }
            }

            if (!foundSpeaker)
            {

                for (int i = 0; i < myPriortySpeakers.Length; i++)
                {
                    if (!myPriortySpeakers[i].GetIsPlaying())
                    {
                        myPriortySpeakers[i].PlaySound(myClip);
                        foundSpeaker = true;
                        break;
                    }
                }

                if (!foundSpeaker)
                {
                    myPriortySpeakers[0].PlaySound(myClip);
                }
            }
        }

    }


    public void FreeToken() {
        /*availableTokens++;
        Debug.Log("Token Freed");*/
    }

    public void ClearTokens() {
        for (int i = 0; i < mySpeakers.Length; i++)
        {
            mySpeakers[i].GetSource().Stop();
        }
        availableTokens = 10;
    }

    

    public void PlayPowerIncreaseSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128) {playsnd(PowerIncreaseSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayPowerDecreaseSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(PowerDecreaseSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayNotEnoughTime(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(NotEnoughTime, priorty, vol, pitch, pan, priortyInt); }
    public void PlayNotEnoughResources(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(NotEnoughResources, priorty, vol, pitch, pan, priortyInt); }
    public void PlayRadationBeltSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(RadationBeltSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayRadationBeltWarningSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128) { playsnd(RadationBeltWarningSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayAsteroidDestroyedSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(AsteroidDestroyedSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayAsteroidBreakUpSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(AsteroidBreakUpSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayPointDefFire(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(PointDefFire, priorty, vol, pitch, pan, priortyInt); }
    public void PlayEnergyWeaponFire(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(EnergyWeaponFire, priorty, vol, pitch, pan, priortyInt); }
    public void PlayFighterExplosionSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(FighterExplosionSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayCapitalExlposionSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(CapitalExlposionSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayMissileExplosionSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128) {playsnd(MissileExplosionSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayMegaMissileExplosionSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128) { playsnd(MegaMissileExplosionSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayArkExplosionSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(ArkExplosionSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayScreamsOfDyingSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(ScreamsOfDyingSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayManufacturingSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(ManufacturingSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayShieldImpactSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(ShieldImpactSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayFlakFireSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(ShieldImpactSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayFighterFireSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(FighterFireSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayMissileFireSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128){playsnd(MissileFireSnd, priorty, vol, pitch, pan, priortyInt); }
    public void PlayGotLootSnd(bool priorty = false, float vol = 1, float pitch = 1, float pan = 0, int priortyInt = 128) { playsnd(GotLootSnd, priorty, vol, pitch, pan, priortyInt); }


}
