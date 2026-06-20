using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

namespace Vehicles
{
    /// <summary>
    /// Lets the player orbit the showroom camera around the displayed vehicle by dragging with the
    /// mouse (desktop) or a finger (mobile). The actual framing is done by Cinemachine: this script
    /// only writes the drag into the <see cref="CinemachineOrbitalFollow"/> axes
    /// (<see cref="CinemachineOrbitalFollow.HorizontalAxis"/> / <see cref="CinemachineOrbitalFollow.VerticalAxis"/>),
    /// which the orbital follow consumes every frame to position the camera on its orbit sphere — no
    /// <c>CinemachineInputAxisController</c> is needed.
    ///
    /// UI rule: if a press/tap STARTS on top of a UI element, that whole gesture is ignored, so dragging
    /// on buttons or panels never rotates the camera. The over-UI test is sampled once when the press
    /// begins and latched for the gesture (re-querying mid-drag would wrongly unblock once the finger
    /// slides off the element).
    ///
    /// Uses the new Input System exclusively (the project's Active Input Handling is "Input System
    /// Package"). The per-frame path is allocation-free.
    /// </summary>
    [RequireComponent(typeof(CinemachineOrbitalFollow))]
    public class GarageOrbitCameraController : MonoBehaviour
    {
        [Header("Orbit Target")]
        [Tooltip("The orbital follow whose axes this drives. Auto-found on this GameObject if left empty.")]
        [SerializeField] private CinemachineOrbitalFollow orbitalFollow;

        [Header("Drag Sensitivity")]
        [Tooltip("Degrees of horizontal (yaw) rotation per pixel of horizontal drag.")]
        [SerializeField, Min(0f)] private float horizontalSpeed = 0.2f;

        [Tooltip("Degrees of vertical (pitch) rotation per pixel of vertical drag.")]
        [SerializeField, Min(0f)] private float verticalSpeed = 0.12f;

        [Tooltip("Flip the horizontal drag direction.")]
        [SerializeField] private bool invertHorizontal;

        [Tooltip("Flip the vertical drag direction.")]
        [SerializeField] private bool invertVertical;

        [Header("Feel")]
        [Tooltip("How quickly a flick's spin decays after release. Higher = stops sooner. 0 = no inertia (instant stop).")]
        [SerializeField, Min(0f)] private float inertiaDamping = 8f;

        [Header("Idle Auto-Rotate")]
        [Tooltip("Degrees/second the camera slowly orbits while idle. 0 disables the auto-spin.")]
        [SerializeField] private float idleAutoRotateSpeed;

        [Tooltip("Seconds of no input (and no leftover inertia) before the idle auto-spin resumes.")]
        [SerializeField, Min(0f)] private float idleResumeDelay = 3f;

        // Below this (deg/sec) the inertia is treated as fully stopped so the idle spin can take over.
        private const float VelocityEpsilon = 0.01f;

        // True while a non-UI gesture is actively dragging the camera.
        private bool _dragging;

        // Latched at press-began: true means the gesture started over UI and must be ignored entirely.
        private bool _blockedThisGesture;

        // Carried angular velocity (deg/sec) for post-release inertia.
        private float _horizontalVelocity;
        private float _verticalVelocity;

        // Time since the orbit last moved, used to delay the idle auto-spin.
        private float _idleTimer;

        private void Awake()
        {
            if (orbitalFollow == null)
                orbitalFollow = GetComponent<CinemachineOrbitalFollow>();

            if (orbitalFollow == null)
            {
                Debug.LogError($"[{nameof(GarageOrbitCameraController)}] No CinemachineOrbitalFollow assigned or found on '{name}'. Orbit input disabled.");
                enabled = false;
            }
        }

        private void Update()
        {
            if (orbitalFollow == null)
                return;

            float dt = Time.deltaTime;
            Vector2 drag = SampleGesture();

            float horizontalDelta;
            float verticalDelta;

            if (_dragging)
            {
                // Active drag drives the orbit directly and seeds the inertia velocity for the release.
                horizontalDelta = (invertHorizontal ? -1f : 1f) * drag.x * horizontalSpeed;
                verticalDelta = (invertVertical ? -1f : 1f) * drag.y * verticalSpeed;

                _horizontalVelocity = dt > 0f ? horizontalDelta / dt : 0f;
                _verticalVelocity = dt > 0f ? verticalDelta / dt : 0f;
                _idleTimer = 0f;
            }
            else
            {
                // No drag: bleed off any inertia from the last flick.
                if (inertiaDamping > 0f)
                {
                    float decay = Mathf.Exp(-inertiaDamping * dt);
                    _horizontalVelocity *= decay;
                    _verticalVelocity *= decay;
                }
                else
                {
                    _horizontalVelocity = 0f;
                    _verticalVelocity = 0f;
                }

                if (Mathf.Abs(_horizontalVelocity) < VelocityEpsilon)
                    _horizontalVelocity = 0f;
                if (Mathf.Abs(_verticalVelocity) < VelocityEpsilon)
                    _verticalVelocity = 0f;

                horizontalDelta = _horizontalVelocity * dt;
                verticalDelta = _verticalVelocity * dt;

                // Once everything has settled, optionally drift the camera as a slow turntable.
                bool settled = _horizontalVelocity == 0f && _verticalVelocity == 0f;
                if (idleAutoRotateSpeed != 0f && settled)
                {
                    _idleTimer += dt;
                    if (_idleTimer >= idleResumeDelay)
                        horizontalDelta += idleAutoRotateSpeed * dt;
                }
            }

            if (horizontalDelta != 0f)
                ApplyHorizontal(horizontalDelta);
            if (verticalDelta != 0f)
                ApplyVertical(verticalDelta);
        }

        // Returns this frame's drag delta in pixels for a valid (non-UI) gesture, or zero otherwise.
        // Also maintains the press/latch/release state machine for a single primary pointer.
        private Vector2 SampleGesture()
        {
            Pointer pointer = Pointer.current;
            if (pointer == null)
            {
                _dragging = false;
                return Vector2.zero;
            }

            // Press began this frame: decide once whether this gesture is allowed to orbit.
            if (pointer.press.wasPressedThisFrame)
            {
                _blockedThisGesture = IsPressOverUI(pointer);
                _dragging = !_blockedThisGesture;
            }

            if (pointer.press.wasReleasedThisFrame)
            {
                _dragging = false;
                _blockedThisGesture = false;
                return Vector2.zero;
            }

            if (_dragging && pointer.press.isPressed)
                return pointer.delta.ReadValue();

            return Vector2.zero;
        }

        // True when the press began on top of a UI element. Uses the pointer-id overload so it is
        // reliable for both mouse (synthetic id) and touch (the primary touch id).
        private static bool IsPressOverUI(Pointer pointer)
        {
            EventSystem eventSystem = EventSystem.current;
            if (eventSystem == null)
                return false; // No EventSystem -> nothing to block against; allow the orbit.

            if (pointer is Touchscreen touchscreen)
            {
                int touchId = touchscreen.primaryTouch.touchId.ReadValue();
                return eventSystem.IsPointerOverGameObject(touchId);
            }

            return eventSystem.IsPointerOverGameObject(PointerInputModule.kMouseLeftId);
        }

        // Adds yaw and wraps within the axis range (HorizontalAxis.Wrap is expected to be true).
        private void ApplyHorizontal(float deltaDegrees)
        {
            InputAxis axis = orbitalFollow.HorizontalAxis;
            axis.Value = axis.ClampValue(axis.Value + deltaDegrees);
            orbitalFollow.HorizontalAxis = axis;
        }

        // Adds pitch and clamps to the axis range so the camera cannot flip over the car.
        private void ApplyVertical(float deltaDegrees)
        {
            InputAxis axis = orbitalFollow.VerticalAxis;
            axis.Value = axis.ClampValue(axis.Value + deltaDegrees);
            orbitalFollow.VerticalAxis = axis;
        }
    }
}
