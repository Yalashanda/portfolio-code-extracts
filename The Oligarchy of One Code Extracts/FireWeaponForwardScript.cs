using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireWeaponForwardScript : MonoBehaviour {
    public int WeaponSpeed;
    public float lifeTime;
    protected float time;
    protected Collider2D myCollider;
    public int MinDam;
    public int MaxDam;
    public AudioClip FireSound;
    public AudioClip ImpactSound;
    public PlayImpactScript Impact;
    public GameObject impactExplosion;
    protected AudioSource mySource;
    // Use this for initialization
    void Start () {
        mySource = GetComponent<AudioSource>();
        time = 0;
        time = lifeTime;
        myCollider = GetComponent<BoxCollider2D>();
        mySource.clip = FireSound;
        mySource.Play();
        
		
	}
 
	// Update is called once per frame
	virtual public void Update () {
       

        transform.Translate(0, WeaponSpeed * Time.deltaTime, 0);
        if (time <= 0)
        {
     
            Die();
        }
        time -= Time.deltaTime;
    }


   
    virtual public void OnTriggerEnter2D(Collider2D coll)
    {
        

        
        if (coll.gameObject.tag == "Enemy")
        {
            if (coll.gameObject.GetComponent<BadguyStatsScript>() != null)
            {
                BadguyStatsScript ship = coll.gameObject.GetComponent<BadguyStatsScript>();
                ship.TakeHarm(Random.Range(MinDam, MaxDam + 1));
            }



            bool foundExplodeSound = false;
            foreach (PlayImpactScript child in ExplosionFinderScript.FindExplode.GetComponentsInChildren<PlayImpactScript>(true))
            {
                if (!child.gameObject.activeSelf)
                {       foundExplodeSound = true;
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
                    child.gameObject.transform.position = transform.position;
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
                Instantiate(impactExplosion, transform.position, Quaternion.identity);
            }

            Die();
        }
        


    }

    virtual public void Die() {
        gameObject.SetActive(false);
        time = lifeTime;
    }


}
