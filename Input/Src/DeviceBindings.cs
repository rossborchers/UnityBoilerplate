using UnityEngine;
using System;

using System.Collections;
using System.Collections.Generic;

public partial class InputManager
{
    //public interface of InputDeviceBindings
    public interface DeviceBindings
    {
        void AddInputAction(string actionName, KeyCode code, InputDirection part, ActivationState position);
        void AddInputAction(string actionName, AxisCode code, InputDirection part, ActivationState position);

        List<KeyBinding> GetKeyBindings();
        List<AxisBinding> GetAxisBindings();

        InputDevice GetDevice();

        //bad idea to expose set device?
    }

    private partial class OpenPlayerProfile : PlayerProfile
    {

        //private interface of the InputDeviceBindings
        private class OpenDeviceBindings : DeviceBindings
        {
            /*
             *  Input Codes to {Action name, Part of the action this input applies to, player this input applies to
             *  Store Action Parts to determine how to apply the value and InputPositions to allow for only up/down actions
             *  We append the player number to the action to allow for different actions per player
             *  List is used to allow mutliple actions per keycode
             */
            private Dictionary<KeyCode, List<Triple<string, InputDirection, ActivationState>>> keyToActionName = new Dictionary<KeyCode, List<Triple<string, InputDirection, ActivationState>>>();
            private Dictionary<AxisCode, List<Triple<string, InputDirection, ActivationState>>> axisToActionName = new Dictionary<AxisCode, List<Triple<string, InputDirection, ActivationState>>>();


            //what happens on the event of multiple keys for one action on one device? just track the last one? any sensible way around this?
            private Dictionary<string, KeyCode> actionNameToKey = new Dictionary<string,KeyCode>();
            private Dictionary<string, AxisCode> actionNameToAxis = new Dictionary<string,AxisCode>();

            /* Final level of abstraction for simple actions we convert the action name to the action itself
            *  using the string as a key allows us to have complex relationships with keys and axes.
            *  We append the player number to the action to allow for different actions per player
            */
            private Dictionary<string, Action> nameToAction = new Dictionary<string, Action>();

            private Queue<Action> actions = new Queue<Action>();

            private InputDevice device;

            public Queue<Action> ToActions(List<Pair<KeyCode, ActivationState>> keyValues, List<Triple<AxisCode, ActivationState, float>> axisValues)
            {
                //we dont remove actions. thats the responsibility of the caller. 

                foreach (Pair<KeyCode, ActivationState> keyPair in keyValues)
                {
                    ConvertInputToAction(keyPair.First, keyPair.Second);
                }

                foreach (Triple<AxisCode, ActivationState, float> keyTrip in axisValues) ConvertInputToAction(keyTrip.First, keyTrip.Second, keyTrip.Third);
                return actions;
            }

            public InputCode GetLastInput(string action)
            {
                InputCode code = new InputCode();

                actionNameToAxis.TryGetValue(action, out code.axisCode);
                actionNameToKey.TryGetValue(action, out code.keyCode);
        
                return code;
            }

             //Input to action processing
            // Assigns input pieces to their respective actions.
            //-------------------------------

            private void ConvertInputToAction(KeyCode code, ActivationState position)
            {
                //try get action
                List<Triple<string, InputDirection, ActivationState>> potentialActions;
                if (!keyToActionName.TryGetValue(code, out potentialActions)) return;

                foreach (Triple<string, InputDirection, ActivationState> trip in potentialActions)
                {
                    //if action expects up or down we want action to trigger only on those. elsewize all three.
                    if (trip.Third == position || trip.Third == ActivationState.Active)
                    {
                        Action action = nameToAction[trip.First];
                        if (trip.Second == InputDirection.Positive) action.value = 1;
                        else action.value = -1; //InputDirection.Negative
                        actions.Enqueue(action);

                        //set as last key for this action
                        actionNameToKey[trip.First] = code;

                        //make keys and actions mutually exclusive for the same action
                        actionNameToAxis[trip.First] = AxisCode.None;
                    }
                }

            }
            private void ConvertInputToAction(AxisCode code, ActivationState position, float value)
            {
                //try get actions
                List<Triple<string, InputDirection, ActivationState>> potentialActions;
                if (!axisToActionName.TryGetValue(code, out potentialActions)) return;

                foreach (Triple<string, InputDirection, ActivationState> trip in potentialActions)
                {
                    //if action expects up or down we want action to trigger only on those. elsewize all three.
                    if (trip.Third == position || trip.Third == ActivationState.Active)
                    {
                        Action action = nameToAction[trip.First];
                        if (trip.Second == InputDirection.Positive && value > 0)
                        {
                            action.value = value; //only assigned when positive
                            actions.Enqueue(action);
                        }
                        else if (trip.Second == InputDirection.Negative && value < 0)
                        {
                            action.value = value; //only assigned when negative
                            actions.Enqueue(action);
                        }

                        //set as last axis for this action
                        actionNameToAxis[trip.First] = code;

                        //make keys and actions mutually exclusive for the same action
                        actionNameToKey[trip.First] = KeyCode.None;
                    }
                }

            }

            /*
             * Publicly accessable from InputDeviceBinding
            */
            //==========================================================

            //Building actions
            //-------------------------------

            public void AddInputAction(string actionName, KeyCode code, InputDirection part, ActivationState position)
            {                      
                if(code.ToString().StartsWith("Joystick"))
                {
                    if (device == InputDevice.MouseKeyboard)
                    {
                        Debug.LogWarning("Attempting to add a Joystick action axis binding to a MouseKeyboard InputBinder. Ignoring.");               
                        return;
                    }
                }
                else
                {
                    if (!(device == InputDevice.MouseKeyboard))
                    {
                            Debug.LogWarning("Attempting to add a MouseKeyboard action axis binding to a Joystick InputBinder. Ignoring.");
                        return;
                    }
                }
                    
                //init action if it doesn't exist, since we can have multiple inputs triggering one action
                if (!nameToAction.ContainsKey(actionName))
                {
                    nameToAction.Add(actionName, new Action(actionName));
                }

                Triple<string, InputDirection, ActivationState> actionData = new Triple<string, InputDirection, ActivationState>(actionName, part, position);


                if (keyToActionName.ContainsKey(code))
                {
                    keyToActionName[code].Add(actionData);
                }
                else
                {
                    keyToActionName.Add(code, new List<Triple<string, InputDirection, ActivationState>>() { actionData });
                }
            }
            public void AddInputAction(string actionName, AxisCode code, InputDirection part, ActivationState position)
            {
                //init action if it doesn't exist, since we can have multiple inputs triggering one action
                if (!nameToAction.ContainsKey(actionName))
                {
                    nameToAction.Add(actionName, new Action(actionName));
                }

                Triple<string, InputDirection, ActivationState> actionData = new Triple<string, InputDirection, ActivationState>(actionName, part, position);

                if (axisToActionName.ContainsKey(code))
                {
                    axisToActionName[code].Add(actionData);
                }
                else
                {
                    axisToActionName.Add(code, new List<Triple<string, InputDirection, ActivationState>>() { actionData });
                }
            }



            public List<KeyBinding> GetKeyBindings()
            {
                List<KeyBinding> bindings = new List<KeyBinding>();
                foreach(var keyActionListPair in keyToActionName)
                {
                    foreach(Triple<string, InputDirection, ActivationState> action in keyActionListPair.Value)
                    {
                        bindings.Add(new KeyBinding(action.First, keyActionListPair.Key, action.Second, action.Third));
                    }
                    
                }
                return bindings;
            }

            public List<AxisBinding> GetAxisBindings()
            {
                List<AxisBinding> bindings = new List<AxisBinding>();
                foreach (var axisActionListPair in axisToActionName)
                {
                    foreach (Triple<string, InputDirection, ActivationState> action in axisActionListPair.Value)
                    {
                        bindings.Add(new AxisBinding(action.First, axisActionListPair.Key, action.Second, action.Third));
                    }

                }
                return bindings;
            }

            public void SetDevice(InputDevice device)
            {
                this.device = device;
            }

            public InputDevice GetDevice()
            {
                return this.device;
            }
        }
    }
}