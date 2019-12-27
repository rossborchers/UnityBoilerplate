
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//store all of the frames current key values for each player
//the decision to hardcode individual players is based on the fact 
//that during input some frames no input from a player is possible 
//and in that instance we either need a key to identify the active player or some kind of sparse array/list
class FrameKeyValues
{
    public FrameKeyValues()
    {
        mkb = new List<Pair<KeyCode, ActivationState>>();
        joystick1 = new List<Pair<KeyCode, ActivationState>>();
        joystick2 = new List<Pair<KeyCode, ActivationState>>();
        joystick3 = new List<Pair<KeyCode, ActivationState>>();
        joystick4 = new List<Pair<KeyCode, ActivationState>>();
    }

    public List<Pair<KeyCode, ActivationState>> mkb;
    public List<Pair<KeyCode, ActivationState>> joystick1;
    public List<Pair<KeyCode, ActivationState>> joystick2;
    public List<Pair<KeyCode, ActivationState>> joystick3;
    public List<Pair<KeyCode, ActivationState>> joystick4;
}

