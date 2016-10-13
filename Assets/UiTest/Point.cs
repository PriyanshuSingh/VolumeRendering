using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;
public class Point : MonoBehaviour ,IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform myRect;
    private RectTransform parentRect;
    private PointBag parentScript;
    // Use this for initialization
	void Start ()
	{

	    myRect = GetComponent<RectTransform>();
	    parentRect = myRect.parent.GetComponent<RectTransform>();
	    parentScript = myRect.parent.GetComponent<PointBag>();

	}
	
	// Late Update is called once per frame
	void LateUpdate () {


	
	}


    public void OnDrag(PointerEventData eventData)
    {






        Vector2 newPos = myRect.anchoredPosition+eventData.delta;


        //anchor at left corner  of parent
        float lowerX = 0;//-parentRect.sizeDelta.x/2;
        float upperX = lowerX+parentRect.sizeDelta.x;

        float lowerY = 0;//-parentRect.sizeDelta.y/2;
        float upperY = lowerY+parentRect.sizeDelta.y;

        newPos.x = Mathf.Clamp(newPos.x, lowerX+myRect.sizeDelta.x*myRect.pivot.x, upperX-myRect.sizeDelta.x*(1-myRect.pivot.x));
        newPos.y = Mathf.Clamp(newPos.y, lowerY+myRect.sizeDelta.y*myRect.pivot.y, upperY-myRect.sizeDelta.y*(1-myRect.pivot.y));

        //TODO floating point cmp!!!
        if (newPos.x != myRect.anchoredPosition.x)
        {
           parentScript.pointsListDirty = true;

        }

        myRect.anchoredPosition = newPos;




    }


    public void OnPointerDown(PointerEventData eventData)
    {
    }
    public void OnPointerUp(PointerEventData eventData)
    {
    }

}
