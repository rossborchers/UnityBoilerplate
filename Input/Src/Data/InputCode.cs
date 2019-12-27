using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


//used when we need to abstract away the axis vs key codes
//for example when retrieving the input from the action.
public class InputCode
{
    public KeyCode keyCode = KeyCode.None;
    public AxisCode axisCode = AxisCode.None;
}
