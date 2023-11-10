using _Scripts.Simulation;
using Shun_Card_System;
using UnityEngine;

namespace _Scripts.CardScript
{
    public class StylizedPawnHandCard : PawnHandCard
    {
        [SerializeField] private ParticleSystem _destroyParticleSystem;
        [SerializeField] private FadeObject _fadeObject;

        [SerializeField] private float _destroyDelay = 2f;
    
        protected override void Awake()
        {
            base.Awake();
        
            _destroyParticleSystem = GetComponentInChildren<ParticleSystem>();
            _fadeObject = GetComponent<FadeObject>();
        }

        public override SimulationPackage Discard()
        {
            var package = new SimulationPackage();
            package.AddToPackage(() =>
            {
                Debug.Log("Discard Card");
                Destroy();
            });
            return package;
        }

        protected override void Destroy()
        {
            ShowDestroyStyleEffect();
        
            Invoke(nameof(DelayDestroy), _destroyDelay);
        }
    
        private void DelayDestroy()
        {
        
            if (TryGetComponent<BaseDraggableObject>(out var baseDraggableObject))
                baseDraggableObject.Destroy();
            Destroy(gameObject);
        }


        private void ShowDestroyStyleEffect()
        {
            _fadeObject.StartFade();
        
            _destroyParticleSystem.gameObject.SetActive(true);
            _destroyParticleSystem.Play();
        }

    }
}