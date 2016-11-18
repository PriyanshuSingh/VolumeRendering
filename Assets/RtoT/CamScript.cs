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
    [SerializeField]
    private LayerMask notVolumeLayer;

    void Awake()
    {




        rt = new RenderTexture(256,256,16,RenderTextureFormat.ARGB32);
        rt.dimension = TextureDimension.Tex3D;
        rt.volumeDepth = 256;
        rt.enableRandomWrite = true;

        rt.Create();



        if (rt.isVolume)
        {
            Debug.Log("created 3d tex");
        }

        _origCam = GetComponent<Camera>();



    }

    void Destroy()
    {
        rt.Release();




    }
	// Use this for initialization
	void Start () {





	}
	
	// Update is called once per frame
	void Update () {





	}


    void LateUpdate()
    {

        if (Input.GetKeyDown("s"))
        {
            StartCoroutine(SaveCameraView());
        }

    }

    public IEnumerator SaveCameraView()
    {
        yield return new WaitForEndOfFrame();



        if (_cam2 == null)
        {
            var go = new GameObject("Cam2");
            _cam2= go.AddComponent<Camera>();
            _cam2.enabled = false;


        }
        _cam2.CopyFrom(_origCam);
        //clear background
        _cam2.clearFlags = CameraClearFlags.SolidColor;
        _cam2.backgroundColor = Color.red;
        _cam2.cullingMask = notVolumeLayer;

        RenderTexture rendText= RenderTexture.active;

        _cam2.targetTexture = rt;


        Graphics.SetRenderTarget(_cam2.targetTexture,0,CubemapFace.Unknown,0);
//        RenderTexture.active = rt;


        _cam2.RenderWithShader(replacementShader,"");

        // create a new Texture2D with the camera's texture, using its height and width
        Texture2D cameraImage= new Texture2D( rt.width  ,rt.height, TextureFormat.RGB24, false);
        Texture3D cImage = new Texture3D(128,128,128,TextureFormat.ARGB32, false);

        cameraImage.ReadPixels(new Rect(0, 0, rt.width  ,rt.height), 0, 0);
        cameraImage.Apply();
//        Graphics.SetRenderTarget(null);
        RenderTexture.active = rendText;

        byte[] bytes = cameraImage.EncodeToPNG();
        System.IO.File.WriteAllBytes(Application.persistentDataPath + "/camera_img.png", bytes);

    }




    private void onPreRender()
    {
//        Graphics.SetRenderTarget(rt);
    }

    private void onPostRender()
    {
//        Graphics.SetRenderTarget(null);

    }


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

    }
}
