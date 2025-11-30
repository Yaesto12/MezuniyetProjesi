using UnityEngine;

// Bu script Player altýndaki Hurtbox objesinde olmalý
[RequireComponent(typeof(Collider))] // Trigger olmalý
public class PlayerHurtbox : MonoBehaviour
{
    // Gerekli sistemlere referanslar
    private PlayerController playerController;
    private PlayerExperience playerExperience;
    private PlayerWallet playerWallet;

    void Awake()
    {
        // Üst objedeki (Player) scriptleri bul
        playerController = GetComponentInParent<PlayerController>();
        playerExperience = GetComponentInParent<PlayerExperience>();
        playerWallet = GetComponentInParent<PlayerWallet>();

        // Baþlangýçta null kontrolü yap
        if (playerController == null)
            Debug.LogError("Hurtbox: PlayerController bulunamadý!", gameObject);
        if (playerExperience == null)
            Debug.LogError("Hurtbox: PlayerExperience bulunamadý!", gameObject);
        if (playerWallet == null)
            Debug.LogError("Hurtbox: PlayerWallet bulunamadý!", gameObject);
    }

    /// <summary>
    /// Bu trigger'a bir þey girdiðinde çaðrýlýr.
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // --- Sadece Ýlgilendiðimiz Þeyleri Kontrol Et ---

        // 1. Düþman Saldýrýsý mý? (Savrulma/Hasar)
        // (Layer karþýlaþtýrmasý en hýzlýsýdýr)
        if (other.gameObject.layer == LayerMask.NameToLayer("EnemyHitbox"))
        {
            EnemyAI enemy = other.GetComponentInParent<EnemyAI>();
            if (enemy != null && playerController != null)
            {
                playerController.ProcessHitByEnemy(enemy);
            }
            return; // Ýþlem tamamlandý, fonksiyondan çýk
        }

        // 2. XP Orb'u mu?
        // (TryGetComponent hýzlý bir kontrol saðlar)
        if (other.TryGetComponent(out XpOrb orb))
        {
            if (playerExperience != null)
            {
                playerExperience.GainXp(orb.GetValue());
            }
            Destroy(orb.gameObject); // Topla ve yok et
            return; // Ýþlem tamamlandý
        }

        // 3. Altýn Coin mi?
        if (other.TryGetComponent(out GoldCoin coin))
        {
            if (playerWallet != null)
            {
                playerWallet.AddGold(1);
            }
            Destroy(coin.gameObject); // Topla ve yok et
            return; // Ýþlem tamamlandý
        }

        // 4. Sandýk mý?
        if (other.TryGetComponent(out Chest chest))
        {
            // Chest script'i zaten kendi OnTriggerEnter'ýnda PlayerHurtbox arýyor.
            // Bu yüzden buraya tekrar mantýk eklemek çift tetiklemeye neden olabilir.
            // Chest'in kendi mantýðýnýn çalýþmasýna izin verelim.
            // (Ýsterseniz log ekleyebilirsiniz: Debug.Log("Sandýða dokunuldu."))
            return;
        }

        // Eðer yukarýdakilerin hiçbiri deðilse, bu bizi ilgilendirmeyen bir çarpýþmadýr.
        // (Zemin, XpMagnet, Düþman Mermisi vb.)
        // Artýk log basmýyoruz.
        // Debug.Log("Tespit edilen obje bilinen türlerden deðil."); // Bu satýrý kaldýrdýk.
    }
}