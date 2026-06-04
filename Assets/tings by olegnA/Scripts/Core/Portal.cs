using System.Collections.Generic;
using UnityEngine;
using Unity.XR.CoreUtils;
using UnityEngine.XR;

public class Portal : MonoBehaviour
{
    [Header("Portal Settings")]
    public Portal linkedPortal;
    public MeshRenderer screen;
    public int recursionLimit = 2;

    [Header("XR")]
    public XROrigin xrOrigin;
    public Camera xrCamera;

    [Header("Clipping")]
    public float nearClipOffset = 0.05f;
    public float nearClipLimit = 0.2f;

    RenderTexture viewTexture;
    Camera portalCam;

    List<PortalTraveller> trackedTravellers =
        new List<PortalTraveller>();

    MeshFilter screenMeshFilter;

    void Awake()
    {

        portalCam = GetComponentInChildren<Camera>();

        if (xrCamera == null)
        {
            xrCamera = Camera.main;
        }

        portalCam.enabled = false;
        portalCam.stereoTargetEye = StereoTargetEyeMask.None;

        Debug.Log($"XR Camera: {xrCamera?.name}");
        Debug.Log($"Portal Camera Stereo Enabled: {portalCam.stereoEnabled}");

        screenMeshFilter =
            screen.GetComponent<MeshFilter>();

        screen.material.SetInt("displayMask", 1);

        CreateViewTexture();
    }

    void LateUpdate()
    {
        HandleTravellers();
    }

    void CreateViewTexture()
    {
        int width = Mathf.Max(Screen.width, 2048);
        int height = Mathf.Max(Screen.height, 2048);

        if (viewTexture != null)
        {
            if (viewTexture.width == width &&
                viewTexture.height == height)
                return;

            viewTexture.Release();
        }

        viewTexture = new RenderTexture(
            width,
            height,
            24,
            RenderTextureFormat.Default
        );

        portalCam.targetTexture = viewTexture;

        linkedPortal.screen.material.SetTexture(
            "_MainTex",
            viewTexture
        );
    }

    public void Render()
    {
        if (xrCamera == null)
            return;

        if (!CameraUtility.VisibleFromCamera(
                linkedPortal.screen,
                xrCamera))
            return;

        Matrix4x4 m =
            transform.localToWorldMatrix *
            linkedPortal.transform.worldToLocalMatrix *
            xrCamera.transform.localToWorldMatrix;



        portalCam.transform.SetPositionAndRotation(
    m.GetColumn(3),
    m.rotation
);

        // Clear any weird XR matrices
        portalCam.ResetProjectionMatrix();
        portalCam.ResetWorldToCameraMatrix();

        Vector3 eyeOffset =
    xrCamera.transform.right * 0.032f;

        // Test both signs
        portalCam.transform.position -= eyeOffset;

        portalCam.fieldOfView =
            xrCamera.fieldOfView;

        portalCam.aspect =
            xrCamera.aspect;

        //SetNearClipPlane();

        screen.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly;

        linkedPortal.screen.material.SetInt(
            "displayMask",
            0
        );

        portalCam.Render();

        linkedPortal.screen.material.SetInt(
            "displayMask",
            1
        );

        screen.shadowCastingMode =
            UnityEngine.Rendering.ShadowCastingMode.On;
    }

    void SetNearClipPlane()
    {
        Transform clipPlane = transform;

        int dot = System.Math.Sign(
            Vector3.Dot(
                clipPlane.forward,
                transform.position -
                portalCam.transform.position
            )
        );

        Vector3 camSpacePos =
            portalCam.worldToCameraMatrix
            .MultiplyPoint(clipPlane.position);

        Vector3 camSpaceNormal =
            portalCam.worldToCameraMatrix
            .MultiplyVector(clipPlane.forward)
            * dot;

        float camSpaceDst =
            -Vector3.Dot(
                camSpacePos,
                camSpaceNormal
            )
            + nearClipOffset;

        if (Mathf.Abs(camSpaceDst) > nearClipLimit)
        {
            Vector4 clipPlaneCameraSpace =
                new Vector4(
                    camSpaceNormal.x,
                    camSpaceNormal.y,
                    camSpaceNormal.z,
                    camSpaceDst
                );

            portalCam.projectionMatrix =
                xrCamera.CalculateObliqueMatrix(
                    clipPlaneCameraSpace
                );
        }
        else
        {
            portalCam.projectionMatrix =
                xrCamera.projectionMatrix;
        }
    }

    void HandleTravellers()
    {
        for (int i = 0; i < trackedTravellers.Count; i++)
        {
            PortalTraveller traveller =
                trackedTravellers[i];

            Transform travellerT =
                traveller.transform;

            Matrix4x4 m =
                linkedPortal.transform.localToWorldMatrix *
                transform.worldToLocalMatrix *
                travellerT.localToWorldMatrix;

            Vector3 offsetFromPortal =
                travellerT.position -
                transform.position;

            int portalSide =
                System.Math.Sign(
                    Vector3.Dot(
                        offsetFromPortal,
                        transform.forward
                    )
                );

            int portalSideOld =
                System.Math.Sign(
                    Vector3.Dot(
                        traveller.previousOffsetFromPortal,
                        transform.forward
                    )
                );

            if (portalSide != portalSideOld)
            {
                Vector3 oldPos =
                    travellerT.position;

                Quaternion oldRot =
                    travellerT.rotation;

                traveller.Teleport(
                    transform,
                    linkedPortal.transform,
                    m.GetColumn(3),
                    m.rotation
                );

                traveller.graphicsClone.transform
                    .SetPositionAndRotation(
                        oldPos,
                        oldRot
                    );

                linkedPortal.OnTravellerEnterPortal(
                    traveller
                );

                trackedTravellers.RemoveAt(i);
                i--;
            }
            else
            {
                traveller.graphicsClone.transform
                    .SetPositionAndRotation(
                        m.GetColumn(3),
                        m.rotation
                    );

                traveller.previousOffsetFromPortal =
                    offsetFromPortal;
            }
        }
    }

    void OnTravellerEnterPortal(
        PortalTraveller traveller)
    {
        if (!trackedTravellers.Contains(traveller))
        {
            traveller.EnterPortalThreshold();

            traveller.previousOffsetFromPortal =
                traveller.transform.position -
                transform.position;

            trackedTravellers.Add(traveller);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        PortalTraveller traveller =
            other.GetComponent<PortalTraveller>();

        if (traveller)
        {
            OnTravellerEnterPortal(traveller);
        }
    }

    void OnTriggerExit(Collider other)
    {
        PortalTraveller traveller =
            other.GetComponent<PortalTraveller>();

        if (traveller &&
            trackedTravellers.Contains(traveller))
        {
            traveller.ExitPortalThreshold();
            trackedTravellers.Remove(traveller);
        }
    }

    public void PrePortalRender()
    {
        // Compatibility stub for original MainCamera.cs
    }

    public void PostPortalRender()
    {
        // Compatibility stub for original MainCamera.cs
    }
}