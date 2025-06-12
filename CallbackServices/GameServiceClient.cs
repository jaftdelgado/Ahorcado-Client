using AhorcadoClient.CallbackServiceReference;
using System.ServiceModel;

namespace AhorcadoClient.CallbackServices
{
    public class GameServiceClient
    {
        private GameManagerClient _client;
        public GameCallbackClient Callback { get; private set; }

        public GameServiceClient(GameCallbackClient callback)
        {
            Callback = callback;
            var instanceContext = new InstanceContext(callback);
            _client = new GameManagerClient(instanceContext);
        }

        public void JoinMatch(int matchId, PlayerInfoDTO player, WordInfoDTO word, int maxAttempts = 6)
        {
            _client.JoinMatch(matchId, player, word, maxAttempts);
        }

        public void LeaveMatch(int matchId, int playerId)
        {
            _client.LeaveMatch(matchId, playerId);
        }

        public void GuessLetter(int matchId, int playerId, string letter)
        {
            _client.GuessLetter(matchId, playerId, letter);
        }

        public void Close()
        {
            if (_client.State == CommunicationState.Opened)
                _client.Close();
        }

        public bool IsConnected => _client?.State == CommunicationState.Opened;

        public void EnsureConnection()
        {
            if (_client.State != CommunicationState.Opened)
            {
                _client.Abort();
                var instanceContext = new InstanceContext(Callback);
                _client = new GameManagerClient(instanceContext);
            }
        }
    }
}
