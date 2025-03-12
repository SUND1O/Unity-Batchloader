using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class BatchLoader : Editor
{
    static GameObject LoadedPrefab = null;
    static Transform LoadedTransform = null;
    #region Show Script Info
    [MenuItem("BatchLoader/Info", false, 0)]
    private static void Info()
    {
        EditorUtility.DisplayDialog(
            "BatchLoader v0.3",
            "BatchLoader functionality simplifies the process of replacing selected objects with a chosen prefab by providing an menu option in the Unity Editor.\n\n" +
            "1. Select a prefab in the Unity asset browser that you want to use as a replacement.\n\n" +
            "2. Right-click on the selected prefab and choose \"BatchLoader > Load this Prefab\" from the context menu.\n\n" +
            "3. Now, select the game objects in your scene that you want to swap with the loaded prefab.\n\n" +
            "4. In the Scene Hierarchy, right-click any selected object and find \"Batch Prefab Selected Objects\" in context menu.\n\n" +
            "NOTE: The swapping will erase object's children that aren't selected.",
            "OK");
    }
    #endregion

    #region Pinging Loaded Prefab
    [MenuItem("BatchLoader/Ping Loaded Prefab", true, 0)]
    private static bool ValidatePingLoadedPrefab()
    {
        return (LoadedPrefab != null);
    }

    [MenuItem("BatchLoader/Ping Loaded Prefab")]
    private static void PingLoadedPrefab()
    {
        if (LoadedPrefab != null)
        {
            EditorGUIUtility.PingObject(LoadedPrefab);
        }
    }
    #endregion

    #region Batch Prefabbing
    [MenuItem("GameObject/Batch Prefab Selected Objects", true, 10)]
    private static bool ValidateApplyPrefab()
    {

        GameObject[] selectedObjects = Selection.gameObjects;

        foreach (GameObject obj in selectedObjects)
        {
            if (!obj.scene.IsValid() || obj.GetComponent<Transform>() == null)
                return false;
        }

        return LoadedPrefab != null && selectedObjects.Length != 0;
    }

    [MenuItem("GameObject/Batch Prefab Selected Objects")]
    private static void ApplyPrefab()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        GameObject parentObject = new GameObject("SpawnedPrefabs");
        Undo.RegisterCreatedObjectUndo(parentObject, "Spawn Prefabs");

        foreach (GameObject obj in selectedObjects)
        {
            Transform objTransform = obj.transform;
            Vector3 position = objTransform.position;
            Quaternion rotation = objTransform.rotation;
            Vector3 scale = objTransform.localScale;
            
            GameObject spawnedPrefab = (GameObject)PrefabUtility.InstantiatePrefab(LoadedPrefab);
            spawnedPrefab.transform.position = position;
            spawnedPrefab.transform.rotation = rotation;
            spawnedPrefab.transform.localScale = scale;

            spawnedPrefab.transform.parent = parentObject.transform;

            Undo.RegisterCreatedObjectUndo(spawnedPrefab, "Spawn Prefabs");
        }

        foreach (GameObject obj in selectedObjects)
        {
            if (obj) Undo.DestroyObjectImmediate(obj);
        }
    }
    #endregion

    #region Loading Prefab
    [MenuItem("Assets/Load this Prefab for Batch Swapping", true, 10)]
    private static bool ValidateLoadPrefab()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        if (prefab != null)
        {
            string prefabName = prefab.name;
            Debug.Log("Selected prefab name: " + prefabName);
        }
        //is gameobject is a prefab & selected only 1
        return Selection.activeObject is GameObject && PrefabUtility.GetPrefabAssetType(Selection.activeObject) != PrefabAssetType.NotAPrefab && Selection.gameObjects.Length == 1;
    }

    [MenuItem("Assets/Load this Prefab for Batch Swapping")]
    private static void LoadPrefab()
    {
        GameObject prefab = Selection.activeObject as GameObject;

        LoadedPrefab = prefab;
        Debug.Log("Loaded a prefab named: " + LoadedPrefab.name);

    }
    #endregion

    #region utilities
    [MenuItem("GameObject/BatchUtils/Get Transform Data", true, 9)]
    private static bool ValidateGetTransform()
    {
        GameObject selectedObject = Selection.activeObject as GameObject;
        return selectedObject;
    }
    [MenuItem("GameObject/BatchUtils/Get Transform Data")]
    private static void GetTransform()
    {
        GameObject selectedObject = Selection.activeObject as GameObject;

        LoadedTransform = selectedObject.transform;
        Debug.Log("Loaded transform from: " + selectedObject.name);
    }

    [MenuItem("GameObject/BatchUtils/Set Transform Data", true, 9)]
    private static bool ValidateSetTransform()
    {
        GameObject prefab = Selection.activeObject as GameObject;
        return prefab && LoadedTransform;
    }
    [MenuItem("GameObject/BatchUtils/Set Transform Data")]
    private static void SetTransform()
    {
        GameObject selectedObject = Selection.activeObject as GameObject;

        Undo.RecordObject(selectedObject.transform, "Apply Transform");
        selectedObject.transform.position = LoadedTransform.position;
        selectedObject.transform.rotation = LoadedTransform.rotation;
        selectedObject.transform.localScale = LoadedTransform.localScale;
        Debug.Log("Applied transform to: " + selectedObject.name);
    }
    #endregion
}
