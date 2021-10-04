using Microsoft.ServiceFabric.Services.Communication.Runtime;
using Microsoft.ServiceFabric.Services.Remoting;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Runtime;
using Microsoft.ServiceFabric.Services.Runtime;
using PlayerCollection.Model;
using System;
using System.Collections.Generic;
using System.Fabric;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCollection
{
    internal sealed class PlayerCollection : StatefulService, IPlayerCollectionService, IService
    {

        private IPlayerRepository _playerRepository;
        private static int _partitionCounter = 3;

        public PlayerCollection(StatefulServiceContext context)
            : base(context)
        { }

        public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                await _playerRepository.AddPlayerAsync(player, cancellationToken);
        }

        public async Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                await _playerRepository.DeletePlayerAsync(playerId, cancellationToken);
        }

        public async Task<Player[]> GetAllPlayersAsync(CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                return (await _playerRepository.GetAllPlayersAsync(cancellationToken)).ToArray();
            return Array.Empty<Player>();
        }

        public async Task<Player> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                return await _playerRepository.GetPlayerAsync(playerId, cancellationToken);
            return null;
        }

        public async Task MovePlayersAsync(CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                await _playerRepository.MovePlayersAsync(cancellationToken);
        }

        public async Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null)
                await _playerRepository.MovePlayerAsync(playerId, cancellationToken);
        }

        public async Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            if (_playerRepository != null && player != null)
                await _playerRepository.UpdatePlayerAsync(player, cancellationToken);
        }

        protected override IEnumerable<ServiceReplicaListener> CreateServiceReplicaListeners()
        {
            return new[]
            {
                new ServiceReplicaListener(context =>
                    new FabricTransportServiceRemotingListener(context, this))
            };
        }

        #pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
        protected override async Task RunAsync(CancellationToken cancellationToken)
        #pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        {
            if (_playerRepository == null)
                _playerRepository = new SFPlayerRepository(StateManager);
        }

        public Task<List<Int64>> GetPartitionsLowKey(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(Common.Library.GetPartitionsLowKey(_partitionCounter));
        }
    }
}
