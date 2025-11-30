using UnityEngine;

public class CharacterAnimation : MonoBehaviour
{
    // Animator bileþenine referans
    private Animator animator;

    void Start()
    {
        // Script'in baðlý olduðu GameObject üzerindeki Animator bileþenini bul ve al
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        // Her karede "W" tuþuna basýlýp basýlmadýðýný kontrol et
        if (Input.GetKey(KeyCode.W))
        {
            // Eðer basýlýyorsa, Animator'daki "isRunning" parametresini 'true' yap
            animator.SetBool("IsRunning", true);
        }
        else
        {
            // Eðer basýlmýyorsa, "isRunning" parametresini 'false' yap
            animator.SetBool("IsRunning", false);
        }
    }
}