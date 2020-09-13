using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapDisplay : MonoBehaviour
{
    public Renderer textureRenderer;
    public MeshRenderer meshRenderer;
    public MeshFilter meshFilter;

    public void DisplayMap(Texture2D texture)
    {
        textureRenderer.sharedMaterial.SetTexture("_BaseMap", texture);
        textureRenderer.transform.localScale = new Vector3(texture.width,1,texture.height);
    }
    
    public void DisplayMesh(Mesh mesh, Texture2D texture)
    {
        meshFilter.mesh = mesh;
        meshRenderer.sharedMaterial.SetTexture("_BaseMap", texture);
    }
}
