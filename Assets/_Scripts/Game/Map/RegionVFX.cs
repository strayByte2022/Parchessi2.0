using System.Collections;
using UnityEngine;

public class RegionVFX : MonoBehaviour
{
    public ParticleSystem particleSystem;
    public float minInterval = 5f;  // Minimum time between particle plays
    public float maxInterval = 10f; // Maximum time between particle plays

    private float timer;
    private float timeToPlay;

    public Vector2 range = new Vector2(10f, 10f);
    public float moveSpeed = 2.0f;

    private Vector3 initialPosition;
    private Vector3 targetPosition;
    private float timeToReachTarget;
    private float timer1;

    void Start()
    {
        initialPosition = particleSystem.transform.position;
        SetRandomTarget();
        SetRandomTimer();

    }

    void Update()
    {
        timer1 += Time.deltaTime;

        if (timer1 >= timeToReachTarget)
        {
            SetRandomTarget();
            timer1 = 0;
        }

        // Interpolate between the initial position and the target position
        float t = timer1 / timeToReachTarget;
        particleSystem.transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

        timer += Time.deltaTime;

        if (timer >= timeToPlay)
        {
            // Play the particle system
            particleSystem.Play();

            // Calculate the duration of the particle system to stop it after that time
            float duration = particleSystem.main.duration;

            // Stop the particle system after its duration
            StartCoroutine(StopParticleSystemAfterDuration(duration));

            // Reset the timer and set a new random time to play
            timer = 0f;
            SetRandomTimer();
        }
    }

    void SetRandomTarget()
    {
        initialPosition = transform.position;
        targetPosition = initialPosition + new Vector3(Random.Range(-range.x, range.x), Random.Range(-range.y, range.y), 0f);
        timeToReachTarget = Vector3.Distance(initialPosition, targetPosition) / moveSpeed;
    }

    private void SetRandomTimer()
    {
        // Calculate a new random time to play the particle
        timeToPlay = Random.Range(minInterval, maxInterval);
    }

    private IEnumerator StopParticleSystemAfterDuration(float duration)
    {
        yield return new WaitForSeconds(duration);
        particleSystem.Stop();
    }
}
