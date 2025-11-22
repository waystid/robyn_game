using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

namespace CozyGame.Building
{
    /// <summary>
    /// Building mode state
    /// </summary>
    public enum BuildMode
    {
        None,
        Placing,
        Editing,
        Demolishing
    }

    /// <summary>
    /// Building placement validation result
    /// </summary>
    public struct PlacementValidation
    {
        public bool isValid;
        public string reason;
        public Vector3 validatedPosition;
        public Quaternion validatedRotation;

        public PlacementValidation(bool valid, string failReason = "")
        {
            isValid = valid;
            reason = failReason;
            validatedPosition = Vector3.zero;
            validatedRotation = Quaternion.identity;
        }
    }

    /// <summary>
    /// Building save data
    /// </summary>
    [System.Serializable]
    public class BuildingSaveData
    {
        public List<PlacedBuildingSaveData> placedBuildings = new List<PlacedBuildingSaveData>();
    }

    /// <summary>
    /// Building system singleton.
    /// Manages building placement, upgrades, and demolition.
    /// Integrates with inventory and currency systems.
    /// </summary>
    public class BuildingSystem : MonoBehaviour
    {
        public static BuildingSystem Instance { get; private set; }

        [Header("Build Mode")]
        [Tooltip("Current build mode")]
        public BuildMode currentMode = BuildMode.None;

        [Tooltip("Currently selected building data")]
        public BuildingData selectedBuilding;

        [Header("Placement Settings")]
        [Tooltip("Preview layer (for raycasting)")]
        public LayerMask placementSurfaceLayers = ~0;

        [Tooltip("Placement raycast distance")]
        public float placementRaycastDistance = 100f;

        [Tooltip("Grid snap enabled")]
        public bool gridSnapEnabled = true;

        [Tooltip("Default grid size")]
        public float defaultGridSize = 1f;

        [Tooltip("Show grid during placement")]
        public bool showPlacementGrid = true;

        [Tooltip("Grid material")]
        public Material gridMaterial;

        [Header("Preview")]
        [Tooltip("Preview object parent")]
        public Transform previewParent;

        [Tooltip("Update preview every frame")]
        public bool continuousPreviewUpdate = true;

        [Header("Validation")]
        [Tooltip("Check for collisions during placement")]
        public bool checkCollisions = true;

        [Tooltip("Collision check radius multiplier")]
        [Range(0.5f, 2f)]
        public float collisionCheckRadiusMultiplier = 1.1f;

        [Tooltip("Layers to check for collisions")]
        public LayerMask collisionCheckLayers = ~0;

        [Header("Camera")]
        [Tooltip("Main camera (auto-detected if null)")]
        public Camera mainCamera;

        [Header("Events")]
        public UnityEvent<PlacedBuilding> OnBuildingPlaced;
        public UnityEvent<PlacedBuilding> OnBuildingDemolished;
        public UnityEvent<PlacedBuilding, int> OnBuildingUpgraded; // building, newTier
        public UnityEvent<BuildMode> OnBuildModeChanged;

        // State
        private GameObject previewObject;
        private Renderer[] previewRenderers;
        private Material[] originalPreviewMaterials;
        private Vector3 currentPlacementPosition;
        private Quaternion currentPlacementRotation;
        private bool isValidPlacement = false;
        private float currentRotationAngle = 0f;

        // Placed buildings tracking
        private List<PlacedBuilding> placedBuildings = new List<PlacedBuilding>();

        // Grid visualization
        private GameObject gridVisualization;

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
        /// Initialize building system
        /// </summary>
        private void Initialize()
        {
            // Get main camera
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            // Create preview parent
            if (previewParent == null)
            {
                GameObject parent = new GameObject("BuildingPreviewParent");
                previewParent = parent.transform;
                previewParent.SetParent(transform);
            }

            // Create grid visualization
            if (showPlacementGrid)
            {
                CreateGridVisualization();
            }

            Debug.Log("[BuildingSystem] Initialized");
        }

        private void Update()
        {
            if (currentMode == BuildMode.Placing && selectedBuilding != null)
            {
                UpdatePlacementPreview();
                HandlePlacementInput();
            }
            else if (currentMode == BuildMode.Demolishing)
            {
                UpdateDemolishPreview();
                HandleDemolishInput();
            }
        }

        /// <summary>
        /// Enter build mode
        /// </summary>
        public void EnterBuildMode(BuildingData building)
        {
            if (building == null)
            {
                Debug.LogWarning("[BuildingSystem] Cannot enter build mode with null building");
                return;
            }

            // Check requirements
            string reason;
            if (!building.MeetsRequirements(out reason))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show(reason, mainCamera.transform.position + mainCamera.transform.forward * 3f, Color.red);
                }
                return;
            }

            selectedBuilding = building;
            currentMode = BuildMode.Placing;
            currentRotationAngle = 0f;

            CreatePreviewObject();
            OnBuildModeChanged?.Invoke(currentMode);

            Debug.Log($"[BuildingSystem] Entered build mode: {building.buildingName}");
        }

        /// <summary>
        /// Enter demolish mode
        /// </summary>
        public void EnterDemolishMode()
        {
            currentMode = BuildMode.Demolishing;
            OnBuildModeChanged?.Invoke(currentMode);

            Debug.Log("[BuildingSystem] Entered demolish mode");
        }

        /// <summary>
        /// Exit build mode
        /// </summary>
        public void ExitBuildMode()
        {
            currentMode = BuildMode.None;
            selectedBuilding = null;
            currentRotationAngle = 0f;

            DestroyPreviewObject();
            OnBuildModeChanged?.Invoke(currentMode);

            Debug.Log("[BuildingSystem] Exited build mode");
        }

        /// <summary>
        /// Create preview object
        /// </summary>
        private void CreatePreviewObject()
        {
            DestroyPreviewObject();

            if (selectedBuilding == null || selectedBuilding.basePrefab == null)
                return;

            // Instantiate preview
            previewObject = Instantiate(selectedBuilding.basePrefab, previewParent);
            previewObject.name = $"Preview_{selectedBuilding.buildingName}";

            // Disable colliders and scripts on preview
            foreach (var collider in previewObject.GetComponentsInChildren<Collider>())
            {
                collider.enabled = false;
            }

            foreach (var script in previewObject.GetComponentsInChildren<MonoBehaviour>())
            {
                script.enabled = false;
            }

            // Get renderers and store original materials
            previewRenderers = previewObject.GetComponentsInChildren<Renderer>();
            originalPreviewMaterials = new Material[previewRenderers.Length];

            for (int i = 0; i < previewRenderers.Length; i++)
            {
                originalPreviewMaterials[i] = previewRenderers[i].material;
            }

            // Apply preview material if available
            if (selectedBuilding.previewMaterial != null)
            {
                ApplyPreviewMaterial(selectedBuilding.previewMaterial);
            }
            else
            {
                // Create default transparent material
                CreateDefaultPreviewMaterials();
            }
        }

        /// <summary>
        /// Destroy preview object
        /// </summary>
        private void DestroyPreviewObject()
        {
            if (previewObject != null)
            {
                Destroy(previewObject);
                previewObject = null;
                previewRenderers = null;
                originalPreviewMaterials = null;
            }
        }

        /// <summary>
        /// Apply preview material to renderers
        /// </summary>
        private void ApplyPreviewMaterial(Material material)
        {
            if (previewRenderers == null)
                return;

            foreach (var renderer in previewRenderers)
            {
                renderer.material = material;
            }
        }

        /// <summary>
        /// Create default preview materials
        /// </summary>
        private void CreateDefaultPreviewMaterials()
        {
            if (previewRenderers == null)
                return;

            foreach (var renderer in previewRenderers)
            {
                Material mat = new Material(Shader.Find("Standard"));
                mat.SetFloat("_Mode", 3); // Transparent mode
                mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                mat.SetInt("_ZWrite", 0);
                mat.DisableKeyword("_ALPHATEST_ON");
                mat.EnableKeyword("_ALPHABLEND_ON");
                mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                mat.renderQueue = 3000;

                Color color = isValidPlacement ? selectedBuilding.validPlacementColor : selectedBuilding.invalidPlacementColor;
                mat.color = color;

                renderer.material = mat;
            }
        }

        /// <summary>
        /// Update placement preview
        /// </summary>
        private void UpdatePlacementPreview()
        {
            if (!continuousPreviewUpdate || previewObject == null)
                return;

            // Raycast to find placement position
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, placementRaycastDistance, placementSurfaceLayers))
            {
                Vector3 position = hit.point + selectedBuilding.placementOffset;
                Quaternion rotation = Quaternion.Euler(0f, currentRotationAngle, 0f);

                // Apply grid snapping
                if (gridSnapEnabled && selectedBuilding.snapToGrid)
                {
                    float gridSize = selectedBuilding.gridCellSize > 0 ? selectedBuilding.gridCellSize : defaultGridSize;
                    position.x = Mathf.Round(position.x / gridSize) * gridSize;
                    position.z = Mathf.Round(position.z / gridSize) * gridSize;
                }

                currentPlacementPosition = position;
                currentPlacementRotation = rotation;

                // Validate placement
                PlacementValidation validation = ValidatePlacement(position, rotation);
                isValidPlacement = validation.isValid;

                if (validation.isValid)
                {
                    currentPlacementPosition = validation.validatedPosition;
                    currentPlacementRotation = validation.validatedRotation;
                }

                // Update preview position
                previewObject.transform.position = currentPlacementPosition;
                previewObject.transform.rotation = currentPlacementRotation;

                // Update preview color
                UpdatePreviewColor();

                // Show grid
                if (gridVisualization != null)
                {
                    gridVisualization.SetActive(true);
                    gridVisualization.transform.position = position;
                }
            }
            else
            {
                // Hide preview if not hovering over valid surface
                if (gridVisualization != null)
                {
                    gridVisualization.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update preview color based on validity
        /// </summary>
        private void UpdatePreviewColor()
        {
            if (previewRenderers == null)
                return;

            Color color = isValidPlacement ? selectedBuilding.validPlacementColor : selectedBuilding.invalidPlacementColor;

            foreach (var renderer in previewRenderers)
            {
                if (renderer.material != null)
                {
                    renderer.material.color = color;
                }
            }
        }

        /// <summary>
        /// Validate placement at position
        /// </summary>
        private PlacementValidation ValidatePlacement(Vector3 position, Quaternion rotation)
        {
            if (selectedBuilding == null)
                return new PlacementValidation(false, "No building selected");

            // Check collision
            if (checkCollisions)
            {
                Vector3 checkSize = new Vector3(
                    selectedBuilding.gridSize.x * selectedBuilding.gridCellSize,
                    selectedBuilding.buildingHeight,
                    selectedBuilding.gridSize.y * selectedBuilding.gridCellSize
                );

                Vector3 checkPosition = position + Vector3.up * (checkSize.y * 0.5f);
                checkSize *= collisionCheckRadiusMultiplier;

                Collider[] colliders = Physics.OverlapBox(checkPosition, checkSize * 0.5f, rotation, collisionCheckLayers);
                if (colliders.Length > 0)
                {
                    return new PlacementValidation(false, "Blocked by obstacle");
                }
            }

            // Check distance from other buildings
            if (selectedBuilding.requirements.minDistanceFromOthers > 0f)
            {
                foreach (var building in placedBuildings)
                {
                    if (building == null)
                        continue;

                    float distance = Vector3.Distance(position, building.transform.position);
                    if (distance < selectedBuilding.requirements.minDistanceFromOthers)
                    {
                        return new PlacementValidation(false, "Too close to another building");
                    }
                }
            }

            // TODO: Check water proximity if required
            // TODO: Check indoor/outdoor requirement
            // TODO: Check flat ground requirement

            var validation = new PlacementValidation(true);
            validation.validatedPosition = position;
            validation.validatedRotation = rotation;
            return validation;
        }

        /// <summary>
        /// Handle placement input
        /// </summary>
        private void HandlePlacementInput()
        {
            // Rotate with R key or mouse wheel
            if (selectedBuilding.allowRotation)
            {
                if (Input.GetKeyDown(KeyCode.R))
                {
                    currentRotationAngle += selectedBuilding.rotationSnapAngle;
                    if (currentRotationAngle >= 360f)
                        currentRotationAngle -= 360f;
                }

                float mouseWheel = Input.GetAxis("Mouse ScrollWheel");
                if (Mathf.Abs(mouseWheel) > 0.01f)
                {
                    currentRotationAngle += mouseWheel * 90f;
                    if (currentRotationAngle >= 360f)
                        currentRotationAngle -= 360f;
                    if (currentRotationAngle < 0f)
                        currentRotationAngle += 360f;
                }
            }

            // Place with left click
            if (Input.GetMouseButtonDown(0))
            {
                if (isValidPlacement)
                {
                    PlaceBuilding();
                }
                else
                {
                    if (FloatingTextManager.Instance != null)
                    {
                        FloatingTextManager.Instance.Show("Cannot place here!", currentPlacementPosition, Color.red);
                    }
                }
            }

            // Cancel with right click or Escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ExitBuildMode();
            }
        }

        /// <summary>
        /// Place building at current position
        /// </summary>
        private void PlaceBuilding()
        {
            if (selectedBuilding == null)
                return;

            // Check resources
            string missingResource;
            if (!selectedBuilding.HasResources(out missingResource))
            {
                if (FloatingTextManager.Instance != null)
                {
                    FloatingTextManager.Instance.Show($"Need: {missingResource}", currentPlacementPosition, Color.red);
                }
                return;
            }

            // Consume resources
            if (!selectedBuilding.ConsumeResources())
            {
                Debug.LogWarning("[BuildingSystem] Failed to consume resources");
                return;
            }

            // Instantiate building
            GameObject buildingObj = Instantiate(selectedBuilding.basePrefab, currentPlacementPosition, currentPlacementRotation);
            buildingObj.name = selectedBuilding.buildingName;

            // Add PlacedBuilding component
            PlacedBuilding placedBuilding = buildingObj.AddComponent<PlacedBuilding>();
            placedBuilding.buildingData = selectedBuilding;
            placedBuilding.currentTier = 0;
            placedBuilding.Initialize();

            // Track building
            placedBuildings.Add(placedBuilding);

            // Trigger event
            OnBuildingPlaced?.Invoke(placedBuilding);

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.ShowCompletion($"{selectedBuilding.buildingName} placed!", currentPlacementPosition + Vector3.up * 2f);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition("building_place", currentPlacementPosition);
            }

            Debug.Log($"[BuildingSystem] Placed {selectedBuilding.buildingName} at {currentPlacementPosition}");

            // Exit build mode after placing (optional - could stay in mode for multiple placements)
            // ExitBuildMode();
        }

        /// <summary>
        /// Update demolish preview (highlight building under cursor)
        /// </summary>
        private void UpdateDemolishPreview()
        {
            // Raycast to find building
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, placementRaycastDistance))
            {
                PlacedBuilding building = hit.collider.GetComponentInParent<PlacedBuilding>();
                if (building != null)
                {
                    // TODO: Highlight building
                }
            }
        }

        /// <summary>
        /// Handle demolish input
        /// </summary>
        private void HandleDemolishInput()
        {
            // Demolish with left click
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, placementRaycastDistance))
                {
                    PlacedBuilding building = hit.collider.GetComponentInParent<PlacedBuilding>();
                    if (building != null)
                    {
                        DemolishBuilding(building);
                    }
                }
            }

            // Cancel with right click or Escape
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                ExitBuildMode();
            }
        }

        /// <summary>
        /// Demolish building
        /// </summary>
        public void DemolishBuilding(PlacedBuilding building)
        {
            if (building == null || !building.buildingData.canDemolish)
                return;

            // Refund resources
            building.buildingData.RefundResources(building.currentTier);

            // Remove from tracking
            placedBuildings.Remove(building);

            // Trigger event
            OnBuildingDemolished?.Invoke(building);

            // Show notification
            if (FloatingTextManager.Instance != null)
            {
                FloatingTextManager.Instance.Show($"{building.buildingData.buildingName} demolished!", building.transform.position + Vector3.up * 2f, Color.yellow);
            }

            // Play sound
            if (AudioManager.Instance != null)
            {
                AudioManager.Instance.PlaySoundAtPosition("building_demolish", building.transform.position);
            }

            // Destroy building
            Destroy(building.gameObject);

            Debug.Log($"[BuildingSystem] Demolished {building.buildingData.buildingName}");
        }

        /// <summary>
        /// Upgrade building to next tier
        /// </summary>
        public bool UpgradeBuilding(PlacedBuilding building)
        {
            if (building == null || !building.CanUpgrade())
                return false;

            return building.Upgrade();
        }

        /// <summary>
        /// Get all placed buildings
        /// </summary>
        public List<PlacedBuilding> GetPlacedBuildings()
        {
            // Clean up null references
            placedBuildings.RemoveAll(b => b == null);
            return placedBuildings;
        }

        /// <summary>
        /// Get placed buildings by category
        /// </summary>
        public List<PlacedBuilding> GetPlacedBuildingsByCategory(BuildingCategory category)
        {
            return placedBuildings.Where(b => b != null && b.buildingData.category == category).ToList();
        }

        /// <summary>
        /// Create grid visualization
        /// </summary>
        private void CreateGridVisualization()
        {
            gridVisualization = GameObject.CreatePrimitive(PrimitiveType.Quad);
            gridVisualization.name = "PlacementGrid";
            gridVisualization.transform.SetParent(previewParent);
            gridVisualization.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
            gridVisualization.transform.localScale = new Vector3(defaultGridSize, defaultGridSize, 1f);

            // Remove collider
            Destroy(gridVisualization.GetComponent<Collider>());

            // Apply material
            if (gridMaterial != null)
            {
                gridVisualization.GetComponent<Renderer>().material = gridMaterial;
            }

            gridVisualization.SetActive(false);
        }

        /// <summary>
        /// Get save data
        /// </summary>
        public BuildingSaveData GetSaveData()
        {
            BuildingSaveData data = new BuildingSaveData();

            foreach (var building in placedBuildings)
            {
                if (building != null)
                {
                    data.placedBuildings.Add(building.GetSaveData());
                }
            }

            return data;
        }

        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(BuildingSaveData data)
        {
            if (data == null)
                return;

            // Clear existing buildings
            foreach (var building in placedBuildings)
            {
                if (building != null)
                {
                    Destroy(building.gameObject);
                }
            }
            placedBuildings.Clear();

            // Load placed buildings
            foreach (var buildingData in data.placedBuildings)
            {
                // TODO: Instantiate and restore building from save data
                // This requires loading BuildingData by ID
            }

            Debug.Log($"[BuildingSystem] Loaded {data.placedBuildings.Count} buildings");
        }
    }
}
