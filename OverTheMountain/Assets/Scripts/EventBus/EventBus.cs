using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

public static class EventBus
{
    private static List<EventSubscriber> subscribers = new List<EventSubscriber>();

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Initialize()
    {
        GameObject gc = new GameObject("EventGarbageCollector");
        UnityEngine.Object.DontDestroyOnLoad(gc);
        gc.AddComponent<EventGarbageCollector>();
    }

    public static void Register(MonoBehaviour o)
    {
        IEnumerable<MethodInfo> methods = o.GetType().GetMethods().Where(m => m.GetCustomAttribute(typeof(SubscribeEventAttribute), true) != null);

        foreach (MethodInfo method in methods)
        {
            ParameterInfo[] parameters = method.GetParameters();
            if (parameters.Length != 1 || !typeof(IEventBase).IsAssignableFrom(parameters[0].ParameterType))
            {
                Debug.LogErrorFormat("Object {0}, EventSubscriber function {1} does not contain a valid parameter of type EventBase", o.GetType().Name, method.Name);
                continue;
            }

            subscribers.Add(new EventSubscriber()
            {
                instance = o,
                eventType = parameters[0].ParameterType,
                function = method
            });
        }
    }

    public static void UnRegister(MonoBehaviour o)
    {
        // Filter out all instances of o
        subscribers = subscribers.Where(s => s.instance != o).ToList();
    }

    public static void Post(IEventBase e)
    {
        Type eventType = e.GetType();
        // Find all living subscribers of matching event type
        IEnumerable<EventSubscriber> candidates = subscribers.Where(s => s.eventType == eventType && s.instance);

        // Invoke all subscribers
        foreach (EventSubscriber sub in candidates)
        {
            try
            {
                sub.function.Invoke(sub.instance, new object[] { e });
            }
            catch (Exception ex)
            {
                Debug.LogError(ex.StackTrace);
            }
        }
    }

    private sealed class EventGarbageCollector : MonoBehaviour
    {
        private void Awake()
        {
            InvokeRepeating("GC", 5F, 5F);
        }

        private void GC()
        {
            // Filter out all dead subscribers
            subscribers = subscribers.Where(s => s.instance).ToList();
        }
    }

    private struct EventSubscriber
    {
        public MonoBehaviour instance;
        public Type eventType;
        public MethodInfo function;
    }
}
