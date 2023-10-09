using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
[ExecuteInEditMode]
public class UILobbyMenu : MonoBehaviour
{
    [SerializeField] private UIDocument _uiDocument;
    [SerializeField] private StyleSheet _styleSheet;
    void OnEnable()
    {
        StartCoroutine(Generate());
    }
    
    private void OnValidate()
    {
        if (!isActiveAndEnabled) return;
        if (Application.isPlaying) return;
        StartCoroutine(Generate());
    }
     private IEnumerator Generate()
    {
        yield return null;
        var root = _uiDocument.rootVisualElement;
        root.Clear();
        root.styleSheets.Add(_styleSheet);
        var container = Create("container");
        var cursorOverlay = Create("cursorOverlay");
        root.Add(cursorOverlay);
        var label = new Label();
        label.AddToClassList("mainMenuTitle");
        label.text = "Lobbies ";
        container.Add(label);
        var menu = Create("menuPaper");
        float startingY = 11;
        float targetY = 0; // Target X position
        float duration = 0.1f; // Duration in seconds
        float elapsedTime = 0;
        menu.schedule.Execute(() =>
        {
            elapsedTime += Time.deltaTime;

            if (elapsedTime < duration)
            {
                float t = elapsedTime / duration;
                float newY = Mathf.Lerp(startingY, targetY, t);
                menu.transform.position = new Vector3(menu.transform.position.x,newY, menu.transform.position.z);
            }
        }).Every(16); 
        var scrollContainer = new ScrollView();
        scrollContainer.mode = ScrollViewMode.Vertical;
        scrollContainer.horizontalScrollerVisibility = ScrollerVisibility.Hidden;
        scrollContainer.verticalScrollerVisibility = ScrollerVisibility.AlwaysVisible;
        scrollContainer.AddToClassList("scrollContainer");
        for (var i = 0; i < 24; i++)
        {
            var lab = new Label("HELLO :)");
            lab.AddToClassList("lobbyLabel");
            scrollContainer.contentContainer.Add(lab);
        }
        // Add label to ScrollView's contentContainer
        menu.Add(scrollContainer);
        container.Add(menu);

        root.Add(container);
    }
     
     private Button AddButtonTo(VisualElement container, String text, params string[] classNames)
     {
         var button = Create<Button>(classNames);
         var buttonText = new Label();
         buttonText.AddToClassList("menuButtonText");
         buttonText.text = text;
         button.Add(buttonText);
         container.Add(button);
         return button;
     }

     VisualElement Create(params string[] classNames)
     {
         return Create<VisualElement>(classNames);
     }

     T Create<T>(params string[] classNames) where T : VisualElement, new()
     {
         var ele = new T();
         foreach (var className in classNames)
         {
             ele.AddToClassList(className);
         }
         return ele;
     }
     
}
