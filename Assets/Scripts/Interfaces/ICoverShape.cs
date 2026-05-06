using UnityEngine;

public interface ICoverShape {
    void BlockAdjacentTiles(Transform coverTransform, CoverObject.CoverType coverType);
}