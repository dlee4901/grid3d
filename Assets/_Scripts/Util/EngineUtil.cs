using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

public static class EngineUtil
{
    public static void SaveJsonData<T>(T data)
    { 
        string jsonData = JsonUtility.ToJson(data);
        string filePath = Application.persistentDataPath + "/" + typeof(T).Name + ".json";
        Debug.Log(filePath);
        File.WriteAllText(filePath, jsonData);
    }

    public static T LoadJsonData<T>()
    {
        string filePath = Application.persistentDataPath + "/" + typeof(T).Name + ".json";
        Debug.Log(filePath);
        string jsonData = File.ReadAllText(filePath);
        return JsonUtility.FromJson<T>(jsonData);
    }

    public static void GetMouseWorldPosition(Camera mainCamera, out Vector3 mouseWorldPosition, out bool error)
    {
        if (!mainCamera)
        {
            mouseWorldPosition = Vector3.zero;
            error = true;
            return;
        }
        var ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            mouseWorldPosition = hit.point;
            error = false;
        }
        else
        {
            mouseWorldPosition = Vector3.zero;
            error = true;
        }
    }

    public static Vector3 GetMousePosition(bool zeroed=true)
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        if (zeroed) mousePosition.z = 0f;
        return mousePosition;
    }

    public static IEnumerator LerpTransform(Transform transform, Vector3 src, Vector3 dst, float overTime)
    {
        float startTime = Time.time;
        while (Time.time < startTime + overTime)
        {
            transform.position = Vector3.Lerp(src, dst, (Time.time - startTime) / overTime);
            yield return null;
        }
        transform.position = dst;
    }

    public static T CreateGameObject<T>() where T : MonoBehaviour
    {
        GameObject gameObj = new GameObject();
        return gameObj.AddComponent<T>();
    }

    public static T CreateUIGameObject<T>() where T : MonoBehaviour
    {
        GameObject gameObj = new GameObject();
        gameObj.AddComponent<RectTransform>();
        return gameObj.AddComponent<T>();
    }

    public static void PrintList<T>(HashSet<T> set)
    {
        foreach (T item in set)
        {
            Debug.Log(item);
        }
    }

    public static void PrintList<T>(List<T> set)
    {
        foreach (T item in set)
        {
            Debug.Log(item);
        }
    }

    public static Vector2Int TupleToVector2Int(Tuple<int, int> tuple)
    {
        return new Vector2Int(tuple.Item1, tuple.Item2);
    }

    public static Tuple<int, int> Vector2IntToTuple(Vector2Int vector)
    {
        return new Tuple<int, int>(vector.x, vector.y);
    }

    public static List<int> Vector2IntRangeToIntList(Vector2Int vectorRange)
    {
        List<int> list = new List<int>();
        int start = vectorRange.x;
        int end = vectorRange.y;
        if (start > end)
        {
            for (int i = start; i >= end; i--) 
            {
                list.Add(i);
            }
        }
        else
        {
            for (int i = start; i <= end; i++) 
            {
                list.Add(i);
            }
        }
        return list;
    }

    public static T GetOrAddComponent<T>(GameObject gameObj) where T : UnityEngine.Component
    {
        if (!gameObj.TryGetComponent<T>(out T component))
        {
            Debug.Log("EngineUtil GetOrAddComponent unable to get component " + typeof(T) + ", adding component");
            return gameObj.AddComponent<T>();
        }
        return component;
    }


    // public static Dictionary<string, NetworkVariable<object>> GetNetworkPropertiesDictionary(object obj)
    // {
    //     Dictionary<string, NetworkVariable<object>> networkPropertiesDictionary = new Dictionary<string, NetworkVariable<object>>();
    //     foreach (PropertyInfo property in Util.GetProperties(obj))
    //     {
    //         Debug.Log(property.Name);
    //         Debug.Log(property.GetValue(obj));
    //         Type typeNetworkVariable = typeof(NetworkVariable<>);
    //         Type[] typeNetworkVariableArgs = {property.GetType()};
    //         Type typeConstructed = typeNetworkVariable.MakeGenericType(typeNetworkVariableArgs);
    //         object networkVariable = Activator.CreateInstance(typeConstructed);
    //         networkPropertiesDictionary.Add(property.Name, (NetworkVariable<object>)networkVariable);
    //     }
    //     return networkPropertiesDictionary;
    // }

    // public static Dictionary<string, NetworkVariable<object>> GetNetworkPropertiesDictionary(object obj)
    // {
    //     Dictionary<string, NetworkVariable<object>> networkPropertiesDictionary = new Dictionary<string, NetworkVariable<object>>();
    //     foreach (var (propertyName, propertyValue) in Util.GetPropertiesDictionary(obj))
    //     {
    //         Debug.Log(propertyName);
    //         Debug.Log(propertyValue);
    //         Debug.Log(propertyValue.GetType());
    //         Type typeNetworkVariable = typeof(NetworkVariable<>);
    //         Type[] typeNetworkVariableArgs = {propertyValue.GetType()};
    //         Type typeConstructed = typeNetworkVariable.MakeGenericType(typeNetworkVariableArgs);
    //         object networkVariable = Activator.CreateInstance(typeConstructed);
    //         networkPropertiesDictionary.Add(propertyName, (NetworkVariable<object>)networkVariable);
    //     }
    //     return networkPropertiesDictionary;
    // }
}