using System.Collections;
using UnityEngine;

public class TimeManager : MonoBehaviour {
    public static TimeManager Instance { get; private set; }

    private Coroutine currentSlowMotion;
    private float savedTimeScale = 1f;

    public bool IsPaused { get; private set; } = false;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Pause() {
        if (IsPaused) return;
        savedTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        IsPaused = true;
    }

    private void Resume() {
        if (!IsPaused) return;
        Time.timeScale = savedTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;
        IsPaused = false;
    }

    public void SlowMotion(float slowTimeScale, float duration) {
        if (IsPaused) {
            savedTimeScale = slowTimeScale;
            return;
        }
        if (currentSlowMotion != null) StopCoroutine(currentSlowMotion);
        currentSlowMotion = StartCoroutine(SlowMotionCoroutine(slowTimeScale, duration));
    }

    public void TogglePause() {
        if (IsPaused) {
            Resume();
        } else {
            Pause();
        }
    }

    private IEnumerator SlowMotionCoroutine(float slowTimeScale, float duration) {
        Time.timeScale = slowTimeScale;
        Time.fixedDeltaTime = 0.02f * Time.timeScale;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }
}