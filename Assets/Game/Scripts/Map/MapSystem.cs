using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

namespace CozyGame.Map
{
    /// <summary>
    /// Map marker type
    /// </summary>
    public enum MapMarkerType
    {
        Player,             // Player position
        Quest,              // Quest objective
        QuestGiver,         // NPC with quest
        Shop,               // Shop/vendor
        CraftingStation,    // Crafting location
        Resource,           // Resource node
        Waypoint,           // Fast travel point
        PointOfInterest,    // POI
        Enemy,              // Enemy/boss
        Custom              // Custom marker
    }

    /// <summary>
    /// Map marker data
    /// </summary>
    [System.Serializable]
    public class MapMarker
    {
        public string markerID;
        public string markerName;
        public MapMarkerType markerType;
        public Vector3 worldPosition;
        public Sprite icon;
        public Color iconColor = Color.white;
        public bool isVisible = true;
        public bool isDiscovered = false;
        public string description;
        public GameObject linkedObject; // Optional reference to world object

        public MapMarker(string id, string name, MapMarkerType type, Vector3 position)
        {
            markerID = id;
            markerName = name;
            markerType = type;
            worldPosition = position;
            isVisible = true;
            isDiscovered = true;
        }
    }

    /// <summary>
    /// Map data save structure
    /// </summary>
    [System.Serializable]
    public class MapSaveData
    {
        public List<string> discoveredMarkerIDs = new List<string>();
        public List<Vector2Int> exploredChunks = new List<Vector2Int>();
        public Vector3 lastPlayerPosition;
    }

    /// <summary>
    /// Map system singleton.
    /// Manages world map, minimap, markers, and fog of war.
    /// </summary>
    public class MapSystem : MonoBehaviour
    {
        public static MapSystem Instance { get; private set; }

        [Header("Map Settings")]
        [Tooltip("World map bounds (min corner)")]
        public Vector2 worldBoundsMin = new Vector2(-500f, -500f);

        [Tooltip("World map bounds (max corner)")]
        public Vector2 worldBoundsMax = new Vector2(500f, 500f);

        [Tooltip("Map texture size")]
        public Vector2Int mapTextureSize = new Vector2Int(1024, 1024);

        [Tooltip("World map texture (optional pre-rendered)")]
        public Texture2D worldMapTexture;

        [Header("Fog of War")]
        [Tooltip("Enable fog of war")]
        public bool enableFogOfWar = true;

        [Tooltip("Exploration chunk size (meters)")]
        public float explorationChunkSize = 10f;

        [Tooltip("Exploration radius around player")]
        public float explorationRadius = 20f;

        [Tooltip("Fog of war texture")]
        public Texture2D fogOfWarTexture;

        [Header("Minimap")]
        [Tooltip("Show minimap")]
        public bool showMinimap = true;

        [Tooltip("Minimap zoom level")]
        [Range(0.5f, 5f)]
        public float minimapZoom = 1f;

        [Tooltip("Minimap rotation (follow player)")]
        public bool minimapRotates = true;

        [Header("Player Tracking")]
        [Tooltip("Player transform (auto-detected if null)")]
        public Transform playerTransform;

        [Tooltip("Update player position every X seconds")]
        public float playerUpdateInterval = 0.1f;

        [Header("Events")]
        public UnityEvent<MapMarker> OnMarkerDiscovered;
        public UnityEvent<MapMarker> OnMarkerAdded;
        public UnityEvent<MapMarker> OnMarkerRemoved;
        public UnityEvent<Vector2Int> OnChunkExplored;

        // State
        private List<MapMarker> activeMarkers = new List<MapMarker>();
        private HashSet<Vector2Int> exploredChunks = new HashSet<Vector2Int>();
        private Vector3 lastPlayerPosition;
        private float playerUpdateTimer = 0f;
        private MapMarker playerMarker;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                Initialize();
            }
            else
            {
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Initialize map system
        /// </summary>
        private void Initialize()
        {
            // Find player if not set
            if (playerTransform == null)
            {
                GameObject player = GameObject.FindGameObjectWithTag("Player");
                if (player != null)
                {
                    playerTransform = player.transform;
                }
            }

            // Create player marker
            if (playerTransform != null)
            {
                playerMarker = new MapMarker("player", "Player", MapMarkerType.Player, playerTransform.position);
                playerMarker.iconColor = Color.green;
                activeMarkers.Add(playerMarker);
            }

            // Initialize fog of war texture
            if (enableFogOfWar && fogOfWarTexture == null)
            {
                CreateFogOfWarTexture();
            }

            Debug.Log("[MapSystem] Initialized");
        }

        private void Update()
        {
            // Update player position
            if (playerTransform != null)
            {
                playerUpdateTimer += Time.deltaTime;

                if (playerUpdateTimer >= playerUpdateInterval)
                {
                    playerUpdateTimer = 0f;
                    UpdatePlayerPosition();
                }
            }
        }

        /// <summary>
        /// Update player position and exploration
        /// </summary>
        private void UpdatePlayerPosition()
        {
            if (playerTransform == null)
                return;

            Vector3 currentPos = playerTransform.position;

            // Update player marker
            if (playerMarker != null)
            {
                playerMarker.worldPosition = currentPos;
            }

            // Check if moved significantly
            if (Vector3.Distance(currentPos, lastPlayerPosition) > 1f)
            {
                lastPlayerPosition = currentPos;

                // Update exploration
                if (enableFogOfWar)
                {
                    ExploreArea(currentPos, explorationRadius);
                }
            }
        }

        /// <summary>
        /// Explore area around position
        /// </summary>
        private void ExploreArea(Vector3 center, float radius)
        {
            // Calculate chunk range
            Vector2Int centerChunk = WorldToChunk(new Vector2(center.x, center.z));
            int chunkRadius = Mathf.CeilToInt(radius / explorationChunkSize);

            for (int x = -chunkRadius; x <= chunkRadius; x++)
            {
                for (int z = -chunkRadius; z <= chunkRadius; z++)
                {
                    Vector2Int chunk = new Vector2Int(centerChunk.x + x, centerChunk.y + z);

                    // Check if within radius
                    Vector2 chunkWorldPos = ChunkToWorld(chunk);
                    float distance = Vector2.Distance(new Vector2(center.x, center.z), chunkWorldPos);

                    if (distance <= radius)
                    {
                        ExploreChunk(chunk);
                    }
                }
            }
        }

        /// <summary>
        /// Explore a chunk
        /// </summary>
        private void ExploreChunk(Vector2Int chunk)
        {
            if (exploredChunks.Contains(chunk))
                return;

            exploredChunks.Add(chunk);
            OnChunkExplored?.Invoke(chunk);

            // Update fog of war texture
            UpdateFogOfWarTexture(chunk);

            // Check for markers in this chunk to discover
            DiscoverMarkersInChunk(chunk);
        }

        /// <summary>
        /// Discover markers in chunk
        /// </summary>
        private void DiscoverMarkersInChunk(Vector2Int chunk)
        {
            Vector2 chunkMin = ChunkToWorld(chunk) - Vector2.one * explorationChunkSize * 0.5f;
            Vector2 chunkMax = chunkMin + Vector2.one * explorationChunkSize;

            foreach (var marker in activeMarkers)
            {
                if (marker.isDiscovered)
                    continue;

                Vector2 markerPos2D = new Vector2(marker.worldPosition.x, marker.worldPosition.z);

                if (markerPos2D.x >= chunkMin.x && markerPos2D.x <= chunkMax.x &&
                    markerPos2D.y >= chunkMin.y && markerPos2D.y <= chunkMax.y)
                {
                    DiscoverMarker(marker);
                }
            }
        }

        /// <summary>
        /// Discover marker
        /// </summary>
        public void DiscoverMarker(MapMarker marker)
        {
            if (marker == null || marker.isDiscovered)
                return;

            marker.isDiscovered = true;
            OnMarkerDiscovered?.Invoke(marker);

            // Show notification
            if (FloatingTextManager.Instance != null && Camera.main != null)
            {
                FloatingTextManager.Instance.Show(
                    $"Discovered: {marker.markerName}",
                    Camera.main.transform.position + Camera.main.transform.forward * 3f,
                    Color.cyan
                );
            }

            Debug.Log($"[MapSystem] Discovered marker: {marker.markerName}");
        }

        /// <summary>
        /// Add marker to map
        /// </summary>
        public MapMarker AddMarker(string id, string name, MapMarkerType type, Vector3 worldPos, bool discovered = false)
        {
            // Check if marker already exists
            MapMarker existing = GetMarker(id);
            if (existing != null)
            {
                Debug.LogWarning($"[MapSystem] Marker {id} already exists!");
                return existing;
            }

            MapMarker marker = new MapMarker(id, name, type, worldPos);
            marker.isDiscovered = discovered;
            marker.icon = GetDefaultIcon(type);
            marker.iconColor = GetDefaultColor(type);

            activeMarkers.Add(marker);
            OnMarkerAdded?.Invoke(marker);

            Debug.Log($"[MapSystem] Added marker: {name} ({type})");

            return marker;
        }

        /// <summary>
        /// Remove marker from map
        /// </summary>
        public void RemoveMarker(string id)
        {
            MapMarker marker = GetMarker(id);
            if (marker == null)
                return;

            activeMarkers.Remove(marker);
            OnMarkerRemoved?.Invoke(marker);

            Debug.Log($"[MapSystem] Removed marker: {marker.markerName}");
        }

        /// <summary>
        /// Get marker by ID
        /// </summary>
        public MapMarker GetMarker(string id)
        {
            return activeMarkers.Find(m => m.markerID == id);
        }

        /// <summary>
        /// Get all markers of type
        /// </summary>
        public List<MapMarker> GetMarkersOfType(MapMarkerType type)
        {
            return activeMarkers.FindAll(m => m.markerType == type);
        }

        /// <summary>
        /// Get all discovered markers
        /// </summary>
        public List<MapMarker> GetDiscoveredMarkers()
        {
            return activeMarkers.FindAll(m => m.isDiscovered && m.isVisible);
        }

        /// <summary>
        /// Convert world position to map UV coordinates (0-1)
        /// </summary>
        public Vector2 WorldToMapUV(Vector3 worldPos)
        {
            Vector2 pos2D = new Vector2(worldPos.x, worldPos.z);
            Vector2 normalized = new Vector2(
                Mathf.InverseLerp(worldBoundsMin.x, worldBoundsMax.x, pos2D.x),
                Mathf.InverseLerp(worldBoundsMin.y, worldBoundsMax.y, pos2D.y)
            );
            return normalized;
        }

        /// <summary>
        /// Convert map UV coordinates to world position
        /// </summary>
        public Vector3 MapUVToWorld(Vector2 uv)
        {
            float x = Mathf.Lerp(worldBoundsMin.x, worldBoundsMax.x, uv.x);
            float z = Mathf.Lerp(worldBoundsMin.y, worldBoundsMax.y, uv.y);
            return new Vector3(x, 0f, z);
        }

        /// <summary>
        /// Convert world position to chunk coordinates
        /// </summary>
        public Vector2Int WorldToChunk(Vector2 worldPos)
        {
            return new Vector2Int(
                Mathf.FloorToInt(worldPos.x / explorationChunkSize),
                Mathf.FloorToInt(worldPos.y / explorationChunkSize)
            );
        }

        /// <summary>
        /// Convert chunk coordinates to world position (center)
        /// </summary>
        public Vector2 ChunkToWorld(Vector2Int chunk)
        {
            return new Vector2(
                chunk.x * explorationChunkSize + explorationChunkSize * 0.5f,
                chunk.y * explorationChunkSize + explorationChunkSize * 0.5f
            );
        }

        /// <summary>
        /// Is chunk explored?
        /// </summary>
        public bool IsChunkExplored(Vector2Int chunk)
        {
            return exploredChunks.Contains(chunk);
        }

        /// <summary>
        /// Create fog of war texture
        /// </summary>
        private void CreateFogOfWarTexture()
        {
            fogOfWarTexture = new Texture2D(mapTextureSize.x, mapTextureSize.y, TextureFormat.RGBA32, false);
            fogOfWarTexture.filterMode = FilterMode.Bilinear;

            // Fill with black (unexplored)
            Color[] pixels = new Color[mapTextureSize.x * mapTextureSize.y];
            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = new Color(0f, 0f, 0f, 1f); // Black, fully opaque
            }

            fogOfWarTexture.SetPixels(pixels);
            fogOfWarTexture.Apply();
        }

        /// <summary>
        /// Update fog of war texture for chunk
        /// </summary>
        private void UpdateFogOfWarTexture(Vector2Int chunk)
        {
            if (fogOfWarTexture == null)
                return;

            // Convert chunk to texture coordinates
            Vector2 chunkWorldPos = ChunkToWorld(chunk);
            Vector2 uv = WorldToMapUV(new Vector3(chunkWorldPos.x, 0f, chunkWorldPos.y));

            int centerX = Mathf.RoundToInt(uv.x * mapTextureSize.x);
            int centerY = Mathf.RoundToInt(uv.y * mapTextureSize.y);

            // Calculate chunk size in pixels
            float chunkSizeX = (explorationChunkSize / (worldBoundsMax.x - worldBoundsMin.x)) * mapTextureSize.x;
            float chunkSizeY = (explorationChunkSize / (worldBoundsMax.y - worldBoundsMin.y)) * mapTextureSize.y;

            int radiusX = Mathf.CeilToInt(chunkSizeX * 0.5f);
            int radiusY = Mathf.CeilToInt(chunkSizeY * 0.5f);

            // Clear fog in chunk area
            for (int x = -radiusX; x <= radiusX; x++)
            {
                for (int y = -radiusY; y <= radiusY; y++)
                {
                    int px = centerX + x;
                    int py = centerY + y;

                    if (px >= 0 && px < mapTextureSize.x && py >= 0 && py < mapTextureSize.y)
                    {
                        fogOfWarTexture.SetPixel(px, py, new Color(0f, 0f, 0f, 0f)); // Transparent (explored)
                    }
                }
            }

            fogOfWarTexture.Apply();
        }

        /// <summary>
        /// Get default icon for marker type
        /// </summary>
        private Sprite GetDefaultIcon(MapMarkerType type)
        {
            // TODO: Load from Resources
            return null;
        }

        /// <summary>
        /// Get default color for marker type
        /// </summary>
        private Color GetDefaultColor(MapMarkerType type)
        {
            switch (type)
            {
                case MapMarkerType.Player: return Color.green;
                case MapMarkerType.Quest: return Color.yellow;
                case MapMarkerType.QuestGiver: return new Color(1f, 0.8f, 0f);
                case MapMarkerType.Shop: return Color.blue;
                case MapMarkerType.CraftingStation: return Color.cyan;
                case MapMarkerType.Resource: return new Color(0.5f, 0.5f, 0.5f);
                case MapMarkerType.Waypoint: return Color.magenta;
                case MapMarkerType.PointOfInterest: return new Color(1f, 0.5f, 0f);
                case MapMarkerType.Enemy: return Color.red;
                default: return Color.white;
            }
        }

        /// <summary>
        /// Get player position
        /// </summary>
        public Vector3 GetPlayerPosition()
        {
            return playerTransform != null ? playerTransform.position : Vector3.zero;
        }

        /// <summary>
        /// Get player rotation
        /// </summary>
        public float GetPlayerRotation()
        {
            return playerTransform != null ? playerTransform.eulerAngles.y : 0f;
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public MapSaveData GetSaveData()
        {
            MapSaveData data = new MapSaveData();

            // Save discovered markers
            foreach (var marker in activeMarkers)
            {
                if (marker.isDiscovered && marker.markerType != MapMarkerType.Player)
                {
                    data.discoveredMarkerIDs.Add(marker.markerID);
                }
            }

            // Save explored chunks
            data.exploredChunks.AddRange(exploredChunks);

            // Save player position
            data.lastPlayerPosition = GetPlayerPosition();

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(MapSaveData data)
        {
            if (data == null)
                return;

            // Load discovered markers
            foreach (string markerID in data.discoveredMarkerIDs)
            {
                MapMarker marker = GetMarker(markerID);
                if (marker != null)
                {
                    marker.isDiscovered = true;
                }
            }

            // Load explored chunks
            exploredChunks.Clear();
            foreach (var chunk in data.exploredChunks)
            {
                exploredChunks.Add(chunk);
                UpdateFogOfWarTexture(chunk);
            }

            Debug.Log($"[MapSystem] Loaded {data.discoveredMarkerIDs.Count} markers, {data.exploredChunks.Count} chunks");
        }
    }
}
