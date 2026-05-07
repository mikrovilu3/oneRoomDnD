using UnityEngine;
using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

[Serializable]
public class SaveData
{
    public int currentLevel = 1;
    public float playerHealth = 100f;
    public int score = 0;
    public float playTimeSeconds = 0f;

    // VR-specific extras you might want:
    // public float playerHeight  = 1.7f;
    // public bool  leftHanded    = false;
    // public float snapTurnAngle = 45f;
}

/// <summary>
/// Binary save system. Works identically in VR and flat.
/// Attach to a persistent GameObject in your first scene.
/// Access data anywhere via SaveManager.CurrentSave.
/// </summary>