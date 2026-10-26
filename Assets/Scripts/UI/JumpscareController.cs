using UnityEngine;
using System.Collections;

public class JumpscareController : MonoBehaviour
{
    [SerializeField] private float fadeDuration = 1.0f;

    private CanvasGroup canvasGroup;
    private AudioSource audioSource;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        audioSource = GetComponent<AudioSource>();

        // Ensure initially hidden
        canvasGroup.alpha = 0f;
    }

    public void TriggerJumpscare()
    {
        canvasGroup.alpha = 1f;

        if (audioSource != null)
        {
            audioSource.Play();
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeOutSequence());
    }

    private IEnumerator FadeOutSequence()
    {
        float elapsedTime = 0f;

        while (elapsedTime < fadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime; // Unscaled so it fades even if Time.timeScale = 0
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 0f;
    }
}
