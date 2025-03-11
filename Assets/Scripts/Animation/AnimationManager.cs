using System.Linq;
using DG.Tweening;
using UnityEngine;

public class AnimationManager : MonoBehaviour
{
    [Header("Swap Objects Settings")]
    [SerializeField] private float swapObjectTime = 0.5f;

    [Header("Drop Animation Settings")]
    [SerializeField] private float objectDropDuration = 0.1f;
    [SerializeField] private float bounceHeight = 0.1f;
    [SerializeField] private float bounceDuration = 0.1f;

    [Header("Rocket Animation Settings")]
    [SerializeField] private float rocketAnimDuration = 0.6f;

    [Header("Particle Settings")]
    [SerializeField] private float particleObstacleScale = 0.12f;
    [SerializeField] private float particleObstacleLiftDuration = 0.01f;
    [SerializeField] private Vector2 particleObstacleRandomFallDistance = new(2f, 3f);
    [SerializeField] private Vector2 particleObstacleRandomDuration = new(0.7f, 1f);

    [Header("Default Destroy Object Settings")]
    [SerializeField] private float defaultObjectCollapseDuration = 0.2f;
    [SerializeField] private Vector3 defaultObjectCollapseScale = new (0f, 0f, 0f);

    [Header("Special Destroy Object Settings")]
    [SerializeField] private float pushStrengthX = 1f;
    [SerializeField] private float pushStrengthY = 1f;
    [SerializeField] private float pushDuration = 0.15f;
    [SerializeField] private float returnDuration = 0.15f;

    public AnimationObjectData AnimationObjectData;
    public Sprite RocketHalfSprite;

    private static AnimationManager _instance;
    public static AnimationManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindFirstObjectByType<AnimationManager>();

                if (_instance == null)
                {
                    Debug.LogError("No AnimationManager found in the scene! Make sure there is an AnimationManager object.");
                }
            }
            return _instance;
        }
    }

    public void DropObjectAnim(GameObject dropObject, Vector3 position, float distance)
    {
        float dropDuration = distance * objectDropDuration;
        float bounceHeight = this.bounceHeight; 
        float bounceDuration = this.bounceDuration;

        Vector3 bouncePosition = new Vector3(position.x, position.y + bounceHeight, position.z);

        Sequence dropSequence = DOTween.Sequence();

        dropSequence.Append(dropObject.transform.DOMove(position, dropDuration).SetEase(Ease.Linear)) 
                    .Append(dropObject.transform.DOMove(bouncePosition, bounceDuration).SetEase(Ease.OutQuad)) 
                    .Append(dropObject.transform.DOMove(position, bounceDuration).SetEase(Ease.InQuad)); 
    }


    public void SwapObjectsAnim(GameObject[] objects, Vector3[] positions)
    {
        if (objects.Length != positions.Length) return;

        Sequence swapSequence = DOTween.Sequence();
        for (int i = 0; i < objects.Length; i++)
        {
            swapSequence.Join(objects[i].transform.DOMove(positions[i], swapObjectTime).SetEase(Ease.InOutQuad));
        }
    }

    public void DestroyObjectAnim(GameObject blastedObject, ObjectColor color, bool isDefaultIcon, float touchedObjectX, float touchedObjectY)
    {
        if (!isDefaultIcon)
        {
            SpecialObjectCollapse(blastedObject, touchedObjectX, touchedObjectY);
            return;
        }

        switch (color)
        {
            case ObjectColor.Blue:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particleBlue);
                break;
            case ObjectColor.Green:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particleGreen);
                break;
            case ObjectColor.Pink:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particlePink);
                break;
            case ObjectColor.Purple:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particlePurple);
                break;
            case ObjectColor.Red:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particleRed);
                break;
            case ObjectColor.Yellow:
                DefaultObjectCollapse(blastedObject, AnimationObjectData.particleYellow);
                break; 
        }
        return;
    }

    public void TntBlastAnim(GameObject blastedObject)
    {
        DefaultObjectCollapse(blastedObject, AnimationObjectData.particleTnt);
    }

    public float RocketBlastAnim(float blastedRocketX, float blastedRocketY, bool isGoingUp, int sortingOrder, Sprite rocketSprite)
    {
        float moveDistance = isGoingUp ? 6f : 4f;
        float moveDuration = isGoingUp ? rocketAnimDuration : rocketAnimDuration * 3 / 4;
        float rotation1 = isGoingUp ? 0f : 90f;
        float rotation2 = isGoingUp ? 180f : -90f;
        Vector3 moveDirection = isGoingUp ? Vector3.up : Vector3.left;

        GameObject newRocket1 = RocketFactory.CreateAnimatedRocket("RocketAnim1", blastedRocketX, blastedRocketY, rotation1, sortingOrder, rocketSprite);
        GameObject newRocket2 = RocketFactory.CreateAnimatedRocket("RocketAnim2", blastedRocketX, blastedRocketY, rotation2, sortingOrder, rocketSprite);

        newRocket1.transform.DOMove(newRocket1.transform.position + moveDirection * moveDistance, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(newRocket1));

        newRocket2.transform.DOMove(newRocket2.transform.position - moveDirection * moveDistance, moveDuration)
            .SetEase(Ease.OutQuad)
            .OnComplete(() => Destroy(newRocket2));

        return moveDuration;
    }

    private void SpecialObjectCollapse(GameObject blastedObject, float touchedObjectX, float touchedObjectY)
    {
        float blastedObjectX = blastedObject.transform.position.x;
        float blastedObjectY = blastedObject.transform.position.y;

        float offsetX = (blastedObjectX - touchedObjectX)/6 * pushStrengthX;
        float offsetY = (blastedObjectY - touchedObjectY)/4.5f * pushStrengthY;

        Vector3 pushVector = new Vector3(offsetX, offsetY, 0f);

        blastedObject.GetComponent<SpriteRenderer>().sortingOrder += 4;
        blastedObject.transform.DOMove(blastedObject.transform.position + pushVector, pushDuration).OnComplete(() =>
        {
            blastedObject.transform.DOMove(new Vector3(touchedObjectX, touchedObjectY, 0), returnDuration).OnComplete(() =>
            {
                Destroy(blastedObject); 
            });
        });
    }

    private void DefaultObjectCollapse(GameObject blastedObject, Sprite[] particleSprites)
    {
        Vector3 targetScale = defaultObjectCollapseScale;
        float scaleDuration = defaultObjectCollapseDuration;
        float x = blastedObject.transform.position.x;
        float y = blastedObject.transform.position.y;

        blastedObject.transform.DOScale(targetScale, scaleDuration)
            .SetLoops(1, LoopType.Yoyo)
            .OnComplete(() =>
            {
                Destroy(blastedObject);
                if (blastedObject.GetComponent<GridEntity>().IsCube())
                {
                    for (int i = 0; i < particleSprites.Length; i++)
                    {
                        CreateParticle(x, y, particleSprites[i], GetOffsetForIndex(i));
                    }
                }
            });
    }

    private Vector2 GetOffsetForIndex(int i)
    {
        return i switch
        {
            0 => new Vector2(-1, 1),
            1 => new Vector2(1, 1),
            2 => new Vector2(0, 0),
            3 => new Vector2(-1, -1),
            4 => new Vector2(1, -1),
            _ => Vector2.zero,
        };
    }


    private void CreateParticle(float x, float y, Sprite particleSprite, Vector2 shatterDirection)
    {
        GameObject particle = ParticleFactory.CreateParticleObject(x, y, particleObstacleScale, particleSprite);

        float randomFallDistance = Random.Range(particleObstacleRandomFallDistance.x, particleObstacleRandomFallDistance.y);
        float randomDuration = Random.Range(particleObstacleRandomDuration.x, particleObstacleRandomDuration.y);
        float randomRotation = Random.Range(0f, 360f);

        Sequence particleSequence = DOTween.Sequence();

        particleSequence.Append(particle.transform.DOMove(new Vector3(x + shatterDirection.x / 4, y + shatterDirection.y / 4, 0), particleObstacleLiftDuration)
            .SetEase(Ease.OutCubic))
            .OnComplete(() =>
            {
                particle.transform.DOMove(new Vector3(x + shatterDirection.x, y - randomFallDistance, 0), randomDuration)
                    .SetEase(Ease.InCubic);

                particle.transform.DORotate(new Vector3(0, 0, randomRotation * 2), randomDuration, RotateMode.FastBeyond360).OnComplete(() =>
                {
                    particle.GetComponent<SpriteRenderer>().DOFade(0f, 0.3f);
                    Destroy(particle, randomDuration + 0.3f);
                });
            });
    }

    public float GetSpecialDestroyAnimDuration()
    {
        return pushDuration + returnDuration;
    }
    public float GetObjectDropDuration()
    {
        return objectDropDuration;
    }
    
}
