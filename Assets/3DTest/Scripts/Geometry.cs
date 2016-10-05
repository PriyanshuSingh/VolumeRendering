using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;

// TAG: TF BEGIN
using System;
using System.CodeDom.Compiler;

[Serializable]
public class ControlPoint{
	public Color color;
	public int isoValue;
}
// TAG: TF END


public class Geometry : MonoBehaviour {




    [SerializeField]
    private Texture2D[] slices;


    [Header("Volume texture size. These must be a power of 2")]
    [SerializeField] private int volumeWidth = 256;
    [SerializeField] private int volumeHeight = 256;
    [SerializeField] private int volumeDepth = 256;


    private Texture3D _volumeBuffer;
//    private Texture3D _normalBuffer;


    private Color[] volumeColors;

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



	    //TODO merge volume and normal texture
	    //volume is greyscale thus can be fitted in alpha component
	    GenerateVolumeTexture();
//	    GenerateVolumeNormal(1);



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
//        if (_normalBuffer != null)
//        {
//            Destroy(_normalBuffer);
//        }
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

        //TODO last arguement mipmapping refer graphics runner
	    _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



//        skip some slices if we can't fit it all in
        var countOffset = (slices.Length - 1) / (float) d;

        volumeColors = new Color[w * h * d];

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
    private void GenerateVolumeNormal(int sampleSize)
    {


        int n = sampleSize;
//        _normalBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



        var gradients = new Color[w * h * d];

        Vector3 s1, s2;

        int index = 0;
        for (int z = 0; z < d; z++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    s1.x = sampleVolume(x - n, y, z);
                    s2.x = sampleVolume(x + n, y, z);
                    s1.y = sampleVolume(x, y - n, z);
                    s2.y = sampleVolume(x, y + n, z);
                    s1.z = sampleVolume(x, y, z - n);
                    s2.z = sampleVolume(x, y, z + n);
                    var vec = Vector3.Normalize(s2 - s1);
                    gradients[index].r = vec.x;
                    gradients[index].g = vec.y;
                    gradients[index++].b = vec.z;
                    //TODO discuss this if
                    if (float.IsNaN(gradients[index - 1].r))
                        gradients[index - 1] = Color.black;
                }
            }
        }
//
//        _normalBuffer.SetPixels(gradients);
//        _normalBuffer.Apply();
//        GetComponent<MeshRenderer>().material.SetTexture("_Normal", _normalBuffer);
//
//


    }
    private float sampleVolume(int x, int y, int z)
    {
        x = Mathf.Clamp(x, 0, _volumeBuffer.height - 1);
        y = Mathf.Clamp(y, 0, _volumeBuffer.height - 1);
        z = Mathf.Clamp(z, 0, _volumeBuffer.depth - 1);
        //accessing r can access any one here for greyscale value
        return volumeColors[x + y * _volumeBuffer.height + z *_volumeBuffer.width * _volumeBuffer.height].r;
    }


}
