using System;
using System.Collections.Generic;
using UnityEngine;

namespace SokobanSummer.Core
{
    /// <summary>
    /// Simple Service Locator for centralized singleton management.
    /// Services are registered at startup (typically by GameInitializer) and accessed via Get&lt;T&gt;().
    /// This replaces ad-hoc static Instance patterns and provides a single, testable service access point.
    /// </summary>
    /// <remarks>
    /// Usage:
    /// - Register: ServiceLocator.Register&lt;IMyService&gt;(myServiceInstance);
    /// - Retrieve: var service = ServiceLocator.Get&lt;IMyService&gt;();
    /// - Check: if (ServiceLocator.TryGet&lt;IMyService&gt;(out var service)) { ... }
    /// 
    /// Thread Safety: This locator is designed for main-thread Unity usage only.
    /// Lifecycle: Services persist until domain reload or explicit Clear() call.
    /// </remarks>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static readonly object lockObject = new object();

        /// <summary>
        /// Register a service instance. Overwrites any existing registration for the same type.
        /// </summary>
        /// <typeparam name="T">Service type (typically an interface or concrete class).</typeparam>
        /// <param name="instance">Service instance to register.</param>
        public static void Register<T>(T instance) where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                if (services.ContainsKey(type))
                {
                    Debug.LogWarning($"[ServiceLocator] Overwriting existing registration for {type.Name}");
                }
                services[type] = instance;
                Debug.Log($"[ServiceLocator] Registered {type.Name}");
            }
        }

        /// <summary>
        /// Retrieve a registered service instance. Throws if not found.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>Service instance.</returns>
        /// <exception cref="InvalidOperationException">Thrown if service is not registered.</exception>
        public static T Get<T>() where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                if (services.TryGetValue(type, out var service))
                {
                    return service as T;
                }
                throw new InvalidOperationException($"[ServiceLocator] Service {type.Name} not registered. Ensure it's registered in GameInitializer before use.");
            }
        }

        /// <summary>
        /// Attempt to retrieve a registered service instance.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <param name="service">Output service instance if found, null otherwise.</param>
        /// <returns>True if service was found, false otherwise.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                if (services.TryGetValue(type, out var obj))
                {
                    service = obj as T;
                    return service != null;
                }
                service = null;
                return false;
            }
        }

        /// <summary>
        /// Check if a service is registered.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>True if registered, false otherwise.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            lock (lockObject)
            {
                return services.ContainsKey(typeof(T));
            }
        }

        /// <summary>
        /// Unregister a service. Useful for testing or manual teardown.
        /// </summary>
        /// <typeparam name="T">Service type.</typeparam>
        /// <returns>True if service was found and removed, false otherwise.</returns>
        public static bool Unregister<T>() where T : class
        {
            lock (lockObject)
            {
                var type = typeof(T);
                if (services.Remove(type))
                {
                    Debug.Log($"[ServiceLocator] Unregistered {type.Name}");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// Clear all registered services. Useful for testing or explicit teardown.
        /// </summary>
        public static void Clear()
        {
            lock (lockObject)
            {
                services.Clear();
                Debug.Log("[ServiceLocator] Cleared all services");
            }
        }

        /// <summary>
        /// Get count of registered services. Useful for validation.
        /// </summary>
        public static int Count
        {
            get
            {
                lock (lockObject)
                {
                    return services.Count;
                }
            }
        }

        /// <summary>
        /// Get all registered service types. Useful for debugging and validation.
        /// </summary>
        public static IEnumerable<Type> GetRegisteredTypes()
        {
            lock (lockObject)
            {
                return new List<Type>(services.Keys);
            }
        }
    }
}
