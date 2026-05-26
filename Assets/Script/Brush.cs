using UnityEngine;

[System.Serializable]
public class Brush
{
    public Texture2D texture;
    public Color color = Color.black;

    public float size = 32f;
    public float hardness = 0.8f;
    public float opacity = 1.0f;

    public Material material;

    public void Apply(Vector2 uv, float pressure, RenderTexture target)
    {
        if (material == null) return;

        // 筆圧反映
        float finalSize = size * Mathf.Lerp(0.5f, 1.5f, pressure);
        float finalOpacity = opacity * pressure;

        material.SetColor("_Color", color);
        material.SetFloat("_Opacity", finalOpacity);
        material.SetFloat("_Hardness", hardness);

        material.SetVector("_BrushPos",
            new Vector4(uv.x, uv.y,
            finalSize / target.width,
            finalSize / target.height));

        RenderTexture temp = RenderTexture.GetTemporary(target.width, target.height);

        Graphics.Blit(target, temp);
        Graphics.Blit(temp, target, material);

        RenderTexture.ReleaseTemporary(temp);
    }
}