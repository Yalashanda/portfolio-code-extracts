using System.Collections;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using UnityEngine;

public class TowerScript : MonoBehaviour/*, IPointerDownHandler, IPointerUpHandler*/
{
    public int Cost;
    public float FireRate;
    public float Range;
    public int Damage;
    public GameObject[] Projectiles;
    public GameStateScript.TowerType Type;
    public GameObject[] TowerVFX;
    public AudioClip[] EffectSounds;
    public AudioClip[] TowerSounds;
    public Sprite[] TowerImages;
    public GameObject InputOne = null;
    public GameObject InputTwo = null;
    public GameObject LineRendObj;
    public Material Red;
    public Material Brown;
    public GameObject CoinText;
    

    

    GameObject LineDrawer;
    int combineRange = 2;

    bool draggingConnector = false;
    bool connected = false;
    Vector3 fixedPoint = new Vector3(0,0,0);
    public bool isConnected = false;

    GameObject towerVFX;
    GameObject towerVFX2 = null;
    float time;
    float laserVanishTime = 1.0f;
    float laserVanishTimeCurr = 1.0f;
    bool needsCoolDown = false;

    float effectTime = 0.1f;
    public float effectTimeCurr;
    float soundClipLength;
    static int TotalSoundsPlaying;
    bool soundCountDown = false;

    int explosionRadius = 8*16;
    AudioSource mySource;
   // AnimationClip[] myClips;
    Animator myAnimator;
    void Start() {

        GameStateScript.GameState.Towers.Add(gameObject);
        mySource = GetComponent<AudioSource>();
        LineDrawer = Instantiate(LineRendObj, transform.position, Quaternion.identity);
        effectTimeCurr = effectTime;
        mySource.clip = TowerSounds[0];
        mySource.Play();
    }

    // Update is called once per frame
    void Update()
    {
        if (GameStateScript.GameState.GetPause())
        {
            return;
        }

        if (testIfAnyInRange())
        {
            EffectSoundTimer();
        }

        if (soundCountDown)
        {
            if (soundClipLength <= 0)
            {
                TotalSoundsPlaying--;
                soundCountDown = false;
            }

            soundClipLength -= Time.deltaTime * GameStateScript.GameState.GetFast();
        }




        if (!isConnected || testIfExemptType())
        {


            if (time <= 0)
            {
                if (testIfAnyInRange())
                {

                    Attack();
                }


                time = FireRate;
            }
            time -= Time.deltaTime * GameStateScript.GameState.GetFast();

            laserCount();
            if (Type != GameStateScript.TowerType.LASERTOWER)
            {
                drawConnector();
            }
        }
        else
        {
            laserCount();
            if (Type != GameStateScript.TowerType.LASERTOWER)
            {
                drawConnector();
            }

        }


    }

    

    public void Attack() {
        switch (Type)
        {
            case GameStateScript.TowerType.ALCHEMY:
                break;
            case GameStateScript.TowerType.BALLISTA:
                BallistaAttack();
                break;
            case GameStateScript.TowerType.CRYSTAL:
                crystalTowerAttack();
                break;
            case GameStateScript.TowerType.FIRE:
                fireTowerAttack();
                break;
            case GameStateScript.TowerType.FLAMINGBALLISTA:
                FireBallistaAttack();
                break;
            case GameStateScript.TowerType.LASERTOWER:
                
                if (!needsCoolDown)
                {

                    laserTowerAttack();
                    laserTowerDrawBeam(true);
                   
                    
                }
                break;
            case GameStateScript.TowerType.LIGHT:
                    lightTowerAttack();
                break;
            case GameStateScript.TowerType.STEAM:
                steamTowerAttack();
                break;
            case GameStateScript.TowerType.WATER:
                waterTowerAttack();
                break;
            default:
                break;
        }

    }

    //retruns if any towers are in range
    bool testIfAnyInRange() {
        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD ||
                enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
                {
                    return true;

                }
            }
        }
        
        return false;
    }

    //returns the closest enemy in range of the tower
    GameObject calculateTarget() {

        List<EnemyScript1> enemiesInRange = new List<EnemyScript1>();
        
        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD || 
                enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
                {
                    enemiesInRange.Add(enemy);
                }
            }
        }

        GameObject closestInRange = enemiesInRange[0].gameObject;
        float ClosestDistance = 50000;

        foreach (EnemyScript1 enemy in enemiesInRange)
        {
            if (Vector2.Distance(GameStateScript.GameState.City.transform.position, enemy.gameObject.transform.position) < ClosestDistance)
            {
                closestInRange = enemy.gameObject;
                ClosestDistance = Vector2.Distance(GameStateScript.GameState.City.transform.position, enemy.gameObject.transform.position);
            }

        }
        return closestInRange;

    }

    public void OnSpawn() {
        myAnimator = GetComponent<Animator>();

        switch (Type)
        {//Costs need to be changed in game state and in tower script
            //For animator state changes 0 crystal, 1 fire, 2 water, 3 laser, 4 steam, 5 ballista, 6 alchemy, 7 light
            case GameStateScript.TowerType.ALCHEMY:
                Damage = 0;
                Cost = 200;
                Range = 0;
                FireRate = 0;
                setImage(0);
                myAnimator.SetInteger("AnimationValue", 6);
                break;
            case GameStateScript.TowerType.BALLISTA:
                Damage = 10;
                Cost =  200;
                Range = 4;
                FireRate = 1;
                towerVFX = SpawnEffect(5);
                setImage(1);
                myAnimator.enabled = false;
                myAnimator.SetInteger("AnimationValue", 5);
                break;
            case GameStateScript.TowerType.CRYSTAL:
                Damage = 0;
                Cost = 750;
                Range = 4.0f;
                FireRate = 0.01f;
                towerVFX = SpawnEffect(2);
                setImage(3);
                myAnimator.SetInteger("AnimationValue", 0);
                break;
            case GameStateScript.TowerType.FIRE:
                Damage = 0;
                Cost = 300;
                Range = 1.5f;
                FireRate = 0.25f;
                towerVFX = SpawnEffect(0);
                setImage(4);
                myAnimator.SetInteger("AnimationValue", 1);
                break;
            case GameStateScript.TowerType.FLAMINGBALLISTA:
                Cost = 0;
                Damage = 10;
                Range = 5.0f;
                FireRate = 1;
                towerVFX = SpawnEffect(5);
                setImage(5);
                myAnimator.enabled = false;
                break;
            case GameStateScript.TowerType.LASERTOWER:
                Cost = 0;
                Damage = 20;
                Range = 10;
                FireRate = 0.05f;
                setImage(6);
                myAnimator.SetInteger("AnimationValue", 3);
                break;
            case GameStateScript.TowerType.LIGHT:
                Damage = 0;
                Cost = 500;
                Range = 3;
                FireRate = 0.01f;
                towerVFX = SpawnEffect(4);
                towerVFX2 = SpawnEffect(6);
                towerVFX2.transform.position = new Vector3(towerVFX2.transform.position.x, towerVFX2.transform.position.y, -6);
                setImage(7);
                myAnimator.SetInteger("AnimationValue", 7);
                break;
            case GameStateScript.TowerType.STEAM:
                Cost = 0;
                Damage = 0;
                Range = 3;
                FireRate = 1;
                towerVFX = SpawnEffect(1);
                setImage(8);
                myAnimator.SetInteger("AnimationValue", 4);
                break;
            case GameStateScript.TowerType.WATER:
                Damage = 5;
                Cost = 300;
                Range = 2.5f;
                FireRate = 0.75f;
                setImage(9);
                myAnimator.SetInteger("AnimationValue", 2);
                break;
            default:
                Type = GameStateScript.TowerType.BALLISTA;
                Damage = 10;
                Cost = 200;
                Range = 4;
                FireRate = 1;
                setImage(1);
                myAnimator.enabled = false;
                myAnimator.SetInteger("AnimationValue", 5);
                break;
        }
        time = FireRate;
        Range *= 16;
        
    }

    GameObject SpawnEffect(int val)
    {

        return Instantiate(TowerVFX[val], new Vector3(transform.position.x, transform.position.y, -5), Quaternion.identity);
    }

    void setImage(int val) {
        GetComponent<SpriteRenderer>().sprite = TowerImages[val];
    }





    void BallistaAttack() {
        bool foundShot = false;
        foreach (BallistaScript ballista in GameStateScript.GameState.Projectiles)
        {
            if (!ballista.gameObject.activeSelf)
            {
                foundShot = true;
                ballista.gameObject.transform.position = transform.position;
                ballista.OnSpawn(calculateTarget(), false);
                break;
            }
        }

        if (!foundShot)
        {
            GameObject shot = Instantiate(Projectiles[0], transform.position, Quaternion.identity);
            shot.GetComponent<BallistaScript>().OnSpawn(calculateTarget(), false);
        }
    }

    void FireBallistaAttack() {

        
            bool foundShot = false;
            foreach (BallistaScript shot in GameStateScript.GameState.Projectiles)
            {
                if (!shot.gameObject.activeSelf)
                {
                    foundShot = true;
                    shot.gameObject.transform.position = transform.position;
                    shot.OnSpawn(calculateTarget(), true);
                    break;
                }
            }

            if (!foundShot)
            {
                GameObject shot = Instantiate(Projectiles[0], transform.position, Quaternion.identity);
                shot.GetComponent<BallistaScript>().OnSpawn(calculateTarget(), true);
            }
        

        }

    void fireTowerAttack() {

        
        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
            {

                enemy.SetFire(true);

            }
        }
    }

    void crystalTowerAttack()
    {
        
        
        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD ||
                enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
                {
                    enemy.SetCrystal(true);
                }
                else
                {
                    if (enemy.GetCrystalState())
                    {
                        enemy.SetCrystal(false);
                    }

                }
            }
        }

    }

    void lightTowerAttack() {

        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD ||
                enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
                {
                    enemy.SetLit(true);
                }
                else
                {
                    if (enemy.GetLit())
                    {
                        enemy.SetLit(false);
                    }

                }
            }
        }


    }
    
    void steamTowerAttack()
    {
        foreach (EnemyScript1 enemy in GameStateScript.GameState.Enemies)
        {
            if (enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD ||
                enemy.gameObject.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                if (Vector2.Distance(gameObject.transform.position, enemy.gameObject.transform.position) < Range)
                {
                    enemy.SetSlowed(true);
                }
                else
                {
                    if (enemy.GetSlowed())
                    {
                        enemy.SetSlowed(false);
                    }

                }
            }
        }
    }

    void waterTowerAttack() {
        bool foundShot = false;
        foreach (WaterBlastScript shot in GameStateScript.GameState.WaterBlasts)
        {
            if (!shot.gameObject.activeSelf)
            {
                foundShot = true;
                shot.gameObject.transform.position = transform.position;
                shot.OnSpawn(calculateTarget());
                break;
            }
        }

        if (!foundShot)
        {
            GameObject shot = Instantiate(Projectiles[1], transform.position, Quaternion.identity);
            shot.GetComponent<WaterBlastScript>().OnSpawn(calculateTarget());
        }
    }

    void laserTowerAttack() {
        calculateTarget().GetComponent<EnemyScript1>().TakeHarm(1);    
    }
    void laserTowerDrawBeam(bool val) {
        if (LineDrawer.GetComponent<LineRenderer>() != null)
        {



            Vector3[] line = new Vector3[2];
            line[0] = transform.position;
            if (val && testIfAnyInRange())
            {
                line[1] = calculateTarget().transform.position;
                //line[1] = new Vector3(transform.position.x, transform.position.y + 350, transform.position.z);


            }
            else
            {
                line[1] = transform.position;

            }
            LineDrawer.GetComponent<LineRenderer>().material = Red;
            
            LineDrawer.GetComponent<LineRenderer>().startColor = Color.cyan;
            LineDrawer.GetComponent<LineRenderer>().endColor = Color.blue;
            LineDrawer.GetComponent<LineRenderer>().SetPositions(line);

        }
    }
    void laserCount() {

        if (laserVanishTimeCurr < 0)
        {
            if (Type == GameStateScript.TowerType.LASERTOWER)
            {
                if (!testIfAnyInRange() && !needsCoolDown)
                {
                    needsCoolDown = false;
                }
                else
                {
                    needsCoolDown = !needsCoolDown;
                }

                
                laserTowerDrawBeam(false);
                

            }
            laserVanishTimeCurr = laserVanishTime;
        }
        laserVanishTimeCurr -= Time.deltaTime * GameStateScript.GameState.GetFast();
    }

    

    public void Die(bool val) {


        

        if (val)
        {
            
            GameStateScript.GameState.AddLoot((int)Mathf.Round(Cost * 0.75f));
            SpawnCoinIndic((int)Mathf.Round(Cost * 0.75f));
        }


        for (int i = 0; i < GameStateScript.GameState.Towers.Count; i++)
        {
            
            if (GameStateScript.GameState.Towers[i].transform.position == gameObject.transform.position)
            {
                GameStateScript.GameState.Towers.RemoveAt(i);
                break;
            }
        }
        if (towerVFX2 != null)
        {
            Destroy(towerVFX2);
        }

       


        Destroy(towerVFX);
        Destroy(LineDrawer);
        Destroy(gameObject);

    }

    public void Explode() {


        ExplodeRadiusDeath(InputOne);
        ExplodeRadiusDeath(InputTwo);
        ExplodeRadiusDeath(gameObject);
        InputOne.GetComponent<TowerScript>().Die(true);
        InputTwo.GetComponent<TowerScript>().Die(true);
        Die(true);
    }




    void SpawnCoinIndic(int val)
    {
        bool foundCoin = false;
        foreach (CoinRiseScript cn in GameStateScript.GameState.CoinNums)
        {
            if (!cn.gameObject.activeSelf)
            {
                foundCoin = true;
                cn.transform.position = transform.position;
                cn.GetComponent<CoinRiseScript>().OnSpawn(val);
                break;
            }
        }

        if (!foundCoin)
        {
            GameObject cn = Instantiate(CoinText, transform.position, Quaternion.identity);
            cn.GetComponent<CoinRiseScript>().OnSpawn(val);
        }
    }

    /*
    void Combine()
    {





        bool valid = false;
        GameStateScript.TowerType type1 = InputOne.GetComponent<TowerScript>().Type;
        GameStateScript.TowerType type2 = InputTwo.GetComponent<TowerScript>().Type;

        switch (type1) {

            case GameStateScript.TowerType.LIGHT:
                if (type2 == GameStateScript.TowerType.CRYSTAL)
                {
                    Type = GameStateScript.TowerType.LASERTOWER;
                    valid = true;
                }
               break;

            case GameStateScript.TowerType.CRYSTAL:
                if (type2 == GameStateScript.TowerType.LIGHT)
                {
                    Type = GameStateScript.TowerType.LASERTOWER;
                    valid = true;
                }
                break;

           case GameStateScript.TowerType.FIRE:
                if (type2 == GameStateScript.TowerType.BALLISTA)
                {
                    Type = GameStateScript.TowerType.FLAMINGBALLISTA;
                    valid = true;
                }
                if (type2 == GameStateScript.TowerType.WATER)
                {
                    Type = GameStateScript.TowerType.STEAM;
                    valid = true;
                }
                break;

            case GameStateScript.TowerType.BALLISTA:
                if (type2 == GameStateScript.TowerType.FIRE)
                {
                    Type = GameStateScript.TowerType.FLAMINGBALLISTA;
                    valid = true;
                }
            break;

            case GameStateScript.TowerType.WATER:
                if (type2 == GameStateScript.TowerType.FIRE)
                {
                    Type = GameStateScript.TowerType.STEAM;
                    valid = true;
                }
                break;

            default:
                break;
        }
        if (!valid)
        {
            Explode();
        }
        else
        {
            OnSpawn();
        }
        
    }

    public void SetInput(GameObject val){

        if (InputOne == null)
        {
            InputOne = val;
        }
        else
        {
            if (InputOne != val)
            {
                if (InputTwo == null)
                {
                    InputTwo = val;
                    Combine();
                }
                else
                {
                    Explode();
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData) {
        if (Type != GameStateScript.TowerType.ALCHEMY)
        {
            GameStateScript.GameState.SetHeldTower(gameObject);
            draggingConnector = true;
        }


        

    }

    public void OnPointerUp(PointerEventData eventData)
    {
        TowerScript tower = null;
        float maxdis = 16;
        
        foreach (GameObject tow in GameStateScript.GameState.Towers)
        {
            if (Vector2.Distance(tow.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition)) < maxdis && 
                tow.GetComponent<TowerScript>().Type == GameStateScript.TowerType.ALCHEMY)
            {
                if (Vector2.Distance(tow.transform.position, gameObject.transform.position) < 32*combineRange)
                {
                    maxdis = Vector2.Distance(tow.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    tower = tow.GetComponent<TowerScript>();
                }
            }

            
        }



        if (tower != null)
        {
            tower.SetInput(GameStateScript.GameState.GetHeldTower());
            
            isConnected = true;
            connected = true;
            fixedPoint = tower.gameObject.transform.position;
            Destroy(towerVFX);
            
        }
        
        
            draggingConnector = false;
        


        
    }
    */


    void drawConnector() {


        LineDrawer.GetComponent<LineRenderer>().material = Brown;
        if (draggingConnector || connected)
        {
            if (!connected)
            {

                Vector3[] line = new Vector3[2];
                line[0] = transform.position;
                line[1] = Camera.main.ScreenToWorldPoint(Input.mousePosition);
                LineDrawer.GetComponent<LineRenderer>().SetPositions(line);
            }
            else
            {
                Vector3[] line = new Vector3[2];
                line[0] = transform.position;
                line[1] = fixedPoint;
                LineDrawer.GetComponent<LineRenderer>().SetPositions(line);
            }
        }
        else
        {
            Vector3[] line = new Vector3[2];
            line[0] = transform.position;
            line[1] = transform.position;
            LineDrawer.GetComponent<LineRenderer>().SetPositions(line);

        }
    


    }

    public Vector3 GetFixedPoint() {

        return fixedPoint;
    }
    public void SetConnectedLoadData(Vector3 val) {
        connected = true;
        isConnected = true;
        Destroy(towerVFX);
        fixedPoint = val;
    }


    bool testIfExemptType() {
        if (Type == GameStateScript.TowerType.ALCHEMY 
            || Type == GameStateScript.TowerType.LASERTOWER 
            || Type == GameStateScript.TowerType.FLAMINGBALLISTA
            || Type == GameStateScript.TowerType.STEAM)
        {
            return true;
        }
        else
        {
            return false;
        }
                
        
    }

    void ExplodeRadiusDeath(GameObject center) {
        bool found = false;
        foreach (ExplodeScript ex in GameStateScript.GameState.Explosions)
        {
            if (!ex.gameObject.activeSelf)
            {
                ex.gameObject.transform.position = center.transform.position;
                ex.gameObject.SetActive(true);
                ex.OnSpawn();
                found = true;
                break;
            }
        }


        if (!found)
        {
            Instantiate(TowerVFX[3], center.transform.position, Quaternion.identity);

        }

        foreach (EnemyScript1 e in GameStateScript.GameState.Enemies)
        {
            if (Vector2.Distance(e.gameObject.transform.position, center.transform.position) < explosionRadius && 
                e.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.DEAD &&
                e.GetComponent<FollowThePathScript>().CurrentState != FollowThePathScript.EnemyStates.FALLING)
            {
                e.Die(true);
            }
        }
    }

    void EffectSoundTimer() {
        if (effectTimeCurr <= 0.1 && TotalSoundsPlaying < 3 && !connected)
        {
            if (!GetComponent<AudioSource>().isPlaying)
            {
                switch (Type)
                {
                    case GameStateScript.TowerType.NULL:
                        break;
                    case GameStateScript.TowerType.ALCHEMY:
                        break;
                    case GameStateScript.TowerType.BALLISTA:
                        break;
                    case GameStateScript.TowerType.CRYSTAL:
                        setSound(2);                        
                        break;
                    case GameStateScript.TowerType.FIRE:
                        setSound(0);
                        break;
                    case GameStateScript.TowerType.FLAMINGBALLISTA:
                        break;
                    case GameStateScript.TowerType.LASERTOWER:
                        break;
                    case GameStateScript.TowerType.LIGHT:
                        setSound(4);
                        break;
                    case GameStateScript.TowerType.STEAM:
                        setSound(1);
                        break;
                    case GameStateScript.TowerType.WATER:
                        break;
                    default:
                        break;
                }
            }
            

            effectTimeCurr = effectTime;
        }

        effectTimeCurr -= Time.deltaTime * GameStateScript.GameState.GetFast();
    }

    void setSound(int val) {
        mySource.clip = EffectSounds[val];
        TotalSoundsPlaying++;
        soundCountDown = true;
        soundClipLength = GetComponent<AudioSource>().clip.length;
        GetComponent<AudioSource>().Play();
    }









    public void GetInputs(out GameObject i1, out GameObject i2) {
        i1 = InputOne;
        i2 = InputTwo;

    }

    public void SetTowerType(GameStateScript.TowerType newType) {
        Type = newType;
    }
    public GameStateScript.TowerType GetTowerType() {
        return Type;
    }
    public bool GetDraggingConnector() {
        return draggingConnector;
    }
    public void SetDraggingConnector(bool val)
    {
        draggingConnector = val;
    }

    public void OnConnecet(TowerScript tower) {
        tower.GetComponent<CombineTowersScripts>().SetInput(GameStateScript.GameState.GetHeldTower());
        isConnected = true;
        connected = true;
        fixedPoint = tower.gameObject.transform.position;
        Destroy(towerVFX);
    }

    public int GetCombineRange() {
        return combineRange;
    }

}
