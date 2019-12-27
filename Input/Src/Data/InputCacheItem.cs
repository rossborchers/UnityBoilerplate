using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

class InputCacheItem
{
    public InputCacheItem()
    {
        lastActivationTime = 0; //Potential unintended activation at Init?
        lastValue = 0;
    }

    public float lastActivationTime;
    public float lastValue;
}

