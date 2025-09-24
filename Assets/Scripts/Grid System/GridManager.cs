using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class GridManager : MonoBehaviour
{
    private IBlastHandler blastHandler;
    private IJrynothHandler jrynothHandler;
    private ShuffleBoard shuffleBoard;

    private CubeSpriteOrganizer cubeSpriteOrganizer;
    private AnimationManager animationManager;
    private ShadowGridService shadowGridService;
    private GridUpdater gridUpdater;


    private int lastDefaultIconIndex = 3;
    private int lastFirstIconIndex = 5;
    private int lastSecondIconIndex = 7;

    private Dictionary<int, List<Vector2Int>> cubeGroups = new Dictionary<int, List<Vector2Int>>();

    private bool isGridUpdating = false;
    private readonly bool gameOver = false;

    private const float powerupSequenceDelay = 0.15f;
    private const int leastGroupCount = 2;

    private void Start()
    {
        animationManager = FindFirstObjectByType<AnimationManager>();
        blastHandler = new BlastHandler();
        jrynothHandler = new JrynothHandler();
        shuffleBoard = new ShuffleBoard();
    }

    public void InitializeGridWithLevelData(ObjectData objectData, ShadowGridService shadowGridService)
    {
        gridUpdater = new GridUpdater(this, shadowGridService);
        cubeSpriteOrganizer = new CubeSpriteOrganizer(objectData, lastDefaultIconIndex, lastFirstIconIndex, lastSecondIconIndex);
        StartCoroutine(HintCoroutine());
    }


    // When trying to keep the default sprites, I encountered an issue where the sprites changed after the hint due to concurrency, so I added a delay to prevent this.
    private IEnumerator HintCoroutine()
    {
        yield return new WaitForSeconds(0.1f);
        CheckHint();
    }

    private void CheckHint()
    {
        cubeGroups.Clear();
        blastHandler.FindGroups(cubeGroups, leastGroupCount);
        if (cubeGroups.Count == 0) 
        {
            StartCoroutine(CheckHintWithDelay());
            return;
        }

        cubeSpriteOrganizer.OrganizeCubeSprites(cubeGroups);
    }
    private IEnumerator CheckHintWithDelay()
    {
        yield return StartCoroutine(shuffleBoard.WaitAndShuffle());
        CheckHint();
    }

    //_________________________________________________________________________________________________________________________


    public void CheckBlast(GridEntity touchedObject)
    {
        if (isGridUpdating || gameOver)
        {
            return;
        }

        if (touchedObject.IsCube()){
            CubeBlast(touchedObject);
            return;
        }

        Powerups powerupComponent = touchedObject.GetComponent<Powerups>();
        if (powerupComponent != null)
        {
            if (powerupComponent.GetPowerupType() == PowerupType.Jrynoth)
            {
                jrynothHandler.HandleJrynothBlast(cubeSpriteOrganizer, animationManager, this, cubeGroups, lastDefaultIconIndex);
                GameGrid.Instance.GridArray[touchedObject.GetGridX(), touchedObject.GetGridY()] = null;
                Destroy(touchedObject.gameObject);
                StartCoroutine(WaitForDestroyAnimToFinish(animationManager.GetSpecialDestroyAnimDuration(), 0, 0, 0));
            }
            else
            {
                StartCoroutine(ActivateBlast(powerupComponent));
            }
        }
    }
    private void CubeBlast(GridEntity touchedObject)
    {
        int touchedIndexX = touchedObject.GetGridX();
        int touchedIndexY = touchedObject.GetGridY();
        float touchedObjectX = touchedObject.transform.position.x;
        float touchedObjectY = touchedObject.transform.position.y;

        foreach (var group in cubeGroups)
        {
            if (group.Value.Contains(new Vector2Int(touchedIndexX, touchedIndexY)))
            {
                foreach (var position in group.Value)
                {
                    int x = position.x;
                    int y = position.y;
                    GameObject objectInGroup = GameGrid.Instance.GridArray[x, y];
                    GameGrid.Instance.GridArray[x, y] = null;
                    CheckAndDamageAdjacentObstacles(new Vector2Int (x,y), ExplosionType.NormalBlast);
                    animationManager.DestroyObjectAnim(objectInGroup, objectInGroup.GetComponent<Object>().GetObjectColor(), objectInGroup.GetComponent<Object>().IsDefaultSprite(), touchedObjectX, touchedObjectY);
                }
                StartCoroutine(WaitForDestroyAnimToFinish(animationManager.GetSpecialDestroyAnimDuration(), touchedIndexX, touchedIndexY, group.Value.Count));
                group.Value.Clear();
                return;
            }
        }
    }

    public void Denemelik(int touchedIndexX, int touchedIndexY)
    {
        foreach (var group in cubeGroups)
        {
            if (group.Value.Contains(new Vector2Int(touchedIndexX, touchedIndexY)))
            {
                foreach (var position in group.Value)
                {
                    int x = position.x;
                    int y = position.y;

                    if (GameGrid.Instance.GridArray[x, y]?.TryGetComponent(out Object objectComponent) == true)
                    {
                        objectComponent.ResetObjectSprites();
                    }
                }
            }
        }
    }

    private void CheckAndDamageAdjacentObstacles(Vector2Int position, ExplosionType explosionType)
    {
        Vector2Int[] directions = { new(1, 0), new(-1, 0), new(0, 1), new(0, -1) };

        foreach (var dir in directions)
        {
            int adjX = position.x + dir.x;
            int adjY = position.y + dir.y;

            if (adjX < 0 || adjX >= GameGrid.Instance.GridWidth || adjY < 0 || adjY >= GameGrid.Instance.GridHeight)
                continue;

            GameObject adjacentObject = GameGrid.Instance.GridArray[adjX, adjY];
            if (adjacentObject == null)
                continue;

            if (adjacentObject.TryGetComponent<Obstacle>(out var obstacle))
            {
                obstacle.TakeHit(explosionType);
            }
        }
    }


    private IEnumerator WaitForDestroyAnimToFinish(float delay, int touchedIndexX, int touchedIndexY, int groupCount)
    {
        isGridUpdating = true;
        yield return new WaitForSeconds(delay*1.01f);

        if (groupCount != 0)
        {
            int spriteIndex = cubeSpriteOrganizer.GetSpriteIndex(groupCount);
            if (spriteIndex != -1)
            {
                PowerupFactory.Instance.CreatePowerup(spriteIndex, touchedIndexX, touchedIndexY);
            }
        }

        gridUpdater.UpdateGridAfterBlast();
    }

    public void GenerateAllPowerups(int touchedIndexX, int touchedIndexY, int groupCount)
    {
        int spriteIndex = cubeSpriteOrganizer.GetSpriteIndex(groupCount);
        if (spriteIndex != -1)
        {
            PowerupFactory.Instance.CreatePowerup(spriteIndex, touchedIndexX, touchedIndexY);
        }
    }

    private IEnumerator ActivateBlast(Powerups touchedPowerup)
    {
        isGridUpdating = true;
        yield return StartCoroutine(blastHandler.PowerupSequenceController(touchedPowerup, animationManager, powerupSequenceDelay));

        gridUpdater.UpdateGridAfterBlast();
    }

    public void UpdateFinished()
    {
        isGridUpdating = false;
        CheckHint();
    }
}
