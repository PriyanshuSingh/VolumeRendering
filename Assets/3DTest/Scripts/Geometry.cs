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
    private Transform dirLightTransform;
    private Material _rayMat;
    public Material _makeMat;

    public Texture2D _transferBuffer ;
    public UnityEngine.UI.Slider iterationSlider;
    public float maxIterations = 512.0f;





    public void SetupUiStuff()
    {
        iterationSlider.onValueChanged.AddListener((float arg0)  => {_rayMat.SetFloat("iterations", arg0 * maxIterations);});
        iterationSlider.normalizedValue = 0.5f;

    }
    public void setupDefaultUniforms()
    {
        _rayMat.SetFloat("alphaThreshold",95f);
        _rayMat.SetFloat("iterations",iterationSlider.normalizedValue*maxIterations);
        _rayMat.SetFloat("baseIterations",512);
        _rayMat.SetInt("compositingType",0);





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









        //TODO merge volume and normal texture
        //volume is greyscale thus can be fitted in alpha component
        SetupUiStuff();
        GenerateVolumeAndNormal();
        setupDefaultUniforms();





    }





    // Update is called once per frame
    void Update () {






        _rayMat.SetVector("L",Vector3.Normalize(-dirLightTransform.forward));








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


    private void GenerateVolumeAndNormal()
    {



        //TODO last arguement mipmapping refer graphics runner
        // sort
        //mrbrain
//        System.Array.Sort(slices, (x, y) => x.name.CompareTo(y.name));

        //TODO parse and obtain ints only
        //other volumes
        System.Array.Sort(slices, (x, y) => int.Parse(x.name).CompareTo(int.Parse(y.name)));

        _volumeBuffer = new Texture3D(volumeWidth, volumeHeight, volumeDepth, TextureFormat.ARGB32, false);


        var w = _volumeBuffer.width;
        var h = _volumeBuffer.height;
        var d = _volumeBuffer.depth;



//        skip some slices if we can't fit it all in

        var countOffset = (slices.Length - 1) / (float) d;
		countOffset = 1;



        List<Color> colors = new List<Color>(w*h*d);


        int sliceCount;
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





    private void initTransferBuffer(){
        if (_transferBuffer == null)
        {

            _transferBuffer = new Texture2D (256, 1, TextureFormat.ARGB32, false);
            _transferBuffer.filterMode = FilterMode.Bilinear;
            _transferBuffer.wrapMode = TextureWrapMode.Clamp;

        }
    }
    public void updateTransferBufer(Color [] colors)
    {
        initTransferBuffer();
        _transferBuffer.SetPixels(colors);
        _transferBuffer.Apply();
        _rayMat.SetTexture ("_transferF", _transferBuffer);
    }



}
