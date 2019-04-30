using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class s_Player : MonoBehaviour
{
    protected int totalBlocksMatched = 0;
    public enum PlayerStates {
        FALLING, //This state exits when all of the s_blocks are in the stopped state  it exits into the  matching state.  It prohibits the player from moving anything.
        MATCHING, //This state is where s_player runs through all the recently stopped moving s_blocks and checks their coorsponding s_tile to see if any matches have been made from the s_block stopping there.  If there have been it matches them all destroys them, and goes into the falling state  then continues the cycle.  If no matches are found then it exits to the moving state.
        MOVING, //This state exits when all of the s_blocks are in the stopped state  it exits into the  matching state.  It is the default state where the player can move the tetris piece and position it.
        TRASHFALLING //the stage where trash falls
    }
    //End game stat variables
    int stats_BlocksMatched = 0;
    int stats_CombosMade = 0;
    int stats_TrashDumpedOnOther = 0;
    int stats_UltimatePointsEarned = 0;
    int stats_UltimatesUsed = 0;


    protected bool foundValidMatch = true;
    public PlayerStates currentState = PlayerStates.MOVING;
    protected s_TetrisBlock myTetrisBlock = null;
    public Text ChosenCharacter = null;
    public enum PlayerNumber {
        ONE,
        TWO,
        AI
    }
    public PlayerNumber Player;
    s_ChooseGameAndPlayerType.character myCharacter;
    List<s_Tile> toCheckTiles = new List<s_Tile>(); //que of the tiles that have landed and are waiting to be have a match check
    public s_Tile currentlyChecking = null; //tile that is the current origin of a check
    public bool ReadyToCheck = true; //ensures that only one tile can be the origin for a check at a time, probably unnecessary
    protected List<s_Block> mySBlocks = new List<s_Block>(); // list of all s_blocks for player
    protected List<s_Block> toCheckSBlocks = new List<s_Block>();

    protected int trashCount = 0;
    protected List<s_Tile> myTiles = new List<s_Tile>();//list that contains all the tiles (tiles are occupied by blocks and are used to check for matches they form a grid)for this player
    public List<s_Tile> matches = new List<s_Tile>(); //This list is used when checking for matches each tile that is adjacent and of the same color is added to this list when checking for matches
    protected List<s_Tile> toFall = new List<s_Tile>();//this list stores all the tiles directly above tiles in matches so they can be told to fall once their support is removed
    protected List<s_Tile> toFallColumns = new List<s_Tile>(); //related to the above list this one has all the tiles in a column that are no longer support preped to fall
    protected float fallSpeed = 1.0f; //used to change the rate at which blocks move, setting to higher than four may cause crashes
    public s_BlockSpawner MyBlockSpawner;
    public s_TileSpawner MyTileSpawner;
    s_AnimationQueue myAnimationQueue;
    float scaleFactor = 1;
    float arFactor = 1;
    Timer meterChargeTimer = new Timer(0.5f);
    protected int ultimateCharge = 0;  //The meter for the ultimate
    protected int ultimateMax = 100;  //the amount of ultimate charge required to fire off the ultimate.
    //public s_StatBarLinear MyMeter;
    public s_StatBarLinear MyMeter;
    public s_PowerBarControler MyComboMeter;
    public s_UltIcon myUltIcon;
    protected float tileXOffset = 0;
    protected float tileYOffset = 0;

    protected int blocksNeededForMatch = 4; //the number of adjacent blocks needed to be a valid match
    protected int score = 0;
    protected int comboCount = 0;
    protected Timer resetComboMeter = new Timer(0.5f); // how long before the reset counter returns to zero

    protected s_Ultimate_Abstract myUltimate;
    int trashToAddToOther = 0;
    int queuedTrash = 0;
    int maxTrashToSpawnPerPhase = 40;
    protected s_TrashDisplay myQueuedTrash;
    protected bool deathFlag = false;

    //public s_CameraShake cameraShake;

    // Awake is called before Start
    void Awake()
    {
        if (Player == PlayerNumber.TWO &&
            s_ChooseGameAndPlayerType.Singleton != null &&
            s_ChooseGameAndPlayerType.Singleton.GetPlayerAI())
        {
            deathFlag = true;
        }
        onAwake();
    }

    protected void onAwake() {
        if (deathFlag)
        {
            return;
        }
        MyBlockSpawner.SetOwningPlayer(this);
        MyTileSpawner.SetOwningPlayer(this);
        MyTileSpawner.OnSpawn();
        MyBlockSpawner.OnSpawn();
        GetComponent<SpriteRenderer>().enabled = false;
        scaleFactor = GetTileList()[0].transform.localScale.x;
        myQueuedTrash = GetComponentInChildren<s_TrashDisplay>();

    }

    void Start() {

        //cameraShake = Camera.main;
        onStart();
    }

    public s_Tile GetTileFromRowAndColumn(int row, int col)
    {

        for (int i = 0; i < myTiles.Count; i += GetColumnCount())
        {
            if (myTiles[i].GetRowIn() == row)
            {
                for (int j = 0; j < GetColumnCount(); j++)
                {
                    if (myTiles[i + j].GetColumnIn() == col)
                    {
                        return myTiles[i + j];
                    }
                }
            }
        }
        return null;
    }


    protected virtual void onStart() {
        if (deathFlag)
        {
            destroyPlayer();
        }
        else
        {
            s_GameManager.Singleton.Players.Add(this);
        
        }
        setCharachterType();
        tileXOffset = MyTileSpawner.GetColOffset();
        tileYOffset = MyTileSpawner.GetRowOffset();
        myUltimate = GetComponent<s_Ultimate_Abstract>();

        if (myUltimate != null)
        {
            myUltimate.OnStart(this);
            if (MyMeter != null)
            {
                MyMeter.SetLinearBarExtremes();
            }
        }
     
    }
    //debug message
    void DB_MatchThatIsbeingCleared(int i) {

        string message = "A tile in matches with position in list of " + matches[i].GetTilePosition() + " is in matches list at position " + matches[i].name + " has content of " + matches[i].GetContents().ToString();
        if (matches[i].GetTileOccupant() != null)
        {
            message += " occupant has type of " + matches[i].GetTileOccupant().GetBlockType().ToString();
        }
        Debug.Log(message);
    }

    //Called when clearing a match and adjacent tiles of same color
    public void SuccessfulMatchClear() {

        for (int i = 0; i < matches.Count; i++)
        {
            if (s_GameManager.GetDebug())
            {
                DB_MatchThatIsbeingCleared(i);
            }


            matches[i].ClearTile(); //clears the tile that is part of a match
            clearAdjacentTrash(matches); //clears the trash adjacent to the match tiles
        }

        //begins set up for tiles above the match to start falling
        for (int i = 0; i < matches.Count; i++)
        {
            if (!matches.Contains(matches[i].GetUpTile()) && matches[i].GetUpTile() != null && matches[i].GetUpTile().GetContents() != s_Tile.Contents.NONE)
            {
                toFall.Add(matches[i].GetUpTile());//starts the list of tiles to fall
                if (s_GameManager.GetDebug())
                {
                    Debug.Log(matches[i].GetUpTile().name + " is a tile that is set to fall and has the contents of " + matches[i].GetUpTile().GetContents().ToString() + "With the occupant of type " + matches[i].GetUpTile().occupant.GetBlockType().ToString());
                }

            }
            else
            {
                if (s_GameManager.GetDebug())
                {
                    if (matches[i].GetUpTile() != null)
                    {
                        Debug.Log(matches[i].GetUpTile().name + " is in matches or is empty and will not be added to fall list");
                    }
                    else
                    {
                        Debug.Log(matches[i].name + "Does not have tile above according to check");
                    }

                }


            }
        }
        startFalling();

    }

    //preps all the tiles above a match to start falling
    void startFalling() {
        if (s_GameManager.GetDebug())
        {
            Debug.Log("Tiles to fall is " + toFall.Count);
        }
        for (int i = 0; i < toFall.Count; i++)
        {
            if (s_GameManager.GetDebug()) {
                Debug.Log("to fall entry " + i + " is " + toFall[i].GetTilePosition());
            }

            if (toFall[i] != null && toFall[i].GetUpTile() != null || toFall[i].GetUpTile().GetContents() != s_Tile.Contents.NONE)
            {
                toFallColumns.Add(toFall[i]);
                toFall[i].SetColumnForFalling();//checks all the tiles above and sets them to fall
            }

        }
        toFall.Clear();


    }

    public void AddToFallColumns(s_Tile tileToFall) {
        toFallColumns.Add(tileToFall);
    }
    public void ResetMatches() {
        for (int i = 0; i < matches.Count; i++)
        {
            matches[i].ClearNoMatch();
        }
        matches.Clear();
        ReadyToCheck = true;
    }



    public List<s_Tile> GetTileList() {
        return myTiles;
    }
    public s_Tile GetTileAtPosition(int pos) {
        return myTiles[pos];
    }
    public float GetRightMostColumnXPos() {
        return GetTileList()[GetColumnCount() - 1].transform.position.x;
    }

    public Vector3 GetTopRightTilePos()
    {
        return GetTileList()[GetColumnCount() - 1].transform.position;
    }
    public float GetLeftMostColumnXPos()
    {
        return GetTileList()[0].transform.position.x;
    }
    //Adds a tile to the master list of all tiles for this player in the game
    public void AddTile(s_Tile tileToAdd)
    {
        myTiles.Add(tileToAdd);
    }
    public void AddMatch(s_Tile tileToAdd)
    {

        if (tileToAdd.GetContents() != s_Tile.Contents.NONE &&
            tileToAdd.GetContents() != s_Tile.Contents.TRASH &&
            !matches.Contains(tileToAdd))// the tile is not already listed in matches as is not empty.
        {
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Adding tile " + tileToAdd.name + " with contents of " + tileToAdd.GetContents() + " to matches");
            }

            matches.Add(tileToAdd);


        }
    }
    public List<s_Tile> GetMatchesList()
    {
        return matches;
    }
    //Gets the fall speed multiplier for Player one
    public float GetFallSpeed()
    {
        return fallSpeed;
    }

    public int GetMatchNumNeeded() {
        return blocksNeededForMatch;
    }

    public void ClearMatches() {
        matches.Clear();
    }

    public s_Tile GetClosestTile(Vector3 posToCheckAgainst) {
        Vector3 pos = posToCheckAgainst;
        float minDist = s_Calculator.square(25);
        s_Tile toReturn = myTiles[0];
        foreach (s_Tile t in myTiles)
        {
            if (s_Calculator.GetDistanceLessThan(t.transform.position, pos, minDist))
            {
                minDist = s_Calculator.distanceSquare(t.transform.position, pos);
                toReturn = t;
            }
        }
        return toReturn;

    }
    //Check this when more awake, should be more efficent?
    public s_Tile GetClosestTileE(Vector3 posToCheckAgainst)
    {
        Vector3 pos = posToCheckAgainst;
        float minDist = s_Calculator.square(25);
        s_Tile toReturn = myTiles[0];
        int posToCheckFrom = 0;
        for (int i = 0; i < myTiles.Count; i += GetColumnCount())
        {
            if (s_Calculator.GetDistanceLessThan(myTiles[i].transform.position, pos, minDist))
            {
                posToCheckFrom = myTiles[i].GetTilePosition();
            }
        }
        minDist = s_Calculator.square(25);
        for (int j = posToCheckFrom; j < (posToCheckFrom + GetColumnCount() - 1); j++)
        {
            if (s_Calculator.GetDistanceLessThan(myTiles[j].transform.position, pos, minDist))
            {
                minDist = s_Calculator.distanceSquare(myTiles[j].transform.position, pos);
                toReturn = myTiles[j];
            }
        }
        return toReturn;
    }

    public void AddSBlock(s_Block blockToAdd) {
        if (!mySBlocks.Contains(blockToAdd))
        {
            trashCount++;
            mySBlocks.Add(blockToAdd);
        }
    }
    public List<s_Block> GetSBlockList() {
        return mySBlocks;
    }
    public void RemoveSBlock(s_Block blockToRemove) {
        if (mySBlocks.Contains(blockToRemove))
        {
            if (trashCount > 1)
            {
                trashCount--;
            }
            mySBlocks.Remove(blockToRemove);
        }
    }

    public float GetTileXOffset() {
        return tileXOffset;
    }
    public float GetTileYOffset()
    {
        return tileYOffset;
    }



    void checkTileBasedOnQueue() {
        int tempBlocksMatched = 0;
        SetFoundValidMatch(false);//currently there was no valid match found.
        convertSblockToCheckToTilesToCheck();
        int listPosToCheck = toCheckTiles.Count - 1; //the first tile to check, this sets it to be the last one added to the que.
        for (int i = listPosToCheck; i > 0; i--)//starting at the most recently added and working backwards
        {
            matches.Clear();//clears the match list so that we can check for a new match, i.e. tiles that were the previous match are not counted as part of this one
            if (toCheckTiles[i] != null)//if the tile is not null, reasonably speaking it should never be, but never hurts to check
            {
                currentlyChecking = toCheckTiles[i];//the tile to currently check  assigning it here is somewhat of a holdover from a previous version, but I left it here in case the need arose to make use of it.

                if (currentlyChecking.GetContents() != s_Tile.Contents.TRASH)
                {
                    int mc = currentlyChecking.CheckMatch();//initates the check with this tile as the origin point for the check
                    if (mc >= GetMatchNumNeeded())//if a match was made
                    {
                        tempBlocksMatched += mc; //record it in the tempBlocksMatched
                        comboCount++;//increase the combo count by 1
                        AddToScore((((mc - GetMatchNumNeeded()) * 100) + 100) * comboCount);
                        
                        s_GameManager.Singleton.GetComboImage(MyComboMeter.transform.position + new Vector3(0.5f + (comboCount * (0.2f * (s_Calculator.GetOneInTwo() ? 1 : -1))), 0, 0)).OnSpawn(comboCount);
                        if (comboCount > 1)
                        {
                            AudioControlerScript.Singleton.PlayPuyoCombo();//Play PuyoCombo sound
                        }
                        AddAnimations(comboCount);
                        if (MyComboMeter != null)
                        {
                            MyComboMeter.SetBar(comboCount, 5);
                        }
                    }
                }

            }
        }


        if (foundValidMatch)//the logic here is that we only need to set tiles to fall if a match was found because if no match was found then none of the tiles would have moved so if nothing was in a position to fall it still will not be in a position to fall
        {
            foreach (s_Tile t in toFallColumns)
            {
                t.SetContentsToFall();
            }
            toFallColumns.Clear();
            totalBlocksMatched += tempBlocksMatched; //increase the total blocks matched by the number of blocks matched this pass itteration

            currentState = PlayerStates.FALLING;//we go to the falling state where all the tiles that no longer have support under them (due to matches) fall, from the falling state we exit back into the matching state and continue the loop until no matches are found
        }
        else
        {
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Combo Length of " + comboCount.ToString() + " with total blocks matched equaling: " + totalBlocksMatched.ToString());
            }
            trashToAddToOther = ((totalBlocksMatched - (3 * comboCount)) * (comboCount));//sets the trash to add to the other

            stats_TrashDumpedOnOther += trashToAddToOther;
            handleTrashAdding();
            spawnQueuedTrashDuringQueuePhase();
            AddToUltimateCharge(4 * comboCount); //adds to the ultimate charge meter
            stats_BlocksMatched += totalBlocksMatched;
            stats_CombosMade += comboCount;
            totalBlocksMatched = 0; //resets total blocks matched
            if (comboCount > 1)
            {
                myAnimationQueue.AddAnimation(s_AnimationQueue.Animations.ATTACK2);
            }
            comboCount = 0; //resets the combo counter since we have now exited any chance of making more matches until thie next tetris piece has stopped moving

            if (MyComboMeter != null)
            {
                resetComboMeter.SetTimerShouldCountDown(true);
            }

            MyBlockSpawner.SpawnPiece();//spawns a new tetris piece
            AudioControlerScript.Singleton.PlayPuyoMatch();// play PuyoMatch
        }


    }
    void checkIfAllTilesStopped() {
        bool allStopped = true;
        for (int i = 0; i < mySBlocks.Count; i++)
        {
            if (mySBlocks[i].GetBlockType() != s_Tile.Contents.TRASH)
            {
                if (mySBlocks[i].GetCurrentState() != s_Block.states.STOPPED &&
                    (mySBlocks[i].GetTetrisParent() == null ||
                    mySBlocks[i].GetTetrisParent().GetPurpose() != s_TetrisBlock.BlockPurpose.DISPLAYNEXT) ||
                 mySBlocks[i].GetSNS().GetStreching()
                )
                {
                    allStopped = false;
                    break;
                }
            }

        }
        if (allStopped)
        {
            currentState = PlayerStates.MATCHING;
        }
    }
    void checkIfAllTrashStopped() {
        bool allStopped = true;
        for (int i = 0; i < mySBlocks.Count; i++)
        {

            if (mySBlocks[i].GetBlockType() == s_Tile.Contents.TRASH)
            {
                if (mySBlocks[i].GetTetrisParent() != null &&
                    mySBlocks[i].GetTetrisParent().GetPurpose() != s_TetrisBlock.BlockPurpose.ULTIMATEEFFECT)
                {

                }
                else
                {
                    if (mySBlocks[i].GetCurrentState() != s_Block.states.STOPPED &&
                    (mySBlocks[i].GetTetrisParent() == null ||
                    mySBlocks[i].GetTetrisParent().GetPurpose() != s_TetrisBlock.BlockPurpose.DISPLAYNEXT) ||
                    mySBlocks[i].GetSNS().GetStreching()
                )
                    {
                        allStopped = false;
                        break;
                    }
                }

            }

        }
        if (allStopped)
        {
            currentState = PlayerStates.MOVING;
        }
    }

    void haveEverythingFall() {
        bool allStopped = true;
        for (int i = 0; i < mySBlocks.Count; i++)
        {
            if (mySBlocks[i].GetBlockType() != s_Tile.Contents.TRASH &&
                mySBlocks[i].GetCurrentState() != s_Block.states.STOPPED &&
                (mySBlocks[i].GetTetrisParent() == null ||
                 mySBlocks[i].GetTetrisParent().GetPurpose() != s_TetrisBlock.BlockPurpose.DISPLAYNEXT) ||
                 mySBlocks[i].GetSNS().GetStreching()
                 )
            {
                allStopped = false;
                break;
            }
        }
        if (allStopped)
        {
            currentState = PlayerStates.MATCHING;
        }


    }

    // Update is called once per frame
    void Update()
    {
        onUpdate();
        checkInputForUltimate();


    }
    protected virtual void onUpdate() {
        if (s_GameManager.Singleton.GetPauseState())
        {
            return;
        }

        if (s_GameManager.Singleton.Cheats)
        {
            if (Input.GetKeyUp(KeyCode.K) && Player == PlayerNumber.ONE)
            {
                callUltimate();
            }
            if (Input.GetKeyUp(KeyCode.Y) && Player == PlayerNumber.ONE)
            {
                callUltimateOnOther();
            }
        }


        if (meterChargeTimer.CountDown())
        {
            AddToUltimateCharge(1);
        }
        if (resetComboMeter.CountDownAutoCheckBool())
        {
            MyComboMeter.SetBar(comboCount, 5);
        }
        actionsBasedOnPlayerState();
    }

    void callUltimate() {
        if (myUltimate != null)
        {
            myUltimate.ActivateUltimate(this);
        }
    }
    protected void callUltimateOnOther()
    {
        if (myUltimate != null)
        {

            for (int i = 0; i < s_GameManager.Singleton.Players.Count; i++)
            {
                if (s_GameManager.Singleton.Players[i] != this && s_GameManager.Singleton.Players[i] != null)
                {
                    if (s_GameManager.GetDebug())
                    {
                        Debug.Log(this.gameObject.name + " is calling ultimate on " + s_GameManager.Singleton.Players[i].name);
                    }
                    myUltimate.ActivateUltimate(s_GameManager.Singleton.Players[i]);
                }
            }
        }
    }


    public int GetColumnCount() {
        return MyTileSpawner.GetColumnCount();
    }
    public int GetRowCount()
    {
        return MyTileSpawner.GetRowCount();
    }
    protected void actionsBasedOnPlayerState()
    {
        switch (currentState)
        {
            case PlayerStates.FALLING:
                haveEverythingFall();//This state exits when all of the s_blocks are in the stopped state  it exits into the  matching state
                break;
            case PlayerStates.MATCHING:
                checkTileBasedOnQueue(); //This state is where s_player runs through all the recently stopped moving s_blocks and checks their coorsponding s_tile to see if any matches have been made from the s_block stopping there.  If there have been it matches them all destroys them, and goes into the falling state  then continues the cycle.  If no matches are found then it exits to the moving state.
                break;
            case PlayerStates.MOVING:
                checkIfAllTilesStopped();//This state exits when all of the s_blocks are in the stopped state  it exits into the  matching state
                break;
            case PlayerStates.TRASHFALLING:
                checkIfAllTrashStopped();
                break;
            default:
                break;
        }
    }
    public void SetFoundValidMatch(bool val) {
        foundValidMatch = val;
    }
    public PlayerStates GetCurrentState() {
        return currentState;
    }
    public void AddToCheckList(s_Tile t) {
        if ((t.GetContents() != s_Tile.Contents.NONE && t.GetContents() != s_Tile.Contents.TRASH))
        {
            if (!toCheckTiles.Contains(t))
            {
                toCheckTiles.Add(t);
            }
        }
    }

    //gives the s_player a refereance to the current s_Tetris block
    public virtual void SetTetrisBlock(s_TetrisBlock currentBlock) {
        myTetrisBlock = currentBlock;
    }
    public s_TetrisBlock GetTetrisBlock() {
        return myTetrisBlock;
    }

    public bool WillBlockBeOutOfBounds(s_Tile val)
    {

        float dist = myTetrisBlock.GetHighestAndLowestDifferance();
        if (val.transform.position.y + dist > myTiles[myTiles.Count - 1].transform.position.y + tileYOffset)
        {
            return true;
        }

        return false;
    }

    public s_BlockSpawner.nextBlockStruct GetNextTetrisBlockType() {
        return MyBlockSpawner.GetNextTetrisBlockType();
    }

    protected void setChosenCharacter(s_ChooseGameAndPlayerType.character val) {
        if (ChosenCharacter != null)
        {
            ChosenCharacter.text = "Player " + Player.ToString().ToLower() + " is " + val.ToString().ToLower();
        }
    }

    void handleTrashAdding() {
        if (trashToAddToOther > -1)
        {
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Combo Length of " + comboCount.ToString() + " with total blocks matched equaling: " + totalBlocksMatched.ToString() + " " + trashToAddToOther + " is the trash to add");
            }
            s_GameManager.Singleton.AddTrash(this, trashToAddToOther);
            trashToAddToOther = 0; //resets the trash to add to the other side
        }
    }

    void clearAdjacentTrash(List<s_Tile> matchBeingCleared) {
        for (int i = 0; i < matchBeingCleared.Count; i++)
        {
            for (int j = 0; j < matchBeingCleared[i].adjacentTiles.Length; j++)
            {
                s_Tile tempTile = matchBeingCleared[i].adjacentTiles[j];
                if (tempTile != null &&
                    tempTile.GetContents() == s_Tile.Contents.TRASH)
                {
                    if (tempTile.GetUpTile() != null &&
                        tempTile.GetUpTile().GetContents() != s_Tile.Contents.NONE &&
                        tempTile.GetUpTile().GetContents() != s_Tile.Contents.TRASH)
                    {
                        toCheckSBlocks.Add(tempTile.GetUpTile().GetTileOccupant());
                    }
                    matchBeingCleared[i].adjacentTiles[j].ClearTile();
                }
            }
        }
    }
    public void QueueTrash(int amountToQueue) {
        queuedTrash += amountToQueue;
        if (myQueuedTrash != null)
        {
            myQueuedTrash.SetTrashDisplay(queuedTrash);
        }
        if (s_GameManager.GetDebug())
        {
            if (this != null)
            {
                Debug.Log("Queued trash is at " + queuedTrash + " for player " + gameObject.name);
            }
        }

    }
    void spawnQueuedTrashDuringQueuePhase() {

        int amountToSpawn = maxTrashToSpawnPerPhase > queuedTrash ? queuedTrash : maxTrashToSpawnPerPhase;
        if (queuedTrash > 10)
        {
            for (int j = 0; j < amountToSpawn; j++)
            {
                queuedTrash--;
                myQueuedTrash.SetTrashDisplay(queuedTrash);
                MyBlockSpawner.SpawnTrash(j);
            }
        }

        currentState = PlayerStates.TRASHFALLING;//the state that allows trash to spawn, i.e. not matching, falling, or moving
    }

    protected void UseUltimate()
    {
        if (ultimateCharge == ultimateMax)
        {
            stats_UltimatesUsed++;
            ultimateCharge = 0;
            if (MyMeter != null)
            {
                MyMeter.SetBar(ultimateCharge, ultimateMax);
            }
            if (myUltIcon != null)
            {
                myUltIcon.SetActive(false);
            }
            callUltimateOnOther();
        }

    }
    public void AddToUltimateCharge(int amountToAdd) {
        if (s_GameManager.Singleton.GetPauseState())
        {
            return;
        }
        stats_UltimatePointsEarned += amountToAdd;
        ultimateCharge += amountToAdd;
        if (ultimateCharge > ultimateMax)
        {
            ultimateCharge = ultimateMax;
            if (myUltIcon != null && !myUltIcon.GetActive())
            {
                myUltIcon.SetActive(true);
            }
        }
        if (MyMeter != null) {
            MyMeter.SetBar(ultimateCharge, ultimateMax);
        }

    }
    public int GetUltimateCharge()
    {
        return ultimateCharge;
    }

    void checkInputForUltimate() {
        if (s_GameManager.Singleton.GetPauseState())
        {
            return;
        }
        if (Player == PlayerNumber.ONE)
        {
            if (Input.GetKeyUp(KeyCode.LeftControl))
            {
                UseUltimate();
            }
        }

        if (Player == PlayerNumber.TWO)
        {
            if (Input.GetKeyUp(KeyCode.RightControl))
            {
                UseUltimate();
            }
        }
    }

    void setCharachterType() {
        if (s_ChooseGameAndPlayerType.Singleton != null)
        {
            s_ChooseGameAndPlayerType.character character;

            if (Player == PlayerNumber.ONE)
            {
                character = s_ChooseGameAndPlayerType.Singleton.GetPlayerOne();
            }
            else
            {
                character = s_ChooseGameAndPlayerType.Singleton.GetPlayerTwo();
            }
            myAnimationQueue = GetComponent<s_AnimationQueue>();
            addUltimateScript(character);
            setPlayerAvatar(character);
            setChosenCharacter(character);
            setUltIcon(s_GameManager.Singleton.GetUltIcon(character));
        }
    }

    protected void addUltimateScript(s_ChooseGameAndPlayerType.character charType) {

        switch (charType)
        {
            case s_ChooseGameAndPlayerType.character.CHOIR:
                gameObject.AddComponent(typeof(s_Ultimate_Choir));
                break;
            case s_ChooseGameAndPlayerType.character.ARTIST:
                gameObject.AddComponent(typeof(s_Ultimate_Artist));
                break;
            case s_ChooseGameAndPlayerType.character.SOFTBALL:
                gameObject.AddComponent(typeof(s_Ultimate_Softball));
                break;
            case s_ChooseGameAndPlayerType.character.STRESSED:
                gameObject.AddComponent(typeof(s_Ultimate_Stressed_Student));
                break;
            default:
                Debug.LogError("Error, unknown character!");
                break;
        }

    }

    protected void destroyPlayer() {
        s_GameManager.Singleton.RemovePlayer(this);
        Destroy(gameObject);
    }

    public float GetScaleFactor() {
        return scaleFactor;
    }

    public float GetAspectRatioScaleFactor() {
        return arFactor;
    }
    public void SetAspectRatioScaleFactor(float val) {
        arFactor = val;
    }

    void setPlayerAvatar(s_ChooseGameAndPlayerType.character charType) {
        Vector3 spawnPos = new Vector3();
        myCharacter = charType;
        if (Player == PlayerNumber.ONE)
        {
            spawnPos = transform.position + new Vector3(2.6f, 1.2f, 0);
        }
        else
        {
            spawnPos = transform.position + new Vector3(-1.0f, 1.2f, 0);
        }

        switch (s_ChooseGameAndPlayerType.Singleton.GetSelectedStage())
        {
            case s_ChooseGameAndPlayerType.SCENETOLOAD.JCScene:
                break;
            case s_ChooseGameAndPlayerType.SCENETOLOAD.StageScene:
                spawnPos += new Vector3(0, 0, 0.5f);
                break;
            case s_ChooseGameAndPlayerType.SCENETOLOAD.LibraryScene:
                break;
            case s_ChooseGameAndPlayerType.SCENETOLOAD.SoftballScene:
                break;
            case s_ChooseGameAndPlayerType.SCENETOLOAD.StatueScene:
                break;
            case s_ChooseGameAndPlayerType.SCENETOLOAD.InsideArtBuilding:
                Debug.Log("Missing case");
                break;
            default:
                Debug.LogError("Unknown scene cann base avatar positions off of scene");
                break;
        }

        if (myAnimationQueue != null && !deathFlag)
        {
            GameObject obj = null;
            switch (charType)
            {
                case s_ChooseGameAndPlayerType.character.CHOIR:
                    obj = s_GameManager.Singleton.GetMusicStudent(spawnPos);
                    break;
                case s_ChooseGameAndPlayerType.character.ARTIST:
                    obj = s_GameManager.Singleton.GetArtistStudent(spawnPos);
                    break;
                case s_ChooseGameAndPlayerType.character.SOFTBALL:
                    obj = s_GameManager.Singleton.GetSoftballStudent(spawnPos);
                    break;
                case s_ChooseGameAndPlayerType.character.STRESSED:
                    obj = s_GameManager.Singleton.GetStressedStudent(spawnPos + new Vector3(0, 0, 0.34f));
                    break;
                default:
                    Debug.LogError("Error, unknown character!");
                    break;
            }
            if (obj != null)
            {
                obj.transform.SetParent(transform.parent);
            }
            if (Player != PlayerNumber.ONE)
            {
                if (obj != null)
                {
                    obj.transform.localEulerAngles = new Vector3(0, 180, 0);
                }
            }

            if (charType == s_ChooseGameAndPlayerType.character.CHOIR)
            {
                if (obj != null)
                {
                    obj.transform.localEulerAngles += new Vector3(0, 180, 0);
                }
            }


            myAnimationQueue.SetAnimator(this, obj);

        }
    }

    void setUltIcon(Sprite spriteToSetIconTo) {
        if (myUltIcon != null)
        {
            myUltIcon.SetSprite(spriteToSetIconTo);
        }
    }
    void AddAnimations(int comboNum) {
        if (myAnimationQueue != null)
        {

            myAnimationQueue.AddAnimation(s_AnimationQueue.Animations.ATTACK1);

        }
    }

    public float GetRandomTileXpos()
    {
        int val = Random.Range(0, GetColumnCount());
        return myTiles[val].transform.position.x;
    }
    public float GetRandomTileYpos()
    {
        int val = Random.Range(0, GetRowCount());
        return myTiles[val * GetColumnCount()].transform.position.y;
    }
    public s_Tile GetRandomTile() {
        return myTiles[Random.Range(0, myTiles.Count)];
    }
    public GameObject GetAvatar() {
        if (myAnimationQueue != null)
        {
            return myAnimationQueue.GetAvatar();
        }
        else
        {
            return null;
        }
    }
    public s_ChooseGameAndPlayerType.character GetMyCharacter() {
        return myCharacter;
    }
    public int GetTrashCount() {
        return trashCount;
    }
    void convertSblockToCheckToTilesToCheck() {
        for (int i = 0; i < toCheckSBlocks.Count; i++)
        {

            if (toCheckSBlocks[i] != null)
            {
                if (toCheckSBlocks[i].GetCurrentState() == s_Block.states.STOPPING)
                {
                    toCheckSBlocks[i].AddSBlockToCheck();
                }
            }

        }
    }



    public void PlayAttack1()
    {
        switch (myCharacter)
        {
            case s_ChooseGameAndPlayerType.character.CHOIR:
                break;
            case s_ChooseGameAndPlayerType.character.ARTIST:
                AudioControlerScript.Singleton.PlayBrushNormal();
                break;
            case s_ChooseGameAndPlayerType.character.SOFTBALL:
                break;
            case s_ChooseGameAndPlayerType.character.STRESSED:
                break;
            default:
                break;
        }
    }
    public void PlayAttack2()
    {
        switch (myCharacter)
        {
            case s_ChooseGameAndPlayerType.character.CHOIR:
                break;
            case s_ChooseGameAndPlayerType.character.ARTIST:
                break;
            case s_ChooseGameAndPlayerType.character.SOFTBALL:
                break;
            case s_ChooseGameAndPlayerType.character.STRESSED:
                break;
            default:
                break;
        }
    }

   
    public void AddToScore(int valToAdd) {
       
        if (valToAdd > 0)
        {
            score += valToAdd;
        }
    }
    public int GetScore() {
        return score;
    }
    /*
    // This is for camera shaking
    public void UltimateShake()
    {
        
        StartCoroutine(cameraShake.Shake(0.15f, 0.4f));
    }
    */

    public s_GameManager.PlayerData CreatePlayerDataStructOfPlayer() {
        s_GameManager.PlayerData pd = new s_GameManager.PlayerData();
         pd.score = score;
        pd.BlocksMatched = stats_BlocksMatched;
        pd.CombosMade = stats_CombosMade;
        pd.TrashDumpedOnOther = stats_TrashDumpedOnOther;
        pd.UltimatePointsEarned = stats_UltimatePointsEarned;
        pd.UltimatesUsed = stats_UltimatesUsed;
        pd.Roll = Player;
        pd.Name = myCharacter;
        if (s_GameManager.GetDebug())
        {
            Debug.Log("Player Datat Created for " + pd.Roll.ToString() + " with name " + pd.Name);
        }
        return pd;
    }
}
