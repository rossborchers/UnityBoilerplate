using System;
public class Message
{
    public Message(string messageName) 
    {
        this.messageName = messageName;
        this.dataType = null;
    }
    private Message() 
    { 
    }

    public string messageName;
    public Type dataType;
}