using System;
using System.Drawing;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class PenColorManager : MonoBehaviour
{
    [SerializeField]
    private CanvasPainter canvasPainter = null;

    [SerializeField] private Material penMaterial = null;
    [SerializeField] private TMP_InputField inputFieldR;
    [SerializeField] private TMP_InputField inputFieldG;
    [SerializeField] private TMP_InputField inputFieldB;
    [SerializeField] private Image penColorImage;

    //[SerializeField, Range(0f, 1f)] private float _hardness = 1f;
    //[SerializeField, Range(0f, 1f)] private float _opacity = 1f;

    [SerializeField] private TMP_InputField inputFieldPenSize;    
    [SerializeField] private TMP_InputField inputFieldOpacity;
    [SerializeField] private TMP_InputField inputFieldHardness;
        
    private static readonly int HardnessID = Shader.PropertyToID("_Hardness");

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Initialize();
    }
    public void Initialize()
    {
        Color32 penMaterialColor = (Color32)penMaterial.color;

        inputFieldR.text = penMaterialColor.r.ToString();
        inputFieldG.text = penMaterialColor.g.ToString();
        inputFieldB.text = penMaterialColor.b.ToString();
        penColorImage.color = penMaterial.color;

        float _hardness = PlayerPrefs.GetFloat("PenHardness");
        inputFieldHardness.text = _hardness.ToString();
        penMaterial.SetFloat(HardnessID, _hardness);

        float _maxOpacity = PlayerPrefs.GetFloat("PenMaxOpacity");
        canvasPainter.maxOpacity = _maxOpacity;
        inputFieldOpacity.text = _maxOpacity.ToString();

        float _size = PlayerPrefs.GetFloat("PenMaxBrushSize");
        canvasPainter.maxBrushSize = _size;
        inputFieldPenSize.text = _size.ToString();
    }

    public void SetPenColor()
    {
        int colorR = Int32.Parse(inputFieldR.text);
        int colorG = Int32.Parse(inputFieldG.text);
        int colorB = Int32.Parse(inputFieldB.text);
        penMaterial.color = new Color32((byte)colorR, (byte)colorG, (byte)colorB, 255);
        penColorImage.color = penMaterial.color;

        //PlayerPrefs.SetInt("ColorR", colorR);
        //PlayerPrefs.SetInt("ColorG", colorG);
        //PlayerPrefs.SetInt("ColorB", colorB);
    }

    public void SetHardness()
    {
        float _hardness = float.Parse(inputFieldHardness.text);
        penMaterial.SetFloat(HardnessID, _hardness);

        PlayerPrefs.SetFloat("PenHardness", _hardness);
    }

    public void SetOpacity()
    {
        canvasPainter.maxOpacity = float.Parse(inputFieldOpacity.text);

        PlayerPrefs.SetFloat("PenMaxOpacity", canvasPainter.maxOpacity);
    }

    public void SetSize()
    {
        canvasPainter.maxBrushSize = float.Parse(inputFieldPenSize.text);

        PlayerPrefs.SetFloat("PenMaxBrushSize", canvasPainter.maxBrushSize);
    }
}
