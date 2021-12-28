namespace Examples2.Scripts.Battle.interfaces
{
    internal enum ScoreType
    {
        BlueHead = 1,
        RedHead = 2,
        BlueWall = 3,
        RedWall = 4
    }

    internal interface IScoreManager
    {
        void AddScore(ScoreType scoreType);

        void AddScore(ScoreType scoreType, int scoreAmount);
    }
}