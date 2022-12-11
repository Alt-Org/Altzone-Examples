using System;
using UnityEngine;

namespace SimpleHTTPServer
{
    internal class TestController : MonoBehaviour
    {
        private void Awake()
        {
            var methods = GetType().GetMethods();
            foreach (var method in methods)
            {
                if (method.IsPublic && method.Name.EndsWith("Method"))
                {
                    Debug.Log($"method {method.Name} returns {method.ReturnType.Name}");
                    foreach (var parameterInfo in method.GetParameters())
                    {
                        Debug.Log($" param {parameterInfo.Name} {parameterInfo.ParameterType.Name}");
                    }
                }
            }
        }

        public void SimpleMethod()
        {
            UnityEngine.Debug.Log("Cool, fire via http connect");
        }

        public string[] SimpleStringMethod()
        {
            return new[]
            {
                "result", "result2"
            };
        }

        public int[] SimpleIntMethod()
        {
            return new[]
            {
                1, 2
            };
        }

        public ReturnResult CustomObjectReturnMethod()
        {
            var result = new ReturnResult
            {
                code = 1,
                msg = "testing"
            };
            return result;
        }

        public ReturnResult CustomObjectReturnMethod(int code, string msg)
        {
            var result = new ReturnResult
            {
                code = code,
                msg = msg
            };
            return result;
        }

        [Serializable]
        public class ReturnResult
        {
            public string msg;
            public int code;
        }
    }
}