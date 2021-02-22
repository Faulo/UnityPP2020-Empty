using TMPro;
using UnityEngine;

public class CanvasController : MonoBehaviour
{
    public TextMeshProUGUI text;

    // Start is called before the first frame update
    private void Start()
    {
        text.text = "Hallo Welt!";
    }

    // Update is called once per frame
    private void Update()
    {

    }
}
