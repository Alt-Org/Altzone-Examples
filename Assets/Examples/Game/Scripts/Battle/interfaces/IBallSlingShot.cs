namespace Examples.Game.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to start the ball aka put the ball into play by sling shot.
    /// </summary>
    public interface IBallSlingShot
    {
        void startBall();
        float currentDistance { get; }
    }
}