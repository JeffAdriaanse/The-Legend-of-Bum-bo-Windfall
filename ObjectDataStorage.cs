using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace The_Legend_of_Bum_bo_Windfall
{
    /// <summary>
    /// Stores floats on objects. Useful for keeping track of custom data specific to an object and that should be lost when the object no longer exists.
    /// </summary>
    public class ObjectDataStorage
    {
        public static List<ObjectDataStorage> Containers = new List<ObjectDataStorage>();

        public Dictionary<string, float> data = new Dictionary<string, float>();

        public object storageObject;

        /// <summary>
        /// Clears unused storage containers.
        /// </summary>
        private static void ClearUnusedContainers()
        {
            foreach (ObjectDataStorage container in Containers)
            {
                if (container == null || container.storageObject == null)
                {
                    Containers.Remove(container);
                }
            }
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

            foreach (ObjectDataStorage container in Containers)
            {
                if (container.storageObject == targetObject)
                {
                    return container;
                }
            }

            return null;
        }

        /// <summary>
        /// Stores a float value on the given object, associated with the provided key.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="key">The key associated with the stored value.</param>
        /// <param name="value">The value to store.</param>
        public static void StoreData(object targetObject, string key, float value)
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
        /// Retrieves a float value stored on the given object, associated with the provided key.
        /// </summary>
        /// <param name="targetObject">The target object.</param>
        /// <param name="key">The key associated with the stored value.</param>
        /// <returns>The stored value, or NaN if no value is found.</returns>
        public static float GetData(object targetObject, string key)
        {
            ObjectDataStorage container = FindContainer(targetObject);

            if (container != null)
            {
                if (container.data.TryGetValue(key, out float value))
                {
                    return value;
                }
            }

            return float.NaN;
        }
    }
}
