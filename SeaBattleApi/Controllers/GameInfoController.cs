using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SeaBattleApi.Auth;
using SeaBattleLogic;
using SeaBattleRepository.DTO;
using SeaBattleRepository.Implement;
using System.Security.Claims;

namespace SeaBattleApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GameInfoController : ControllerBase
    {
        GameLogic gameLogic;

        public GameInfoController(GameLogic gameLogic)
        {
            this.gameLogic = gameLogic;
        }

        [Authorize]
        [HttpPost("CreateGame")]
        public async Task<ActionResult<GameDTO>> CreateGame()
        {
            int idUser = HttpContextInfo.GetUserID(this.HttpContext);
            GameDTO game = await gameLogic.CreateGameAsync(idUser);
            return game;
        }

        [Authorize]
        [HttpPost("ListGame")]
        public async Task<ActionResult<List<GameDTO>>> ListGame()
        {
            int opponentId = HttpContextInfo.GetUserID(this.HttpContext);
            List<GameDTO> games = gameLogic.ListFreeGame(opponentId);
            return games;
        }

        [Authorize]
        [HttpPost("JoinGame")]
        public async Task<ActionResult<bool>> JoinGame(int idGame)
        {
            int opponentId = HttpContextInfo.GetUserID(this.HttpContext);
            return await gameLogic.JoinGameAsync(opponentId, idGame);
        }        
    }
}
