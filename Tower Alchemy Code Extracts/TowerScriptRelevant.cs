using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TowerScriptRelevant : MonoBehaviour {
    // The functions in TowerScript.cs that are called in CombineTowersScripts.cs

    public GameStateScript.TowerType Type;
    int combineRange = 2;
    bool draggingConnector = false;
    bool connected = false;
    Vector3 fixedPoint = new Vector3(0, 0, 0);
    public bool isConnected = false;
    public GameObject InputOne = null;
    public GameObject InputTwo = null;
    GameObject towerVFX;




    public void GetInputs(out GameObject i1, out GameObject i2)
    {
        i1 = InputOne;
        i2 = InputTwo;

    }

    public void SetTowerType(GameStateScript.TowerType newType)
    {
        Type = newType;
    }
    public GameStateScript.TowerType GetTowerType()
    {
        return Type;
    }
    public bool GetDraggingConnector()
    {
        return draggingConnector;
    }
    public void SetDraggingConnector(bool val)
    {
        draggingConnector = val;
    }

    public void OnConnecet(TowerScript tower)
    {
        tower.GetComponent<CombineTowersScripts>().SetInput(GameStateScript.GameState.GetHeldTower());
        isConnected = true;
        connected = true;
        fixedPoint = tower.gameObject.transform.position;
        Destroy(towerVFX);
    }

    public int GetCombineRange()
    {
        return combineRange;
    }
}
