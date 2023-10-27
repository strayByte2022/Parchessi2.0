using System;
using System.Collections;
using System.Collections.Generic;
using Priority_Queue;
using UnityEngine;
using UnityUtilities;

namespace _Scripts.Simulation
{
    public class SimulationManager : SingletonMonoBehaviour<SimulationManager>
    {
        private SimplePriorityQueue<SimulationPackage> _simulationQueue = new();


        private bool _isExecuting;

        public void AddCoroutineSimulationObject(SimulationPackage simulationPackage)
        {
            if (simulationPackage == null || _simulationQueue.Contains(simulationPackage))
            {
                return;
            }
            _simulationQueue.Enqueue(simulationPackage, simulationPackage.Priority);
        }

        public void RemoveCoroutineSimulationObject(SimulationPackage simulationPackage)
        {
            if (simulationPackage == null || !_simulationQueue.Contains(simulationPackage))
            {
                return;
            }
            _simulationQueue.Remove(simulationPackage);
        }

        public void ClearAll()
        {
            _simulationQueue.Clear();
        }

        private void FixedUpdate()
        {
            StartCoroutine(ExecuteAll());
        }
        

        IEnumerator ExecuteAll()
        {
            if (_isExecuting)
            {
                yield break;
            }

            _isExecuting = true;
            while (_simulationQueue.Count > 0)
            {
                var simulationObject = _simulationQueue.Dequeue();
                yield return StartCoroutine(ExecuteCoroutineSimulationConcurrent(simulationObject));
            }

            _isExecuting = false;
        }
        IEnumerator ExecuteCoroutineSimulationConcurrent(SimulationPackage simulationPackage)
        {
            foreach (var enumerator in simulationPackage.ExecuteEvents)
            {
                if (enumerator == null) continue;
                yield return StartCoroutine(enumerator.Invoke());
            }

            yield return null;
}
        
        IEnumerator ExecuteCoroutineSimulationParallel(SimulationPackage simulationPackage)
        {
            List<Coroutine> runningCoroutines = new List<Coroutine>();

            foreach (var enumerator in simulationPackage.ExecuteEvents)
            {
                if (enumerator == null) continue;
                Coroutine newCoroutine = StartCoroutine(enumerator.Invoke());
                runningCoroutines.Add(newCoroutine);
            }

            // WaitPhase for all running coroutines to complete
            yield return WaitAllCoroutines(runningCoroutines);
        }

        IEnumerator WaitAllCoroutines(List<Coroutine> coroutines)
        {
            foreach (var coroutine in coroutines)
            {
                yield return coroutine;
            }
        }
    }
}

