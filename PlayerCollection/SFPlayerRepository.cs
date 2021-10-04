using Microsoft.ServiceFabric.Data;
using Microsoft.ServiceFabric.Data.Collections;
using PlayerCollection.Model;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PlayerCollection
{
    class SFPlayerRepository : IPlayerRepository
    {
        private readonly IReliableStateManager stateManager;

        public SFPlayerRepository(IReliableStateManager stateManager)
        {
            this.stateManager = stateManager;
        }

        public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");

            using ITransaction tx = stateManager.CreateTransaction();

            await players.TryAddAsync(tx, player.Id, player);

            await tx.CommitAsync();
        }

        public async Task<IEnumerable<Player>> GetAllPlayersAsync(CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");
            List<Player> result = new();

            using (ITransaction tx = stateManager.CreateTransaction())
            {
                Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<Guid, Player>> allPlayers = await players.CreateEnumerableAsync(tx);

                using Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<Guid, Player>> enumerator = allPlayers.GetAsyncEnumerator();

                while (await enumerator.MoveNextAsync(CancellationToken.None))
                {
                    result.Add(enumerator.Current.Value);
                }
            }

            return result;
        }

        public async Task DeletePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");
            using ITransaction tx = stateManager.CreateTransaction();

            await players.TryRemoveAsync(tx, playerId);

            await tx.CommitAsync();
        }

        public async Task MovePlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");
            using ITransaction tx = stateManager.CreateTransaction();

            var player = await players.TryGetValueAsync(tx, playerId);

            if (player.HasValue)
                player.Value.Move();

            await tx.CommitAsync();
        }

        public async Task MovePlayersAsync(CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");

            using ITransaction tx = stateManager.CreateTransaction();

            Microsoft.ServiceFabric.Data.IAsyncEnumerable<KeyValuePair<Guid, Player>> allPlayers = await players.CreateEnumerableAsync(tx);

            using Microsoft.ServiceFabric.Data.IAsyncEnumerator<KeyValuePair<Guid, Player>> enumerator = allPlayers.GetAsyncEnumerator();

            while (await enumerator.MoveNextAsync(CancellationToken.None))
            {
                enumerator.Current.Value.Move();
            }

            await tx.CommitAsync();
        }

        public async Task<Player> GetPlayerAsync(Guid playerId, CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");

            using ITransaction tx = stateManager.CreateTransaction();
            ConditionalValue<Player> player = await players.TryGetValueAsync(tx, playerId);

            return player.HasValue ? player.Value : null;
        }

        public async Task UpdatePlayerAsync(Player updatedPlayer, CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");

            using ITransaction tx = stateManager.CreateTransaction();

            var player = await players.TryGetValueAsync(tx, updatedPlayer.Id);
            await players.TryUpdateAsync(tx, updatedPlayer.Id, updatedPlayer, player.Value);

            await tx.CommitAsync();
        }

        public async Task<int> GetActivePlayerCount(CancellationToken cancellationToken = default)
        {
            IReliableDictionary<Guid, Player> players = await stateManager.GetOrAddAsync<IReliableDictionary<Guid, Player>>("players");

            using ITransaction tx = stateManager.CreateTransaction();

            int count = (int)await players.GetCountAsync(tx);

            return count;
        }
    }
}
