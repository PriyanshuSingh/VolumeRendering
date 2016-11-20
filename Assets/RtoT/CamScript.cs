using System;
using UnityEngine;
using System.Collections;
using UnityEngine.Rendering;

[RequireComponent(typeof(Camera))]

public class CamScript : MonoBehaviour
{


    private Camera _cam2;
    private Camera _origCam;
    private RenderTexture rt;
    public Shader replacementShader;
    public Material _Mat;
    [SerializeField]
    private LayerMask emptyLayer;


    [SerializeField] private Texture2D[] slices;
    private Texture3D _volumeBuffer;

    public int volumeWidth = 512;
    public int volumeHeight = 512;
    public int volumeDepth = 512;



    private int slicePos= 0;



    void Awake()
    {




        rt = new RenderTexture(volumeWidth,volumeHeight,16,RenderTextureFormat.ARGB32);
        rt.Create();



        _origCam = GetComponent<Camera>();

        System.Array.Sort(slices, (x, y) => int.Parse(x.name).CompareTo(int.Parse(y.name)));
        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);


        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



//        skip some slices if we can't fit it all in
        var countOffset = (slices.Length - 1) / (float) d;

        var volumeNormalColors = new Color[w * h * d];

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
                        volumeNormalColors[idx].a = slices[sliceCount].GetPixelBilinear(x / (float) w, y / (float) h).r;
                    //store the greyscale value in alpha of 3D Texture


                }
            }
        }

        _volumeBuffer.SetPixels(volumeNormalColors);
        _volumeBuffer.Apply();
        _volumeBuffer.filterMode = FilterMode.Bilinear;

        //TODO this eliminates some of problem but currently no way to have border sampling in Unity3D

        //porbably just add a border of black voxels around

        _volumeBuffer.wrapMode = TextureWrapMode.Clamp;
        _Mat.SetTexture("_Volume", _volumeBuffer);

        _Mat.SetFloat("Depth",0);



        //set to null and pray to GC
        slices = null;



    }

    void Destroy()
    {

        _cam2.targetTexture = null;
        rt.Release();

        if (_volumeBuffer != null)
        {
            Destroy(_volumeBuffer);
        }



    }
	// Use this for initialization
	void Start () {






	}
	
	// Update is called once per frame
	void Update () {


	    if (Input.GetKeyDown(KeyCode.C))
	    {
	        slicePos = volumeDepth;
//	        slicePos = (slicePos+50)%volumeDepth;
	        _Mat.SetFloat("Depth",1.0f*slicePos/volumeDepth);

	    }



	}


    void LateUpdate()
    {


        if (_cam2 == null)
        {
            var go = new GameObject("Cam2");
            _cam2= go.AddComponent<Camera>();
            _cam2.enabled = false;
            _cam2.targetTexture = rt;
            _cam2.cullingMask = emptyLayer;

        }



        var saveRt = RenderTexture.active;
        RenderTexture.active = rt;




        if (Input.GetKeyDown("s"))
        {





            // create a new Texture2D with the camera's texture, using its height and width
            Texture2D cameraImage = new Texture2D(rt.width, rt.height, TextureFormat.RGB24, false);

            for (int i = 0; i <= volumeDepth; ++i)
            {
                _Mat.SetFloat("Depth",1.0f*i/volumeDepth);
                _cam2.Render();

                //draw quad
                GL.PushMatrix();
                GL.LoadOrtho();

                _Mat.SetPass(0);


                GL.Begin(GL.QUADS);

                GL.MultiTexCoord2(0, 0.0f, 0.0f);
                GL.Vertex3(0.0f, 0.0f, 0.0f);

                GL.MultiTexCoord2(0, 1.0f, 0.0f);
                GL.Vertex3(1.0f, 0.0f, 0.0f);

                GL.MultiTexCoord2(0, 1.0f, 1.0f);
                GL.Vertex3(1.0f, 1.0f, 0.0f);

                GL.MultiTexCoord2(0, 0.0f, 1.0f);
                GL.Vertex3(0.0f, 1.0f, 0.0f);

                GL.End();
                GL.PopMatrix();

                cameraImage.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
                cameraImage.Apply();
                for (int y = 0; y < volumeHeight; y++)
                {
                    for (int x = 0; x < volumeWidth; x++)
                    {
                        var idx = x + y * volumeWidth + i * volumeWidth * volumeHeight;

                    }
                }




//                byte[] bytes = cameraImage.EncodeToPNG();
//                System.IO.File.WriteAllBytes(Application.persistentDataPath + "/camera_img-"+i+".png", bytes);


            }


        }


        RenderTexture.active = saveRt;
    }

    public IEnumerator SaveCameraView()
    {
        yield return new WaitForEndOfFrame();




    }




    private void onPreRender()
    {
    }

    private void onPostRender()
    {




    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

















        if (Camera.main == GetComponent<Camera>() && destination == GetComponent<Camera>().targetTexture && RenderTexture.active == destination)
        {
            Debug.Log("this shit");
        }


    }
}
