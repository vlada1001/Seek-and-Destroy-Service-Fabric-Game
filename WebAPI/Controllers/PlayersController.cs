using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Actors;
using Microsoft.ServiceFabric.Actors.Client;
using Microsoft.ServiceFabric.Services.Client;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using Microsoft.ServiceFabric.Services.Remoting.V2.FabricTransport.Client;
using PlayerCollection.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using UserActor.Interfaces;
using WebAPI.Models;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        public static IPlayerCollectionService _service;

        public PlayersController()
        {
            var proxyFactory = new ServiceProxyFactory(
                c => new FabricTransportServiceRemotingClientFactory());

            _service = proxyFactory.CreateServiceProxy<IPlayerCollectionService>(
                new Uri("fabric:/OnboardingApplication/PlayerCollection"),
                new ServicePartitionKey(0L));
        }

        [HttpGet]
        public async Task<IEnumerable<PlayerAPI>> GetPlayersAsync()
        {
            IEnumerable<Player> allPlayers = await _service.GetAllPlayersAsync();

            return allPlayers.Select(p => new PlayerAPI()
            {
                Id = p.Id.ToString(),
                Username = p.Username,
                HP = p.HP,
                AD = p.AD,
                State = Common.Library.StatusToString(p.State),
                NumberOfFights = p.NumberOfFights,
                Coordinates = new CoordAPI() { X = p.Coordinates.X, Y = p.Coordinates.Y }
            });
        }

        [HttpPut("createPlayer")]
        public async Task CreatePlayer()
        {
            Player player = new Player().Init();
            IUserActor actor = GetActor(player.Id);

            await actor.AddPlayerAsync(player);
        }

        [HttpPut("createPlayers")]
        public async Task CreatePlayers([FromQuery] int count)
        {
            if (count > 0)
            {
                for (int i = 0; i < count; i++)
                {
                    CreatePlayer();
                }
            }
        }

        [HttpDelete("deletePlayer/{playerId}")]
        public async Task DeletePlayerAsync([FromRoute] Guid playerId)
        {
            IUserActor actor = GetActor(playerId);

            IActorService userActorServiceProxy = ActorServiceProxy.Create(
                new Uri("fabric:/OnboardingApplication/UserActorService"),
                actor.GetActorId());

            //await actor.DeletePlayerAsync();
            await userActorServiceProxy.DeleteActorAsync(actor.GetActorId(), CancellationToken.None);
        }

        [HttpGet("movePlayer/{playerId}")]
        public async Task MovePlayerAsync([FromRoute] Guid playerId)
        {
            IUserActor actor = GetActor(playerId);

            await actor.MovePlayerAsync();
        }

        [HttpGet("movePlayers")]
        public async Task MovePlayersAsync()
        {
            var allPlayers = await GetPlayersAsync();

            Parallel.ForEach(allPlayers, async p =>
            {
                IUserActor actor = GetActor(Guid.Parse(p.Id));
                await actor.MovePlayerAsync();
            });
        }

        [HttpGet("{playerId}")]
        public async Task<PlayerAPI> GetPlayerAsync(Guid playerId)
        {
            IUserActor actor = GetActor(playerId);
            var p = await actor.GetPlayerAsync();

            if (p == null) return null;

            var playersApi = new PlayerAPI()
            {
                Id = p.Id.ToString(),
                Username = p.Username,
                HP = p.HP,
                AD = p.AD,
                State = Common.Library.StatusToString(p.State),
                NumberOfFights = p.NumberOfFights,
                Coordinates = new CoordAPI() { X = p.Coordinates.X, Y = p.Coordinates.Y }
            };

            return playersApi;
        }

        [HttpGet("updatePlayer")]
        public async Task UpdatePlayerAsync([FromQuery] string playerId, [FromQuery] int hp, [FromQuery] int ad,
            [FromQuery] int numberOfFights)
        {
            var player = new Player()
            {
                Id = Guid.Parse(playerId),
                HP = hp,
                AD = ad,
                NumberOfFights = numberOfFights,
            };

            IUserActor actor = GetActor(player.Id);
            await actor.UpdatePlayerAsync(player);
        }

        public static void CreateProxy()
        {
            var proxyFactory = new ServiceProxyFactory(
                c => new FabricTransportServiceRemotingClientFactory());

            var key = Common.Library.RandomLong();

            _service = proxyFactory.CreateServiceProxy<IPlayerCollectionService>(
                new Uri("fabric:/OnboardingApplication/PlayerCollection"),
                new ServicePartitionKey(key));
        }

        public static IPlayerCollectionService GetServiceProxy(long partitionKey)
        {
            var proxyFactory = new ServiceProxyFactory(
                c => new FabricTransportServiceRemotingClientFactory());

            return proxyFactory.CreateServiceProxy<IPlayerCollectionService>(
                new Uri("fabric:/OnboardingApplication/PlayerCollection"),
                new ServicePartitionKey(partitionKey));
        }

        public static IUserActor GetActor(Guid userId)
        {
            return ActorProxy.Create<IUserActor>(
                new ActorId(userId),
                new Uri("fabric:/OnboardingApplication/UserActorService"));
        }
    }
}
