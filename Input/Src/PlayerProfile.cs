using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;

public partial class InputManager
{
    //show only the public interface here
    public interface PlayerProfile
    {
        string GetName();

        void Listen(string action, ActionMessageHandler.Listener listener);
        void StopListenting(string action, ActionMessageHandler.Listener listener);

        List<InputDevice> GetOwnedDevices();

        DeviceBindings GetDeviceBindings(InputDevice device);
        List<DeviceBindings> GetAllDeviceBindings();
        DeviceBindings AddDeviceBindings(InputDevice device);
        void RemoveDeviceBindings(InputDevice device);
        InputCode GetInputForAction(string action);
    }

    //responsible for converting between a specific device. internal nested class only directly used by InputManager.
    private partial class OpenPlayerProfile : PlayerProfile
    {
        string profileName;

        HashSet<InputDevice> ownedDevices = new HashSet<InputDevice>();
        Dictionary<InputDevice, OpenDeviceBindings> deviceBindings = new Dictionary<InputDevice, OpenDeviceBindings>();

        InputDevice lastActive;

        private ActionMessageHandler messenger = new ActionMessageHandler();

        //Interface thats only accessable directly
        //==========================================================
        public OpenPlayerProfile(string name)
        {
            this.profileName = name;
        }

        public void Update(List<Pair<KeyCode, ActivationState>> keyValues, List<Triple<AxisCode, ActivationState, float>> axisValues, InputDevice device)
        {
            if(!OwnsDevice(device))
            {
                Debug.LogWarning("Update to "+device.ToString()+" called on '"+ profileName + "' but it doesn't own the device. Ignoring.");
                return;
            }

            OpenDeviceBindings binding;
            if(deviceBindings.TryGetValue(device, out binding))
            {
                Queue<Action> actions = binding.ToActions(keyValues, axisValues);

                lastActive = device;

                while(actions.Count > 0)
                {
                    messenger.Send(actions.Dequeue());      
                }
            }
            else
            {
                Debug.LogWarning("'"+ profileName + "' Owns " +device.ToString()+ " but has no bindings for it. Ignoring.");
            }
        }

        public void AddOwnedDevice(InputDevice device)
        {
            ownedDevices.Add(device); 
        }

        public void RemoveOwnedDevice(InputDevice device)
        {
            ownedDevices.Remove(device);
        }

        public bool OwnsDevice(InputDevice device)
        {
            return ownedDevices.Contains(device);
        }


        //Interface accessable through PlayerInputProfile
        //==========================================================   
        public void Listen(string action, ActionMessageHandler.Listener listener)
        {
            messenger.Subscribe(action, listener);
        }

        public void StopListenting(string action, ActionMessageHandler.Listener listener)
        {
            messenger.Unsubscribe(action, listener);
        }

        public DeviceBindings GetDeviceBindings(InputDevice device)
        {
            OpenDeviceBindings binding;
            if(!deviceBindings.TryGetValue(device, out binding))
            {
                   binding = new OpenDeviceBindings();
                   binding.SetDevice(device);
                   deviceBindings.Add(device, binding);
            }
            return binding;
            
        }

        public List<DeviceBindings> GetAllDeviceBindings()
        {
            List<DeviceBindings> bindings = new List<DeviceBindings>();
            foreach (OpenDeviceBindings binding in deviceBindings.Values)
            {
                bindings.Add(binding);
            }
            return bindings;
        }

		public DeviceBindings AddDeviceBindings(InputDevice device)
        {
            if (!deviceBindings.ContainsKey(device))
            {
                OpenDeviceBindings binding = new OpenDeviceBindings();
                binding.SetDevice(device);
                deviceBindings.Add(device, binding);
            }
			return GetDeviceBindings(device);
        }

        public void RemoveDeviceBindings(InputDevice device)
        {
            if (deviceBindings.ContainsKey(device))
            {
                deviceBindings.Remove(device);
            }
        }
        public string GetName()
        {
            return profileName;
        }

        public List<InputDevice> GetOwnedDevices()
        {
            List<InputDevice> devices = new List<InputDevice>();
            foreach(InputDevice device in ownedDevices)
            {
                devices.Add(device);
            }
            return devices;
        }


        public InputCode GetInputForAction(string action)
        {
            InputCode code = new InputCode();
            OpenDeviceBindings bindings;
            if(deviceBindings.TryGetValue(lastActive, out bindings))
            {
                code = bindings.GetLastInput(action);
            }
            else
            {
                Debug.LogWarning("Last active device binding is not a valid binding in player profile '" + this.profileName + "'. Returning empty InputCode.");
            }
            return code;
        }


    }
}