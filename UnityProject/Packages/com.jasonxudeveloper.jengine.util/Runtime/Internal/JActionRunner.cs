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
    /// Static runner that drives <see cref="JAction"/> execution on the main thread.
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
        private static readonly ConcurrentQueue<JAction> PendingQueue = new();
        private static readonly List<JAction> ActiveActions = new(32);
        private static bool _runtimeInitialized;

#if UNITY_EDITOR
        private static bool _editorInitialized;
#endif

        /// <summary>
        /// Marker struct for identifying our update in the PlayerLoop.
        /// </summary>
        private struct JActionUpdate : IEquatable<JActionUpdate>
        {
            public bool Equals(JActionUpdate other) => true;
            public override bool Equals(object obj) => obj is JActionUpdate;
            public override int GetHashCode() => 0;
        }

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
                ActiveActions.Clear();
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
        /// Registers a <see cref="JAction"/> for main-thread execution.
        /// </summary>
        /// <param name="action">The action to register. Null values are ignored.</param>
        /// <remarks>
        /// Thread-safe and lock-free via ConcurrentQueue.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void Register(JAction action)
        {
            if (action == null) return;
            PendingQueue.Enqueue(action);
        }

        private static void Update()
        {
            // Drain pending queue into active list (single-threaded, no lock needed)
            while (PendingQueue.TryDequeue(out var action))
            {
                ActiveActions.Add(action);
            }

            if (ActiveActions.Count == 0) return;

            // Process in reverse to allow safe removal during iteration
            for (int i = ActiveActions.Count - 1; i >= 0; i--)
            {
                var action = ActiveActions[i];
                if (action == null)
                {
                    ActiveActions.RemoveAt(i);
                    continue;
                }

                try
                {
                    if (action.Tick())
                    {
                        ActiveActions.RemoveAt(i);
                    }
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                    ActiveActions.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// Gets the approximate number of currently active JActions.
        /// </summary>
        public static int ActiveCount => ActiveActions.Count + PendingQueue.Count;
    }
}
