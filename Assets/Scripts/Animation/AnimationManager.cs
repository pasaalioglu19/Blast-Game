using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class AnimationManager : MonoBehaviour
{
    [Header("Drop Animation Settings")]
    [SerializeField] private float oneUnitDropDuration = 0.1f;

    public void DropObject(GameObject dropObject, Vector3 position, int distance)
    {
        float dropDuration = distance * oneUnitDropDuration;
        dropObject.transform.DOMove(position, dropDuration)
            .SetEase(Ease.Linear);
    }
}
