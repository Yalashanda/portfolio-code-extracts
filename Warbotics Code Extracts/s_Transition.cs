using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class s_Transition : MonoBehaviour
{
    [SerializeField]
    GameObject ContinueBlinker;
    [SerializeField]
    float DelaySecondsAfterLoadComplete = 2.0f;
    public delegate void TransitionStateEventHandler(e_transitionStates state);
    public static event TransitionStateEventHandler OnTransitionChange;
    public delegate void TransitionEndEventHandler();
    public static event TransitionEndEventHandler OnTransitionEnd;
    bool transitioning = false;
    bool resetsCalled = false;
    TransitionType typeOfTransitionEffectToLoad = TransitionType.FADEBLANK;

    public enum TransitionType
    {
        FADEBLANK,
        FADE,
        FADEWAITFORUSER,
        HORZONTALLEFT,
        HORZONTALRIGHT,
        VERTICALBOTTOM,
        VERTICALTOP,
        RADIAL90BOTTOMLEFT,
        RADIAL90BOTTOMRIGHT,
        RADIAL90TOPLEFT,
        RADIAL90TOPRIGHT,
        RADIAL180TOP,
        RADIAL180BOTTOM,
        RADIAL180LEFT,
        RADIAL180RIGHT,
        RADIAL360TOP,
        RADIAL360BOTTOM,
        RADIAL360LEFT,
        RADIAL360RIGHT,
        FANCYCONTROLLER,
        MAPTRANSITION
    }

    public enum e_transitionStates {
        NULL,
        StartTransition,
        WaitingForTransitionSceneToLoad,
        WaitingForTransitionToReachUnloadPoint,
        WaitingForSceneSwap,
        WaitingForTransitionVFXToEnd,
        StartSceneSwap,
        TransitionEnd

    }
    e_transitionStates currentState = e_transitionStates.WaitingForTransitionSceneToLoad;
    string toUnload;
    string toLoad;
    string transitionScene = "SC_Transition";
    bool playerInputForTransition = true;
    Scene toTrans;

    private void OnEnable()
    {
        s_TransitionEffectAbstract.SwitchToTransitionState += changeState;

    }

    private void OnDisable()
    {
        s_TransitionEffectAbstract.SwitchToTransitionState -= changeState;
    }


    public void LoadMainMenu()
    {

        unload();
        
        SceneManager.LoadSceneAsync("SC_Menu_Main", LoadSceneMode.Additive);
        s_GameManager.Singleton.ShouldRenderUI("SC_Menu_Main");
    }

    public void LoadCredits() {
        unload();
        SceneManager.LoadSceneAsync("SC_Credits", LoadSceneMode.Additive);
        s_GameManager.Singleton.ShouldRenderUI("SC_Credits");
    }
    void unload()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            if (SceneManager.GetSceneAt(i).name != transitionScene && SceneManager.GetSceneAt(i) != gameObject.scene && SceneManager.GetSceneAt(i).name != "SC_Persistent")
            {
                SceneManager.UnloadSceneAsync(SceneManager.GetSceneAt(i).buildIndex);
            }
        }
    }
    public void StartTransition(string toLoadName, string toUnloadName, TransitionType type, bool transitionWaitForPlayerInput) {
        bool check1 = IsValidLevelName(toLoadName);
        bool check2 = IsValidLevelName(toUnloadName);
        playerInputForTransition = transitionWaitForPlayerInput;
        if (check1 && check2 && !transitioning)
        {
            s_GameManager.Singleton.ShouldRenderUI(toLoad);
            s_GameManager.Singleton.SetAcceptPlayerInput(false);
            transitioning = true;
            toLoad = toLoadName;
            toUnload = toUnloadName;
            typeOfTransitionEffectToLoad = type;
            changeState(e_transitionStates.StartTransition);
            foreach (s_Player p in s_GameManager.Singleton.GetPlayersList())
            {
                p.ChangeState(s_Player.States.Invul, 5);
            }
        }
    }
    public void StartTransition(string toLoadName, string toUnloadName)
    {
        bool check1 = IsValidLevelName(toLoadName);
        bool check2 = IsValidLevelName(toUnloadName);
        playerInputForTransition = false;
        if (check1 && check2 && !transitioning)
        {
            s_GameManager.Singleton.SetAcceptPlayerInput(false);
            s_GameManager.Singleton.ShouldRenderUI(toLoad);
            transitioning = true;
            toLoad = toLoadName;
            toUnload = toUnloadName;
            typeOfTransitionEffectToLoad = TransitionType.FADEBLANK;
            changeState(e_transitionStates.StartTransition);
            foreach (s_Player p in s_GameManager.Singleton.GetPlayersList())
            {
                p.ChangeState(s_Player.States.Invul, 5);
            }
        }
    }

    string cleanPath(string name) {
        name = name.Replace("Assets/Scenes/", "");
        name = name.Replace(".unity", "");
        return name;

    }
    public bool IsValidLevelName(string sceneToName) {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            string name = cleanPath(SceneUtility.GetScenePathByBuildIndex(i));
            if (name == sceneToName)
            {
                return true;
            }
        }

        if (sceneToName == "")
        {
            Debug.LogError("Scene name is not a valid scene name, it is blank.");

        }
        else
        {
            Debug.LogError(sceneToName + " is not a valid scene name.  There is either a typo in the scene name, or the scene " + sceneToName + " may not have been added to the build index.");
        }
        

        return false;
    }

    void loadingBehavior() {
        switch (currentState)
        {
            case e_transitionStates.NULL:
                break;
            case e_transitionStates.StartTransition:
                SceneManager.LoadSceneAsync(transitionScene, LoadSceneMode.Additive);
                changeState(e_transitionStates.WaitingForTransitionSceneToLoad);
                s_GameManager.Singleton.SetAcceptPlayerInput(false);
                break;
            case e_transitionStates.WaitingForTransitionSceneToLoad:
                if (SceneManager.GetSceneByName(transitionScene).isLoaded)
                {
                    FindObjectOfType<s_DisplayTierBar>().SetIcon(toLoad);
                    
                    changeState(e_transitionStates.WaitingForTransitionToReachUnloadPoint);
                }
                break;
            case e_transitionStates.WaitingForTransitionToReachUnloadPoint:
                //logic in a given s_TransitionEffectABstract child will call the switch from this state to StartSceneSwap
                break;
            case e_transitionStates.StartSceneSwap:
                SceneManager.LoadSceneAsync(toLoad, LoadSceneMode.Additive);
                SceneManager.UnloadSceneAsync(toUnload);
                changeState(e_transitionStates.WaitingForSceneSwap);
                break;
            case e_transitionStates.WaitingForSceneSwap:
                if (!SceneManager.GetSceneByName(toUnload).isLoaded && SceneManager.GetSceneByName(toLoad).isLoaded)
                {
                    
                    SceneManager.SetActiveScene(SceneManager.GetSceneByName(toLoad));
                    changeState(e_transitionStates.WaitingForTransitionVFXToEnd);
                    
                }
                break;
            case e_transitionStates.WaitingForTransitionVFXToEnd:
                if (!resetsCalled)
                {
                    s_GameManager.Singleton.StageReset();
                    s_GameManager.Singleton.GetBulletSpawner().ClearLists();
                    resetsCalled = true;
                }
               
                //exiting this state is handeled by logic in the currently being used transition effect
                //which calls the protected function callTranstionChangeStateEvent at the appropriate time in its visualization.  
                //it should only be called once the transition is done as ending the transition will unload the transition scene
                break;
            case e_transitionStates.TransitionEnd:
                s_GameManager.Singleton.SetAcceptPlayerInput(true);
                s_GameManager.Singleton.SetSceneThatIsGoingToBeLoaded(toLoad);
                transitionEnd();
                break;
            default:
                break;
        }
    }

  

    void changeState(e_transitionStates stateToChangeTo) {
        if (currentState == e_transitionStates.WaitingForTransitionVFXToEnd)
        {
            if (ContinueBlinker != null && playerInputForTransition)
                ContinueBlinker.SetActive(true);
        }
        currentState = stateToChangeTo;
        OnTransitionChange?.Invoke(currentState);
        
    }
    // Update is called once per frame
    void Update()
    {
        loadingBehavior();
    }
    
    void transitionEnd() {
        changeState(e_transitionStates.NULL);
        resetsCalled = false;
        s_GameManager.Singleton.SetAcceptPlayerInput(true);
        transitioning = false;
        if (ContinueBlinker != null)
            ContinueBlinker.SetActive(false);
        foreach (s_Player p in s_GameManager.Singleton.GetPlayersList())
        {
            p.DropWeaponOnLevelTransition();
        }
        OnTransitionEnd?.Invoke();
        SceneManager.UnloadSceneAsync(transitionScene);
        
    }

    public TransitionType GetTransitionType()
    {
        return typeOfTransitionEffectToLoad;
    }
    public bool GetPlayerInputForTransition() { return playerInputForTransition; }
    public float GetDelaySecondsAfterLoadComplete() { return DelaySecondsAfterLoadComplete; }
    public string[] GetCurrentlyLoadedSceneNames() {
        List<string> myLevelNames = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string _tempString = SceneManager.GetSceneAt(i).name;
            if (_tempString != "SC_Persistent" && _tempString != "SC_Transition")
            {
                myLevelNames.Add(_tempString);
            }
        }
        return myLevelNames.ToArray();
    }
    public bool IsSceneLoaded(string _sceneToCheckFor)
    {
        List<string> myLevelNames = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string _tempString = SceneManager.GetSceneAt(i).name;
            if (_tempString != "SC_Persistent" && _tempString != "SC_Transition")
            {
                myLevelNames.Add(_tempString);
            }
        }
        return myLevelNames.Contains(_sceneToCheckFor);

    }

    public bool IsSceneLoaded(string[] _sceneToCheckFor)
    {
        List<string> myLevelNames = new List<string>();
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            string _tempString = SceneManager.GetSceneAt(i).name;
            if (_tempString != "SC_Persistent" && _tempString != "SC_Transition")
            {
                myLevelNames.Add(_tempString);
            }
        }
        for (int i = 0; i < _sceneToCheckFor.Length; i++)
        {
            if (myLevelNames.Contains(_sceneToCheckFor[i]))
            {
                return true;
            }
        }
        return false;

    }

    public bool IsOnTransitionScreen()
    {
        return transitioning;
    }
}
