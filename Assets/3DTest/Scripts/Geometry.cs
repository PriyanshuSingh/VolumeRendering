using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using System.Linq;


public class Geometry : MonoBehaviour {




    [SerializeField]
    private Texture2D[] slices;


    [Header("Volume texture size. These must be a power of 2")]
    [SerializeField] private int volumeWidth =256;
    [SerializeField] private int volumeHeight = 256;
    [SerializeField] private int volumeDepth = 256;



    private MeshRenderer myRenderer;
    private Texture3D _volumeBuffer;
//    private Color[] volumeNormalColors;
    private Transform dirLightTransform;
    private Material _rayMat;
    public Material _makeMat;

    public Texture2D _transferBuffer ;
    public UnityEngine.UI.Slider slider;
    public float maxIterations = 512.0f;


    private void initTransferBuffer(){
        if (_transferBuffer == null)
        {

            _transferBuffer = new Texture2D (256, 1, TextureFormat.ARGB32, false);
            _transferBuffer.filterMode = FilterMode.Bilinear;
            _transferBuffer.wrapMode = TextureWrapMode.Clamp;

        }
    }


    void Awake()
    {

        myRenderer = GetComponent<MeshRenderer>();
        _rayMat = myRenderer.material;

        initTransferBuffer();
    }
// Use this for initialization
    void Start ()
    {




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









	    slider.onValueChanged.AddListener((float arg0)  => {_rayMat.SetFloat("Iterations", arg0 * maxIterations);});
//	    _rayMat.SetFloat("Iterations",256);



        //TODO merge volume and normal texture
        //volume is greyscale thus can be fitted in alpha component
        GenerateVolumeAndNormal(1);




    }





    // Update is called once per frame
    void Update () {






        //TODO currently Idenity if stays this remove this multiplication
//	    Matrix4x4 lightInverter = transform.worldToLocalMatrix;
        _rayMat.SetVector("L",Vector3.Normalize(-dirLightTransform.forward));
//
//		myRenderer.material.SetTexture ("_transferF", _transferBuffer);
//







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



        //TODO last arguement mipmapping refer graphics runner
        // sort
        //mrbrain
//        System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

        //other volumes
        System.Array.Sort(slices, (x, y) => int.Parse(x.name).CompareTo(int.Parse(y.name)));

        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);


        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



//        skip some slices if we can't fit it all in

        var countOffset = (slices.Length - 1) / (float) d;
		countOffset = 1;
//        volumeNormalColors = new Color[w * h * d];



        List<Color> colors = new List<Color>(w*h*d);


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

                    if (x < 2 || y < 2 || x > w - 3 || y > h - 3)
                    {
                        colors.Add(new Color(0, 0, 0, 0));
                    }

                    else
                    {
                        colors.Add(new Color(0,0,0,slices[sliceCount].GetPixelBilinear(x / (float) w, y / (float) h).r));
                        //store the greyscale value in alpha of 3D Texture
                    }

                }
            }
        }

        _volumeBuffer.SetPixels(colors.ToArray());
        _volumeBuffer.Apply();
        _volumeBuffer.filterMode = FilterMode.Bilinear;
        _volumeBuffer.wrapMode = TextureWrapMode.Clamp;



        //setup make mat
        _makeMat.SetTexture("_Volume",_volumeBuffer);
        //TODO setup this value in shader
//        _makeMat.SetFloat("NormalDelta",);



        //setup render texture
        var rt = RenderTexture.GetTemporary(volumeWidth, volumeHeight,16,RenderTextureFormat.ARGB32);
        var saveRt = RenderTexture.active;
        RenderTexture.active = rt;

        //setup cam
        var tempGo = new GameObject("tempCam");
        var tempCam = tempGo.AddComponent<Camera>();
        tempCam.enabled = false;
        tempCam.targetTexture = rt;
        tempCam.cullingMask = LayerMask.GetMask("Nothing");




        colors.Clear();

        Texture2D quadImage = new Texture2D(rt.width, rt.height, TextureFormat.ARGB32, false);



        for (int i = 0; i < volumeDepth; ++i)
        {
            _makeMat.SetFloat("Depth",1.0f*i/(volumeDepth-1));
            tempCam.Render();

            //draw quad
            GL.PushMatrix();
            GL.LoadOrtho();

            _makeMat.SetPass(0);


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

            quadImage.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
            //TODO see if this apply required
            quadImage.Apply();

            colors.AddRange(quadImage.GetPixels());


//for testing normals we can set output to image
//            byte[] bytes = quadImage.EncodeToPNG();
//            System.IO.File.WriteAllBytes(Application.persistentDataPath + "/camera_img-"+i+".png", bytes);





        }



        RenderTexture.active = saveRt;
        RenderTexture.ReleaseTemporary(rt);




        _volumeBuffer.SetPixels(colors.ToArray());
        _volumeBuffer.Apply();
        _volumeBuffer.filterMode = FilterMode.Bilinear;
        _volumeBuffer.wrapMode = TextureWrapMode.Clamp;
        _rayMat.SetTexture("_Volume", _volumeBuffer);



        //set to null and pray to GC
        slices = null;

    }

//    private float sampleVolume(int x, int y, int z)
//    {
//        x = Mathf.Clamp(x, 0, _volumeBuffer.width - 1);
//        y = Mathf.Clamp(y, 0, _volumeBuffer.height - 1);
//        z = Mathf.Clamp(z, 0, _volumeBuffer.depth - 1);
//        //accessing r can access any one here for greyscale value
//        return volumeNormalColors[x + y * _volumeBuffer.height + z *_volumeBuffer.width * _volumeBuffer.height].a;
//    }
//
//    private void filterNxNxN(int n)
//    {
//        int index = 0;
//        for (int z = 0; z < _volumeBuffer.depth; z++)
//        {
//            for (int y = 0; y < _volumeBuffer.height; y++)
//            {
//                for (int x = 0; x < _volumeBuffer.width; x++,index++)
//                {
//                    var sampleResult = sampleNxNxN(x, y, z, n);
//                    volumeNormalColors[index].r = sampleResult.x;
//                    volumeNormalColors[index].g = sampleResult.y;
//                    volumeNormalColors[index].b = sampleResult.z;
//
//                }
//            }
//        }
//    }
//    private bool isInBounds(int x, int y, int z)
//    {
//        return  x >= 0 && x < _volumeBuffer.width && y >= 0 && y < _volumeBuffer.height &&  z >= 0 && z < _volumeBuffer.depth;
//    }
//
//    private Vector3 sampleNxNxN(int x, int y, int z, int n)
//    {
//        n = (n - 1) / 2;
//
//        Vector3 average = Vector3.zero;
//        int num = 0;
//
//        for (int k = z - n; k <= z + n; k++)
//        {
//            for (int j = y - n; j <= y + n; j++)
//            {
//                for (int i = x - n; i <= x + n; i++)
//                {
//                    if (isInBounds(i, j, k))
//                    {
//                        average += sampleGradients(i, j, k);
//                        num++;
//                    }
//                }
//            }
//        }
//
//        average /= (float)num;
//        if (average.x != 0.0f && average.y != 0.0f && average.z != 0.0f)
//            average.Normalize();
//
//        return average;
//    }
//
//    private Vector3 sampleGradients(int x, int y, int z)
//    {
//        var v = volumeNormalColors[x + y * _volumeBuffer.width + z * _volumeBuffer.height * _volumeBuffer.width];
//        return new Vector3(v.r,v.g,v.b);
//    }




    public void updateTransferBufer(Color [] colors)
    {
        initTransferBuffer();
        _transferBuffer.SetPixels(colors);
        _transferBuffer.Apply();
        _rayMat.SetTexture ("_transferF", _transferBuffer);
    }

}
