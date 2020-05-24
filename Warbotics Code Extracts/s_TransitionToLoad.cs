using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_TransitionToLoad : MonoBehaviour
{
    public Canvas canvas;
    public s_TransitionEffectAbstract[] MyTransitions;
    [SerializeField]
    Sprite[] m_mapSprites;
    [SerializeField]
    Sprite[] m_levelTextSprites;
    [SerializeField]
    Material[] m_levelBackgroundSprites;
    public Sprite[] loadScreens;
    public Sprite BlankFade;
    public s_FancyControllerScreenWrapper FancyTransition;
    public s_colorfulTransitionEffect ColorfulBackgroundTransition;
    // Start is called before the first frame update
    void Awake()
    {
        s_Transition myTransitionScript = FindObjectOfType<s_Transition>();
        if (myTransitionScript != null)
        {
            if ((int)myTransitionScript.GetTransitionType() >= (int)s_Transition.TransitionType.HORZONTALLEFT && (int)myTransitionScript.GetTransitionType() <= (int)s_Transition.TransitionType.RADIAL360RIGHT)
            {
                GameObject go = Instantiate(MyTransitions[2].gameObject, canvas.transform, false);
                s_RadialTransitions srt = go.GetComponent<s_RadialTransitions>();
                srt.SetImage(s_Calculator.GetRandFromArray(loadScreens));
                srt.SetParameters(myTransitionScript.GetTransitionType(), myTransitionScript.GetPlayerInputForTransition(), myTransitionScript.GetDelaySecondsAfterLoadComplete());

            }
            else
            {
                    int adjusment = ((int)myTransitionScript.GetTransitionType() < (int)s_Transition.TransitionType.HORZONTALLEFT ? 0 : (s_Transition.TransitionType.RADIAL360RIGHT - s_Transition.TransitionType.HORZONTALLEFT));

                GameObject go = null;
                s_TransitionEffectAbstract srt = null;
                if (myTransitionScript.GetTransitionType() != s_Transition.TransitionType.FANCYCONTROLLER && myTransitionScript.GetTransitionType()  != s_Transition.TransitionType.MAPTRANSITION)
                {
                    FancyTransition.DeactivateImage(4, false);
                    Destroy(FancyTransition.gameObject);

                    go = Instantiate(MyTransitions[(int)myTransitionScript.GetTransitionType() - adjusment].gameObject, canvas.transform, false);
                    srt = go.GetComponent<s_TransitionEffectAbstract>();
                }


                switch (myTransitionScript.GetTransitionType())
                {
                    case s_Transition.TransitionType.FADEBLANK:
                        srt.SetImage(BlankFade);
                        break;
                    case s_Transition.TransitionType.FANCYCONTROLLER:
                        FancyTransition.DeactivateImage(6, false);
                        FancyTransition.gameObject.SetActive(true);
                        FindObjectOfType<s_DisplayTierBar>().GetComponent<UnityEngine.UI.Image>().enabled = false;
                        break;
                    case s_Transition.TransitionType.MAPTRANSITION:
                        FancyTransition.SetFadeInImage(1, GetTransitionImageFromLevel(m_mapSprites), new Vector3(1.5f, 1.5f, 1.0f));
                        FancyTransition.SetFadeInImage(2, GetTransitionImageFromLevel(m_levelTextSprites));
                        FancyTransition.DeactivateImage(0, false);
                        FancyTransition.DeactivateImage(5, false);
                        ColorfulBackgroundTransition.SetMaterial(GetTransitionImageFromLevel(m_levelBackgroundSprites));
                        ColorfulBackgroundTransition.gameObject.SetActive(true);
                        FancyTransition.gameObject.SetActive(true);
                        FindObjectOfType<s_DisplayTierBar>().GetComponent<UnityEngine.UI.Image>().enabled = false;
                        break;
                    case s_Transition.TransitionType.FADE:
                    case s_Transition.TransitionType.FADEWAITFORUSER:
                    default:
                        srt.SetImage(s_Calculator.GetRandFromArray(loadScreens));
                        break;
                }                    
            }

        }
        else
        {
            Debug.LogWarning("No transition script found.  This may be intentional if you running tests on transitions in the transition scene.  If this is the case ignore this warning message.");
        }
        
    }
    public T GetTransitionImageFromLevel<T>(T[] _spritesToChooseFrom)
    {
        
        int index = (int)((_spritesToChooseFrom.Length - 1) * 0.5f) + s_GameManager.Singleton.GetTransitionIndexModifier();
      
        if (index < _spritesToChooseFrom.Length && index > -1)
            return _spritesToChooseFrom[index];
        if (_spritesToChooseFrom.Length < 1)
        {
            Debug.LogError("Transition sprite to choose is out of range");
            return default;
        }
        return _spritesToChooseFrom[0];
        
    }    
}
