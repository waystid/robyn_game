using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using CozyGame.Map;

namespace CozyGame.UI
{
    /// <summary>
    /// Minimap UI controller.
    /// Shows real-time player position and nearby markers.
    /// </summary>
    public class MinimapUI : MonoBehaviour
    {
        public static MinimapUI Instance { get; private set; }

        [Header("UI Elements")]
        [Tooltip("Minimap container")]
        public RectTransform minimapContainer;

        [Tooltip("Map image (background)")]
        public RawImage mapImage;

        [Tooltip("Player icon")]
        public RectTransform playerIcon;

        [Tooltip("Marker icon prefab")]
        public GameObject markerIconPrefab;

        [Tooltip("Marker container")]
        public RectTransform markerContainer;

        [Header("Settings")]
        [Tooltip("Minimap size (radius in meters)")]
        public float minimapRadius = 50f;

        [Tooltip("Rotate with player")]
        public bool rotateWithPlayer = true;

        [Tooltip("Show fog of war on minimap")]
        public bool showFogOfWar = true;

        [Tooltip("Update frequency (times per second)")]
        public float updateFrequency = 10f;

        [Header("Marker Filters")]
        [Tooltip("Show quest markers")]
        public bool showQuestMarkers = true;

        [Tooltip("Show waypoint markers")]
        public bool showWaypointMarkers = true;

        [Tooltip("Show POI markers")]
        public bool showPOIMarkers = true;

        [Tooltip("Show enemy markers")]
        public bool showEnemyMarkers = false;

        // State
        private Dictionary<string, GameObject> markerIcons = new Dictionary<string, GameObject>();
        private float updateTimer = 0f;
        private Camera mainCamera;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else if (Instance != this)
            {
                Destroy(gameObject);
            }
        }

        private void Start()
        {
            mainCamera = Camera.main;

            // Setup map image
            if (mapImage != null && MapSystem.Instance != null)
            {
                if (MapSystem.Instance.worldMapTexture != null)
                {
                    mapImage.texture = MapSystem.Instance.worldMapTexture;
                }
            }

            // Subscribe to map events
            if (MapSystem.Instance != null)
            {
                MapSystem.Instance.OnMarkerAdded.AddListener(OnMarkerAdded);
                MapSystem.Instance.OnMarkerRemoved.AddListener(OnMarkerRemoved);
            }
        }

        private void Update()
        {
            updateTimer += Time.deltaTime;

            if (updateTimer >= 1f / updateFrequency)
            {
                updateTimer = 0f;
                UpdateMinimap();
            }
        }

        /// <summary>
        /// Update minimap display
        /// </summary>
        private void UpdateMinimap()
        {
            if (MapSystem.Instance == null)
                return;

            // Update player icon
            UpdatePlayerIcon();

            // Update marker icons
            UpdateMarkerIcons();

            // Update map rotation
            if (rotateWithPlayer && minimapContainer != null)
            {
                float rotation = -MapSystem.Instance.GetPlayerRotation();
                minimapContainer.rotation = Quaternion.Euler(0f, 0f, rotation);
            }
        }

        /// <summary>
        /// Update player icon
        /// </summary>
        private void UpdatePlayerIcon()
        {
            if (playerIcon == null || minimapContainer == null)
                return;

            // Player is always at center of minimap
            playerIcon.anchoredPosition = Vector2.zero;

            // Rotate player icon to face forward
            if (!rotateWithPlayer)
            {
                float rotation = MapSystem.Instance.GetPlayerRotation();
                playerIcon.rotation = Quaternion.Euler(0f, 0f, -rotation);
            }
            else
            {
                playerIcon.rotation = Quaternion.identity;
            }
        }

        /// <summary>
        /// Update marker icons
        /// </summary>
        private void UpdateMarkerIcons()
        {
            if (markerContainer == null || MapSystem.Instance == null)
                return;

            Vector3 playerPos = MapSystem.Instance.GetPlayerPosition();
            List<MapMarker> discoveredMarkers = MapSystem.Instance.GetDiscoveredMarkers();

            // Track which markers are still active
            HashSet<string> activeMarkerIDs = new HashSet<string>();

            foreach (var marker in discoveredMarkers)
            {
                // Skip player marker
                if (marker.markerType == MapMarkerType.Player)
                    continue;

                // Check if marker type should be shown
                if (!ShouldShowMarkerType(marker.markerType))
                    continue;

                // Check distance
                float distance = Vector3.Distance(playerPos, marker.worldPosition);
                if (distance > minimapRadius)
                    continue;

                activeMarkerIDs.Add(marker.markerID);

                // Get or create marker icon
                GameObject iconObj;
                if (!markerIcons.TryGetValue(marker.markerID, out iconObj))
                {
                    iconObj = CreateMarkerIcon(marker);
                    markerIcons[marker.markerID] = iconObj;
                }

                if (iconObj != null)
                {
                    // Update position
                    Vector2 localPos = WorldToMinimapPosition(marker.worldPosition, playerPos);
                    RectTransform iconRect = iconObj.GetComponent<RectTransform>();
                    if (iconRect != null)
                    {
                        iconRect.anchoredPosition = localPos;
                    }

                    // Show icon
                    iconObj.SetActive(true);
                }
            }

            // Hide/remove inactive markers
            List<string> toRemove = new List<string>();
            foreach (var kvp in markerIcons)
            {
                if (!activeMarkerIDs.Contains(kvp.Key))
                {
                    if (kvp.Value != null)
                    {
                        kvp.Value.SetActive(false);
                    }
                }
            }
        }

        /// <summary>
        /// Convert world position to minimap position
        /// </summary>
        private Vector2 WorldToMinimapPosition(Vector3 worldPos, Vector3 playerPos)
        {
            // Get offset from player
            Vector3 offset = worldPos - playerPos;

            // Convert to 2D
            Vector2 offset2D = new Vector2(offset.x, offset.z);

            // If rotating with player, rotate offset
            if (rotateWithPlayer)
            {
                float angle = MapSystem.Instance.GetPlayerRotation() * Mathf.Deg2Rad;
                float cos = Mathf.Cos(angle);
                float sin = Mathf.Sin(angle);
                offset2D = new Vector2(
                    offset2D.x * cos - offset2D.y * sin,
                    offset2D.x * sin + offset2D.y * cos
                );
            }

            // Scale to minimap size
            if (minimapContainer != null)
            {
                float minimapSize = minimapContainer.rect.width; // Assuming square minimap
                float scale = minimapSize / (2f * minimapRadius);
                offset2D *= scale;
            }

            return offset2D;
        }

        /// <summary>
        /// Create marker icon
        /// </summary>
        private GameObject CreateMarkerIcon(MapMarker marker)
        {
            if (markerIconPrefab == null || markerContainer == null)
                return null;

            GameObject iconObj = Instantiate(markerIconPrefab, markerContainer);
            iconObj.name = $"MarkerIcon_{marker.markerID}";

            // Set icon sprite
            Image iconImage = iconObj.GetComponent<Image>();
            if (iconImage != null)
            {
                if (marker.icon != null)
                {
                    iconImage.sprite = marker.icon;
                }
                iconImage.color = marker.iconColor;
            }

            return iconObj;
        }

        /// <summary>
        /// Should show marker type on minimap?
        /// </summary>
        private bool ShouldShowMarkerType(MapMarkerType type)
        {
            switch (type)
            {
                case MapMarkerType.Quest:
                case MapMarkerType.QuestGiver:
                    return showQuestMarkers;
                case MapMarkerType.Waypoint:
                    return showWaypointMarkers;
                case MapMarkerType.PointOfInterest:
                    return showPOIMarkers;
                case MapMarkerType.Enemy:
                    return showEnemyMarkers;
                default:
                    return true;
            }
        }

        /// <summary>
        /// Marker added callback
        /// </summary>
        private void OnMarkerAdded(MapMarker marker)
        {
            // Will be created on next update if in range
        }

        /// <summary>
        /// Marker removed callback
        /// </summary>
        private void OnMarkerRemoved(MapMarker marker)
        {
            if (markerIcons.ContainsKey(marker.markerID))
            {
                GameObject iconObj = markerIcons[marker.markerID];
                if (iconObj != null)
                {
                    Destroy(iconObj);
                }
                markerIcons.Remove(marker.markerID);
            }
        }

        /// <summary>
        /// Set minimap visibility
        /// </summary>
        public void SetVisible(bool visible)
        {
            if (minimapContainer != null)
            {
                minimapContainer.gameObject.SetActive(visible);
            }
        }

        /// <summary>
        /// Set minimap zoom
        /// </summary>
        public void SetZoom(float zoom)
        {
            minimapRadius = 50f / zoom;
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (MapSystem.Instance != null)
            {
                MapSystem.Instance.OnMarkerAdded.RemoveListener(OnMarkerAdded);
                MapSystem.Instance.OnMarkerRemoved.RemoveListener(OnMarkerRemoved);
            }
        }
    }
}
