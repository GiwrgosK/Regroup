using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class TurnHandlerUI : MonoBehaviour {
	[Header("Turn Handler UI Configuration")]
	[SerializeField] private TextMeshProUGUI turnNumberText;
	[SerializeField] private TextMeshProUGUI enemyTurnText;

	[Header("Turn Handler UI Button & Sound Effect")]
	[SerializeField] private Button endTurnButton;
	[SerializeField] private AudioClip onClickSoundEffect;

	private Coroutine pulseCoroutine;
    private Color originalTextColor;
	private readonly float pulseSpeed = 2f;
	
	private void Start() {
		if (enemyTurnText != null) {
            originalTextColor = enemyTurnText.color;
        }

		TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
		endTurnButton.onClick.AddListener(() => { 
			TurnHandler.Instance.NextTurn(); 
			AudioManager.Instance.PlayClip(onClickSoundEffect);
		});
		UpdateTurnText();
		UpdateEnemyTurnVisual();
		UpdateEndTurnButton();
	}

	private void OnDestroy() {
		TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;	
	}
	
	private void UpdateTurnText() {
		turnNumberText.text = "Turn " +TurnHandler.Instance.Turn;
	}
	
	private void UpdateEnemyTurnVisual() {
        if (!TurnHandler.Instance.IsPlayersTurn) {
            enemyTurnText.gameObject.SetActive(true);
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            pulseCoroutine = StartCoroutine(PulseTextRoutine());
        } else {
            if (pulseCoroutine != null) StopCoroutine(pulseCoroutine);
            enemyTurnText.color = originalTextColor; 
            enemyTurnText.gameObject.SetActive(false);
        }
	}
	
	private void UpdateEndTurnButton() {
		endTurnButton.gameObject.SetActive(TurnHandler.Instance.IsPlayersTurn);
	}

	private void TurnHandler_OnTurnChanged() {
		UpdateTurnText();
		UpdateEnemyTurnVisual();
		UpdateEndTurnButton();
	}

	private IEnumerator PulseTextRoutine() {
        while (true) {
            float pingPongValue = Mathf.PingPong(Time.time * pulseSpeed, 1f);
            float currentAlpha = Mathf.Lerp(0.3f, 1f, pingPongValue);
            enemyTurnText.color = new Color(originalTextColor.r, originalTextColor.g, originalTextColor.b, currentAlpha);
            yield return null;
        }
    }
}