using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActionMessageHandler
{
    public ActionMessageHandler()
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
    public void Unsubscribe(string actionName, Listener listener)
    {
        if (subscribers.ContainsKey(actionName))
        {
            subscribers[actionName].Remove(listener);
        }
    }

    public void Send(Action action)
    {
        if (subscribers.ContainsKey(action.actionName))
        {
            foreach (Listener listener in subscribers[action.actionName])
            {
                listener(action);
            }
        }
    }

    //dictionary with message name as key and set of Listener delegates for each key.
    public delegate void Listener(Action action);

    private Dictionary<string, HashSet<Listener>> subscribers;
}

