namespace Examples2.Scripts.Battle.interfaces
{
    public interface IPlayerShield
    {
        void SetupShield(int playerPos);
        void SetShieldState(int playMode, int rotationIndex);
        void PlayHitEffects();
    }
}