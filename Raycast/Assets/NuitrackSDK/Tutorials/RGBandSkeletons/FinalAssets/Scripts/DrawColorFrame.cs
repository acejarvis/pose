using UnityEngine;
using UnityEngine.UI;

using NuitrackSDK.Frame;

public class DrawColorFrame : MonoBehaviour
{
    [SerializeField] RawImage background;

    void Start()
    {
        NuitrackManager.onColorUpdate += DrawColor;
    }

    void DrawColor(nuitrack.ColorFrame frame)
    {
        background.texture = frame.ToTexture2D();
    }

    void Destroy()
    {
        NuitrackManager.onColorUpdate -= DrawColor;
    }
}
