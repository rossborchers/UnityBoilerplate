using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public static class Lerp
{
    public static ILerpChain Linear(float from, float to, float time, Action<float> setter)
    {
        LerpChain c = new LerpChain();
        c.CoroutineInstance = CoroutineManager.StartCoroutine(Interpolate(from, to, time, setter, Easing.Linear, c.ExecuteOnComplete));
        c.Running = true;
        return c;
    }

    public interface ILerpChain
    {
        bool Running { get; }

        ILerpChain Then(Action onComplete);
        void Interrupt();
    }

    private class LerpChain : ILerpChain
    {
        public Coroutine CoroutineInstance;

        private event Action _onComplete = delegate {};

        public void ExecuteOnComplete()
        {
            _onComplete();
            Running = false;
        }

        public bool Running
        {
            get;
            set;
        }

        public ILerpChain Then(Action onComplete)
        {
            _onComplete += onComplete;
            return this;
        }

        public void Interrupt()
        {
            CoroutineManager.StopCoroutine(CoroutineInstance);
            ExecuteOnComplete();
        }
    }

    private static IEnumerator Interpolate(float from, float to, float time, Action<float> setter, Func<float, float> easingFunction, Action onComplete)
    {
        float unscaledCurrentTime = 0;
        while (unscaledCurrentTime < time)
        {
            unscaledCurrentTime += Time.deltaTime * time;
            float t = unscaledCurrentTime / time;
            float tEased = easingFunction(t);
            float result = Mathf.Lerp(from, to, tEased);
            setter?.Invoke(result);
            yield return null;
        }

        onComplete?.Invoke();
    }
}
