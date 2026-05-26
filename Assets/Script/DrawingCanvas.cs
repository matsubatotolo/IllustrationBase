using UnityEngine;

public class DrawingCanvas : MonoBehaviour
{
    public RenderTexture renderTexture;
    public RectTransform canvasRect;

    //void Awake()
    //{
    //    renderTexture = new RenderTexture((int)canvasRect.rect.size.x, (int)canvasRect.rect.size.y, 0, RenderTextureFormat.ARGB32);
    //    renderTexture.enableRandomWrite = false;
    //    renderTexture.Create();

    //    Clear(Color.white);
    //}

    //public void Clear(Color color)
    //{
    //    RenderTexture active = RenderTexture.active;
    //    RenderTexture.active = renderTexture;

    //    Texture2D tex = new Texture2D(1, 1);
    //    tex.SetPixel(0, 0, color);
    //    tex.Apply();

    //    Graphics.Blit(tex, renderTexture);

    //    Destroy(tex);

    //    RenderTexture.active = active;
    //}
    void Awake()
    {
        renderTexture = new RenderTexture((int)canvasRect.rect.size.x, (int)canvasRect.rect.size.y, 0, RenderTextureFormat.ARGB32);
        renderTexture.enableRandomWrite = false;
        renderTexture.Create();

        // ★重要: 見た目は「白」だけど、中身は「透明（アルファ0）」でクリアする
        Clear(new Color(1f, 1f, 1f, 1f));
    }

    public void Clear(Color color)
    {
        if (renderTexture == null) return;

        // 現在アクティブなRenderTextureを退避
        RenderTexture active = RenderTexture.active;

        // 操作対象のRenderTextureをアクティブにする
        RenderTexture.active = renderTexture;

        // ★【修正のキモ】GL.Clear を使う
        // 第1引数: Zバッファをクリアするか（2Dペイントなので一応trueでOK）
        // 第2引数: カラーバッファをクリアするか（必ずtrue）
        // 第3引数: クリアする色（アルファ値も100%正確に書き込まれます）
        GL.Clear(true, true, color);

        // アクティブな情報を元に戻す
        RenderTexture.active = active;
    }
}