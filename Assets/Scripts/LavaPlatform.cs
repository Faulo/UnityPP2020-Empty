using System.Collections;
using UnityEngine;

public class LavaPlatform : MonoBehaviour
{
    [SerializeField]
    private ParticleSystem particlePrefab = default;
    [SerializeField]
    private float deathDuration = 1;
    [SerializeField]
    private float respawnDuration = 1;
    private RobotController avatar;
    private Vector3 startPosition;
    private CameraController avatarCamera;
    private float startDuration;

    private void Start()
    {
        avatar = FindObjectOfType<RobotController>();
        startPosition = avatar.transform.position;

        avatarCamera = FindObjectOfType<CameraController>();
        startDuration = avatarCamera.moveDuration;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (CalculateContact(collision, out Vector2 position))
        {
            StartCoroutine(Death(position));
        }
    }

    private IEnumerator Death(Vector2 position)
    {
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

    private bool CalculateContact(Collision2D collision, out Vector2 contactPosition)
    {
        // let's iterate over all contact points to calculate their average
        Vector2 contactPositionSum = Vector2.zero;
        int contactPositionCount = 0;
        for (int i = 0; i < collision.contactCount; i++)
        {
            ContactPoint2D contact = collision.GetContact(i);
            contactPositionSum += contact.point;
            contactPositionCount++;
        }
        if (contactPositionCount > 0)
        {
            // calculate the average
            contactPosition = contactPositionSum / contactPositionCount;
            return true;
        }
        else
        {
            contactPosition = Vector2.zero;
            return false;
        }
    }
}
