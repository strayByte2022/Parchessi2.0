using UnityEngine;

namespace _Scripts.Game.StyleEffect
{
    [RequireComponent(typeof(ParticleSystem))]
    public class MouseParticleFollow : MonoBehaviour
    {
        private ParticleSystem _particleSystem;

        private void Start()
        {
            _particleSystem = GetComponent<ParticleSystem>();
            _particleSystem.Stop(); // Stop the particles initially
        }

        private void Update()
        {
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = 0; // Ensure the same z-position as your camera

            transform.position = mousePosition; // Set the particle system's position to the mouse cursor

            if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
            {
                _particleSystem.Play(); // Start the particle effect when the left mouse button is clicked
            }
            else if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
            {
                _particleSystem.Stop(); // Stop the particle effect when the left mouse button is released
            }
        }
    }
}