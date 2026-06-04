using UnityEngine;

public class MainCamera : MonoBehaviour
{
    private Portal[] portals;

    void Start()
    {
        portals = FindObjectsByType<Portal>(
            FindObjectsSortMode.None);
    }

    void LateUpdate()
    {
        if (portals == null)
            return;

        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null)
                portals[i].PrePortalRender();
        }

        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null)
                portals[i].Render();
        }

        for (int i = 0; i < portals.Length; i++)
        {
            if (portals[i] != null)
                portals[i].PostPortalRender();
        }
    }
}