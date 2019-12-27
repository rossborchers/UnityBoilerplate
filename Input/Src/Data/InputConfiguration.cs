using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

[Serializable]
class InputConfig
{
   public List<ProfileConfig> playerInputProfiles = new List<ProfileConfig>();
}

[Serializable]
class ProfileConfig
{
    public string name;
    public List<string> ownedDevices;
    public List<DeviceBindingsConfig> deviceBindings = new List<DeviceBindingsConfig>();
}

[Serializable]
class DeviceBindingsConfig
{
    public string device;
    public List<BindingConfig> deviceBindings = new List<BindingConfig>();
}

[Serializable]
class BindingConfig
{
    public string actionName;
    public string input;
    public string direction;
    public string activationState;
}


