using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CombineTowersScripts : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    GameObject InputOne = null;
    GameObject InputTwo = null;
    public TowerScript myTower;
    void Start() {
        myTower = GetComponent<TowerScript>();
    }

    void Combine()
    {

        bool valid = false;
        
        myTower.GetInputs(out InputOne, out InputTwo);
        
        GameStateScript.TowerType type1 = InputOne.GetComponent<TowerScript>().Type;
        GameStateScript.TowerType type2 = InputTwo.GetComponent<TowerScript>().Type;

        switch (type1)
        {

            case GameStateScript.TowerType.LIGHT:
                if (type2 == GameStateScript.TowerType.CRYSTAL)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.LASERTOWER);
                    valid = true;
                }
                break;

            case GameStateScript.TowerType.CRYSTAL:
                if (type2 == GameStateScript.TowerType.LIGHT)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.LASERTOWER);
                    valid = true;
                }
                break;

            case GameStateScript.TowerType.FIRE:
                if (type2 == GameStateScript.TowerType.BALLISTA)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.FLAMINGBALLISTA);
                    valid = true;
                }
                if (type2 == GameStateScript.TowerType.WATER)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.STEAM);
                    valid = true;
                }
                break;

            case GameStateScript.TowerType.BALLISTA:
                if (type2 == GameStateScript.TowerType.FIRE)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.FLAMINGBALLISTA);
                    valid = true;
                }
                break;

            case GameStateScript.TowerType.WATER:
                if (type2 == GameStateScript.TowerType.FIRE)
                {
                    myTower.SetTowerType(GameStateScript.TowerType.STEAM);
                    valid = true;
                }
                break;

            default:
                break;
        }
        if (!valid)
        {
            myTower.Explode();
        }
        else
        {
            myTower.OnSpawn();
        }

    }

    public void SetInput(GameObject val)
    {

        if (myTower.InputOne == null)
        {
            myTower.InputOne = val;
        }
        else
        {
            if (myTower.InputOne != val)
            {
                if (myTower.InputTwo == null)
                {
                    myTower.InputTwo = val;
                    Combine();
                }
                else
                {
                    myTower.Explode();
                }
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (myTower.GetTowerType() != GameStateScript.TowerType.ALCHEMY)
        {
            GameStateScript.GameState.SetHeldTower(gameObject);
            myTower.SetDraggingConnector(true);
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
                if (Vector2.Distance(tow.transform.position, gameObject.transform.position) < 32 * myTower.GetCombineRange())
                {
                    maxdis = Vector2.Distance(tow.transform.position, Camera.main.ScreenToWorldPoint(Input.mousePosition));
                    tower = tow.GetComponent<TowerScript>();
                }
            }


        }



        if (tower != null)
        {
            myTower.OnConnecet(tower);
        }


        myTower.SetDraggingConnector(false);




    }



}
