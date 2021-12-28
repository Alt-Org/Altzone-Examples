namespace Examples2.Scripts.Battle.interfaces
{
    internal enum ScoreType
    {
        PlayerHed = 0,
        BlueWall = 1,
        RedWall = 2
    }

    internal interface IScoreManager
    {
        void AddScore(ScoreType scoreType);

        void AddScore(ScoreType scoreType, int scoreAmount);
    }
}