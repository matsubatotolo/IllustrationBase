using UnityEditor.U2D.Aseprite;
using UnityEngine;
using UnityEngine.UI;
using static PaintLayer;

public class LayerUIUnit : MonoBehaviour
{
    public int layerIndex { get; private set; } = 0;
    public bool layerVisibleFlag { get; private set; } = false;
    public string layerName { get; private set; }

    private PaintLayer.LayerType layerType;

    private LayerManager layerManager;

    [SerializeField]
    private GameObject layerIconObjNormal = null;
    [SerializeField]
    private GameObject layerIconObjBg = null;

    [SerializeField]
    public Image backgroundImage;

    [SerializeField]
    private Color selectedColor;
    [SerializeField]
    private Color notSelectedColor;

    public void SetLayerIndex(int index)
    {
        this.layerIndex = index;
        if (this.layerIndex == 0)
            SetLayerIcon(LayerType.Background);
        else if (this.layerIndex > 0)
            SetLayerIcon(LayerType.Normal);
    }

    public void SetLayerVisibleFlag(bool visible)
    {
        this.layerVisibleFlag = visible;
    }

    public void SetLayerName(string name)
    {
        this.layerName = name;
    }

    /// <summary>
    /// LayerManager の参照をセットし、Toggle の onValueChanged を購読する。
    /// LayerListPanel.AddLayerUnit() 内で呼ぶこと。
    /// </summary>
    public void Initialize(LayerManager manager)
    {
        layerManager = manager;

        Toggle toggle = GetComponentInChildren<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = layerVisibleFlag;
            toggle.onValueChanged.AddListener(OnToggleChanged);
        }
    }

    private void OnToggleChanged(bool isOn)
    {
        layerVisibleFlag = isOn;
        layerManager?.SetLayerVisible(layerIndex, isOn);
    }

    public void OnButtonClicked()
    {
        Debug.Log("Layer Selected, layerIndex = "+ layerIndex);
        if (layerIndex > 0)
        {
            layerManager?.SetActiveLayerIndex(layerIndex);
            layerManager?.SetUnitBackgroundColor(layerIndex);
        }
    }

    public void SetLayerIcon(PaintLayer.LayerType layerType) 
    {
        if (layerType == LayerType.Background)
        {
            layerIconObjNormal.SetActive(false);
            layerIconObjBg.SetActive(true);
        }
        else if (layerType == LayerType.Normal)
        {
            layerIconObjNormal.SetActive(true);
            layerIconObjBg.SetActive(false);
        }

    }

    public void SetBackgroundColor(int index)
    { 
        if(index == layerIndex)
            backgroundImage.color = selectedColor;
        else
            backgroundImage.color = notSelectedColor;
    }

    private void OnDestroy()
    {
        Toggle toggle = GetComponentInChildren<Toggle>();
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
