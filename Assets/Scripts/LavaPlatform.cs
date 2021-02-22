using System.Collections;
using UnityEngine;

public class LavaPlatform : MonoBehaviour {
    [SerializeField]
    ParticleSystem particlePrefab = default;
    [SerializeField]
    float deathDuration = 1;
    [SerializeField]
    float respawnDuration = 1;

    RobotController avatar;
    Vector3 startPosition;
    CameraController avatarCamera;
    float startDuration;

    void Start() {
        avatar = FindObjectOfType<RobotController>();
        startPosition = avatar.transform.position;

        avatarCamera = FindObjectOfType<CameraController>();
        startDuration = avatarCamera.moveDuration;
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (CalculateContact(collision, out var position)) {
            StartCoroutine(Death(position));
        }
    }
    IEnumerator Death(Vector2 position) {
        // An der Kontaktstelle wird eine Partikelexplosion erzeugt.
        Instantiate(particlePrefab, position, Quaternion.identity);

        // Der Roboter wird deaktiviert.
        avatar.gameObject.SetActive(false);

        // Es wird deathDuration Sekunden gewartet.
        yield return new WaitForSeconds(deathDuration);

        // Der Roboter wird an die Position gesetzt, an der er sich zu Szenenbeginn befand.
        avatar.transform.position = startPosition;

        // Er wieder aktiviert.
        avatar.gameObject.SetActive(true);

        // Die moveDuration der Kamera wird auf respawnDuration gesetzt.
        avatarCamera.moveDuration = respawnDuration;

        // Es wird respawnDuration gewartet.
        yield return new WaitForSeconds(respawnDuration);

        // Die moveDuration der Kamera wird wieder auf den Wert gesetzt, den sie zu Szenenbeginn besaß. (Das ist auch der Wert, den die moveDuration zum Zeitpunkt v.hatte)
        avatarCamera.moveDuration = startDuration;
    }
    bool CalculateContact(Collision2D collision, out Vector2 contactPosition) {
        // let's iterate over all contact points to calculate their average
        var contactPositionSum = Vector2.zero;
        int contactPositionCount = 0;
        for (int i = 0; i < collision.contactCount; i++) {
            var contact = collision.GetContact(i);
            contactPositionSum += contact.point;
            contactPositionCount++;
        }
        if (contactPositionCount > 0) {
            // calculate the average
            contactPosition = contactPositionSum / contactPositionCount;
            return true;
        } else {
            contactPosition = Vector2.zero;
            return false;
        }
    }
}
