using System.Collections.Generic;
using UnityEngine;
using XboxCtrlrInput;
using System;
using System.Management;
using InControl;

public class s_PlayerControls : MonoBehaviour
{
    
    XboxController xboxController;
    InputDevice m_controllerDevice;
    public bool ShowDebug = false;
    public enum ControlSystem
    {
        Default,
        TankControls,
        FireAndAimAsOne

    }

    enum XEI
    {
        LeftTrigger = 9,
        RightTrigger = 10,
        LeftStickX = 1,
        LeftStickY = 2,
        RightStickX = 4,
        RightStickY = 5,
        Button0 = 350,
        Button1 = 351,
        Button2 = 352,
        Button3 = 353,
        Button4 = 354,
        Button5 = 355,
        Button6 = 356,
        Button7 = 357
    }
    public ControlSystem ControlScheme;
    
    private void OnEnable()
    {
        if (InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.InControl)
        {
            InputManager.OnDeviceDetached += deviceDetached;
            InputManager.OnDeviceAttached += deviceAttached;
        }
    }
    private void Disable()
    {
        if (InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.InControl)
        {
            InputManager.OnDeviceDetached -= deviceDetached;
            InputManager.OnDeviceAttached -= deviceAttached;
        }
    }
    public class CommandA
    {
        protected s_Player myPlayer = null;
        protected KeyCode keyCode = 0;
        protected XboxButton xbb;
        protected bool isAxis = false;
        public CommandA(s_Player p, KeyCode kc) {
            myPlayer = p;
            keyCode = kc;
        }
        public CommandA(s_Player p, XboxButton kc)
        {
            myPlayer = p;
            xbb = kc;
        }
        public bool IsAxis()
        {
            return isAxis;
        }
        public KeyCode GetKeyCode() {
            return keyCode;
        }
        public XboxButton GetXboxButtonCode() {
            return xbb;
        }
        public virtual void Execute()
        {

        }

        public virtual void ExecuteOnUp()
        {

        }


    }
    public class XCIButton : CommandA
    {
        protected XboxButton xboxButton = XboxButton.X;
        string myButtonName = "";
        public XCIButton(s_Player p, string buttonName) : base(p, (KeyCode)0)
        {
            myButtonName = buttonName;
        }
        public XCIButton(s_Player p, XboxButton buttonValue) : base(p, (KeyCode)0)
        {
            xboxButton = buttonValue;
            myButtonName = buttonValue.ToString();
        }

        public string GetAxisName()
        { return myButtonName; }
        public XboxButton GetXAxisValue() {
            return xboxButton;
        }
        public virtual void Execute(float axisValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
            {
                if (myButtonName != "")
                {
                    Debug.Log("Call to " + myButtonName + " positive");
                }
                else
                {
                    Debug.Log("Call to " + xboxButton.ToString() + " positive");
                }
            }
            

        }
        public virtual void Execute0(float axisValue)
        {
            noAxis(axisValue);

        }

        protected virtual void positiveAxis(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Postive");
        }

        protected virtual void negativeAxis(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Negative");
        }
        protected virtual void noAxis(float axValue)
        {
           
        }
        protected virtual void negativeAxis0(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Null Axis -");
        }
        protected virtual void positiveAxis0(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Null Axis +");
        }
        
    }
    public class Axis : CommandA
    {
        protected XboxAxis xboxAxis = XboxAxis.LeftTrigger;
        bool commitPush = false;
        bool inverse = false;
        string myAxisName = "";
        public Axis(s_Player p, string axisName, bool fullPush = false) : base(p, (KeyCode)0)
        {
            commitPush = fullPush;
            myAxisName = axisName;
            isAxis = true;
        }
        public Axis(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, (KeyCode)0)
        {
            commitPush = fullPush;
            xboxAxis = axisValue;
        }

        public void InverseAxis(bool _val)
        {
            inverse = _val;
        }
        public string GetAxisName()
        { return myAxisName; }
        public XboxAxis GetXAxisValue() {
            return xboxAxis;
        }
        public virtual void Execute(float axisValue)
        {
            if (commitPush)
            {
                if (axisValue == 1)
                {
                    if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug) { Debug.Log("Call to " + myAxisName + " positive"); }
                        
                    positiveAxis(inverse ? axisValue * -1 : axisValue);
                }
                

                if (axisValue == -1)
                {
                    
                    if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug) { Debug.Log("Call to " + myAxisName + " negative"); }
                    negativeAxis(inverse ? axisValue * -1 : axisValue);
                }

                if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug) { Debug.Log("Axis value for" + myAxisName + " is " + axisValue); }
                    

                if (axisValue < 1 && axisValue > -1)
                {
                    noAxis(inverse ? axisValue * -1 : axisValue);
                }
                
            }
            else
            {
                if (axisValue > 0)
                {
                    
                    positiveAxis(inverse ? axisValue * -1 : axisValue);
                    if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                    {
                        if (myAxisName != "")
                        {
                            Debug.Log("Call to " + myAxisName + " positive");
                        }
                        else
                        {
                            Debug.Log("Call to " + axisValue.ToString() + " positive");
                        }
                    }
                }

                if (axisValue < 0)
                {
                    if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug) {
                            if (myAxisName != "")
                            {
                                Debug.Log("Call to " + myAxisName + " negative");
                            }
                            else
                            {
                            
                                Debug.Log("Call to " + axisValue.ToString() + " negative");
                            }
                       }
                        
                    negativeAxis(inverse ? axisValue * -1 : axisValue);
                }

                if (axisValue == 0)
                {
                    noAxis(inverse ? axisValue * -1 : axisValue);
                }
            }

        }
        public virtual void Execute0(float axisValue)
        {
            noAxis(inverse ? axisValue * -1 : axisValue);

        }

        protected virtual void positiveAxis(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Postive");
        }

        protected virtual void negativeAxis(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Negative");
        }
        protected virtual void noAxis(float axValue)
        {

        }
        protected virtual void negativeAxis0(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Null Axis -");
        }
        protected virtual void positiveAxis0(float axValue)
        {
            if (s_GameManager.ShowDebug() || myPlayer.GetMovementScript().ShowDebug)
                Debug.Log("Null Axis +");
        }
        
    }

    public class NothingA : Axis
    {
        public NothingA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public NothingA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            
        }

        protected override void negativeAxis(float axValue = 0)
        {
            
        }

    }

    public class MoveLeftRightA : Axis
    {
        public MoveLeftRightA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public MoveLeftRightA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            myPlayer.GetMovementScript().MoveJoy(axValue, true);
        }

        protected override void negativeAxis(float axValue = 0)
        {
           
            myPlayer.GetMovementScript().MoveJoy(axValue, true);
        }
        protected override void noAxis(float axValue)
        {
            myPlayer.GetMovementScript().MoveJoy(axValue, true);
        }

    }
    public class MoveUpDownA : Axis
    {
        public MoveUpDownA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public MoveUpDownA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            int dir = 1;
            if (xboxAxis == XboxAxis.LeftTrigger)
            {
                dir = -1;
            }
            myPlayer.GetMovementScript().MoveSouth();
            myPlayer.GetMovementScript().MoveJoy(axValue * dir, false);
        }

        protected override void negativeAxis(float axValue = 0)
        {
            int dir = 1;
            if (xboxAxis == XboxAxis.LeftTrigger)
            {
                dir = -1;
            }
            myPlayer.GetMovementScript().MoveJoy(axValue * dir, false);
        }
        protected override void noAxis(float axValue)
        {

            myPlayer.GetMovementScript().MoveJoy(axValue * -1, false);
        }

    }
    public class AimLeftRightA : Axis
    {
        public AimLeftRightA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public AimLeftRightA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            myPlayer.GetMovementScript().RotateRight();
        }

        protected override void negativeAxis(float axValue = 0)
        {
            myPlayer.GetMovementScript().RotateLeft();
        }

    }
    public class AimUpDownA : Axis
    {
        public AimUpDownA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public AimUpDownA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        public override void Execute()
        {
            myPlayer.GetMovementScript().RotateRight();
        }
        protected override void positiveAxis(float axValue = 0)
        {
        }

        protected override void negativeAxis(float axValue = 0)
        {
        }

    }

    public class FireWeaponA : Axis
    {
        public FireWeaponA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public FireWeaponA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            myPlayer.FireWeapon();
            myPlayer.SetFireButtonDown(true);
        }
        protected override void noAxis(float axValue)
        {
            myPlayer.SetFireButtonDown(false);
        }



    }
    public class FireWeaponSecondaryA : Axis
    {
        public FireWeaponSecondaryA(s_Player p, string axisName, bool fullPush = false) : base(p, axisName, fullPush)
        {
        }
        public FireWeaponSecondaryA(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
            myPlayer.FireWeaponSecondary();
        }



    }
    public class FireWeaponAutoH : Axis
    {
        bool fireWhenMoved = false;
        public FireWeaponAutoH(s_Player p, string axisName, bool fullPush = false, bool fireAndAim = false) : base(p, axisName, fullPush)
        {
            fireWhenMoved = fireAndAim;
        }
        public FireWeaponAutoH(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }
        protected override void positiveAxis(float axValue = 0)
        {
           
            myPlayer.GetMovementScript().AimJoy(axValue, true);
            if (fireWhenMoved)
            {
                myPlayer.FireWeaponAuto();
            }
            
        }

        protected override void negativeAxis(float axValue = 0)
        {
            
            myPlayer.GetMovementScript().AimJoy(axValue, true);
            if (fireWhenMoved)
            {
                myPlayer.FireWeaponAuto();
            }
        }

    }
    public class FireWeaponAutoV : Axis
    {
        bool fireWhenMoved = false;
        public FireWeaponAutoV(s_Player p, string axisName, bool fullPush = false, bool fireAndAim = false) : base(p, axisName, fullPush)
        {
            fireWhenMoved = fireAndAim;
        }
        public FireWeaponAutoV(s_Player p, XboxAxis axisValue, bool fullPush = false) : base(p, axisValue, fullPush)
        {
        }

        protected override void positiveAxis(float axValue = 0)
        {
            int dir = 1;
            if (xboxAxis == XboxAxis.LeftTrigger)
            {
                dir = -1;
            }

            myPlayer.GetMovementScript().AimJoy(axValue * dir, false);
            if (fireWhenMoved)
            {
                myPlayer.FireWeaponAuto();
            }
        }

        protected override void negativeAxis(float axValue = 0)
        {
            int dir = 1;
            if (xboxAxis == XboxAxis.LeftTrigger)
            {
                dir = -1;
            }
            myPlayer.GetMovementScript().AimJoy(axValue * dir, false);
            if (fireWhenMoved)
            {
                myPlayer.FireWeaponAuto();
            }
        }

    }
    

    public class Nothing : CommandA
    {
        public Nothing(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {

        }
    }
    public class FireWeapon : CommandA
    {
        public FireWeapon(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.FireWeapon();
            myPlayer.SetFireButtonDown(true);
        }
        public override void ExecuteOnUp()
        {
            myPlayer.SetFireButtonDown(false);
        }

    }    
    public class DropWeapon : CommandA
    {
        public DropWeapon(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public DropWeapon(s_Player p, XboxButton kc) : base(p, kc)
        {

        }

        public override void Execute()
        {
            myPlayer.DropWeapon(true);
        }
        public override void ExecuteOnUp()
        {
            myPlayer.DropWeaponRelease();
        }
    }
    public class Kick : CommandA
    {
        public Kick(s_Player p, KeyCode kc) : base(p, kc)
        {
        }
        public Kick(s_Player p, XboxButton kc) : base(p, kc)
        {
        }
        public override void Execute()
        {
            myPlayer.Kick();
        }

    }
    public class Dodge : CommandA
    {
        public Dodge(s_Player p, KeyCode kc) : base(p, kc)
        {
        }
        public Dodge(s_Player p, XboxButton kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.Dodge();
        }

    }
    public class FireWeaponSecondary : CommandA
    {
        public FireWeaponSecondary(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.FireWeaponSecondary();
        }

    }

    //Cardnial directions
    public class MoveWest : CommandA
    {
        public MoveWest(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveWest();
        }
        public override void ExecuteOnUp()
        {
            myPlayer.GetMovementScript().MoveWestUp();
        }
    }
    public class MoveEast : CommandA
    {
        public MoveEast(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveEast();
        }
        public override void ExecuteOnUp()
        {
            myPlayer.GetMovementScript().MoveEastUp();
        }
    }
    public class MoveNorth : CommandA
    {
        public MoveNorth(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveNorth();
        }
        public override void ExecuteOnUp()
        {
            myPlayer.GetMovementScript().MoveNorthUp();
        }

    }
    public class MoveSouth : CommandA
    {
        public MoveSouth(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveSouth();
        }
        public override void ExecuteOnUp()
        {
            myPlayer.GetMovementScript().MoveSouthUp();
        }
    }

    public class MoveBack : CommandA
    {
        public MoveBack(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveBackwards();
        }

    }

    public class MoveForward : CommandA
    {
        public MoveForward(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().MoveForward();
        }

    }
    
    public class RotateLeft : CommandA
    {
        public RotateLeft(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().RotateLeft();
        }

    }

    public class RotateRight : CommandA
    {
        public RotateRight(s_Player p, KeyCode kc) : base(p, kc)
        {
        }

        public override void Execute()
        {
            myPlayer.GetMovementScript().RotateRight();
        }

    }


    List<InControlWrapper> m_inControlWrappers = new List<InControlWrapper>();

    abstract class InControlWrapper {
        protected InputDevice m_device = null;
        protected CommandA m_myCommandA = null;
        protected Axis m_myAxis = null;
 
        protected InControlWrapper(InputDevice  _device, CommandA _myCommandA)
        {
            m_device = _device;
            m_myCommandA = _myCommandA;
            if (m_myCommandA is Axis)
                m_myAxis = (m_myCommandA as Axis);

        }
        protected abstract float getValue();
        protected abstract bool hasChanged();
        public void ControlInUse() {
            if (m_myAxis != null)
            {
                float axis = getValue();
                if (axis != 0)
                    m_myAxis.Execute(axis);
                else
                    m_myAxis.Execute(0);
            }
            else
            {
                if (getValue() > 0)
                {
                    m_myCommandA.Execute();
                }
                else
                {
                    m_myCommandA.ExecuteOnUp();
                }
            }
        }
        public void ControlNotInUse() {
           if (m_myAxis != null)
            {
                m_myAxis.Execute(0);
            }
            else
            {
                m_myCommandA.ExecuteOnUp();
            }
        }
        public void Inverse(bool _val)
        {
            if (m_myAxis != null)
            {
                m_myAxis.InverseAxis(_val);
            }
        }
        public void ResetDevice(InputDevice _device)
        {
            m_device = _device;
        }
    };

    class ICLeftStickX : InControlWrapper {

        public ICLeftStickX(InputDevice _device, CommandA _myCommandA) : base( _device,  _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.LeftStickX;
        }

        protected override bool hasChanged()
        {
            return m_device.LeftStickX.HasChanged;
        }
    }
    class ICLeftStickY : InControlWrapper
    {

        public ICLeftStickY(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.LeftStickY;
        }
        protected override bool hasChanged()
        {
            return m_device.LeftStickY.HasChanged;
        }
    }
    class ICRightStickX : InControlWrapper
    {

        public ICRightStickX(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.RightStickX;
        }
        protected override bool hasChanged()
        {
            return m_device.RightStickX.HasChanged;
        }
    }
    class ICRightStickY : InControlWrapper
    {

        public ICRightStickY(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.RightStickY;
        }
        protected override bool hasChanged()
        {
            return m_device.RightStickY.HasChanged;
        }
    }
    class ICRightTrigger : InControlWrapper
    {

        public ICRightTrigger(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.RightTrigger;
        }
        protected override bool hasChanged()
        {
            return m_device.RightTrigger.HasChanged;
        }
    }
    class ICLeftTrigger : InControlWrapper
    {

        public ICLeftTrigger(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.LeftTrigger;
        }
        protected override bool hasChanged()
        {
            return m_device.LeftTrigger.HasChanged;
        }
    }
    class ICRightBumper : InControlWrapper
    {

        public ICRightBumper(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.RightBumper;
        }
        protected override bool hasChanged()
        {
            return m_device.RightBumper.HasChanged;
        }
    }
    class ICLeftBumper : InControlWrapper
    {

        public ICLeftBumper(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.LeftBumper;
        }
        protected override bool hasChanged()
        {
            return m_device.LeftBumper.HasChanged;
        }
    }
    class ICAButton : InControlWrapper
    {

        public ICAButton(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.Action1;
        }
        protected override bool hasChanged()
        {
            return m_device.Action1.HasChanged;
        }
    }
    class ICBButton : InControlWrapper
    {

        public ICBButton(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.Action2;
        }
        protected override bool hasChanged()
        {
            return m_device.Action2.HasChanged;
        }
    }
    class ICXButton : InControlWrapper
    {

        public ICXButton(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.Action3;
        }
        protected override bool hasChanged()
        {
            return m_device.Action3.HasChanged;
        }
    }
    class ICYButton : InControlWrapper
    {

        public ICYButton(InputDevice _device, CommandA _myCommandA) : base(_device, _myCommandA) { }
        protected override float getValue()
        {
            if (m_device == null)
                return 0;
            return m_device.Action4;
        }
        protected override bool hasChanged()
        {
            return m_device.Action4.HasChanged;
        }
    }
    s_Player myPlayer;
    public List<CommandA> myKeys = new List<CommandA>();
    public List<Axis> myXCIAxis = new List<Axis>();
    public List<CommandA> myXCIButtons = new List<CommandA>();
    //first number is the keycode value, second number is the index in the list
    public List<KeyValuePair<int, int>> keyCodeMyKeysLocation = new List<KeyValuePair<int, int>>();
    public List<KeyValuePair<int, int>> XCIMyKeysLocation = new List<KeyValuePair<int, int>>();
    public List<KeyValuePair<int, int>> XCIMyKeyButtonLocation = new List<KeyValuePair<int, int>>();
    // Start is called before the first frame update
    public void InitializeControls()
    {        
        KeyCode[] myKeyCodes = (KeyCode[])Enum.GetValues(typeof(KeyCode));
        myPlayer = GetComponent<s_Player>();
        xboxController = (XboxController)myPlayer.GetPlayerID() + 1;

        for (int i = 0; i < myKeyCodes.Length; i++)
        {
            myKeys.Add(new Nothing(myPlayer, 0));            
            keyCodeMyKeysLocation.Add(new KeyValuePair<int, int>((int)myKeyCodes[i], i));
        }

        XboxAxis[] myContInput = (XboxAxis[])Enum.GetValues(typeof(XboxAxis));
        for (int i = 0; i <  myContInput.Length; i++)
        {
            myXCIAxis.Add(new NothingA(myPlayer, ""));
            XCIMyKeysLocation.Add(new KeyValuePair<int, int>((int)myContInput[i], i));
        }

        XboxButton[] myContButtonInput = (XboxButton[])Enum.GetValues(typeof(XboxButton));
        for (int i = 0; i < myContButtonInput.Length; i++)
        {
            myXCIButtons.Add(new NothingA(myPlayer, ""));
            XCIMyKeyButtonLocation.Add(new KeyValuePair<int, int>((int)myContButtonInput[i], i));
        }

        keyCodeMyKeysLocation.Sort((x, y) => x.Key.CompareTo(y.Key));
        XCIMyKeysLocation.Sort((x, y) => x.Key.CompareTo(y.Key));
        XCIMyKeyButtonLocation.Sort((x, y) => x.Key.CompareTo(y.Key));

        if (InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.InControl)
        {
            if (myPlayer.GetPlayerID() < InputManager.Devices.Count)
            {
                m_controllerDevice = InputManager.Devices[myPlayer.GetPlayerID()];
            }
            else
            {
                if (!myPlayer.GetAI() && myPlayer.GetPlayerID() > InputManager.Devices.Count + 1)
                {
                    s_ErrorMessage.AddMesage("Error Connecting controllers");
                }
               
            }
        }
        setDefaultControls();
        for (int i = 0; i < m_inControlWrappers.Count; i++)
            m_inControlWrappers[i].ControlNotInUse();
    }



    int getIndexOfButton(KeyCode val) {
        int valI = (int)val;
        int indexToCheck = keyCodeMyKeysLocation.Count / 2;
        int timesRound = 0;
        int l = 0;
        int r = keyCodeMyKeysLocation.Count - 1;

        while (l <= r)
        {
            int m = l + (r - l) / 2;         
            if (keyCodeMyKeysLocation[m].Key == valI) // Check if valI is present at mid 
            {
                return m;
            }

            if (keyCodeMyKeysLocation[m].Key < valI) // If valI greater, ignore left half 
            {
                l = m + 1;
            }
            else // If valI is smaller, ignore right half 
            {
                r = m - 1;
            }

            timesRound++;
            if (timesRound > 150)
            {
                Debug.Log("Failed to find index, breaking loop");
                break;
            }
        }



        return -1;
    }
    int getIndexOfAxis(XboxAxis val)
    {
        int valI = (int)val;
        int indexToCheck = XCIMyKeysLocation.Count / 2;
        int timesRound = 0;
        int l = 0;
        int r = XCIMyKeysLocation.Count - 1;

        while (l <= r)
        {
            int m = l + (r - l) / 2;
            if (XCIMyKeysLocation[m].Key == valI) // Check if valI is present at mid 
            {
                return m;
            }

            if (XCIMyKeysLocation[m].Key < valI) // If valI greater, ignore left half 
            {
                l = m + 1;
            }
            else // If valI is smaller, ignore right half 
            {
                r = m - 1;
            }

            timesRound++;
            if (timesRound > 150)
            {
                Debug.Log("Failed to find index, breaking loop");
                break;
            }
        }


        Debug.LogError("Unable to find value");
        return -1;
    }
    int getIndexOfXboxButton(XboxButton val)
    {
        int valI = (int)val;
        int indexToCheck = XCIMyKeyButtonLocation.Count / 2;
        int timesRound = 0;
        int l = 0;
        int r = XCIMyKeyButtonLocation.Count - 1;

        while (l <= r)
        {
            int m = l + (r - l) / 2;
            if (XCIMyKeyButtonLocation[m].Key == valI) // Check if valI is present at mid 
            {
                return m;
            }

            if (XCIMyKeyButtonLocation[m].Key < valI) // If valI greater, ignore left half 
            {
                l = m + 1;
            }
            else // If valI is smaller, ignore right half 
            {
                r = m - 1;
            }

            timesRound++;
            if (timesRound > 150)
            {
                Debug.Log("Failed to find index, breaking loop");
                break;
            }
        }


        Debug.LogError("Unable to find value");
        return -1;
    }

    public string getProperAxisName(s_Player p, int axisNumber, string axisPrefix = "") {
        switch (axisNumber)
        {
            case 1:
                return axisPrefix + "Horizontal" + "p" + p.GetPlayerID().ToString();
            case 2:
                return axisPrefix + "Vertical" + "p" + p.GetPlayerID().ToString();
            default:
                return axisPrefix + "Axis" + axisNumber.ToString() + "p" + p.GetPlayerID().ToString();
        }


    }


    // Update is called once per frame
    void Update()
    {
        if (myPlayer.GetAI() || s_GameManager.GetPaused())
        {
            return;
        }
        

        switch (InputManagerA.GetInputLibrary())
        {
            
            case InputManagerA.InputLibrary.XCI:
                if (!s_GameManager.Singleton.GetAcceptPlayerInput())
                {
                    for (int i = 0; i < myXCIAxis.Count; i++)
                    {
                        myXCIAxis[i].Execute0(0);
                    }
                    return;
                }
                for (int i = 0; i < myXCIAxis.Count; i++)
                {
                    AxisControlsXInput(i);
                }
                for (int i = 0; i < myXCIButtons.Count; i++)
                {
                    ButtonControlsXInput(i);
                }
                break;
            case InputManagerA.InputLibrary.InControl:
                for (int i = 0; i < m_inControlWrappers.Count; i++)
                    m_inControlWrappers[i].ControlInUse();
                break;
            case InputManagerA.InputLibrary.Unity:
            default:/*
                if(!myPlayer.GetUseKeyboardAndMouse())// if this is true then this function is being called elsewhere so we do not want to call it here
                    */
                CallKeyboardAndMouseInput();
                break;
        }

        
    }

    public void CallKeyboardAndMouseInput() {

        if (Input.GetMouseButtonDown(0))
        {
            myPlayer.SetFireButtonDown(true);
        }
        if (Input.GetMouseButtonUp(0))
        {
            myPlayer.SetFireButtonDown(false);
        }
        
        for (int i = 0; i < myKeys.Count; i++)
        {
            if (myKeys[i].IsAxis() && InputManagerA.GetInputLibrary() == InputManagerA.InputLibrary.Unity)
            {
                AxisControls(i, (myKeys[i] as Axis).GetAxisName());
            }
            else
            {
                keyCheck(i);
            }
        }
    }

    void keyCheck(int i)
    {
        if (Input.GetKey(myKeys[i].GetKeyCode()))
        {         
            myKeys[i].Execute();
        }
        if (Input.GetKeyUp(myKeys[i].GetKeyCode()))
        {
            myKeys[i].ExecuteOnUp();
        }
    }


    void AxisControlsXInput(int i)
    {
        XboxAxis val = myXCIAxis[i].GetXAxisValue();
        float axisVal = XCI.GetAxis(val, xboxController);
        if (axisVal != 0)
        {
            if (s_GameManager.ShowDebug() || ShowDebug)
            {
                Debug.Log(val.ToString() + " is value of " + axisVal);
            }
            myXCIAxis[i].Execute(axisVal);
            
        }
        else
        {
            myXCIAxis[i].Execute0(axisVal);           
        }
    }

    void ButtonControlsXInput(int i) {
        XboxButton val = myXCIButtons[i].GetXboxButtonCode();
        if (XCI.GetButtonDown(val, xboxController))
        {
            if (s_GameManager.ShowDebug() || ShowDebug)
                Debug.Log(val.ToString() + " is value of down");
            myXCIButtons[i].Execute();
        }
        if (XCI.GetButtonUp(val, xboxController))
        {
            if (s_GameManager.ShowDebug() || ShowDebug)
                Debug.Log(val.ToString() + " is value of up");
            myXCIButtons[i].ExecuteOnUp();
        }


    }
    void AxisControls(int index, string axis) {
       float axisVal = Input.GetAxis(axis);
        if (axisVal != 0)
        {
            (myKeys[index] as Axis).Execute(axisVal);

        }
        else
        {

            (myKeys[index] as Axis).Execute0(axisVal);
            if (s_GameManager.ShowDebug() || ShowDebug)
            {
                if (axis == "Verticalp1" || axis == "Horizontalp1")
                {
                    Debug.Log(axis + " is value of " + axisVal);
                }
            }

        }
    }

    public InputDevice GetICDevice()
    {
        return m_controllerDevice;
    }

    private void deviceDetached(InputDevice _device)
    {
        if (m_controllerDevice == _device)
        {
            m_controllerDevice = null;
            s_ErrorMessage.AddMesage("Controller dissconnected for player " + (myPlayer.PlayerId + 1), 2);
        }
    }
    private void deviceAttached(InputDevice _device)
    {
        if (m_controllerDevice == null)
        {
            for (int i = 0; i < s_GameManager.Singleton.GetPlayersList().Count; i++)
            {
                if (s_GameManager.Singleton.GetPlayersList()[i].GetControlsScript().GetICDevice() == _device)
                {
                    return;
                }
            }
            m_controllerDevice = _device;
            for (int i = 0; i < m_inControlWrappers.Count; i++)
            {
                m_inControlWrappers[i].ResetDevice(m_controllerDevice);
            }
            s_ErrorMessage.AddMesage("Controller connected for player " + (myPlayer.PlayerId + 1), 2);

        }
    }

    void setDefaultControls()
    {
        switch (ControlScheme)
        {
            case ControlSystem.Default:
                myKeys[getIndexOfButton(KeyCode.W)] = new MoveNorth(myPlayer, KeyCode.W);
                myKeys[getIndexOfButton(KeyCode.S)] = new MoveSouth(myPlayer, KeyCode.S);
                myKeys[getIndexOfButton(KeyCode.A)] = new MoveWest(myPlayer, KeyCode.A);
                myKeys[getIndexOfButton(KeyCode.D)] = new MoveEast(myPlayer, KeyCode.D);
                myKeys[getIndexOfButton(KeyCode.Space)] = new FireWeapon(myPlayer, KeyCode.Space);
                myKeys[getIndexOfButton(KeyCode.E)] = new DropWeapon(myPlayer, KeyCode.E);
                myKeys[getIndexOfButton(KeyCode.Mouse1)] = new Dodge(myPlayer, KeyCode.Mouse1);
                myKeys[getIndexOfButton(KeyCode.Mouse0)] = new FireWeapon(myPlayer, KeyCode.Mouse0);
                myKeys[getIndexOfButton(KeyCode.Mouse2)] = new FireWeaponSecondary(myPlayer, KeyCode.Mouse2);
                myKeys[getIndexOfButton(KeyCode.Q)] = new Dodge(myPlayer, KeyCode.Q);
                myKeys[getIndexOfButton(KeyCode.F)] = new Kick(myPlayer, KeyCode.F);


                switch (InputManagerA.GetInputLibrary())
                {

                    case InputManagerA.InputLibrary.XCI:
                        myXCIAxis[getIndexOfAxis(XboxAxis.LeftStickX)] = new MoveLeftRightA(myPlayer, XboxAxis.LeftStickX);
                        myXCIAxis[getIndexOfAxis(XboxAxis.LeftStickY)] = new MoveUpDownA(myPlayer, XboxAxis.LeftStickY);
                        myXCIAxis[getIndexOfAxis(XboxAxis.RightStickX)] = new FireWeaponAutoH(myPlayer, XboxAxis.RightStickX);
                        myXCIAxis[getIndexOfAxis(XboxAxis.RightStickY)] = new FireWeaponAutoV(myPlayer, XboxAxis.RightStickY);
                        myXCIAxis[getIndexOfAxis(XboxAxis.RightTrigger)] = new FireWeaponA(myPlayer, XboxAxis.RightTrigger, true);
                        myXCIAxis[getIndexOfAxis(XboxAxis.LeftTrigger)] = new FireWeaponSecondaryA(myPlayer, XboxAxis.LeftTrigger, true);
                        myXCIButtons[getIndexOfXboxButton(XboxButton.Y)] = new DropWeapon(myPlayer, XboxButton.Y);
                        myXCIButtons[getIndexOfXboxButton(XboxButton.RightBumper)] = new Kick(myPlayer, XboxButton.RightBumper);
                        myXCIButtons[getIndexOfXboxButton(XboxButton.LeftBumper)] = new Dodge(myPlayer, XboxButton.LeftBumper);

                        break;
                    case InputManagerA.InputLibrary.InControl:
                        m_inControlWrappers.Add(new ICLeftStickX(m_controllerDevice, new MoveLeftRightA(myPlayer, "", false)));
                        m_inControlWrappers.Add(new ICLeftStickY(m_controllerDevice, new MoveUpDownA(myPlayer, "", false)));
                        m_inControlWrappers[m_inControlWrappers.Count - 1].Inverse(true);
                        m_inControlWrappers.Add(new ICRightStickX(m_controllerDevice, new FireWeaponAutoH(myPlayer, "", false)));
                        m_inControlWrappers.Add(new ICRightStickY(m_controllerDevice, new FireWeaponAutoV(myPlayer, "", false)));
                        m_inControlWrappers[m_inControlWrappers.Count - 1].Inverse(true);
                        m_inControlWrappers.Add(new ICRightTrigger(m_controllerDevice, new FireWeaponA(myPlayer, "", false)));
                        m_inControlWrappers.Add(new ICLeftTrigger(m_controllerDevice, new FireWeaponSecondaryA(myPlayer, "", false)));
                        m_inControlWrappers.Add(new ICRightBumper(m_controllerDevice, new Kick(myPlayer, (KeyCode)0)));
                        m_inControlWrappers.Add(new ICLeftBumper(m_controllerDevice, new Dodge(myPlayer, (KeyCode)0)));
                        m_inControlWrappers.Add(new ICYButton(m_controllerDevice, new DropWeapon(myPlayer, (KeyCode)0)));


                        break;
                    case InputManagerA.InputLibrary.Unity:
                    default:
                        myKeys.Add(new MoveLeftRightA(myPlayer, getProperAxisName(myPlayer, (int)XEI.LeftStickX)));
                        myKeys.Add(new MoveUpDownA(myPlayer, getProperAxisName(myPlayer, (int)XEI.LeftStickY)));
                        myKeys.Add(new FireWeaponAutoH(myPlayer, getProperAxisName(myPlayer, (int)XEI.RightStickX)));
                        myKeys.Add(new FireWeaponAutoV(myPlayer, getProperAxisName(myPlayer, (int)XEI.RightStickY)));
                        myKeys.Add(new FireWeaponA(myPlayer, getProperAxisName(myPlayer, (int)XEI.RightTrigger)));
                        myKeys.Add(new FireWeaponSecondaryA(myPlayer, getProperAxisName(myPlayer, (int)XEI.LeftTrigger)));
                        myKeys[getIndexOfButton(((KeyCode)((int)(XEI.Button3)) + 20 * myPlayer.GetPlayerID()))] = new DropWeapon(myPlayer, ((KeyCode)((int)(XEI.Button3)) + 20 * myPlayer.GetPlayerID()));
                        myKeys[getIndexOfButton(((KeyCode)((int)(XEI.Button4)) + 20 * myPlayer.GetPlayerID()))] = new Dodge(myPlayer, ((KeyCode)((int)(XEI.Button4)) + 20 * myPlayer.GetPlayerID()));
                        myKeys[getIndexOfButton(((KeyCode)((int)(XEI.Button5)) + 20 * myPlayer.GetPlayerID()))] = new Kick(myPlayer, ((KeyCode)((int)(XEI.Button5)) + 20 * myPlayer.GetPlayerID()));

                        break;
                }
                break;
            case ControlSystem.TankControls:
                myKeys[getIndexOfButton(KeyCode.W)] = new MoveForward(myPlayer, KeyCode.W);
                myKeys[getIndexOfButton(KeyCode.S)] = new MoveBack(myPlayer, KeyCode.S);
                myKeys[getIndexOfButton(KeyCode.A)] = new RotateLeft(myPlayer, KeyCode.A);
                myKeys[getIndexOfButton(KeyCode.D)] = new RotateRight(myPlayer, KeyCode.D);


                myKeys.Add(new AimLeftRightA(myPlayer, getProperAxisName(myPlayer, 4)));
                myKeys.Add(new FireWeaponA(myPlayer, getProperAxisName(myPlayer, 10), true));
                myKeys.Add(new AimUpDownA(myPlayer, getProperAxisName(myPlayer, 5)));
                myKeys.Add(new MoveLeftRightA(myPlayer, getProperAxisName(myPlayer, 1)));
                myKeys.Add(new MoveUpDownA(myPlayer, getProperAxisName(myPlayer, 2)));
                break;
            case ControlSystem.FireAndAimAsOne:
                myKeys[getIndexOfButton(KeyCode.W)] = new MoveNorth(myPlayer, KeyCode.W);
                myKeys[getIndexOfButton(KeyCode.S)] = new MoveSouth(myPlayer, KeyCode.S);
                myKeys[getIndexOfButton(KeyCode.A)] = new MoveEast(myPlayer, KeyCode.A);
                myKeys[getIndexOfButton(KeyCode.D)] = new MoveWest(myPlayer, KeyCode.D);

                myKeys.Add(new FireWeaponAutoH(myPlayer, getProperAxisName(myPlayer, 4), false, true));
                myKeys.Add(new FireWeaponAutoV(myPlayer, getProperAxisName(myPlayer, 5), false, true));
                myKeys.Add(new MoveLeftRightA(myPlayer, getProperAxisName(myPlayer, 1)));
                myKeys.Add(new MoveUpDownA(myPlayer, getProperAxisName(myPlayer, 2)));

                break;
            default:
                break;
        }

    }
}














