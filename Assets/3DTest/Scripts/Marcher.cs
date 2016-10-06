using UnityEngine;
using System.Collections;
using UnityEngine.Assertions;

[RequireComponent(typeof(Camera))]
public class Marcher : MonoBehaviour {


    //layer on which the volume object is
    [SerializeField]
    private LayerMask volumeLayer;

    //reference to the material of the volume
//    [SerializeField]
    private Material volumeMaterial;



    private Camera _cam2;
    private Camera _origCam;



    [SerializeField]
    private Shader renderFrontDepthShader;
    [SerializeField]
    private Shader renderBackDepthShader;




    public float scaleX = 0.1f;
    public float scaleZ = 0.1f;




    // Use this for initialization
    private void Start()
    {
        _origCam = GetComponent<Camera>();



        var cube = GameObject.FindGameObjectWithTag("ProxyCube");
//        if (cube != null)
//        {
//            Debug.Log("found cube");
//        }
        Assert.IsTrue(cube != null);
        volumeMaterial = cube.GetComponent<MeshRenderer>().material;

    }

    public float speedH = 2.0f;
    public float speedV = 2.0f;

    private float yaw = 0.0f;
    private float pitch = 0.0f;


    // Update is called once per frame
	private void Update(){
	    float xAxisValue = Input.GetAxis("Horizontal")*scaleX;
	    float zAxisValue = Input.GetAxis("Vertical")*scaleZ;
//	    if (Camera.current != null)
//	    {
	        _origCam.transform.Translate(new Vector3(xAxisValue, 0.0f, zAxisValue));
//	    }





	    if (Input.GetMouseButton(0))
	    {
	        yaw += speedH * Input.GetAxis("Mouse X");
	        pitch -= speedV * Input.GetAxis("Mouse Y");


	        transform.eulerAngles = new Vector3(pitch, yaw, 0.0f);

	    }

	}

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {






//        Debug.Log("Hello Render Image");
//
        if (_cam2 == null)
        {
            var go = new GameObject("Cam2");
            _cam2= go.AddComponent<Camera>();
            _cam2.enabled = false;


        }
        _cam2.CopyFrom(_origCam);
        //clear background
        _cam2.clearFlags = CameraClearFlags.SolidColor;
        _cam2.backgroundColor = Color.white;
        _cam2.cullingMask = volumeLayer;


        //check  support for float format
        Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32));

        //render depths
//        var frontDepth = RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);
        var backDepth =  RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);

        backDepth.filterMode = FilterMode.Bilinear;
        backDepth.wrapMode = TextureWrapMode.Clamp;




//        Texture = <Back>;
//        MinFilter = LINEAR;
//        MagFilter = LINEAR;
//        MipFilter = LINEAR;
//
//        AddressU = Border;				// border sampling in U
//        AddressV = Border;				// border sampling in V
//        BorderColor = float4(0,0,0,0);	// outside of border should be black

        //Two pass render
        //render with replaced shaders
//        _cam2.targetTexture = frontDepth;
//        _cam2.RenderWithShader(renderFrontDepthShader, "RenderType");
//        _cam2.targetTexture = backDepth;
//        _cam2.RenderWithShader(renderBackDepthShader, "RenderType");



//Only Back Pass Render
        //TODO this causes 1 frame lag between the uniforms being updated and used by the volume
        //render with replaced shaders
//        _cam2.targetTexture = frontDepth;
//        _cam2.RenderWithShader(renderFrontDepthShader, "RenderType");
        _cam2.targetTexture = backDepth;
        _cam2.RenderWithShader(renderBackDepthShader, "RenderType");





        //TODO see raymarcher shader parameters



//        var cube = GameObject.FindGameObjectWithTag("ProxyCube");
//        volumeMaterial = cube.GetComponent<MeshRenderer>().material ;




        //assign uniform values in the Volume material
        volumeMaterial.SetTexture("_BackTex",backDepth);








//        RenderTexture.ReleaseTemporary(frontDepth);
        RenderTexture.ReleaseTemporary(backDepth);





    }

}
