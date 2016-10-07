using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {


    private float yaw = 0.0f;
    private float pitch = 0.0f;

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    void Start () {






	
	}
	void Update () {





	    if (Input.GetKey(KeyCode.Space))
	    {


	        yaw += speedH;
	        transform.eulerAngles = new Vector3(0.0f, yaw, 0.0f);

	    }







	}




}
