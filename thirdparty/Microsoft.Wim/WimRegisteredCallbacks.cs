// Copyright (c). All rights reserved.
//
// Licensed under the MIT license.

using System;
using System.Collections.Generic;

namespace Microsoft.Wim
{
    /// <summary>
    /// Represents a collection of registered callbacks.  Callbacks can be registered globally or per <see cref="WimHandle" />.
    /// </summary>
    /// <remarks>
    /// This class is not thread safe.  Locks should be used when adding, removing, or checking the existence of
    /// items.
    /// </remarks>
    internal sealed class WimRegisteredCallbacks
    {
        /// <summary>
        /// Stores a list of registered callbacks by their WimHandle.
        /// </summary>
        private readonly Dictionary<WimHandle, Dictionary<WimMessageCallback, WimMessageCallbackWrapper>> _registeredCallbacksByHandle = [];

        /// <summary>
        /// Stores a list of globally registered callbacks.
        /// </summary>
        private readonly Dictionary<WimMessageCallback, WimMessageCallbackWrapper> _registeredCallbacksGlobal = [];

        /// <summary>
        /// Gets a native callback for passing to the WIMGAPI for the specified registered callback associated with the
        /// <see cref="WimHandle" />.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle" /> of a windows image file.</param>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method that was registered.</param>
        /// <returns>A <see cref="WimgApi.WIMMessageCallback" /> method that can be passed to the native WIMGAPI.</returns>
        /// <exception cref="InvalidOperationException">
        /// The specified handle has no registered callbacks or the specified callback
        /// is not registered for the handle.
        /// </exception>
        public WimgApi.WIMMessageCallback GetNativeCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // Verify the callback is registered for the handle
            if (!IsCallbackRegistered(wimHandle, messageCallback))
            {
                throw new InvalidOperationException("Specified callback is not registered.");
            }

            // Return the native callback
            return _registeredCallbacksByHandle[wimHandle][messageCallback].NativeCallback;
        }

        /// <summary>
        /// Gets a native callback for passing to the WIMGAPI for the specified globally registered callback.
        /// </summary>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method that was registered.</param>
        /// <returns>A <see cref="WimgApi.WIMMessageCallback" /> method that can be passed to the native WIMGAPI.</returns>
        /// ///
        /// <exception cref="InvalidOperationException">The specified callback is not registered.</exception>
        public WimgApi.WIMMessageCallback GetNativeCallback(WimMessageCallback messageCallback)
        {
            // Verify the callback is registered
            if (!IsCallbackRegistered(messageCallback))
            {
                throw new InvalidOperationException("Specified callback is not registered.");
            }

            // Return the native callback
            return _registeredCallbacksGlobal[messageCallback].NativeCallback;
        }

        /// <summary>
        /// Gets a value indicating if the specified callback is registered for the handle.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle" /> of a windows image file.</param>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method that was registered.</param>
        /// <returns><c>true</c> if the callback is registered, otherwise <c>false</c>.</returns>
        public bool IsCallbackRegistered(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            return _registeredCallbacksByHandle.ContainsKey(wimHandle) && _registeredCallbacksByHandle[wimHandle].ContainsKey(messageCallback);
        }

        /// <summary>
        /// Gets a value indicating if the specified callback is globally registered.
        /// </summary>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method that was registered.</param>
        /// <returns><c>true</c> if the callback is registered, otherwise <c>false</c>.</returns>
        public bool IsCallbackRegistered(WimMessageCallback messageCallback)
        {
            return _registeredCallbacksGlobal.ContainsKey(messageCallback);
        }

        /// <summary>
        /// Registers a callback for the specified <see cref="WimHandle" />.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle" /> of a windows image file.</param>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method to register.</param>
        /// <param name="userData">User-defined data to pass to the callback.</param>
        /// <returns><c>true</c> if the callback was successfully registered, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">wimHandle or messageCallback is null.</exception>
        public bool RegisterCallback(WimHandle wimHandle, WimMessageCallback messageCallback, object userData)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if messageCallback is null
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // See if the callback is already registered
            if (IsCallbackRegistered(wimHandle, messageCallback))
            {
                return false;
            }

            // See if the dictionary doesn't contain the wimHandle
            if (!_registeredCallbacksByHandle.ContainsKey(wimHandle))
            {
                // Add an item for the wimHandle to the dictionary
                _registeredCallbacksByHandle.Add(wimHandle, []);
            }

            // Create a callback wrapper and add the callback to the list
            _registeredCallbacksByHandle[wimHandle].Add(messageCallback, new WimMessageCallbackWrapper(messageCallback, userData));

            return true;
        }

        /// <summary>
        /// Registers a callback globally.
        /// </summary>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method to register.</param>
        /// <param name="userData">User-defined data to pass to the callback.</param>
        /// <returns><c>true</c> if the callback was successfully registered, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        public bool RegisterCallback(WimMessageCallback messageCallback, object userData)
        {
            // See if messageCallback is null
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // See if the callback is already registered
            if (IsCallbackRegistered(messageCallback))
            {
                return false;
            }

            // Create a callback wrapper and add the callback to the list
            _registeredCallbacksGlobal.Add(messageCallback, new WimMessageCallbackWrapper(messageCallback, userData));

            return true;
        }

        /// <summary>
        /// Un-registers the specified callback for the <see cref="WimHandle" />.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle" /> of a windows image file.</param>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method to un-register.</param>
        /// <returns><c>true</c> if the callback was successfully un-registered, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">wimHandle or messageCallback is null.</exception>
        public bool UnregisterCallback(WimHandle wimHandle, WimMessageCallback messageCallback)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if messageCallback is null
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // See if the callback isn't registered
            if (!IsCallbackRegistered(wimHandle, messageCallback))
            {
                return false;
            }

            // Remove the callback from the list
            _ = _registeredCallbacksByHandle[wimHandle].Remove(messageCallback);

            // See if the dictionary for the wimHandle is now empty
            if (_registeredCallbacksByHandle[wimHandle].Count == 0)
            {
                // Remove the wimHandle dictionary item
                _ = _registeredCallbacksByHandle.Remove(wimHandle);
            }

            return true;
        }

        /// <summary>
        /// Un-registers the specified callback.
        /// </summary>
        /// <param name="messageCallback">The <see cref="WimMessageCallback" /> method to un-register.</param>
        /// <returns><c>true</c> if the callback was successfully un-registered, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">messageCallback is null.</exception>
        public bool UnregisterCallback(WimMessageCallback messageCallback)
        {
            // See if messageCallback is null
            if (messageCallback == null)
            {
                throw new ArgumentNullException(nameof(messageCallback));
            }

            // See if the callback isn't registered
            if (!IsCallbackRegistered(messageCallback))
            {
                return false;
            }

            // Remove the callback from the list
            _ = _registeredCallbacksGlobal.Remove(messageCallback);

            return true;
        }

        /// <summary>
        /// Un-registers all callbacks for the specified <see cref="WimHandle" />.
        /// </summary>
        /// <param name="wimHandle">A <see cref="WimHandle" /> of a windows image file.</param>
        /// <returns><c>true</c> if the all of the callbacks were successfully un-registered, otherwise <c>false</c>.</returns>
        /// <exception cref="ArgumentNullException">wimHandle is null.</exception>
        public bool UnregisterCallbacks(WimHandle wimHandle)
        {
            // See if wimHandle is null
            if (wimHandle == null)
            {
                throw new ArgumentNullException(nameof(wimHandle));
            }

            // See if the wimHandle doesn't have any registered callbacks
            if (!_registeredCallbacksByHandle.ContainsKey(wimHandle))
            {
                return false;
            }

            // Remove the wimHandle from the list
            _ = _registeredCallbacksByHandle.Remove(wimHandle);

            return true;
        }

        /// <summary>
        /// Un-registers all globally registered callbacks and callbacks associated with <see cref="WimHandle" /> objects.
        /// </summary>
        /// <returns><c>true</c> if the all of the callbacks were successfully un-registered, otherwise <c>false</c>.</returns>
        public bool UnregisterCallbacks()
        {
            // Clear the dictionary of handles and callbacks
            _registeredCallbacksByHandle.Clear();

            // Clear the list of globally registered callbacks
            _registeredCallbacksGlobal.Clear();

            return true;
        }
    }
}