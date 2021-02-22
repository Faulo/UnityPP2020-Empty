using TMPro;
using UnityEngine;

public class HUDController : MonoBehaviour {
    [SerializeField]
    TextMeshProUGUI coinText = default;
    [SerializeField]
    TextMeshProUGUI deathText = default;

    RobotController avatar;

    void Start() {
        avatar = FindObjectOfType<RobotController>();
    }

    void Update() {
        coinText.text = $"Coins collected: {avatar.currentCoinCount}/{avatar.maximumCoinCount}";
        deathText.text = $"Deaths: {avatar.deathCount}";
    }
}
