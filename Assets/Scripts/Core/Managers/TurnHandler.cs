using UnityEngine;
using System;

public class TurnHandler : MonoBehaviour {
	public static TurnHandler Instance { get; private set; }
	
	public event Action OnTurnChanged;
	
	private bool isPlayersTurn = true;
	public bool IsPlayersTurn => isPlayersTurn;
	
	private int turn = 1;
	public int Turn => turn;
	
	private void Awake() {
		if (Instance != null) {
			Destroy(gameObject);
			return;
		}
		Instance = this;
	}
	
	public void NextTurn() {
		turn++;
		isPlayersTurn = !isPlayersTurn;
		OnTurnChanged?.Invoke();
	}
}