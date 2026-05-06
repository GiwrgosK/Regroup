using UnityEngine;

public class CoverObject : MonoBehaviour {
    public enum CoverType {
        None,
        Half,
        Full
    }

    public enum CoverDirection {
        North,
        South,
        East,
        West
    }

    public enum CoverShape {
        Horizontal,
        Gamma,
        Pi,
        SingleTile
    }

    [Header("Cover Object Configuration")]
    [SerializeField] private CoverType coverType;
    [SerializeField] private CoverShape coverShape;

    private ICoverShape iCoverShape;
    public CoverType CoverTypeProperty => coverType;

    private void Awake() {
        iCoverShape = coverShape switch {
            CoverShape.Horizontal => new HorizontalSandbagShape(),
            CoverShape.Gamma => new GammaSandbagShape(),
            CoverShape.Pi => new PiSandbagShape(),
            CoverShape.SingleTile => new SingleTileShape(),
            _ => throw new System.ArgumentOutOfRangeException()
        };
    }
    
    public void SetupCover() {
        iCoverShape?.BlockAdjacentTiles(transform, coverType);
    }
}