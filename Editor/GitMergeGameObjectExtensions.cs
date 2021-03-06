﻿using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

public static class GitMergeGameObjectExtensions
{
    //This dict holds all of "their" GameObjects
    //<GameObject, originallyActive>
    private static Dictionary<GameObject, bool> theirObjects = new Dictionary<GameObject, bool>();

	public static void SetAsMergeObject(this GameObject go, bool active)
    {
        if(!theirObjects.ContainsKey(go))
        {
            theirObjects.Add(go, go.activeSelf);
        }
        go.SetActiveForMerging(false);
    }

    public static void SetActiveForMerging(this GameObject go, bool active)
    {
        go.SetActive(active);
        go.hideFlags = active ? HideFlags.None : HideFlags.HideAndDontSave;
    }

    public static GameObject InstantiateForMerging(this GameObject go)
    {
        var copy = GameObject.Instantiate(go) as GameObject;
        
        bool wasActive;
        if(!theirObjects.TryGetValue(go, out wasActive))
        {
            wasActive = go.activeSelf;
        }

        copy.SetActive(wasActive);
        copy.hideFlags = HideFlags.None;
        copy.name = go.name;

        return copy;
    }

    public static void DestroyAllMergeObjects()
    {
        foreach(var obj in theirObjects.Keys)
        {
            Object.DestroyImmediate(obj);
        }
        theirObjects.Clear();
    }
    
    public static Component AddComponent(this GameObject go, Component original)
    {
        var c = go.AddComponent(original.GetType());

        var originalSerialized = new SerializedObject(original).GetIterator();
        var newSerialized = new SerializedObject(c).GetIterator();

        if(originalSerialized.Next(true))
        {
            newSerialized.Next(true);

            while(originalSerialized.NextVisible(false))
            {
                newSerialized.NextVisible(false);

                newSerialized.SetValue(originalSerialized.GetValue());
            }
        }
        
        return c;
    }

    public static void Highlight(this GameObject go)
    {
        Selection.activeGameObject = go;
        EditorGUIUtility.PingObject(go);

        var view = SceneView.lastActiveSceneView;
        if(view)
        {
            view.FrameSelected();
        }
    }
}
