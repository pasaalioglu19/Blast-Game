using DG.Tweening;
using UnityEngine;

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

    public void SwapObject(GameObject obj1, GameObject obj2, Vector3 pos1, Vector3 pos2)
    {
        // DoTween ile animasyonlu hareket (eþ zamanlý olarak)
        Sequence swapSequence = DOTween.Sequence();

        swapSequence.Append(obj1.transform.DOMove(pos1, 0.5f).SetEase(Ease.InOutQuad));
        swapSequence.Join(obj2.transform.DOMove(pos2, 0.5f).SetEase(Ease.InOutQuad));
    }
}
