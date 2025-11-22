using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections.Generic;
using CozyGame.Map;

namespace CozyGame.UI
{
    /// <summary>
    /// World map UI controller.
    /// Full-screen interactive map with markers and fog of war.
    /// </summary>
    public class WorldMapUI : MonoBehaviour
    {
        public static WorldMapUI Instance { get; private set; }

        [Header("UI Panels")]
        [Tooltip("World map panel")]
        public GameObject worldMapPanel;

        [Header("Map Display")]
        [Tooltip("Map image")]
        public RawImage mapImage;

        [Tooltip("Fog of war overlay")]
        public RawImage fogOfWarImage;

        [Tooltip("Map rect transform (for panning/zooming)")]
        public RectTransform mapRectTransform;

        [Header("Player")]
        [Tooltip("Player icon")]
        public RectTransform playerIcon;

        [Header("Markers")]
        [Tooltip("Marker container")]
        public RectTransform markerContainer;

        [Tooltip("Marker button prefab")]
        public GameObject markerButtonPrefab;

        [Header("Details Panel")]
        [Tooltip("Marker details panel")]
        public GameObject markerDetailsPanel;

        [Tooltip("Marker name text")]
        public TextMeshProUGUI markerNameText;

        [Tooltip("Marker description text")]
        public TextMeshProUGUI markerDescriptionText;

        [Tooltip("Marker type text")]
        public TextMeshProUGUI markerTypeText;

        [Tooltip("Travel button (if waypoint)")]
        public Button travelButton;

        [Header("Controls")]
        [Tooltip("Close button")]
        public Button closeButton;

        [Tooltip("Zoom in button")]
        public Button zoomInButton;

        [Tooltip("Zoom out button")]
        public Button zoomOutButton;

        [Tooltip("Recenter button")]
        public Button recenterButton;

        [Header("Zoom Settings")]
        [Tooltip("Min zoom level")]
        public float minZoom = 0.5f;

        [Tooltip("Max zoom level")]
        public float maxZoom = 3f;

        [Tooltip("Current zoom")]
        public float currentZoom = 1f;

        [Tooltip("Zoom speed (scroll wheel)")]
        public float zoomSpeed = 0.1f;

        [Header("Pan Settings")]
        [Tooltip("Enable panning")]
        public bool enablePanning = true;

        [Tooltip("Pan speed (mouse drag)")]
        public float panSpeed = 1f;

        // State
        private Dictionary<string, GameObject> markerButtons = new Dictionary<string, GameObject>();
        private MapMarker selectedMarker;
        private Vector2 panOffset = Vector2.zero;
        private bool isPanning = false;
        private Vector2 lastMousePosition;

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
            // Setup buttons
            if (closeButton != null)
            {
                closeButton.onClick.AddListener(CloseMap);
            }

            if (zoomInButton != null)
            {
                zoomInButton.onClick.AddListener(() => Zoom(0.2f));
            }

            if (zoomOutButton != null)
            {
                zoomOutButton.onClick.AddListener(() => Zoom(-0.2f));
            }

            if (recenterButton != null)
            {
                recenterButton.onClick.AddListener(RecenterOnPlayer);
            }

            if (travelButton != null)
            {
                travelButton.onClick.AddListener(OnTravelClicked);
            }

            // Setup map textures
            if (MapSystem.Instance != null)
            {
                if (mapImage != null && MapSystem.Instance.worldMapTexture != null)
                {
                    mapImage.texture = MapSystem.Instance.worldMapTexture;
                }

                if (fogOfWarImage != null && MapSystem.Instance.fogOfWarTexture != null)
                {
                    fogOfWarImage.texture = MapSystem.Instance.fogOfWarTexture;
                }
            }

            // Subscribe to events
            if (MapSystem.Instance != null)
            {
                MapSystem.Instance.OnMarkerAdded.AddListener(OnMarkerAdded);
                MapSystem.Instance.OnMarkerRemoved.AddListener(OnMarkerRemoved);
                MapSystem.Instance.OnMarkerDiscovered.AddListener(OnMarkerDiscovered);
            }

            // Hide panels initially
            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(false);
            }

            if (markerDetailsPanel != null)
            {
                markerDetailsPanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (worldMapPanel == null || !worldMapPanel.activeSelf)
                return;

            // Handle zoom with mouse wheel
            float scrollDelta = Input.mouseScrollDelta.y;
            if (Mathf.Abs(scrollDelta) > 0.01f)
            {
                Zoom(scrollDelta * zoomSpeed);
            }

            // Handle panning with mouse drag
            if (enablePanning)
            {
                if (Input.GetMouseButtonDown(0))
                {
                    // Check if not clicking on UI
                    if (!EventSystem.current.IsPointerOverGameObject())
                    {
                        isPanning = true;
                        lastMousePosition = Input.mousePosition;
                    }
                }

                if (Input.GetMouseButtonUp(0))
                {
                    isPanning = false;
                }

                if (isPanning && Input.GetMouseButton(0))
                {
                    Vector2 currentMousePosition = Input.mousePosition;
                    Vector2 delta = currentMousePosition - lastMousePosition;
                    panOffset += delta * panSpeed / currentZoom;
                    lastMousePosition = currentMousePosition;

                    UpdateMapPosition();
                }
            }

            // Update player icon position
            UpdatePlayerIconPosition();

            // Update marker positions
            UpdateMarkerPositions();
        }

        /// <summary>
        /// Open world map
        /// </summary>
        public void OpenMap()
        {
            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(true);
            }

            // Recenter on player
            RecenterOnPlayer();

            // Refresh markers
            RefreshAllMarkers();
        }

        /// <summary>
        /// Close world map
        /// </summary>
        public void CloseMap()
        {
            if (worldMapPanel != null)
            {
                worldMapPanel.SetActive(false);
            }

            if (markerDetailsPanel != null)
            {
                markerDetailsPanel.SetActive(false);
            }

            selectedMarker = null;
        }

        /// <summary>
        /// Zoom map
        /// </summary>
        public void Zoom(float delta)
        {
            currentZoom = Mathf.Clamp(currentZoom + delta, minZoom, maxZoom);

            if (mapRectTransform != null)
            {
                mapRectTransform.localScale = Vector3.one * currentZoom;
            }
        }

        /// <summary>
        /// Recenter map on player
        /// </summary>
        public void RecenterOnPlayer()
        {
            panOffset = Vector2.zero;
            UpdateMapPosition();
        }

        /// <summary>
        /// Update map position (from panning)
        /// </summary>
        private void UpdateMapPosition()
        {
            if (mapRectTransform != null)
            {
                mapRectTransform.anchoredPosition = panOffset;
            }
        }

        /// <summary>
        /// Update player icon position
        /// </summary>
        private void UpdatePlayerIconPosition()
        {
            if (playerIcon == null || MapSystem.Instance == null || mapRectTransform == null)
                return;

            Vector3 playerPos = MapSystem.Instance.GetPlayerPosition();
            Vector2 uv = MapSystem.Instance.WorldToMapUV(playerPos);

            // Convert UV to map rect position
            Vector2 mapSize = mapRectTransform.rect.size;
            Vector2 localPos = new Vector2(
                (uv.x - 0.5f) * mapSize.x,
                (uv.y - 0.5f) * mapSize.y
            );

            playerIcon.anchoredPosition = localPos;

            // Rotate player icon
            float rotation = MapSystem.Instance.GetPlayerRotation();
            playerIcon.rotation = Quaternion.Euler(0f, 0f, -rotation);
        }

        /// <summary>
        /// Update marker positions
        /// </summary>
        private void UpdateMarkerPositions()
        {
            if (MapSystem.Instance == null || mapRectTransform == null)
                return;

            Vector2 mapSize = mapRectTransform.rect.size;

            foreach (var kvp in markerButtons)
            {
                MapMarker marker = MapSystem.Instance.GetMarker(kvp.Key);
                if (marker != null && marker.isDiscovered && marker.isVisible)
                {
                    Vector2 uv = MapSystem.Instance.WorldToMapUV(marker.worldPosition);
                    Vector2 localPos = new Vector2(
                        (uv.x - 0.5f) * mapSize.x,
                        (uv.y - 0.5f) * mapSize.y
                    );

                    RectTransform buttonRect = kvp.Value.GetComponent<RectTransform>();
                    if (buttonRect != null)
                    {
                        buttonRect.anchoredPosition = localPos;
                    }

                    kvp.Value.SetActive(true);
                }
                else
                {
                    kvp.Value.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Refresh all markers
        /// </summary>
        private void RefreshAllMarkers()
        {
            if (MapSystem.Instance == null)
                return;

            List<MapMarker> discoveredMarkers = MapSystem.Instance.GetDiscoveredMarkers();

            foreach (var marker in discoveredMarkers)
            {
                if (marker.markerType == MapMarkerType.Player)
                    continue;

                if (!markerButtons.ContainsKey(marker.markerID))
                {
                    CreateMarkerButton(marker);
                }
            }
        }

        /// <summary>
        /// Create marker button
        /// </summary>
        private void CreateMarkerButton(MapMarker marker)
        {
            if (markerButtonPrefab == null || markerContainer == null)
                return;

            GameObject buttonObj = Instantiate(markerButtonPrefab, markerContainer);
            buttonObj.name = $"MarkerButton_{marker.markerID}";

            // Set icon
            Image iconImage = buttonObj.GetComponent<Image>();
            if (iconImage != null)
            {
                if (marker.icon != null)
                {
                    iconImage.sprite = marker.icon;
                }
                iconImage.color = marker.iconColor;
            }

            // Setup button click
            Button button = buttonObj.GetComponent<Button>();
            if (button != null)
            {
                MapMarker m = marker; // Capture for lambda
                button.onClick.AddListener(() => OnMarkerClicked(m));
            }

            markerButtons[marker.markerID] = buttonObj;
        }

        /// <summary>
        /// Marker clicked callback
        /// </summary>
        private void OnMarkerClicked(MapMarker marker)
        {
            selectedMarker = marker;
            ShowMarkerDetails(marker);
        }

        /// <summary>
        /// Show marker details panel
        /// </summary>
        private void ShowMarkerDetails(MapMarker marker)
        {
            if (markerDetailsPanel == null)
                return;

            markerDetailsPanel.SetActive(true);

            // Update name
            if (markerNameText != null)
            {
                markerNameText.text = marker.markerName;
            }

            // Update description
            if (markerDescriptionText != null)
            {
                markerDescriptionText.text = marker.description;
            }

            // Update type
            if (markerTypeText != null)
            {
                markerTypeText.text = marker.markerType.ToString();
            }

            // Show travel button if waypoint
            if (travelButton != null)
            {
                travelButton.gameObject.SetActive(marker.markerType == MapMarkerType.Waypoint);
            }
        }

        /// <summary>
        /// Travel button clicked
        /// </summary>
        private void OnTravelClicked()
        {
            if (selectedMarker == null)
                return;

            // Open fast travel UI
            if (FastTravelUI.Instance != null)
            {
                // Find waypoint for this marker
                FastTravel.WaypointSystem waypoint System = FastTravel.WaypointSystem.Instance;
                if (waypointSystem != null)
                {
                    FastTravel.Waypoint waypoint = waypointSystem.GetWaypoint(selectedMarker.markerID);
                    if (waypoint != null)
                    {
                        CloseMap();
                        FastTravelUI.Instance.ShowTravelConfirmation(waypoint);
                    }
                }
            }
        }

        /// <summary>
        /// Marker added callback
        /// </summary>
        private void OnMarkerAdded(MapMarker marker)
        {
            if (marker.isDiscovered && marker.markerType != MapMarkerType.Player)
            {
                CreateMarkerButton(marker);
            }
        }

        /// <summary>
        /// Marker removed callback
        /// </summary>
        private void OnMarkerRemoved(MapMarker marker)
        {
            if (markerButtons.ContainsKey(marker.markerID))
            {
                GameObject buttonObj = markerButtons[marker.markerID];
                if (buttonObj != null)
                {
                    Destroy(buttonObj);
                }
                markerButtons.Remove(marker.markerID);
            }
        }

        /// <summary>
        /// Marker discovered callback
        /// </summary>
        private void OnMarkerDiscovered(MapMarker marker)
        {
            if (marker.markerType != MapMarkerType.Player)
            {
                CreateMarkerButton(marker);
            }
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (MapSystem.Instance != null)
            {
                MapSystem.Instance.OnMarkerAdded.RemoveListener(OnMarkerAdded);
                MapSystem.Instance.OnMarkerRemoved.RemoveListener(OnMarkerRemoved);
                MapSystem.Instance.OnMarkerDiscovered.RemoveListener(OnMarkerDiscovered);
            }
        }
    }
}
