using UnityEngine;
using System;
using System.Collections.Generic;

public class EnemyAI : MonoBehaviour {
	private enum State {
		Waiting,
		Active,
		Busy
	}

	private State state;
	private List<Unit> remainingEnemyUnits;
	private float timer;

	private void Awake() {
		state = State.Waiting;
	}

	private void Start() {
		TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
	}

	private void Update() {
		if (TurnHandler.Instance.IsPlayersTurn) return;

		switch (state) {
			case State.Waiting:
				break;
			case State.Active:
				timer -= Time.deltaTime;
				if (timer <= 0f) {
					if (ProcessEnemyTurn(SetStateActive)) {
						state = State.Busy;
					} else {
						//Debug.Log("All enemies completed actions. Ending enemy turn.");
						TurnHandler.Instance.NextTurn();
					}
				}
				break;
			case State.Busy:
				timer -= Time.deltaTime;
				if (timer <= -6f) {
					//Debug.LogWarning("AI timed out in Busy state, skipping turn.");
					SetStateActive();
				}
				break;
			default:
				break;
		}
	}

	private void OnDestroy() {
		TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;
	}

	private void SetStateActive() {
		timer = 0.5f;
		state = State.Active;
	}

	private bool ProcessEnemyTurn(Action onEnemyActionComplete) {
        remainingEnemyUnits.RemoveAll(unit => unit == null || unit.IsDead());
		if (remainingEnemyUnits.Count == 0) return false;

		Unit currentEnemy = remainingEnemyUnits[0];

		if (TryGetBestActionForUnit(currentEnemy, out BaseAction bestAction, out EnemyAIAction bestAIAction)) {
            if (currentEnemy.PlayAction(bestAction)) {
                bestAction.TakeAction(bestAIAction.gridPosition, onEnemyActionComplete);
                return true; 
            }
        }

		remainingEnemyUnits.RemoveAt(0);
		return ProcessEnemyTurn(onEnemyActionComplete);
    }

	private bool TryGetBestActionForUnit(Unit enemyUnit, out BaseAction bestBaseAction, out EnemyAIAction bestAIAction) {
        bestAIAction = null;
        bestBaseAction = null;

        foreach (BaseAction baseAction in enemyUnit.BaseActionArray) {
            if (!enemyUnit.IsActionPlayable(baseAction)) continue;

            EnemyAIAction testEnemyAIAction = baseAction.GetBestEnemyAIAction();
            if (testEnemyAIAction == null || testEnemyAIAction.actionValue <= 0f) continue;

            if (bestAIAction == null || testEnemyAIAction.actionValue > bestAIAction.actionValue) {
                bestAIAction = testEnemyAIAction;
                bestBaseAction = baseAction;
            }
        }
        return bestAIAction != null;
    }

	private void TurnHandler_OnTurnChanged() {
		if (!TurnHandler.Instance.IsPlayersTurn) {
			remainingEnemyUnits = new List<Unit>();
			foreach(Unit enemy in SpawnedUnitHandler.Instance.EnemyUnits) {
                if (!enemy.IsDead()) {
                    remainingEnemyUnits.Add(enemy);
                }
            }
			state = State.Active;
			timer = 1f;
		}
	}
}