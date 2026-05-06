using UnityEngine;
using System;

public class Unit : MonoBehaviour {
	public static event Action OnAnyActionPointChange;
	public static event Action<Unit> OnAnyUnitSpawn;
	public static event Action<Unit> OnAnyUnitDeath;
	public event Action OnSuppressed;

	[Header("Unit Configuration")]
	[SerializeField] private HealthHandler healthHandler;
	[SerializeField] private UnitUI unitUI;
	[SerializeField] private bool isEnemy;
	[SerializeField] private int maxActionPoints;

	private GridPosition gridPosition;
	private BaseAction[] baseActionArray;
	private SoldierData data;
	private int actionPoints;
	private bool isSuppressed = false;
	private int suppressedTurnsRemaining = 0;

	public UnitUI GetUnitUI() => unitUI;
	public SoldierData Data => data;
	public GridPosition UnitGridPosition => gridPosition;
	public BaseAction[] BaseActionArray => baseActionArray;
	public int ActionPoints => actionPoints;
	public bool IsSuppressed => isSuppressed;
	public bool IsEnemy => isEnemy;
	public bool FiresBurst => Data.roleData.roleName == "Officer" || Data.roleData.roleName == "Heavy Gunner";
	public bool FiresShotgun => Data.roleData.roleName == "Breacher";
	public bool IsInAmbush { get; set; }

	private void Awake() {
		baseActionArray = GetComponents<BaseAction>();
		actionPoints = maxActionPoints;
	}
	
	private void Start() {
		gridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
		LevelGrid.Instance.AddUnitAtPosition(gridPosition, this);
		TurnHandler.Instance.OnTurnChanged += TurnHandler_OnTurnChanged;
		healthHandler.OnDead += HealthHandler_OnDead;
		OnAnyUnitSpawn?.Invoke(this);
	}

	private void Update() {
		GridPosition newGridPosition = LevelGrid.Instance.GetGridPosition(transform.position);
		if (newGridPosition != gridPosition) {
			GridPosition oldGridPosition = gridPosition;
			gridPosition = newGridPosition;
			LevelGrid.Instance.UnitMovedPosition(this, oldGridPosition, newGridPosition);
		}
	}

	private void OnDestroy() {
		TurnHandler.Instance.OnTurnChanged -= TurnHandler_OnTurnChanged;
		healthHandler.OnDead -= HealthHandler_OnDead;
	}

	public void Initialize(SoldierData soldierData) {
		data = soldierData;
		healthHandler.Initialize(data.currentHealth, data.roleData.maxHealth);
	}

	public T GetAction<T>() where T : BaseAction {
		foreach (BaseAction baseAction in baseActionArray) {
			if (baseAction is T t) {
				return t;
			}
		}
		return null;
	}
	
	public bool PlayAction(BaseAction baseAction) {
		IAction action = baseAction;
		if (IsActionPlayable(baseAction)) {
			UseActionPoints(action.ActionCost);
			return true;
		} else {
			return false;
		}
	}
	
	public bool IsActionPlayable(BaseAction baseAction) {
		IAction action = baseAction;
		return actionPoints >= action.ActionCost;
	}
	
	private void UseActionPoints(int amount) {
		actionPoints -= amount;
		OnAnyActionPointChange?.Invoke();
	}

	public float GetHealth() {
		return healthHandler.CurrentHealth;
	}

	public bool IsDead() {
		return healthHandler.IsDead;
	}
	
	public void Damage(int damage, Vector3 sourcePosition, string sourceType) {
		healthHandler.Damage(damage, sourcePosition, sourceType);
		unitUI.SetDamageText(damage);
	}
	
	public Vector3 GetWorldPosition() {
		return transform.position;
	}

	public Vector3 GetForwardDirection() {
		return transform.forward;
	}

	public void ApplySuppression(int turns = 1) {
		isSuppressed = true;
		suppressedTurnsRemaining = turns;
		OnSuppressed.Invoke();
	}

	private void ClearSuppression() {
		isSuppressed = false;
		suppressedTurnsRemaining = 0;
		unitUI.ClearSuppression();
	}

	public void EmptyActionPoints() {
        actionPoints = 0;
		OnAnyActionPointChange?.Invoke();
    }

	private void TurnHandler_OnTurnChanged() {
		bool isMyTurn = (isEnemy && !TurnHandler.Instance.IsPlayersTurn) || (!isEnemy && TurnHandler.Instance.IsPlayersTurn);
		
		if (isMyTurn) {
			actionPoints = maxActionPoints;
        	OnAnyActionPointChange?.Invoke();
		} else {
			if (isSuppressed) {
            	suppressedTurnsRemaining--;
            	if (suppressedTurnsRemaining <= 0) {
                	ClearSuppression();
            	}
        	}
		}
	}

	private void HealthHandler_OnDead(HealthHandler.OnDeadEventArgs _) {
		LevelGrid.Instance.RemoveUnitAtPosition(gridPosition, this);
		if (data != null && GameManager.Instance.squad.Contains(data)) {
			GameManager.Instance.squad.Remove(data);
		}
		OnAnyUnitDeath?.Invoke(this);
		Destroy(gameObject);
	}
}