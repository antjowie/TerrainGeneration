using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EndlessTerrain : MonoBehaviour
{
    public Transform viewer;
    public static Vector2 viewerPosition;

    const int chunkSize = MapGenerator.mapChunkSize - 1;
    const int chunkViewDst = 500;
    int chunkViewCoordDst;

    Dictionary<Vector2, ChunkData> chunks = new Dictionary<Vector2, ChunkData>();
    List<ChunkData> chunksInPreviousFrame = new List<ChunkData>();

    void Start()
    {
        chunkViewCoordDst = Mathf.RoundToInt(chunkViewDst / chunkSize);
    }

    void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);

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
                    chunks.Add(chunkCoord, new ChunkData(chunkCoord, chunkSize, transform));
                }
            }
    }

    public class ChunkData
    {
        Vector2 position;

        GameObject mesh;
        Bounds bounds;

        public ChunkData(Vector2 coord, int size, Transform transform)
        {
            position = coord * size;
            bounds = new Bounds(position, Vector2.one * size);

            mesh = GameObject.CreatePrimitive(PrimitiveType.Plane);
            mesh.transform.position = new Vector3(position.x,0,position.y);
            mesh.transform.localScale = Vector3.one * size / 10f;
            mesh.transform.parent = transform;
            SetVisible(false);
        }

        public void UpdateChunkInRange()
        {
            float toChunkDst = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = toChunkDst <= chunkViewDst;
            SetVisible(visible);
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
}
