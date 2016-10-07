﻿using UnityEngine;
using System.Collections;

public class RotateEmpty : MonoBehaviour {

	void Start () {

	}
	
    private float yaw = 0.0f;
    private float pitch = 0.0f;




    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private void Update(){




        if (Input.GetMouseButton(0))
        {
            yaw += speedH * Input.GetAxis("Mouse X");
            pitch -= speedV * Input.GetAxis("Mouse Y");


            transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

        }

    }
}