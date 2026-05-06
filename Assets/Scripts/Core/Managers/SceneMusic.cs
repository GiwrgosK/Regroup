using UnityEngine;

public class SceneMusic : MonoBehaviour {
    [Header("Scene Music AudioClip Configuration")]
    [SerializeField] private AudioClip sceneMusic;

    private void Start() {
        if (sceneMusic != null) {
            AudioManager.Instance.PlayMusic(sceneMusic, true);
        }
    }
}