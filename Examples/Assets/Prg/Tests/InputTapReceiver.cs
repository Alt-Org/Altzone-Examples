using System.Collections;
using UnityEngine;

namespace Prg.Tests
{
    public class InputTapReceiver : MonoBehaviour, IInputTapReceiver
    {
        private IEnumerator Start()
        {
            Debug.Log($"start {Time.frameCount}");
            InputSlowTapHandlerTest implementation = null;
            yield return new WaitUntil(() => (implementation = FindObjectOfType<InputSlowTapHandlerTest>()) != null);
            yield return new WaitUntil(() => implementation.enabled);
            if (implementation is IInputTapHandler handler)
            {
                handler.SetTapReceiver(this);
            }
            Debug.Log($"done {Time.frameCount}");
        }

        void IInputTapReceiver.Tap(Vector2 position)
        {
            Debug.Log($"pos {position}");
        }

        void IInputTapReceiver.LongTap(Vector2 position)
        {
            Debug.Log($"pos {position}");
        }
    }
}