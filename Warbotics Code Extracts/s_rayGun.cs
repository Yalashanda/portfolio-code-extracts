using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
public class s_rayGun : s_WeaponRanged
{
    LineRenderer m_lineSegment;
    s_Player m_hitPlayer = null;
    s_HazardBarrel m_barrel = null;
    bool m_beamOn = false;
    Timer lineSegmentOff = new Timer(0.3f);
    Timer DamageRate = new Timer(0.5f);
    Vector3 m_beamOrigin;
    float m_distance = 20;
    const float M_DISTANCERESET = 100;
    AudioControlerScript.Handle m_handle = null;
    float yPos = 1.0f;
    public float sphereRadius = 0.25f;

    Timer AIFireRate = new Timer(0.5f);
    // Start is called before the first frame update
    void Start()
    {
        myWeaponRange = WeaponDistance.Ranged;
        onStart();
        m_lineSegment = GetComponentInChildren<LineRenderer>();
       
    }

    // Update is called once per frame
    void Update()
    {
        if (s_GameManager.GetPaused())
            return;
        onUpdate();
        if (holder != null)
        {
            if (holder.GetAI())
            {
                if (AIFireRate.CountDownAutoCheckBool())
                {
                    holder.SetFireButtonDown(false);
                }
            }
            if (holder.GetFireButtonDown())
            {
                m_beamOn = true;
                lineSegmentOff.SetTimer(lineSegmentOff.timeReset);

            }
            fireBeam();
            if (lineSegmentOff.CountDown())
                m_beamOn = false;

        }
        else
        {
            turnOffBeam();
        }
    }

    protected override void fireWeapon()
    {
        if (holder.GetAI())
        {
            holder.SetFireButtonDown(true);
            AIFireRate.SetTimerShouldCountDown(true);
            AIFireRate.SetTimer(AIFireRate.timeReset);
        }
    }
    protected void fireBeam()
    {

        if (m_beamOn)
        {
            
            float yOrigin = holder.transform.position.y + yPos;
            m_distance = M_DISTANCERESET;
            m_beamOrigin = new Vector3(m_pointForBullets.transform.position.x, yOrigin, m_pointForBullets.transform.position.z);
            Vector3 direction = m_beamOrigin + (holder.GetAimDirection() * m_distance);
            m_lineSegment.SetPositions(new Vector3[] { m_beamOrigin, direction });
            Ray beam = new Ray(m_beamOrigin, holder.GetAimDirection());
            RaycastHit[] hits;
            hits = Physics.SphereCastAll(beam, sphereRadius, m_distance, Physics.AllLayers).OrderBy(h => h.distance).ToArray();
            for (int i = 0; i < hits.Length; i++)
            {
                s_Obstacle obs = hits[i].collider.gameObject.GetComponent<s_Obstacle>();
                s_Wall wall = hits[i].collider.gameObject.GetComponent<s_Wall>();
                if (hits[i].collider.gameObject == holder.gameObject || (hits[i].collider.gameObject.GetComponent<s_Floor>() != null && obs == null && wall == null))
                {
                    continue;
                }
                m_hitPlayer = hits[i].collider.gameObject.GetComponent<s_Player>();
                m_barrel = hits[i].collider.gameObject.GetComponent<s_HazardBarrel>();

                s_Hazard hazard = hits[i].collider.gameObject.GetComponent<s_Hazard>();
                if (m_barrel != null)
                {
                    m_distance = Vector3.Distance(m_beamOrigin, m_barrel.transform.position);
                    direction = m_beamOrigin + (holder.GetAimDirection() * m_distance);
                    m_lineSegment.SetPositions(new Vector3[] { m_beamOrigin, direction });
                    break;
                }
                if (m_hitPlayer != null && m_hitPlayer.GetCurrentState() != s_Player.States.Dead)
                {
                    m_distance = Vector3.Distance(m_beamOrigin, m_hitPlayer.transform.position);
                    direction = m_beamOrigin + (holder.GetAimDirection() * m_distance);
                    m_lineSegment.SetPositions(new Vector3[] { m_beamOrigin, direction });
                    break;
                }

                if (obs != null || wall != null || hazard != null)
                {
                    
                    m_distance = Vector3.Distance(m_beamOrigin, hits[i].point);
                    direction = m_beamOrigin + (holder.GetAimDirection() * m_distance);
                    m_lineSegment.SetPositions(new Vector3[] { m_beamOrigin, direction });
                    break;
                }
                if (hits[i].collider != null && hits[i].collider.gameObject != holder.gameObject)
                {
                    if (!hits[i].collider.isTrigger)
                    {
                        m_distance = M_DISTANCERESET;
                        if(hits[i].collider.gameObject != null)
                            m_distance = Vector3.Distance(m_beamOrigin, hits[i].collider.gameObject.transform.position);
                        direction = m_beamOrigin + (holder.GetAimDirection() * m_distance);
                        m_lineSegment.SetPositions(new Vector3[] { m_beamOrigin, direction });
                    }
                }
            }

            if (m_handle == null)
            {
                m_handle = AudioControlerScript.PlaySoundLoop(AudioControlerScript.Clips.phaser_Alexander, 0);
            }
            m_lineSegment.gameObject.SetActive(true);
            if (DamageRate.CountDown())
            {
                if (m_barrel != null)
                {
                    AudioControlerScript.PlaySound(AudioControlerScript.Clips.damage_001);
                    m_barrel.TakeDamage(damage);
                }
                if (m_hitPlayer != null && m_hitPlayer != holder)
                {
                    AudioControlerScript.PlaySound(AudioControlerScript.Clips.damage_001);
                    m_hitPlayer.TakeDamage(damage);

                }
            }
        }
        else
        {
            turnOffBeam();
        }
        
        

    }

    private void OnDestroy()
    {
        if (m_handle != null)
        {
            AudioControlerScript.StopSoundLoop(m_handle, 0);
            m_handle = null;
        }
    }

    void turnOffBeam()
    {
        m_lineSegment.gameObject.SetActive(false);
        if (m_handle != null)
        {
            AudioControlerScript.StopSoundLoop(m_handle, 0);
            m_handle = null;
        }
    }
}
