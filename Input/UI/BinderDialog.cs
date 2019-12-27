using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class BinderDialog : MonoBehaviour
{
    [SerializeField]
    private Font font;

    [Range(1,5)][SerializeField]
    private int maxPlayers;

    private GameObject tabRoot;

    //foreach player we have a tab with UI content encapsulated by PlayerBindings.
    List<Pair<GameObject, PlayerBindings>> playerBindings = new List<Pair<GameObject,PlayerBindings>>();
    private class PlayerBindings
    {
        public GameObject scrollView;
        public GameObject content;
        public GameObject scrollBar;
    }

    void Awake()
    {
       // tabRoot = new GameObject("TabRoot", typeof<RectTransform>)
    }


    public void Open()
    {
        tabRoot.SetActive(false);
    }

    public void Close() 
    {
        tabRoot.SetActive(false);
    }

    public void SetMaxPlayers(int players)
    {
        players = Mathf.Max(0, Mathf.Min(5, players));
    }

    private PlayerBindings AddPlayerBindingsMenu(GameObject root)
    {
        //initialise
        PlayerBindings bindings = new PlayerBindings();
        bindings.scrollView = new GameObject("ScrollView", typeof(Image), typeof(ScrollRect), typeof(Mask));
        bindings.content = new GameObject("Content", typeof(RectTransform), typeof(ContentSizeFitter));
        bindings.scrollBar = new GameObject("ScrollBar", typeof(Image), typeof(Scrollbar));
        GameObject slidingArea = new GameObject("SlidingArea", typeof(RectTransform));
        GameObject scrollHandle = new GameObject("Handle", typeof(Image));

        //build heirachy
        bindings.scrollView.transform.SetParent(root.transform);
            bindings.content.transform.SetParent(bindings.scrollView.transform);
            bindings.scrollBar.transform.SetParent(root.transform);
        slidingArea.transform.SetParent(bindings.scrollBar.transform);
            scrollHandle.transform.SetParent(slidingArea.transform);
      
        //set layouts
        SetAnchorStretch(root);
        SetSize(root, 0f,0f,0f,0f);

        SetAnchorStretch(bindings.scrollView);
        SetSize(bindings.scrollView, 0f,0f,0f,0f);

        SetAnchorStretch(bindings.content);
        SetSize(bindings.content, 0f,0f,0f,0f);

        SetAnchorStretch(slidingArea);
        SetSize(slidingArea, 0f,0f,0f,0f);

        SetAnchorRightStretch(bindings.scrollBar);
        SetSize(bindings.scrollBar, 0f,0f,13f,0f);


        //set component values
        bindings.scrollView.GetComponent<Mask>().showMaskGraphic = false;
        bindings.scrollView.GetComponent<ScrollRect>().content = bindings.content.GetComponent<RectTransform>();
        bindings.scrollView.GetComponent<ScrollRect>().verticalScrollbar = bindings.scrollBar.GetComponent<Scrollbar>();

        bindings.scrollBar.GetComponent<Scrollbar>().handleRect = scrollHandle.GetComponent<RectTransform>();
        bindings.scrollBar.GetComponent<Scrollbar>().SetDirection(Scrollbar.Direction.BottomToTop, true);

        bindings.content.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;


        
        return bindings;
    }

    //helpers for handling positioning nad scaling

    private void SetAnchorStretch(GameObject obj)
    {
        RectTransform trans = obj.GetComponent<RectTransform>();

        trans.anchorMin = Vector2.zero;
        trans.anchorMax = new Vector2(1, 1);
        trans.pivot = new Vector2(0.5f, 0.5f);
    }

    private void SetAnchorRightStretch(GameObject obj)
    {
        RectTransform trans = obj.GetComponent<RectTransform>();

        trans.anchorMin = new Vector2(1, 0);
        trans.anchorMax = new Vector2(1, 1);
        trans.pivot = new Vector2(0.5f, 0.5f);        
    }

    private void SetSize(GameObject target, float x, float y, float z,  float w)
    {
        RectTransform trans = target.GetComponent<RectTransform>();
        trans.sizeDelta = new Vector2(z, w);
        trans.anchoredPosition = new Vector2(x,y);
    }
}
