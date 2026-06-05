using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;        // swap for UnityEngine.UI if using legacy Text
using UnityEngine.SceneManagement;

public class TextSequencer : MonoBehaviour
{
    [System.Serializable]
    public class TextEntry
    {
        public string text;
        public float holdDuration = 2f;
    }

    [Tooltip("The single TMP label used to display all entries.")]
    public TMP_Text label;          // drag your Text (TMP) component here

    public List<TextEntry> entries = new();

    [Tooltip("Seconds to fade in / fade out each entry.")]
    public float fadeDuration = 0.5f;
    public string nextScene;

    void Start() => StartCoroutine(RunSequence());

    IEnumerator RunSequence()
    {
        label.alpha = 0f;

        foreach (TextEntry entry in entries)
        {
            label.text = entry.text;

            yield return Fade(0f, 1f);
            yield return new WaitForSeconds(entry.holdDuration);
            yield return Fade(1f, 0f);
        }
        SceneManager.LoadScene(nextScene);
    }

    IEnumerator Fade(float from, float to)
    {
        float elapsed = 0f;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            label.alpha = Mathf.Lerp(from, to, elapsed / fadeDuration);
            yield return null;
        }

        label.alpha = to;
    }
}