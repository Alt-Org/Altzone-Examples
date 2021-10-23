namespace Examples.Game.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to start the ball aka put the ball into play by sling shot.
    /// </summary>
    public interface IBallSlingShot
    {
        /// <summary>
        /// Starts the ball in motion.
        /// </summary>
        void startBall();

        /// <summary>
        /// Current distance between sling head and tail positions.
        /// </summary>
        float currentDistance { get; }

        /// <summary>
        /// Attack force of current players "holding" the sling.
        /// </summary>
        float attackForce { get; }
    }
}