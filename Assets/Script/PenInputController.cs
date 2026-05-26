using UnityEngine;
using UnityEngine.InputSystem;

public class PenInputController : MonoBehaviour
{
    public DrawManager drawManager;

    private Vector2 lastPos;
    private bool drawing = false;

    void Update()
    {
        if (Pen.current != null)
        {
            Vector2 pos = Pen.current.position.ReadValue();
            float pressure = Pen.current.pressure.ReadValue();
            bool pressed = Pen.current.tip.isPressed;

            if (pressed)
            {
                if (!drawing)
                {
                    drawing = true;
                    lastPos = pos;
                }

                drawManager.DrawLine(lastPos, pos, pressure);
                lastPos = pos;
            }
            else
            {
                drawing = false;
            }
        }
    }
}