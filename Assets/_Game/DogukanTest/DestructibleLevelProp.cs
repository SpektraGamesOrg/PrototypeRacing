using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace _Game.Scripts.RaceSystem
{
    [RequireComponent(typeof(BoxCollider))]
    public class DestructibleLevelProp : MonoBehaviour
    {
        public GameObject baseVisual;
        public GameObject destructedVisual;
        public float stayDuration = 10f;
        public List<Rigidbody> destructedRigidbodies = new List<Rigidbody>();

        [SerializeField] private string vehicleTag = "Player";

        private bool _isDestructed;

        private void Awake()
        {
            baseVisual.SetActive(true);
            destructedVisual.SetActive(false);

            foreach (var rb in destructedRigidbodies)
                rb.gameObject.SetActive(false);
        }

        private void OnCollisionEnter(Collision other)
        {
            if (_isDestructed)
                return;

            if (!other.transform.root.CompareTag(vehicleTag))
                return;

            Destruct();
        }

        private void Destruct()
        {
            _isDestructed = true;

            baseVisual.SetActive(false);
            destructedVisual.SetActive(true);

            foreach (var rb in destructedRigidbodies)
                rb.gameObject.SetActive(true);

            StartCoroutine(HideAfterDelay());
        }

        private IEnumerator HideAfterDelay()
        {
            yield return new WaitForSeconds(stayDuration);

            foreach (var rb in destructedRigidbodies)
                rb.gameObject.SetActive(false);
        }
    }
}