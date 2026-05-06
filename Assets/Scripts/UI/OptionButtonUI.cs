using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

public class OptionHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    [Header("Option Hover Sound Effect")]
    [SerializeField] private AudioClip onHoverSoundEffect;
    
    private TextMeshProUGUI buttonText;
    private Vector3 originalScale;
    private Color originalColor;
    private Color hoverColor = Color.black;
    private readonly float hoverScale = 1.1f;
    private readonly float transitionSpeed = 10f;
    private bool isHovered = false;

    private void Start() {
        buttonText = GetComponent<TextMeshProUGUI>();
        originalScale = transform.localScale;
        originalColor = buttonText.color;
    }

    private void Update() {
        Vector3 targetScale = isHovered ? originalScale * hoverScale : originalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * transitionSpeed);
        Color targetColor = isHovered ? hoverColor : originalColor;
        buttonText.color = Color.Lerp(buttonText.color, targetColor, Time.deltaTime * transitionSpeed);
    }

    public void OnPointerEnter(PointerEventData eventData) {
        if (GetComponent<Button>().interactable) isHovered = true;
        AudioManager.Instance.PlayClip(onHoverSoundEffect);
    }

    public void OnPointerExit(PointerEventData eventData) {
        isHovered = false;
    }
}