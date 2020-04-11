// NOTE DONT put in an editor folder!
using UnityEngine;

/// <summary>
///  use [Autohook] to modify Component or Prefab(GameObject)
/// </summary>
public class AutohookAttribute : PropertyAttribute
{
    public HookType hookType;
    public string prefabPath;

    public enum HookType {
        Component,
        Prefab,
        Auto
    }

    public AutohookAttribute(HookType hookType = HookType.Auto, string prefabPath = "Assets/Prefabs")
    {
        this.hookType = hookType;
        this.prefabPath = prefabPath;
    }
}