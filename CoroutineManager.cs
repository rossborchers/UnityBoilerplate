using System.Collections;
using UnityEngine;

public class CoroutineManager : MonoBehaviour
{
    private static CoroutineManager _instance = null;

    private Coroutine StartInstanceCoroutine(IEnumerator coroutine)
    {
        return base.StartCoroutine(coroutine);
    }

    private void StopInstanceCoroutine(Coroutine coroutine)
    {
        base.StopCoroutine(coroutine);
    }

    private static void LazyInitInstance()
    {
        if (_instance == null)
        {
            GameObject mgrObject = new GameObject("CoroutineManager");
            _instance = mgrObject.AddComponent<CoroutineManager>();
            DontDestroyOnLoad(mgrObject);
        }
    }

    public new static Coroutine StartCoroutine(IEnumerator coroutine)
    {
        LazyInitInstance();
        return _instance.StartInstanceCoroutine(coroutine);
    }


    public new static void StopCoroutine(Coroutine coroutine)
    {
        LazyInitInstance();
        _instance.StopInstanceCoroutine(coroutine);
    }
}
