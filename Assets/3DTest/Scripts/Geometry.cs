using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;

// TAG: TF BEGIN
using System;

[Serializable]
public class ControlPoint{
	public Color color;
	public int isoValue;
}
// TAG: TF END


public class Geometry : MonoBehaviour {




    [SerializeField]
    private Texture2D[] slices ;


    [Header("Volume texture size. These must be a power of 2")]
    [SerializeField] private int volumeWidth = 256;
    [SerializeField] private int volumeHeight = 256;
    [SerializeField] private int volumeDepth = 256;


    private Texture3D _volumeBuffer;
    private Material _mat;

	//TAG: TF BEGIN
	private Texture2D _transferBuffer;
	[Header("Control Points (0 and 255 are necessary)")]
	[SerializeField] private ControlPoint[] controlPoints;
	//TAG: TF END

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
        if (_transferBuffer != null)
        {
            Destroy(_transferBuffer);
        }
    }


    private void GenerateVolumeTexture()
    {
        // sort
        System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

		// TAG: TF BEGIN
		System.Array.Sort (controlPoints, (x, y) => x.isoValue.CompareTo (y.isoValue));
		_transferBuffer = new Texture2D (256, 1, TextureFormat.ARGB32, false);
		var tsf = new Color[256];
		for (int i = 1; i < controlPoints.Length; i++) {
			int st = controlPoints [i - 1].isoValue;
			int en = controlPoints [i].isoValue;
			Color a = controlPoints [i - 1].color;
			Color b = controlPoints [i].color;

			for (int j = st; j < en; j++) {
				float t = j - st;

				t /= en - st + 1;
				tsf [j] = Color.Lerp (a, b, t); 
//				Debug.Log (a + " " + b + " " + t);
//				Debug.Log (j + " " + tsf [j]);
			}
		}
		tsf [255] = controlPoints [controlPoints.Length - 1].color;
		_transferBuffer.SetPixels (tsf);
		_transferBuffer.Apply();
        // TAG: TF END

	    _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



//        skip some slices if we can't fit it all in
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
		//TAG: TF BEGIN
		GetComponent<MeshRenderer> ().material.SetTexture ("_transferF", _transferBuffer); 
        //TAG: TF END



		//set to null and pray to GC
        slices = null;

    }
}
