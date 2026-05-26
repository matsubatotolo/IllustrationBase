using Unity.VisualScripting;
using UnityEngine;
// ⚠️ 筆圧取得のために新インプットシステムを使用します。
// Unityの独自Input設定（Input Systemパッケージ）が有効であることを確認してください。
using UnityEngine.InputSystem; 

public class CanvasPainter : MonoBehaviour
{
    //public RenderTexture canvasTexture; // 描き込み先のレンダーテクスチャ
    //public DrawingCanvas canvas;

    // ★レイヤーマネージャーへの参照を追加します（インスペクターでセットするか、Findする）
    public LayerManager layerManager;

    public Material brushMaterial;      // URP_Simple（アプローチA）のシェーダーをセットしたマテリアル
    
    [Header("Brush Settings")]
    public float maxBrushSize = 50f;    // 筆圧が最大のときのブラシの直径（ピクセル）
    public float minSizePercent = 0.1f; // 筆圧がゼロに近いときの最小サイズ比率（0.1 = 最大の10%の太さ）
    [Range(0, 1)]
    public float maxOpacity = 1.0f;     // 最大の不透明度

    [Header("Painting Quality")]
    [Tooltip("値を小さくするほど滑らかになりますが、処理が重くなります。0.1〜0.2が理想（サイズに対して10〜20%の間隔）")]
    public float brushSpacing = 0.1f; // ★ブラシの間隔比率

    private Mesh quadMesh;

    // ★前回の状態を記憶するための変数
    private Vector2? lastPixelCoords = null;
    private float lastPressure = 1.0f;

    void Start()
    {
        // 1x1 のシンプルな四角形メッシュを作成
        quadMesh = CreateQuadMesh();
    }

    void Update()
    {
        // 開発中のテスト用：マウスまたはペンが画面にタッチしているかチェック
        //if (Pointer.current != null && Pointer.current.press.isPressed)
        if (Pen.current != null && Pen.current.tip.isPressed)
        {
            // 現在のスクリーン座標（ピクセル）を取得
            //Vector2 screenPos = Pointer.current.position.ReadValue();
            Vector2 screenPos = Pen.current.position.ReadValue();

            // スクリーン座標をレンダーテクスチャのピクセル座標に変換する処理
            // (※メインカメラから見たキャンバスの配置に合わせて調整してください)
            Vector2 currentPixelCoords = ConvertScreenToTexturePixels(screenPos);

            // 筆圧の取得（デフォルトは 1.0f）
            float currentPressure = 1.0f;

            // ペンデバイス（Wacom等）が接続されていれば、そこからリアルタイムな筆圧（0.0 〜 1.0）を取得
            //if (Pen.current != null)
            //{
                currentPressure = Pen.current.pressure.ReadValue();
            //}

            // ★【変更点①】LayerManager から「現在選択されているレイヤーのテクスチャ」を毎フレーム取得する
            RenderTexture targetLayer = layerManager.GetActiveLayerTexture();
            if (targetLayer == null) return;

            // 筆圧を適用して描画を実行！
            //PaintAtPositionWithPressure(pixelCoords, pressure);
            // ★描き始め（最初の1点目）か、2点目以降（線を描いている最中）かで処理を分ける
            if (lastPixelCoords == null)
            {
                // 最初の一歩はそのまま描画
                //PaintAtPositionWithPressure(currentPixelCoords, currentPressure);

                // ★【変更点②】引数に targetLayer を追加
                PaintAtPositionWithPressure(targetLayer, currentPixelCoords, currentPressure);
            }
            else
            {
                // 2手目以降は、前回の位置からの隙間を埋める（補間描画）
                //PaintLineInterpolated(lastPixelCoords.Value, currentPixelCoords, lastPressure, currentPressure);
                // ★【変更点③】引数に targetLayer を追加
                PaintLineInterpolated(targetLayer, lastPixelCoords.Value, currentPixelCoords, lastPressure, currentPressure);
            }
            // ★【変更点④】現在のレイヤーに描き込みが終わったので、全体の画面を再合成してRawImageに反映する
            layerManager.UpdateCanvasDisplay();

            // 今回の位置と筆圧を「前回のデータ」として保存
            lastPixelCoords = currentPixelCoords;
            lastPressure = currentPressure;
        }
        else
        {
            // ペンが画面から離れたら、前回の位置をリセットする
            lastPixelCoords = null;
        }

    }

    // ★点と点の間を補間して描画する関数
    //private void PaintLineInterpolated(Vector2 start, Vector2 end, float startPressure, float endPressure)
    // ★【変更点⑤】第1引数に RenderTexture target を追加
    private void PaintLineInterpolated(RenderTexture target, Vector2 start, Vector2 end, float startPressure, float endPressure)
    {
        Debug.Log("CanvasPainter.PaintLineInterpolated");

        // 2点間の距離を計算（ピクセル単位）
        float distance = Vector2.Distance(start, end);

        // 今回の平均的なブラシサイズを基準に、どれくらいの間隔（ピクセル）でスタンプを押すか決める
        float avgPressure = (startPressure + endPressure) * 0.5f;
        float currentSize = Mathf.Lerp(maxBrushSize * minSizePercent, maxBrushSize, avgPressure);
        float stepDistance = currentSize * brushSpacing; // 例: サイズが50pxでSpacingが0.1なら、5pxごとに描画

        // 描画する回数（スタンプの数）を計算
        int steps = Mathf.Max(1, Mathf.FloorToInt(distance / stepDistance));

        // 描画先の設定を一括で行う（ループの外に出すことで高速化）
        //RenderTexture.active = /*canvasTexture*/canvas.renderTexture;
        // ★【変更点⑥】固定の canvasTexture ではなく、引数で受け取った target をアクティブにする
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = target;

        GL.PushMatrix();
        //GL.LoadPixelMatrix(0, /*canvasTexture*/canvas.renderTexture.width, 0, /*canvasTexture*/canvas.renderTexture.height);
        // ★ここも target のサイズに合わせる
        GL.LoadPixelMatrix(0, target.width, 0, target.height);

        // 隙間を Lerp（線形補間）しながら連続で描画
        for (int i = 1; i <= steps; i++)
        {
            float t = (float)i / steps;

            // 座標と筆圧をスタートからエンドに向かって徐々に変化させる
            Vector2 interpolatedPos = Vector2.Lerp(start, end, t);
            float interpolatedPressure = Mathf.Lerp(startPressure, endPressure, t);

            // 各ステップでのサイズと不透明度
            float size = Mathf.Lerp(maxBrushSize * minSizePercent, maxBrushSize, interpolatedPressure);
            float opacity = maxOpacity * interpolatedPressure;

            // シェーダーに透明度をセット
            brushMaterial.SetFloat("_Opacity", opacity);

            // 行列を作ってメッシュを描画
            Vector3 pos = new Vector3(interpolatedPos.x, interpolatedPos.y, 0);
            Vector3 scale = new Vector3(size, size, 1);
            Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);

            brushMaterial.SetPass(0);
            Graphics.DrawMeshNow(quadMesh, matrix);
        }

        // 後片付け
        GL.PopMatrix();
        RenderTexture.active = null;
    }

    //public void PaintAtPositionWithPressure(Vector2 pixelCoords, float pressure)
    // ★【変更点⑦】第1引数に RenderTexture target を追加
    public void PaintAtPositionWithPressure(RenderTexture target, Vector2 pixelCoords, float pressure)
    {
        Debug.Log("CanvasPainter.PaintAtPositionWithPressure");

        //if (canvas.renderTexture == null || brushMaterial == null) return;
        if (target == null || brushMaterial == null) return;

        // 1. 筆圧に応じてサイズと不透明度を計算
        // 筆圧が弱くても、完全に消えないように最小サイズ（minSizePercent）を設定
        float currentSize = Mathf.Lerp(maxBrushSize * minSizePercent, maxBrushSize, pressure);
        float currentOpacity = maxOpacity * pressure;

        // 2. マテリアル（シェーダー）に今回の不透明度をリアルタイムに送る
        brushMaterial.SetFloat("_Opacity", currentOpacity);

        // 3. 描画先（キャンバス）をアクティブにする
        //RenderTexture.active = canvas.renderTexture;
        // ★【変更点⑧】引数で受け取った target をアクティブにする
        RenderTexture active = RenderTexture.active;
        RenderTexture.active = target;

        // 4. 2D描画用の正投影（Ortho）マトリクスを設定（ピクセル単位で指定可能にする）
        GL.PushMatrix();

        //GL.LoadPixelMatrix(0,canvas.renderTexture.width, 0, canvas.renderTexture.height);
        GL.LoadPixelMatrix(0, target.width, 0, target.height);

        // 5. ペン位置（中心）に、筆圧計算後のサイズ（currentSize）でリサイズした行列を作成
        Vector3 pos = new Vector3(pixelCoords.x, pixelCoords.y, 0);
        Vector3 scale = new Vector3(currentSize, currentSize, 1);
        Matrix4x4 matrix = Matrix4x4.TRS(pos, Quaternion.identity, scale);

        // 6. マテリアルを適用してメッシュをその場所に描画
        brushMaterial.SetPass(0);
        Graphics.DrawMeshNow(quadMesh, matrix);

        GL.PopMatrix();
        RenderTexture.active = null;
    }

    // スクリーン座標をレンダーテクスチャ上のピクセル座標にマッピングする簡易関数
    // ※画面全体にキャンバスが表示されている前提の計算です。UIの配置に合わせてカスタマイズしてください。
    private Vector2 ConvertScreenToTexturePixels(Vector2 screenPos)
    {
        float xNorm = screenPos.x / Screen.width;
        float yNorm = screenPos.y / Screen.height;

        //return new Vector2(xNorm * canvas.renderTexture.width, yNorm * canvas.renderTexture.height);
        var target = layerManager.GetActiveLayerTexture();
        return new Vector2(xNorm * target.width, yNorm * target.height);
        
    }

    // 1x1のシンプルな四角形を作るヘルパー関数
    private Mesh CreateQuadMesh()
    {
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, -0.5f, 0),
            new Vector3( 0.5f, -0.5f, 0),
            new Vector3(-0.5f,  0.5f, 0),
            new Vector3( 0.5f,  0.5f, 0)
        };
        mesh.uv = new Vector2[] {
            new Vector2(0, 0),
            new Vector2(1, 0),
            new Vector2(0, 1),
            new Vector2(1, 1)
        };
        mesh.triangles = new int[] { 0, 2, 1, 1, 2, 3 };
        return mesh;
    }
}