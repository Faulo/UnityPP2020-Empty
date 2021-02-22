using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI coinText = default;
    [SerializeField]
    private TextMeshProUGUI deathText = default;
    private RobotController avatar;

    private void Start()
    {
        avatar = FindObjectOfType<RobotController>();
    }

    private void Update()
    {
        coinText.text = $"Coins collected: {avatar.currentCoinCount}/{avatar.maximumCoinCount}";
        deathText.text = $"Deaths: {avatar.deathCount}";
    }
}
