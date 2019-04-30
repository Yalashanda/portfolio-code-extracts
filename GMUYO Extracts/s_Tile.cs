using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_Tile : MonoBehaviour
{
    //checks at intervals to make sure there are no hanging s_blocks
    Timer checkBelow = new Timer();
    Timer shouldTileBeEmpty = new Timer(0.08f);
    public int columnIn;
    public int rowIn;
    bool checkedAdjacentTiles = false;//used to avoid infinite loops by falggin tiles that have already check adjacent tiles
    //allows for multiple players by having each player be based on a player class that then has other classes/object that are linked to an instance of player
    s_Player owningPlayer = null;
    //the enum used to describe what type of block a tile contains
    public enum Contents {
        NONE,
        TRASH,
        RED,
        GREEN,
        BLUE,
        ORANAGE
    }
    
    //the positsion in the list of tiles in tile spawner that this tile occupies
    public int tilePosition = 0;

    //used to match  the array with the proper direction for human readabilty
    enum tileDirection {
        LEFT,
        RIGHT,
        UP,
        DOWN
    }

    //array that holds referances to all adjacent tile
    public s_Tile[] adjacentTiles = new s_Tile[4];

    public Contents content = Contents.NONE; //the content of the tile, i.e. empty or color of block that occupies it, used for checking matches
    public s_Block occupant = null; //the block that occupies the square used to clear the block in cases of match

    //match checking variables
    public s_Tile checkedBy = null; //the tile that checked this tile to see if it matched used to avoid adding the same tile multipule times to the matches list
    public List<s_Tile> sameColor = new List<s_Tile>(); // a list of the adjacent tiles to this one that are the same color, excludes the one that initally checked it

 
    //Sets the corresponding direction to the tile passed in
    public void SetLeftTile(s_Tile val) {
        adjacentTiles[(int)tileDirection.LEFT] = val;
    }
    public void SetRightTile(s_Tile val)
    {
        adjacentTiles[(int)tileDirection.RIGHT] = val;
    }
    public void SetUpTile(s_Tile val)
    {
        adjacentTiles[(int)tileDirection.UP] = val;
    }
    public void SetDownTile(s_Tile val)
    {
        adjacentTiles[(int)tileDirection.DOWN] = val;
    }

    //Gets the corresponding tile to the [direction] of this tile
    public s_Tile GetLeftTile() {
        return adjacentTiles[(int)tileDirection.LEFT];
    }
    public s_Tile GetRightTile()
    {
        return adjacentTiles[(int)tileDirection.RIGHT];
    }
    public s_Tile GetUpTile()
    {
        return adjacentTiles[(int)tileDirection.UP];
    }
    public s_Tile GetDownTile()
    {
        return adjacentTiles[(int)tileDirection.DOWN];
    }

    //Get the type of block currently in this tile
    public Contents GetContents() {

        return content;
    }
    public void SetContents(Contents blockType) {
        content = blockType;
    }

    //returns the position of the tile in in the master spawner list
    public int GetTilePosition() {
        return tilePosition;
    }
    //Sets the tile position variable
    public void SetTilePosition(int val)
    {
        tilePosition = val;
    }

    public void SetColumnAndRowPositions() {
        columnIn = tilePosition % owningPlayer.GetColumnCount();
        rowIn = (((tilePosition - (tilePosition % owningPlayer.GetColumnCount())) / owningPlayer.GetColumnCount()));
    }
    //gives the tile a referance to the block occupying it
    public void SetTileOccupant(s_Block myBlock) {
        occupant = myBlock;
    }
    public s_Block GetTileOccupant() {

        return occupant;
    }

    //clears the tile, block and tile based variables
    public void ClearTile() {
        checkedBy = null;
        content = Contents.NONE;
        clearOccupant();
        checkedAdjacentTiles = false;
        for (int i = 0; i < sameColor.Count; i++)
        {
            sameColor[i].checkedBy = null;
        }
        sameColor.Clear();
    }
    //clears the block from the tile
    void clearOccupant() {
        if (occupant != null)
        {
            occupant.Die();
            occupant = null;
        }
        else
        {
            if (s_GameManager.GetDebug())
            {
                Debug.Log("Occupant of tile is already set to null and cannot be cleared.  This may be an error.  If game contiues to work please ignore.");
            }
        }
    }

    //resets tiles after unsuccessful match check
    public void ClearNoMatch() {
        checkedBy = null;
        checkedAdjacentTiles = false;
        for (int i = 0; i < sameColor.Count; i++)
        {
            sameColor[i].checkedBy = null;
        }
        sameColor.Clear();
    }

    //adds all the tiles above this one that are not empty toa que to fall. e.g. after a match has been made all the above tiles need to start falling
    public void SetColumnForFalling() {
        if (GetUpTile() != null && GetUpTile().content != Contents.NONE)
        {
            GetUpTile().SetColumnForFalling();
            owningPlayer.AddToFallColumns(GetUpTile());
            
        }
    }
    //compares the contents of the tile passed in with this tile's content returns true if they match
    bool compareTileContenets(s_Tile toCheck) {

        if (toCheck.GetTileOccupant() != null)// if there is a block in this toCheck tile
        {
            if (toCheck.content != toCheck.GetTileOccupant().GetBlockType()) // make sure the toCheck tile has the same content as the block
            {
                toCheck.content = toCheck.GetTileOccupant().GetBlockType(); //if toCheck does not then set them to match
            }
        }
        else
        {
            if (toCheck.content != Contents.NONE)// if the toCheck tile is empty make sure it registers as empty
            {
                toCheck.content = Contents.NONE;
            }
        }
        if (toCheck.content == content)// if the content of the tile that is being check
        {
            if (checkedBy == null || checkedBy != toCheck)
            {
                toCheck.SetCheckedBy(this);
                if (!sameColor.Contains(toCheck))
                {
                    sameColor.Add(toCheck);//adds the tile to thge adjacent tiles that are the same color and will need to also be checked for adjacent tiles              
                }
                owningPlayer.AddMatch(toCheck);
                
                return true;
            }
            else
            {
                if (s_GameManager.GetDebug())
                {
                    Debug.Log("Checked by was not valid for " + gameObject.name);
                }
            }
            
        }
        return false;
    }

    public int CheckMatch() {
        int tempMatchCountHolder = 0;
        if (occupant != null)
        {
            if (content != occupant.GetBlockType())
            {
                content = occupant.GetBlockType();
            }
        }
        else
        {
            content = Contents.NONE;
        }

        if (content == Contents.NONE) //makes sure not to call a match check on an empty tile
        {
            return tempMatchCountHolder;
        }
        checkedAdjacentTiles = true;

        for (int i = 0; i < adjacentTiles.Length; i++)
        {
            if (adjacentTiles[i] != null)//checks to exclude nonexistent tiles, i.e. tiles on the edges do not have things on at least on side
            {
                if (!adjacentTiles[i].GetCheckedAdjacentTiles())
                {
                    if (s_GameManager.GetDebug()) {
                        adjacentTilesDebugMessage(i);
                    }
                    
                    compareTileContenets(adjacentTiles[i]);// checks each adjacent tile to see if it should be added to list of adjacent matching color tiles
                }
            }
        }
      

        for (int i = 0; i < sameColor.Count; i++)
        {

            sameColor[i].CheckMatch(); //for each adjacent tile that matches in color this function is called spiralling out to get all adjacent tiles
            
            
        }
        
        //if this tile was not checked by any tile i.e. was the first tile (and thus all adjacent tiles and adjacent adjacent tiles etc. have been checked) then perform this code block
        if (checkedBy == null)
        {
            owningPlayer.AddMatch(this);
            tempMatchCountHolder = owningPlayer.GetMatchesList().Count;
            if (owningPlayer.GetMatchesList().Count >= owningPlayer.GetMatchNumNeeded()) // if there are at least four adjacent tiles
            {
                if (s_GameManager.GetDebug())
                {
                    Debug.Log("MATCH FOUND WITH " + owningPlayer.GetMatchesList().Count);
                }
                
                owningPlayer.SetFoundValidMatch(true);
                owningPlayer.SuccessfulMatchClear(); //commence the successeful clear funtions
                
            }
            else
            {
                owningPlayer.ResetMatches(); //reset and prepare for another check
            }
            owningPlayer.ReadyToCheck = true;
            

        }
        return tempMatchCountHolder;
    }

    void adjacentTilesDebugMessage(int i)
    {
        //debug test messages
        string message = occupant.gameObject.name + " is checking ";
        if (adjacentTiles[i].occupant != null)
        {
            message += adjacentTiles[i].occupant.gameObject.name;
        }
        else
        {
            message += "empty tile to " + ((tileDirection)i).ToString();
        }

        if (compareTileContenets(adjacentTiles[i]))
        {
            message += " it was the same color";
        }
        else
        {
            message += " it was not the same color";
        }
        Debug.Log(message);
    }

    public void SetOwningPlayer(s_Player player)
    {
        owningPlayer = player;
        
    }
    public s_Player GetOwningPlayer() {
        return owningPlayer;
    }
    //sets the occupant to fall and tile to empty
    public void SetContentsToFall() {
        if (occupant != null)
        {
            int jumpAmount = 0;
            foreach (s_Tile T in owningPlayer.GetTileList())
            {
                if (T != this && T.GetColumnIn() == GetColumnIn() && T.transform.position.y < transform.position.y)
                {
                    jumpAmount++;
                }
            }
            occupant.transform.position += new Vector3(0, 0.05f * jumpAmount, 0);
            //* (tilePosition / owningPlayer.GetColumnCount()
            occupant.StartFalling();
            occupant = null;
        }
        content = Contents.NONE;
        
    }

    public void SetCheckedBy(s_Tile tileThatIsChecking) {
        checkedBy = tileThatIsChecking;
    }

    public int GetColumnIn() {
        return columnIn;
    }
    public int GetRowIn()
    {
        return rowIn;
    }

    public s_Tile FindFreeTileInColumn() {
        if (GetUpTile() != null)
        {
            if (GetUpTile().occupant == null)
            {
                return GetUpTile();
            }
            else
            {
                if (s_GameManager.GetDebug())
                {
                    Debug.Log("Checking " + GetUpTile().GetUpTile().name);
                }
                return GetUpTile().FindFreeTileInColumn();
            }
        }
        return null;
    }

    public bool GetCheckedAdjacentTiles() {
        return checkedAdjacentTiles;
    }

    //function used to avoid hanging s_Blocks
    void checkIfLowerTileEmpty() {
        if (occupant != null && GetDownTile() != null)
        {
            if (checkBelow.CountDown())
            {
                if (GetDownTile().content == Contents.NONE)
                {
                    occupant.StartFalling();
                }
            }
        }
    }


    void checkIfTileShouldBeSetToEmpty() {

        if (shouldTileBeEmpty.CountDown())
        {
            if (occupant == null)
            {
                if (content != Contents.NONE)
                {
                    content = Contents.NONE;
                }
            }
        }
        
    }
    //.returns true if the tile below this is either the last tile in a column or is full
    public bool CanSupportBlock() {       
        return GetDownTile() == null || GetDownTile().GetContents() != Contents.NONE;
    }


    void Start() {
        checkBelow.SetTimer(0.5f);
        
    }
    // Update is called once per frame
    void Update()
    {
        checkIfLowerTileEmpty();
        checkIfTileShouldBeSetToEmpty();
    }

    public void OverrideContents(s_Tile.Contents toOverRideWith) {
        if (GetTileOccupant() != null)
        {
            GetTileOccupant().SetBlockType(toOverRideWith);
        }
        else
        {
            owningPlayer.MyBlockSpawner.SpawnTrash(transform.position);
        }



    }

}
