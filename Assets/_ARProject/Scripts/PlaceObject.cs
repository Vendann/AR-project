using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using EnhancedTouch = UnityEngine.InputSystem.EnhancedTouch;

namespace _ARProject.Scripts {
    [RequireComponent(typeof(ARRaycastManager),
        typeof(ARPlaneManager),
        typeof(ARAnchorManager))]
    public class PlaceObject : MonoBehaviour {
        [SerializeField] private GameObject prefab;

        private ARRaycastManager _arRaycastManager;
        private ARPlaneManager _arPlaneManager;
        private ARAnchorManager _arAnchorManager;
        
        private readonly List<ARRaycastHit> _hits = new();

        private void Awake() {
            _arRaycastManager = GetComponent<ARRaycastManager>();
            _arPlaneManager = GetComponent<ARPlaneManager>();
            _arAnchorManager = GetComponent<ARAnchorManager>();
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
            ARPlane plane = _arPlaneManager.GetPlane(hit.trackableId);
            
            if (plane == null)
                return;

            ARAnchor anchor = _arAnchorManager.AttachAnchor(plane, hit.pose);

            if (anchor == null)
                return;

            GameObject obj = Instantiate(prefab, anchor.transform);
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            Vector3 dir = Camera.main.transform.position - anchor.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                obj.transform.rotation = Quaternion.LookRotation(dir);
        }
    }
}