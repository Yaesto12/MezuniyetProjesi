using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UI;

[System.Serializable]
public class WorldObjectData
{
    public string name;
    public GameObject prefab;
    public int count;
    public float yOffset = 0f;
    public float minScale = 1f;
    public float maxScale = 1f;
}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

    [Header("Layer Ayarlarý (YENÝ)")]
    [Tooltip("Duvarlarýn ve zeminin atanacaðý Layer adý")]
    [SerializeField] private string mapLayerName = "MapEnvironment";

    [Header("Harita Ayarlarý")]
    [SerializeField] private float tileSize = 30f;
    [SerializeField] private float yOffsetCorrection = 0f;
    [SerializeField] private int targetTileCount = 100;
    [SerializeField] private int mapRadius = 8;
    [SerializeField] private float generationSpeed = 0.05f;

    [Header("Oyuncu Ayarlarý")]
    [SerializeField] private float playerSpawnHeight = 2f;

    [Header("Yükseklik Ayarlarý")]
    [SerializeField] private int maxHeight = 2;
    [SerializeField] private int minHeight = -2;
    [Range(0f, 1f)][SerializeField] private float rampChance = 0.3f;

    [Header("Doldurma Ayarlarý")]
    [SerializeField] private bool fillGaps = true;
    [Range(1, 4)][SerializeField] private int minNeighborsToFill = 3;
    [SerializeField] private float foundationMinY = -90f;
    [SerializeField] private float foundationStartOffset = 15f;

    [Header("Duvar Ayarlarý")]
    [SerializeField] private bool createWalls = true;
    [SerializeField] private int wallHeightAboveStart = 3;
    [SerializeField] private GameObject wallPrefab;

    [Header("Yükleme Ekraný Ayarlarý")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private Camera loadingCamera;

    [Header("Özel Objeler")]
    [SerializeField] private GameObject dragonSummonerPrefab;

    [Header("Prefablar")]
    [SerializeField] private GameObject startTile;
    [SerializeField] private GameObject groundTile;
    [SerializeField] private GameObject rampUpPrefab;
    [SerializeField] private GameObject rampDownPrefab;
    [SerializeField] private GameObject fillerTile;

    [Header("Obje Daðýtýmý")]
    [SerializeField] private List<WorldObjectData> objectsToSpawn;
    [SerializeField] private Transform objectsParent;

    public List<Vector3> SpawnablePositions { get; private set; } = new List<Vector3>();
    public Vector3 StartTilePosition { get; private set; }
    public bool IsMapGenerated { get; private set; } = false;

    private Dictionary<Vector2Int, int> heightMap = new Dictionary<Vector2Int, int>();
    private List<Vector3Int> activePoints = new List<Vector3Int>();
    private int mapLayerIndex; // Layer numarasýný tutacak

    void Awake() { if (Instance != null && Instance != this) Destroy(gameObject); else Instance = this; }

    void Start()
    {
        // Layer numarasýný bul (Ýsimden ID'ye çevir)
        mapLayerIndex = LayerMask.NameToLayer(mapLayerName);
        if (mapLayerIndex == -1)
        {
            Debug.LogError($"HATA: '{mapLayerName}' adýnda bir Layer bulunamadý! Lütfen Unity'de Edit -> Layers kýsmýndan bu layerý oluþtur.");
            // Hata çýkmasýn diye Default layer'a atayalým
            mapLayerIndex = 0;
        }

        // Loading Kamerasý iþlemleri
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
        if (loadingCamera != null)
        {
            loadingCamera.gameObject.SetActive(true);
            var lListener = loadingCamera.GetComponent<AudioListener>();
            if (lListener != null) lListener.enabled = true;
        }

        AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
        foreach (var listener in allListeners)
        {
            if (loadingCamera != null && listener.gameObject != loadingCamera.gameObject)
            {
                listener.enabled = false;
            }
        }

        StartCoroutine(GenerateMap());
    }

    enum MoveType { Flat, RampUpPair, RampDownPair }
    struct GenerationStep { public MoveType Type; public Vector2Int Direction; public Vector3Int CurrentPos; }

    IEnumerator GenerateMap()
    {
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(true);
        if (loadingCamera != null) loadingCamera.gameObject.SetActive(true);

        heightMap.Clear();
        activePoints.Clear();
        SpawnablePositions.Clear();
        IsMapGenerated = false;

        if (objectsParent != null)
        {
            for (int i = objectsParent.childCount - 1; i >= 0; i--) Destroy(objectsParent.GetChild(i).gameObject);
        }
        else
        {
            GameObject parentObj = new GameObject("SpawnedObjects");
            parentObj.transform.SetParent(this.transform);
            objectsParent = parentObj.transform;
        }

        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child == objectsParent) continue;
            Destroy(child.gameObject);
        }

        yield return null;

        CreateTile(startTile, new Vector2Int(0, 0), 0, Quaternion.identity, false);
        activePoints.Add(Vector3Int.zero);

        int tilesCreated = 1;
        int attempts = 0;

        while (tilesCreated < targetTileCount && activePoints.Count > 0)
        {
            attempts++;
            if (attempts > 10000) break;

            int index = Random.Range(0, activePoints.Count);
            Vector3Int current = activePoints[index];
            List<GenerationStep> validSteps = GetValidSteps(current);

            if (validSteps.Count > 0)
            {
                GenerationStep step = validSteps[Random.Range(0, validSteps.Count)];
                Vector3Int nextActivePoint = current;
                Vector3 direction3D = new Vector3(step.Direction.x, 0, step.Direction.y);
                Quaternion lookRot = (direction3D != Vector3.zero) ? Quaternion.LookRotation(direction3D) : Quaternion.identity;

                if (step.Type == MoveType.Flat)
                {
                    Vector3Int pos = current + new Vector3Int(step.Direction.x, 0, step.Direction.y);
                    CreateTile(groundTile, new Vector2Int(pos.x, pos.z), pos.y, Quaternion.identity, false);
                    nextActivePoint = pos;
                    tilesCreated++;
                }
                else if (step.Type == MoveType.RampUpPair)
                {
                    Vector3Int rampPos = current + new Vector3Int(step.Direction.x, 0, step.Direction.y);
                    Quaternion upRot = lookRot * Quaternion.Euler(0, 180, 0);
                    CreateTile(rampUpPrefab, new Vector2Int(rampPos.x, rampPos.z), rampPos.y + 1, upRot, true);

                    Vector3Int flatPos = current + new Vector3Int(step.Direction.x * 2, 1, step.Direction.y * 2);
                    CreateTile(groundTile, new Vector2Int(flatPos.x, flatPos.z), flatPos.y, Quaternion.identity, false);
                    nextActivePoint = flatPos;
                    tilesCreated += 2;
                }
                else if (step.Type == MoveType.RampDownPair)
                {
                    Vector3Int rampPos = current + new Vector3Int(step.Direction.x, -1, step.Direction.y);
                    CreateTile(rampDownPrefab, new Vector2Int(rampPos.x, rampPos.z), rampPos.y + 1, lookRot, true);

                    Vector3Int flatPos = current + new Vector3Int(step.Direction.x * 2, -1, step.Direction.y * 2);
                    CreateTile(groundTile, new Vector2Int(flatPos.x, flatPos.z), flatPos.y, Quaternion.identity, false);
                    nextActivePoint = flatPos;
                    tilesCreated += 2;
                }

                activePoints.Add(nextActivePoint);
                if (generationSpeed > 0) yield return new WaitForSeconds(generationSpeed);

                if (loadingSlider != null)
                {
                    float progress = (float)tilesCreated / targetTileCount * 0.8f;
                    loadingSlider.value = progress;
                }
            }
            else
            {
                activePoints.RemoveAt(index);
            }
        }

        if (fillGaps)
        {
            FillMapGaps();
            if (loadingSlider != null) loadingSlider.value = 0.85f;
            yield return new WaitForSeconds(0.1f);
        }

        if (createWalls)
        {
            GenerateMapWalls();
            if (loadingSlider != null) loadingSlider.value = 0.90f;
            yield return new WaitForSeconds(0.1f);
        }

        StartCoroutine(TeleportPlayerAndSpawnObjects());
        Debug.Log($"Harita Tamamlandý. Parça Sayýsý: {heightMap.Count}");
        IsMapGenerated = true;
    }

    IEnumerator TeleportPlayerAndSpawnObjects()
    {
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        GameObject player = GameObject.FindGameObjectWithTag("Player");

        float waitTimer = 0f;
        while (player == null && waitTimer < 5f)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            waitTimer += Time.deltaTime;
            yield return null;
        }

        // --- LAYER MASK OLUÞTURUYORUZ ---
        // Sadece harita layerýna (ör: Layer 6) çarpmasý için bit mask oluþturuyoruz.
        int layerMask = 1 << mapLayerIndex;

        if (player != null)
        {
            if (SpawnablePositions.Count > 0)
            {
                Vector3 randomSpot = SpawnablePositions[Random.Range(0, SpawnablePositions.Count)];
                Vector3 rayOrigin = new Vector3(randomSpot.x, 200f, randomSpot.z);
                RaycastHit hit;

                // DÜZELTME: Raycast sadece 'layerMask' (MapEnvironment) ile çarpýþacak.
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f, layerMask))
                {
                    StartTilePosition = hit.point + Vector3.up * playerSpawnHeight;

                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    player.transform.position = StartTilePosition;
                    if (cc != null) cc.enabled = true;

                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }

                    AudioListener[] allListeners = FindObjectsByType<AudioListener>(FindObjectsSortMode.None);
                    foreach (var listener in allListeners) listener.enabled = false;

                    Camera playerCam = player.GetComponentInChildren<Camera>();
                    if (playerCam == null) playerCam = Camera.main;

                    if (playerCam != null)
                    {
                        playerCam.gameObject.SetActive(true);
                        var playerListener = playerCam.GetComponent<AudioListener>();
                        if (playerListener != null) playerListener.enabled = true;
                        else playerCam.gameObject.AddComponent<AudioListener>();
                    }
                }
            }
        }
        else
        {
            Debug.LogError("HATA: Player bulunamadý!");
        }

        SpawnDragonSummoner(layerMask); // LayerMask gönderiyoruz
        SpawnWorldObjects(layerMask);   // LayerMask gönderiyoruz

        if (loadingSlider != null) loadingSlider.value = 1f;
        yield return new WaitForSeconds(0.5f);

        if (loadingCamera != null) loadingCamera.gameObject.SetActive(false);
        if (loadingScreenPanel != null) loadingScreenPanel.SetActive(false);
        IsMapGenerated = true;
    }

    // --- ÖZEL OBJELERDE LAYER MASK EKLENDÝ ---
    void SpawnDragonSummoner(int layerMask)
    {
        if (dragonSummonerPrefab == null) return;
        if (SpawnablePositions.Count == 0) return;
        int randomIndex = Random.Range(0, SpawnablePositions.Count);
        Vector3 spawnPos = SpawnablePositions[randomIndex];

        // Burada da zemine tam oturtmak için Raycast atabiliriz
        Vector3 rayOrigin = new Vector3(spawnPos.x, 200f, spawnPos.z);
        RaycastHit hit;

        // Varsayýlan pozisyon (Eðer raycast baþarýsýz olursa)
        Vector3 finalPos = spawnPos;

        // DÜZELTME: Sadece zemine çarp.
        if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f, layerMask))
        {
            finalPos = hit.point;
        }

        Instantiate(dragonSummonerPrefab, finalPos, Quaternion.identity, objectsParent);
        SpawnablePositions.RemoveAt(randomIndex);
    }

    void SpawnWorldObjects(int layerMask)
    {
        if (objectsToSpawn == null || objectsToSpawn.Count == 0) return;
        if (SpawnablePositions.Count == 0) return;
        Vector3 playerPosFlat = new Vector3(StartTilePosition.x, 0, StartTilePosition.z);

        foreach (var objData in objectsToSpawn)
        {
            if (objData.prefab == null) continue;
            for (int i = 0; i < objData.count; i++)
            {
                if (SpawnablePositions.Count == 0) break;
                int randomIndex = Random.Range(0, SpawnablePositions.Count);
                Vector3 basePos = SpawnablePositions[randomIndex];
                if (Vector3.Distance(new Vector3(basePos.x, 0, basePos.z), playerPosFlat) < tileSize)
                {
                    randomIndex = Random.Range(0, SpawnablePositions.Count);
                    basePos = SpawnablePositions[randomIndex];
                }
                float jitter = tileSize / 3f;
                float offsetX = Random.Range(-jitter, jitter);
                float offsetZ = Random.Range(-jitter, jitter);
                Vector3 rayOrigin = new Vector3(basePos.x + offsetX, 200f, basePos.z + offsetZ);
                RaycastHit hit;

                // DÜZELTME: Sadece 'MapEnvironment' layerýna sahip objelere çarp.
                // Oyuncuya veya diðer objelere çarpma.
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f, layerMask))
                {
                    Vector3 finalPos = hit.point + Vector3.up * objData.yOffset;
                    Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    GameObject newObj = Instantiate(objData.prefab, finalPos, randomRot, objectsParent);
                    float randomScale = Random.Range(objData.minScale, objData.maxScale);
                    newObj.transform.localScale = Vector3.one * randomScale;
                }
            }
        }
    }

    // --- TILE (Zemin) OLUÞTURMA ---
    void CreateTile(GameObject prefab, Vector2Int coord2D, int height, Quaternion rot, bool isRamp, bool isFoundation = false)
    {
        float yPos = (height * tileSize) + yOffsetCorrection;
        if (isRamp) yPos -= 15f;
        Vector3 worldPos = new Vector3(coord2D.x * tileSize, yPos, coord2D.y * tileSize);

        GameObject newObj = Instantiate(prefab, worldPos, rot, transform);

        // Layer atamasý (Daha önce yaptýðýnýz gibi, doðru çalýþýyor)
        newObj.layer = mapLayerIndex;
        foreach (Transform child in newObj.transform) child.gameObject.layer = mapLayerIndex;

        if (!isFoundation && !heightMap.ContainsKey(coord2D))
        {
            heightMap.Add(coord2D, height);
            if (!isRamp) SpawnablePositions.Add(worldPos);
            if (fillerTile != null)
            {
                float currentY = yPos - tileSize;
                currentY -= foundationStartOffset;
                currentY += 15f;
                if (isRamp) currentY += 15f;
                while (currentY >= foundationMinY)
                {
                    Vector3 fillerPos = new Vector3(worldPos.x, currentY, worldPos.z);
                    GameObject filler = Instantiate(fillerTile, fillerPos, Quaternion.identity, transform);

                    filler.layer = mapLayerIndex;
                    foreach (Transform child in filler.transform) child.gameObject.layer = mapLayerIndex;

                    currentY -= tileSize;
                }
            }
        }
    }

    int FillMapGaps()
    {
        if (heightMap.Count == 0) return 0;
        int minX = int.MaxValue, maxX = int.MinValue;
        int minZ = int.MaxValue, maxZ = int.MinValue;
        foreach (var key in heightMap.Keys) { if (key.x < minX) minX = key.x; if (key.x > maxX) maxX = key.x; if (key.y < minZ) minZ = key.y; if (key.y > maxZ) maxZ = key.y; }
        List<GenerationStep> gapsToFill = new List<GenerationStep>();
        for (int x = minX; x <= maxX; x++)
        {
            for (int z = minZ; z <= maxZ; z++)
            {
                Vector2Int currentCoord = new Vector2Int(x, z);
                if (!heightMap.ContainsKey(currentCoord))
                {
                    int neighborCount = 0;
                    List<int> neighborHeights = new List<int>();
                    Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
                    foreach (var dir in dirs) { Vector2Int neighborCoord = currentCoord + dir; if (heightMap.ContainsKey(neighborCoord)) { neighborCount++; neighborHeights.Add(heightMap[neighborCoord]); } }
                    if (neighborCount >= minNeighborsToFill)
                    {
                        var heightGroups = neighborHeights.GroupBy(h => h).OrderByDescending(g => g.Count());
                        int targetHeight = heightGroups.First().Key;
                        gapsToFill.Add(new GenerationStep { Type = MoveType.Flat, Direction = Vector2Int.zero, CurrentPos = new Vector3Int(x, targetHeight, z) });
                    }
                }
            }
        }
        foreach (var gap in gapsToFill)
        {
            Vector2Int coord = new Vector2Int(gap.CurrentPos.x, gap.CurrentPos.z);
            if (!heightMap.ContainsKey(coord)) CreateTile(groundTile, coord, gap.CurrentPos.y, Quaternion.identity, false);
        }
        return gapsToFill.Count;
    }

    public Vector3 GetRandomSpawnPosition() { if (SpawnablePositions.Count == 0) return StartTilePosition; return SpawnablePositions[Random.Range(0, SpawnablePositions.Count)]; }

    List<GenerationStep> GetValidSteps(Vector3Int current)
    {
        List<GenerationStep> steps = new List<GenerationStep>();
        Vector2Int[] dirs = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var dir in dirs)
        {
            Vector3Int target1 = current + new Vector3Int(dir.x, 0, dir.y);
            Vector2Int t1_2D = new Vector2Int(target1.x, target1.z);
            bool canGoFlat = !IsOccupied(t1_2D) && IsInBounds(t1_2D);
            if (canGoFlat) steps.Add(new GenerationStep { Type = MoveType.Flat, Direction = dir, CurrentPos = current });
            if (Random.value < rampChance)
            {
                Vector3Int target2 = current + new Vector3Int(dir.x * 2, 0, dir.y * 2);
                Vector2Int t2_2D = new Vector2Int(target2.x, target2.z);
                bool canPlacePair = canGoFlat && !IsOccupied(t2_2D) && IsInBounds(t2_2D);
                if (canPlacePair)
                {
                    if (current.y < maxHeight) steps.Add(new GenerationStep { Type = MoveType.RampUpPair, Direction = dir, CurrentPos = current });
                    if (current.y > minHeight) steps.Add(new GenerationStep { Type = MoveType.RampDownPair, Direction = dir, CurrentPos = current });
                }
            }
        }
        return steps;
    }

    bool IsOccupied(Vector2Int coord) { return heightMap.ContainsKey(coord); }
    bool IsInBounds(Vector2Int coord) { return Mathf.Abs(coord.x) <= mapRadius && Mathf.Abs(coord.y) <= mapRadius; }

    void GenerateMapWalls()
    {
        HashSet<Vector2Int> wallCoordinates = new HashSet<Vector2Int>();
        Vector2Int[] directions = { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right };
        foreach (var kvp in heightMap)
        {
            Vector2Int currentCoord = kvp.Key;
            foreach (var dir in directions)
            {
                Vector2Int neighborCoord = currentCoord + dir;
                if (!heightMap.ContainsKey(neighborCoord)) wallCoordinates.Add(neighborCoord);
            }
        }
        GameObject prefabToUse = wallPrefab != null ? wallPrefab : fillerTile;
        if (prefabToUse == null) return;
        foreach (var coord in wallCoordinates)
        {
            float topY = (wallHeightAboveStart * tileSize) + yOffsetCorrection;
            float bottomY = foundationMinY;
            float currentY = topY;
            while (currentY >= bottomY)
            {
                Vector3 wallPos = new Vector3(coord.x * tileSize, currentY, coord.y * tileSize);

                GameObject wall = Instantiate(prefabToUse, wallPos, Quaternion.identity, transform);

                wall.layer = mapLayerIndex;
                foreach (Transform child in wall.transform) child.gameObject.layer = mapLayerIndex;

                currentY -= tileSize;
            }
        }
    }
}