using System;

namespace Examples2.Scripts.Battle.interfaces
{
    internal interface IPlayerManager
    {
        void StartCountdown(Action countdownFinished);
        void StartGameplay();
        void StopGameplay();
    }
}