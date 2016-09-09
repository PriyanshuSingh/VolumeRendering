using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

public class Geometry : MonoBehaviour {




    [SerializeField]
    private Texture2D[] slices ;


    [Header("Volume texture size. These must be a power of 2")]
    [SerializeField] private int volumeWidth = 256;
    [SerializeField] private int volumeHeight = 256;
    [SerializeField] private int volumeDepth = 256;


    private Texture3D _volumeBuffer;
    private Material _mat;



// Use this for initialization
	void Start () {


	    //TODO see how to programatically load raw textures in Unity
	    if (slices == null)
	    {

//	        Debug.Log("failed to load textures");
	    }
	    else
	    {
//	        Debug.Log("size of slices " + slices.Length);
	    }


	    GenerateVolumeTexture();



	}
	
	// Update is called once per frame
	void Update () {



        transform.Rotate(Vector3.up,100* Time.deltaTime);



	}


    private void OnDestroy()
    {
        if (_volumeBuffer != null)
        {
            Destroy(_volumeBuffer);
        }
    }


    private void GenerateVolumeTexture()
    {
        // sort
        System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

        // use a bunch of memory!
        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;

//         skip some slices if we can't fit it all in
        var countOffset = (slices.Length - 1) / (float) d;

        var volumeColors = new Color[w * h * d];

        var sliceCount = 0;
        var sliceCountFloat = 0f;
        for (int z = 0; z < d; z++)
        {
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);


            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                        var idx = x + y * w + z * w * h;
                        volumeColors[idx] = slices[sliceCount].GetPixelBilinear(x / (float) w, y / (float) h);

                }
            }
        }

        _volumeBuffer.SetPixels(volumeColors);
        _volumeBuffer.Apply();
        GetComponent<MeshRenderer>().material.SetTexture("_Volume", _volumeBuffer);
        //TODO see C# release way
//        slices = null;

    }
}
