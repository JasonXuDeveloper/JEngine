// JActionRunner.cs
// PlayerLoop-based driver for JAction main-thread execution (lock-free)
//
// Author: JasonXuDeveloper

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.LowLevel;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace JEngine.Util.Internal
{
    /// <summary>
    /// Static runner that drives <see cref="JActionExecutionContext"/> execution on the main thread.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Uses Unity's PlayerLoopSystem API in PlayMode and EditorApplication.update in EditMode,
    /// avoiding any dependency on MonoBehaviour or GameObjects.
    /// </para>
    /// <para>
    /// Lock-free design: uses ConcurrentQueue for thread-safe registration,
    /// single-threaded consumption on main thread.
    /// </para>
    /// </remarks>
    internal static class JActionRunner
    {
        private static readonly ConcurrentQueue<JActionExecutionContext> PendingQueue = new();
        private static readonly List<JActionExecutionContext> ActiveContexts = new(32);
        private static bool _runtimeInitialized;

#if UNITY_EDITOR
        private static bool _editorInitialized;
#endif

        /// <summary>
        /// Marker struct for identifying our update in the PlayerLoop.
        /// </summary>
        private struct JActionUpdate { }

#if UNITY_EDITOR
        [InitializeOnLoadMethod]
        private static void InitializeEditor()
        {
            if (_editorInitialized) return;
            _editorInitialized = true;

            EditorApplication.update -= Update;
            EditorApplication.update += Update;
            EditorApplication.playModeStateChanged += OnPlayModeStateChanged;
        }

        private static void OnPlayModeStateChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingPlayMode)
            {
                // Drain the queue
                while (PendingQueue.TryDequeue(out _)) { }
                ActiveContexts.Clear();
                _runtimeInitialized = false;
            }
        }
#endif

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void InitializeRuntime()
        {
            if (_runtimeInitialized) return;
            _runtimeInitialized = true;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();
            InsertUpdateSystem(ref playerLoop);
            PlayerLoop.SetPlayerLoop(playerLoop);
        }

        private static void InsertUpdateSystem(ref PlayerLoopSystem rootLoop)
        {
            var subSystems = rootLoop.subSystemList;
            if (subSystems == null) return;

            for (int i = 0; i < subSystems.Length; i++)
            {
                if (subSystems[i].type == typeof(UnityEngine.PlayerLoop.Update))
                {
                    ref var updateLoop = ref subSystems[i];
                    var updateSubSystems = updateLoop.subSystemList ?? Array.Empty<PlayerLoopSystem>();

                    var newSubSystems = new PlayerLoopSystem[updateSubSystems.Length + 1];
                    Array.Copy(updateSubSystems, newSubSystems, updateSubSystems.Length);

                    newSubSystems[updateSubSystems.Length] = new PlayerLoopSystem
                    {
                        type = typeof(JActionUpdate),
                        updateDelegate = Update
                    };

                    updateLoop.subSystemList = newSubSystems;
                    break;
                }
            }
        }

        /// <summary>
        /// Registers a <see cref="JActionExecutionContext"/> for main-thread execution.
        /// </summary>
        /// <param name="context">The context to register. Null values are ignored.</param>
        /// <remarks>
        /// Thread-safe and lock-free via ConcurrentQueue.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(JActionExecutionContext context)
        {
            if (context == null) return;
            PendingQueue.Enqueue(context);
        }

        private static void Update()
        {
            // Drain pending queue into active list (single-threaded, no lock needed)
            while (PendingQueue.TryDequeue(out var context))
            {
                ActiveContexts.Add(context);
            }

            if (ActiveContexts.Count == 0) return;

            // Process in reverse to allow safe removal during iteration
            for (int i = ActiveContexts.Count - 1; i >= 0; i--)
            {
                var context = ActiveContexts[i];
                if (context == null)
                {
                    ActiveContexts.RemoveAt(i);
                    continue;
                }

                try
                {
                    if (context.Tick())
                    {
                        ActiveContexts.RemoveAt(i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    ActiveContexts.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Gets the approximate number of currently active execution contexts.
        /// </summary>
        public static int ActiveCount => ActiveContexts.Count + PendingQueue.Count;
    }
}
