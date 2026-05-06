using UnityEngine;
using TMPro;

public class GridDebugObject : MonoBehaviour {
	[Header("Grid Debug Object Text")]
	[SerializeField] private TextMeshPro textMeshPro;

	private object gridObject;

	public virtual object GridObject {
		get => gridObject;
		set {
			gridObject = value;
			UpdateText();
		}
	}
	
	private void UpdateText() {
		if (textMeshPro != null && gridObject != null) {
			textMeshPro.text = gridObject.ToString();
		}
	}
}