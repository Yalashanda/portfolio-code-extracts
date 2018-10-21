using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlackHoleScript : FireWeaponForwardScript
{
    Collider2D shipCollider;
    GameObject ship;
    public bool stop;





    // Use this for initialization
    void Start()
    {
        stop = false;
        ship = GameObject.FindGameObjectWithTag("Ship");
        mySource = GetComponent<AudioSource>();
        time = lifeTime;
        myCollider = GetComponent<CircleCollider2D>();
        mySource.clip = FireSound;
        mySource.Play();
    }


    override public void Update()
    {
        if (!stop)
        {
            transform.Translate(0, WeaponSpeed * Time.deltaTime, 0);
        }
        else {
            foreach (BadguyStatsScript child in FindBadguyScript.FindBadguy.gameObject.GetComponentsInChildren<BadguyStatsScript>())
            {
                if (Vector3.Distance(child.gameObject.transform.position, transform.position) < 4)
                {
                    child.gameObject.transform.position = Vector3.MoveTowards(child.gameObject.transform.position, transform.position, 4 * Time.deltaTime);
                }

            }


            foreach (EnemyFire child in GameObject.FindGameObjectWithTag("LaserHolder").GetComponentsInChildren<EnemyFire>())
            {

                if (child.gameObject.activeSelf && child.TargetType == EnemyFire.TargetingType.HOMING)
                {
                    if (Vector3.Distance(child.gameObject.transform.position, transform.position) < 4)
                    {
                        child.gameObject.transform.position = Vector3.MoveTowards(child.gameObject.transform.position, transform.position, 4 * Time.deltaTime);
                    }
                }

            }
        }
        
        if (time <= 0)
        {
            Die();
        }
        time -= Time.deltaTime;


        

    }


    void LateUpdate()
    {
        if (Vector3.Distance(transform.position, ship.transform.position) > 4.0f && !stop)
        {
            stop = true;

        }

    }

    override public void OnTriggerEnter2D(Collider2D coll)
    {
    }


    public void OnTriggerStay2D(Collider2D coll)
    {

        if (coll.gameObject.tag == "Ship" && stop == true)
        {

            ShipStatsScript ship = coll.gameObject.GetComponent<ShipStatsScript>();
            if (!ship.IsPhasing)
            {
                ship.TakeHarm(Random.Range(MinDam * 5, MaxDam * 5 + 1));
                
            }
        }
        else if (coll.gameObject.tag == "Enemy")
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

                }
            }

            bool foundExplodeSound = false;
            foreach (PlayImpactScript child in ExplosionFinderScript.FindExplode.GetComponentsInChildren<PlayImpactScript>(true))
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

        }




    }

    override public void Die()
    {
        stop = false;

        time = lifeTime;
        
        gameObject.SetActive(false);

    }



}
