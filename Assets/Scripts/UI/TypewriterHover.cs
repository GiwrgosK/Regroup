using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class TypewriterHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler{
    [Header("Typewriter Hover Configuration")]
    [SerializeField] private TextMeshProUGUI text;
    [SerializeField] private Button button;
    [SerializeField] private AudioClip typingSoundEffect;

    private Coroutine typingCoroutine;
    private readonly float startDelay = 0.15f;
    private readonly float typingSpeed = 0.09f;
    private readonly bool randomVariance = true;
    private int totalVisibleCharacters;

    private void OnEnable() {
        ResetText();
    }

    private void OnDisable() {
        ResetText();
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (button != null && !button.interactable) return;
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        typingCoroutine = StartCoroutine(TypeText());
    }

    public void OnPointerExit(PointerEventData eventData) {
        ResetText();
    }

    private void ResetText() {
        if (typingCoroutine != null) StopCoroutine(typingCoroutine);
        if (text != null) text.maxVisibleCharacters = 99999;
    }

    private IEnumerator TypeText() {
        yield return new WaitForSecondsRealtime(startDelay);
        text.ForceMeshUpdate();
        totalVisibleCharacters = text.textInfo.characterCount;
        text.maxVisibleCharacters = 0;

        for (int i = 0; i <= totalVisibleCharacters; i++) {
            AudioManager.Instance.PlayClip(typingSoundEffect);
            text.maxVisibleCharacters = i;
            float delay = typingSpeed;
            if (randomVariance) delay += Random.Range(-0.02f, 0.02f);
            yield return new WaitForSecondsRealtime(Mathf.Max(0.01f, delay));
        }
    }
}