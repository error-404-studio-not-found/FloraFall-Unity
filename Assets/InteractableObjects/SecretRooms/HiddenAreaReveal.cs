using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Tilemaps;


public class HiddenAreaReveal : MonoBehaviour
{
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private Tilemap hiddenTileMap;
    [SerializeField] private int zoomedInPPU = 40;
    private Vignette vignette;
    [SerializeField] private float vignetteIntensity;
    private PixelPerfectCamera ppc;
    [SerializeField] Volume areaVolume;

    private int startPPU;
    private float startIntensity;
    private bool entered = false;
    private float startOrthoSize;
    [SerializeField] private float zoomedInOrthoSize = 3.5f;


    private void Awake()
    {
        Volume volume = Camera.main.GetComponent<Volume>();
        ppc = Camera.main.GetComponent<PixelPerfectCamera>();
        startPPU = ppc.assetsPPU;
        startOrthoSize = Camera.main.orthographicSize;
        if (areaVolume.profile.TryGet(out vignette))
        {
            startIntensity = vignette.intensity.value;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!entered)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                entered = true;
                Debug.Log("Entered");
                StartCoroutine(fade(0f, vignetteIntensity, zoomedInPPU, zoomedInOrthoSize));
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (entered)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                entered = false;
                StartCoroutine(fade(1f, startIntensity, startPPU, startOrthoSize));
                vignette.intensity.value = startIntensity;
            }
        }
    }

    private IEnumerator fade(float alpha, float endIntensity, int endPPU, float endOrtho)
    {
        ppc.assetsPPU = ppc.assetsPPU;
        ppc.enabled = false;
        Color startColor = hiddenTileMap.color;
        float startAlpha = startColor.a;
        float intensity = vignette.intensity.value;
        int startPPU = ppc.assetsPPU;
        float startOrtho = Camera.main.orthographicSize;
        float t = 0f;

        while (t < fadeTime)
        {
            if (hiddenTileMap == null) break;
            t += Time.deltaTime;
            float k = t / fadeTime;
            k = 1f - Mathf.Cos(k * Mathf.PI * 0.5f);
            float a = Mathf.Lerp(startAlpha, alpha, k);
            float i = Mathf.Lerp(intensity, endIntensity, k);
            Camera.main.orthographicSize = Mathf.Lerp(startOrtho, endOrtho, k);
            yield return null;
            vignette.intensity.value = i;
            hiddenTileMap.color = new Color(startColor.r, startColor.g, startColor.b, a);
        }
        hiddenTileMap.color = new Color(startColor.r,startColor.g,startColor.b, alpha);
        ppc.assetsPPU = endPPU;
        ppc.enabled = true;
    }
}
