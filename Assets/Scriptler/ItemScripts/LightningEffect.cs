using UnityEngine;
using System.Collections;

[RequireComponent(typeof(LineRenderer))]
public class LightningEffect : MonoBehaviour
{
    private LineRenderer lineRenderer;
    [SerializeField] private float duration = 0.2f; // Efektin ekranda kalma süresi
    [SerializeField] private float textureScrollSpeed = 10f; // Elektrik animasyonu hýzý

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0; // Baþlangýçta görünmesin
    }

    public void Zap(Vector3 startPos, Vector3 endPos)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPosition(0, startPos);
        lineRenderer.SetPosition(1, endPos);

        StartCoroutine(FadeOut());
    }

    private IEnumerator FadeOut()
    {
        float timer = 0f;
        while (timer < duration)
        {
            timer += Time.deltaTime;
            // Basit bir titreme/animasyon etkisi için texture offset'i kaydýrýlabilir
            // (Materyalin buna uygun olmasý gerekir)
            // float offset = Time.time * textureScrollSpeed;
            // lineRenderer.material.mainTextureOffset = new Vector2(offset, 0);

            // Ýsteðe baðlý: Geniþliði zamanla azalt
            float width = Mathf.Lerp(0.5f, 0f, timer / duration);
            lineRenderer.widthMultiplier = width;

            yield return null;
        }
        Destroy(gameObject);
    }
}