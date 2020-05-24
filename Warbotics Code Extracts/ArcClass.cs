using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcClass {
    public float arc = 1;
    public float lastArc = -999;
    float maxAngle = 180;
    public ArcClass(float last)
    {
        ResetArc(last);
    }


    public void ResetArc(float ToCheck)
    {

        if (ToCheck > (maxAngle))
        {
            ToCheck -= maxAngle;
        }
        if (lastArc != ToCheck || lastArc == -999)
        {
            arc = ((maxAngle - ToCheck) / maxAngle);
            lastArc = ToCheck;
        }
    }
}
