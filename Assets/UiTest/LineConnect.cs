﻿using System;
using UnityEngine;
using System.Collections;

using System.Linq;
using UnityEngine.UI.Extensions;

public class LineConnect : MonoBehaviour {



    private UILineRenderer lineComp;

    // Use this for initialization
	void Start ()
	{


	    lineComp = GetComponent<UILineRenderer>();

	}
	
	// Update is called once per frame
	void Update () {
	
	}


    void LateUpdate ()
    {



        var parentScript = GetComponent<RectTransform>().parent.GetComponent<PointBag>();

		Vector2 [] arr = new Vector2[parentScript.linePoints.Count];
		for (int i = 0; i < parentScript.linePoints.Count; i++)
        {
			arr [i] = parentScript.linePoints [i];
        }

        lineComp.Points = arr;
        lineComp.SetAllDirty();


    }
}

