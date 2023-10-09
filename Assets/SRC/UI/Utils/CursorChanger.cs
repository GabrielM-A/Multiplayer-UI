using UnityEngine;

public class CursorChanger : MonoBehaviour
{
    public Texture2D cursorTexture; // Assign this in the Inspector
    public Vector2 hotSpot = new Vector2(0, 0); // The "active" point of the cursor

    void Start()
    {
        // This will set the cursor for the entire game
        Cursor.SetCursor(cursorTexture, hotSpot, CursorMode.Auto);
    }
}