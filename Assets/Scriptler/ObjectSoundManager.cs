using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ObjectSoundManager : MonoBehaviour
{
    [Header("Ses Dosyasý")]
    public AudioClip soundEffect;

    [Header("Ayarlar")]
    [Range(0f, 1f)]
    public float volume = 0.7f;

    [Tooltip("Pitch (Perde) rastgele olsun mu? (Daha doðal duyulur)")]
    public bool randomizePitch = true;
    public Vector2 pitchRange = new Vector2(0.85f, 1.15f);

    [Header("Otomatik Tetikleyiciler")]
    [Tooltip("Baþka bir objeye çarptýðýnda ses çalsýn mý?")]
    public bool playOnCollision = false;
    [Tooltip("Trigger alanýna girildiðinde ses çalsýn mý?")]
    public bool playOnTrigger = false;

    private AudioSource source;

    void Awake()
    {
        source = GetComponent<AudioSource>();
        source.playOnAwake = false; // Baþlar baþlamaz çalmasýn
        source.spatialBlend = 0.5f; // Yarý 2D yarý 3D (yaklaþýnca sesi artar)
    }

    // --- DIÞARIDAN ÇAÐIRMAK ÝÇÝN FONKSÝYON ---
    public void PlaySound()
    {
        if (soundEffect == null) return;

        // Rastgele tonlama yap (Robotik duyulmamasý için)
        if (randomizePitch)
        {
            source.pitch = Random.Range(pitchRange.x, pitchRange.y);
        }
        else
        {
            source.pitch = 1f;
        }

        // PlayOneShot: Üst üste çalmaya izin verir
        source.PlayOneShot(soundEffect, volume);
    }

    // --- FÝZÝKSEL ÇARPIÞMALAR ÝÇÝN ---
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (playOnCollision)
        {
            // Ýstersen burada "Sadece yere çarpýnca çal" gibi filtreler ekleyebilirsin
            PlaySound();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (playOnTrigger)
        {
            // Örn: Mermi karaktere deðdiðinde
            PlaySound();
        }
    }
}