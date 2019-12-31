using System.Collections.Generic;
using UnityEngine;

public class LevelData : ScriptableObject
{
    public List<Level> Levels = new List<Level>();

    [SerializeField]
    public class Level
    {
        public string LevelName;
        public List<string> SceneStack;
    }
}
