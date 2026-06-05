using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImageSequencer : MonoBehaviour
{
    [System.Serializable]
    public class ImageEntry
    {
        public Sprite sprite;
        public float holdDuration = 2f;
    }

    [Tooltip("The single Image used to display all entries.")]
    public Image display;

    public List<ImageEntry> entries = new();

    [Tooltip("Seconds to fade in / fade out each entry.")]
    public float fadeDuration = 0.5f;

    void Start() => StartCoroutine(RunSequence());

    IEnumerator RunSequence()
    {
        SetAlpha(0f);

        foreach (ImageEntry entry in entries)
        {
            display.sprite = entry.sprite;

            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(entry.holdDuration);
            yield return Fade(1f, 0f);
        }
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            SetAlpha(Mathf.Lerp(from, to, elapsed / fadeDuration));
            yield return null;
        }

        SetAlpha(to);
    }

    void SetAlpha(float a)
    {
        Color c = display.color;
        c.a = a;
        display.color = c;
    }
}