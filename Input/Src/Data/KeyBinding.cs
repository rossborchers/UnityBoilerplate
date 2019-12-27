
using UnityEngine;

public class KeyBinding
{
    public KeyBinding(string name, KeyCode code, InputDirection direction, ActivationState activationState)
    {
        this.name = name;
        this.code = code;
        this.direction = direction;
        this.activationState = activationState;
    }

    public string name;
    public KeyCode code;
    public InputDirection direction;
    public ActivationState activationState;
}



