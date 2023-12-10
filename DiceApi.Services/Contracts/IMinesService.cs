using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Requests;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public interface IMinesService
    {
        Task<CreateMinesGameResponce> CreateMinesGame(CreateMinesGameRequest request);

        Task<(OpenCellResponce, ActiveMinesGame)> OpenCell(OpenCellRequest request);

        Task<MinesGameApiModel> GetActiveMinesGameByUserId(GetByUserIdRequest request);

        Task<(FinishMinesGameResponce, ActiveMinesGame)> FinishGame(GetByUserIdRequest request);

        Task<List<MinesGame>> GetMinesGamesByUserId(long userId);
    }
}
