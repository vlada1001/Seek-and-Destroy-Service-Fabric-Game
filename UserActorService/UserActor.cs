using GameConsole;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Runtime;
using PlayerCollection.Model;
using System.Threading;
using System.Threading.Tasks;
using UserActor.Interfaces;
using WebAPI.Controllers;
using static Common.Library;

namespace UserActor
{
    [StatePersistence(StatePersistence.Persisted)]
    internal class UserActor : Actor, IUserActor
    {
        public UserActor(ActorService actorService, ActorId actorId) : base(actorService, actorId)
        {
        }

        public async Task AddPlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            await StateManager.TryAddStateAsync("player", player, cancellationToken);

            var proxy = PlayersController.GetServiceProxy(GetPartitionKey(player.Id));

            await proxy.AddPlayerAsync(player, cancellationToken);
        }

        public async Task DeletePlayerAsync(CancellationToken cancellationToken = default)
        {
            await StateManager.TryRemoveStateAsync("player", cancellationToken);

            var proxy = PlayersController.GetServiceProxy(GetPartitionKey(Id.GetGuidId()));

            await proxy.DeletePlayerAsync(Id.GetGuidId(), cancellationToken);
        }

        public async Task<Player> GetPlayerAsync(CancellationToken cancellationToken = default)
        {
            var proxy = PlayersController.GetServiceProxy(GetPartitionKey(Id.GetGuidId()));
            return await proxy.GetPlayerAsync(Id.GetGuidId(), cancellationToken);
        }

        public async Task MovePlayerAsync(CancellationToken cancellationToken = default)
        {
            var player = await GetPlayerAsync(cancellationToken);

            if (player != null && player.State.Equals(Status.Exploring))
            {
                player = await StateManager.AddOrUpdateStateAsync<Player>("player", null, (key, value) => value.Move(), cancellationToken);

                var proxy = PlayersController.GetServiceProxy(GetPartitionKey(Id.GetGuidId()));

                await proxy.UpdatePlayerAsync(player, cancellationToken);
                GameConsoleEventSource.Current.Message($"[USER.MOVE]: {player.Username} moved to {player.Coordinates}.");
                //ActorEventSource.Current.ActorMessage(this, $"[USER.MOVE]: {player.Username} moved to {player.Coordinates}.");
            }
        }

        public async Task UpdatePlayerAsync(Player player, CancellationToken cancellationToken = default)
        {
            var p = await GetPlayerAsync(cancellationToken);

            if (player != null && p != null && p.Id == player.Id)
            {
                player = await StateManager.AddOrUpdateStateAsync<Player>(
                    "player",
                    null,
                    (key, value) => value.UpdatePlayer(player.HP, player.AD, player.NumberOfFights),
                    cancellationToken);

                var proxy = PlayersController.GetServiceProxy(GetPartitionKey(Id.GetGuidId()));

                await proxy.UpdatePlayerAsync(player, cancellationToken);
                //ActorEventSource.Current.ActorMessage(this, $"[USER.MOVE]: {player.Username} moved to {player.Coordinates}.");
            }
        }

        protected override Task OnActivateAsync()
        {
            GameConsoleEventSource.Current.Message($"[USER.CREATE]: {Id} logged in.");
            //ActorEventSource.Current.ActorMessage(this, $"[USER.CREATE]: {Id} logged in.");
            return Task.CompletedTask;
        }

        protected override async Task OnDeactivateAsync()
        {
            await StateManager.TryRemoveStateAsync("player");

            var proxy = PlayersController.GetServiceProxy(GetPartitionKey(Id.GetGuidId()));

            await proxy.DeletePlayerAsync(Id.GetGuidId(), CancellationToken.None);
            GameConsoleEventSource.Current.Message($"[USER.DELETE]: {Id} logged out.");
            //ActorEventSource.Current.ActorMessage(this, $"[USER.DELETE]: {Id} logged out.");
            //return base.OnDeactivateAsync();
        }
    }
}
