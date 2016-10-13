using UnityEngine;
using System.Collections;
using UnityEngine.UI.Extensions;

public class TestLine : MonoBehaviour
{


    private UILineRenderer lineComp;


    public int yMultiplier = 10;
    public int xMultiplier = 10;
    public int sampleRange = 100;

    private int elements = 1000;

    private Vector2[] points;
    // Use this for initialization
	void Start ()
	{
	    lineComp = GetComponent<UILineRenderer>();
	    if (lineComp == null)
	    {
	        Debug.Log("cannot find the lineComp");
	    }



	    points  =new Vector2[elements];
	    for (int i = 0; i < elements; ++i)
	    {
	        points[i] = new Vector2(i*xMultiplier,yMultiplier*Mathf.Sin(Mathf.PI*i*(1.0f*sampleRange/elements)));
	    }


	    lineComp.Points = points;



	}
	
	//LateUpdate is called once per frame
	void LateUpdate () {

	    for (int i = 0; i < elements; ++i)
	    {
	        points[i] = new Vector2(i*xMultiplier,yMultiplier*Mathf.Sin(Mathf.PI*i*(1.0f*sampleRange/elements)));
	    }

	    lineComp.Points = points;

	    lineComp.SetAllDirty();

	
	}
}
