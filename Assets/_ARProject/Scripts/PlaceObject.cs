using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

namespace _ARProject.Scripts {
    [RequireComponent(typeof(ARRaycastManager), typeof(ARPlaneManager))]
    public class PlaceObject : MonoBehaviour {
        [SerializeField] private GameObject prefab;

        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private readonly List<ARRaycastHit> _hits = new();

        private void Awake() {
            _arRaycastManager = GetComponent<ARRaycastManager>();
            _arPlaneManager = GetComponent<ARPlaneManager>();
        }

#if !UNITY_EDITOR
        private void OnEnable() {
            EnhancedTouch.EnhancedTouchSupport.Enable();
            EnhancedTouch.Touch.onFingerDown += FingerDown;
        }

        private void OnDisable() {
            EnhancedTouch.Touch.onFingerDown -= FingerDown;
            EnhancedTouch.EnhancedTouchSupport.Disable();
        }

        private void FingerDown(EnhancedTouch.Finger finger) {
            if (finger.index != 0)
                return;

            TryPlaceObject(finger.currentTouch.screenPosition);
        }
#endif

#if UNITY_EDITOR
        private void Update() {
            if (Mouse.current == null)
                return;

            if (Mouse.current.leftButton.wasPressedThisFrame) {
                TryPlaceObject(Mouse.current.position.ReadValue());
            }
        }
#endif

        private void TryPlaceObject(Vector2 screenPosition) {
            if (!_arRaycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
                return;

            ARRaycastHit hit = _hits[0];
            Pose pose = hit.pose;
            GameObject obj = Instantiate(prefab, pose.position, pose.rotation);
            ARPlane plane = _arPlaneManager.GetPlane(hit.trackableId);

            if (plane != null && plane.alignment == PlaneAlignment.HorizontalUp) {
                Vector3 direction = Camera.main.transform.position - obj.transform.position;
                direction.y = 0f;
                
                if (direction.sqrMagnitude > 0.001f) {
                    obj.transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }
}