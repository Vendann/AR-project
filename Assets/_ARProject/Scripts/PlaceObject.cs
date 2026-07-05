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

        private GameObject _placedObject;
        private ARAnchor _currentAnchor;

        private void Awake()
        {
            _arRaycastManager = GetComponent<ARRaycastManager>();
            _arPlaneManager = GetComponent<ARPlaneManager>();
            _arAnchorManager = GetComponent<ARAnchorManager>();
        }

#if !UNITY_EDITOR
        private void OnEnable()
        {
            EnhancedTouch.EnhancedTouchSupport.Enable();
            EnhancedTouch.Touch.onFingerDown += FingerDown;
        }

        private void OnDisable()
        {
            EnhancedTouch.Touch.onFingerDown -= FingerDown;
            EnhancedTouch.EnhancedTouchSupport.Disable();
        }

        private void FingerDown(EnhancedTouch.Finger finger)
        {
            if (finger.index != 0)
                return;

            TryPlaceObject(finger.currentTouch.screenPosition);
        }
#endif

#if UNITY_EDITOR
        private void Update()
        {
            if (Mouse.current != null &&
                Mouse.current.leftButton.wasPressedThisFrame)
            {
                TryPlaceObject(Mouse.current.position.ReadValue());
            }
        }
#endif

        private void TryPlaceObject(Vector2 screenPosition)
        {
            if (!_arRaycastManager.Raycast(screenPosition, _hits, TrackableType.PlaneWithinPolygon))
                return;

            ARRaycastHit hit = _hits[0];
            ARPlane plane = _arPlaneManager.GetPlane(hit.trackableId);

            if (plane == null)
                return;

            if (_currentAnchor != null)
                Destroy(_currentAnchor.gameObject);

            _currentAnchor = _arAnchorManager.AttachAnchor(plane, hit.pose);

            if (_currentAnchor == null)
                return;

            if (_placedObject == null)
            {
                _placedObject = Instantiate(prefab, _currentAnchor.transform);
            }
            else
            {
                _placedObject.transform.SetParent(_currentAnchor.transform, false);
            }

            _placedObject.transform.localPosition = Vector3.zero;
            _placedObject.transform.localRotation = Quaternion.identity;

            Vector3 direction = Camera.main.transform.position - _currentAnchor.transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.001f)
            {
                _placedObject.transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}