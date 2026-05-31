using UnityEngine;
using UnityEngine.UI;

public class LayerUIUnit : MonoBehaviour
{
    public int layerIndex { get; private set; } = 0;
    public bool layerVisibleFlag { get; private set; } = false;
    public string layerName { get; private set; }

    private LayerManager layerManager;

    public void SetLayerIndex(int index)
    {
        this.layerIndex = index;
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

    private void OnDestroy()
    {
        Toggle toggle = GetComponentInChildren<Toggle>();
        if (toggle != null)
            toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }
}
