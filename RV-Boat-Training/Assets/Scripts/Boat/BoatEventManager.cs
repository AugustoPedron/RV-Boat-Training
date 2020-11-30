using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;

public class BoatEventManager : MonoBehaviour
{
    private Dictionary<string, Action> eventDictionary;
    private Dictionary<string, Action<float>> eventDictionaryFloat;

    private static BoatEventManager eventManager;

    public static BoatEventManager instance
    {
        get
        {
            if (!eventManager)
            {
                eventManager = FindObjectOfType(typeof(BoatEventManager)) as BoatEventManager;

                if (!eventManager)
                {
                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
                }
                else
                {
                    eventManager.Init();
                }
            }

            return eventManager;
        }
    }

    void Init()
    {
        if (eventDictionary == null)
        {
            eventDictionary = new Dictionary<string, Action>();
            eventDictionaryFloat = new Dictionary<string, Action<float>>();
        }
    }

    public static void StartListening(string eventName, Action listener)
    {
        Action thisEvent;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            instance.eventDictionary.Add(eventName, thisEvent);
        }
    }

    public static void StopListening(string eventName, Action listener)
    {
        if (eventManager == null) return;
        Action thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            instance.eventDictionary[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(string eventName)
    {
        Action thisEvent = null;
        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke();
        }
    }

    public static void StartListening(string eventName, Action<float> listener)
    {
        Action<float> thisEvent;
        if (instance.eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
        {
            thisEvent += listener;
            instance.eventDictionaryFloat[eventName] = thisEvent;
        }
        else
        {
            thisEvent += listener;
            instance.eventDictionaryFloat[eventName] = thisEvent;
        }
    }

    public static void StopListening(string eventName, Action<float> listener)
    {
        if (eventManager == null) return;
        Action<float> thisEvent = null;
        if (instance.eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
        {
            thisEvent -= listener;
            instance.eventDictionaryFloat[eventName] = thisEvent;
        }
    }

    public static void TriggerEvent(string eventName, float param)
    {
        Action<float> thisEvent = null;
        if (instance.eventDictionaryFloat.TryGetValue(eventName, out thisEvent))
        {
            thisEvent.Invoke(param);
        }
    }
}
