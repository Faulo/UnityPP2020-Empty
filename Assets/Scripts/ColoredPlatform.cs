using UnityEngine;

public class ColoredPlatform : MonoBehaviour
{
    public Color platformColor;
    public Renderer attachedRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        if (attachedRenderer)
        {
            attachedRenderer.material.SetColor("_BaseColor", platformColor);
        }
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
