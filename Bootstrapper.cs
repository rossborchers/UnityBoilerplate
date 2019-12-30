using Fragments.Extensions;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
    using UnityEditor;
#endif

namespace Fragments.Core
{
    public class Bootstrapper : MonoBehaviour
    {
        [Tooltip("If true the bootstrapper UI will be accessible from the game through the exit button")]
        [SerializeField]
        private bool _bootstrapperHidden = false;

        //Scene configuration
        [Tooltip("Adding a scene name here will cause it to be hidden. Eg. The bootstrapper scene.")]
        [SerializeField]
        private string _startSceneIfBootstrapperHidden;

        [Space]
        //Scene configuration
        [Tooltip("Adding a scene name here will cause it to be hidden. Eg. The bootstrapper scene.")]
        [SerializeField]
        private List<string> _ignoreScenes;

        [Tooltip("Determines what scenes will load before level scene. These will also be hidden")]
        [SerializeField]
        private List<string> _preloadScenes;

        [Tooltip("Determines what scenes will load before level scene. These will also be hidden")]
        [SerializeField]
        private List<string> _postloadScenes;

        [Tooltip("Adding a scene name here will cause the preload and postload scenes not to load for it")]
        [SerializeField]
        private List<string> _independentSceneNames;

        [Tooltip("Adding a scene name here will cause the postload scenes not to be loaded when it is.")]
        [SerializeField]
        private List<string> _skipPostloadScenes;

        //Found scenes
        private List<string> _existingPreloadSceneNames;
        private List<string> _existingLevelSceneNames;
        private List<string> _existingPostloadSceneNames;

        //Loaded and to load scenes
        private Queue<string> _scenesToLoad;
        private Stack<string> _loadedScenes;

        private event Action _onAllScenesUnloaded = delegate { };
        private event Action _onAllScenesLoaded = delegate { };

        private bool _unloadPreloadScenes;

        public bool ShowUI
        {
            get;
            set;
        }

        // Start is called before the first frame update
        void Start()
        {
            _scenesToLoad = new Queue<string>();
            _loadedScenes = new Stack<string>();

            _existingPreloadSceneNames = new List<string>();
            _existingPostloadSceneNames = new List<string>();

            SceneManager.sceneUnloaded += (Scene s) =>
            {              
                if (_loadedScenes.Count > 0)
                {                   
                    if (!_unloadPreloadScenes)
                    {
                        string toUnload = _loadedScenes.Peek();
                        if (_existingPreloadSceneNames.Contains(toUnload))
                        {
                            _onAllScenesUnloaded();
                        }
                        else
                        {
                            SceneManager.UnloadSceneAsync(_loadedScenes.Pop());
                        }
                    }
                    else
                    {
                        SceneManager.UnloadSceneAsync(_loadedScenes.Pop());
                    }
                }
                else
                {
                    //all scenes unloaded. show bootstrapper ui.
                    if(_bootstrapperHidden)
                    {
                        Debug.LogWarning("[Boostrapper] All Scenes closed! Bootstrapper hidden so quitting application.");
                        Application.Quit();
                    }
                    else
                    {
                        _onAllScenesUnloaded();
                    }
                }
            };

            SceneManager.sceneLoaded += (Scene s, LoadSceneMode mode) =>
            {
                _loadedScenes.Push(s.name);

                //Last loaded scene is always active
                SceneManager.SetActiveScene(s);

                if (_scenesToLoad.Count > 0)
                {
                    SceneManager.LoadSceneAsync(_scenesToLoad.Dequeue(), LoadSceneMode.Additive);
                }
                else if(_loadedScenes.Count > 0)
                {
                    //all scenes loaded. hide bootstrapper ui.
                    ShowUI = false;
                    _onAllScenesLoaded();
                }
            };

            _existingLevelSceneNames = new List<string>();
            for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
            {
                string[] pathParts = SceneUtility.GetScenePathByBuildIndex(i).Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
                string sceneName = pathParts[pathParts.Length - 1].Replace(".unity", ""); ;

                if (!string.IsNullOrEmpty(_ignoreScenes.Find((s) => { return (s.ToLower().Trim() == sceneName.ToLower().Trim()); })))
                {
                    continue;
                }
                
                if (!string.IsNullOrEmpty(_preloadScenes.Find((s) => { return (s.ToLower().Trim() == sceneName.ToLower().Trim()); })))
                {
                    _existingPreloadSceneNames.Add(sceneName);
                }
                else if (!string.IsNullOrEmpty(_postloadScenes.Find((s) => { return (s.ToLower().Trim() == sceneName.ToLower().Trim()); })))
                {
                    _existingPostloadSceneNames.Add(sceneName);
                }
                else
                {
                    _existingLevelSceneNames.Add(sceneName);
                }
            }

            //Connect to editor (if in editor) and atuo bootstrap the last loaded scene if the option is checked
            //TODO: in the future this should be set from some persistant state so builds can bootstrap the starting scene automatically
            bool autoBootstrapScene = false;
            string autoBootstrapSceneName = string.Empty;

#if UNITY_EDITOR
            autoBootstrapScene = EditorPrefs.GetBool(LoadBootstrapper.AUTO_BOOTSTRAP_CURRENT_SCENE_TOGGLE, false);
            if (autoBootstrapScene)
            {
                autoBootstrapSceneName = EditorPrefs.GetString(LoadBootstrapper.AUTO_BOOTSTRAP_EDITOR_SCENE, string.Empty);
            }
#endif

            if(_bootstrapperHidden)
            {
                autoBootstrapScene = true;
                autoBootstrapSceneName = _startSceneIfBootstrapperHidden;
            }

            Debug.Log("Bootstrapping. Auto bootstrap is: " + autoBootstrapScene + "... auto scene name is: " + autoBootstrapSceneName);

            //check for a scene to auto load
            if (autoBootstrapScene && _existingLevelSceneNames.Contains(autoBootstrapSceneName))
            {
                ShowUI = false;
                LoadScene(autoBootstrapSceneName);
            }
            else
            {
                if(_bootstrapperHidden)
                {
                    Debug.LogWarning("[Boostrapper] All Scenes closed! Bootstrapper hidden so quitting application.");
                    Application.Quit();
                }
                else
                {
                    ShowUI = true;
                }
            }
        }

        private void OnGUI()
        {
            if(!ShowUI)
            {
                if(Input.GetKeyUp(KeyCode.Escape) && !_bootstrapperHidden)
                {
                    //will unload and show UI once done
                    StartUnload(true, ()=> 
                    {
                        ShowUI = true;
                    });
                }
                return;
            }

            float padding = 10;
            Vector2 screenSize = ScreenExtensions.GetGameViewSize();
            GUILayout.BeginArea(new Rect(padding, padding, screenSize.x - padding * 2, screenSize.y - padding * 2));

            GUILayout.Label("Load Scenes: ");
            foreach (string sceneName in _existingLevelSceneNames)
            {
                if (GUILayout.Button(sceneName))
                {
                    LoadScene(sceneName);
                    break;
                }
            }

            GUILayout.Space(30);

            bool enabledBefore = GUI.enabled;
            if (Application.isEditor)
            {
                GUI.enabled = false;
            }

            if (GUILayout.Button("Quit"))
            {
                Application.Quit();
            }

            GUI.enabled = enabledBefore;

            GUILayout.EndArea();
        }

        /// <summary>
        /// Load a level scene while keeping preload scenes. Will reload postload scenes
        /// </summary>
        public void LoadLevel(string sceneName, Action onLoaded)
        {
            StartUnload(false, ()=> 
            {
                LoadScene(sceneName, false, onLoaded);
            });
        }

        private void LoadScene(string sceneName, bool loadPreloadScenes = true, Action onLoaded = null)
        {
            _onAllScenesLoaded = delegate { };
            _onAllScenesLoaded += () =>
            {
                onLoaded?.Invoke();
            };

            //check if this scene is independent. if so don't preload/postload
            bool preloadPostLoad = true;
            foreach (string indieScene in _independentSceneNames)
            {
                if (indieScene.ToLower().Trim() == sceneName.ToLower().Trim())
                {
                    preloadPostLoad = false;
                }
            }

            if (preloadPostLoad && loadPreloadScenes)
            {
                foreach (string preloadSceneName in _existingPreloadSceneNames)
                {
                    _scenesToLoad.Enqueue(preloadSceneName);
                }
            }

            _scenesToLoad.Enqueue(sceneName);

            if (preloadPostLoad && !_skipPostloadScenes.Contains(sceneName))
            {
                foreach (string postloadSceneName in _existingPostloadSceneNames)
                {
                    _scenesToLoad.Enqueue(postloadSceneName);
                }
            }

            //will load all queued scenes async and in order then hide bootstrappper UI
            if (_scenesToLoad.Count > 0)
            {
                SceneManager.LoadSceneAsync(_scenesToLoad.Dequeue(), LoadSceneMode.Additive);
            }
        }

        private void StartUnload(bool unloadPreloadScenes, Action onFinished)
        {
            _unloadPreloadScenes = unloadPreloadScenes;
            _onAllScenesUnloaded = delegate { };
            _onAllScenesUnloaded += onFinished;

            if (_loadedScenes.Count > 0)
            {
                if (!_unloadPreloadScenes)
                {
                    string toUnload = _loadedScenes.Peek();
                    if(_existingPreloadSceneNames.Contains(toUnload))
                    {
                        _onAllScenesUnloaded();
                        return;
                    }
                    else
                    {
                        SceneManager.UnloadSceneAsync(_loadedScenes.Pop());
                    }
                }
                else
                {
                    SceneManager.UnloadSceneAsync(_loadedScenes.Pop());
                }   
            }
            else
            {
                _onAllScenesUnloaded();
            }
        }
    }
}
