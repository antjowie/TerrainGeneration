using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EndlessTerrain : MonoBehaviour
{
    const float scale = 5f;
    
    public LODInfo[] lodInfos;

    public Transform viewer;
    public static Vector2 viewerPosition;
    public static MapGenerator mapGenerator;
    public Material material;

    const int chunkSize = MapGenerator.mapChunkSize - 1;
    static int chunkViewDst;
    int chunkViewCoordDst;

    Dictionary<Vector2, TerrainChunk> chunks = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> chunksInPreviousFrame = new List<TerrainChunk>();

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
    
        chunkViewDst = lodInfos[lodInfos.Length - 1].distance;    
        chunkViewCoordDst = Mathf.RoundToInt(chunkViewDst / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z) / scale;

        UpdateChunksInRange();
    }

    void UpdateChunksInRange()
    {
        for(int i = 0; i < chunksInPreviousFrame.Count; i++)
        {
            chunksInPreviousFrame[i].SetVisible(false);    
        }
        chunksInPreviousFrame.Clear();
        
        Vector2 currentChunkCoord = new Vector2();
        currentChunkCoord.x = Mathf.RoundToInt((float)viewerPosition.x / (float)chunkSize);
        currentChunkCoord.y = Mathf.RoundToInt((float)viewerPosition.y / (float)chunkSize);

        for (int x = -chunkViewCoordDst; x <= chunkViewCoordDst; x++)
            for (int y = -chunkViewCoordDst; y <= chunkViewCoordDst; y++)
            {
                Vector2 chunkCoord = currentChunkCoord + new Vector2(x, y);
                Vector2 chunkPos = chunkCoord * chunkSize;

                if (chunks.ContainsKey(chunkCoord))
                {
                    chunks[chunkCoord].UpdateChunkInRange();
                    chunksInPreviousFrame.Add(chunks[chunkCoord]);
                }
                else
                {
                    chunks.Add(chunkCoord, new TerrainChunk(chunkCoord, chunkSize, transform, material, lodInfos));
                }
            }
    }

    public class TerrainChunk
    {
        Vector2 position;

        GameObject mesh;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;
        
        MapData mapData;
        bool hasMapData;
        
        LODMesh[] lodMeshes;
        LODInfo[] lodInfos;
        int lodIndex = -1;
        
        Bounds bounds;

        public TerrainChunk(Vector2 coord, int size, Transform transform, Material material, LODInfo[] lodInfos)
        {
            this.lodInfos = lodInfos;
            lodMeshes = new LODMesh[lodInfos.Length];
            for(int i = 0; i < lodInfos.Length; i++)
            {
                lodMeshes[i] = new LODMesh(lodInfos[i].lod);
            }
            
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            mesh = new GameObject("Terrain Chunk");
            meshRenderer = mesh.AddComponent<MeshRenderer>();
            meshFilter = mesh.AddComponent<MeshFilter>();
            
            meshRenderer.material = material;
            mesh.transform.position = new Vector3(position.x,0,position.y) * scale;
            mesh.transform.parent = transform;
            mesh.transform.localScale = Vector3.one * scale;
            
            SetVisible(false);
            
            EndlessTerrain.mapGenerator.RequestMapData(OnMapDataRequest,position);
        }

        void OnMapDataRequest(MapData mapData)
        {
            this.mapData = mapData;
            hasMapData = true;
            // EndlessTerrain.mapGenerator.RequestMeshData(OnMeshDataRequest,mapData.heightMap,);
            Texture2D texture = TextureGenerator.CreateTextureFromColorMap(mapData.colorMap,chunkSize + 1,chunkSize + 1);
            meshRenderer.material.SetTexture("_BaseMap", texture);
        }
        
        // void OnMeshDataRequest(MeshData meshData)
        // {
        //     meshFilter.mesh = meshData.CreateMesh();
        // }
        
        public void UpdateChunkInRange()
        {
            if(!hasMapData) return;
            
            float sqrToChunkDst = bounds.SqrDistance(viewerPosition);
            float toChunkDst = Mathf.Sqrt(sqrToChunkDst);
            bool visible = toChunkDst <= chunkViewDst;
            SetVisible(visible);
            
            if(visible)
            {
                for(int i = 0; i < lodInfos.Length; i++)
                {
                    if(toChunkDst <= lodInfos[i].distance)
                    {
                        if(lodIndex != i && lodMeshes[i].hasMesh)
                        {
                            meshFilter.mesh = lodMeshes[i].mesh;
                            lodIndex = i;
                        }
                        else if (!lodMeshes[i].hasRequestedMesh)
                        {
                            mapGenerator.RequestMeshData(lodMeshes[i].OnMeshDataRequest,mapData.heightMap,lodInfos[i].lod);
                            lodMeshes[i].hasRequestedMesh = true;
                        }
                        break;
                    }
                }
            }
        }

        public void SetVisible(bool visible)
        {
            mesh.SetActive(visible);
        }

        public bool GetVisible()
        {
            return mesh.activeSelf;
        }
    }
    
    class LODMesh
    {
        public Mesh mesh;
        public bool hasRequestedMesh;
        public bool hasMesh;
        public int lod;
        
        public LODMesh(int lod)
        {
            this.lod = lod;
        }
        
        public void OnMeshDataRequest(MeshData meshData)
        {
            mesh = meshData.CreateMesh();
            hasMesh = true;
        }
    }
}

[System.Serializable]
public class LODInfo
{
    public int lod;
    public int distance;

    public LODInfo(int lod, int distance)
    {
        this.lod = lod;
        this.distance = distance;
    }
}