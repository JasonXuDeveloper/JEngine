// MessageBox.cs
// 
//  Author:
//        JasonXuDeveloper <jason@xgamedev.net>
// 
//  Copyright (c) 2025 JEngine
// 
//  Permission is hereby granted, free of charge, to any person obtaining a copy
//  of this software and associated documentation files (the "Software"), to deal
//  in the Software without restriction, including without limitation the rights
//  to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//  copies of the Software, and to permit persons to whom the Software is
//  furnished to do so, subject to the following conditions:
// 
//  The above copyright notice and this permission notice shall be included in
//  all copies or substantial portions of the Software.
// 
//  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//  OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
//  THE SOFTWARE.

using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace JEngine.Core.Misc
{
    public class MessageBox
    {
        /// <summary>
        /// Awaitable class for MessageBox operations
        /// </summary>
        private class MessageBoxAwaiter : IUniTaskSource<bool>
        {
            private UniTaskCompletionSourceCore<bool> _core;

            public bool GetResult(short token) => _core.GetResult(token);

            void IUniTaskSource.GetResult(short token)
            {
                _core.GetResult(token);
            }

            public UniTaskStatus GetStatus(short token) => _core.GetStatus(token);
            public UniTaskStatus UnsafeGetStatus() => _core.UnsafeGetStatus();

            public void OnCompleted(Action<object> continuation, object state, short token) =>
                _core.OnCompleted(continuation, state, token);

            /// <summary>
            /// Gets the UniTask for this awaiter
            /// </summary>
            public UniTask<bool> Task => new(this, _core.Version);

            /// <summary>
            /// Completes the task with the specified result (safe - won't throw if already completed)
            /// </summary>
            public bool TrySetResult(bool result) => _core.TrySetResult(result);

            /// <summary>
            /// Cancels the task (safe - won't throw if already completed)
            /// </summary>
            public void TrySetCanceled()
            {
                _core.TrySetCanceled();
            }

            /// <summary>
            /// Gets whether the task is already completed
            /// </summary>
            public bool IsCompleted => _core.GetStatus(_core.Version).IsCompleted();

            /// <summary>
            /// Resets the awaiter for reuse (zero allocation)
            /// </summary>
            public void Reset() => _core.Reset();
        }

        private static GameObject Prefab
        {
            get
            {
                if (_prefab != null) return _prefab;
                _prefab = Resources.Load<GameObject>("Prefabs/MessageBox");
                if (_prefab == null)
                {
                    Debug.LogError("Failed to load MessageBox prefab from Resources/MessageBox");
                }

                return _prefab;
            }
        }

        private static GameObject _prefab;

        private static readonly HashSet<MessageBox> ActiveMessageBoxes = new();
        private static readonly Stack<MessageBox> PooledMessageBoxes = new();

        private const int MaxPoolSize = 10;

        private TextMeshProUGUI _content;
        private TextMeshProUGUI _textNo;
        private TextMeshProUGUI _textOk;
        private TextMeshProUGUI _title;
        private Button _buttonOk;
        private Button _buttonNo;
        private CanvasGroup _canvasGroup;

        private bool _visible = true;
        private bool _isProcessingEvent; // Prevent double-click issues
        private readonly MessageBoxAwaiter _awaiter = new(); // Class instance, reusable
        private static readonly Vector3 InitialScale = new(0.9f, 0.9f, 0.9f);

        private MessageBox(string title, string content, string ok, string no)
        {
            if (Prefab == null)
            {
                throw new InvalidOperationException("MessageBox prefab is null. Cannot create MessageBox instance.");
            }

            GameObject = Object.Instantiate(Prefab);
            GameObject.name = "MessageBox";

            // Validate and cache component references
            if (!ValidateAndCacheComponents())
            {
                throw new InvalidOperationException(
                    "MessageBox prefab is missing required components. Please check the prefab structure.");
            }

            // Add listeners once during creation
            _buttonOk.onClick.AddListener(OnClickOk);
            _buttonNo.onClick.AddListener(OnClickNo);

            Init(title, content, ok, no);
        }

        private GameObject GameObject { get; set; }

        /// <summary>
        /// Gets the count of currently active message boxes
        /// </summary>
        public static int ActiveCount => ActiveMessageBoxes.Count;

        /// <summary>
        /// Gets the count of pooled message boxes
        /// </summary>
        public static int PooledCount => PooledMessageBoxes.Count;

        public static void Dispose()
        {
            // Clean up pooled instances
            while (PooledMessageBoxes.Count > 0)
            {
                var messageBox = PooledMessageBoxes.Pop();
                messageBox.Destroy();
            }

            // Clean up active instances and cancel any pending tasks
            foreach (var messageBox in ActiveMessageBoxes)
            {
                messageBox.CancelTask();
                messageBox.Destroy();
            }

            ActiveMessageBoxes.Clear();
        }

        public static void CloseAll()
        {
            // Create a copy to avoid modification during iteration
            var activeBoxes = new MessageBox[ActiveMessageBoxes.Count];
            ActiveMessageBoxes.CopyTo(activeBoxes);

            foreach (var messageBox in activeBoxes)
            {
                messageBox.CancelAndClose();
            }
        }

        /// <summary>
        /// Shows a message box and returns a UniTask that completes when a button is clicked
        /// Reuses class-based awaiter to avoid struct copying issues
        /// </summary>
        /// <param name="title">Title of the message box</param>
        /// <param name="content">Content/message text</param>
        /// <param name="ok">Text for the OK button (default: "确定"). Pass null to hide this button.</param>
        /// <param name="no">Text for the cancel/no button (default: "取消"). Pass null to hide this button.</param>
        /// <returns>UniTask that returns true if OK was clicked, false if Cancel/No was clicked</returns>
        /// <remarks>If both buttons are null/empty, a default OK button will be shown to prevent unusable message box.</remarks>
        public static UniTask<bool> Show(string title, string content, string ok = "确定", string no = "取消")
        {
            if (Prefab == null)
            {
                Debug.LogError("Cannot show MessageBox: Prefab is null");
                return UniTask.FromResult(false);
            }

            try
            {
                // Try to get from pool first
                MessageBox messageBox;
                if (PooledMessageBoxes.Count > 0)
                {
                    messageBox = PooledMessageBoxes.Pop();

                    // Validate and cache component references
                    if (!messageBox.ValidateAndCacheComponents())
                    {
                        throw new InvalidOperationException(
                            "MessageBox prefab is missing required components. Please check the prefab structure.");
                    }

                    messageBox.Init(title, content, ok, no);
                    messageBox.GameObject.SetActive(true);
                }
                else
                {
                    messageBox = new MessageBox(title, content, ok, no);
                }

                return messageBox._awaiter.Task;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to create MessageBox: {ex.Message}");
                return UniTask.FromResult(false);
            }
        }

        private bool ValidateAndCacheComponents()
        {
            try
            {
                _title ??= GetComponent<TextMeshProUGUI>("Title");
                _content ??= GetComponent<TextMeshProUGUI>("Content/Text");
                _textOk ??= GetComponent<TextMeshProUGUI>("Buttons/Ok/Text");
                _textNo ??= GetComponent<TextMeshProUGUI>("Buttons/No/Text");
                _buttonOk ??= GetComponent<Button>("Buttons/Ok");
                _buttonNo ??= GetComponent<Button>("Buttons/No");
                _canvasGroup ??= GameObject.GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = GameObject.AddComponent<CanvasGroup>();
                }

                // Validate critical components exist
                if (_title == null || _content == null || _textOk == null ||
                    _textNo == null || _buttonOk == null || _buttonNo == null)
                {
                    Debug.LogError("MessageBox prefab is missing required components");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to validate MessageBox components: {ex.Message}");
                return false;
            }
        }

        private void Destroy()
        {
            // Cancel any pending task before destroying
            CancelTask();

            // Clean up event listeners to prevent memory leaks
            if (_buttonOk != null)
            {
                _buttonOk.onClick.RemoveListener(OnClickOk);
            }

            if (_buttonNo != null)
            {
                _buttonNo.onClick.RemoveListener(OnClickNo);
            }

            // Clear references
            _title = null;
            _textOk = null;
            _textNo = null;
            _content = null;
            _buttonOk = null;
            _buttonNo = null;
            _canvasGroup = null;

            if (GameObject != null)
            {
                // Use Destroy instead of DestroyImmediate for safety
                if (Application.isPlaying)
                {
                    Object.Destroy(GameObject);
                }
                else
                {
                    Object.DestroyImmediate(GameObject);
                }

                GameObject = null;
            }
        }

        private void Init(string title, string content, string ok, string no)
        {
            // Validate components before using them
            if (_title == null || _content == null)
            {
                Debug.LogError("Cannot initialize MessageBox: missing components");
                return;
            }

            _title.text = title ?? "";
            _content.text = content ?? "";

            // Safety check: if both buttons are null/empty, show default OK button
            if (string.IsNullOrEmpty(ok) && string.IsNullOrEmpty(no))
            {
                ok = "确定"; // Default OK text
            }

            // Handle OK button visibility and text
            if (string.IsNullOrEmpty(ok))
            {
                if (_buttonOk != null) _buttonOk.gameObject.SetActive(false);
            }
            else
            {
                if (_buttonOk != null) _buttonOk.gameObject.SetActive(true);
                if (_textOk != null) _textOk.text = ok;
            }

            // Handle NO button visibility and text
            if (string.IsNullOrEmpty(no))
            {
                if (_buttonNo != null) _buttonNo.gameObject.SetActive(false);
            }
            else
            {
                if (_buttonNo != null) _buttonNo.gameObject.SetActive(true);
                if (_textNo != null) _textNo.text = no;
            }

            ActiveMessageBoxes.Add(this);
            _visible = true;
            _isProcessingEvent = false;

            // Reset the awaiter for reuse
            _awaiter.Reset();

            // Set initial state for animation and play it
            GameObject.transform.localScale = InitialScale;
            Object.DontDestroyOnLoad(GameObject);
        }

        private T GetComponent<T>(string path) where T : Component
        {
            try
            {
                var trans = GameObject?.transform.Find(path);
                return trans?.GetComponent<T>();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to get component at path '{path}': {ex.Message}");
                return null;
            }
        }

        private void OnClickNo()
        {
            HandleEvent(false);
        }

        private void OnClickOk()
        {
            HandleEvent(true);
        }

        private void HandleEvent(bool isOk)
        {
            // Prevent double-click issues
            if (_isProcessingEvent) return;
            _isProcessingEvent = true;

            // Complete the task with the result (safe - won't throw if already completed)
            bool wasCompleted = _awaiter.TrySetResult(isOk);

            // Only close if we successfully completed the task
            if (wasCompleted)
            {
                Close();
            }
        }

        private void Close()
        {
            if (!_visible) return; // Already closed

            Hide();

            // Remove from active set
            ActiveMessageBoxes.Remove(this);

            // Return to pool if not at capacity
            if (PooledMessageBoxes.Count < MaxPoolSize)
            {
                PooledMessageBoxes.Push(this);
            }
            else
            {
                // Pool is full, destroy this instance
                Destroy();
            }
        }

        private void CancelAndClose()
        {
            CancelTask();
            Close();
        }

        private void CancelTask()
        {
            // Safe cancellation - won't throw if already completed
            if (!_awaiter.IsCompleted)
            {
                _awaiter.TrySetCanceled();
            }
        }

        private void Hide()
        {
            if (GameObject != null)
            {
                GameObject.SetActive(false);
            }

            _visible = false;
        }
    }
}