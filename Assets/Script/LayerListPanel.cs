using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static PaintLayer;

/// <summary>
/// LayerUnit を縦に並べるスクロール可能なレイヤーリストパネル。
/// </summary>
public class LayerListPanel : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private RectTransform content;
    [SerializeField] private GameObject layerUnitPrefab;
    [SerializeField] private LayerManager layerManager;

    private readonly List<GameObject> unitInstances = new List<GameObject>();   

    /// <summary>リストの末尾に LayerUnit を1つ追加する。</summary>
    public GameObject AddLayerUnit(string layerName, LayerType type)
    {
        var unit = Instantiate(layerUnitPrefab, content);
        unit.transform.SetAsFirstSibling();
        LayerUIUnit layerUIUnit = unit.GetComponent<LayerUIUnit>();
        if (layerUIUnit != null)
        {
            layerUIUnit.SetLayerIndex(GetLayerIndex());
            layerUIUnit.SetLayerName(layerName);
            layerUIUnit.SetLayerVisibleFlag(true);
            layerUIUnit.Initialize(layerManager);
        }
        unitInstances.Add(unit);
        
        return unit;
    }

    /// <summary>指定インデックスの LayerUnit を削除する。</summary>
    public void RemoveLayerUnit(int index)
    {
        if (index < 0 || index >= unitInstances.Count) return;
        Destroy(unitInstances[index]);
        unitInstances.RemoveAt(index);
    }

    /// <summary>すべての LayerUnit を削除する。</summary>
    public void ClearAll()
    {
        foreach (var unit in unitInstances)
            if (unit != null) Destroy(unit);
        unitInstances.Clear();
    }

    public int Count => unitInstances.Count;

    public GameObject GetUnit(int index) =>
        (index >= 0 && index < unitInstances.Count) ? unitInstances[index] : null;

    private int GetLayerIndex()
    {
        int newIndex = -1;
        foreach (var instance in unitInstances)
        {
            var layerUIUnit = instance.GetComponent<LayerUIUnit>();
            if(layerUIUnit.layerIndex > newIndex)
                newIndex = layerUIUnit.layerIndex;
        }
        return newIndex + 1;
    }
}
