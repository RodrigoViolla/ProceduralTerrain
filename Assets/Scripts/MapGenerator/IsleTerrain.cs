using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsleTerrain : MonoBehaviour
{
    public GameObject viewer;
    public Material material;
    [Range(0, 6)]
    public int levelOfDetail;
    public Material waterMaterial;

    public float waterLevel;

    public MapObjectData[] objects;
    const float scale = 5f;
    const float viewerMoveThresholdForChunkUpdate = 25f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;
    static MapGenerator mapGenerator;
    private GameObject meshObject;    
    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private int chunkSize;

    void Start()
    {
        mapGenerator = FindObjectOfType<MapGenerator>();
        mapGenerator.seed = Random.Range(0, 1000);
        chunkSize = MapGenerator.mapChunkSize - 1;

        CreateIsle();
    }

    void CreateIsle(){
        meshObject = new GameObject("Isle");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshRenderer.material = material;

        mapGenerator.RequestMapData(viewer.transform.position, OnMapDataReceived);
    }

    void OnMapDataReceived(MapData mapData){
        Texture2D texture = TextureGenerator.TextureFromColorMap(mapData.colourMap, MapGenerator.mapChunkSize, MapGenerator.mapChunkSize);
        meshRenderer.material.mainTexture = texture;

        GameObject waterObj = new GameObject("water");
        MeshRenderer waterRenderer = waterObj.AddComponent<MeshRenderer>();
        MeshFilter waterMeshFilter = waterObj.AddComponent<MeshFilter>();
        waterRenderer.material = waterMaterial;
        waterMeshFilter.mesh = GenerateWaterMesh(mapData.heightMap, levelOfDetail).CreateMesh();

        meshFilter.mesh = GenerateWaterMesh(mapData.heightMap, levelOfDetail).CreateMesh();
        mapGenerator.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);        
        waterObj.transform.position = new Vector3(waterObj.transform.position.x, waterLevel, waterObj.transform.position.z);
    }

    void OnMeshDataReceived(MeshData meshData)
    {
        meshFilter.mesh = meshData.CreateMesh();       

        meshObject.AddComponent<MeshCollider>();        
        PlaceObjects(meshData);
    }

    public static MeshData GenerateWaterMesh(float[,] heightMap, int levelOfDetail)
    {        
        int width = heightMap.GetLength(0);
        int height = heightMap.GetLength(1);
        float topLeftX = (width - 1f) / -2f;
        float topLeftZ = (height - 1f) / 2f;
        
        MeshData meshData = new MeshData(width, height);
        int meshSimplificationIncrement = levelOfDetail == 0 ? 1 : levelOfDetail * 2;
        int verticesPerLine = (width - 1) / meshSimplificationIncrement + 1;

        int vertexIndex = 0;

        for (int y = 0; y < height; y += meshSimplificationIncrement)
        {
            for (int x = 0; x < width; x += meshSimplificationIncrement)
            {
                meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, 0, topLeftZ - y);
                meshData.uvs[vertexIndex] = new Vector2(x / (float)width, y / (float)height);

                if(x < width-1 && y < height - 1)
                {
                    meshData.AddTriangle(vertexIndex, vertexIndex + verticesPerLine + 1, vertexIndex + verticesPerLine);
                    meshData.AddTriangle(vertexIndex + verticesPerLine + 1, vertexIndex, vertexIndex + 1);
                }

                vertexIndex++;
            }
        }

        return meshData;
    }

    void PlaceObjects(MeshData meshData)
    {
        foreach (Vector3 position in meshData.vertices)
        {
            foreach (MapObjectData mapObjectData in objects)
            {
                if(Random.Range(1, 100) < mapObjectData.spawnProbability)
                {
                    if(position.y > mapObjectData.spawnHeightRange.x && position.y <= mapObjectData.spawnHeightRange.y)
                    {             
                        float randomRange = 1;    
                        RaycastHit hit; 
                        
                        Vector3 randomPosition = new Vector3(position.x + Random.Range(-randomRange, randomRange), position.y, position.z + Random.Range(randomRange, randomRange));
                        Physics.Raycast(randomPosition, Vector3.down, out hit);

                        randomPosition = hit.point;

                        Instantiate(mapObjectData.prefab, randomPosition, Quaternion.identity);
                    }
                }
            }            
        }
    }
    
}

[System.Serializable]
public struct MapObjectData{
    [Range(0, 100)]
    public int spawnProbability;
    public Vector2 spawnHeightRange;
    public GameObject prefab;

}