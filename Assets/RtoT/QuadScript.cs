using UnityEngine;
using System.Collections;

public class QuadScript : MonoBehaviour {



    private Material _mat;

    private int index;
    void Awake()
    {
        _mat = GetComponent<MeshRenderer>().material;
        index = 0;


    }
    // Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {


	    _mat.SetInt("index3D",index);
	    index = (index + 1) % 256;

	}
}
