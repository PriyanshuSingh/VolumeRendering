using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.Assertions;
using UnityEngine.UI;
using UnityEngine.EventSystems;


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

public class PointBag : MonoBehaviour,IPointerClickHandler
{



    public GameObject pointPrefab;
    public List<GameObject> points;
    public bool pointsListDirty = false;
    private RectTransform myRect;

	private List<ControlPoint> mAlphaKnots;
	private List<ControlPoint> mColorKnots;


    private Geometry geoScript;
	public List<Vector2> linePoints;
	public bool assoc = false;

    void Awake()
    {

        GetComponent<CanvasRenderer> ().SetAlpha (0.25f);
        myRect = GetComponent<RectTransform>();
        points = new List<GameObject>();

        Assert.IsTrue(pointPrefab !=null);

        mColorKnots = new List<ControlPoint> {
            new ControlPoint(0.91f, .7f, .61f, 0),
            new ControlPoint(0.91f, .7f, .61f, 80),
            new ControlPoint(1.0f, 1.0f, .85f, 82),
            new ControlPoint(1.0f, 1.0f, .85f, 256)
        };

        mAlphaKnots = new List<ControlPoint> {
            new ControlPoint(0.0f, 0),
            new ControlPoint(0.0f, 40),
            new ControlPoint(0.2f, 60),
            new ControlPoint(0.05f, 63),
            new ControlPoint(0.0f, 80),
            new ControlPoint(0.9f, 82),
            new ControlPoint(1.0f, 256)
        };


    }
// Use this for initialization
	void Start ()
	{






	    geoScript = GameObject.FindGameObjectWithTag("ProxyCube").GetComponent<Geometry>();


//		for (int i = 1; i < mAlphaKnots.Count-1; i++) {
//			var point = Instantiate (pointPrefab);
//
//			Vector2 position = new Vector2();
//			position.x = 1.0f * mAlphaKnots [i].mIsoValue / 256.0f * myRect.sizeDelta.x;
//			position.y = 1.0f * mAlphaKnots [i].mColor.w * myRect.sizeDelta.y;
//			point.GetComponent<RectTransform> ().anchoredPosition = position;
//			points.Add (point);
//		}

		computeTransferFunction ();

	}

    public void OnPointerClick(PointerEventData eventData)
    {

        addPoint(eventData);

    }

	//TAG: TF BEGIN
	public void computeTransferFunction(){


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


		linePoints.Clear ();

		for (int i = 0; i < 256; i++) {
			Vector4 color = transferFunc [i];
			data [i].r = Mathf.Clamp (color.x, 0, 1.0f);
			data [i].g = Mathf.Clamp(color.y, 0, 1.0f);
			data [i].b = Mathf.Clamp(color.z, 0, 1.0f);
			data [i].a = Mathf.Clamp(color.w, 0, 1.0f);
			if (assoc) {
				data [i].r *= data [i].a;
				data [i].g *= data [i].a;
				data [i].b *= data [i].a;
			}
			linePoints.Add (new Vector2 ((float)i / 256.0f * myRect.sizeDelta.x, data [i].a * myRect.sizeDelta.y));
//			Debug.Log ("data at " + i + " is " + data [i]);
		}


	    geoScript.updateTransferBufer(data);


	}
		
    void Update () {

    }



    void LateUpdate()
    {

        if (pointsListDirty)
        {
            pointsListDirty = false;
            points.Sort((a,b)=>a.GetComponent<RectTransform>().anchoredPosition.x.CompareTo(b.GetComponent<RectTransform>().anchoredPosition.x));
			mAlphaKnots.Clear ();
			mAlphaKnots.Add (new ControlPoint (0.0f, 0));
			for (int i = 0; i < points.Count; i++) {
				Vector2 position = points [i].GetComponent<RectTransform> ().anchoredPosition;
				int x = (int)(position.x / myRect.sizeDelta.x * 256.0f);
				if (x == 0 || x == 256)
					continue;
				mAlphaKnots.Add (new ControlPoint (1.0f * position.y / myRect.sizeDelta.y, x));

//				Debug.Log (position.y / myRect.sizeDelta.y);
//				Debug.Log (x);
			}
			mAlphaKnots.Add (new ControlPoint (1.0f, 256));
			computeTransferFunction ();
        }

    }




    void addPoint(PointerEventData eventData)
    {
        var point = Instantiate(pointPrefab);
        var pointRect = point.GetComponent<RectTransform>();

        Vector2 position = Vector2.zero;


        pointRect.parent = myRect;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(myRect,eventData.position,null,out position);

        //clamp spawn position inside the parent
//        position.x = Mathf.Clamp(position.x,pointRect.sizeDelta.x*pointRect.pivot.x,myRect.sizeDelta.x-pointRect.sizeDelta.x*(1-pointRect.pivot.x));
//        position.y = Mathf.Clamp(position.y,pointRect.sizeDelta.y*pointRect.pivot.y,myRect.sizeDelta.y-pointRect.sizeDelta.y*(1-pointRect.pivot.y));
//
		position.x = Mathf.Clamp(position.x, 0, myRect.sizeDelta.x);
		position.y = Mathf.Clamp (position.y, 0, myRect.sizeDelta.y);

        pointRect.anchoredPosition = position;

        points.Add(point);
        pointsListDirty = true;
    }
}
