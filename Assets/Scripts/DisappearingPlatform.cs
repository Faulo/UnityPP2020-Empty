using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    public Collider2D attachedCollider;
    public Renderer attachedRenderer;

    [SerializeField, Range(0, 5)]
    private float visibleDuration = 1;
    [SerializeField, Range(0, 5)]
    private float disappearingDuration = 1;
    [SerializeField, Range(0, 5)]
    private float goneDuration = 1;
    [SerializeField, Range(0, 5)]
    private float appearingDuration = 1;

    private void Start()
    {
        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForEndOfFrame();
        Color color = attachedRenderer.material.color;
        while (true)
        {
            // Collider ist aktiv, Sprite ist sichtbar
            yield return new WaitForSeconds(visibleDuration);

            // Collider ist aktiv, Sprite verschwindet
            for (float timer = 0; timer < disappearingDuration; timer += Time.deltaTime)
            {
                color.a = 1 - (timer / disappearingDuration);
                attachedRenderer.material.color = color;
                yield return null;
            }
            color.a = 0;
            attachedRenderer.material.color = color;

            // Collider ist inaktiv, Sprite ist unsichtbar
            attachedCollider.enabled = false;


            yield return new WaitForSeconds(goneDuration);

            // Collider ist aktiv, Sprite erscheint
            attachedCollider.enabled = true;
            for (float timer = 0; timer < appearingDuration; timer += Time.deltaTime)
            {
                color.a = timer / appearingDuration;
                attachedRenderer.material.color = color;
                yield return null;
            }
            color.a = 1;
            attachedRenderer.material.color = color;
        }
    }
}
