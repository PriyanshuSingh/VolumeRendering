using UnityEngine;
using System.Collections;

public class Create2DTex : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{


	    StartCoroutine(Stuff());

	}
	
	// Update is called once per frame
	void Update () {
	
	}


    IEnumerator Stuff()
    {

        yield return new WaitForSeconds(2.0f);



        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
        for (int i = 0; i < vertices.Length; ++i)
        {
            Debug.Log(vertices[i]);

        }




    }

}
