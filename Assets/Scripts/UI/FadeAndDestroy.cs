using UnityEngine;

public class FadeAndDestroy : MonoBehaviour
{
    public float fadeDuration = 5f;
    public float lifeTime = 25f;

    private float timer = 0f;
    private CanvasGroup canvasGroup;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer > lifeTime)
        {
            float fade = 1f - (timer - lifeTime) / fadeDuration;
            canvasGroup.alpha = Mathf.Clamp01(fade);
            if (fade <= 0f)
                Destroy(gameObject);
        }
    }
} 