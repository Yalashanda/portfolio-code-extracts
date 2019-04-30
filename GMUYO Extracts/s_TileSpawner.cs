using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_TileSpawner : MonoBehaviour
{
    //allows for multiple players by having each player be based on a player class that then has other classes/object that are linked to an instance of player
    s_Player owningPlayer = null;
    public GameObject Tile;
    //List<s_Tile> tiles = new List<s_Tile>();
    float colOffsets = 32; //column offsets
    float rowOffsets = 32; //row offsets
    public int rows = 10; //rows
    public int cols = 10; //columns
    Vector2 tileColliderSize;
    public List<float> xPositionsForTiles = new List<float>();
     
    public void OnSpawn() {
        tileColliderSize = Tile.GetComponent<BoxCollider2D>().size;
        float offset = 0.01f;
        colOffsets = (tileColliderSize.x + 0.01f + offset) * Tile.transform.localScale.x;
        rowOffsets = (tileColliderSize.y + 0.01f + offset) * Tile.transform.localScale.y;
        SpawnTiles();
        
    }

    public void SpawnTiles() {
        //spawns tiles in a grid pattern at evenly spaced intervals
        for (int j = 0; j < rows; j++)
        {
            for (int i = 0; i < cols; i++)
            {
                GameObject newTile = Instantiate(Tile, transform.position + new Vector3(colOffsets * i, rowOffsets * j, 0), Quaternion.identity); // spawns tiles at intervals
                newTile.name += " " + i.ToString() + "-" + j.ToString();
                s_Tile myTile = newTile.GetComponent<s_Tile>();
                myTile.SetOwningPlayer(owningPlayer);
                owningPlayer.AddTile(myTile);
            }
        }
        setAdjacentTiles();
        for (int i = 0; i < owningPlayer.GetTileList().Count; i++)
        {
            owningPlayer.GetTileList()[i].transform.SetParent(transform);
        }
    }
    
    void setAdjacentTiles() {
        int topRight = owningPlayer.GetTileList().Count - 1;  //the position of the tile at the top right of the grid in the list of tiles
        int current = topRight;

        
        //tells each tile what the adjacent tiles are
        for (int i = current; i > -1; i--)
        {
            owningPlayer.GetTileList()[i].SetTilePosition(i);
            owningPlayer.GetTileList()[i].SetColumnAndRowPositions();
            if (i % cols != 0)
            {
                owningPlayer.GetTileList()[i].SetLeftTile(owningPlayer.GetTileList()[i - 1]);
            }
            if (i % cols != cols - 1)
            {
                owningPlayer.GetTileList()[i].SetRightTile(owningPlayer.GetTileList()[i + 1]);
            }
            if (i < topRight - cols)
            {
                owningPlayer.GetTileList()[i].SetUpTile(owningPlayer.GetTileList()[i + cols]);
            }
            if (i >= cols)
            {
                owningPlayer.GetTileList()[i].SetDownTile(owningPlayer.GetTileList()[i - cols]);
            }

           
        }
    }
    public void SetOwningPlayer(s_Player player)
    {
        owningPlayer = player;
    }

    public float GetColOffset() {
          return colOffsets;
    }
    public float GetRowOffset()
    {
        return rowOffsets;
    }

    public float GetTileCOlliderSize() {
        return tileColliderSize.x;
    }

    public int GetColumnCount() {
        return cols;
    }

    public int GetRowCount() {
        return rows;
    }

    public List<float> GetXPositionsForTiles() {
        if (xPositionsForTiles.Count < 1)
        {
            for (int i = 0; i < cols; i++)
            {
                xPositionsForTiles.Add(owningPlayer.GetTileList()[i].transform.position.x);
            }
        }

        return xPositionsForTiles;
    }

    
}
