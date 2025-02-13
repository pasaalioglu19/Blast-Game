using UnityEngine;

public static class RocketFactory
{
    public static GameObject CreateAnimatedRocket(string name, float x, float y, float rotation, int sortingOrder, Sprite rocketSprite)
    {
        GameObject rocket = new GameObject(name);
        rocket.transform.position = new Vector3(x, y, 0);
        rocket.transform.localScale = new Vector3(0.5f, 0.5f, 1f);
        rocket.transform.rotation = Quaternion.Euler(0, 0, rotation);

        SpriteRenderer spriteRenderer = rocket.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = rocketSprite;
        spriteRenderer.sortingOrder = sortingOrder;

        return rocket;
    }
}
