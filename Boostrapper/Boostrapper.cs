﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public abstract class BoostrapperBase : MonoBehaviour
{
    [SerializeField]
    protected LevelData _levelData;

    private string _activeLevel = null;
    private Stack<string> _activePostloadScenes = new Stack<string>();

    private LevelData.Level GetLevelData(string levelName)
    {
        LevelData.Level level = null;
        foreach (LevelData.Level l in _levelData.Levels)
        {
            if (l.LevelName.ToLower().Trim() == levelName.ToLower().Trim())
            {
                level = l;
            }
        }

        if (level == null)
        {
            throw new Exception("[Bootstrapper] Failed to find level in LevelData. Critical.");
        }

        return level;
    }

    protected bool Busy
    {
        get;
        private set;
    }

    protected void LoadLevel(string levelName, Action onFinished = null)
    {
        if(Busy)
        {
            throw new Exception("LoadLevel called while level is busy being loaded. This is not allowed. You can poll BusyLoading if necessary.");
        }

        Busy = true;
        Action finishedIntercept = () =>
        {
            Busy = false;
            onFinished?.Invoke();
        };

        if (_activeLevel != null)
        {
            Debug.LogWarning("[Bootstrapper] Loading level when there is already one loaded. Can not load multiple levels at once. Unloading before loading." +
                "If you want multiple levels consider making scenes part of one level stacks. If you want to unload the old level please make it explicit by calling LoadLevel in the onFinished callback of UnloadLevel");
            Busy = false;
            UnloadLevel(()=>
            {
                LoadLevel(levelName, finishedIntercept);
            });
        }

        LevelData.Level level = GetLevelData(levelName);

        _activeLevel = level.LevelName;
        LoadStack(level.SceneStack, finishedIntercept);
    }

    private void LoadStack(List<string> remainingScenes, Action onFinished)
    {
        if(remainingScenes.Count == 0)
        {
            onFinished();
            return;
        }

        List<string> scenes = new List<string>(remainingScenes);
        string toLoad = scenes[0];
        scenes.RemoveAt(0);

        SceneManager.LoadSceneAsync(toLoad, LoadSceneMode.Additive);
        SceneManager.sceneLoaded += OnPostLoadLoaded;

        void OnPostLoadLoaded(Scene s, LoadSceneMode mode)
        {
            _activePostloadScenes.Push(s.name);

            SceneManager.sceneLoaded -= OnPostLoadLoaded;
            LoadStack(scenes, onFinished);
        }
    }

    protected void UnloadLevel(Action onFinished = null)
    {
        if(Busy)
        {
            throw new Exception("LoadLevel called while level is busy being loaded. This is not allowed. You can poll BusyLoading if necessary.");
        }

        Busy = true;
        Action finishedIntercept = () =>
        {
            Busy = false;
            _activeLevel = null;
            onFinished?.Invoke();
        };

        if (_activeLevel == null)
        {
            Debug.LogWarning("[Bootstrapper] Unloading level but there are none loaded. Skipping.");
            finishedIntercept();
        }

        UnloadStack(finishedIntercept);
    }

    private void UnloadStack(Action onFinished)
    {
        if (_activePostloadScenes.Count == 0)
        {
            onFinished?.Invoke();
            return;
        }

        string toUnload = _activePostloadScenes.Pop();

        SceneManager.UnloadSceneAsync(toUnload);
        SceneManager.sceneUnloaded += OnPostLoadUnloaded;

        void OnPostLoadUnloaded(Scene s)
        {
            SceneManager.sceneUnloaded -= OnPostLoadUnloaded;
            UnloadStack(onFinished);
        }
    }
}
