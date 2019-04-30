using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_Block : MonoBehaviour
{
    public enum states {
        MOVING,
        STOPPING,
        STOPPED,
    }
    enum otherNodeDir {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }
    public Sprite[] MySprites;


    ParticleSystem mySystem;
    float defaultSimSpeed = 1;
    ParticleSystem.MainModule main;    
    s_squashNStretch mySNS;
    protected bool hasSquashed = false;


    public GameObject[] Arms = new GameObject[4];
    //used only when this s_block is part of an s_TetrisBlock, it is used as part of the arm display code to show linke pyuos
    public s_Block[] MyOtherNodes;

    //used to determine if this tile will try to stop in a tile when it enters it.  Set to false when doing rotaion checks
    bool canOccupyTile = true;
    //used to provide a slight delay before the match check so all tiles can settle into place
    Timer checkMatchTimer = new Timer(0.1f);
    Timer fallCheck = new Timer(0.05f);
    s_Player owningPlayer = null;
    public states currentState = states.MOVING;
    public s_Tile currentTile = null; // the tile that is being regarded as this blocks "home" i.e. where it stopped
    public s_Tile enteredTile = null;// the last tile collider that this block's collider entered
    public s_Tile.Contents blockType = s_Tile.Contents.RED;
    Timer FindPlayer = new Timer(1.0f);
    //the Tetris block that holds all the node(s_Block)s
    s_TetrisBlock tetrisParent = null;
    int MoveUpsCalled = 0;
    bool fell = false;
    float trashFallRate = 7;

 
    // Start is called before the first frame update
    void Start()
    {
      
        FindPlayer.SetTimerShouldCountDown(true);
        mySNS = GetComponentInChildren<s_squashNStretch>();
    }

    // Update is called once per frame
    void Update()
    {
        if (owningPlayer == null)
        {
            if (FindPlayer.CountDownAutoCheckBool())
            {
                owningPlayer = FindObjectOfType<s_Player>();
            }
        }
        if (s_GameManager.Singleton.GetPauseState())
        {
            return;
        }

        withOwningPlayer();
        fallIfNothingSupporting();

    }

    void withOwningPlayer() {
        if (owningPlayer != null)
        {
            movement();
            float yToCheckAgainst = owningPlayer.GetTileList()[0].transform.position.y;
            if (transform.position.y < yToCheckAgainst)
            {
                s_Tile nearest = owningPlayer.GetClosestTile(transform.position);
                nearest = nearest.FindFreeTileInColumn();
                if (nearest != null)
                {
                    enteredTile = nearest;
                    currentTile = enteredTile;
                    transform.position = new Vector3(transform.position.x, nearest.transform.position.y + 0.1f, transform.position.z);
                    setState(states.STOPPING);
                }

            }

        }
    }

    //if a block is just stopped check if nothing is below it.  If nothing is supporting it startin falling
    void fallIfNothingSupporting() {
            if (currentState == states.STOPPED)
            {
                if (fallCheck.CountDown())
                {
                    if (currentTile != null && currentTile.GetDownTile() != null && currentTile.GetDownTile().GetContents() == s_Tile.Contents.NONE)
                    {
                        StartFalling();
                    }
                }
            }
        
    }
    
    void movement() {
        //speed multiplier dependent on player so different players can have blocks fall at different rates if desired
        float speedMultiplier = owningPlayer.GetFallSpeed();
        switch (currentState)
        {
            case states.MOVING:
                if (owningPlayer.GetCurrentState() != s_Player.PlayerStates.MATCHING)
                { moving(speedMultiplier); }
                break;
            case states.STOPPING:
                stopping(speedMultiplier);
                break;
            case states.STOPPED:
                stopped();
                break;
            default:
                break;
        }

    }


    void toggleParticleEffect() {
        if (mySystem.main.simulationSpeed != 0) //if game pasued stops particle movement
        {
            main = mySystem.main;
            main.simulationSpeed = 0;
            mySystem.Clear();

        }
        else if (mySystem.main.simulationSpeed != defaultSimSpeed) //if game unpaused returns particle movement to default speed
        {
            main = mySystem.main;
            main.simulationSpeed = defaultSimSpeed;
        }
    }
    public void SetStateToStopped() {
        setState(states.STOPPED);
    }
    void moving(float speed = 1) {
        //setEnteredTileViaCalc();
        if (transform.parent == null)
        {
            transform.position += (new Vector3(0, -0.5f, 0) * Time.deltaTime * speed * owningPlayer.GetScaleFactor() * (blockType == s_Tile.Contents.TRASH  && owningPlayer.GetCurrentState() == s_Player.PlayerStates.TRASHFALLING ? trashFallRate : 1));
            if (currentTile != null && enteredTile != null)
            {
                if (s_Calculator.AreNear(currentTile.GetTilePosition(), enteredTile.GetTilePosition(), 11))
                {
                    setState(states.STOPPING);
                    //currentState = states.STOPPING;
                }
            }

        }
    }
    void stopping(float speed = 1) {
        //moves the block towards the center of tile it is going to stop in
        if (currentTile != null)
        {
            transform.position -= (transform.position - currentTile.transform.position).normalized * Time.deltaTime * speed * owningPlayer.GetScaleFactor();
            //transform.position = Vector3.MoveTowards(transform.position, currentTile.transform.position, Time.deltaTime * speed);

            //when close to the center of the tile it is going to stop in the block is set in the center and its state set to stopped
            if (s_Calculator.GetDistanceLessThan(transform.position, currentTile.transform.position, 0.01f))
            {
                onBlockStop();
            }
        }
        
        
    }
   

    void stopped() {
        if (currentTile == null)// if it is stopped but does not have a tile that it is in start the block falling
        {
            StartFalling();
        }
        else
        {
            if (currentTile.occupant == this || currentTile.occupant == null)
            {
                if (fell)
                {
                    fell = false;
                    addSBlockToCheckList();
                }
                return;
            }
            else
            {//if it is stopped but the tile that it is in has another occupant already move up a tile to find a new one.
                //StartFalling();
                MoveUpATile();
            }
        }
    }
    void setState(states toSetTo)
    {
        
        currentState = toSetTo;
       
        switch (currentState)
        {
            case states.MOVING:
                hasSquashed = false;
                toggleParticleEffect();
                currentTile = null;
                break;
            case states.STOPPING:
                currentTile.SetTileOccupant(this);
                
                break;
            case states.STOPPED:
                if (tetrisParent != null && tetrisParent.GetPurpose() != s_TetrisBlock.BlockPurpose.DISPLAYNEXT)
                {
                    
                    transform.parent = null;
                    if (tetrisParent != null)
                    {
                        tetrisParent.PauseMovement();
                        tetrisParent.RemoveBlock(this);
                    }
                }
                StartSquashAndStretch();
              
                toggleParticleEffect();
                break;

            default:
                break;
        }

    }

    void StartSquashAndStretch() {
        if (tetrisParent != null && tetrisParent.GetPurpose() != s_TetrisBlock.BlockPurpose.DISPLAYNEXT)
        {
            List<int> otherBlocksInCol = new List<int>();
            for (int i = 0; i < tetrisParent.GetContainedBlocksArray().Count; i++)
            {
                s_Tile tempTile = tetrisParent.GetContainedBlocksArray()[i].GetLastTileEntered();
                if (tempTile == null)
                {
                    tempTile = owningPlayer.GetClosestTile(transform.position);
                }
                if (tempTile.GetColumnIn() == enteredTile.GetColumnIn())
                {
                    otherBlocksInCol.Add(tempTile.GetRowIn());
                }
            }
            int height = otherBlocksInCol.Count;
            for (int i = 0; i < otherBlocksInCol.Count; i++)
            {
                if (otherBlocksInCol[i] > enteredTile.GetRowIn())
                {
                    height--;
                }
            }

            mySNS.StartSquash(currentTile, height);
        }
        else
        {
            if (enteredTile != null)
            {
                int h = 0;
                /*for (int i = 0; i < owningPlayer.GetSBlockList().Count; i++)
                {
                    s_Tile tempTile = owningPlayer.GetSBlockList()[i].GetEnteredTile();
                    if (tempTile != null)
                    {
                        if (tempTile.GetColumnIn() == enteredTile.GetColumnIn())
                        {
                            if (tempTile.GetRowIn() < enteredTile.GetRowIn() && !tempTile.GetTileOccupant().hasSquashed)
                            {
                                h++;
                            }
                        }
                    }
                }*/
                
                mySNS.StartSquash(currentTile, h);
                hasSquashed = true;
            }

        }

    }
    public s_squashNStretch GetSNS() {
        return mySNS;
    }

    //the code called for when a block stops in a tile
    void onBlockStop() {
        if (tetrisParent != null && tetrisParent.GetPurpose() == s_TetrisBlock.BlockPurpose.VIRTUAL)
        {
            
            return;
        }
        currentTile.SetContents(blockType); //set the tile's contents to the block's type
        transform.position = new Vector3(currentTile.transform.position.x, currentTile.transform.position.y, transform.position.z); //jumps the block to the center of the tile
        addSBlockToCheckList();
        setState(states.STOPPED); //sets the block's movement type to stopped
 
        AudioControlerScript.Singleton.PlayPuyoSet();

    }

    public void AddSBlockToCheck() {
        addSBlockToCheckList();
    }

    void addSBlockToCheckList() {
        if (blockType != s_Tile.Contents.TRASH)//if it is trash we do not need to check it for a match
        {
            if (currentTile == null)
            {
                if (enteredTile != null)
                {
                    owningPlayer.AddToCheckList(enteredTile);
                }
                else
                {
                    Debug.LogError("Both enteredTile and currentTile are null and cannot be added to toChecklist.");
                }
            }
            else
            {
                owningPlayer.AddToCheckList(currentTile);
            }
        }
    }

    //Called on collisions
    void OnTriggerEnter2D(Collider2D col)
    {

        onEnter(col);
        
    }
    
    //code to help keep s_blocks from overlapping
    void OnTriggerStay2D(Collider2D c) {
        if (canOccupyTile)
        {
            //onStay(c);
        }
    }
    //This code runs when a block enters a new tile
    protected void onEnter(Collider2D c) {
        s_Tile myTile = c.gameObject.GetComponent<s_Tile>(); //gets a referance to the tile the block just entered
        s_Block block = c.gameObject.GetComponent<s_Block>(); //gets a referance to the tile the block just entered
        if (canOccupyTile && tetrisParentTest(s_TetrisBlock.BlockPurpose.VIRTUAL, true))
        {
            if (myTile != null)//if it has collided with a tile
            {
                SetLastTileEntered(myTile);
                if (myTile.GetTilePosition() < owningPlayer.GetColumnCount() && 
                    (myTile.occupant == null || myTile.occupant == this))
                {
                    currentTile = enteredTile;
                    transform.position = currentTile.transform.position;
                    setState(states.STOPPING);
                }
                else
                {
                    BeginStopInEnteredTile();
                }
            }

        }

    

    }

    void setEnteredTileViaCalc() {
        //+ new Vector3(owningPlayer.GetTileXOffset() * 0.01f, owningPlayer.GetTileYOffset()*0.05f, 0)
        enteredTile = owningPlayer.GetClosestTile(transform.position);
        if (GetEnteredTile().CanSupportBlock())
        {
            if (owningPlayer.Player == s_Player.PlayerNumber.ONE)
            {

                Debug.Log("tile below entered tile " + GetEnteredTile().name + " is " + 
                    (GetEnteredTile().GetDownTile() == null ? "null" : 
                    GetEnteredTile().GetDownTile().name) + 
                    " with a contents of " +
                    (GetEnteredTile().GetDownTile() == null ? "null" : GetEnteredTile().GetDownTile().GetContents().ToString()));   
            }
        }
        BeginStopInEnteredTile();

    }

    public void BeginStopInEnteredTile() {
        if (enteredTile != null && enteredTile.CanSupportBlock()) //checks to see if the tile below this one is empty or if there is no tile below this one
        {
            if (currentTile == null)
            {
                
                currentTile = enteredTile; //Sets the tile that block will have as its location to the current tile it is in
                s_Block otherSBlock = currentTile.GetTileOccupant();
                if (otherSBlock == null || otherSBlock == this)
                {
                    setState(states.STOPPING); //if it is empty then transitions to stopping
                    
                }
                else
                {
                    if (//((int)currentState < (int)otherSBlock.GetCurrentState()) || 
                        (otherSBlock.transform.position.y < transform.position.y))
                    {
                        MoveUpATile();
                    }
                    else
                    {
                        
                        otherSBlock.MoveUpATile();
                    }
                }
            }

        }
    }

    public void MoveUpATile() {
        string message = "";
        if (s_GameManager.GetDebug()) { message = gameObject.name + "cannot reside in " + currentTile.GetTilePosition() + " because " + currentTile.GetTileOccupant().name + " is there so instead residing in "; }
        MoveUpsCalled++;
        if (MoveUpsCalled > owningPlayer.GetRowCount()*2)
        {
            MoveUpsCalled = 0;
            Debug.Log("Break move up");
            return;
        }

        
        if (currentTile != null)
        {
            if (currentTile.occupant == null || currentTile.occupant == this)
            {
                MoveUpsCalled = 0;
                transform.position = currentTile.transform.position + new Vector3(0, 0.1f,0);
                setState(states.STOPPING); //if it is empty then transitions to stopping
                
            }
            else
            {
                currentTile = currentTile.GetUpTile();
                MoveUpATile();
            }
        }
        else
        {
            if (blockType != s_Tile.Contents.TRASH)
            {
                LoseGame();
            }

        }
        if (s_GameManager.GetDebug()) { Debug.Log(message + currentTile.GetTilePosition()); }
        SetLastTileEntered(currentTile);
    }

    protected void onStay(Collider2D c) {
        if (tetrisParentTest(s_TetrisBlock.BlockPurpose.VIRTUAL, true))
        {
            s_Block block = c.gameObject.GetComponent<s_Block>(); //gets a referance to the tile the block just entered
            if (block != null)
            {
                if (transform.position == block.transform.position)// if the transform of this is equal to the transform of another block which should only happen if they are occupying the same tile
                {
                    if (currentTile != null && currentTile.occupant != this) //if this does have a current tile and that tile's occupant is not this block
                    {
                        if (currentTile.GetUpTile() != null)// if there is a tile above the tile with overlapping s_Blocks
                        {

                            //transform.position = currentTile.GetUpTile().transform.position;//move the s_block up a tile
                            //currentState = states.MOVING;
                            //currentTile = null;

                        }
                        else
                        {
                            LoseGame();// Call the EndGame function from the game manager script
                        }

                    }
                }
            }
        }
    }

    public void OnSpawn(s_Player myPlayer) {
        
        owningPlayer = myPlayer;
        owningPlayer.AddSBlock(this);
        mySystem = GetComponentInChildren<ParticleSystem>(); //gets referance to particle system
        defaultSimSpeed = mySystem.main.simulationSpeed; //set the default speed of the system i.e. what it will return to when unpaused
       

    }
    public void OnSpawn(Vector2 startPos)
    {
        
        transform.position = startPos;
        gameObject.SetActive(true);
        mySystem = GetComponentInChildren<ParticleSystem>(); //gets referance to particle system
        defaultSimSpeed = mySystem.main.simulationSpeed; //set the default speed of the system i.e. what it will return to when unpaused
       
    }

    public void SetParticle(){

        if (blockType != s_Tile.Contents.TRASH)
        {
            mySystem.gameObject.SetActive(false);
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Setting particle");
            }
            //Byron make notes of any adjustements to particle system unique to each stage so I can get Unity to apply them on a per stage basis
            ParticleSystemRenderer psr = mySystem.GetComponent<ParticleSystemRenderer>();
            Mesh[] m = s_GameManager.Singleton.GetParticleMeshByStage();
            psr.SetMeshes(m);
            
            
            mySystem.GetComponent<ParticleSystemRenderer>().material = s_GameManager.Singleton.GetParticleMaterialByStage();
            main = mySystem.main;
            mySystem.gameObject.SetActive(true);
            //use switch statement below to affect the particle system in other ways such as speed, veloity, etc.
            switch (s_ChooseGameAndPlayerType.Singleton.GetSelectedStage())
            {
                case s_ChooseGameAndPlayerType.SCENETOLOAD.JCScene:
                    main.startRotationY = 180;
                    main.startSize = 0.08f;
                    break;
                case s_ChooseGameAndPlayerType.SCENETOLOAD.StageScene:
                    main.startSize = 0.1f;
                    break;
                case s_ChooseGameAndPlayerType.SCENETOLOAD.LibraryScene:
                    main.startSize = 0.08f;
                    break;
                case s_ChooseGameAndPlayerType.SCENETOLOAD.SoftballScene:
                    main.startSize = 0.07f;
                    break;
                case s_ChooseGameAndPlayerType.SCENETOLOAD.StatueScene:
                    main.startSize = 0.1f;
                    break;
                case s_ChooseGameAndPlayerType.SCENETOLOAD.InsideArtBuilding:
                    Debug.Log("Missing case");
                    break;
                default:
                    break;
            }

        }
        else
        {
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Particle Counts as trash");
            }
            
            
            mySystem.gameObject.SetActive(false);
            mySystem.GetComponent<ParticleSystemRenderer>().material = s_GameManager.Singleton.m_Trash;
            ParticleSystemRenderer psr = mySystem.GetComponent<ParticleSystemRenderer>();
            Mesh[] m = s_GameManager.Singleton.Trash;
            psr.SetMeshes(m);
            mySystem.gameObject.SetActive(true);
            
        }



    }

    public s_Player GetOwningPlayer() {
        return owningPlayer;
    }

    public void StartFalling() {
        actuallyStartFalling();
    }

    void actuallyStartFalling() {
        if (currentTile != null && currentState == states.STOPPED)
        {
            if (currentTile.occupant == this)//if the tile it is in has this block as an occupant tell that tile that its occupant is no longer valid
            {
                currentTile.occupant = null;
            }
            setState(states.MOVING);
            fell = true;
        }
    }
    public void Die() {

        owningPlayer.RemoveSBlock(this);
        Destroy(gameObject);

    }

    //sets the color of this block
    public void SetBlockType(s_Tile.Contents blockColor) {
        blockType = blockColor;
        if (mySystem == null)
        {
            mySystem = GetComponentInChildren<ParticleSystem>();
        }
        main = mySystem.main;
        SpriteRenderer myRenderer = GetComponent<SpriteRenderer>();
        

        switch (blockType)
        {
            case s_Tile.Contents.NONE:
                Debug.LogError("Uh Oh! A block had its type set to none!!");
                break;
            case s_Tile.Contents.TRASH:
                //main.startColor = Color.red;
                //myRenderer.color = Color.red;
                myRenderer.sprite = MySprites[0];
                break;
            case s_Tile.Contents.RED:
                //GetComponent<SpriteRenderer>().color = Color.red;
                //main.startColor = Color.magenta;
                myRenderer.sprite = MySprites[1];
                break;
            case s_Tile.Contents.GREEN:
                //GetComponent<SpriteRenderer>().color = Color.green;
               // main.startColor = Color.green;
                myRenderer.sprite = MySprites[2];
                break;
            case s_Tile.Contents.BLUE:
                //main.startColor = Color.black;
                //GetComponent<SpriteRenderer>().color = Color.blue;
                myRenderer.sprite = MySprites[3];
                break;
            case s_Tile.Contents.ORANAGE:
                //main.startColor = Color.white;
                //GetComponent<SpriteRenderer>().color = Color.yellow;
                myRenderer.sprite = MySprites[4];
                break;
            default:
                break;
        }
        SetParticle();

    }

    //sets the parent for this s_Block
    public void SetTetrisParent(s_TetrisBlock myParent) {
        tetrisParent = myParent;
    }
    public s_TetrisBlock GetTetrisParent()
    {
        return tetrisParent;
    }
    public void ResetCheckCountDown(){
        checkMatchTimer.SetTimerShouldCountDown(true);
    }

    public void SetCanOccupyTile(bool val) {
        canOccupyTile = val;
    }

    public bool GetCanOccupyTile() {
        return canOccupyTile;
    }
    public s_Tile GetLastTileEntered() {
        return enteredTile;
    }
    public void SetLastTileEntered(s_Tile toBeLastEntered)
    {
        enteredTile = toBeLastEntered;
    }

    //returns the current state of the s_block
    public states GetCurrentState() {
        return currentState;
    }

    public void SnapToEnteredTile() {
        transform.position = enteredTile.transform.position;

    }

    public s_Tile GetEnteredTile() {
        /*if (enteredTile == null)
        {
            return owningPlayer.GetClosestTile(transform.position);
        }
        else
        {*/
            return enteredTile;
        //}
    }

    public s_Tile.Contents GetBlockType() {
        return blockType;
    }

    bool tetrisParentTest(s_TetrisBlock.BlockPurpose testAgainst, bool testForNotEqual) {

        if (testForNotEqual)
        {
            return tetrisParent == null || tetrisParent.GetPurpose() != testAgainst;
        }
        else
        {
            return tetrisParent == null || tetrisParent.GetPurpose() == testAgainst;
        }

    }

    void setVisibleArms() {

        for (int i = 0; i < Arms.Length; i++)
        {
            if (Arms[i] == null)
            {
                Debug.LogError("The arm objects for pyuos have not been set!  Please set them in the inspector.");
                return;
            }
            Arms[i].SetActive(false);
        }

        if (tetrisParent == null)
        {
            if (currentTile != null &&
                currentTile.GetLeftTile() != null &&
                currentTile.GetLeftTile().GetContents() != s_Tile.Contents.NONE)
            {
                Arms[(int)otherNodeDir.LEFT].SetActive(true);
            }

            if (currentTile != null &&
                currentTile.GetRightTile() != null &&
                currentTile.GetRightTile().GetContents() != s_Tile.Contents.NONE)
            {
                Arms[(int)otherNodeDir.RIGHT].SetActive(true);
            }

            if (currentTile != null &&
                currentTile.GetUpTile() != null &&
                currentTile.GetUpTile().GetContents() != s_Tile.Contents.NONE)
            {
                Arms[(int)otherNodeDir.UP].SetActive(true);
            }

            if (currentTile != null &&
                currentTile.GetDownTile() != null &&
                currentTile.GetDownTile().GetContents() != s_Tile.Contents.NONE)
            {
                Arms[(int)otherNodeDir.DOWN].SetActive(true);
            }
        }
        else
        {
            if (MyOtherNodes[(int)otherNodeDir.LEFT] != null)
            {
                Arms[(int)otherNodeDir.LEFT].SetActive(true);
            }

            if (MyOtherNodes[(int)otherNodeDir.RIGHT] != null)
            {
                Arms[(int)otherNodeDir.RIGHT].SetActive(true);
            }

            if (MyOtherNodes[(int)otherNodeDir.UP] != null)
            {
                Arms[(int)otherNodeDir.UP].SetActive(true);
            }

            if (MyOtherNodes[(int)otherNodeDir.DOWN] != null)
            {
                Arms[(int)otherNodeDir.DOWN].SetActive(true);
            }
        }
    }

    public void LoseGame() {
        s_GameManager.PlayerData losingPlayer = owningPlayer.CreatePlayerDataStructOfPlayer();
        s_GameManager.PlayerData winningPlayer = new s_GameManager.PlayerData();

        // Game Over Functionality
        // Debug.LogError("NO FREE SPACE YOU LOSE. Lost by " + gameObject.name + " at pos " + transform.position);
        for (int i = 0; i < s_GameManager.Singleton.Players.Count; i++)
        {

            if (s_GameManager.Singleton.Players[i] != owningPlayer && s_GameManager.Singleton.Players[i] != null && s_GameManager.Singleton.Players[i].gameObject != null)
            {
                winningPlayer = s_GameManager.Singleton.Players[i].CreatePlayerDataStructOfPlayer();
                break;
            }

        }

        s_GameManager.Singleton.EndGame(losingPlayer, winningPlayer); // Call the EndGame function from the game manager script

    }
}
