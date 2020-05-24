using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;


public class GameStateScript : MonoBehaviour {

    public string LevelToLoad;
    public enum EnemyType
    {
        WAVEBREAK,
        GOBLIN,
        SHADOWGOBLIN,
        ORC,
        LIZARDMAN,
        SALAMANDER,
        GIANT,
        HYDRA,
        DRAGON


    };

    public enum TowerType
    {
        NULL,
        ALCHEMY,
        BALLISTA,
        CRYSTAL,
        FIRE,
        FLAMINGBALLISTA,
        LASERTOWER,
        LIGHT,
        STEAM,
        WATER

    };


    public static GameStateScript GameState;
    public List<FindNextTileScript> PathTiles;
    public List<EnemyScript1> Enemies;
    public List<TerrainScript> TerrainElements;
    public List<BallistaScript> Projectiles;
    public List<WaterBlastScript> WaterBlasts;
    public List<ExplodeScript> Explosions;
    public List<CoinRiseScript> CoinNums;
    public List<ParticleKillScript> BloodSpurts;
    public List<PlaySoundEffectsScripts> SoundPlayers;
    public List<PlaySoundEffectsScripts> PainSoundPlayers;
    public List<GameObject> Towers;
    public GameObject TowerToSpawn;
    public GameObject City;
    public GameObject DraggedTowerCursor;


    public Sprite[] DraggedTowerCursorImages;

    //the images of the terrain types, should corraspon numerically with the enum in terrain scripts, i.e. if TREE is at 0 in enum then tree sprite should be at 0 here
    public Sprite[] TerrainSprites;

    //text referances for displays
    public Text Resources;
    public Text LivesLeft;
    public Text WaveNum;
    public Text ToolTip;


    public GameObject ScrollView;
    public GameObject InterLevelBank;
    public Canvas canvas;
    public Button nextLevelButton;
    public GameObject BloodSpurt;
    public GameObject Speaker;
    //The object that hold the audio Sliders
    public GameObject Sliders;


    public GameObject heldTower;


    Vector3 snapTo;
    int Loot = 0;
    int Lives = 50;
    TowerType SelectedType = TowerType.FIRE;
    //bool that controls if the selected tower is drawn at the pointer position
    bool DrawTowerOnCursor = false;
    //if the spot where the pointer is can have a tower placed there
    bool canPlace = false;
    //if game is paused or not
    bool pause = false;
    //if game is fastforwarding
    float  fastForward = 1.0f;
    //if inifiite mode is active on the current map
    public bool InfiniteMode = false;
    //if the pointer is in a region that does not allow tower placement
    public bool noPlaceZone = false;
    //the cursor image, up and down
    

    AudioSource mySource;
    void Awake() {
        GameState = this;
        mySource = GetComponent<AudioSource>();
        Instantiate(InterLevelBank, transform.position, Quaternion.identity);
    }


    // Use this for initialization
    void Start() {
        Debug.Log(SceneManager.GetActiveScene().name + " loaded");

        if (LoadOnStartScript.canload != null)
        {
            InfiniteMode = LoadOnStartScript.canload.InfiniteMode;
        }


        if (LoadOnStartScript.canload != null)
        {

            if (LoadOnStartScript.canload.GetCanLoad())
            {
                Load();
               
            }
        }
        


        if (LoadOnStartScript.canload != null)
        {
            LoadOnStartScript.canload.SetCanLoad(false);
        }
        for (int i = 0; i < PlaySoundEffectsScripts.MaxSoundsPlaying; i++)
        {


            GameObject spek = Instantiate(Speaker, transform.position, Quaternion.identity);
            spek.GetComponent<PlaySoundEffectsScripts>().OnSpawn(false);

        }

        for (int i = 0; i < 3; i++)
        {


            GameObject spek = Instantiate(Speaker, transform.position, Quaternion.identity);
            spek.GetComponent<PlaySoundEffectsScripts>().OnSpawn(true);


        }



        
        if (SaveCoinValuesBetweenLevelsScript.coinStore != null)
        {

            Lives += SaveCoinValuesBetweenLevelsScript.coinStore.GetLifeReserve();
            AddLoot(SaveCoinValuesBetweenLevelsScript.coinStore.GetCoinReserve() + 500 + Lives * 3);
        }
        else
        {

            AddLoot(500 + Lives * 3);
        }






        LivesLeft.text = Lives.ToString();
        //LivesLeft.text = "Lives: " + Lives.ToString();


        GameObject scroll = Instantiate(ScrollView, new Vector3(0, 0, 0), Quaternion.identity);
        scroll.transform.localScale = new Vector3((1.0f / 1024) * Screen.width, (1.0f / 768) * Screen.height, 1);
        scroll.transform.SetParent(canvas.transform, false);






        

    }

    // Update is called once per frame
    void Update() {

        


        if (Input.GetKeyUp(KeyCode.P) || Input.GetKeyUp(KeyCode.Space))
        {
            SetPause(!pause);
        }


        if (Input.GetKeyUp(KeyCode.Escape))
        {
            Application.Quit();
        }

        snapTo = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (DrawTowerOnCursor)
        {
            DraggedTowerCursor.transform.position = new Vector3((int)Mathf.Round(snapTo.x / 16) * 16, (int)Mathf.Round(snapTo.y / 16) * 16, -5);
        }


        if (Input.GetMouseButtonUp(0))
        {
            if (canPlace == true && !noPlaceZone &&
            Camera.main.ScreenToViewportPoint(Input.mousePosition).x > 0 &&
            Camera.main.ScreenToViewportPoint(Input.mousePosition).x < 1
            )
            {
                canPlace = false;
                DrawTowerOnCursor = false;
                DraggedTowerCursor.SetActive(false);
                //Vector3 snapTo = Camera.main.ScreenToViewportPoint(Input.mousePosition);

                int GridNumX = (int)Mathf.Round(snapTo.x / 16) * 16;
                int GridNumY = (int)Mathf.Round(snapTo.y / 16) * 16;
                snapTo = new Vector3(GridNumX, GridNumY, -5);

                if (TestIfEmpty() && Loot >= GetTowerCost(SelectedType))
                {
                    GameObject tower = Instantiate(TowerToSpawn, snapTo, Quaternion.identity);
                    tower.GetComponent<TowerScript>().Type = SelectedType;
                    tower.GetComponent<TowerScript>().OnSpawn();
                    
                    SpendLoot(GetTowerCost(SelectedType));
                    mySource.Play();//plays the sound of the tower being placed
                }

            }

            else
            {
                canPlace = false;
                DrawTowerOnCursor = false;
                DraggedTowerCursor.SetActive(false);
            }
        }



    }



    public void AddLoot(int gain) {
        Loot += gain;
        Resources.text = " " + Loot.ToString();
    }
    public void SpendLoot(int cost) {
        Loot -= cost;
        if (Loot <= 0)
        {
            Loot = 0;
        }
        Resources.text = " " + Loot.ToString();
    }
    public int GetTowerCost(TowerType Type)
    {//Costs need to be changed in game state and in tower script
        int Cost = 0;
        switch (Type)
        {
            case TowerType.ALCHEMY:
                Cost = 200;
                break;
            case TowerType.BALLISTA:
                Cost = 200;
                break;
            case TowerType.CRYSTAL:
                Cost = 750;
                break;
            case TowerType.FIRE:
                Cost = 300;
                break;
            case TowerType.FLAMINGBALLISTA:
                Cost = 0;
                break;
            case TowerType.LASERTOWER:
                Cost = 0;
                break;
            case TowerType.LIGHT:
                Cost = 500;
                break;
            case TowerType.STEAM:
                Cost = 0;
                break;
            case TowerType.WATER:
                Cost = 300;
                break;
            default:
                Cost = 200;
                break;
        }

        return Cost;

    }
    public void SetHeldTower(GameObject val) {
        heldTower = val;
    }
    public GameObject GetHeldTower() {
        return heldTower;
    }


    bool TestIfEmpty() {

        foreach (GameObject tower in Towers)
        {
            if (snapTo == tower.transform.position)
            {
                return false;
            }
        }

        foreach (TerrainScript tile in TerrainElements)
        {
            //if (snapTo == tile.gameObject.transform.position)
            if ((snapTo.x < tile.gameObject.transform.position.x + tile.gameObject.GetComponent<BoxCollider2D>().size.x / 2 &&
                snapTo.x > tile.gameObject.transform.position.x - tile.gameObject.GetComponent<BoxCollider2D>().size.x / 2 &&
                snapTo.y < tile.gameObject.transform.position.y + tile.gameObject.GetComponent<BoxCollider2D>().size.y / 2 &&
                snapTo.y > tile.gameObject.transform.position.y - tile.gameObject.GetComponent<BoxCollider2D>().size.y / 2)
                || snapTo == tile.gameObject.transform.position
                )
            {
                return false;
            }
        }

        foreach (FindNextTileScript tile in PathTiles)
        {
            if ((snapTo.x < tile.gameObject.transform.position.x + tile.gameObject.GetComponent<BoxCollider2D>().size.x / 2 &&
                snapTo.x > tile.gameObject.transform.position.x - tile.gameObject.GetComponent<BoxCollider2D>().size.x / 2 &&
                snapTo.y < tile.gameObject.transform.position.y + tile.gameObject.GetComponent<BoxCollider2D>().size.y / 2 &&
                snapTo.y > tile.gameObject.transform.position.y - tile.gameObject.GetComponent<BoxCollider2D>().size.y / 2)
                || snapTo == tile.gameObject.transform.position
                )
            {
                return false;
            }
        }

        return true;
    }

    public void LoseLife() {
        Lives--;

        if (Lives <= 0)
        {
            Lives = 0;
            EndGame();

        }
        LivesLeft.text = Lives.ToString();


    }

    public int GetLives()
    {
        return Lives;
    }
    public void Die() {
        int indexToRemove = 0;
        for (int i = 0; i < Towers.Count; i++)
        {
            if (Towers[i].transform.position == gameObject.transform.position)
            {
                indexToRemove = i;
                break;
            }
        }
        Towers.RemoveAt(indexToRemove);
    }

    public void SetSelectedType(GameStateScript.TowerType input) {
        SelectedType = input;
    }

    public void SetTowerDragImage(){

        canPlace = true;
        DrawTowerOnCursor = true;
        DraggedTowerCursor.SetActive(true);
        
        switch (SelectedType)
        {
            case TowerType.ALCHEMY:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[0];
                break;
            case TowerType.BALLISTA:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[1];
                break;
            case TowerType.CRYSTAL:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[2];
                break;
            case TowerType.FIRE:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[3];
                break;
            case TowerType.FLAMINGBALLISTA:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[4];
                break;
            case TowerType.LASERTOWER:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[5];
                break;
            case TowerType.LIGHT:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[6];
                break;
            case TowerType.STEAM:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[7];
                break;
            case TowerType.WATER:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[8];
                break;
            default:
                DraggedTowerCursor.GetComponent<SpriteRenderer>().sprite = DraggedTowerCursorImages[0];
                break;
        }




        DraggedTowerCursor.GetComponentInChildren<DrawTowerRadiusScript>().radius = GetRange(SelectedType)*16;

    }

    float GetRange(GameStateScript.TowerType val)
    {


        switch (val)
        {

            case GameStateScript.TowerType.ALCHEMY:
                return 0;

            case GameStateScript.TowerType.BALLISTA:
                return 4;

            case GameStateScript.TowerType.CRYSTAL:
                return 4.0f;

            case GameStateScript.TowerType.FIRE:
                return 1.5f;

            case GameStateScript.TowerType.FLAMINGBALLISTA:
                return 4;


            case GameStateScript.TowerType.LASERTOWER:
                return 10;

            case GameStateScript.TowerType.LIGHT:
                return 3;

            case GameStateScript.TowerType.STEAM:
                return 3;

            case GameStateScript.TowerType.WATER:
                return 2;

            default:
                return 3;


        }




    }


    public void SetNoPlaceZone(bool val) {
        noPlaceZone = val;
    }
    public int GetLoot() {
        return Loot;
    }

    public void LoadNextLevel(string val) {

        SaveCoinValuesBetweenLevelsScript.coinStore.SetCoinReserve(Loot);
        SaveCoinValuesBetweenLevelsScript.coinStore.SetLifeReserve(Lives);
        //Save(LevelToLoad, Lives, GetLoot());
        SceneManager.LoadScene(val);
    }
    public void playGivenSound(AudioClip val)
    {
        foreach (PlaySoundEffectsScripts spek in PainSoundPlayers)
        {
            if (!spek.IsPlaying())
            {
                spek.PlaySound(val);
                break;
            }
        }
    }

    public void playGivenSound(AudioClip val, float vol)
    {
        foreach (PlaySoundEffectsScripts spek in PainSoundPlayers)
        {
            if (!spek.IsPlaying())
            {
                spek.PlaySound(val, vol);
                break;
            }
        }
    }



    public void pauseButton() {
        SetPause(!pause);

        if (Sliders.gameObject.activeSelf)
        {
            Sliders.gameObject.SetActive(false);
        }
        else {
            Sliders.gameObject.SetActive(true);
        }
    }

    public void SetPause(bool val) {
        pause = val;
    }
    public bool GetPause() {
        return pause;
    }


    public void SetFast(float val)
    {
        fastForward = val;
    }
    public float GetFast()
    {
        return fastForward;
    }

  

    public void EndGame()
    {
        SaveCoinValuesBetweenLevelsScript.coinStore.SetCoinReserve(0);
        SaveCoinValuesBetweenLevelsScript.coinStore.SetLifeReserve(0);
        Save(SceneManager.GetActiveScene().name, GameState.GetLives(), GameState.GetLoot());
        SceneManager.LoadScene("MenuScene");
    }

    public void CloseGame() {
        Application.Quit();
    }














    public static string GetLeveltoLoad()
    {

        if (!LoadOnStartScript.canload.InfiniteMode)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/PlayerSaveData.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            return data.LevelName;
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(Application.persistentDataPath + "/PlayerSaveDataInfi.dat", FileMode.Open);
            PlayerData data = (PlayerData)bf.Deserialize(file);
            file.Close();

            return data.LevelName;
        }
    }

    public void Save(string levelVal, int lives, int loot) {
        if (!InfiniteMode)
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/PlayerSaveData.dat");

    
            bf.Serialize(file, CreateData(levelVal, lives, loot));
            file.Close();
            Debug.Log("Game Saved Normal");
        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(Application.persistentDataPath + "/PlayerSaveDataInfi.dat");


            bf.Serialize(file, CreateData(levelVal, lives, loot));
            file.Close();
            Debug.Log("Game Saved Infini");
        }
        

    }

    public void Load() {
        PlayerData data = new PlayerData(); ;
        if (!InfiniteMode)
        {
            
            if (File.Exists(Application.persistentDataPath + "/PlayerSaveData.dat"))
            {
                Debug.Log("Normal data loaded");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/PlayerSaveData.dat", FileMode.Open);
                data = (PlayerData)bf.Deserialize(file);
                file.Close();
            }
        }
        else
        {
            if (File.Exists(Application.persistentDataPath + "/PlayerSaveDataInfi.dat"))
            {
                Debug.Log("Infi data loaded");
                BinaryFormatter bf = new BinaryFormatter();
                FileStream file = File.Open(Application.persistentDataPath + "/PlayerSaveDataInfi.dat", FileMode.Open);
                data = (PlayerData)bf.Deserialize(file);
                file.Close();
           
            }
        }

        
        
        
        
         

        Loot = data.LootData;
        Lives = data.LivesData;

        EnemySpawnScript.spawner.DisplayedWave = data.waveNum;
        EnemySpawnScript.spawner.currentWave = data.waveCurr;
        EnemySpawnScript.spawner.currentCount = data.waveCurrCount;
        EnemySpawnScript.spawner.SetTime(data.waveTime);
        EnemySpawnScript.spawner.SetCurrLength(data.waveLengthCurr);


        for (int i = 0; i < data.TowerCount; i++)
        {
            GameObject tower = Instantiate(TowerToSpawn, new Vector3(data.TowerPositions[i].x, data.TowerPositions[i].y, data.TowerPositions[i].z), Quaternion.identity);
            tower.GetComponent<TowerScript>().Type = (TowerType)data.TowerTypes[i];
            tower.GetComponent<TowerScript>().OnSpawn();

            if (data.TowerIsConnected[i])
            {
                tower.GetComponent<TowerScript>().SetConnectedLoadData(new Vector3(data.TowerFixedPos[i].x, data.TowerFixedPos[i].y, data.TowerFixedPos[i].z));
            }



        }

        for (int i = 0; i < data.EnemyCount; i++)
        {
            GameObject ene = Instantiate(EnemySpawnScript.spawner.ToSpawn, transform.position, Quaternion.identity);
            EnemyScript1 eneScript = ene.GetComponent<EnemyScript1>();
            eneScript.SetPathScript(eneScript.gameObject.GetComponent<FollowThePathScript>());
            eneScript.Die(false);

            eneScript.gameObject.transform.position = new Vector3(data.EnemyPositions[i].x, data.EnemyPositions[i].y, data.EnemyPositions[i].z); ;
            eneScript.gameObject.SetActive(true);
            eneScript.SetType((EnemyType)data.EnemyTypes[i]);
            eneScript.Health = data.EnemyHp[i];
            eneScript.SetFire(data.EnemyStatusEffects[i].IsOnFire);
            eneScript.SetWet(data.EnemyStatusEffects[i].IsWet);





        }




        EnemySpawnScript.spawner.SetWaveText();
        foreach (Slider item in Sliders.GetComponentsInChildren<Slider>())
        {
            if (item.GetComponent<AudioVOlumeAdjusterScript>()!= null)
            {
               item.GetComponent<AudioVOlumeAdjusterScript>().SetSliders(data.sfxValue, data.musvalue);
            }
        }

        Debug.Log("Game Loaded");
    }
    //saves out relevant data
    PlayerData CreateData(string levelVal, int lives, int loot) {

        PlayerData data = new PlayerData { };

        if (Sliders.GetComponentsInChildren<Slider>()[0].GetComponent<AudioVOlumeAdjusterScript>().IsSFX)
        {
            data.sfxValue = (int)Sliders.GetComponentsInChildren<Slider>()[0].value;
            
        }
        else
        {
            data.musvalue = (int)Sliders.GetComponentsInChildren<Slider>()[0].value;
            
        }


        if (Sliders.GetComponentsInChildren<Slider>()[1].GetComponent<AudioVOlumeAdjusterScript>().IsSFX)
        {
            data.sfxValue = (int)Sliders.GetComponentsInChildren<Slider>()[1].value;
            
        }
        else
        {
            data.musvalue = (int)Sliders.GetComponentsInChildren<Slider>()[1].value;
            
        }

        //saves out current level and loot and lives
        data.LootData = loot;
        data.LivesData = lives;
        data.LevelName = levelVal;

        //save outs current wave and make up
        data.waveNum = EnemySpawnScript.spawner.DisplayedWave;
        data.waveCurr = EnemySpawnScript.spawner.currentWave;
        data.waveCurrCount = EnemySpawnScript.spawner.currentCount;
        data.waveTime = EnemySpawnScript.spawner.GetTime();
        data.waveLengthCurr = EnemySpawnScript.spawner.GetCurrLength();



        //saves out tower data number, type, and position, is connected, coming soon draw line when connected
        data.TowerCount = Towers.Count;
        foreach (GameObject tow in Towers)
        {
            if (Towers.Count > 0)
            {
                TowerScript tow2 = tow.GetComponent<TowerScript>();
                int val = (int)tow2.Type;
                data.TowerTypes.Add(val);
                data.TowerPositions.Add(data.createVector(tow.transform.position.x, tow.transform.position.y, tow.transform.position.z));
                data.TowerIsConnected.Add(tow2.isConnected);
                data.TowerFixedPos.Add(data.createVector(tow2.GetFixedPoint().x, tow2.GetFixedPoint().y, tow2.GetFixedPoint().z));

            }
        }



        //saves out enemies location, number, health, type
        data.EnemyCount = Enemies.Count;
        foreach (EnemyScript1 ene in Enemies)
        {
            if (Enemies.Count > 0)
            {
                data.EnemyTypes.Add((int)ene.Type);
                data.EnemyHp.Add(ene.Health);
                data.EnemyPositions.Add(data.createVector(ene.gameObject.transform.position.x, ene.gameObject.transform.position.y, ene.gameObject.transform.position.z));
                data.EnemyStatusEffects.Add(data.CreateEnemyStatusEffects(ene.GetFire(), ene.GetWet()));
            }
        }

        return data;
    }



    [Serializable]
    public class PlayerData
    {
        [Serializable]
        public struct vector {
            public float x;
            public float y;
            public float z;

        };
        [Serializable]
        public struct EnemyBools {
            public bool IsOnFire;
            public bool IsWet;

        }

        public vector createVector(float X, float Y, float Z) {

            vector val = new vector();
            val.x = X;
            val.y = Y;
            val.z = Z;
            return val;

        }

        public EnemyBools CreateEnemyStatusEffects(bool fire, bool wet) {
            EnemyBools val = new EnemyBools();
            val.IsOnFire = fire;
            val.IsWet = wet;

            return val;
        }

        public int sfxValue;
        public int musvalue;

        public string LevelName;
        public int waveNum;
        public int waveCurr;
        public int waveCurrCount;
        public float waveTime;
        public int waveLengthCurr;

        public int LootData;
        public int LivesData;

        public int EnemyCount;
        public int TowerCount;
        public List<int> EnemyTypes = new List<int>();
        public List<int> EnemyHp = new List<int>();
        public List<int> TowerTypes = new List<int>();
        public List<vector> EnemyPositions = new List<vector>();
        public List<EnemyBools> EnemyStatusEffects = new List<EnemyBools>();


        public List<vector> TowerPositions = new List<vector>();
        public List<vector> TowerFixedPos = new List<vector>();
        public List<bool> TowerIsConnected = new List<bool>();

    }


}





