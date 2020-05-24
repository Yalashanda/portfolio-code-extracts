using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using WarboticsEnums;

public class s_Player : MonoBehaviour
{
    bool m_waitToFireUntilAnimationDone = false;
    bool m_useKeyboardAndMouse = false;
    GameObject KickingFoot = null;
    public class StateStorer
    {
        public States storedState;
        public float time;
        public StateStorer(States _state, float _time)
        {
            storedState = _state;
            time = _time;
        }

    }

    float statusResetValue = -1;
    public delegate void TeamDeadEventHandler(int team);
    public static event TeamDeadEventHandler OnDieHandler;
    public delegate void TeamRespawnEventHandler(int team);
    public static event TeamRespawnEventHandler OnRespawnHandler;
    public delegate void FireWeaponEventHandler(s_Player _firerer);
    public static event FireWeaponEventHandler OnPlayerFire;
    public delegate void DiedEventHandler();
    public event DiedEventHandler OnKilled;
    public delegate void PlayerInstanceRespanedEventHandler();
    public event PlayerInstanceRespanedEventHandler OnRespawned;
    public delegate void WeaponEquippedEventHandler(s_Weapon _weapon);
    public event WeaponEquippedEventHandler OnWeaponEquipped;
    public bool DrawDirectionVectors = false;
    public GameObject AvatarGameObject;
    public Color PlayerColor;
    bool isAI = false;
    [SerializeField]
    int spawnIndex = 0;
    s_HoldWeaponHand hands = null;
    Vector3 startPos = new Vector3();
    Timer DropButtonHoldTime = new Timer(0.1f);
    Timer SpawnKickPuff = new Timer(0.5f);
    s_DodgeTrail m_dodgeEffect = null;
    //class used to described interpolated movement in a direction, i.e. dodgeing or being kicked
    class InterpolatedMovement
    {
        public InterpolatedMovement(Vector3 targ, float f)
        {
            target = targ;
            force = f;
        }
        public Vector3 target = Vector3.zero;
        public float force = 0;
    }

    s_SurfaceAndGravity m_surface = null;
    enum im
    { //interpoltedMovements
        kick,
        dodge,
        recoil
    }

    public bool[] InterpStatuses = new bool[3];

    InterpolatedMovement[] interpolatedMovements = new InterpolatedMovement[3];

    Timer delayedExecute = new Timer(0.05f);
    Timer dodgeEnd = new Timer(0.5f);
    Timer returnToIdleArmedPose = new Timer(0.5f);
    //ANIMATION
    s_PlayerAnimations myAnimationController;
    bool subscribedTomyAnimationController = false;

    [Tooltip("Which team a player is on.")]
    public PlayerTeam Team;

    [Tooltip("How long the player remains in an Invul state after respawning")]
    public float invulLength = 1.0f;

    [Tooltip("How long the invulnarabilty from the dodge lasts. Eventually will be tied animation event.")]
    public float DodgeLength = 0.5f;

    [Tooltip("How far the player travels (in Unity units) from a dodge.")]
    public float DodgeDistance = 5.0f;

    [Tooltip("How fast the player covers the DodgeDistance.")]
    public float DodgeSpeed = 6.0f;

    [Tooltip("How fast the player covers the DodgeDistance.")]
    public float DodgeCooldown = 1.0f;

    //direction variables
    Vector3 forwardVector = new Vector3(-1, 0, 0); //Direction player is facing(firing, also movign if using tank controls).
    Vector3 upVector = new Vector3(0, 1, 0);
    Vector3 initalForwardVector = new Vector3(-1, 0, 0);

    //Status effects are tied to states and represent timed effects that end when the status effect countdown for it hits zero.  They add additional behavior as opposed to changing existing behaviors
    public enum StatusEffects
    {
        Invul,
        Slowed,
        Stunned,
        Deflecting,
        Dead
    }
    public List<float> statusTimers = new List<float>();

    //States affect behavior in some manner, different behavior will occur depending on what state is active.
    public enum States
    {
        Invul,
        Alive,
        Dead,
        Stunned,
        Dodging,
        Falling
    }
    public States currentState = States.Alive;
    public States lastState = States.Alive;
    Stack<StateStorer> stackedStates = new Stack<StateStorer>();
    bool m_Bdied = false;



    public enum PlayerNum
    {
        ONE,
        TWO,
        THREE,
        FOUR
    }

    public enum PlayerTeam
    {
        ONE,
        TWO,
        THREE,
        FOUR,
        NONE
    }
    public int PlayerId = 1;
    static int KeyBoardAndMousePlayer = 10;
    public s_DrawVectorDirections DirecitonFacing;
    public s_DrawVectorDirections DirecitonUp;
    protected enum directions
    {
        FORWARD,
        BACK,
        LEFT,
        RIGHT
    }

    Vector3 directionToMoveIn = Vector3.zero;
    public s_Weapon equippedWeapon = null;
    s_Weapon justDroppedWeapon = null;
    Timer pickupAgainCoolDown = new Timer(0.5f); //how long before a player can pick up a weapon the dropped again
    //death variables
    [Tooltip("The base respawn time")]
    public float respawnTime = 1.5f;
    int timesDied = 0;

    bool started = false;
    [Tooltip("Base move spped of player")]
    public float moveSpeed = 7;

    [Range(1, 180)]
    public float KickArc = 180;
    [Tooltip("The distance from which you can kick other players.")]
    public float KickRange = 5.0f;
    [Tooltip("The amount of strength behind a kick.")]
    public float KickForce = 1;
    Timer delayedStart = new Timer();
    //private variables
    public const int HEALTHMAX = 10;
    int health = 10;
    s_PlayerMovement myMovement = null;
    s_PlayerControls m_myControls = null;
    s_PlayerTalk myTalk = null;
    float slowedPercent = 1.0f; //movement speed multiplier
    Timer kickTimer = new Timer(1.0f);
    bool canKick = true;
    Timer dodgeTimer = new Timer(1.0f);
    bool canDodge = true;
    s_Weapon overlappedWeapon = null;
    bool m_fireButtonDown = false;
    ArcClass kickArcC;
    Outline outlineScript = null;

   
    // Update is called once per frame
    void Update()
    {
        if (delayedStart.CountDownAutoCheckBool())
        {
            InitPlayer();
        }
        if (s_GameManager.GetPaused())
            return;
        if (DrawDirectionVectors)
            drawLines();

        for (int i = 0; i < InterpStatuses.Length; i++)
            InterpStatuses[i] = GetIMovement((im)i) != null;

        if (!GetAI())
            animateMovement();

        timers();
        kickInterpolation();
        dodgeInterpolation();
        recoilInterpolation();

    }
    void timers()
    {
        if (kickTimer.CountDownAutoCheckBool())
        {
            canKick = true;
        }
        if (dodgeTimer.CountDownAutoCheckBool())
        {
            canDodge = true;
        }
        if (delayedExecute.CountDownAutoCheckBool())
        {
            fireWeapon();
        }
        if (dodgeEnd.CountDownAutoCheckBool())
        {
            endDodgeState();
        }

        if (!checkIfCurrentState(new States[] { States.Dead, States.Dodging })) //if state is not dead or dodging
        {
            CountDownStatusEffects();
        }
        else
        {
            countDownSpecificStatus(StatusEffects.Dead);
        }

        if (DropButtonHoldTime.CountDownAutoCheckBool())
        {
            dropWeapon(false);
            if (overlappedWeapon != null && overlappedWeapon != justDroppedWeapon)
            {
                overlappedWeapon.PickUp(this);
            }
        }
        if (pickupAgainCoolDown.CountDownAutoCheckBool())
        {
            justDroppedWeapon = null;
        }
        if (returnToIdleArmedPose.CountDownAutoCheckBool())
        {
            if (checkForAnimController())
            {
                if (equippedWeapon != null)
                    equippedWeapon.PositionMesh(0);
                myAnimationController.SetWeaponAttacking(false);
            }
        }
        if (SpawnKickPuff.CountDownAutoCheckBool())
        {

            s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(VFX.KickAirPuff).OnSpawn(GetKickingFoot().transform.position);
        }

    }


    //Initalization and Destruction
    private void Awake()
    {
        myMovement = GetComponent<s_PlayerMovement>();
        myTalk = GetComponent<s_PlayerTalk>();
        if (myTalk == null)
        {
            myTalk = GetComponentInChildren<s_PlayerTalk>();
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        delayedStart.SetTimerShouldCountDown(true);
        dodgeEnd.SetTimer(DodgeLength);
        dodgeEnd.SetTimerShouldCountDown(false);
    }
    void onStart()
    {
        if (started)
        {
            return;
        }
        m_surface = GetComponent<s_SurfaceAndGravity>();
        interpolatedMovements = new InterpolatedMovement[Enum.GetValues(typeof(im)).Length];
        for (int i = 0; i < interpolatedMovements.Length; i++)
        {
            interpolatedMovements[i] = null;
        }
        stackedStates.Push(new StateStorer(currentState, 0));
        started = true;
        kickArcC = new ArcClass(KickArc);
        dodgeTimer.SetTimer(DodgeCooldown);
        CustomizedPlayers playerCustomization = null;
        if (s_GameManager.Singleton != null)
        {
            s_GameManager.Singleton.AddPlayer(this);
            playerCustomization = s_GameManager.Singleton.GetPlayerCustomizationForPlayerNum(spawnIndex);
            if (playerCustomization == null)
            {
                Debug.LogError("Unable to retrieve custom player data for spawn index " + spawnIndex);
            }
        }

        if (hands == null)
        {
            hands = GetComponent<s_HoldWeaponHand>();
        }

        setUpCustomization(playerCustomization);
        GetControlsScript().InitializeControls();
        for (int i = 0; i < Enum.GetValues(typeof(StatusEffects)).Length; i++)
        {
            statusTimers.Add(0);
        }

        forwardVector = transform.position.normalized * -1;
        startPos = transform.position;
        s_GameManager.Singleton.SetCursorColor(this);

    }
    void setUpCustomization(CustomizedPlayers _playerCustomization)
    {
        if (_playerCustomization != null)
        {

            //APPLY CUSTOMIZATION STUFF HERE
            PlayerId = _playerCustomization.PlayerIndex;
            PlayerColor = _playerCustomization.CustomColor;

            Team = _playerCustomization.Team;
            //Spawn the correct model
            if (!AvatarGameObject.activeSelf)
            {
                GameObject go = s_GameManager.Singleton.GetModel(_playerCustomization.ModelVariation, GetTeam());
                if (go == null)
                {
                    AvatarGameObject = Instantiate(s_GameManager.Singleton.GetModel(0, GetTeam()), transform);
                }
                else
                {
                    AvatarGameObject = Instantiate(go, transform);
                }
            }

            //Accessories
            s_AccessoriesCustomization outfit = AvatarGameObject.GetComponent<s_AccessoriesCustomization>();
            if (outfit == null)
            {
                Debug.LogError("No Accessories script attached to model!");
            }
            else
            {
                if (_playerCustomization.MyCustomFeatures == null)
                {
                    Debug.LogError("Player customization class AccessoriesCustomization is null! cannot be used to customization model.");
                }
                else
                {
                    outfit.SetActiveComponents(_playerCustomization.MyCustomFeatures, GetTeam());
                    outfit.SetColor(PlayerColor);
                }
            }



            Renderer myRenderer = AvatarGameObject.GetComponentInChildren<s_mainBod>().GetComponent<SkinnedMeshRenderer>();
            if (myRenderer == null)
                myRenderer = AvatarGameObject.GetComponentInChildren<s_mainBod>().GetComponent<MeshRenderer>();
            Material[] m = s_GameManager.Singleton.GetCoreMaterialForMainBody(GetTeam(), _playerCustomization.SkinVariation);
            if (m != null)
            {
                myRenderer.materials = m;
            }
            else
            {
                Debug.LogError("Skin material is null for player " + gameObject.name);
            }
            //find skin render, get the chosen skin as an array assign it to the found renderer
            if (_playerCustomization.isAI && gameObject.GetComponent<s_Player_AI>() == null)
            {
                SetAI(_playerCustomization.isAI);
                gameObject.AddComponent<s_Player_AI>();
            }

            hands.GetHands();
            myAnimationController = GetComponentInChildren<s_PlayerAnimations>();
            if (myAnimationController == null)
            {
                s_ErrorMessage.AddMesage("Spawned avatar does not have a s_PlayerAnimations script attached so will not be able to animate.");
            }
            else
            {

                if (!subscribedTomyAnimationController)
                {
                    myAnimationController.LerpToReadyFinishedHandler += weaponInPostion;
                    subscribedTomyAnimationController = true;
                }

            }
            //USE VALUES TO GET CORRECT SKIN, MODEL, COLOR, ETC.
        }
        else
        {
            PlayerId = spawnIndex;
            Color[] teamColors = new Color[] { Color.blue, Color.red, Color.green, Color.yellow };
            PlayerColor = teamColors[spawnIndex];
            if (Team == PlayerTeam.TWO)
            {
                Animator[] anims = GetComponentsInChildren<Animator>(true);
                AvatarGameObject = anims[1].gameObject;
            }
            AvatarGameObject.SetActive(true);
        }
        outlineScript = AvatarGameObject.AddComponent<Outline>();
        if (outlineScript != null)
        {

            outlineScript.OutlineColor = PlayerColor;
            outlineScript.OutlineColor = new Color(outlineScript.OutlineColor.r, outlineScript.OutlineColor.g, outlineScript.OutlineColor.b, (Team == PlayerTeam.ONE ? 0.5f : 0.15f));

        }

        name += "_id_" + PlayerId.ToString() + "_team_" + GetTeam().ToString();
    }
    private void OnEnable()
    {
        if (InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.InControl)
        {
            InControl.InputManager.OnDeviceDetached += deviceDetached;
            InControl.InputManager.OnDeviceAttached += deviceAttached;
        }
    }
    private void OnDisable()
    {
        if (subscribedTomyAnimationController)
            myAnimationController.LerpToReadyFinishedHandler -= weaponInPostion;

        if (InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.InControl)
        {
            InControl.InputManager.OnDeviceDetached -= deviceDetached;
            InControl.InputManager.OnDeviceAttached -= deviceAttached;
        }
    }

    public void InitPlayer()
    {
        onStart();
    }
    public void SetOutlineType(Outline.Mode _mode)
    {
        if (outlineScript != null && _mode != outlineScript.OutlineMode)
        {
            outlineScript.OutlineMode = _mode;
        }
    }

    //Respawn
    float getTimeBeforeRespawn()
    {
        return Mathf.Pow(respawnTime, timesDied - 1) + 1;
    }

    public float GetTimeBeforeRespawn()
    {
        return getTimeBeforeRespawn();
    }
    public void Respawn()
    {
        stackedStates.Clear();
        stackedStates.Push(new StateStorer(States.Alive, 0));
        slowedPercent = 1.0f;
        resetStatuses();
        AvatarGameObject.SetActive(true);
        timesDied++;
        m_Bdied = true;
        changeState(States.Alive);
        OnRespawned?.Invoke();
        if (allTeamAlive())
        {
            OnRespawnHandler?.Invoke(GetTeam());
        }
        JumpToStart();
        health = HEALTHMAX;

    }

    //Animation
    void animateMovement()
    {
        if (checkForAnimController())
        {
            float x2 = 0;
            float y2 = 0;
            if (GetCanMove())
            {


                if (GetUseKeyboardAndMouse())
                {
                    Vector3 dir = GetMovementScript().GetDirectionRaw(false);
                    Vector2 usableDir = new Vector2(dir.x, dir.z);
                    if (usableDir != Vector2.zero)
                    {
                        Vector2 modifiedAimDirection = new Vector2(GetAimDirection().x, GetAimDirection().z).normalized;
                        float angle = Mathf.RoundToInt(Vector2.SignedAngle(modifiedAimDirection, usableDir)); //gets the angle difference between the forwards and the direction being moved in
                        float angleInRads = angle * Mathf.Deg2Rad;
                        x2 = Mathf.Sin(angleInRads) * -1;
                        y2 = Mathf.Cos(angleInRads);
                    }

                }
                else
                {
                    Vector2 joyStickDirection = InputManagerA.GetLeftStick(PlayerId + 1); //direction of movement
                    if (joyStickDirection.x != 0 || joyStickDirection.y != 0)
                    {
                        Vector2 modifiedAimDirection = new Vector2(GetAimDirection().x, GetAimDirection().z).normalized;
                        float angle = Mathf.RoundToInt(Vector2.SignedAngle(modifiedAimDirection, joyStickDirection)); //gets the angle difference between the forwards and the direction being moved in
                        float angleInRads = angle * Mathf.Deg2Rad;
                        x2 = Mathf.Sin(angleInRads) * -1;
                        y2 = Mathf.Cos(angleInRads);

                    }
                }
            }
            myAnimationController.ChangeAnimation(EAnimationState.run, new float[] { x2, y2 });
        }
        else
        {
            Debug.LogWarning("No animation controller attached to " + gameObject.name + "! Error in setting direction for blend tree.");
        }
    }
    void animateWeaponHolding()
    {
        if (checkForAnimController() && GetEquippedWeapon() != null)
        {
            switch (GetEquippedWeapon().HowToHoldWeapon)
            {
                case s_Weapon.WeaponHoldingType.OneHandedMelee:
                    myAnimationController.ChangeAnimation(EAnimationState.holding1M, new float[0]);
                    break;
                case s_Weapon.WeaponHoldingType.TwoHandedMelee:
                    myAnimationController.ChangeAnimation(EAnimationState.holding2M, new float[0]);
                    break;
                case s_Weapon.WeaponHoldingType.OneHandedRanged:
                    myAnimationController.ChangeAnimation(EAnimationState.holding1r, new float[0]);
                    break;
                case s_Weapon.WeaponHoldingType.TwoHandedRanged:
                    myAnimationController.ChangeAnimation(EAnimationState.holding2r, new float[0]);
                    break;
                case s_Weapon.WeaponHoldingType.OneHandedThrown:
                    myAnimationController.ChangeAnimation(EAnimationState.holding1T, new float[0]);
                    break;
                default:
                    Debug.LogError("Unable to load animation unknown weapon holding type " + GetEquippedWeapon().HowToHoldWeapon.ToString());
                    break;
            }
        }
        else
        {
            myAnimationController.ChangeAnimation(EAnimationState.unarmed, new float[0]);
        }

    }
    bool checkForAnimController()
    {
        if (myAnimationController != null)
        {
            return true;
        }
        else
        {
            myAnimationController = GetComponentInChildren<s_PlayerAnimations>();
            if (myAnimationController != null)
            {
                if (!subscribedTomyAnimationController)
                {
                    myAnimationController.LerpToReadyFinishedHandler += weaponInPostion;
                    subscribedTomyAnimationController = true;
                }
                return true;
            }
            return false;
        }
    }
    void weaponInPostion(bool _val)
    {
        m_waitToFireUntilAnimationDone = _val;
    }


    //Movement and Direction
    Vector3 getDodgeDirection()
    {
        if (GetUseKeyboardAndMouse())
            return (myMovement.GetDirection() * DodgeDistance);
        else
            return (myMovement.GetDirection() * DodgeDistance);
    }

    public virtual Vector3 GetAimDirection()
    {
        return myMovement.GetAimDirection();
    }
    public float GetMoveSpeed()
    {
        return moveSpeed * (equippedWeapon == null ? 1 : (equippedWeapon.GetWeaponRange() == 0 ? 1.25f : 1));
    }
    public float GetSlowedAmount()
    {
        return slowedPercent;
    }
    public bool GetCanMove()
    {
        if (AvatarGameObject.activeSelf == false)
            changeState(States.Dead);
        return checkIfCurrentState(AliveOrInvul()) && !IsAnInterpolatedMovementActive() && !s_GameManager.GetPaused() && s_GameManager.Singleton.GetAcceptPlayerInput();
    }
    public void Dodge()
    {
        if (canDodge && checkIfCurrentState(new States[] { States.Alive, States.Invul }))
        {
            changeState(States.Dodging);
        }
    }
    public void SetPosition(Vector3 _newPostition)
    {
        transform.position = _newPostition;
    }

    //Movment interpolation
    void kickInterpolation()
    {
        if (GetIMovement(im.kick) != null)
        {
            if (s_GameManager.IsNaN(GetIMovement(im.kick).target))
            {
                SetIMovement(im.kick, null);
            }
            else
            {
                Vector3 target = new Vector3(GetIMovement(im.kick).target.x, transform.position.y, GetIMovement(im.kick).target.z);

                SetPosition(Vector3.MoveTowards(transform.position, target, GetIMovement(im.kick).force * moveSpeed * 1.5f * Time.deltaTime));
                if (s_Calculator.GetDistanceLessThan(transform.position, target, 0.25f))
                {
                    SetIMovement(im.kick, null);
                }
            }
        }
    }
    void dodgeInterpolation()
    {
        if (GetIMovement(im.dodge) != null)
        {
            if (s_GameManager.IsNaN(GetIMovement(im.dodge).target))
            {
                endDodgeState();
            }
            else
            {
                SetPosition(Vector3.MoveTowards(transform.position, GetIMovement(im.dodge).target, GetIMovement(im.dodge).force * Time.deltaTime));
            }

        }
    }
    void recoilInterpolation()
    {
        if (GetIMovement(im.recoil) != null)
        {
            if (s_GameManager.IsNaN(GetIMovement(im.recoil).target))
            {
                SetIMovement(im.recoil, null);
            }
            else
            {
                Vector3 target = new Vector3(GetIMovement(im.recoil).target.x, transform.position.y, GetIMovement(im.recoil).target.z);
                SetPosition(Vector3.MoveTowards(transform.position, target, GetIMovement(im.recoil).force * moveSpeed * Time.deltaTime));
                if (s_Calculator.GetDistanceLessThan(transform.position, target, 0.05f))
                {
                    SetIMovement(im.recoil, null);
                }
            }

        }
    }
    InterpolatedMovement GetIMovement(im _typeOfMovement)
    {
        return interpolatedMovements[(int)_typeOfMovement];
    }
    void SetIMovement(im _typeOfMovement, InterpolatedMovement _movement)
    {
        interpolatedMovements[(int)_typeOfMovement] = _movement;
    }

    public bool IsAnInterpolatedMovementActive()
    {
        for (int i = 0; i < interpolatedMovements.Length; i++)
        {
            if (interpolatedMovements[i] != null)
            {
                return true;
            }
        }
        return false;
    }
    public Vector3 GetForwardVector()
    {
        return forwardVector;

    }
    public void SetForwardVector(Vector3 val)
    {
        forwardVector = val;
    }
    public s_PlayerMovement GetMovementScript()
    {
        if (myMovement == null)
        {
            myMovement = GetComponent<s_PlayerMovement>();
        }
        return myMovement;
    }
    public void TakeKnockBack(Vector3 direction, float forceOfKnockBack, float JitterInRadians = 0, bool interpolate = false)
    {
        if (!checkIfCurrentState(States.Dodging))//can only be knocked back if not dodging;
        {
            if (!interpolate)
            {
                if (JitterInRadians != 0)
                {
                    float spreadAmount = UnityEngine.Random.Range(-JitterInRadians, JitterInRadians);
                    float x = (direction.x * Mathf.Cos(spreadAmount)) + (direction.z * Mathf.Sin(spreadAmount));
                    float y = direction.y;
                    float z = (-direction.x * Mathf.Sin(spreadAmount)) + (direction.z * Mathf.Cos(spreadAmount));


                    RecoilEffect(new Vector3(x, y, z), forceOfKnockBack);
                }
                else
                {
                    RecoilEffect(direction, forceOfKnockBack);
                }
            }
            else
            {
                SetIMovement(im.kick, new InterpolatedMovement(transform.position + (direction * forceOfKnockBack), forceOfKnockBack));
            }
        }
    }
    public void TakeSlowEffect(float slowTime, float slowEffect)
    {

        if (checkIfCurrentState(new States[] { States.Alive, States.Falling, States.Invul }))
        {
            if (slowedPercent > slowEffect)
            {
                slowedPercent = slowEffect;
            }
            AddDifferenceToStatusTimer(StatusEffects.Slowed, slowTime);
        }
    }
    public void TakeStunEffect(float stunTime)
    {
        if (!isStatusEffectActive(StatusEffects.Stunned) && !checkIfCurrentState(States.Dodging))//Only stuns a player if they are not already stunned and not dodging
        {
            changeState(States.Stunned, stunTime);
        }
    }
    public void RecoilEffect(Vector3 directionToBePushedIn, float dist)
    {
        if (GetIMovement(im.recoil) == null)
        {
            SetIMovement(im.recoil, new InterpolatedMovement(transform.position + (directionToBePushedIn.normalized * dist), Mathf.Clamp(dist, 1, dist + 1)));
        }
    }


    //Stage Set up
    public Vector3 GetStartPosition() { return startPos; }
    public void StageReset()
    {
        if (currentState == States.Dead || isStatusEffectActive(StatusEffects.Dead))
        {
            Respawn();
        }
        health = HEALTHMAX;
        timesDied = 0;
        resetStatuses();
        changeState(States.Alive);//to set the current and last state to Alive
        endDodgeState();

        canKick = true;
        SetIMovement(im.kick, null);
    }
    public void SetNewStartPosition(Vector3 sp)
    {
        SetPosition(sp);
        startPos = sp;
        if (GetAI())
        {

            GetComponent<s_Player_AI>().MoveToNewStart();
        }
    }
    public void JumpToStart()
    {
        SetPosition(startPos + new Vector3(0, 4, 0));
    }
    
    //Audio
    public void PlayHitEnemySound()
    {
        if (myTalk != null)
            myTalk.PlayHitEnemySound();
    }
    public void PlayKilledEnemySound()
    {
        if (myTalk != null)
            myTalk.PlayKilledEnemySound();
    }
    public void PlayPickupSound()
    {
        if (myTalk != null)
            myTalk.PlayPickupSound();
    }

    //Weapons
    void fireWeapon()
    {
        endInvul();

        if (equippedWeapon != null && checkIfCurrentState(AliveOrInvul()))
        {
            OnPlayerFire?.Invoke(this);
            bool playFireAnimation = equippedWeapon.GetWeaponIsAbleToFire();


            if (checkForAnimController())
            {
                if (m_waitToFireUntilAnimationDone)
                {
                    equippedWeapon.FireWeapon();
                }
            }
            else
            {
                equippedWeapon.FireWeapon();
            }
            if (checkForAnimController() && equippedWeapon != null)
            {


                if (equippedWeapon.IsWeaponRanged())
                {
                    returnToIdleArmedPose.SetTimer(0.5f);
                    myAnimationController.SetWeaponAttacking(true);
                }
                else
                {
                    float lengthOfClip = myAnimationController.GetAnimationLength(equippedWeapon.HowToHoldWeapon);
                    if (!myAnimationController.GetWeaponAttacking())
                    {
                        returnToIdleArmedPose.SetTimer(lengthOfClip);
                    }

                }
                returnToIdleArmedPose.SetTimerShouldCountDown(true);
                equippedWeapon.PositionMesh(1);
                animateWeaponHolding();
                if (playFireAnimation)
                {

                    if (equippedWeapon.IsWeaponRanged() && (equippedWeapon as s_WeaponRanged).BurstFire)
                    {

                        myAnimationController.ChangeAnimation(EAnimationState.fireWeapon, new float[] { myAnimationController.GetAnimationLength(equippedWeapon.HowToHoldWeapon), equippedWeapon.GetFireRate(), (equippedWeapon as s_WeaponRanged).GetFireRateForIndividualBurstFireShots() });
                    }
                    else
                    {
                        if (!equippedWeapon.IsWeaponRanged())
                        {

                            if (!myAnimationController.GetWeaponAttacking())
                            {
                                myAnimationController.SetWeaponAttacking(true);
                                myAnimationController.ChangeAnimation(EAnimationState.fireWeapon, new float[] { myAnimationController.GetAnimationLength(equippedWeapon.HowToHoldWeapon), equippedWeapon.GetFireRate() });
                            }

                        }
                        else
                        {
                            myAnimationController.ChangeAnimation(EAnimationState.fireWeapon, new float[] { myAnimationController.GetAnimationLength(equippedWeapon.HowToHoldWeapon), equippedWeapon.GetFireRate() });
                        }

                    }
                }

            }
        }
    }
    void dropWeapon(bool died)
    {
        if (equippedWeapon != null)
        {
            justDroppedWeapon = equippedWeapon;
            pickupAgainCoolDown.SetTimerShouldCountDown(true);
            equippedWeapon.Dropped(died);
            equippedWeapon = null;
            OnWeaponEquipped?.Invoke(equippedWeapon);
            animateWeaponHolding();

        }
    }
    bool isProjectileInDelfectionArc(Vector3 v)
    {

        s_MeleeWeapon mw = ((s_MeleeWeapon)GetEquippedWeapon());
        if (mw != null)
        {
            Vector3 dirToTarget = transform.position - v;
            float dotResult = Vector3.Dot(dirToTarget.normalized, -GetAimDirection().normalized);
            float usedResult = dotResult;
            bool checkValue = usedResult > mw.GetDeflectionArc();
            string checkValueString = " dotResult > mw.GetDeflectionArc() is ";

            if (checkValue)
            {
                return true;
            }


        }

        return false;
    }

    public void DropWeaponOnLevelTransition()
    {
        if (equippedWeapon != null)
        {
            s_Weapon toDelete = equippedWeapon;
            DropWeapon(true);
            Destroy(toDelete.gameObject);
        }
    }   
    public void Kick()
    {

        if (canKick)
        {
            canKick = false;
            kickTimer.SetTimerShouldCountDown(true);
            Vector3 forward = GetAimDirection();
            List<s_Player> kickables = s_GameManager.Singleton.GetPlayersOnTeam(GetTeam(), true);
            kickArcC.ResetArc(KickArc);
            if (checkForAnimController())
            {
                myAnimationController.ChangeAnimation(EAnimationState.kick, new float[] { 0 });
            }
            bool hitSomeone = false;
            for (int i = 0; i < kickables.Count; i++)
            {
                if (s_Calculator.GetDistanceLessThan(transform.position, kickables[i].transform.position, KickRange))
                {
                    Vector3 dirToTarget = transform.position - kickables[i].transform.position;
                    float dotResult = Vector3.Dot(dirToTarget.normalized, forward.normalized);
                    s_Player kicky = kickables[i];
                    if (dotResult < 0)
                    {
                        if (dotResult * -1 > kickArcC.arc)
                        {
                            kicky.TakeKnockBack(-dirToTarget.normalized, KickForce, 0, true);
                            hitSomeone = true;
                            s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType((kicky.GetTeam() == 0 ? VFX.KickHumanHit : VFX.KickRobotHit)).OnSpawn(kicky.transform.position);
                        }
                    }

                }
            }
            if (!hitSomeone)
            {
                SpawnKickPuff.SetTimer(0.5f);
                if (checkForAnimController())
                {
                    SpawnKickPuff.SetTimer(myAnimationController.GetAnimationLength("Kick") * 0.5f);

                }
                SpawnKickPuff.SetTimerShouldCountDown(true);

            }
        }
    }
    public GameObject GetKickingFoot()
    {
        if (KickingFoot == null)
            KickingFoot = AvatarGameObject.GetComponentInChildren<s_KickingFoot>().gameObject;
        if (KickingFoot == null)
            KickingFoot = AvatarGameObject;
        return KickingFoot;
    }
    public void FireWeapon()
    {
        fireWeapon();
    }
    public void FireWeaponSecondary()
    {
        if (equippedWeapon != null)
        {
            equippedWeapon.FireWeaponSecondary();
        }

    }
    public void FireWeaponAuto()
    {
        delayedExecute.SetTimerShouldCountDown(true);
    }
    public void DropWeapon(bool instant = false)
    {
        if (instant)
        {
            dropWeapon(false);
        }
        else
        {
            DropButtonHoldTime.SetTimerShouldCountDown(true);
        }

    }
    public void DropWeaponRelease()
    {
        DropButtonHoldTime.SetTimerShouldCountDown(false);
        DropButtonHoldTime.SetCurrentTime(DropButtonHoldTime.timeReset);
    }
    public void SetOverlapped(s_Weapon weapon)
    {
        if (overlappedWeapon == weapon)
        {
            overlappedWeapon = null;
        }
        else
        {
            overlappedWeapon = weapon;
        }
    }
    public void AddWeaponToHand(s_Weapon weapon)
    {
        hands.StartHolding(weapon);

    }
    public s_HoldWeaponHand GetHands()
    {
        return hands;
    }
    public bool SetEquippedWeapon(s_Weapon weaponToEquip)
    {
        if (justDroppedWeapon == null || weaponToEquip != justDroppedWeapon) // confirms that the weapon being trying to be equipped is not one that was just dropped
        {
            endInvul();
            equippedWeapon = weaponToEquip;
            OnWeaponEquipped?.Invoke(equippedWeapon);
            animateWeaponHolding();
            return true;
        }
        return false;
    }
    public s_Weapon GetEquippedWeapon()
    {
        return equippedWeapon;
    }
    public void AIFire()
    {
        FireWeapon();
    }
    public bool GetDeflecting(s_Projectile p)
    {
        return isStatusEffectActive(StatusEffects.Deflecting) && isProjectileInDelfectionArc(p.transform.position);
    }



    //Impact and Damage 
    void die()
    {
        if (!s_GameManager.Singleton.GetAllDead())
        {
            AvatarGameObject.SetActive(false);
            m_Bdied = true;
            endDodgeState();
            SetIMovement(im.kick, null);
            SetIMovement(im.recoil, null);
            changeState(States.Dead, getTimeBeforeRespawn());
            OnKilled?.Invoke();
        }
        else
        {
            health = HEALTHMAX;
        }

    }
    void hitVFX()
    {
        if (myTalk != null)
        {
            myTalk.PlayPainSound();
        }
    }

    public bool TakeDamage(int damageAmount)//return true if the hit kills the player
    {
        if (checkIfCurrentState(new States[] { States.Alive, States.Stunned }))
        {
            if (s_GameManager.Singleton.GetStageManager().GetSuddenDeathDoDamage())
                health -= damageAmount;
            else
                health = HEALTHMAX;
            float offset = 1;
            s_GameManager.Singleton.GetFloatingText().OnSpawn(transform.position + new Vector3(UnityEngine.Random.Range(-offset, offset), 0, UnityEngine.Random.Range(-offset, offset)), "-" + damageAmount.ToString(), PlayerColor);
            if (health <= 0)
            {
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.dead_001);
                health = 0;
                die();
                return true;
            }
            else
            {
                AudioControlerScript.PlaySound(AudioControlerScript.Clips.damage_001);
                hitVFX();
                return false;
            }

        }
        return false;
    }
    public int GetHealth()
    {
        return health;
    }
    public float GetHealthAsPercent()
    {
        return (health / (float)HEALTHMAX);
    }
    public float GetReloadTimeAsPercent()
    {
        if (GetEquippedWeapon() != null)
        {
            return GetEquippedWeapon().GetReloadTimeAsPercent();
        }
        return 0;
    }
    public bool IsValidTarget()
    {
        return !checkIfCurrentState(States.Dead);
    }



    //States 
    bool checkIfCurrentState(States[] states)
    {
        for (int i = 0; i < states.Length; i++)
        {
            if (currentState == states[i])
            {
                return true;
            }
        }
        return false;

    }
    bool checkIfCurrentState(States state)
    {
        if (currentState == state)
        {
            return true;
        }
        return false;

    }
    void changeState(States stateToChangeTo, float _statusTime = 0)
    {
        if (stateToChangeTo != States.Alive && currentState == States.Dead)
        {
            return;
        }
        if (stateToChangeTo != States.Alive && currentState == stateToChangeTo)
        {
            return;
        }
        //on exiting current state
        switch (currentState)
        {
            case States.Alive:
                break;
            case States.Dead:

                break;
            case States.Stunned:

                break;
            case States.Invul:

                break;
            case States.Dodging:
                break;
            case States.Falling:
                break;
            default:
                break;
        }

        stateSwap(stateToChangeTo);

        //on entering stateToChangeTo
        switch (currentState)
        {
            case States.Alive:
                if (m_Bdied)
                {
                    m_Bdied = false;
                    changeState(States.Invul, invulLength);
                }
                break;
            case States.Dead:
                dropWeapon(true);
                ActivateStatusEffect(StatusEffects.Dead, _statusTime);
                if (isTPK())
                {
                    OnDieHandler?.Invoke(GetTeam());

                }
                break;
            case States.Stunned:
                s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(VFX.StunEffect).OnSpawn(gameObject, _statusTime);
                ActivateStatusEffect(StatusEffects.Stunned, _statusTime);
                break;
            case States.Invul:
                if (!isStatusEffectActive(StatusEffects.Invul))
                {
                    ActivateStatusEffect(StatusEffects.Invul, _statusTime);
                }
                break;
            case States.Dodging:
                canDodge = false;
                SetIMovement(im.dodge, new InterpolatedMovement(transform.position + getDodgeDirection(), DodgeSpeed));
                m_dodgeEffect = s_GameManager.Singleton.GetVFXSpawner().GetVFXOfType(VFX.DodgeTrail) as s_DodgeTrail;
                m_dodgeEffect.OnSpawn(AvatarGameObject.transform.position, 10);
                m_dodgeEffect.SetObject(gameObject);
                dodgeEnd.SetTimer(DodgeDistance / DodgeSpeed);
                dodgeEnd.SetTimerShouldCountDown(true);
                break;
            case States.Falling:
                break;
            default:
                break;
        }
    }
    bool stackHasState(States _state)
    {
        foreach (StateStorer s in stackedStates)
        {
            if (s.storedState == _state)
            {
                return true;
            }
        }


        return false;
    }
    void stateSwap(States _stateToChangeTo)
    {

        float revertTime = checkStatusTime(currentState);
        if ((revertTime > 0 && !stackHasState(currentState)))
        {
            stackedStates.Push(new StateStorer(currentState, revertTime));
            resetStatusTime(currentState);
        }
        currentState = _stateToChangeTo;
    }
    void endInvul()
    {
        if (isStatusEffectActive(StatusEffects.Invul))
            statusTimers[(int)StatusEffects.Invul] = 0.1f;
    }
    float checkStatusTime(States _s)
    {
        switch (_s)
        {
            case States.Invul:
                return GetStatusEffectTime(StatusEffects.Invul);
            case States.Dead:
                return GetStatusEffectTime(StatusEffects.Dead);
            case States.Stunned:
                return GetStatusEffectTime(StatusEffects.Stunned);
            case States.Alive:
            case States.Dodging:
            case States.Falling:
                return 0;
            default:
                Debug.LogError("State to status not accounted for");
                return 0;
        }
    }
    void resetStatusTime(States _s)
    {
        switch (_s)
        {
            case States.Invul:
                statusTimers[(int)StatusEffects.Invul] = statusResetValue;
                break;
            case States.Dead:
                statusTimers[(int)StatusEffects.Dead] = statusResetValue;
                break;
            case States.Stunned:
                statusTimers[(int)StatusEffects.Stunned] = statusResetValue;
                break;
            case States.Alive:
            case States.Dodging:
            case States.Falling:
                break;
            default:
                Debug.LogError("State to status not accounted for");
                break;
        }
    }
    void CountDownStatusEffects()
    {

        for (int i = 0; i < statusTimers.Count; i++)
        {
            reduceStatusTimer(i);
        }
    }
    void countDownSpecificStatus(StatusEffects effectToCountDown)
    {
        int i = (int)effectToCountDown;
        reduceStatusTimer(i);
    }
    void reduceStatusTimer(int i)
    {
        if (statusTimers[i] >= 0)
        {
            statusTimers[i] -= Time.deltaTime;
            if (statusTimers[i] <= 0)
            {
                onStatusEnd(i);
                statusTimers[i] = statusResetValue;
            }
        }
    }
    bool isStatusEffectActive(StatusEffects toCheck)
    {
        return GetStatusEffectTime(toCheck) > 0;
    }
    void setDeadStatusTimer()
    {
        statusTimers[(int)StatusEffects.Dead] = 5.0f;
    }
    void onStatusEnd(int i)
    {
        StatusEffects effect = (StatusEffects)i;
        switch (effect)
        {
            case StatusEffects.Slowed:
                slowedPercent = 1.0f;
                break;
            case StatusEffects.Dead:
                if (s_GameManager.Singleton.CanRespawn())
                {
                    Respawn();
                }
                break;
            case StatusEffects.Stunned:
                RevertToLastState();
                break;
            case StatusEffects.Invul:
                RevertToLastState();
                break;
            case StatusEffects.Deflecting:
                break;
            default:
                break;
        }

    }
    void resetStatuses()
    {
        for (int i = 0; i < statusTimers.Count; i++)
        {
            statusTimers[i] = statusResetValue;
        }
    }
    bool isTPK()
    {
        foreach (s_Player p in s_GameManager.Singleton.GetPlayersOnTeam(GetTeam()))
        {
            if (p.GetCurrentState() != States.Dead)
            {
                return false;

            }
        }
        return true;
    }
    bool allTeamAlive()
    {
        foreach (s_Player p in s_GameManager.Singleton.GetPlayersOnTeam(GetTeam()))
        {
            if (p.GetCurrentState() == States.Dead)
            {
                return false;

            }

        }

        return true;
    }

    public void ChangeState(States stateToChangeTo, float statusTime = 0)
    {
        changeState(stateToChangeTo, statusTime);
    }
    public void RevertToLastState()
    {

        if (stackedStates.Count > 1)
        {
            StateStorer popped = stackedStates.Pop();
            changeState(popped.storedState, popped.time);
        }
        else
        {
            changeState(States.Alive);
        }


    }
    public States GetCurrentState()
    {
        return currentState;
    }
    public bool IsTPK()
    {
        return isTPK();
    }
    public int GetTimesDead()
    {
        return timesDied;
    }
    public void ActivateStatusEffect(StatusEffects statusToSet, float timeForStatusToBeActive)
    {
        statusTimers[(int)statusToSet] = timeForStatusToBeActive;
        if (statusToSet == StatusEffects.Dead)
        {
            setDeadStatusTimer();
        }
    }
    public void AddStatusToTimer(StatusEffects effectToAddTo, float timeToAdd)
    {
        if ((int)effectToAddTo < statusTimers.Count)
        {
            statusTimers[(int)effectToAddTo] += timeToAdd;
        }
        else
        {
            Debug.LogError("Failed to add time to timer for status " + effectToAddTo.ToString() + " out of bounds.  Int value is " + ((int)effectToAddTo).ToString() + " and timers list count is " + statusTimers.Count);
        }
    }
    public void AddDifferenceToStatusTimer(StatusEffects effectToAddTo, float timeToAdd)
    {
        if ((int)effectToAddTo < statusTimers.Count)
        {
            timeToAdd -= statusTimers[(int)effectToAddTo];
            if (timeToAdd > 0)
            {
                statusTimers[(int)effectToAddTo] += timeToAdd;
            }
        }
        else
        {
            Debug.LogError("Failed to add difference to timer for status " + effectToAddTo.ToString() + " out of bounds.  Int value is " + ((int)effectToAddTo).ToString() + " and timers list count is " + statusTimers.Count);
        }

    }
    public float GetStatusEffectTime(StatusEffects statusToCheck)
    {

        if ((int)statusToCheck < statusTimers.Count)
        {
            return statusTimers[(int)statusToCheck];
        }
        else
        {
            Debug.LogError("Failed to get time for timer for status " + statusToCheck.ToString() + " out of bounds.  Int value is " + ((int)statusToCheck).ToString() + " and timers list count is " + statusTimers.Count);
            return 0;
        }

    }
    public bool GetCanDodge()
    {
        return canDodge;
    }
    public bool GetCanKick()
    {
        return canKick;
    }
    public void endDodgeState()
    {
        SetIMovement(im.dodge, null);
        if (m_dodgeEffect != null)
        {
            m_dodgeEffect.SetObject(null);
        }
        dodgeTimer.SetTimerShouldCountDown(true);
        RevertToLastState();
    }
    public States[] AliveOrInvul()
    {
        return new States[] { States.Alive, States.Invul, States.Falling };
    }

    //Collision Detection
    public void OnCollisionEnter(Collision collision)
    {
        onEnter(collision.collider);
    }
    private void OnCollisionStay(Collision collision)
    {
        onEnter(collision.collider);
    }
    void onEnter(Collider c)
    {
        if (GetIMovement(im.kick) != null || GetIMovement(im.recoil) != null)
        {
            s_Obstacle o = c.gameObject.GetComponent<s_Obstacle>();
            if (o != null)
            {
                SetIMovement(im.kick, null);
                SetIMovement(im.recoil, null);
                endDodgeState();
            }
        }

    }

    //Indentifiaction
    public PlayerTeam GetTeamEnum()
    {
        return Team;
    }
    public int GetTeam()
    {
        return (int)Team;
    }
    public int GetSpawnIndex()
    {
        return spawnIndex;
    }
    public void SetSpawnIndex(int _index)
    {
        spawnIndex = _index;
    }
    public int GetPlayerID()
    {
        return PlayerId;
    }
    public int GetPlayerTeamAsNumber()
    {
        switch (Team)
        {
            case PlayerTeam.ONE:
                return 1;
            case PlayerTeam.TWO:
                return 2;
            case PlayerTeam.THREE:
                return 3;
            case PlayerTeam.FOUR:
                return 4;
            default:
                Debug.LogError("UNKNOWN PLAYER ID CANNOT RETURN AS INT.  RETURNING ZERO.");
                return 0;
        }
    }
    public void SetAI(bool val)
    {
        isAI = val;
        if (isAI)
        {
            m_surface.enabled = false;
        }
        else
        {
            m_surface.enabled = true;
            if (GetComponent<Rigidbody>() == null)
            {
                Rigidbody mb = gameObject.AddComponent<Rigidbody>();
                mb.constraints = RigidbodyConstraints.FreezeRotation | RigidbodyConstraints.FreezePositionY;
                mb.mass = 50;
            }
        }
    }
    public bool GetAI() { return isAI; }

    //Player Input
    private void deviceDetached(InControl.InputDevice _device)
    {
        KeyBoardAndMousePlayer = 10;
        int cc = InputManagerA.GetControllerCount();
        int pc = 0;
        foreach (s_Player p in s_GameManager.Singleton.GetPlayersList())
        {
            if (!p.GetAI())
            {
                pc++;
            }
        }
        if (cc >= pc)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }

    }
    private void deviceAttached(InControl.InputDevice _device)
    {
        KeyBoardAndMousePlayer = 10;
        if (GetUseKeyboardAndMouse())
            s_GameManager.Singleton.SetCursorColor(this, true);
        int cc = InputManagerA.GetControllerCount();
        int pc = 0;
        foreach (s_Player p in s_GameManager.Singleton.GetPlayersList())
        {
            if (!p.GetAI())
            {
                pc++;
            }
        }
        if (cc >= pc)
        {
            Cursor.visible = false;
        }
        else
        {
            Cursor.visible = true;
        }
    }

    public void SetFireButtonDown(bool _val)
    {
        m_fireButtonDown = _val;
    }
    public bool GetFireButtonDown()
    {
        return m_fireButtonDown;
    }
    public bool GetUseKeyboardAndMouse()
    {
        if (PlayerId + 1 > InputManagerA.GetControllerCount() && PlayerId < KeyBoardAndMousePlayer && !isAI)
        {
            KeyBoardAndMousePlayer = PlayerId;
            s_GameManager.Singleton.SetCursorColor(this, true);
        }
        return PlayerId == KeyBoardAndMousePlayer;
    }
    public s_PlayerControls GetControlsScript()
    {
        if (m_myControls == null)
        {
            m_myControls = GetComponent<s_PlayerControls>();
        }
        return m_myControls;
    }
    public InControl.InputDevice GetICDevice()
    {
        return GetControlsScript().GetICDevice();
    }

}

