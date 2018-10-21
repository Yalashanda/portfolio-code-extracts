using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeamCannonScript : MonoBehaviour {

   
    protected Collider2D myCollider;
    public int MinDam;
    public int MaxDam;
    
    public AudioClip ImpactSound;
    public PlayImpactScript Impact;
    public GameObject impactExplosion;
    protected AudioSource mySource;

    float posX;
    float posY;
    float posZ;
    float drainRate = 0.02f;
    float time;
    // Use this for initialization
    void Start () {
        posX = transform.localPosition.x;
        posY = transform.localPosition.y;
        posZ = transform.localPosition.z;
        myCollider = GetComponent<BoxCollider2D>();
        mySource = GetComponent<AudioSource>();
        time = drainRate;
    }
	
	// Update is called once per frame
	void Update () {
        //due to limiations in times we ended up using a sub optimal graphic for the beam requiring that I scale it.
        float xscale = 1f / transform.parent.transform.localScale.x;
        float yscale = 62.5f / transform.parent.transform.localScale.y;

        transform.localScale = new Vector3(xscale, yscale, 1);
        transform.localPosition = new Vector3(posX / transform.parent.transform.localScale.x, posY / transform.parent.transform.localScale.y, posZ / transform.parent.transform.localScale.z);

        if (time <= 0)
        {
            ShipStatsScript.shipStats.SetSpecialAmmo(-1);
            time = drainRate;
        }

        time -= Time.deltaTime;

        if (ShipStatsScript.shipStats.GetSpecialAmmo() < 1)
        {
            Die();
        }

    }




    public void OnTriggerStay2D(Collider2D coll)
    {



        if (coll.gameObject.tag == "Enemy")
        {
            if (coll.gameObject.GetComponent<BadguyStatsScript>() != null)
            {
                if (coll.gameObject.GetComponent<BossScript>() == null)
                {
                    BadguyStatsScript ship = coll.gameObject.GetComponent<BadguyStatsScript>();
                    ship.TakeHarm(Random.Range(MinDam, MaxDam + 1));
                }
                else
                {
                    BadguyStatsScript ship = coll.gameObject.GetComponent<BadguyStatsScript>();
                    ship.TakeHarm(1);
                    ship.GetComponent<BossScript>().SetfireBeam(true);
                    ship.GetComponent<BossScript>().SetSpawning(true);

                }
            }

            bool foundExplodeSound = false;
            foreach (PlayImpactScript child in GameObject.FindGameObjectWithTag("LaserHolder").GetComponentsInChildren<PlayImpactScript>(true))
            {
                if (!child.gameObject.activeSelf)
                {
                    foundExplodeSound = true;
                    child.gameObject.transform.position = transform.position;
                    child.gameObject.SetActive(true);
                    child.SetClip(ImpactSound);
                    child.PlaySound();

                    break;

                }
            }

            if (!foundExplodeSound)
            {
                PlayImpactScript impact = Instantiate(Impact, transform.position, Quaternion.identity);
                impact.SetClip(ImpactSound);
                impact.PlaySound();
            }




            bool foundExplode = false;
            foreach (ParticleDestoryScript child in ExplosionFinderScript.FindExplode.GetComponentsInChildren<ParticleDestoryScript>(true))
            {
                if (!child.gameObject.activeSelf)
                {
                    child.gameObject.transform.position = coll.gameObject.transform.position;
                    if (child.GetComponent<ParticleSystem>() != null)
                    {
                        child.gameObject.SetActive(true);
                        child.gameObject.GetComponent<ParticleSystem>().Play();
                        foundExplode = true;
                        break;
                    }
                }
            }
            if (!foundExplode)
            {
                Instantiate(impactExplosion, coll.gameObject.transform.position, Quaternion.identity);
            }
            
        }
        


    }

   
}
