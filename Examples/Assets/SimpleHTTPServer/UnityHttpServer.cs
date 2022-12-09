using UnityEngine;

namespace SimpleHTTPServer
{
    internal class UnityHttpServer : MonoBehaviour
    {
        [SerializeField] private int _port;
        [SerializeField] private MonoBehaviour _controller;

        private SimpleHttpServer _myServer;

        private void Awake()
        {
            _myServer = new SimpleHttpServer(Application.streamingAssetsPath, _port, _controller);
            _myServer.OnJsonSerialized += JsonUtility.ToJson;
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            _myServer.Stop();
        }
    }
}