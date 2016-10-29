using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Camera))]

public class CamScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {



	
	}


    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {






////        Debug.Log("Hello Render Image");
////
//        if (_cam2 == null)
//        {
//            var go = new GameObject("Cam2");
//            _cam2= go.AddComponent<Camera>();
//            _cam2.enabled = false;
//
//
//        }
//        _cam2.CopyFrom(_origCam);
//        //clear background
//        _cam2.clearFlags = CameraClearFlags.SolidColor;
//        _cam2.backgroundColor = Color.white;
//        _cam2.cullingMask = volumeLayer;
//
//
//        //check  support for float format
//        Assert.IsTrue(SystemInfo.SupportsRenderTextureFormat(RenderTextureFormat.ARGB32));
//
//        //render depths
//        var backDepth =  RenderTexture.GetTemporary(source.width, source.height, 0, RenderTextureFormat.ARGBFloat);
//
//        backDepth.filterMode = FilterMode.Bilinear;
//        backDepth.wrapMode = TextureWrapMode.Clamp;
//
//
//
//
//
//
//
//
////Only Back Pass Render
//        //TODO this causes 1 frame lag between the uniforms being updated and used by the volume
//        //render with replaced shaders
//        _cam2.targetTexture = backDepth;
//        _cam2.RenderWithShader(renderBackDepthShader, "RenderType");
//
//
//
//        //assign uniform values in the Volume material
//        volumeMaterial.SetTexture("_BackTex",backDepth);
//        RenderTexture.ReleaseTemporary(backDepth);





    }
}
