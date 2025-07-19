using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonClickHandler : SingletonBase<ButtonClickHandler>, IPointerDownHandler, IPointerUpHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    //handle pointer clicked event
    public void OnPointerUp(PointerEventData eventData)
    {
        // Handle pointer click event here
        //Debug.Log("Sprint button clicked");

        UpdateText.Instance.isSprintButtonPressed = false; // Set sprint button pressed when pointer clicked
    }
    // This method is required by IPointerDownHandler
    public void OnPointerDown(PointerEventData eventData)
    {
        // Handle pointer down event here
        UpdateText.Instance.isSprintButtonPressed = true; // Set sprint button pressed when pointer down
        //Debug.Log("Sprint button pressed");

    }
}
