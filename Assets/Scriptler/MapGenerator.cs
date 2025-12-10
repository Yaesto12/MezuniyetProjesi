using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class WorldObjectData
{
    public string name;
    public GameObject prefab;
    public int count; // Ýstediðin kadar sayý yazabilirsin
    [Tooltip("Objeyi zeminden ne kadar yukarý kaldýralým?")]
    public float yOffset = 0f;
}

public class MapGenerator : MonoBehaviour
{
    public static MapGenerator Instance { get; private set; }

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

    void Awake() { if (Instance != null && Instance != this) Destroy(gameObject); else Instance = this; }
    void Start() { StartCoroutine(GenerateMap()); }

    enum MoveType { Flat, RampUpPair, RampDownPair }
    struct GenerationStep { public MoveType Type; public Vector2Int Direction; public Vector3Int CurrentPos; }

    IEnumerator GenerateMap()
    {
        // --- TEMÝZLÝK KISMI ---
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

        // --- HARÝTA OLUÞTURMA DÖNGÜSÜ ---
        Vector3Int startPos = Vector3Int.zero;
        CreateTile(startTile, new Vector2Int(0, 0), 0, Quaternion.identity, false);
        activePoints.Add(startPos);

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
            }
            else
            {
                activePoints.RemoveAt(index);
            }
        }

        if (fillGaps)
        {
            FillMapGaps();
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

        // 1. OYUNCU YERLEÞTÝRME
        if (SpawnablePositions.Count > 0)
        {
            Vector3 randomSpot = SpawnablePositions[Random.Range(0, SpawnablePositions.Count)];
            Vector3 rayOrigin = new Vector3(randomSpot.x, 200f, randomSpot.z);
            RaycastHit hit;

            if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f))
            {
                StartTilePosition = hit.point + Vector3.up * playerSpawnHeight;
                PlayerStats player = FindAnyObjectByType<PlayerStats>();
                if (player != null)
                {
                    CharacterController cc = player.GetComponent<CharacterController>();
                    if (cc != null) cc.enabled = false;
                    player.transform.position = StartTilePosition;
                    if (cc != null) cc.enabled = true;

                    Rigidbody rb = player.GetComponent<Rigidbody>();
                    if (rb != null) { rb.linearVelocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
                }
            }
        }

        // 2. SANDIK VE OBJE YERLEÞTÝRME (SINIRSIZ & RASTGELE)
        SpawnWorldObjects();
    }

    void SpawnWorldObjects()
    {
        if (objectsToSpawn == null || objectsToSpawn.Count == 0) return;
        if (SpawnablePositions.Count == 0) return; // Harita yoksa spawn yok

        // Oyuncunun baþlangýç noktasýný referans al (Üzerine düþmesin diye basit kontrol)
        Vector3 playerPosFlat = new Vector3(StartTilePosition.x, 0, StartTilePosition.z);

        foreach (var objData in objectsToSpawn)
        {
            if (objData.prefab == null) continue;

            // Ýstenen sayý kadar döngü kur (Yer sýnýrý kontrolü yok!)
            for (int i = 0; i < objData.count; i++)
            {
                // Rastgele bir nokta seç (Her seferinde tüm liste içinden)
                int randomIndex = Random.Range(0, SpawnablePositions.Count);
                Vector3 basePos = SpawnablePositions[randomIndex];

                // Eðer oyuncunun çok yakýnýndaysa bu seferlik pas geçelim (veya tekrar deneyebiliriz ama pas geçmek daha performanslý)
                if (Vector3.Distance(new Vector3(basePos.x, 0, basePos.z), playerPosFlat) < tileSize)
                {
                    // Þanssýzlýk, oyuncunun üstüne denk geldi.
                    // Bir hak daha verelim:
                    randomIndex = Random.Range(0, SpawnablePositions.Count);
                    basePos = SpawnablePositions[randomIndex];
                }

                // Rastgele Sapma (Offset) Ekle
                float jitter = tileSize / 3f;
                float offsetX = Random.Range(-jitter, jitter);
                float offsetZ = Random.Range(-jitter, jitter);

                Vector3 rayOrigin = new Vector3(basePos.x + offsetX, 200f, basePos.z + offsetZ);

                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, Vector3.down, out hit, 500f))
                {
                    Vector3 finalPos = hit.point + Vector3.up * objData.yOffset;
                    Quaternion randomRot = Quaternion.Euler(0, Random.Range(0, 360), 0);
                    Instantiate(objData.prefab, finalPos, randomRot, objectsParent);
                }
            }
        }
    }

    void CreateTile(GameObject prefab, Vector2Int coord2D, int height, Quaternion rot, bool isRamp, bool isFoundation = false)
    {
        float yPos = (height * tileSize) + yOffsetCorrection;
        if (isRamp) yPos -= 15f;

        Vector3 worldPos = new Vector3(coord2D.x * tileSize, yPos, coord2D.y * tileSize);
        Instantiate(prefab, worldPos, rot, transform);

        if (!isFoundation && !heightMap.ContainsKey(coord2D))
        {
            heightMap.Add(coord2D, height);
            SpawnablePositions.Add(worldPos);

            if (fillerTile != null)
            {
                float currentY = yPos - tileSize;
                currentY -= foundationStartOffset;
                currentY += 15f;
                if (isRamp) currentY += 15f;

                while (currentY >= foundationMinY)
                {
                    Vector3 fillerPos = new Vector3(worldPos.x, currentY, worldPos.z);
                    Instantiate(fillerTile, fillerPos, Quaternion.identity, transform);
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
}