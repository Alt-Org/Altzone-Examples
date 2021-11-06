namespace Examples2.Scripts.Battle.interfaces
{
    public interface IPlayerLineConnector
    {
        void Connect(IPlayerActor playerActor);
        IPlayerActor GetNearest();
        void Hide();
    }
}