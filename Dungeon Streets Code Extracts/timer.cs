using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class timer {

   private float _time;
   private float _timeR;
    public float timeR{
        get {
            return _timeR;
        }

    }

    
    public float time
    {
        get
        {
            return _time;
        }

    }

    public void SetTimeR(float val)
        {
            _timeR = val;
            _time = val;
        }
    public void SetTime(float val)
    {
        _time = val;
    }

    public bool countDown()
        {
            if (time <= 0)
            {
                _time = _timeR;
                return true;

            }
            else
            {
            
            _time -= Time.deltaTime;
            return false;
            }

        }
    
}
