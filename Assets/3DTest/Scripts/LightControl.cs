using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	// Use this for initialization
	void Start () {





	
	}
	
	// Update is called once per frame
	void Update () {




	    float xAxisValue = Input.GetAxis("Horizontal");
	    float zAxisValue = Input.GetAxis("Vertical");

	    transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));







	}
}
