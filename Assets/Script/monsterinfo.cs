using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class monsterinfo : MonoBehaviour
{
    [SerializeField] Texture2D hoveron;
    [SerializeField] Texture2D hoveroff;

    void Start()
    {
        Cursor.SetCursor(hoveroff, Vector2.zero, CursorMode.ForceSoftware);

        
    }
    private void OnMouseEnter()
    {
        Cursor.SetCursor(hoveron, Vector2.zero, CursorMode.ForceSoftware);
    }
    private void OnMouseExit()
    {
        Cursor.SetCursor(hoveroff, Vector2.zero, CursorMode.ForceSoftware);
    }
  
   
}
 