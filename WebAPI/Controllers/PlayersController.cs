using Common.API;
using Microsoft.AspNetCore.Mvc;
using Microsoft.ServiceFabric.Services.Remoting.Client;
using PlayerCollection.Model;
using UserOrchestrator.Interfaces;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PlayersController : ControllerBase
    {
        public PlayersController()
        {
        }

        [HttpGet]
        public async Task<IEnumerable<PlayerAPI>> GetPlayersAsync()
        {
            return await GetUserOrchestratorServiceProxy().GetPlayersAsync();
        }

        [HttpPut("createPlayer")]
        public async Task CreatePlayer()
        {
            await GetUserOrchestratorServiceProxy().CreatePlayerAsync();
        }

        [HttpPut("createPlayers")]
        public async Task CreatePlayers([FromQuery] int count)
        {
            await GetUserOrchestratorServiceProxy().CreatePlayersAsync(count);
        }

        [HttpDelete("deletePlayer/{playerId}")]
        public async Task DeletePlayerAsync([FromRoute] Guid playerId)
        {
            await GetUserOrchestratorServiceProxy().DeletePlayerAsync(playerId);
        }

        [HttpGet("movePlayer/{playerId}")]
        public async Task MovePlayerAsync([FromRoute] Guid playerId)
        {
            await GetUserOrchestratorServiceProxy().MovePlayerAsync(playerId);
        }

        [HttpGet("movePlayers")]
        public async Task MovePlayersAsync()
        {
            await GetUserOrchestratorServiceProxy().MovePlayersAsync();
        }

        [HttpGet("{playerId}")]
        public async Task<PlayerAPI> GetPlayerAsync(Guid playerId)
        {
            return await GetUserOrchestratorServiceProxy().GetPlayerAsync(playerId);
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

            await GetUserOrchestratorServiceProxy().UpdatePlayerAsync(player);
        }

        [HttpGet("activePlayersCount")]
        public async Task<Int64> GetActivePlayersCountAsync()
        {
            return await GetUserOrchestratorServiceProxy().GetActivePlayersCountAsync();
        }

        public static IUserOrchestrator GetUserOrchestratorServiceProxy()
        {
            return ServiceProxy.Create<IUserOrchestrator>(new Uri("fabric:/OnboardingApplication/UserOrchestrator"));
        }
    }
}
