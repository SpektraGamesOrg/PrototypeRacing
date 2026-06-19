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
        public List<Rigidbody> destructedRigidbodies = new List<Rigidbody>();

        [SerializeField] private float debrisHideDelay = 10f; // parçalar kaç sn sonra kaybolsun
        [SerializeField] private float respawnDelay = 30f;    // ondan sonra kaç sn beklensin
        [SerializeField] private string vehicleTag = "Player";
        [Range(0f, 1f)]
        [SerializeField] private float speedMultiplierOnHit = 0.9f; // 1 = hiç yavaşlamasın

        private bool _isDestructed;
        private BoxCollider _boxCollider;
        private Renderer _visibilityRenderer;

        private Vector3[] _originalPositions;
        private Quaternion[] _originalRotations;

        private void Awake()
        {
            _boxCollider = GetComponent<BoxCollider>();
            _boxCollider.isTrigger = true;

            _visibilityRenderer = destructedVisual.GetComponentInChildren<Renderer>(true);

            CacheOriginalTransforms();
            ResetState();
        }

        private void CacheOriginalTransforms()
        {
            _originalPositions = new Vector3[destructedRigidbodies.Count];
            _originalRotations = new Quaternion[destructedRigidbodies.Count];

            for (int i = 0; i < destructedRigidbodies.Count; i++)
            {
                var t = destructedRigidbodies[i].transform;
                _originalPositions[i] = t.localPosition;
                _originalRotations[i] = t.localRotation;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isDestructed)
                return;

            if (!other.transform.root.CompareTag(vehicleTag))
                return;

            ApplySlowdown(other);
            Destruct();
        }

        private void ApplySlowdown(Collider other)
        {
            if (speedMultiplierOnHit >= 1f)
                return;

            var rb = other.attachedRigidbody;
            if (rb != null)
                rb.linearVelocity *= speedMultiplierOnHit;
        }

        private void Destruct()
        {
            _isDestructed = true;

            baseVisual.SetActive(false);
            destructedVisual.SetActive(true);

            foreach (var rb in destructedRigidbodies)
            {
                rb.gameObject.SetActive(true);
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            StartCoroutine(DestructRoutine());
        }

        private IEnumerator DestructRoutine()
        {
            // 1) Belli süre sonra uçuşan parçaları gizle
            yield return new WaitForSeconds(debrisHideDelay);
            SetDebrisActive(false);

            // 2) Bir süre daha bekle
            yield return new WaitForSeconds(respawnDelay);

            // 3) Hiçbir kamera görmeyene kadar bekle, sonra sıfırla
            while (IsVisible())
                yield return null;

            ResetState();
        }

        private bool IsVisible()
        {
            return _visibilityRenderer != null && _visibilityRenderer.isVisible;
        }

        private void SetDebrisActive(bool active)
        {
            foreach (var rb in destructedRigidbodies)
                rb.gameObject.SetActive(active);
        }

        private void ResetState()
        {
            for (int i = 0; i < destructedRigidbodies.Count; i++)
            {
                var rb = destructedRigidbodies[i];

                rb.transform.localPosition = _originalPositions[i];
                rb.transform.localRotation = _originalRotations[i];
                rb.gameObject.SetActive(false);
            }

            destructedVisual.SetActive(false);
            baseVisual.SetActive(true);

            _isDestructed = false;
        }
    }
}