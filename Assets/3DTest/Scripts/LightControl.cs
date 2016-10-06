using UnityEngine;
using System.Collections;

public class LightControl : MonoBehaviour {

	// Use this for initialization
	void Start () {





        StartCoroutine(rotator());


	
	}

    IEnumerator rotator() {
        while (true)
        {

//
//            for (int i = 0; i <= 180; i++)
//            {
//                for (int j = 0; j < 360; ++j)
//                {
//            transform.rotation = Random.rotation;
            Debug.Log("rotation is "+transform.rotation.eulerAngles);

//                }
//
//            }






            yield return new WaitForSeconds(4.0f);
        }
    }
	
    // Update is called once per frame
	void Update () {






//
//	    float xAxisValue = Input.GetAxis("Horizontal");
//	    float zAxisValue = Input.GetAxis("Vertical");
//
//	    transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));







	}




}
