using UnityEngine;
using System.Collections.Generic; // Listeler için
using System.Linq; // OrderBy, Take, Select gibi metotlar için
using TMPro; // TextMeshPro kullanmasak bile UIManager bunu kullanýyor olabilir

public class TargetingSystem : MonoBehaviour
{
    [Header("Hedefleme Ayarlarý")]
    [Tooltip("Hedef arama menzili.")]
    [SerializeField] private float targetingRange = 25f;

    [Header("Referanslar")]
    [Tooltip("Seçilen hedefin altýnda görünecek olan gösterge prefab'ý.")]
    [SerializeField] private GameObject groundIndicatorPrefab;
    [Tooltip("Yer göstergesinin, düþman boyutuna göre ne kadar büyük olacaðý.")]
    [SerializeField] private float groundIndicatorSizeMultiplier = 1.2f;

    // Hedef tiplerini yönetmek için bir enum
    public enum TargetTypePriority { Any, Mob, Elite, MiniBoss, Boss }
    private TargetTypePriority currentTargetPriority = TargetTypePriority.Any;

    // Mevcut hedefi dýþarýdan okunabilir yapýyoruz
    public Transform CurrentTarget { get; private set; }

    // Dahili Deðiþkenler
    private PlayerInputActions playerInputActions;
    private List<Transform> allEnemiesInRange = new List<Transform>(); // Menzildeki düþmanlarýn listesi
    private Camera mainCamera; // Ana kamera referansý
    private GameObject currentGroundIndicator; // Aktif yer göstergesi objesi

    private float findEnemiesInterval = 0.5f; // Düþmanlarý ne sýklýkla arayacaðýmýz (saniye)
    private float findEnemiesTimer; // Düþman arama zamanlayýcýsý

    private void Awake()
    {
        // Ana kamerayý bul
        mainCamera = Camera.main;
        if (mainCamera == null) Debug.LogError("TargetingSystem: Ana Kamera bulunamadý!");

        // Input sistemini kur
        playerInputActions = new PlayerInputActions();
        playerInputActions.Player.Enable();
        // Hedef tipi deðiþtirme tuþuna basýldýðýnda CyclePriority fonksiyonunu çaðýr
        playerInputActions.Player.CycleTargetType.performed += context => CyclePriority();

        // Yer göstergesi prefabýnýn atanýp atanmadýðýný kontrol et
        if (groundIndicatorPrefab == null) Debug.LogWarning("TargetingSystem: Ground Indicator Prefab atanmamýþ!");
    }

    private void Start()
    {
        // Baþlangýçta hedef önceliði UI'ýný güncelle
        UpdatePriorityUIInternal();
        // Oyun baþlar baþlamaz düþmanlarý ara
        findEnemiesTimer = 0f;
        Debug.Log("TargetingSystem Baþlatýldý.");
    }

    void Update()
    {
        // Düþmanlarý periyodik olarak ara
        findEnemiesTimer -= Time.deltaTime;
        if (findEnemiesTimer <= 0f)
        {
            FindAllEnemies(); // Menzildeki düþmanlarý bul ve listeyi güncelle
            findEnemiesTimer = findEnemiesInterval; // Zamanlayýcýyý sýfýrla
        }

        // Her frame'de en uygun hedefi bul ve ayarla
        FindAndSetTarget();
        // Her frame'de göstergeleri (sadece yer göstergesi) güncelle
        UpdateIndicators();
    }

    /// <summary>
    /// Hedef önceliðini bir sonrakine geçirir ve UI'ý günceller.
    /// </summary>
    private void CyclePriority()
    {
        // Enum'daki bir sonraki deðere geç (döngüsel olarak)
        int nextPriority = ((int)currentTargetPriority + 1) % System.Enum.GetValues(typeof(TargetTypePriority)).Length;
        currentTargetPriority = (TargetTypePriority)nextPriority;
        Debug.Log($"Hedef Önceliði Deðiþtirildi: {currentTargetPriority}");
        UpdatePriorityUIInternal(); // UI'ý yeni öncelikle güncelle

        // Öncelik deðiþtiðinde hedefi hemen yeniden deðerlendir ve göstergeleri güncelle
        FindAndSetTarget();
        UpdateIndicators();
    }

    /// <summary>
    /// UIManager aracýlýðýyla hedef önceliði metnini günceller.
    /// </summary>
    private void UpdatePriorityUIInternal()
    {
        if (UIManager.Instance != null)
        {
            UIManager.Instance.UpdatePriorityText(currentTargetPriority.ToString());
        }
        else { Debug.LogError("TargetingSystem: UIManager bulunamadý!"); }
    }

    /// <summary>
    /// Menzil içindeki tüm geçerli düþmanlarý bulur ve allEnemiesInRange listesini günceller.
    /// </summary>
    private void FindAllEnemies()
    {
        allEnemiesInRange.Clear(); // Önceki listeyi temizle
        string[] enemyTags = { "Mob", "Elite", "MiniBoss", "Boss" }; // Düþman etiketleriniz

        foreach (string tag in enemyTags)
        {
            GameObject[] enemies = GameObject.FindGameObjectsWithTag(tag);
            foreach (GameObject enemy in enemies)
            {
                // Düþman null deðilse ve aktifse
                if (enemy != null && enemy.activeInHierarchy)
                {
                    float distance = Vector3.Distance(transform.position, enemy.transform.position);
                    // Menzil içindeyse listeye ekle
                    if (distance <= targetingRange)
                    {
                        allEnemiesInRange.Add(enemy.transform);
                    }
                }
            }
        }

        // --- HATA AYIKLAMA LOGU EKLENDÝ ---
        //Debug.Log($"[FindAllEnemies Check] Menzildeki Düþman Listesi ({allEnemiesInRange.Count} adet): {string.Join(", ", allEnemiesInRange.Select(t => t != null ? t.name : "NULL"))}");
        // ------------------------------------
    }

    /// <summary>
    /// Menzildeki düþmanlar arasýndan mevcut önceliðe göre en uygun hedefi bulur ve ayarlar.
    /// Yok edilmiþ objelere karþý güvenli hale getirildi.
    /// </summary>
    private void FindAndSetTarget()
    {
        Transform previousTarget = CurrentTarget; // Mevcut hedefi sakla (deðiþiklik kontrolü için)
        Transform closestPriorityTarget = null; // Önceliðe uyan en yakýn
        Transform closestAnyTarget = null; // Herhangi bir en yakýn
        float minPriorityDistance = float.MaxValue;
        float minAnyDistance = float.MaxValue;

        // Liste kopyasý üzerinde çalýþ ve null'larý temizle (Ekstra güvenlik)
        List<Transform> currentTargetsInLoop = new List<Transform>(allEnemiesInRange);
        currentTargetsInLoop.RemoveAll(item => item == null);

        // Temizlenmiþ kopya üzerinde döngüye gir
        foreach (Transform potentialTarget in currentTargetsInLoop)
        {
            // Maksimum güvenlik kontrolü
            if (potentialTarget == null) continue;
            GameObject targetGO = potentialTarget.gameObject;
            if (targetGO == null || !targetGO.activeInHierarchy) continue;

            // Hesaplamalar...
            float distance = Vector3.Distance(transform.position, potentialTarget.position);

            if (distance < minAnyDistance)
            {
                minAnyDistance = distance;
                closestAnyTarget = potentialTarget;
            }

            bool typeMatch = (currentTargetPriority == TargetTypePriority.Any) || potentialTarget.CompareTag(currentTargetPriority.ToString());

            if (typeMatch && distance < minPriorityDistance)
            {
                minPriorityDistance = distance;
                closestPriorityTarget = potentialTarget;
            }
        } // Döngü sonu

        // Nihai hedefi belirle
        Transform finalTarget = (closestPriorityTarget != null) ? closestPriorityTarget : closestAnyTarget;

        // Hedeflerin isimlerini güvenli al
        string prevTargetName = "Yok";
        if (previousTarget != null && previousTarget.gameObject != null) { try { prevTargetName = previousTarget.name; } catch (MissingReferenceException) { prevTargetName = "Yok Edilmiþ"; } }
        string finalTargetName = "Yok";
        if (finalTarget != null && finalTarget.gameObject != null) { try { finalTargetName = finalTarget.name; } catch (MissingReferenceException) { finalTargetName = "Yok Edilmiþ (Hýzlý)"; } }

        // Sadece hedef deðiþtiyse log at
        if (previousTarget != finalTarget)
        {
           // Debug.Log($"FindAndSetTarget Sonuç: Önceki Hedef='{prevTargetName}', Yeni Hedef='{finalTargetName}' (Öncelik={currentTargetPriority})");
        }

        // Sadece hedef gerçekten deðiþtiyse CurrentTarget'ý güncelle ve göstergeleri yönet
        if (CurrentTarget != finalTarget)
        {
            CurrentTarget = finalTarget;

            // Yer göstergesini yönet
            if (currentGroundIndicator != null)
            {
                Destroy(currentGroundIndicator);
                currentGroundIndicator = null;
            }
            if (CurrentTarget != null && groundIndicatorPrefab != null)
            {
                currentGroundIndicator = Instantiate(groundIndicatorPrefab, CurrentTarget.position, groundIndicatorPrefab.transform.rotation);
                Collider targetCollider = CurrentTarget.GetComponent<Collider>();
                if (targetCollider != null)
                {
                    float enemyFootprintSize = Mathf.Max(targetCollider.bounds.size.x, targetCollider.bounds.size.z);
                    float finalIndicatorSize = enemyFootprintSize * groundIndicatorSizeMultiplier;
                    currentGroundIndicator.transform.localScale = new Vector3(finalIndicatorSize, finalIndicatorSize, 1f);
                }
            }
        }
    }

    /// <summary>
    /// SADECE yer göstergesini günceller. Ekran üstü gösterge kaldýrýldý.
    /// </summary>
    private void UpdateIndicators()
    {
        // Yer göstergesinin pozisyonunu güncelle
        if (currentGroundIndicator != null && CurrentTarget != null)
        {
            Collider targetCollider = CurrentTarget.GetComponent<Collider>();
            Vector3 groundPosition;
            if (targetCollider != null)
            {
                float groundYPosition = targetCollider.bounds.center.y - targetCollider.bounds.extents.y;
                groundPosition = new Vector3(CurrentTarget.position.x, groundYPosition, CurrentTarget.position.z);
            }
            else
            {
                groundPosition = CurrentTarget.position;
                groundPosition.y = transform.position.y; // Collider yoksa oyuncuyla ayný seviyeye koymayý dene
                // Debug.LogWarning($"Hedef '{CurrentTarget.name}' üzerinde Collider bulunamadý, yer göstergesi pozisyonu tahmini yapýlýyor.");
            }
            currentGroundIndicator.transform.position = groundPosition + new Vector3(0, 0.05f, 0);
        }
        else if (currentGroundIndicator != null && CurrentTarget == null)
        {
            Destroy(currentGroundIndicator);
            currentGroundIndicator = null;
        }
    }

    /// <summary>
    /// Script devre dýþý býrakýldýðýnda çalýþýr.
    /// </summary>
    private void OnDisable()
    {
        playerInputActions?.Player.Disable();
        if (currentGroundIndicator != null) Destroy(currentGroundIndicator);
        // UIManager çaðrýsý kaldýrýldý.
        Debug.Log("TargetingSystem Devre Dýþý Býrakýldý.");
    }

    /// <summary>
    /// Inspector'da deðerler deðiþtirildiðinde Editör'de çalýþýr (güvenlik için).
    /// </summary>
    void OnValidate()
    {
        if (targetingRange < 0) targetingRange = 0;
        if (groundIndicatorSizeMultiplier < 0) groundIndicatorSizeMultiplier = 0;
    }
} // SINIF BURADA BÝTER