using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Boostrapper
{
    public abstract class Manager
    {
        protected static List<Manager> _globalManagers = new List<Manager>();
        protected static Dictionary<string, List<Manager>> _groupedManagers;

        protected Manager()
        {
            _globalManagers.Add(this);
        }

        //Override in manager
        protected Manager(string managerGroup)
        {
            if(_groupedManagers.TryGetValue(managerGroup, out List<Manager> group))
            {
                group.Add(this);
            }
            else
            {
                _groupedManagers.Add(managerGroup, new List<Manager>() { this });
            }
        }

        public static T Get<T>() where T : Manager
        {
            foreach (Manager m in _globalManagers)
            {
                if (typeof(T).IsAssignableFrom(m.GetType()))
                {
                    return (T)m;
                }
            }
            return null;
        }

        public static T Get<T>(string group) where T : Manager
        {
            Debug.Assert(string.IsNullOrEmpty(group), "[Manager] group is null or empty when calling Get()");

            if(_groupedManagers.TryGetValue(group, out var value))
            {
                foreach(Manager m in value)
                {
                    if(typeof(T).IsAssignableFrom(m.GetType()))
                    {
                        return (T)m;
                    }
                }
            }
            return null;
        }

        public abstract void UpdateManager();
        public abstract void ShutdownManager();
    }
}