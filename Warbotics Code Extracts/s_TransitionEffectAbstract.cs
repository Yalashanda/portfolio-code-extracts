using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class s_TransitionEffectAbstract : MonoBehaviour
{
   
    public delegate void CallToSceneSwapEventHandler(s_Transition.e_transitionStates state);
    public static event CallToSceneSwapEventHandler SwitchToTransitionState;


    
    [Tooltip("How long the transition will take in seconds")]
    public float TransitionTime = 1.0f;
    protected float TransitionRate = 1.0f;
    private void OnEnable()
    {
        s_Transition.OnTransitionChange += OnTransitionShift;

    }

    private void OnDisable()
    {
        s_Transition.OnTransitionChange -= OnTransitionShift;
    }

    protected virtual void onStart() {
        TransitionRate = 1 / TransitionTime;
    }
    //overide in the specific effects to have them behave appropriately
    public virtual void OnTransitionShift(s_Transition.e_transitionStates state) {
        switch (state)
        {
            case s_Transition.e_transitionStates.NULL:
                break;
            case s_Transition.e_transitionStates.StartTransition:
                break;
            case s_Transition.e_transitionStates.WaitingForTransitionSceneToLoad:
                break;
            case s_Transition.e_transitionStates.WaitingForTransitionToReachUnloadPoint:
                break;
            case s_Transition.e_transitionStates.WaitingForSceneSwap:
                break;
            case s_Transition.e_transitionStates.StartSceneSwap:
                break;
            case s_Transition.e_transitionStates.TransitionEnd:
                break;
        }

    }


    //goes in Update
    protected virtual void behaviorByState() {
    }

    protected void callTranstionChangeStateEvent(s_Transition.e_transitionStates newState) {
        SwitchToTransitionState?.Invoke(newState);
    }

    public virtual void SetImage(Sprite var) {

    }
    
}
