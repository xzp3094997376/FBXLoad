using System;
using System.Collections;
using UnityEngine;

namespace TriLibCore
{
    /// <summary>
    /// Represents a class that forces GC collection using a fixed interval.
    /// </summary>
    public class GCHelper : MonoBehaviour
    {
        public float Interval = 1f;
        public float RemoveInstanceDelay = 10f;

        private int _loadingCount;

        private static GCHelper _instance;
        public static GCHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new GameObject("TriLibGCHelper").AddComponent<GCHelper>();
                }
                return _instance;
            } 
        }

        private void Start()
        {
            StartCoroutine(CollectGC());
        }

        private IEnumerator CollectGC()
        {
            while (true)
            {
                if (_loadingCount >= 0)
                {
                    yield return new WaitForSeconds(Interval);
                    GC.Collect();
                }
                yield return null;
            }
        }

        private IEnumerator RemoveInstanceInternal()
        {
            yield return new WaitForSeconds(RemoveInstanceDelay);
            _loadingCount = Mathf.Max(0, _loadingCount-1);
        }

        public void AddInstance()
        {
            _loadingCount++;
        }

        public void RemoveInstance()
        {
            StartCoroutine(RemoveInstanceInternal());
        }
    }
}