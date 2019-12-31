using System;
using System.Collections.Generic;

namespace Boostrapper
{
    //Simplifies state management by providing an easy to use calling paradim with Entered, Updated and Exited events.
    //Pass an enum and call "For(T state)" to begin building callbacks
    public class StateManager<T> : Manager where T : struct, IComparable
    {
        // protected members 
        protected class StateHandler
        {
            public event Action OnEntered = delegate { };
            public event Action OnExited = delegate { };
            public event Action OnUpdated = delegate { };

            public void ExecuteEntered()
            {
                OnEntered();
            }
            public void ExecuteExited()
            {
                OnExited();
            }
            public void ExecuteUpdated()
            {
                OnUpdated();
            }
        }

        //Dict would be slower than list
        protected List<KeyValuePair<T, StateHandler>> _handlers = new List<KeyValuePair<T, StateHandler>>();
        protected List<KeyValuePair<T, StateData>> _data = new List<KeyValuePair<T, StateData>>();
        protected U GetOrCreateValueForKVPList<U>(List<KeyValuePair<T, U>> list, T key, Func<U> onNew) where U : class
        {
            U value = null;
            foreach (KeyValuePair<T, U> kvp in list)
            {
                if (kvp.Key.CompareTo(key) == 0)
                {
                    value = kvp.Value;
                }
            }
            if (value == null)
            {
                U newValue = onNew();
                list.Add(new KeyValuePair<T, U>(key, newValue));
                return newValue;
            }
            return value;
        }

        protected StateHandler GetHandler(T state)
        {
            return GetOrCreateValueForKVPList(_handlers, state, ()=>new StateHandler());
        }

        protected bool _stateInitialized;

        protected T _state;

        // Public members

        public class StateData
        {
            private StateManager<T> _manager;

            public StateData(StateManager<T> manager)
            {
                _manager = manager;
            }

            public T _state;

            public StateData Entered(Action callback)
            {
                StateHandler handler = _manager.GetHandler(_state);
                handler.OnEntered += callback;
                return this;
            }

            public StateData Updated(Action callback)
            {
                StateHandler handler = _manager.GetHandler(_state);
                handler.OnUpdated += callback;
                return this;
            }

            public StateData Exited(Action callback)
            {
                StateHandler handler = _manager.GetHandler(_state);
                handler.OnExited += callback;
                return this;
            }
        }

        public StateManager() : base()
        {
            _stateInitialized = false;
        }

        public StateManager(string managerGroup) : base(managerGroup)
        {
            _stateInitialized = false;
        }

        public StateData For(T state)
        {
            return GetOrCreateValueForKVPList(_data, state, ()=>new StateData(this));
        }

        public T State
        {
            get
            {
                return _state;
            }
            set
            {
                if (value.CompareTo(_state) != 0 || !_stateInitialized)
                {
                    if (_stateInitialized)
                    {
                        GetHandler(_state).ExecuteExited();
                    }
                    _state = value;
                    GetHandler(_state).ExecuteEntered();
                    _stateInitialized = true;
                }
            }
        }

        public override void UpdateManager()
        {
            GetHandler(_state).ExecuteUpdated();
        }

        public override void ShutdownManager(){}
    }
}
