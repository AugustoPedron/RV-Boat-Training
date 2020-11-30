//using System;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;
//using UnityEngine.Events;

//public class EventManager<T> : MonoBehaviour
//{

//    private Dictionary<string, UnityEvent<T>> eventDictionary;

//    private static EventManager<T> eventManager;


//    public static EventManager<T> instance
//    {
//        get
//        {
//            if (!eventManager)
//            {
//                eventManager = FindObjectOfType(typeof(EventManager<T>)) as EventManager<T>;

//                if (!eventManager)
//                {
//                    Debug.LogError("There needs to be one active EventManger script on a GameObject in your scene.");
//                }
//                else
//                {
//                    eventManager.Init();
//                }
//            }

//            return eventManager;
//        }
//    }

//    void Init()
//    {
//        if (eventDictionary == null)
//        {
//            eventDictionary = new Dictionary<string, UnityEvent<T>>();
//        }
//    }

//    public static void StartListening(string eventName, UnityAction<T> listener)
//    {
//        UnityEvent<T> thisEvent = null;
//        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
//        {
//            thisEvent.AddListener(listener);
//        }
//        else
//        {
//            thisEvent = (UnityEvent<T>)Activator.CreateInstance(typeof(UnityEvent<T>));
//            thisEvent.AddListener(listener);
//            instance.eventDictionary.Add(eventName, thisEvent);
//        }
//    }

//    public static void StopListening(string eventName, UnityAction<T> listener)
//    {
//        if (eventManager == null) return;
//        UnityEvent<T> thisEvent = null;
//        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
//        {
//            thisEvent.RemoveListener(listener);
//        }
//    }

//    public static void TriggerEvent(string eventName, T value)
//    {
//        UnityEvent<T> thisEvent = null;
//        if (instance.eventDictionary.TryGetValue(eventName, out thisEvent))
//        {
//            //if (thisEvent.GetType() == typeof(UnityEvent))
//            //{
//            //    thisEvent.Invoke();
//            //}
//            //else
//            //{
//            //    thisEvent.Invoke(value);
//            //}
//            thisEvent.Invoke(value);
//        }
//    }
//}
