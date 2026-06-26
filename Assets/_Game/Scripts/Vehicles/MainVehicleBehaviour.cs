using System;
using SpektraGames.RuntimeUI.Runtime;
using UnityEngine;

namespace Vehicles
{
    public class MainVehicleBehaviour : MonoBehaviour
    {
        [SerializeField]
        private VehicleID vehicleID = VehicleID.None;
        public VehicleID VehicleID
        {
            get => vehicleID;
            set => vehicleID = value;
        }

        [SerializeField]
        private RCC_CarControllerV4 vehicleController = null;
        public RCC_CarControllerV4 VehicleController => vehicleController;

        [SerializeField]
        private new Rigidbody rigidbody = null;
        public Rigidbody Rigidbody => rigidbody;

        private void OnValidate()
        {
            Validate();
        }

        public void Validate()
        {
#if UNITY_EDITOR

            bool anyChange = false;

            if (Application.isPlaying)
                return;

            if (vehicleController != GetComponent<RCC_CarControllerV4>())
            {
                vehicleController = GetComponent<RCC_CarControllerV4>();
                anyChange = true;
            }

            if (rigidbody != GetComponent<Rigidbody>())
            {
                rigidbody = GetComponent<Rigidbody>();
                anyChange = true;
            }

            if (anyChange)
            {
                UnityEditor.EditorUtility.SetDirty(this);
            }
#endif
        }
    }
}