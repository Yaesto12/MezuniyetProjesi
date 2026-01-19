using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(AudioSource))]
public class PlayerFootsteps : MonoBehaviour
{
    [Header("Ses Ayarlar�")]
    [Tooltip("Y�r�rken �alacak ses listesi (Birden fazla koyarsan rastgele se�er)")]
    public List<AudioClip> footstepSounds;

    [Range(0f, 1f)]
    public float volume = 0.5f;

    [Tooltip("Ne s�kl�kla ses ��ks�n? (Saniye cinsinden)")]
    public float stepInterval = 0.35f; // Ko�uyorsan bu say�y� k���lt (0.25f gibi)

    [Header("Pitch (Ton) Ayar�")]
    public bool randomizePitch = true;
    [Range(0.8f, 1.2f)] public float minPitch = 0.9f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.1f;

    private AudioSource audioSource;
    private Rigidbody2D rb2d; // E�er 2D fizik kullan�yorsan
    private Rigidbody rb;     // E�er 3D fizik kullan�yorsan
    private float stepTimer;

    void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        audioSource.playOnAwake = false;
        audioSource.spatialBlend = 0f; // Karakter sesi oldu�u i�in 2D (Kula��n�n dibinde) olmal�

        // Karakterinde hangi Rigidbody varsa onu almay� dene
        rb2d = GetComponent<Rigidbody2D>();
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        if (IsMoving())
        {
            stepTimer -= Time.deltaTime;

            if (stepTimer <= 0f)
            {
                PlayFootstep();
                stepTimer = stepInterval; // Sayac� s�f�rla
            }
        }
        else
        {
            // Durdu�unda sayac� s�f�rla ki tekrar y�r�meye ba�lar ba�lamaz ses ��ks�n
            stepTimer = 0f;
        }
    }

    bool IsMoving()
    {
        // 1. �nce Rigidbody2D kontrol� (Unity 6 ve �ncesi uyumlu)
        if (rb2d != null)
        {
            // Unity 6 kullan�yorsan 'velocity' yerine 'linearVelocity' kullanman gerekebilir.
            // �imdilik standart velocity kontrol� yap�yoruz:
            return rb2d.linearVelocity.magnitude > 0.1f;
        }

        // 2. Rigidbody (3D) kontrol�
        if (rb != null)
        {
            // Unity 6 i�in linearVelocity, eskiler i�in velocity
            return rb.linearVelocity.magnitude > 0.1f;
        }

        // 3. Hi�biri yoksa Input kontrol� (Yedek plan)
        return Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0;
    }

    void PlayFootstep()
    {
        if (footstepSounds.Count == 0) return;

        // Listeden rastgele bir ses se�
        int randomIndex = Random.Range(0, footstepSounds.Count);
        AudioClip clip = footstepSounds[randomIndex];

        // Pitch (Ton) rastgelele�tirme
        if (randomizePitch)
        {
            audioSource.pitch = Random.Range(minPitch, maxPitch);
        }
        else
        {
            audioSource.pitch = 1f;
        }

        // Sesi bir kez �al
        audioSource.PlayOneShot(clip, volume);
    }
}