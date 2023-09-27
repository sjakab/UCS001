using System.Collections.Generic;
using System.Diagnostics.Tracing;
using UnityEngine;

[CreateAssetMenu(menuName = "SO Event")]
public class SOEvent : ScriptableObject
{
    private List<SOEventListener> listeners = new List<SOEventListener>();
    public void TriggerEvent()
    {
        for (int i = listeners.Count - 1; i >= 0; i--)
        {
            listeners[i].OnEventTriggered();
        }
    }
    public void AddListener(SOEventListener listener)
    {
        listeners.Add(listener);
    }
    public void RemoveListener(SOEventListener listener)
    {
        listeners.Remove(listener);
    }
}