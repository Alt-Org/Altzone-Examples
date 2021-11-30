namespace Examples.Game.Scripts.Battle.interfaces
{
    /// <summary>
    /// Interface to handle "catch a ball" when player touches the ball.
    /// </summary>
    public interface ICatchABall
    {
        void catchABall(IBallControl ball, int playerPos);
    }
}