using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InfinityTerrain : MonoBehaviour
{
    public const float maxViewDistance = 300;
    public Transform viewer;
    public static Vector2 viewerPosition;
    public Material mapMaterial;
    static MapGenerator mapGenerator;

    int chunkSize;
    int chuksVisible;

    Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();

    void Start()
    {
        chunkSize = MapGenerator.mapChunkSize - 1;
        chuksVisible = Mathf.RoundToInt(maxViewDistance / chunkSize);
        mapGenerator = FindObjectOfType<MapGenerator>();
    }

    private void Update()
    {
        viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
        UpdateVisibleChunks();
    }

    void UpdateVisibleChunks()
    {
        int currentChunkCoodX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
        int currentChunkCoodY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

        for(int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
        {
            terrainChunksVisibleLastUpdate[i].SetVisible(false);
        }

        terrainChunksVisibleLastUpdate.Clear();

        for (int yOffset = -chuksVisible; yOffset <= chuksVisible; yOffset++)
        {
            for (int xOffset = -chuksVisible; xOffset <= chuksVisible; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoodX + xOffset, currentChunkCoodY + yOffset);

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].Update();
                    if (terrainChunkDictionary[viewedChunkCoord].IsVisible())
                    {
                        terrainChunksVisibleLastUpdate.Add(terrainChunkDictionary[viewedChunkCoord]);
                    }
                }
                else
                {
                    terrainChunkDictionary.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, chunkSize, transform, mapMaterial));
                }
            }
        }
    }

    public class TerrainChunk
    {
        GameObject meshObject;
        Vector2 position;
        Bounds bounds;
        MeshRenderer meshRenderer;
        MeshFilter meshFilter;

        public TerrainChunk(Vector2 coordenate, int size, Transform parent, Material material)
        {
            position = coordenate * size;
            bounds = new Bounds(position, Vector2.one * size);
            Vector3 positionV3 = new Vector3(position.x, 0, position.y);

            meshObject = new GameObject("Terrain Chunk");
            meshRenderer = meshObject.AddComponent<MeshRenderer>();
            meshFilter = meshObject.AddComponent<MeshFilter>();
            meshRenderer.material = material;

            meshObject.transform.position = positionV3;
            meshObject.transform.parent = parent;
            SetVisible(false);

            mapGenerator.RequestMapData(OnMapDataReceived);
        }

        void OnMeshDataReceived(MeshData meshData)
        {
            meshFilter.mesh = meshData.CreateMesh();
        }

        void OnMapDataReceived(MapData mapData)
        {
            mapGenerator.RequestMeshData(mapData, OnMeshDataReceived);
        }

        public void Update()
        {
            float viewerDistanceFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
            bool visible = viewerDistanceFromNearestEdge <= maxViewDistance;
            SetVisible(visible);
        }

        public void SetVisible(bool visible)
        {
            meshObject.SetActive(visible);
        }

        public bool IsVisible()
        {
            return meshObject.activeSelf;
        }
    }
}
