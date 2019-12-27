using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using System.Collections.Generic;

class PromptData
{
    public GameObject canvas;
    public GameObject image;
    public RectTransform canvasTransform;
    public RectTransform imageTransform;
}

public class InputPrompter : MonoBehaviour 
{
    public string playerProfileName;
    public float zOffset2D = -5;

    //soft cap. if more than the number prompts are continuously being drawn none will be removed.
    int cacheSize = 15;

    //could use a dictionary here. should?
    List<Triple<string, PromptData, int>> prompts;

    private InputManager manager = null;
    private InputManager.PlayerProfile profile;

    public void Start()
    {
        manager = FindObjectOfType<InputManager>();
        if (!manager) Debug.LogError("Input Prompter can't find InputManager.");

        profile = manager.GetPlayerProfile(playerProfileName);
    }

    public void Draw(string action, Vector2 position)
    {
        Draw(action, new Vector3(position.x, position.y, zOffset2D), new Vector3(0, 0, -1));
    }

    public void Draw(string action, Vector3 position, Vector3 forward)
    {
        action = action.ToUpper();
        
        bool exists = false;
        Triple<string, PromptData, int> data = null;

        //check if prompt already exists in cache and store
        foreach(var triple in prompts)
        {
            if(triple.First.Equals(action))
            {
                exists = true;
                data = triple;
            }
        }

        //else load prompt
        if(!exists)
        {
            profile = manager.GetPlayerProfile(playerProfileName);
            InputCode code = profile.GetInputForAction(action);
            Sprite icon = manager.GetIconForInput(code);

            data = new Triple<string, PromptData, int>();

            data.First = action.ToUpper();
            data.Second = CreatePrompt(icon);

            prompts.Add(data);
        }

        //draw now that we have data
        DrawExact(data, position, forward);
    }

    private PromptData CreatePrompt(Sprite icon)
    {
        //TODO: 
        return null;
    }

    private void DrawExact(Triple<string, PromptData, int> data, Vector3 position, Vector3 forward)
    {
        //TODO:
  
    }


    //clean up undrawn prompts when cache size is over expected
    void LateUpdate()
    {
           for(int i = 0; i < prompts.Count; i++) 
           {
               if(prompts[i].Third == 0 && prompts.Count > cacheSize)
               {
                   Destroy(prompts[i].Second.canvas);
                   prompts.RemoveAt(i);
               }
               else if(prompts[i].Third == 1)
               {
                   prompts[i].Third--;
               }
               else
               {
                   prompts[i].Second.canvas.SetActive(false);
               }
           }
    }

}
