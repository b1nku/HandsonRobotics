using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HandsOnRobotics.UI
{
    /* Direction I (BIG IF): anchors the virtual table and robot to a physical desk
       detected via Quest Pro scene understanding.

    ── Prerequisites (one-time setup) ──────────────────────────────────────────────
    1. Run Space Setup on the Quest (Settings → Physical Space → Space Setup) and
       scan your desk so it is classified as TABLE.
       (Note: DESK was deprecated at SDK v65 — it maps to TABLE in the current API.)
    2. In OVRManager (the persistent scene OVRCameraRig GO):
         Scene Support       = Required
         Passthrough Support = Required (or Supported)
    3. In Edit → Project Settings → Meta XR → Required Features, enable "Scene".
       This adds the USE_SCENE permission to AndroidManifest automatically.
    4. Add an OVRPassthroughLayer to the OVRCameraRig GO.
    5. Add an OVRSceneManager component anywhere in the scene.
    6. Assign _sceneManager, _robotRoot, _tableRoot, and _trackingSpace below.
       For _trackingSpace: assign OVRCameraRig.trackingSpace (the child Transform
       of OVRCameraRig that converts tracking-space poses to world space).

    ── How it works ────────────────────────────────────────────────────────────────
    OVRSceneManager fires SceneModelLoadedSuccessfully once the scene is ready.
    This script then queries all scene anchors with OVRSemanticLabels and finds the
    first one classified as TABLE (which is what the headset calls a scanned desk).
    When found, it repositions the virtual table/robot so the virtual surface aligns
    with the physical desk surface detected by scene understanding.
    ──────────────────────────────────────────────────────────────────────────────── */
    public class PassthroughAnchor : MonoBehaviour
    {
        [Tooltip("OVRSceneManager in the scene — subscribe to its loaded event.")]
#pragma warning disable 0618   // OVRSceneManager deprecated since SDK v65; MRUK not in this project
        [SerializeField] OVRSceneManager _sceneManager;
#pragma warning restore 0618

        [Tooltip("Root of the Niryo One robot prefab.")]
        [SerializeField] Transform _robotRoot;

        [Tooltip("Root of the virtual table prefab.")]
        [SerializeField] Transform _tableRoot;

        [Tooltip("OVRCameraRig.trackingSpace — converts tracking-space anchor poses to world space.")]
        [SerializeField] Transform _trackingSpace;

        [Tooltip("Height of the virtual table surface above the table-root origin (metres).")]
        [SerializeField] float _tableSurfaceHeight = 0.02f;

#if UNITY_ANDROID
#pragma warning disable 0618   // OVRSceneManager deprecated since SDK v65; MRUK not in this project
        void OnEnable()
        {
            if (_sceneManager != null)
                _sceneManager.SceneModelLoadedSuccessfully += OnSceneLoaded;
        }

        void OnDisable()
        {
            if (_sceneManager != null)
                _sceneManager.SceneModelLoadedSuccessfully -= OnSceneLoaded;
        }
#pragma warning restore 0618

        void OnSceneLoaded() => StartCoroutine(AnchorToScene());

        IEnumerator AnchorToScene()
        {
            // One frame for locatable poses to become valid after the scene loads.
            yield return null;

            var anchors         = new List<OVRAnchor>();
            var classifications = new List<OVRSemanticLabels.Classification>();

            // SDK 85: FetchAnchorsAsync with FetchOptions (deprecated generic overload removed).
            var task = OVRAnchor.FetchAnchorsAsync(anchors, new OVRAnchor.FetchOptions
            {
                SingleComponentType = typeof(OVRSemanticLabels)
            });
            while (!task.IsCompleted) yield return null;

            foreach (var anchor in anchors)
            {
                // Check semantic classification — use enum API (string Labels deprecated at v65).
                if (!anchor.TryGetComponent<OVRSemanticLabels>(out var labels)) continue;
                classifications.Clear();
                labels.GetClassifications(classifications);
                if (!classifications.Contains(OVRSemanticLabels.Classification.Table)) continue;

                // Get the world-space pose via the locatable component.
                if (!anchor.TryGetComponent<OVRLocatable>(out var locatable)) continue;
                if (!locatable.TryGetSceneAnchorPose(out OVRLocatable.TrackingSpacePose pose)) continue;

                // TrackingSpacePose.Position/Rotation are in tracking space (nullable).
                // Use the Transform overload (not the Camera overload, which is deprecated) so
                // we read the tracking-to-world transform from the OVRCameraRig rather than
                // deriving it from a potentially stale camera pose.
                var ts = _trackingSpace != null ? _trackingSpace : transform;
                var worldPos = pose.ComputeWorldPosition(ts);
                var worldRot = pose.ComputeWorldRotation(ts);
                if (!worldPos.HasValue || !worldRot.HasValue) continue;

                // The TABLE anchor origin is the surface centre. Offset by _tableSurfaceHeight
                // so the virtual table surface sits flush with the physical one.
                var surfacePos = worldPos.Value;
                var surfaceRot = worldRot.Value;
                var offset     = surfaceRot * Vector3.down * _tableSurfaceHeight;

                if (_tableRoot) _tableRoot.SetPositionAndRotation(surfacePos + offset, surfaceRot);
                if (_robotRoot) _robotRoot.SetPositionAndRotation(surfacePos + offset, surfaceRot);

                Debug.Log($"[PassthroughAnchor] Anchored to TABLE at {surfacePos}.");
                yield break;
            }

            Debug.LogWarning(
                "[PassthroughAnchor] No TABLE anchor found. " +
                "Run Space Setup on the Quest and re-launch the app.");
        }
#else
        void Start() =>
            Debug.Log("[PassthroughAnchor] Scene anchoring is Quest-only; skipped in editor.");
#endif
    }
}
