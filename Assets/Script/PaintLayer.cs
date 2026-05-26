using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// 1つのレイヤーのデータを保持するクラス
[System.Serializable]
public class PaintLayer
{
    public string name;
    public RenderTexture texture;
    [Range(0f, 1f)] public float opacity = 1f;
    public bool isVisible = true;

    public PaintLayer(string name, int width, int height)
    {
        this.name = name;
        // 各レイヤーは「透明な白」で初期化する
        texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        texture.Create();
        ClearTexture(texture);
    }

    private void ClearTexture(RenderTexture rt)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
        RenderTexture.active = active;
    }

    public void Release()
    {
        if (texture != null) texture.Release();
    }
}