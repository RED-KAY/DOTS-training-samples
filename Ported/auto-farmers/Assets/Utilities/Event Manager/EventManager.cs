using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System;
using Utilities.Singleton;

namespace Utilities.EventManager
{
    public class EventManager : SingletonPersistent<EventManager>
    {
        private static Dictionary<string, List<InvokableActionBase>> globalEventTable = new Dictionary<string, List<InvokableActionBase>>();

        protected virtual void Init()
        {
            if (globalEventTable == null)
            {
                globalEventTable = new Dictionary<string, List<InvokableActionBase>>();
            }
        }

        private static List<InvokableActionBase> GetActionList(string eventName)
        {
            if (globalEventTable.TryGetValue(eventName, out var value))
            {
                return value;
            }
            return null;
        }

        private static void CheckForEventRemoval(string eventName, List<InvokableActionBase> actionList)
        {
            if (actionList.Count == 0)
            {
                globalEventTable.Remove(eventName);
            }
        }
        
        /// <summary>
        /// A static interface to subscribe to a default Action
        /// </summary>
        /// <param name="eventName">The name of the Action to subscribe</param>
        /// <param name="action">The Action to be executed on intercepting the Action</param>
        public static void StartListening(string eventName, Action action)
        {
            InvokableAction invokableAction = new InvokableAction();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }
        
        /// <summary>
        /// A static interface to subscribe to a single parameter Action
        /// </summary>
        /// <param name="eventName">The name of the Action to subscribe</param>
        /// <param name="action">The Action to be executed on intercepting the Action</param>
        public static void StartListening<T1>(string eventName, Action<T1> action)
        {
            InvokableAction<T1> invokableAction = new InvokableAction<T1>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }
        
        /// <summary>
        /// A static interface to subscribe to two parameters Action
        /// </summary>
        /// <param name="eventName">The name of the Action to subscribe</param>
        /// <param name="action">The Action to be executed on intercepting the Action</param>
        public static void StartListening<T1, T2>(string eventName, Action<T1, T2> action)
        {
            InvokableAction<T1, T2> invokableAction = new InvokableAction<T1, T2>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }
        
        /// <summary>
        /// A static interface to subscribe to three parameters Action
        /// </summary>
        /// <param name="eventName">The name of the Action to subscribe</param>
        /// <param name="action">The Action to be executed on intercepting the Action</param>
        public static void StartListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            InvokableAction<T1, T2, T3> invokableAction = new InvokableAction<T1, T2, T3>();
            invokableAction.Initialize(action);
            RegisterEvent(eventName, invokableAction);
        }

        private static void RegisterEvent(string eventName, InvokableActionBase listener)
        {
            if (globalEventTable.TryGetValue(eventName, out var thisEvent))
            {
                thisEvent.Add(listener);
            }
            else
            {
                thisEvent = new List<InvokableActionBase>();
                thisEvent.Add(listener);
                globalEventTable.Add(eventName, thisEvent);
            }
        }

        /// <summary>
        /// A static interface to unsubscribe to a default UnityEvent
        /// </summary>
        /// <param name="eventName">The name of the UnityEvent to unsubsubscribe from</param>
        /// <param name="listener">The listener that needs to be removed</param>
        public static void StopListening(string eventName, Action listener)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList == null)
            {
                return;
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                InvokableAction invokableAction = actionList[i] as InvokableAction;
                if (invokableAction.IsAction(listener))
                {
                    //GSGenericObjectPool.Return<GSInvokableAction>(invokableAction);
                    actionList.RemoveAt(i);
                    break;
                }
            }
            CheckForEventRemoval(eventName, actionList);
        }

        public static void StopListening<T1>(string eventName, Action<T1> action)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList == null)
            {
                return;
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                InvokableAction<T1> invokableAction = actionList[i] as InvokableAction<T1>;
                if (invokableAction.IsAction(action))
                {
                    //GSGenericObjectPool.Return<GSInvokableAction<T1>>(invokableAction);
                    actionList.RemoveAt(i);
                    break;
                }
            }
            CheckForEventRemoval(eventName, actionList);
        }
        
        public static void StopListening<T1, T2>(string eventName, Action<T1, T2> action)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList == null)
            {
                return;
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                InvokableAction<T1, T2> invokableAction = actionList[i] as InvokableAction<T1, T2>;
                if (invokableAction.IsAction(action))
                {
                    //GSGenericObjectPool.Return<GSInvokableAction<T1>>(invokableAction);
                    actionList.RemoveAt(i);
                    break;
                }
            }
            CheckForEventRemoval(eventName, actionList);
        }
        
        public static void StopListening<T1, T2, T3>(string eventName, Action<T1, T2, T3> action)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList == null)
            {
                return;
            }
            for (int i = 0; i < actionList.Count; i++)
            {
                InvokableAction<T1, T2, T3> invokableAction = actionList[i] as InvokableAction<T1, T2, T3>;
                if (invokableAction.IsAction(action))
                {
                    //GSGenericObjectPool.Return<GSInvokableAction<T1>>(invokableAction);
                    actionList.RemoveAt(i);
                    break;
                }
            }
            CheckForEventRemoval(eventName, actionList);
        }

        /// <summary>
        /// A static interface to trigger a UnityEvent
        /// </summary>
        /// <param name="eventName">The name of the UnityEvent that needs to be triggered</param>
        public static void TriggerEvent(string eventName)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList != null)
            {
                for (int num = actionList.Count - 1; num >= 0; num--)
                {
                    (actionList[num] as InvokableAction).Invoke();
                }
            }
        }

        /// <summary>
        /// A static interface to trigger a UnityEvent having single string parameter
        /// </summary>
        /// <param name="eventName">The name of the UnityEvent that needs to be triggered</param>
        /// <param name="param">The object of T type that is to be passed to the listener</param>
        public static void TriggerEvent<T1>(string eventName, T1 param)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList != null)
            {
                for (int num = actionList.Count - 1; num >= 0; num--)
                {
                    (actionList[num] as InvokableAction<T1>).Invoke(param);
                }
            }
        }
        
        public static void TriggerEvent<T1, T2>(string eventName, T1 param, T2 param2)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList != null)
            {
                for (int num = actionList.Count - 1; num >= 0; num--)
                {
                    (actionList[num] as InvokableAction<T1, T2>).Invoke(param, param2);
                }
            }
        }
        
        public static void TriggerEvent<T1, T2, T3>(string eventName, T1 param, T2 param2, T3 param3)
        {
            List<InvokableActionBase> actionList = GetActionList(eventName);
            if (actionList != null)
            {
                for (int num = actionList.Count - 1; num >= 0; num--)
                {
                    (actionList[num] as InvokableAction<T1, T2, T3>).Invoke(param, param2, param3);
                }
            }
        }
        
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        public static void DomainReset()
        {
            if (globalEventTable != null)
            {
                globalEventTable.Clear();
            }
        }
    }
}

