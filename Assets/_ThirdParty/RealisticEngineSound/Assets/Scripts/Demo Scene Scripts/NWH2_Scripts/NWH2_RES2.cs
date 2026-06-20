//______________________________________________//
//___________Realistic Engine Sounds____________//
//______________________________________________//
//_______Copyright © 2023 Skril Studio__________//
//______________________________________________//
//__________ http://skrilstudio.com/ ___________//
//______________________________________________//
//________ http://fb.com/yugelmobile/ __________//
//______________________________________________//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NWH.VehiclePhysics2;
using NWH.Common.Cameras;

namespace SkrilStudio
{
    public class NWH2_RES2 : MonoBehaviour
    {
        RealisticEngineSound[] res2;
        VehicleController vc;
        CameraChanger nvhCamera;
        private GameObject car;
        private AudioListener audioListener;
        private WaitForSeconds _wait;
        public bool releaseGasAtShifting = true;
        public int interiorCamNumber = 2;
        private int currentCamNo;
        private int currentActivePrefab = 0; // 0 = exterior, 1 = interior
        private float revLimiter;
        void Start()
        {
            _wait = new WaitForSeconds(0.15f); // setup wait
            res2 = GetComponentsInChildren<RealisticEngineSound>();
            car = gameObject.GetFirstParentWithComponent<VehicleController>();
            vc = car.GetComponent<VehicleController>();
            if (nvhCamera == null)
                nvhCamera = car.GetComponentInChildren<CameraChanger>();
            // disable default engine sound
            StartCoroutine(WaitForStart());
            // get audio listener from camera
            audioListener = car.GetComponentInChildren<AudioListener>(true);
            revLimiter = vc.powertrain.engine.revLimiterRPM;
            for (int i = 0; i < res2.Length; i++)
            {
                res2[i].maxRPMLimit = revLimiter;
                res2[i].carMaxSpeed = 250; // needed for straight cut gearbox script
                res2[i].gasPedalValueSetting = RealisticEngineSound.GasPedalValue.NotSimulated;
                if (res2[i].audioListener == null)
                    res2[i].audioListener = audioListener;
            }
        }
        void Update()
        {
            if (res2[currentActivePrefab].enabled && vc.powertrain.engine.IsRunning)
            {
                if (vc.soundManager.engineRunningComponent.IsActive)
                    vc.soundManager.engineRunningComponent.VC_Disable(true);
                // current rpm
                res2[currentActivePrefab].engineCurrentRPM = vc.powertrain.engine.RPMPercent * revLimiter;
                // current speed
                res2[currentActivePrefab].carCurrentSpeed = vc.Speed; // needed for straight cut gearbox script
                // engine load
                res2[currentActivePrefab].engineLoad = vc.powertrain.engine.Load;
                // gas pedal value
                if (vc.input.Vertical == 0)
                {
                    res2[currentActivePrefab].gasPedalPressing = false;
                }
                else
                {
                    if (!vc.powertrain.transmission.isShifting)
                    {
                        if (!vc.brakes.IsBraking)
                        {
                            res2[currentActivePrefab].gasPedalPressing = true;
                        }
                        else
                        {
                            if (vc.input.Handbrake == 0)
                                res2[currentActivePrefab].gasPedalPressing = false;
                        }
                        res2[currentActivePrefab].isShifting = false; // needed for shifting sounds script
                    }
                    else
                    {
                        if (releaseGasAtShifting)
                            res2[currentActivePrefab].gasPedalPressing = false;
                    }
                }
                if (vc.input.Vertical > 1) // stationary burnout
                {
                    res2[currentActivePrefab].gasPedalPressing = true;
                }
                // is in reverse
                if (vc.powertrain.transmission.Gear < 0)
                    res2[currentActivePrefab].isReversing = true;
                else
                    res2[currentActivePrefab].isReversing = false;
            }
            else
            {
                res2[currentActivePrefab].engineCurrentRPM = 0;
            }
            if (!vc.isActiveAndEnabled || vc.powertrain.engine.ignition == false) // check if car is enabled or is it's engine running
            {
                if (gameObject.GetComponent<AudioReverbZone>() != null)
                    gameObject.GetComponent<AudioReverbZone>().enabled = false;
                res2[currentActivePrefab].gameObject.SetActive(false);
            }
            if (vc.isActiveAndEnabled && vc.powertrain.engine.ignition == true)
            {
                if (gameObject.GetComponent<AudioReverbZone>() != null)
                    gameObject.GetComponent<AudioReverbZone>().enabled = true;
                res2[currentActivePrefab].gameObject.SetActive(true);
            }
            // cam and prefab controller
            if (currentCamNo != nvhCamera.currentCameraIndex && res2.Length > 1)
            {
                if (nvhCamera.currentCameraIndex == interiorCamNumber) // interior camera view
                {
                    res2[1].gameObject.SetActive(true); // interior prefab
                    res2[0].gameObject.SetActive(false); // exterior prefab
                    currentCamNo = nvhCamera.currentCameraIndex;
                    currentActivePrefab = 1; // interior
                }
                else // exterior camera view
                {
                    res2[1].gameObject.SetActive(false); // interior prefab
                    res2[0].gameObject.SetActive(true); // exterior prefab
                    currentCamNo = nvhCamera.currentCameraIndex;
                    currentActivePrefab = 0; // exterior
                }
            }
        }
        void LateUpdate()
        {
            if (vc.powertrain.transmission.isShifting)
                res2[currentActivePrefab].isShifting = true; // needed for shifting sounds script
            else
                res2[currentActivePrefab].isShifting = false;       
        }
        private void OnEnable()
        {
            Start();
        }
        // wait for start
        IEnumerator WaitForStart()
        {
            while (true)
            {
                yield return _wait; // this is needed to avoid duplicate audio sources
                if (res2.Length > 1)
                {
                    if (nvhCamera.currentCameraIndex == interiorCamNumber) // interior camera view
                    {
                        res2[1].gameObject.SetActive(true); // interior prefab
                        res2[0].gameObject.SetActive(false); // exterior prefab
                        currentCamNo = nvhCamera.currentCameraIndex;
                        currentActivePrefab = 1; // interior
                    }
                    else // exterior camera view
                    {
                        res2[1].gameObject.SetActive(false); // interior prefab
                        res2[0].gameObject.SetActive(true); // exterior prefab
                        currentCamNo = nvhCamera.currentCameraIndex;
                        currentActivePrefab = 0; // exterior
                    }
                }
                break;
            }
        }
    }
}
