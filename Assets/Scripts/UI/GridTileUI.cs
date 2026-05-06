using UnityEngine;
using UnityEngine.UI;

public class GridTileUI : MonoBehaviour {
    [Header("Grid Tile UI Sprites & Images")]
    [SerializeField] private Sprite halfCoverImage;
    [SerializeField] private Sprite fullCoverImage;
    [SerializeField] private Image northImage;
    [SerializeField] private Image southImage;
    [SerializeField] private Image eastImage;
    [SerializeField] private Image westImage;

    private void Awake() {
        ClearAll();
    }

    public void SetCover(CoverObject.CoverDirection direction, CoverObject.CoverType type) {
        Image coverIcon = GetImage(direction);

        if (type == CoverObject.CoverType.None) {
            coverIcon.enabled = false;
            return;
        }
        
        gameObject.SetActive(true);
        coverIcon.sprite = type == CoverObject.CoverType.Full ? fullCoverImage : halfCoverImage;
        coverIcon.enabled = true;
    }

    public void ClearAll() {
        northImage.enabled = false;
        southImage.enabled = false;
        eastImage.enabled = false;
        westImage.enabled = false;
    }

    private Image GetImage(CoverObject.CoverDirection direction) {
        return direction switch {
            CoverObject.CoverDirection.North => northImage,
            CoverObject.CoverDirection.South => southImage,
            CoverObject.CoverDirection.East => eastImage,
            CoverObject.CoverDirection.West => westImage,
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
}