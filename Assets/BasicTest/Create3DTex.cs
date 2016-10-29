using UnityEngine;
using System.Collections;


public class Create3DTex : MonoBehaviour
{

    public Texture3D tex;
    public int size = 16;

    void Start()
    {


        tex = new Texture3D(size, size, size, TextureFormat.ARGB32, true);
        var cols = new Color[size * size * size];
        float mul = 1.0f / (size - 1);
        int idx = 0;
        Color c = Color.white;
        for (int z = 0; z < size; ++z)
        {
            for (int y = 0; y < size; ++y)
            {
                for (int x = 0; x < size; ++x, ++idx)
                {


//                    c.r = ((x&1) != 0) ? x * mul : 1 - x * mul;
//                    c.g = ((y&1) != 0) ? y * mul : 1 - y * mul;
//                    c.b = ((z&1) != 0) ? z * mul : 1 - z * mul;




                    cols[idx] = c;



                    if (x > size / 2.0 && y > size / 2.0 && z > size / 2.0)
                    {
                        cols[idx] = Color.black;

                    }
                }
            }
        }



        tex.SetPixels(cols);
        tex.Apply();
        var myRender = GetComponent<Renderer>();
        myRender.material.SetTexture("_Volume", tex);


//        var verts = GetComponent<MeshFilter>().mesh.vertices;
//        Debug.Log("len is "+ verts.Length);
//        foreach (Vector3 vert in verts)
//        {
//            Debug.Log("vert position" + vert);
//        }

        StartCoroutine(Stuff());
    }

    void Update()
    {
        var trans = GetComponent<Transform>();

        trans.Rotate(Vector3.up,30*Time.deltaTime);













    }


    IEnumerator Stuff()
    {

        yield return new WaitForSeconds(2.0f);
//        var verts = GetComponent<MeshFilter>().mesh.vertices;
//        Debug.Log("len is " + verts.Length);
//        foreach (Vector3 vert in verts)
//        {
//            Debug.Log("vert position" + vert);
//        }



//        Vector3[] vertices = GetComponent<MeshFilter>().mesh.vertices;
//        Debug.Log("len is " + verts.Length);
//        for (int i = 0; i < vertices.Length; ++i)
//        {
//            var vec = new Vector3(vertices[i].x + 0.5f, vertices[i].y + 0.5f, vertices[i].z + 0.5f);
//            Debug.Log(vec);

//        }
//        GetComponent<MeshFilter>().mesh.vertices = vertices;




    }


}
