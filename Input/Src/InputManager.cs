using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

using System.IO;
using System.Text.RegularExpressions;

public partial class InputManager : MonoBehaviour
{
    /*
    *  timing and value cache.
    *  Used to calculate ups and downs and time last activations.
    *  we need to allow ups and downs on axes if we want a uniform action interface, and 
    *  since we are calculating them we do it for keypresses aswell to keep the 
    *  code as uniform as possible.
    */

    //TODO: what are we doing with the times in InputCacheItem?

    private Dictionary<KeyCode, InputCacheItem> keyCodeTimes = new Dictionary<KeyCode, InputCacheItem>();
    private Dictionary<DeviceAxisCode, InputCacheItem> axisCodeTimes = new Dictionary<DeviceAxisCode, InputCacheItem>();

    //all active players indexed by their input device.

    //Each input device has one player and each player has multiple(or no) input devices(Possible duplicate OpenPlayerInputProfile refs)
    private Dictionary<InputDevice, string> devicesToPlayers = new Dictionary<InputDevice, string>();
    private Dictionary<string, OpenPlayerProfile> players = new Dictionary<string, OpenPlayerProfile>();

    private void Update()
    {

        //get all relevant input data.
        FrameKeyValues keyValues = GetActiveKeys();
        FrameAxisValues axisValues = GetActiveAxes();


        //foreach player subscribed to each input device
        foreach (KeyValuePair<InputDevice, string> devicePlayerPair in devicesToPlayers)
        {
            //update key and axis values and tell player that its input for this frame is done.
            OpenPlayerProfile player = players[devicePlayerPair.Value];
            switch(devicePlayerPair.Key)
            {
                case InputDevice.MouseKeyboard:
                    player.Update(keyValues.mkb, axisValues.mkb, InputDevice.MouseKeyboard);
                break;
                case InputDevice.Joystick1:
                    player.Update(keyValues.joystick1, axisValues.joystick1, InputDevice.Joystick1);
                break;
                case InputDevice.Joystick2:
                    player.Update(keyValues.joystick2, axisValues.joystick2, InputDevice.Joystick2);
                break;
                case InputDevice.Joystick3:
                    player.Update(keyValues.joystick3, axisValues.joystick3, InputDevice.Joystick3);
                break;
                case InputDevice.Joystick4:
                    player.Update(keyValues.joystick4, axisValues.joystick4, InputDevice.Joystick4);
                break;
            }
        }
    }

    /*
     * Central input collection 
     * Find active keys and axees, cache values and times, calculate ups and downs.
    */
    //==========================================================

    private FrameKeyValues GetActiveKeys()
    {
        //find and add keycodes.
        FrameKeyValues values = new FrameKeyValues();
        foreach (KeyCode keyCode in System.Enum.GetValues(typeof(KeyCode)))
        {
            //lazy init items to get past inconsistency in the contents of KeyCode (Different codes when init in Start() for some reason.)
            if (!keyCodeTimes.ContainsKey(keyCode))
            {
                keyCodeTimes.Add(keyCode, new InputCacheItem());
            }
            InputCacheItem keyCache = keyCodeTimes[keyCode];

            string codeString = keyCode.ToString();
            if (Input.GetKey(keyCode)) //down or active
            {
                if (keyCache.lastValue == 0) //down
                {
                    AddKeyCode(ref values, keyCode, codeString, ActivationState.Down);
                    keyCache.lastValue = 1; //update cache
                }
                else //active 
                {
                    AddKeyCode(ref values, keyCode, codeString, ActivationState.Active);
                }
                keyCache.lastActivationTime = Time.time; //update cache activation time (should we do this on up too?)
            }
            else if (keyCache.lastValue != 0) //up
            {
                AddKeyCode(ref values, keyCode, codeString, ActivationState.Up);
                keyCache.lastValue = 0; //update cache
            }
        }
        return values;
    }

    private void AddKeyCode(ref FrameKeyValues values, KeyCode keyCode, string codeString, ActivationState position)
    {
        if (codeString.StartsWith("Joystick")) //Joystick
        {
            switch (position)
            {
                //We only care about numbered joysticks. (this discards the JoystickButton events with no joystick number)
                case ActivationState.Up:
                    {
                        if (codeString[8] == '1') values.joystick1.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Up));
                        else if (codeString[8] == '2') values.joystick2.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Up));
                        else if (codeString[8] == '3') values.joystick3.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Up));
                        else if (codeString[8] == '4') values.joystick4.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Up));
                        break;
                    }
                case ActivationState.Down:
                    {
                        if (codeString[8] == '1') values.joystick1.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Down));
                        else if (codeString[8] == '2') values.joystick2.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Down));
                        else if (codeString[8] == '3') values.joystick3.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Down));
                        else if (codeString[8] == '4') values.joystick4.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Down));
                        break;
                    }
                default:
                    {

                        if (codeString[8] == '1') values.joystick1.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Active));
                        else if (codeString[8] == '2') values.joystick2.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Active));
                        else if (codeString[8] == '3') values.joystick3.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Active));
                        else if (codeString[8] == '4') values.joystick4.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Active));
                        break;
                    }
            }
        }
        else //keyboard
        {
           
            switch (position)
            {

                case ActivationState.Up:
                    values.mkb.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Up));
                    break;
                case ActivationState.Down:
                    values.mkb.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Down));
                    break;
                default:
                    values.mkb.Add(new Pair<KeyCode, ActivationState>(keyCode, ActivationState.Active));
                    break;
            }
        }
    }

    private FrameAxisValues GetActiveAxes()
    {
        //find and add axiscodes.
        FrameAxisValues values = new FrameAxisValues();
        foreach (DeviceAxisCode axisCode in System.Enum.GetValues(typeof(DeviceAxisCode)))
        {
            if (!axisCodeTimes.ContainsKey(axisCode))
            {
                axisCodeTimes.Add(axisCode, new InputCacheItem());
            }
            InputCacheItem axisCache = axisCodeTimes[axisCode];
            string axisString = axisCode.ToString();
            float axisValue = Input.GetAxis(axisString);
            if (axisValue != 0) //down or active
            {
                if (axisCache.lastValue == 0) //down
                {
                    AddAxisCode(ref values, axisCode, axisString, ActivationState.Down, axisValue);
                }
                else //active 
                {
                    AddAxisCode(ref values, axisCode, axisString, ActivationState.Active, axisValue);
                }
                axisCache.lastActivationTime = Time.time; //update cache activation time (should we do this on up too?)
            }
            else if (axisCache.lastValue != 0) //up
            {
                AddAxisCode(ref values, axisCode, axisString, ActivationState.Up, axisValue);
            }
            axisCache.lastValue = axisValue; //update cache
        }
        return values;
    }
    private void AddAxisCode(ref FrameAxisValues values, DeviceAxisCode axisCode, string codeString, ActivationState position, float value)
    {
        if (codeString.StartsWith("Joystick")) //Joystick
        {
            switch (position)
            {
                //We only care about numbered joysticks. (this discards the JoystickButton events with no joystick number)

                case ActivationState.Up:
                    {
                        if (codeString[8] == '1') values.joystick1.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Up, value));
                        else if (codeString[8] == '2') values.joystick2.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Up, value));
                        else if (codeString[8] == '3') values.joystick3.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Up, value));
                        else if (codeString[8] == '4') values.joystick4.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Up, value));
                        break;
                    }
                case ActivationState.Down:
                    {
                        if (codeString[8] == '1') values.joystick1.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Down, value));
                        else if (codeString[8] == '2') values.joystick2.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Down, value));
                        else if (codeString[8] == '3') values.joystick3.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Down, value));
                        else if (codeString[8] == '4') values.joystick4.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Down, value));
                        break;
                    }
                default:
                    {
                        if (codeString[8] == '1') values.joystick1.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Active, value));
                        else if (codeString[8] == '2') values.joystick2.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Active, value));
                        else if (codeString[8] == '3') values.joystick3.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Active, value));
                        else if (codeString[8] == '4') values.joystick4.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Active, value));
                        break;
                    }
            }
        }
        else //mouse and scroll wheel
        {
            switch (position)
            {
                case ActivationState.Up:
                    values.mkb.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Up, value));
                    break;
                case ActivationState.Down:
                    values.mkb.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Down, value));
                    break;
                default:
                    values.mkb.Add(new Triple<AxisCode, ActivationState, float>(DeviceAxisToAxis(codeString), ActivationState.Active, value));
                    break;
            }
        }
    }

    private AxisCode DeviceAxisToAxis(string code)
    {
         if(AxisCode.MouseXAxis.ToString().Equals(code)) return AxisCode.MouseXAxis;
         else if (AxisCode.MouseYAxis.ToString().Equals(code)) return AxisCode.MouseYAxis;
         else if (AxisCode.MouseScrollAxis.ToString().Equals(code)) return AxisCode.MouseScrollAxis;
         else return (AxisCode)Enum.Parse(typeof(AxisCode), code.Remove(8,1)); //only remaining codes are joystick ones
    }

    //Interface
    //==========================================================

    /// <summary>
    /// Add a new player profile.
    /// </summary>
    /// <param name="playerName">The name used to identify the profile. Must be unique.</param>
    /// <returns>A reference to the input manager for building.</returns>
	public InputManager.PlayerProfile AddPlayerProfile(string playerName)
    {
        if (players.ContainsKey(playerName))
        {
            Debug.LogWarning("Tried to add player input profile that already exists '" + playerName + "' is already taken. Ignoring.");
			return GetPlayerProfile(playerName);
		}

        players.Add(playerName, new OpenPlayerProfile(playerName));
		return GetPlayerProfile(playerName);
    }

    /// <summary>
    /// Remove a player profile.
    /// </summary>
    /// <param name="playerName">The name used to identify the profile.</param>
    /// <returns>A reference to the input manager for building.</returns>
    public InputManager RemovePlayerProfile(string playerName)
    {
        if (!players.ContainsKey(playerName))
        {
            Debug.LogWarning("Tried to remove player input profile but the player '" + playerName + "' is nonexistant. Ignoring.");
            return this;
        }

        //remove any references in devicesToPlayers
        List<InputDevice> toRemove = new List<InputDevice>();
        foreach (KeyValuePair<InputDevice, string> devicePlayerPair in devicesToPlayers)
        {
            if(devicePlayerPair.Value.Equals(playerName))
            {
                toRemove.Add(devicePlayerPair.Key);
            }

        }
        foreach(InputDevice device in toRemove)
        {
            devicesToPlayers.Remove(device);
        }

        //safe to remove
        players.Remove(playerName);

        return this;
    }

    /// <summary>
    /// Connect a device to an existing player. Players can have multiple devices but devices can only have one player.
    /// This will remove previous connections to the device.
    /// </summary>
    /// <param name="playerName">The name used to identify the profile.</param>
    /// <param name="device">The device we wish to connect the player to.</param>
    /// <returns>A reference to the input manager for building.</returns>
    public InputManager ConnectDevice(string playerName, InputDevice device)
    {
        OpenPlayerProfile player;
       if (!players.TryGetValue(playerName, out player))
       {
            Debug.LogWarning("Attempt to add input device '"+device.ToString()+ "' to player '"+playerName+"' but player does not exist! Ignoring.");
            return this;
       }

        //we allow only one player per device
       string currentOwner;
       if(devicesToPlayers.TryGetValue(device, out currentOwner))
       {
           if (!currentOwner.Equals(playerName)) devicesToPlayers.Remove(device);
           else return this; //profile already has the device.
       }

       devicesToPlayers.Add(device, playerName);
       player.AddOwnedDevice(device);

       return this;
    }

    /// <summary>
    /// Disconnect a device from an existing player. Players can have multiple devices but devices can only have one player.
    /// </summary>
    /// <param name="playerName">The name used to identify the profile.</param>
    /// <param name="device">The device we wish to disconnect the player from.</param>
    /// <returns>A reference to the input manager for building.</returns>
    public InputManager DisconnectDevice(string playerName, InputDevice device)
    {
       OpenPlayerProfile player;
       if (!players.TryGetValue(playerName, out player))
       {
            Debug.LogWarning("Attempt to add disconnect device '" + device.ToString() + "' from player '" + playerName + "' but player does not exist! Ignoring.");
            return this;
       }

       //player checked so we only ever remove device from expected owner
       string currentOwner;
       if(devicesToPlayers.TryGetValue(device, out currentOwner))
       {
           if (playerName.Equals(currentOwner))
           {
               devicesToPlayers.Remove(device);
               player.RemoveOwnedDevice(device);
           }
       }
       return this;
    }

    /// <summary>
    /// Get an existing player profile.
    /// </summary>
    /// <param name="playerName">The name used to identify the profile.</param>
    /// <returns>The profile requested or a new profile with the given name if the requested profile does not exist.</returns>
    public PlayerProfile GetPlayerProfile(string playerName)
    {
        OpenPlayerProfile player;
        if(players.TryGetValue(playerName, out player))
        {
            return player;
        }
        else
        {
            Debug.LogWarning("Tried to get nonexistant player. Adding player '" + playerName + "' and returning new instance");
			return AddPlayerProfile(playerName);
		}
	}

    //TODO:
    public Sprite GetIconForInput(InputCode input)
    {
        if (input.axisCode != AxisCode.None) return GetIconForInput(input.axisCode);
        if (input.axisCode != AxisCode.None) return GetIconForInput(input.axisCode);
        else
        {
               //Todo: load missing sprite here 
               return null;
        }
    }

    public Sprite GetIconForInput(KeyCode input)
    {
        return null;
    }

    public Sprite GetIconForInput(AxisCode input)
    {
        return null;
    }

    public void SaveConfiguration()
    {
        InputConfig config = new InputConfig();
        
        foreach(OpenPlayerProfile player in players.Values) 
        {
            ProfileConfig profileCfg = new ProfileConfig();

            //profile name
            profileCfg.name = player.GetName();

            //profile owned devices
            profileCfg.ownedDevices = new List<string>();
            List<InputDevice> devices = player.GetOwnedDevices();
            foreach(InputDevice device in devices)
            {
                 profileCfg.ownedDevices.Add(device.ToString());
            }
        
            //foeach DeviceBindings
            foreach(DeviceBindings binding in player.GetAllDeviceBindings())
            {
                DeviceBindingsConfig bindingsCfg = new DeviceBindingsConfig();
                bindingsCfg.device = binding.GetDevice().ToString();

                //foreach keyBinding in DeviceBindings
                List<KeyBinding> keyBindings = binding.GetKeyBindings();
                foreach (KeyBinding keyBinding in keyBindings)
                {
                    BindingConfig keyBindingCfg = new BindingConfig();
                    
                    keyBindingCfg.actionName = keyBinding.name;
                    keyBindingCfg.input = keyBinding.code.ToString();
                    keyBindingCfg.direction = keyBinding.direction.ToString();
                    keyBindingCfg.activationState = keyBinding.activationState.ToString();

                    bindingsCfg.deviceBindings.Add(keyBindingCfg);
                }

                //foreach AxisBinding in DeviceBindings
                List<AxisBinding> axisBindings = binding.GetAxisBindings();
                foreach (AxisBinding axisBinding in axisBindings)
                {
                    BindingConfig axisBindingCfg = new BindingConfig();

                    axisBindingCfg.actionName = axisBinding.name;
                    axisBindingCfg.input = axisBinding.code.ToString();
                    axisBindingCfg.direction = axisBinding.direction.ToString();
                    axisBindingCfg.activationState = axisBinding.activationState.ToString();

                    bindingsCfg.deviceBindings.Add(axisBindingCfg);
                }
                profileCfg.deviceBindings.Add(bindingsCfg);
            }
            config.playerInputProfiles.Add(profileCfg);
        }
   
        //serialize
        string json = JsonUtility.ToJson(config);
            try
            {
                StreamWriter file = new StreamWriter(Application.dataPath + "/InputConfig.json");
                file.WriteLine(json);
                file.Close();
            }
            catch (Exception e)
            {
                Debug.LogError("On writing input configuration: \n" + e.Message);
            }
    }

    public bool LoadConfiguration()
    {   
        string json = "";
        try 
        {
            StreamReader file = new StreamReader(Application.dataPath + "/InputConfig.json");
            json = file.ReadToEnd();
            file.Close();
        }
        catch(Exception e)
        {
            Debug.LogError("On loading input configuration: \n" + e.Message);
            return false;
        }
        InputConfig config = JsonUtility.FromJson<InputConfig>(json);

        foreach(ProfileConfig playerCfg in config.playerInputProfiles)
        {
            AddPlayerProfile(playerCfg.name);

            foreach(DeviceBindingsConfig bindingsCfg in playerCfg.deviceBindings)
            {
                try
                {
                    //adds on no device
                    DeviceBindings bindings = GetPlayerProfile(playerCfg.name).GetDeviceBindings((InputDevice)Enum.Parse(typeof(InputDevice), bindingsCfg.device, true));

                    foreach(BindingConfig bindingCfg in bindingsCfg.deviceBindings)
                    {
                        string actionName = bindingCfg.actionName;

                        InputDirection direction;                         
                        try
                        {
                            direction = (InputDirection)Enum.Parse(typeof(InputDirection), bindingCfg.direction, true);
                        }
                        catch (Exception)
                        {
                            //sets default
                            Debug.LogError("Failure parsing input direction while loading input configuration. defaulting to positive.");
                            direction = InputDirection.Positive;
                        }

                        ActivationState state;
                        try
                        {
                            state = (ActivationState)Enum.Parse(typeof(ActivationState), bindingCfg.activationState, true);
                        }
                        catch (Exception)
                        {
                            //sets default
                            Debug.LogError("Failure parsing activation state while loading input configuration. defaulting to active.");
                            state = ActivationState.Active;
                        }


                        KeyCode keyCode;
                        AxisCode axisCode;
                       
                        try
                        {
                            keyCode = (KeyCode)Enum.Parse(typeof(KeyCode), bindingCfg.input, true);
                            bindings.AddInputAction(actionName, keyCode, direction, state);
                        }
                        catch(Exception)
                        {
                            try
                            {
                                axisCode = (AxisCode)Enum.Parse(typeof(AxisCode), bindingCfg.input, true);
                                bindings.AddInputAction(actionName, axisCode, direction, state);
                            }
                            catch (Exception)
                            {
                                Debug.LogError("Failure parsing input key/axis while loading input configuration. Skippping binding.");
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    //ignores entire bindings
                    Debug.LogError("Failure parsing bindings input device while loading input configuration. Skipping bindings.");
                }
                
                
            }

            foreach(string device in playerCfg.ownedDevices)
            {
                try
                { 
                    ConnectDevice(playerCfg.name, (InputDevice)Enum.Parse(typeof(InputDevice), device, true));
                }
                catch(Exception)
                {
                    //player will not own device
                    Debug.LogError("Failure parsing owned input device while loading input configuration. Ignoring.");
                }
            }
            
        }
		return true;
    }


}
