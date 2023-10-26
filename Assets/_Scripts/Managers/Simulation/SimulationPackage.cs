using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;

namespace _Scripts.Simulation
{
    public class SimulationPackage
    {
        public float Priority { get; set; }
        public readonly List<Func<IEnumerator>> ExecuteEvents = new();

        public SimulationPackage(float priority = 0)
        {
            Priority = priority;
        }
        
        
        
        public void AddToPackage(Action action)
        {
            ExecuteEvents.Add(ConvertToIEnumerator(action));
        }

        public void AddToPackage(Func<IEnumerator> coroutine)
        {
            ExecuteEvents.Add(coroutine);
        }

        public void AddToPackage(Tween tween)
        {
            ExecuteEvents.Add(ConvertToIEnumerator(tween));
        }

        private Func<IEnumerator> ConvertToIEnumerator(Action action)
        {
            return new Func<IEnumerator>(() =>
            {
                IEnumerator CoroutineWrapper()
                {
                    action();
                    yield return null; // Yielding once to make it a valid coroutine
                }

                return CoroutineWrapper();
            });
        }

        private Func<IEnumerator> ConvertToIEnumerator(Tween tween)
        {
            if (tween == null || tween.IsActive()) return null;
            
            tween.Pause();
            return new Func<IEnumerator>(() =>
            {
                IEnumerator CoroutineWrapper()
                {
                    tween.SetAutoKill(false);
                    tween.Play();
                    while (!tween.IsComplete())
                    {
                        yield return null;
                    }
                    tween.Kill();
                }

                return CoroutineWrapper();
            });
        }
    }
}