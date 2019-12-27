using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class MessageHandler
{
    public MessageHandler()
    {
        subscribers = new Dictionary<string, HashSet<Listener>>();
    }

    public void Subscribe(string messageName, Listener listener)
    {
        if (subscribers.ContainsKey(messageName))
        {
            subscribers[messageName].Add(listener);
        }
        else
        {
            subscribers.Add(messageName, new HashSet<Listener>() { listener });
        }
    }
    public void Unsubscribe(string messageName, Listener listener)
    {
        if (subscribers.ContainsKey(messageName))
        {
            subscribers[messageName].Remove(listener);
        }
    }

    public void Send(Message message)
    {
        if (subscribers.ContainsKey(message.messageName))
        {
            foreach (Listener listener in subscribers[message.messageName])
            {
                listener(message);
            }
        }
    }

    //dictionary with message name as key and set of Listener delegates for each key.
    public delegate void Listener(Message message);

    private Dictionary<string, HashSet<Listener>> subscribers;
}

