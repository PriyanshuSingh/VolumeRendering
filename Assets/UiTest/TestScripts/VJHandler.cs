using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class VJHandler : MonoBehaviour,IDragHandler, IPointerUpHandler, IPointerDownHandler {
    private Image jsContainer;
    private Image joystick;

    public Vector3 InputDirection ;

    void Start(){

        jsContainer = GetComponent<Image>();
        joystick = transform.GetChild(0).GetComponent<Image>(); //this command is used because there is only one child in hierarchy
        InputDirection = Vector3.zero;
    }

    public void OnDrag(PointerEventData ped){


        Vector2 position = Vector2.zero;

        //To get InputDirection relative to pivot point
        RectTransformUtility.ScreenPointToLocalPointInRectangle(jsContainer.rectTransform,ped.position,ped.pressEventCamera,out position);

        //normalize positions
        position.x /= jsContainer.rectTransform.sizeDelta.x;
        position.y /= jsContainer.rectTransform.sizeDelta.y;


        //convert to range (-1,1)
        float x = (jsContainer.rectTransform.pivot.x == 1f) ? position.x *2 + 1 : position.x *2 - 1;
        float y = (jsContainer.rectTransform.pivot.y == 1f) ? position.y *2 + 1 : position.y *2 - 1;

        InputDirection = new Vector3 (x,y,0);
        Debug.Log("input direction is "+InputDirection);


        InputDirection = (InputDirection.magnitude > 1) ? InputDirection.normalized : InputDirection;
//
        //to define the area in which joystick can move around
        joystick.rectTransform.anchoredPosition = new Vector3 (InputDirection.x * (jsContainer.rectTransform.sizeDelta.x/3),InputDirection.y * (jsContainer.rectTransform.sizeDelta.y)/3);

    }

    public void OnPointerDown(PointerEventData ped){


//        Debug.Log("called pointer down handler");
        OnDrag(ped);
    }

    public void OnPointerUp(PointerEventData ped){

//        InputDirection = Vector3.zero;
        joystick.rectTransform.anchoredPosition = Vector3.zero;


    }
}