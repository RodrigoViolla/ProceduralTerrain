using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IsleTerrain : MonoBehaviour
{

    public GameObject tree;
    public GameObject viewer;
    public Material material;
    [Range(0, 6)]
    public int levelOfDetail;

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
        
        mapGenerator.RequestMeshData(mapData, levelOfDetail, OnMeshDataReceived);        
    }

    void OnMeshDataReceived(MeshData meshData)
    {
        meshFilter.mesh = meshData.CreateMesh();
        meshObject.AddComponent<MeshCollider>();        
        PlaceObjects(meshData);
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
                        Instantiate(mapObjectData.prefab, position, Quaternion.identity);
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