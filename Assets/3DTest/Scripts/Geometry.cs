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


    private MeshRenderer myRenderer;
    private Texture3D _volumeBuffer;
    private Color[] volumeNormalColors;
    private Transform dirLightTransform;
    private Material _mat;

	//TAG: TF BEGIN
	private Texture2D _transferBuffer;
	[Header("Control Points (0 and 255 are necessary)")]
	[SerializeField] private ControlPoint[] controlPoints;
	//TAG: TF END

// Use this for initialization
	void Start ()
	{


	    myRenderer = GetComponent<MeshRenderer>();



	    dirLightTransform = GameObject.FindGameObjectWithTag("DirLight").transform;

	    //TODO see how to programatically load raw textures in Unity
	    if (slices == null)
	    {

	        Debug.Log("failed to load textures");
	    }
	    else
	    {
//	        Debug.Log("size of slices " + slices.Length);
	    }






	    //TODO merge volume and normal texture
	    //volume is greyscale thus can be fitted in alpha component
	    GenerateVolumeAndNormal(1);




	}


    private float timeToChange = 10.0f;
    private float acc = 0;
    private Vector3 start = new Vector3(0.5f,0.0f,0.5f);
    private Vector3 end = new Vector3(0.5f,0.8f,0.5f);

    // Update is called once per frame
	void Update () {





	    if (acc > timeToChange)
	    {
	        acc = 0;
	        Vector3 temp = start;
	        start = end;
	        end = temp;
	    }
	    else
	    {
	        acc += Time.deltaTime;


	    }


	    //TODO currently Idenity if stays this remove this multiplication
//	    Matrix4x4 lightInverter = transform.worldToLocalMatrix;
	    myRenderer.material.SetVector("L",Vector3.Normalize(-dirLightTransform.forward));










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


    private void GenerateVolumeAndNormal(int sampleSize)
    {



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
			}
		}
		tsf [255] = controlPoints [controlPoints.Length - 1].color;
		_transferBuffer.SetPixels (tsf);
		_transferBuffer.Apply();
        _transferBuffer.wrapMode = TextureWrapMode.Clamp;
        // TAG: TF END

        //TODO last arguement mipmapping refer graphics runner

        // sort
        System.Array.Sort(slices, (x, y) => int.Parse(x.name).CompareTo(int.Parse(y.name)));
        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);

        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;




//        skip some slices if we can't fit it all in
        var countOffset = (slices.Length - 1) / (float) d;

        volumeNormalColors = new Color[w * h * d];

        var sliceCount = 0;
        var sliceCountFloat = 0f;


        //fill in colors
        for (int z = 0; z < d; z++)
        {
            //TODO interpolate between textures based on this factor
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                        var idx = x + y * w + z * w * h;



                    if (x<2 || y<2  || x > w - 3 || y > h - 3)
                        volumeNormalColors[idx].a = 0;
                    else
                        //store the greyscale value in alpha of 3D Texture
                        volumeNormalColors[idx].a = slices[sliceCount].GetPixelBilinear(x / (float) w, y / (float) h).r;


                }
            }
        }





        int n = sampleSize;
        Vector3 s1, s2;
        //fill in normal vectors
        for (int z = 0; z < d; z++)
        {
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    var idx = x + y * w + z * w * h;
                        s1.x = sampleVolume(x - n, y, z);
                        s2.x = sampleVolume(x + n, y, z);
                        s1.y = sampleVolume(x, y - n, z);
                        s2.y = sampleVolume(x, y + n, z);
                        s1.z = sampleVolume(x, y, z - n);
                        s2.z = sampleVolume(x, y, z + n);
                        var vec = Vector3.Normalize(s2 - s1);
                        volumeNormalColors[idx].r = vec.x;
                        volumeNormalColors[idx].g = vec.y;
                        volumeNormalColors[idx].b = vec.z;
                        //TODO check this with priyanshu
                        if (float.IsNaN(volumeNormalColors[idx].r))
                            volumeNormalColors[idx] = new Color(0,0,0,volumeNormalColors[idx].a);



                }
            }
        }

//        filterNxNxN(3);







        _volumeBuffer.SetPixels(volumeNormalColors);
        _volumeBuffer.Apply();
        //TODO this eliminates some of problem but currently no way to have border sampling in Unity3D
        //porbably just add a border of black voxels around
        _volumeBuffer.wrapMode = TextureWrapMode.Clamp;
        myRenderer.material.SetTexture("_Volume", _volumeBuffer);

        //TAG: TF BEGIN
        myRenderer.material.SetTexture ("_transferF", _transferBuffer);
        //TAG: TF END



		//set to null and pray to GC
        slices = null;

    }

    private float sampleVolume(int x, int y, int z)
    {
        x = Mathf.Clamp(x, 0, _volumeBuffer.width - 1);
        y = Mathf.Clamp(y, 0, _volumeBuffer.height - 1);
        z = Mathf.Clamp(z, 0, _volumeBuffer.depth - 1);
        //accessing r can access any one here for greyscale value
        return volumeNormalColors[x + y * _volumeBuffer.height + z *_volumeBuffer.width * _volumeBuffer.height].a;
    }

    private void filterNxNxN(int n)
    {
        int index = 0;
        for (int z = 0; z < _volumeBuffer.depth; z++)
        {
            for (int y = 0; y < _volumeBuffer.height; y++)
            {
                for (int x = 0; x < _volumeBuffer.width; x++,index++)
                {
                    var sampleResult = sampleNxNxN(x, y, z, n);
                    volumeNormalColors[index].r = sampleResult.x;
                    volumeNormalColors[index].g = sampleResult.y;
                    volumeNormalColors[index].b = sampleResult.z;

                }
            }
        }
    }
    private bool isInBounds(int x, int y, int z)
    {
        return  x >= 0 && x < _volumeBuffer.width && y >= 0 && y < _volumeBuffer.height &&  z >= 0 && z < _volumeBuffer.depth;
    }

    private Vector3 sampleNxNxN(int x, int y, int z, int n)
    {
        n = (n - 1) / 2;

        Vector3 average = Vector3.zero;
        int num = 0;

        for (int k = z - n; k <= z + n; k++)
        {
            for (int j = y - n; j <= y + n; j++)
            {
                for (int i = x - n; i <= x + n; i++)
                {
                    if (isInBounds(i, j, k))
                    {
                        average += sampleGradients(i, j, k);
                        num++;
                    }
                }
            }
        }

        average /= (float)num;
        if (average.x != 0.0f && average.y != 0.0f && average.z != 0.0f)
            average.Normalize();

        return average;
    }

    private Vector3 sampleGradients(int x, int y, int z)
    {
        var v = volumeNormalColors[x + y * _volumeBuffer.width + z * _volumeBuffer.height * _volumeBuffer.width];
        return new Vector3(v.r,v.g,v.b);
    }



}
