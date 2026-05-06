using UnityEngine;
using UnityEngine.EventSystems;
using System;

public class TooltipTrigger : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
    public static event Action<string, string, string> OnHoverStart;
    public static event Action OnHoverEnd;

    private string actionName;
    private string description;
    private string actionCost;

    public void InitializeTooltip(string actionName, string description, string actionCost) {
        this.actionName = actionName;
        this.description = description;
        this.actionCost = actionCost;
    }

    public void OnPointerEnter(PointerEventData eventData) {
        OnHoverStart?.Invoke(actionName, description, actionCost);
    }

    public void OnPointerExit(PointerEventData eventData) {
        OnHoverEnd?.Invoke();
    }
}