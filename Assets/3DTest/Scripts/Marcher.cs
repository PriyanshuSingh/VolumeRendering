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
    public float scaleY = 0.1f;
    private int holdWidth;
    private int holdHeight;



    // Use this for initialization
    private void Start()
    {
        _origCam = GetComponent<Camera>();



        var cube = GameObject.FindGameObjectWithTag("ProxyCube");

        Assert.IsTrue(cube != null);
        volumeMaterial = cube.GetComponent<MeshRenderer>().material;


    }


	private void Update(){


	    //TODO reset camera back to volume

	    if (Input.GetKeyDown(KeyCode.R))
	    {


	    }
	    float scrollValue = Input.GetAxis("Mouse ScrollWheel");



	    float xAxisValue = Input.GetAxis("Horizontal")*scaleX;
	    float yAxisValue = Input.GetAxis("Vertical")*scaleY;
        _origCam.transform.Translate(new Vector3(xAxisValue,yAxisValue ,scrollValue));



	}


    private void OnPreRender(){

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
        Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGBFloat));

        //render depths
        var backDepth =  RenderTexture.GetTemporary(holdWidth, holdHeight, 0, RenderTextureFormat.ARGBFloat);

        backDepth.filterMode = FilterMode.Bilinear;
        backDepth.wrapMode = TextureWrapMode.Clamp;








        //Only Back Pass Render
        //render with replaced shaders
        _cam2.targetTexture = backDepth;
        _cam2.RenderWithShader(renderBackDepthShader, "RenderType");



        //assign uniform values in the Volume material
        volumeMaterial.SetTexture("_BackTex",backDepth);

        RenderTexture.ReleaseTemporary(backDepth);



    }
    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {

        holdWidth = source.width;
        holdHeight = source.height;

    }

}
