using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using TMPro;

public class CreditsLink : MonoBehaviour, IPointerClickHandler {
    [Header("Credits Link Text")]
    [SerializeField] private TextMeshProUGUI text;

    private Color hoverColor = new Color(0.2f, 0.6f, 1f, 1f);
    private int currentLinkIndex = -1;

    private void LateUpdate() {
        CheckForLinkHover();
    }

    public void OnPointerClick(PointerEventData eventData) {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Mouse.current.position.ReadValue(), null);
        if (linkIndex != -1) {
            TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
            string selectedLink = linkInfo.GetLinkID();

            if (!string.IsNullOrEmpty(selectedLink)) {
                Application.OpenURL(selectedLink);
            }
        }
    }

    private void CheckForLinkHover() {
        int linkIndex = TMP_TextUtilities.FindIntersectingLink(text, Mouse.current.position.ReadValue(), null);
        if (linkIndex != -1 && linkIndex != currentLinkIndex) {
            ResetColor();
            currentLinkIndex = linkIndex;
            SetLinkColor(linkIndex, hoverColor);
        } else if (linkIndex == -1 && currentLinkIndex != -1) {
            ResetColor();
            currentLinkIndex = -1;
        }
    }

    private void SetLinkColor(int linkIndex, Color32 color) {
        TMP_LinkInfo linkInfo = text.textInfo.linkInfo[linkIndex];
        if (text.textInfo.characterInfo.Length == 0) return;

        for (int i = 0; i < linkInfo.linkTextLength; i++) {
            int characterIndex = linkInfo.linkTextfirstCharacterIndex + i;
            TMP_CharacterInfo charInfo = text.textInfo.characterInfo[characterIndex];
            if (!charInfo.isVisible) continue;

            int materialIndex = charInfo.materialReferenceIndex;
            int vertexIndex = charInfo.vertexIndex;
            Color32[] vertexColors = text.textInfo.meshInfo[materialIndex].colors32;
            vertexColors[vertexIndex + 0] = color;
            vertexColors[vertexIndex + 1] = color;
            vertexColors[vertexIndex + 2] = color;
            vertexColors[vertexIndex + 3] = color;
        }
        text.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
    }

    private void ResetColor() {
        text.ForceMeshUpdate();
    }
}