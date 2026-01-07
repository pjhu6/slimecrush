using System.Collections;
using UnityEngine;

public class BreakableManager : MonoBehaviour
{
    public float shakeDelay = 0.5f;
    public float breakDelay = 1f;
    public float respawnDelay = 2f;
    public float shakeMagnitude = 0.05f;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // Check if the object colliding is the player
        if (collision.gameObject.CompareTag("Player"))
        {
            // Use DotTest to check if player is above
            if (collision.transform.DotTest(transform, Vector2.down))
            {
                StartCoroutine(BreakCoroutine());
            }
        }
    }

    private IEnumerator BreakCoroutine()
    {
        Debug.Log("Breakable hit from above, breaking...");
        yield return new WaitForSeconds(shakeDelay);

        // Shake the object side to side before breaking
        Vector3 originalPosition = transform.position;
        float elapsedTime = 0f;
        float shakeDuration = breakDelay - shakeDelay; // Shake for the rest of the duration

        while (elapsedTime < shakeDuration)
        {
            float offsetX = Mathf.Sin(elapsedTime * 50f) * shakeMagnitude; // Oscillate side to side
            transform.position = originalPosition + new Vector3(offsetX, 0f, 0f);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        transform.position = originalPosition; // Reset position after shaking

        // Instead of SetActive(false), disable only the visuals/colliders
        GetComponent<SpriteRenderer>().enabled = false;
        GetComponent<Collider2D>().enabled = false;

        Debug.Log("After Breakable breaking...");

        // Respawn
        yield return new WaitForSeconds(respawnDelay);

        // Re-enable the object
        GetComponent<SpriteRenderer>().enabled = true;
        GetComponent<Collider2D>().enabled = true;

        Debug.Log("Breakable will respawn...");
    }
}
