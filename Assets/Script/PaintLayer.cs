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

    public enum LayerType
    {
        Background,
        Normal,
        Multiply,
        Screen,
        Overlay
    }

    public PaintLayer(string name, int width, int height, LayerType type)
    {
        this.name = name;
        // 各レイヤーは「透明な白」で初期化する
        texture = new RenderTexture(width, height, 0, RenderTextureFormat.ARGB32);
        texture.Create();
        ClearTexture(texture, type);
    }

    private void ClearTexture(RenderTexture rt, LayerType type)
    {
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = rt;
        if(type == LayerType.Background)
            GL.Clear(true, true, new Color(1f, 1f, 1f, 1f));
        else
            GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
        //GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
        RenderTexture.active = active;
    }

    public void Release()
    {
        if (texture != null) texture.Release();
    }
}