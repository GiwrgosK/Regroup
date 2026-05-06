using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransitionManager : MonoBehaviour {
    public static SceneTransitionManager Instance { get; private set;}

    [Header("Scene Transition Manager Fade Configuration")]
    [SerializeField] private CanvasGroup fadeCanvasGroup;

    private readonly float fadeDuration = 0.5f;

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void LoadScene(string sceneName) {
        AudioManager.Instance.PlaySceneChangeSoundEffect();
        StartCoroutine(FadeAndSwitchScenes(sceneName));
    }

    private IEnumerator FadeAndSwitchScenes(string sceneName) {
        yield return FadeInOut(0f, 1f);
        yield return SceneManager.LoadSceneAsync(sceneName);
        yield return FadeInOut(1f, 0f);
    }

    private IEnumerator FadeInOut(float source, float destination) {
        if (fadeCanvasGroup == null) {
            Debug.LogError("FadeCanvas not assigned!");
            yield break;
        }

        float time = 0f;
        while (time <= fadeDuration) {
            time += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(source, destination, time / fadeDuration);
            yield return null;
        }
    }
}