using AhorcadoClient.CallbackServiceReference;
using System;

namespace AhorcadoClient.CallbackServices
{
    public class GameCallbackClient : IGameManagerCallback
    {
        public Action<int, PlayerInfoDTO> OnPlayerJoinedAction { get; set; }
        public Action<int, int> OnPlayerLeftAction { get; set; }
        public Action<int, MatchInfoDTO> OnMatchReadyAction { get; set; }
        public Action<int, string, bool, int, bool> OnLetterGuessedAction { get; set; }

        public GameCallbackClient()
        {
        }

        public void OnPlayerJoined(int matchId, PlayerInfoDTO player)
        {
            if (OnPlayerJoinedAction != null)
                OnPlayerJoinedAction(matchId, player);
        }

        public void OnPlayerLeft(int matchId, int playerId)
        {
            if (OnPlayerLeftAction != null)
                OnPlayerLeftAction(matchId, playerId);
        }

        public void OnMatchReady(int matchId, MatchInfoDTO matchInfo)
        {
            if (OnMatchReadyAction != null)
                OnMatchReadyAction(matchId, matchInfo);
        }

        public void OnLetterGuessed(int matchId, string letter, bool isCorrect, int remainingAttempts, bool isGameOver)
        {
            if (OnLetterGuessedAction != null)
                OnLetterGuessedAction(matchId, letter, isCorrect, remainingAttempts, isGameOver);
        }
    }
}