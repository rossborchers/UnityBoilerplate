using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

//store all of the frames current axis values for each player
//the decision to hardcode individual players is based on the fact 
//that during input some frames no input from a player is possible 
//and in that instance we either need a key to identify the active player or some kind of sparse array/list
//its simpler just to set up 4 cases for each player specifically.
class FrameAxisValues
{
    public FrameAxisValues()
    {
        mkb = new List<Triple<AxisCode, ActivationState, float>>();
        joystick1 = new List<Triple<AxisCode, ActivationState, float>>();
        joystick2 = new List<Triple<AxisCode, ActivationState, float>>();
        joystick3 = new List<Triple<AxisCode, ActivationState, float>>();
        joystick4 = new List<Triple<AxisCode, ActivationState, float>>();
    }

    public List<Triple<AxisCode, ActivationState, float>> mkb;
    public List<Triple<AxisCode, ActivationState, float>> joystick1;
    public List<Triple<AxisCode, ActivationState, float>> joystick2;
    public List<Triple<AxisCode, ActivationState, float>> joystick3;
    public List<Triple<AxisCode, ActivationState, float>> joystick4;
}