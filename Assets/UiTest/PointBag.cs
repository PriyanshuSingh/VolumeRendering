using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PointBag : MonoBehaviour,IPointerClickHandler
{




    public GameObject pointPrefab;
    public List<GameObject> points;
    public bool pointsListDirty = false;
    private RectTransform myRect;

// Use this for initialization
	void Start ()
	{






	    myRect = GetComponent<RectTransform>();
	    points = new List<GameObject>();

	    Assert.IsTrue(pointPrefab !=null);

	}

    public void OnPointerClick(PointerEventData eventData)
    {

        addPoint(eventData);

    }


    void Update () {

    }



    void LateUpdate()
    {

        if (pointsListDirty)
        {
            pointsListDirty = false;
            points.Sort((a,b)=>a.GetComponent<RectTransform>().anchoredPosition.x.CompareTo(b.GetComponent<RectTransform>().anchoredPosition.x));

        }

    }




    void addPoint(PointerEventData eventData)
    {
        var point = Instantiate(pointPrefab);
        var pointRect = point.GetComponent<RectTransform>();

        Vector2 position = Vector2.zero;


        pointRect.parent = myRect;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect,eventData.position,null,out position);

        //clamp spawn position inside the parent
        position.x = Mathf.Clamp(position.x,pointRect.sizeDelta.x*pointRect.pivot.x,myRect.sizeDelta.x-pointRect.sizeDelta.x*(1-pointRect.pivot.x));
        position.y = Mathf.Clamp(position.y,pointRect.sizeDelta.y*pointRect.pivot.y,myRect.sizeDelta.y-pointRect.sizeDelta.y*(1-pointRect.pivot.y));

        pointRect.anchoredPosition = position;

        points.Add(point);
        pointsListDirty = true;
    }
}
