using UnityEngine;

public class GridTile : MonoBehaviour {
    [Header("Grid Tile Mesh Renderer")]
    [SerializeField] private MeshRenderer meshRenderer;

    public bool IsVisible {
        get => meshRenderer.enabled;
        set => meshRenderer.enabled = value;
    }

    public Material TileMaterial {
        get => meshRenderer.material;
        set {
            meshRenderer.material = value;
            meshRenderer.enabled = true;
        }
    }
}