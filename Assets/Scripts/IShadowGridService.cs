using System.Collections.Generic;
using UnityEngine;

public interface IShadowGridService
{
    void AddToShadowGrid(Vector2Int position, Object obj);
    void ResetShadowGrid();
    Dictionary<Vector2Int, Object> GetShadowGrid();
}
