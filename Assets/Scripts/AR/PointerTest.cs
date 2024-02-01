using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
 
public class PointerTest : MonoBehaviour, IPointerDownHandler, IPointerUpHandler {
 
    // Reason why it is created, to make sure that the event is working
    
    // Issue arises when 2 canvas exist in the scene

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log("Pointer Test: Point Down");
    }
 
    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log("Pointer Test: Point Up");
    }
}