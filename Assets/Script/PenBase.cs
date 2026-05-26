using UnityEngine;
using UnityEngine.InputSystem;

public class PenBase : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }
    void Update()
    {
        if (Pen.current != null)
        {
            float pressure = Pen.current.pressure.ReadValue();
            Vector2 position = Pen.current.position.ReadValue();  // 座標
            Vector2 tilt = Pen.current.tilt.ReadValue();           // 傾き
            float twist = Pen.current.twist.ReadValue();           // 回転（対応デバイスのみ）            
            float inRange = Pen.current.inRange.ReadValue();         // ペンが近くにある
            float tip = Pen.current.tip.ReadValue();             // ペンが接触してるか

            Debug.Log($"tip: {tip}");
            Debug.Log($"position: {position}");
            Debug.Log($"inRange: {inRange}");
            if (pressure > 0f)
            {
                Debug.Log($"Pressure: {pressure}");                
                Debug.Log($"tilt: {tilt}");
                Debug.Log($"twist: {twist}");
                                
            }
        }
    }

}
