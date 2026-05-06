using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InfoButtonUI : MonoBehaviour {
    [Header("Info Button UI Configuration")]
    [SerializeField] private TextMeshProUGUI textMeshPro;
    [SerializeField] private Button button;
    [SerializeField] private Image buttonIcon;
    [SerializeField] private TooltipTrigger tooltipTrigger;
    
    [Header("Info Button UI Visuals")]
    [SerializeField] private Sprite infoIconSprite;

    private Unit unit;
    private HealthHandler unitHealthHandler;

    private void OnDestroy() {
        if (unitHealthHandler != null) {
            unitHealthHandler.OnDamage -= UnitHealthHandler_OnDamage;
        }
    }

    public void SetUnit(Unit unit) {
        button.enabled = false;
        this.unit = unit;
        unitHealthHandler = unit.GetComponent<HealthHandler>();
        if (unitHealthHandler != null) {
            unitHealthHandler.OnDamage += UnitHealthHandler_OnDamage;
        }
        textMeshPro.text = "INFO"; 

        if (infoIconSprite != null) {
            buttonIcon.overrideSprite = infoIconSprite;
        }

        SoldierData data = unit.Data;
        SoldierRoleData role = data.roleData;

        string headerName = $"{data.firstName} <color=#AAAAAA>[{data.serialNumber}]</color> {data.lastName}";

        string statsContent = 
            $"<b>Role:</b> {role.roleName}\n" +
            $"<b>Weapon:</b> {role.weaponName} <color=#FF8888>(Dmg: {role.weaponDamage}, Rng: {role.weaponRange})</color>\n" +
            $"<b>Health:</b> {data.currentHealth} / {role.maxHealth} | <b>Move:</b> {role.movementRange} | <b>Total Grenades:</b> {role.grenadeAmount}\n" +
            $"<color=#555555>-----------------------------------</color>\n" +
            $"<i>{data.bio}</i>";

        tooltipTrigger.InitializeTooltip(headerName, statsContent, "");
    }

    private void UnitHealthHandler_OnDamage() {
        if (unit == null) return;

        SoldierData data = unit.Data;
        SoldierRoleData role = data.roleData;

        string headerName = $"{data.firstName} <color=#AAAAAA>[{data.serialNumber}]</color> {data.lastName}";

        string statsContent = 
            $"<b>Role:</b> {role.roleName}\n" +
            $"<b>Weapon:</b> {role.weaponName} <color=#FF8888>(Dmg: {role.weaponDamage}, Rng: {role.weaponRange})</color>\n" +
            $"<b>Health:</b> {unit.GetHealth()} / {role.maxHealth} | <b>Move:</b> {role.movementRange} | <b>Total Grenades:</b> {role.grenadeAmount}\n" +
            $"<color=#555555>-----------------------------------</color>\n" +
            $"<i>{data.bio}</i>";

        tooltipTrigger.InitializeTooltip(headerName, statsContent, "");
    }
}