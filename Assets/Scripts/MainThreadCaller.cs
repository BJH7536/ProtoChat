using UnityEngine;
using System.Collections.Generic;
using System;

public class MainThreadCaller : MonoBehaviour
{
    private static readonly Queue<Action> pendingActionsQueue = new Queue<Action>();
    private static MainThreadCaller instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        lock (pendingActionsQueue)
        {
            while (pendingActionsQueue.Count > 0)
            {
                pendingActionsQueue.Dequeue().Invoke();
            }
        }
    }

    public static void EnqueueAction(Action action)
    {
        lock (pendingActionsQueue)
        {
            pendingActionsQueue.Enqueue(action);

        }
    }
}
