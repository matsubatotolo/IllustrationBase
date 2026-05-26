using UnityEngine;
using System.IO; // ファイル書き込みに使用します

public class CanvasSaver : MonoBehaviour
{
    //public RenderTexture canvasTexture; // 保存したいレンダーテクスチャ
    public DrawingCanvas canvas;

    // UIのボタンなどからこの関数を呼び出します
    public void SaveCanvasAsPNG()
    {
        if (canvas.renderTexture == null)
        {
            Debug.LogError("保存するRenderTextureが設定されていません。");
            return;
        }

        // 1. RenderTextureと同じサイズ、フォーマットの Texture2D を作成
        Texture2D texture2D = new Texture2D(canvas.renderTexture.width, canvas.renderTexture.height, TextureFormat.RGBA32, false);

        // 2. 読み取り元としてRenderTextureをアクティブにする
        RenderTexture.active = canvas.renderTexture;

        // 3. RenderTextureのピクセル情報をTexture2Dにコピー
        // (0, 0)からテクスチャ全画面分を読み込みます
        texture2D.ReadPixels(new Rect(0, 0, canvas.renderTexture.width, canvas.renderTexture.height), 0, 0);
        texture2D.Apply(); // 変更を確定

        // 後片付け（アクティブを解除）
        RenderTexture.active = null;

        // 4. Texture2D を PNG 形式のバイト配列に変換
        // ※透明度を無視してJPGで保存したい場合は texture2D.EncodeToJPG(); を使います
        byte[] bytes = texture2D.EncodeToPNG();

        // 不要になったメモリ上のTexture2Dを破壊してクリーンアップ
        Destroy(texture2D);

        // 5. 保存先のパス（ファイル名）を設定
        // ここでは「MySketch_連番.png」のような名前で保存します
        string fileName = "MySketch_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";

        // エディタ環境やPCビルドならプロジェクトフォルダ直下、モバイルならそれぞれのセーブ領域
        string savePath = Path.Combine(Application.persistentDataPath, fileName);

        // 6. 実際にファイルとして書き出し
        File.WriteAllBytes(savePath, bytes);

        Debug.Log("絵を保存しました！ パス: " + savePath);
    }
}