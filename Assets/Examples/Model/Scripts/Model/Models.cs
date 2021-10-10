using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Examples.Model.Scripts.Model
{
    /// <summary>
    /// Container for named model objects.
    /// </summary>
    public static class Models
    {
        private static readonly Dictionary<string, AbstractModel> models = new Dictionary<string, AbstractModel>();

        public static void Add(AbstractModel model, string modelName)
        {
            var modelType = model.GetType();
            var key = $"{modelType.Name}.{modelName}";
            if (models.ContainsKey(key))
            {
                throw new UnityException($"model key already exists: {key}");
            }
            Debug.Log($"Add model: {key}={model}");
            models.Add(key, model);
        }

        public static T Get<T>(object modelName) where T : AbstractModel
        {
            var modelType = typeof(T);
            var key = $"{modelType.Name}.{modelName}";
            if (!models.TryGetValue(key, out var anyModel))
            {
                throw new UnityException($"model not found for key: {key}");
            }
            if (!(anyModel is T exactModel))
            {
                throw new UnityException(
                    $"model type {anyModel.GetType().Name} is different than excepted type  {modelType.Name}for key: {key}");
            }
            return exactModel;
        }

        public static List<T> GetAll<T>() where T : AbstractModel
        {
            return models.Values.OfType<T>().ToList();
        }
    }
}