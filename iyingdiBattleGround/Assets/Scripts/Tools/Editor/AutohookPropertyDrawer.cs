// NOTE put in a Editor folder
using UnityEditor;
using UnityEngine;
using UnityEngine.Assertions;

[CustomPropertyDrawer(typeof(AutohookAttribute))]
public class AutohookPropertyDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        //if (property.objectReferenceValue != null)
        //{
        //    EditorGUI.PropertyField(position, property, label);
        //    return;
        //}
        AutohookAttribute autohookAttribute = (AutohookAttribute)attribute;

        // check if auto set hookType
        if (autohookAttribute.hookType == AutohookAttribute.HookType.Auto)
        {
            if (property.name.EndsWith("Prefab") && GetTypeFromProperty(property).Equals(typeof(GameObject)))
            {
                autohookAttribute.hookType = AutohookAttribute.HookType.Prefab;
            } else
            {
                autohookAttribute.hookType = AutohookAttribute.HookType.Component;
            }
        }

        if (autohookAttribute.hookType == AutohookAttribute.HookType.Component)
        {
            //Assert.IsFalse(GetTypeFromProperty(property).Equals(typeof(GameObject)), 
            //    "use [Autohook] to modify "+property.name);
            var component = FindAutohookTarget(property);
            if (component != null)
            {
                //  if (property.objectReferenceValue == null)
                property.objectReferenceValue = component;
            }
        } else
        {
            //Assert.IsTrue(GetTypeFromProperty(property).Equals(typeof(GameObject)),
            //    "use [Autohook(AutohookAttribute.HookType.Prefab)] to modify " + property.name);
            string name = property.name;
            if (name.EndsWith("Prefab"))
            {
                name = name.Remove(name.Length - 6);
            }
            var prefab = FindPrefab(property, name, autohookAttribute.prefabPath);
            if (prefab != null)
            {
                property.objectReferenceValue = prefab;
            }
        }


        EditorGUI.PropertyField(position, property, label);
    }

    private GameObject FindPrefab(SerializedProperty property, string name, string assertPath)
    {
        string[] guids = AssetDatabase.FindAssets("t:Prefab", new string[] { assertPath });
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            string prefabName = System.IO.Path.GetFileNameWithoutExtension(path);
            if (name == prefabName)
            {
                return AssetDatabase.LoadAssetAtPath<GameObject>(path);
            }
        }
        return null;
    }

    /// <summary>
    /// Takes a SerializedProperty and finds a local component that can be slotted into it.
    /// Local in this context means its a component attached to the same GameObject.
    /// This could easily be changed to use GetComponentInParent/GetComponentInChildren
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private Component FindAutohookTarget(SerializedProperty property)
    {

        var root = property.serializedObject;

        if (root.targetObject is Component)
        {
            // first, lets find the type of component were trying to autohook...
            var type = GetTypeFromProperty(property);

            // ...then use GetComponent(type) to see if there is one on our object.
            var component = (Component)root.targetObject;
            //  var gb = (GameObject) root.targetObject;
            //  Debug.LogError(component.transform(type));
            var components = component.GetComponentsInChildren(type);
            foreach (var item in components)
            {
                //确保GameObject不要有重名的
                if (item.gameObject.name == property.name)
                {
                    return item.gameObject.GetComponent(type);
                }
            }
        }
        else
        {
            Debug.Log("OH NO handle fails here better pls");
        }

        return null;
    }

    /// <summary>
    /// Uses reflection to get the type from a serialized property
    /// </summary>
    /// <param name="property"></param>
    /// <returns></returns>
    private static System.Type GetTypeFromProperty(SerializedProperty property)
    {
        // first, lets get the Type of component this serialized property is part of...
        var parentComponentType = property.serializedObject.targetObject.GetType();
        // ... then, using reflection well get the raw field info of the property this
        // SerializedProperty represents...
        var fieldInfo = parentComponentType.GetField(property.propertyPath);
        // ... using that we can return the raw .net type!
        return fieldInfo.FieldType;
    }
}