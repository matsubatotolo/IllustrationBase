using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PaintLayer;

public class LayerManager : MonoBehaviour
{
    public RectTransform canvasRect;
    public Material compositeMaterial; // ★後述のレイヤー合成用マテリアル
    public RawImage canvasDisplay;     // 画面に最終結果を表示するUI
    [SerializeField]
    private LayerListPanel layerListPanel;

    private List<PaintLayer> layers = new List<PaintLayer>();
    private int activeLayerIndex = 0;
    private RenderTexture finalCombinedTexture; // 最終出力用

    void Start()
    {
        int w = (int)canvasRect.rect.size.x;
        int h = (int)canvasRect.rect.size.y;

        // 最終合成用のテクスチャを作成
        finalCombinedTexture = new RenderTexture(w, h, 0, RenderTextureFormat.ARGB32);
        finalCombinedTexture.Create();
        canvasDisplay.texture = finalCombinedTexture;

        // 初期レイヤーを2つ作成（例: 背景レイヤー、線画レイヤー）
        AddNewLayer("Background", LayerType.Background);
        AddNewLayer("LineArt", LayerType.Normal);

        activeLayerIndex = 1; // 最初は上の「LineArt」を選択状態にする

        UpdateCanvasDisplay();
    }
    public void AddNewLayer(string name, LayerType layerType)
    {
        int w = (int)canvasRect.rect.size.x;
        int h = (int)canvasRect.rect.size.y;
        layers.Add(new PaintLayer(name, w, h, layerType));
        layerListPanel.AddLayerUnit(name, layerType);
    }

    public void AddNewLayer()
    {
        AddNewLayer("New Layer", LayerType.Normal);
    }

    // ★重要: CanvasPainterからはこの関数経由で「現在のレイヤー」に対して描画させる
    public RenderTexture GetActiveLayerTexture()
    {
        if (layers.Count == 0) return null;
        return layers[activeLayerIndex].texture;
    }

    // レイヤーの順序や不透明度が変わったとき、または毎フレームの終わりに呼ぶ
    public void UpdateCanvasDisplay()
    {
        if (layers.Count == 0) return;

        // 最初（一番下）のレイヤーで最終バッファを初期化
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = finalCombinedTexture;

        // 背景を透明な白でクリア
        GL.Clear(true, true, new Color(1f, 1f, 1f, 0f));
        RenderTexture.active = active;

        // 1枚目のレイヤーをコピー（下地）
        if (layers[0].isVisible)
        {
            compositeMaterial.SetFloat("_LayerOpacity", layers[0].opacity);
            Graphics.Blit(layers[0].texture, finalCombinedTexture, compositeMaterial, 0); // Pass 0: 最初のコピー
        }

        // 2枚目以降のレイヤーを下から順番に上に重ねていく（ブレンド）
        for (int i = 1; i < layers.Count; i++)
        {
            if (!layers[i].isVisible) continue;

            // シェーダーに重ねるレイヤーのテクスチャと不透明度を送る
            compositeMaterial.SetTexture("_OverlayTex", layers[i].texture);
            compositeMaterial.SetFloat("_LayerOpacity", layers[i].opacity);

            // 現在の合成結果の上に、新しいレイヤーをブレンドして上書き
            // RenderTextureは自分自身に直接Blitできないため、一時的なバッファを挟むのが安全です
            RenderTexture temp = RenderTexture.GetTemporary(finalCombinedTexture.width, finalCombinedTexture.height, 0, finalCombinedTexture.format);

            Graphics.Blit(finalCombinedTexture, temp, compositeMaterial, 1); // Pass 1: ブレンド合成
            Graphics.Blit(temp, finalCombinedTexture); // 結果を戻す

            RenderTexture.ReleaseTemporary(temp);
        }
    }

    public void SetLayerVisible(int index, bool visible)
    {
        if (index < 0 || index >= layers.Count) return;
        layers[index].isVisible = visible;
        UpdateCanvasDisplay();
    }

    private void OnDestroy()
    {
        foreach (var layer in layers) layer.Release();
        if (finalCombinedTexture != null) finalCombinedTexture.Release();
    }

    public RenderTexture GetFinalCombinedTexture()
    {
        return /*finalCombinedTexture*/layers[1].texture; 
    }
}