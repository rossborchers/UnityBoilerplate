
public class AxisBinding
{
    public AxisBinding(string name, AxisCode code, InputDirection direction, ActivationState activationState)
    {
        this.name = name;
        this.code = code;
        this.direction = direction;
        this.activationState = activationState;
    }

    public string name;
    public AxisCode code;
    public InputDirection direction;
    public ActivationState activationState;
}



