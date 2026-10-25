// Module: Race   Owner: Member 2   Purpose: Procedural track generation using Unity Splines

using UnityEngine;
using UnityEngine.Splines;
using Unity.Mathematics;
using System.Collections.Generic;

namespace NeonRush
{
    [RequireComponent(typeof(SplineContainer))]
    public class TrackBuilder : MonoBehaviour
    {
        [Header("Track Settings")]
        public float trackWidth = 18f;
        public float wallHeight = 2.5f;
        public int segmentsPerUnit = 2;
        
        [Header("Materials")]
        public Material roadMaterial;
        public Material wallMaterial;
        public Material glowStripMaterial;
        
        [Header("Track Features")]
        public bool autoPlaceCheckpoints = true;
        public float checkpointInterval = 100f;
        
        public bool autoPlaceBoostPads = true;
        public int boostPadCount = 8;
        
        public bool autoPlaceWeaponPads = true;
        public int weaponPadCount = 12;
        
        public bool autoPlaceEnergyCells = true;
        public int energyCellCount = 15;
        
        [Header("Prefabs")]
        public GameObject checkpointPrefab;
        public GameObject boostPadPrefab;
        public GameObject weaponPadPrefab;
        public GameObject energyCellPrefab;
        
        private SplineContainer splineContainer;
        private GameObject trackMeshObject;
        private List<GameObject> spawnedObjects = new List<GameObject>();
        
        void Awake()
        {
            splineContainer = GetComponent<SplineContainer>();
        }
        
        [ContextMenu("Build Track")]
        public void BuildTrack()
        {
            if (splineContainer == null || splineContainer.Spline == null)
            {
                Debug.LogError("TrackBuilder: No spline found!");
                return;
            }
            
            ClearTrack();
            GenerateTrackMesh();
            PlaceTrackFeatures();
            
            Debug.Log($"TrackBuilder: Track built with {splineContainer.Spline.Count} knots");
        }
        
        void ClearTrack()
        {
            // Remove old track mesh
            if (trackMeshObject != null)
            {
                DestroyImmediate(trackMeshObject);
            }
            
            // Remove spawned objects
            foreach (var obj in spawnedObjects)
            {
                if (obj != null) DestroyImmediate(obj);
            }
            spawnedObjects.Clear();
        }
        
        void GenerateTrackMesh()
        {
            trackMeshObject = new GameObject("TrackMesh");
            trackMeshObject.transform.SetParent(transform);
            trackMeshObject.transform.localPosition = Vector3.zero;
            
            Spline spline = splineContainer.Spline;
            float splineLength = spline.GetLength();
            int totalSegments = Mathf.CeilToInt(splineLength * segmentsPerUnit);
            
            // Create mesh data
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            for (int i = 0; i <= totalSegments; i++)
            {
                float t = (float)i / totalSegments;
                float3 position = spline.EvaluatePosition(t);
                float3 tangent = spline.EvaluateTangent(t);
                float3 up = spline.EvaluateUpVector(t);
                float3 right = math.normalize(math.cross(up, tangent));
                
                // Road surface vertices
                float halfWidth = trackWidth * 0.5f;
                Vector3 leftEdge = (Vector3)(position + right * -halfWidth);
                Vector3 rightEdge = (Vector3)(position + right * halfWidth);
                
                vertices.Add(leftEdge);
                vertices.Add(rightEdge);
                
                uvs.Add(new Vector2(0f, t * splineLength * 0.1f));
                uvs.Add(new Vector2(1f, t * splineLength * 0.1f));
                
                // Create triangles (except for last segment)
                if (i < totalSegments)
                {
                    int baseIndex = i * 2;
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 1);
                    
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 3);
                }
            }
            
            // Create road mesh
            Mesh roadMesh = new Mesh();
            roadMesh.name = "TrackRoadMesh";
            roadMesh.vertices = vertices.ToArray();
            roadMesh.triangles = triangles.ToArray();
            roadMesh.uv = uvs.ToArray();
            roadMesh.RecalculateNormals();
            roadMesh.RecalculateBounds();
            
            GameObject roadObj = new GameObject("Road");
            roadObj.transform.SetParent(trackMeshObject.transform);
            roadObj.transform.localPosition = Vector3.zero;
            
            MeshFilter mf = roadObj.AddComponent<MeshFilter>();
            MeshRenderer mr = roadObj.AddComponent<MeshRenderer>();
            MeshCollider mc = roadObj.AddComponent<MeshCollider>();
            
            mf.mesh = roadMesh;
            mc.sharedMesh = roadMesh;
            mr.material = roadMaterial != null ? roadMaterial : CreateDefaultMaterial(Color.gray);
            
            roadObj.layer = LayerMask.NameToLayer("Default");
            
            Debug.Log($"TrackBuilder: Generated road mesh with {vertices.Count} vertices");
        }
        
        void PlaceTrackFeatures()
        {
            Spline spline = splineContainer.Spline;
            float splineLength = spline.GetLength();
            
            // Place checkpoints
            if (autoPlaceCheckpoints && checkpointPrefab != null)
            {
                int checkpointCount = Mathf.FloorToInt(splineLength / checkpointInterval);
                for (int i = 0; i < checkpointCount; i++)
                {
                    float t = (float)(i + 1) / (checkpointCount + 1);
                    PlaceObjectAtSplinePosition(checkpointPrefab, t, "Checkpoint");
                }
            }
            
            // Place boost pads
            if (autoPlaceBoostPads && boostPadPrefab != null)
            {
                for (int i = 0; i < boostPadCount; i++)
                {
                    float t = UnityEngine.Random.Range(0.1f, 0.9f);
                    PlaceObjectAtSplinePosition(boostPadPrefab, t, "BoostPad");
                }
            }
            
            // Place weapon pads
            if (autoPlaceWeaponPads && weaponPadPrefab != null)
            {
                for (int i = 0; i < weaponPadCount; i++)
                {
                    float t = UnityEngine.Random.Range(0.1f, 0.9f);
                    PlaceObjectAtSplinePosition(weaponPadPrefab, t, "WeaponPad");
                }
            }
            
            // Place energy cells
            if (autoPlaceEnergyCells && energyCellPrefab != null)
            {
                for (int i = 0; i < energyCellCount; i++)
                {
                    float t = UnityEngine.Random.Range(0.1f, 0.9f);
                    PlaceObjectAtSplinePosition(energyCellPrefab, t, "EnergyCell");
                }
            }
        }
        
        void PlaceObjectAtSplinePosition(GameObject prefab, float t, string baseName)
        {
            Spline spline = splineContainer.Spline;
            float3 position = spline.EvaluatePosition(t);
            float3 tangent = spline.EvaluateTangent(t);
            float3 up = spline.EvaluateUpVector(t);
            
            Quaternion rotation = Quaternion.LookRotation((Vector3)tangent, (Vector3)up);
            
            GameObject obj = Instantiate(prefab, (Vector3)position + ((Vector3)up * 0.5f), rotation);
            obj.name = $"{baseName}_{spawnedObjects.Count:D3}";
            obj.transform.SetParent(transform);
            
            spawnedObjects.Add(obj);
        }
        
        Material CreateDefaultMaterial(Color color)
        {
            Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            return mat;
        }
    }
}
