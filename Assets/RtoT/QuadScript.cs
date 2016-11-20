using UnityEngine;
using System.Collections;

public class QuadScript : MonoBehaviour {



    private Material _mat;

    private int index;
    void Awake()
    {
//        _mat = GetComponent<MeshRenderer>().material;
//        index = 0;
//        var verts = GetComponent<MeshFilter>().mesh.vertices;
//        for (int i = 0; i < verts.Length; i++)
//        {
//            Debug.Log(verts[i]);
//        }


    }
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {



	       transform.Rotate(Vector3.up,60*Time.deltaTime);


	}
}
