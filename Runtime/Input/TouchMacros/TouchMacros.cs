#if UNITY_EDITOR

using LSCore.Editor;
using UnityEngine;

[ExecuteAlways]
public partial class TouchMacros : MonoBehaviour
{
    [SerializeField] private Data[] touches;
    private Texture2D touchTexture;
    
    private void OnEnable()
    {
        GameViewExt.PostGUI -= OnGui;
        GameViewExt.PostGUI += OnGui;
        touchTexture = LSIcons.Get("touch.png");
    }
    
    private void OnDisable()
    {
        GameViewExt.PostGUI -= OnGui;
    }

    private void Update()
    {
        if (Application.isPlaying)
        {
            for (int i = 0; i < touches.Length; i++)
            {
                var data = touches[i];
                var keyCode = (KeyCode)data.key;
                
                if(Input.GetKeyDown(keyCode))
                {
                    touches[i].isTouching = true;
                    Down(data.cursorPosition);
                }
                else if(Input.GetKeyUp(keyCode))
                {
                    touches[i].isTouching = false;
                    Up(data.cursorPosition);
                }
            }
        }
    }

    private void OnGui(Rect rect)
    {
        rect = GUIUtility.GUIToScreenRect(rect);
        rect.y += 21;
        var viewRect = GameViewExt.ViewRect;
        var screenSize = GameViewExt.ScreenSize;
        var factor = (viewRect.width / screenSize.x);
        var touchRect = new Rect();
        touchRect.position = viewRect.position;
        var size = new Vector2(60, 60) * factor;
        var halfSize = size / 2;
        touchRect.size = size;
        touchRect.position -= halfSize;
        touchRect.y += 21;
        var textStyle = new GUIStyle();
        textStyle.fontStyle = FontStyle.Bold;
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontSize = (int)(30 * factor);

        for (int i = 0; i < touches.Length; i++)
        {
            var data = touches[i];
            var prevPos = touchRect.position;
            touchRect.position += new Vector2(data.xPosition * screenSize.x, data.yPosition * screenSize.y) * factor;
            touches[i].cursorPosition = rect.position + touchRect.position + halfSize;
            var prevColor = GUI.color;
            if (data.isTouching)
            {
                GUI.color = new Color(0.21f, 0.93f, 0.35f);
            }
            GUI.DrawTexture(touchRect, touchTexture, ScaleMode.ScaleToFit);
            GUI.color = prevColor;
            GUI.TextArea(touchRect, data.KeyLabel, textStyle);
            touchRect.position = prevPos;
        }
    }
}

#endif