using UnityEngine;
using TMPro;

public class TooltipPanel : MonoBehaviour {
    [Header("Tooltip Panel Configuration")]
    [SerializeField] private GameObject tooltipPanel;
    [SerializeField] private RectTransform rectTransform;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI contentText;

    private void Update() {
        if (tooltipPanel.activeSelf) {
            Vector2 mousePosition = InputManager.Instance.GetMousePosition();
            float pivotX = mousePosition.x / Screen.width;
            float pivotY = mousePosition.y / Screen.height;
            float finalPivotX = pivotX > 0.5f ? 1f : 0f;
            float finalPivotY = pivotY > 0.5f ? 1f : 0f;
            rectTransform.pivot = new Vector2(finalPivotX, finalPivotY);
            float offsetX = finalPivotX == 0 ? 15f : -15f;
            float offsetY = finalPivotY == 0 ? 15f : -15f;
            transform.position = mousePosition + new Vector2(offsetX, offsetY);
        }
    }

    private void OnEnable() {
        TooltipTrigger.OnHoverStart += TooltipTrigger_OnHoverStart;
        TooltipTrigger.OnHoverEnd += TooltipTrigger_OnHoverEnd;
    }

    private void OnDisable() {
        TooltipTrigger.OnHoverStart -= TooltipTrigger_OnHoverStart;
        TooltipTrigger.OnHoverEnd -= TooltipTrigger_OnHoverEnd;
    }

    private void TooltipTrigger_OnHoverStart(string header, string description, string actionCost) {
        headerText.text = header;

        if (string.IsNullOrEmpty(actionCost)) {
            contentText.text = description;
        } else {
            contentText.text = $"{description}\n<color=#FFD700><b>- {actionCost} Action Points</b></color>";
        }

        tooltipPanel.SetActive(true);
        Update();
    }

    private void TooltipTrigger_OnHoverEnd() {
        tooltipPanel.SetActive(false);
    }
}