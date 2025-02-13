using UnityEngine;

public static class ParticleFactory
{
    public static GameObject CreateParticleObject(float x, float y, float scale, Sprite sprite)
    {
        GameObject particle = new($"Particle_{sprite.name}");
        particle.transform.position = new Vector3(x, y, 0);
        SpriteRenderer spriteRenderer = particle.AddComponent<SpriteRenderer>();
        spriteRenderer.sprite = sprite;
        spriteRenderer.sortingOrder = 12;
        particle.transform.localScale = new Vector3(scale, scale, 1f);
        return particle;
    }
}
