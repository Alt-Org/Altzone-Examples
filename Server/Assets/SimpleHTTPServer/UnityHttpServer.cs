using UnityEngine;

namespace SimpleHTTPServer
{
    /// <summary>
    /// Adopted form https://github.com/sableangle/UnityHTTPServer
    /// </summary>
    internal class UnityHttpServer : MonoBehaviour
    {
        [SerializeField] private int _port;
        [SerializeField] private MonoBehaviour _controller;

        private SimpleHttpServer _myServer;

        public SimpleHttpServer SimpleHttpServer => _myServer;

        private void Awake()
        {
            name = nameof(UnityHttpServer);
            _myServer = new SimpleHttpServer(Application.streamingAssetsPath, _port, _controller);
            DontDestroyOnLoad(gameObject);
        }

        private void OnApplicationQuit()
        {
            _myServer.Stop();
        }
    }
}