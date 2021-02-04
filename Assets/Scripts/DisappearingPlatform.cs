using System.Collections;
using UnityEngine;

public class DisappearingPlatform : MonoBehaviour
{
    public Collider2D attachedCollider;
    public Renderer attachedRenderer;

    private void Start()
    {
        StartCoroutine(DisappearRoutine());
    }

    private IEnumerator DisappearRoutine()
    {
        yield return new WaitForSeconds(1);
        Debug.Log(this);
    }
}
