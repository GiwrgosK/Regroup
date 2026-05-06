using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class UnitUI : MonoBehaviour {
    [Header("Unit Handler UI Configuration")]
    [SerializeField] private Unit unit;
    [SerializeField] private HealthHandler healthHandler;

    [Header("Unit Handler UI Visuals")]
    [SerializeField] private Image roleIconImage;
    [SerializeField] private Image healthBarImage;
    [SerializeField] private Image ambushIndicatorImage;
    [SerializeField] private Image suppressIndicatorImage;
    [SerializeField] private TextMeshProUGUI actionText;
    [SerializeField] private TextMeshProUGUI actionPointsText;
    [SerializeField] private TextMeshProUGUI hitChanceText;

    private void Start() {
        BaseAction.OnAnyActionStart += BaseAction_OnAnyActionStart;
        AmbushAction.OnAmbushSet += AmbushAction_OnAmbushSet;
        AmbushAction.OnAmbushEnded += AmbushAction_OnAmbushEnded;
        unit.OnSuppressed += Unit_OnSuppressed;
        Unit.OnAnyActionPointChange += Unit_OnAnyActionPointChange;
        TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
        healthHandler.OnDamage += HealthHandler_OnDamage;
        roleIconImage.sprite = unit.Data.roleData.roleIcon;
        UpdateActionPointsText();
        UpdateHealthBar();
    }

    private void OnDestroy() {
        BaseAction.OnAnyActionStart -= BaseAction_OnAnyActionStart;
        AmbushAction.OnAmbushSet -= AmbushAction_OnAmbushSet;
        AmbushAction.OnAmbushEnded -= AmbushAction_OnAmbushEnded;
        unit.OnSuppressed -= Unit_OnSuppressed;
        Unit.OnAnyActionPointChange -= Unit_OnAnyActionPointChange;
        TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;
        healthHandler.OnDamage -= HealthHandler_OnDamage;
    }

    private void UpdateActionPointsText() {
        actionPointsText.text = unit.ActionPoints.ToString();
    }

    private void UpdateHealthBar() {
        healthBarImage.fillAmount = healthHandler.GetHealthNormalized();
    }

    public void SetHitChance(int? value) {
        if (value.HasValue) {
            hitChanceText.gameObject.SetActive(true);
            hitChanceText.text = $"{value.Value}%";
        } else {
            hitChanceText.gameObject.SetActive(false);
        }
    }

    public void SetDamageText(int amount) {
        actionText.text = $"- {amount}";
        actionText.color = Color.red;
        actionText.enabled = true;
        StartCoroutine(AnimateActionText());
    }

    private void BaseAction_OnAnyActionStart(BaseAction baseAction) {
        if (baseAction != null && baseAction.Unit == unit) {
            actionText.text = ((IAction)baseAction).ActionName;
            actionText.enabled = true;
            StartCoroutine(AnimateActionText());
        }
    }

    private void AmbushAction_OnAmbushSet(Unit targetUnit) {
        if (unit != targetUnit) return;
        ambushIndicatorImage.gameObject.SetActive(true);
    }

    private void AmbushAction_OnAmbushEnded(Unit targetUnit) {
        if (unit != targetUnit) return;
        ambushIndicatorImage.gameObject.SetActive(false);
    }

    private void Unit_OnSuppressed() {
        suppressIndicatorImage.gameObject.SetActive(true);
    }

    public void ClearSuppression() {
        suppressIndicatorImage.gameObject.SetActive(false);
    }

    private void Unit_OnAnyActionPointChange() {
        UpdateActionPointsText();
    }

    private void TurnHandler_OnTurnChanged() {
        if (TurnHandler.Instance.IsPlayersTurn && unit.IsEnemy) actionPointsText.enabled = false;
        else if (TurnHandler.Instance.IsPlayersTurn && !unit.IsEnemy) actionPointsText.enabled = true;
        
        if (!TurnHandler.Instance.IsPlayersTurn && !unit.IsEnemy) actionPointsText.enabled = false;
        else if (!TurnHandler.Instance.IsPlayersTurn && unit.IsEnemy) actionPointsText.enabled = true;
    }

    private void HealthHandler_OnDamage() {
        UpdateHealthBar();
    }

    private IEnumerator AnimateActionText() {
        Vector3 startPos = actionText.transform.localPosition;
        float duration = 1.5f;
        float currentTime = 0f;
        float floatSpeed = 1f;
        actionText.alpha = 1f; 

        while (currentTime < duration) {
            actionText.transform.localPosition += floatSpeed * Time.deltaTime * Vector3.up;
            float fadeStartTime = duration * 0.5f;
            if (currentTime > fadeStartTime) {
                float fadeProgress = (currentTime - fadeStartTime) / (duration - fadeStartTime);
                actionText.alpha = Mathf.Lerp(1f, 0f, fadeProgress);
            }

            currentTime += Time.deltaTime;
            yield return null;
        }
        actionText.transform.localPosition = startPos;
        actionText.enabled = false;
        actionText.color = Color.white;
    }
}