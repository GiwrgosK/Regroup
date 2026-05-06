using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ButtonUI : MonoBehaviour {
	[Header("Action Button UI Configuration")]
	[SerializeField] private TextMeshProUGUI textMeshPro;
	[SerializeField] private Button button;
	[SerializeField] private GameObject selectedGameObject;
	[SerializeField] private TextMeshProUGUI actionPointsText;
	[SerializeField] private Image buttonIcon;
	[SerializeField] private TooltipTrigger tooltipTrigger;
	
	[Header("Action Button UI Sprites")]
	[SerializeField] private Sprite shootActionIcon;
	[SerializeField] private Sprite moveActionIcon;
	[SerializeField] private Sprite meleeActionIcon;
	[SerializeField] private Sprite grenadeActionIcon;
	[SerializeField] private Sprite ambushActionIcon;
	[SerializeField] private Sprite suppressActionIcon;

	private BaseAction baseAction;
	
	public void SetBaseAction(BaseAction baseAction) {
		this.baseAction = baseAction;
		IAction action = baseAction;
		textMeshPro.text = action.ActionName.ToUpper();
		actionPointsText.text = action.ActionCost.ToString();
		button.onClick.RemoveAllListeners();
		button.onClick.AddListener(() => { UnitHandler.Instance.SetSelectedAction(baseAction); });
		AssignButtonIcon(baseAction);
	}
	
	public void UpdateButtonVisual() {
		BaseAction selectedBaseAction = UnitHandler.Instance.GetSelectedAction();
		selectedGameObject.SetActive(selectedBaseAction == baseAction);
	}

	private void AssignButtonIcon(IAction action) {
        switch (action.ActionName) {
			case "Fire":
				buttonIcon.overrideSprite = shootActionIcon;
				tooltipTrigger.InitializeTooltip("Shoot", action.ActionDescription, action.ActionCost.ToString());
				break;
			case "Marching Forward":
				buttonIcon.overrideSprite = moveActionIcon;
				tooltipTrigger.InitializeTooltip("Move", action.ActionDescription, action.ActionCost.ToString());
				break;
			case "Knife":
				buttonIcon.overrideSprite = meleeActionIcon;
				tooltipTrigger.InitializeTooltip("Melee", action.ActionDescription, action.ActionCost.ToString());
				break;
			case "Frag Grenade":
				buttonIcon.overrideSprite = grenadeActionIcon;
				tooltipTrigger.InitializeTooltip(action.ActionName, action.ActionDescription, action.ActionCost.ToString());
				break;
			case "Ambush - Watching Post":
				buttonIcon.overrideSprite = ambushActionIcon;
				tooltipTrigger.InitializeTooltip(action.ActionName, action.ActionDescription, "All");
				break;
			case "Tactical Suppression - Covering Fire":
				buttonIcon.overrideSprite = suppressActionIcon; 
				tooltipTrigger.InitializeTooltip(action.ActionName, action.ActionDescription, action.ActionCost.ToString());
				break;
			default:
				break;
		}
    }
}