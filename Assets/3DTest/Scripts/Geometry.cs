using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
// TAG: TF BEGIN
using System;
using System.CodeDom.Compiler;

[Serializable]
public class ControlPoint{
	public Vector4 mColor;
	public int mIsoValue;

	public ControlPoint(float r, float g, float b, int isoValue){
		mColor.x = r;
		mColor.y = g;
		mColor.z = b;
		mIsoValue = isoValue;
	}

	public ControlPoint(float alpha, int isoValue){
		mColor.x = mColor.y = mColor.z = 0.0f;
		mColor.w = alpha;
		mIsoValue = isoValue;
	}

}

class Cubic{
	private Vector4 a, b, c, d;
	public Cubic(Vector4 a, Vector4 b, Vector4 c, Vector4 d){
		this.a = a;
		this.b = b;
		this.c = c;
		this.d = d;
	}

	public Vector4 getPointOnSpline(float s){
		return (((d * s) + c) * s + b) * s + a;
	}

	private static Vector4 divide(Vector4 a, Vector4 b){
		return new Vector4 (a.x / b.x, a.y / b.y, a.z / b.z, a.w / b.w);
	}

	private static Vector4 mult(Vector4 a, Vector4 b){
		return new Vector4 (a.x * b.x, a.y * b.y, a.z * b.z, a.w * b.w);
	}

	public static Cubic[] calculateCubicSpline(int n, List<ControlPoint> v){
		Vector4[] gamma = new Vector4[n + 1];
		Vector4[] delta = new Vector4[n + 1];
		Vector4[] D = new Vector4[n + 1];
		int i;

		gamma [0] = Vector4.zero;
		gamma [0].x = 1.0f / 2.0f;
		gamma [0].y = 1.0f / 2.0f;
		gamma [0].z = 1.0f / 2.0f;
		gamma [0].w = 1.0f / 2.0f;
		for( i = 1; i < n; i++){
			gamma[i] = divide(Vector4.one ,(4 * Vector4.one) - gamma[i - 1]);
		}
		gamma[n] = divide(Vector4.one , (2 * Vector4.one) - gamma[n - 1]);

		delta[0] = 3 * mult(v[1].mColor - v[0].mColor , gamma[0]);
		for (i = 1; i < n; i++) {
			delta [i] = mult( 3 *(v [i + 1].mColor - v [i - 1].mColor) - delta [i - 1] , gamma [i]);
		}
		delta [n] = mult(3 * (v [n].mColor - v [n - 1].mColor) - delta [n - 1], gamma [n]);

		D [n] = delta [n];
		for (i = n - 1; i >= 0; i--) {
			D [i] = delta [i] - mult(gamma [i] , D [i + 1]);
		}

		Cubic[] C = new Cubic[n];
		for (i = 0; i < n; i++) {
			C [i] = new Cubic (v [i].mColor, D [i], 3 * (v [i + 1].mColor - v [i].mColor) - 2 * D [i] - D [i + 1],
				2 * (v [i].mColor - v [i + 1].mColor) + D [i] + D [i + 1]);
		}
		return C;
	}
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
//	[Header("Control Points (0 and 255 are necessary)")]
//	[SerializeField] private ControlPoint[] controlPoints;
	private List<ControlPoint> mAlphaKnots;
	private List<ControlPoint> mColorKnots;
	//TAG: TF END

// Use this for initialization
	void Start ()
	{


	    myRenderer = GetComponent<MeshRenderer>();



	    dirLightTransform = GameObject.FindGameObjectWithTag("DirLight").transform;

	    //TODO see how to programatically load raw textures in Unity
	    if (slices == null)
	    {

//	        Debug.Log("failed to load textures");
	    }
	    else
	    {
//	        Debug.Log("size of slices " + slices.Length);
	    }




		mColorKnots = new List<ControlPoint> {
			new ControlPoint(0.0f, .7f, .61f, 0),
			new ControlPoint(0.0f, .7f, .61f, 200),
			new ControlPoint(1.0f, 0.0f, .85f, 202),
			new ControlPoint(1.0f, 0.0f, .85f, 256)
		};

		mAlphaKnots = new List<ControlPoint> {
			new ControlPoint(0.0f, 0),
			new ControlPoint(0.0f, 40),
			new ControlPoint(0.2f, 60),
			new ControlPoint(0.05f, 63),
			new ControlPoint(0.0f, 200),
			new ControlPoint(0.9f, 202),
			new ControlPoint(1.0f, 256)
		};

		computeTransferFunction ();

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
	    myRenderer.material.SetVector("L",Vector3.Normalize(dirLightTransform.forward));










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

	//TAG: TF BEGIN
	private void computeTransferFunction(){
		// TAG: TF BEGIN
//		System.Array.Sort (controlPoints, (x, y) => x.isoValue.CompareTo (y.isoValue));
//
//		var tsf = new Color[256];
//		for (int i = 1; i < controlPoints.Length; i++) {
//			int st = controlPoints [i - 1].isoValue;
//			int en = controlPoints [i].isoValue;
//			Color a = controlPoints [i - 1].color;
//			Color b = controlPoints [i].color;
//
//			for (int j = st; j < en; j++) {
//				float t = j - st;
//
//				t /= en - st + 1;
//				tsf [j] = Color.Lerp (a, b, t);
//			}
//		}
//		tsf [255] = controlPoints [controlPoints.Length - 1].color;

		// TAG: TF END

		Vector4[] transferFunc = new Vector4[256];
		List<ControlPoint> tempColorKnots = new List<ControlPoint> (mColorKnots);
		List<ControlPoint> tempAlphaKnots = new List<ControlPoint> (mAlphaKnots);

		Cubic[] colorCubic = Cubic.calculateCubicSpline (mColorKnots.Count - 1, tempColorKnots);
		Cubic[] alphaCubic = Cubic.calculateCubicSpline (mAlphaKnots.Count - 1, tempAlphaKnots);

		int numTF = 0;
		for (int i = 0; i < mColorKnots.Count - 1; i++) {
			int steps = mColorKnots [i + 1].mIsoValue - mColorKnots [i].mIsoValue;
			for (int j = 0; j < steps; j++) {
				float k = (float)j / (float)(steps - 1);
				transferFunc [numTF++] = colorCubic [i].getPointOnSpline (k);
			}
		}

		numTF = 0;
		for (int i = 0; i < mAlphaKnots.Count - 1; i++) {
			int steps = mAlphaKnots [i + 1].mIsoValue - mAlphaKnots [i].mIsoValue;
			for (int j = 0; j < steps; j++) {
				float k = (float)j / (float)(steps - 1);
				transferFunc [numTF++].w = alphaCubic [i].getPointOnSpline (k).w;
			}
		}
		Color[] data = new Color [256];

		for (int i = 0; i < 256; i++) {
			Vector4 color = transferFunc [i] * 255.0f;
			data [i].r = (int)Mathf.Clamp (color.x, 0, 255.0f);
			data [i].g = (int)Mathf.Clamp(color.y, 0, 255.0f);
			data [i].b = (int)Mathf.Clamp(color.z, 0, 255.0f);
			data [i].a = (int)Mathf.Clamp(color.w, 0, 255.0f);
			Debug.Log ("data at " + i + " is " + data [i]); 
		}
		_transferBuffer = new Texture2D (256, 1, TextureFormat.ARGB32, false);
		_transferBuffer.SetPixels (data);
		_transferBuffer.Apply ();
//		_transferBuffer.wrapMode = TextureWrapMode.Clamp;

	}
	//TAG: TF END


    private void GenerateVolumeAndNormal(int sampleSize)
    {



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
            sliceCountFloat += countOffset;
            sliceCount = Mathf.FloorToInt(sliceCountFloat);

            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                        var idx = x + y * w + z * w * h;
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


}
