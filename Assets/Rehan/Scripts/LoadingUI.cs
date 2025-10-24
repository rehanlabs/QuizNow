using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class LoadingUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject loadingPanel;   // visual panel (child) — enable/disable this
    public Slider progressBar;
    public TextMeshProUGUI statusText;

    [Header("Simulation settings")]
    public float fakeFillRate = 0.25f;     // how fast fake progress moves (units/sec)
    public float maxFakeProgress = 0.95f;  // fake progress caps here until we finish

    private Coroutine fakeCoroutine;
    private Coroutine finishCoroutine;

    void Awake()
    {
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (progressBar != null) progressBar.value = 0f;
    }

    /// <summary>Show the loading visuals and start simulated progress.</summary>
    public void Show(string message = "Loading...")
    {
        if (loadingPanel == null) return;

        // Ensure any existing coroutines are stopped
        if (fakeCoroutine != null) StopCoroutine(fakeCoroutine);
        if (finishCoroutine != null) StopCoroutine(finishCoroutine);

        loadingPanel.SetActive(true);
        if (statusText != null) statusText.text = message;
        if (progressBar != null) progressBar.value = 0f;

        fakeCoroutine = StartCoroutine(FakeProgress());
    }

    /// <summary>Simulated progress: gradually fills up to maxFakeProgress.</summary>
    private IEnumerator FakeProgress()
    {
        while (progressBar != null && progressBar.value < maxFakeProgress)
        {
            progressBar.value = Mathf.MoveTowards(progressBar.value, maxFakeProgress, fakeFillRate * Time.deltaTime);
            yield return null;
        }
        fakeCoroutine = null;
    }

    /// <summary>Externally set the normalized progress (0..1). Stops simulated progress.</summary>
    public void SetProgress(float normalized)
    {
        if (progressBar == null) return;

        normalized = Mathf.Clamp01(normalized);

        if (fakeCoroutine != null)
        {
            StopCoroutine(fakeCoroutine);
            fakeCoroutine = null;
        }

        progressBar.value = normalized;
    }

    /// <summary>Update status text without restarting progress.</summary>
    public void UpdateStatus(string message)
    {
        if (statusText == null) return;
        statusText.text = message;
    }

    /// <summary>Begin finishing animation (from current value to 1.0), then hide the panel.</summary>
    public void Complete()
    {
        if (finishCoroutine != null) StopCoroutine(finishCoroutine);
        finishCoroutine = StartCoroutine(FinishAndHide());
    }

    private IEnumerator FinishAndHide()
    {
        // stop fake progress if running
        if (fakeCoroutine != null)
        {
            StopCoroutine(fakeCoroutine);
            fakeCoroutine = null;
        }

        float start = (progressBar != null) ? progressBar.value : 0f;
        float remaining = 1f - start;

        // if already full, immediate hide
        if (remaining <= 0f)
        {
            if (progressBar != null) progressBar.value = 1f;
            if (loadingPanel != null) loadingPanel.SetActive(false);
            finishCoroutine = null;
            yield break;
        }

        // animate to full — duration scales with remaining to look smooth
        float duration = Mathf.Clamp(remaining * 0.8f, 0.2f, 1.2f);
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            if (progressBar != null) progressBar.value = Mathf.Lerp(start, 1f, t);
            yield return null;
        }

        if (progressBar != null) progressBar.value = 1f;
        if (loadingPanel != null) loadingPanel.SetActive(false);
        finishCoroutine = null;
    }

    /// <summary>Hide immediately and stop everything (use for error/fallback).</summary>
    public void HideImmediate()
    {
        if (fakeCoroutine != null) { StopCoroutine(fakeCoroutine); fakeCoroutine = null; }
        if (finishCoroutine != null) { StopCoroutine(finishCoroutine); finishCoroutine = null; }
        if (loadingPanel != null) loadingPanel.SetActive(false);
        if (progressBar != null) progressBar.value = 0f;
    }
}
