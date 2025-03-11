using UnityEngine;

public class GameViewManager 
{
    private readonly float backgroundWidthMultiplier = 108;
    private readonly float backgroundHeigthMultiplier = 110;

    public void InitializeView(GameObject gridBackground)
    {
        AdjustBackground(gridBackground);
        AdjustCamera();
    }
    private void AdjustBackground(GameObject gridBackground)
    {
        int gridWidth = GameGrid.Instance.GridWidth;
        int gridHeight = GameGrid.Instance.GridHeight;
        gridBackground.SetActive(true);
        RectTransform gridRectTransform = gridBackground.GetComponent<RectTransform>();
        gridRectTransform.sizeDelta = new Vector2((backgroundWidthMultiplier - gridWidth) * gridWidth, (backgroundHeigthMultiplier - gridHeight) * gridHeight);
    }
    private void AdjustCamera()
    {
        int gridWidth = GameGrid.Instance.GridWidth;
        int gridHeight = GameGrid.Instance.GridHeight;
        float gridPositionXYOffset = GameGrid.Instance.GridPositionXYOffset;
        Camera camera = Camera.main;
        camera.transform.position = new Vector3((gridWidth - 1) * gridPositionXYOffset / 2, (gridHeight + 4.6F) * gridPositionXYOffset / 2, -10);
    }
}
