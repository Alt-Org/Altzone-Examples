using Examples.Game.Scripts.Battle.interfaces;
using UnityEngine;

namespace Examples.Game.Scripts.Battle.Scene
{
    public class SceneConfig : MonoBehaviour
    {
        /// <summary>
        /// Camera for this scene.
        /// </summary>
        public Camera _camera;

        /// <summary>
        /// Is game camera rotated upside down (180 degrees)?
        /// </summary>
        public bool isCameraRotated { get; private set; }

        /// <summary>
        /// Nice actors can put themselves here - not to pollute top <c>GameObject</c> hierarchy.
        /// </summary>
        public GameObject actorParent;

        /// <summary>
        /// Game arena
        /// </summary>
        public IGameArena gameArena => _gameArena;

        [SerializeField] private GameArena _gameArena;

        /// <summary>
        /// Player start (instantiation) positions on game arena.
        /// </summary>
        public Transform[] playerStartPos = new Transform[4];

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public Collider2D upperTeamCollider;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public Collider2D lowerTeamCollider;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for upper team activity.
        /// </summary>
        public SpriteRenderer upperTeamSprite;

        /// <summary>
        /// Ball needs to know where it travels and collides, this is area for lower team activity.
        /// </summary>
        public SpriteRenderer lowerTeamSprite;

        public static SceneConfig Get()
        {
            if (_Instance == null)
            {
                _Instance = FindObjectOfType<SceneConfig>();
                if (_Instance == null)
                {
                    throw new UnityException("SceneConfig not found");
                }
            }
            return _Instance;
        }

        private static SceneConfig _Instance;

        public Rect getPlayArea(int playerPos)
        {
            // For convenience player start positions are kept under corresponding play area as child objects.
            // - play area is marked by collider to get its bounds for player area calculation!
            var playAreaTransform = playerStartPos[playerPos].parent;
            var center = playAreaTransform.position;
            var bounds = playAreaTransform.GetComponent<Collider2D>().bounds;
            return calculateRectFrom(center, bounds);
        }

        public void rotateGameCamera(bool upsideDown)
        {
            // Rotate game camera for upper team
            var cameraTransform = _camera.transform;
            isCameraRotated = upsideDown;
            var rotation = upsideDown
                ? Quaternion.Euler(0f, 0f, 180f) // Upside down
                : Quaternion.Euler(0f, 0f, 0f); // Normal orientation
            cameraTransform.rotation = rotation;
        }

        private static Rect calculateRectFrom(Vector3 center, Bounds bounds)
        {
            var extents = bounds.extents;
            var size = bounds.size;
            var x = center.x - extents.x;
            var y = center.y - extents.y;
            var width = size.x;
            var height = size.y;
            return new Rect(x, y, width, height);
        }
    }
}