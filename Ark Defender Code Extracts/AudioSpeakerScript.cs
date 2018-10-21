using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioSpeakerScript : MonoBehaviour {

    AudioSource mySource;
    bool playing = false;
    float time = 1;

    // Use this for initialization
    void Start () {
        mySource = GetComponent<AudioSource>();
	}
	
	// Update is called once per frame
	void Update () {
        
            if (time <= 0 && playing)
            {
                playing = false;
                AudioControlerScript.Singleton.FreeToken();
                if (mySource.isPlaying)
                {
                    mySource.Stop();
                }
            }
            time -= Time.deltaTime;
        

        
	}


    public AudioSource GetSource() {
        return mySource;
    }
    public bool GetIsPlaying() {

        return mySource.isPlaying;
    }

    public void PlaySound(AudioClip myClip, float vol = 1, float pitch = 1, float pan = 0, int priorty = 128) {

        time = myClip.length;
        if (time > 5.0f)
        {
            time = 5.0f;
        }
        playing = true;
        mySource.volume = vol;
        mySource.pitch = pitch;
        mySource.panStereo = pan;
        mySource.priority = priorty;
        mySource.clip = myClip;
        mySource.Play();
    }



}
