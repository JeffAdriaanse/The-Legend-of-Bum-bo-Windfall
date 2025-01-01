using System.Collections.Generic;
using System.Linq;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Stores data on objects. Useful for keeping track of custom data specific to an object and that should be lost when the object no longer exists.
    /// </summary>
    public class ObjectDataStorage
    {
        public static List<ObjectDataStorage> Containers = new List<ObjectDataStorage>();

        public Dictionary<string, object> data = new Dictionary<string, object>();

        public object storageObject;

        /// <summary>
        /// Clears unused storage containers.
        /// </summary>
        private static void ClearUnusedContainers()
        {
            //Note: Storage objects must be cast to UnityEngine.Object and checked against null a second time. This is because Unity uses wrapper objects that sometimes persist after the object has been destroyed.
            //When the object is cast to UnityEngine.Object before the null check, the comparison operator properly check against null.
            //See https://blog.unity.com/technology/custom-operator-should-we-keep-it
            Containers.RemoveAll(container => (container == null || container.storageObject == null || (container.storageObject is UnityEngine.Object && (container.storageObject as UnityEngine.Object) == null)));
        }

        /// <summary>
        /// Finds a storage container that holds the given object.
        /// </summary>
        /// <param name="targetObject">The object to find a container of.</param>
        /// <returns>The first storage container that holds the given object.</returns>
        private static ObjectDataStorage FindContainer(object targetObject)
        {
            ClearUnusedContainers();

            if (targetObject == null)
            {
                return null;
            }

            return Containers.FirstOrDefault(container => container.storageObject == targetObject);
        }

        /// <summary>
        /// Stores data on the given object, associated with the provided key.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="key">The key associated with the stored data.</param>
        /// <param name="value">The data to store.</param>
        public static void StoreData<T>(object targetObject, string key, T value)
        {
            ObjectDataStorage container = FindContainer(targetObject);

            if (container != null)
            {
                container.data[key] = value;
                return;
            }

            ObjectDataStorage newContainer = new ObjectDataStorage();
            newContainer.storageObject = targetObject;
            newContainer.data.Add(key, value);
            Containers.Add(newContainer);
        }

        /// <summary>
        /// Retrieves data stored on the given object, associated with the provided key.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="key">The key associated with the stored data.</param>
        /// <returns>The stored data, or default value if no value is found.</returns>
        public static T GetData<T>(object targetObject, string key)
        {
            ObjectDataStorage container = FindContainer(targetObject);

            if (container != null)
            {
                if (container.data.TryGetValue(key, out object value))
                {
                    if (value is T typedValue)
                    {
                        return typedValue;
                    }
                }
            }

            return default(T);
        }

        /// <summary>
        /// Whether the object has data of the given type associated with the provided key.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="key">The key associated with the stored data.</param>
        /// <returns>True if data is found. False if no data is found.</returns>
        public static bool HasData<T>(object targetObject, string key)
        {
            ObjectDataStorage container = FindContainer(targetObject);

            if (container != null)
            {
                if (container.data.TryGetValue(key, out object value))
                {
                    if (value is T)
                    {
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
