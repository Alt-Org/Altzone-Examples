using System;
using UnityEngine;

namespace SimpleHTTPServer
{
    internal class TestController : MonoBehaviour
    {
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

        public ReturnResult CustomObjectReturnMethodWithQuery(int code, string msg)
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