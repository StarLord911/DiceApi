using AutoMapper;
using DiceApi.Common;
using DiceApi.Data;
using DiceApi.Data.ApiModels;
using DiceApi.Data.ApiReqRes;
using DiceApi.Data.Requests;
using DiceApi.DataAcces.Repositoryes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class MinesService : IMinesService
    {
        private readonly ICacheService _cacheService;
        private readonly IUserService _userService;
        private readonly IMinesRepository _minesRepository;
        private readonly Dictionary<int, List<double>> chanses;
        private readonly IMapper _mapper;

        public MinesService(ICacheService cacheService,
            IUserService userService,
            IMinesRepository minesRepository,
            IMapper mapper)
        {
            _cacheService = cacheService;
            _userService = userService;
            _minesRepository = minesRepository;

            _mapper = mapper;

            chanses = GetChanses();
        }

        public async Task<CreateMinesGameResponce> CreateMinesGame(CreateMinesGameRequest request)
        {
            var user = _userService.GetById(request.UserId);

            if (request.MinesCount > 24 || request.MinesCount < 2)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = $"Change mines count. Mines count = {request.MinesCount}" };
            }

            if (user.Ballance <= request.Sum)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = "Lack of balance" };
            }

            if (!user.IsActive)
            {
                return new CreateMinesGameResponce() { Succes = false, Info = "User blocked" };
            }

            var game = new ActiveMinesGame(request.MinesCount);
            game.BetSum = request.Sum;
            game.UserId = request.UserId;
            game.Chances = chanses[game.MinesCount];

            var serializedGame = SerializationHelper.Serialize(game);

            await _cacheService.WriteCache(CacheConstraints.MINES_KEY + request.UserId, serializedGame);

            await _userService.UpdateUserBallance(request.UserId, user.Ballance - request.Sum);

            return new CreateMinesGameResponce() { Succes = true, Info = "Game created" };
        }

        public async Task<MinesGameApiModel> GetActiveMinesGameByUserId(GetByUserIdRequest request)
        {
            var serializedGame = await _cacheService.ReadCache(CacheConstraints.MINES_KEY + request.Id);
            var game = SerializationHelper.Deserialize<ActiveMinesGame>(serializedGame);
            return new MinesGameApiModel() { Cells = MapCells(game.GetCells()), CanWin = game.CanWin, OpenedCount = game.OpenedCellsCount };

        }

        public async Task<(FinishMinesGameResponce, ActiveMinesGame)> FinishGame(GetByUserIdRequest request)
        {
            var serializedGame = await _cacheService.ReadCache(CacheConstraints.MINES_KEY + request.Id);

            if (serializedGame == null)
            {
                return (new FinishMinesGameResponce { Succes = false, Message = "Game not found" }, null);
            }

            await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + request.Id);

            var game = SerializationHelper.Deserialize<ActiveMinesGame>(serializedGame);

            if (!game.IsActiveGame())
            {
                return (new FinishMinesGameResponce { Succes = false, Message = "Game already finished" }, null);
            }

            await _userService.UpdateUserBallance(request.Id, game.CanWin);
            game.FinishGame = true;

            var mappedGame = _mapper.Map<MinesGame>(game);
            await _minesRepository.AddMinesGame(mappedGame);
            var user = _userService.GetById(request.Id);

            return (new FinishMinesGameResponce { UserBallance = user.Ballance }, game);
        }

        public async Task<(OpenCellResponce, ActiveMinesGame)> OpenCell(OpenCellRequest request)
        {
            var serializedGame = await _cacheService.ReadCache(CacheConstraints.MINES_KEY + request.UserId);
            var game = SerializationHelper.Deserialize<ActiveMinesGame>(serializedGame);

            if (!game.IsActiveGame())
            {
                game.FinishGame = false;
                return (new OpenCellResponce { Succes = false, Message = "Game over" }, game);
            }

            var openResult = game.OpenCell(request.X, request.Y);

            if (!game.IsActiveGame())
            {
                game.FinishGame = false;
                var mappedGame = _mapper.Map<MinesGame>(game);
                await _minesRepository.AddMinesGame(mappedGame);
            }

            await _cacheService.DeleteCache(CacheConstraints.MINES_KEY + request.UserId);

            var newGame = SerializationHelper.Serialize<ActiveMinesGame>(game);

            await _cacheService.WriteCache(CacheConstraints.MINES_KEY + request.UserId, newGame);
            return (new OpenCellResponce { Succes = true, Message = "Game continuate", Result = openResult }, game);
        }

        public async Task<List<MinesGame>> GetMinesGamesByUserId(long userId)
        {
            return await _minesRepository.GetMinesGamesByUserId(userId);
        }


        #region
        public CellApiModel[,] MapCells(Cell[,] cells)
        {
            int rows = cells.GetLength(0);
            int columns = cells.GetLength(1);

            CellApiModel[,] cellApiModels = new CellApiModel[rows, columns];

            for (int row = 0; row < rows; row++)
            {
                for (int column = 0; column < columns; column++)
                {
                    Cell cell = cells[row, column];
                    CellApiModel cellApiModel = _mapper.Map<CellApiModel>(cell);
                    cellApiModels[row, column] = cellApiModel;
                }
            }

            return cellApiModels;
        }

        private Dictionary<int, List<double>> GetChanses()
        {
            var dic = new Dictionary<int, List<double>>();

            dic.Add(2, new List<double>() { 1.09, 1.19, 1.3, 1.43, 1.58, 1.75, 1.96, 2.21, 2.5, 2.86, 3.3, 3.85, 4.55, 5.45, 6.67, 8.33, 10.71, 14.29, 20, 30, 50, 100, 300 });
            dic.Add(3, new List<double>() { 1.14, 1.3, 1.49, 1.73, 2.02, 2.37, 2.82, 3.38, 4.11, 5.05, 6.32, 8.04, 10.45, 13.94, 19.17, 27.38, 41.07, 65.7, 115, 230, 575, 2300 });
            dic.Add(4, new List<double>() { 1.19, 1.43, 1.73, 2.11, 2.61, 3.26, 4.13, 5.32, 6.95, 9.27, 12.64, 17.69, 25.56, 38.33, 60.24, 100.4, 180.71, 361.43, 843.33, 2530, 12650 });
            dic.Add(5, new List<double>() { 1.25, 1.58, 2.02, 2.61, 3.43, 4.57, 6.2, 8.59, 12.16, 17.69, 26.54, 41.28, 67.08, 115, 210.83, 421.67, 948.75, 2530, 8855, 53130 });
            dic.Add(6, new List<double>() { 1.32, 1.75, 2.37, 3.26, 4.57, 6.53, 9.54, 14.31, 22.12, 35.38, 58.97, 103.21, 191.67, 383.33, 843.33, 2108.33, 6325, 25300, 177100 });
            dic.Add(7, new List<double>() { 1.39, 1.96, 2.82, 4.13, 6.2, 9.54, 15.1, 24.72, 42.02, 74.7, 140.06, 280.13, 606.94, 1456.67, 4005.83, 13352.78, 60087.5, 480700 });
            dic.Add(8, new List<double>() { 1.47, 2.21, 3.38, 5.32, 8.59, 14.31, 24.72, 44.49, 84.04, 168.08, 360.16, 840.38, 2185, 6555, 24035, 120175, 1081575 });
            dic.Add(9, new List<double>() { 1.56, 2.5, 4.11, 6.95, 12.16, 22.12, 42.02, 84.04, 178.58, 408.19, 1020.47, 2857.31, 9286.25, 37145, 204297.5, 2042975 });
            dic.Add(10, new List<double>() { 1.67, 2.86, 5.05, 9.27, 17.69, 35.38, 74.7, 168.08, 408.19, 1088.5, 3265.49, 11429.23, 49526.67, 297160, 3268760 });
            dic.Add(11, new List<double>() { 1.79, 3.3, 6.32, 12.64, 26.54, 58.97, 140.06, 360.16, 1020.47, 3265.49, 12245.6, 57146.15, 371450, 4457400 });
            dic.Add(12, new List<double>() { 1.92, 3.85, 8.04, 17.69, 41.28, 103.21, 280.13, 840.38, 2857.31, 11429.23, 57146.14, 400023.08, 5200300 });
            dic.Add(13, new List<double>() { 2.08, 4.55, 10.45, 25.56, 67.08, 191.67, 606.94, 2185, 9286.25, 49526.67, 371450, 5200300 });
            dic.Add(14, new List<double>() { 2.27, 5.45, 13.94, 38.33, 115, 383.33, 1456.67, 6555, 37145, 297160, 4457400 });
            dic.Add(15, new List<double>() { 2.5, 6.67, 19.17, 60.24, 210.83, 843.33, 4005.83, 24035, 204297.5, 3268760 });
            dic.Add(16, new List<double>() { 2.78, 8.33, 27.38, 100.4, 421.67, 2108.33, 13352.78, 120175, 2042975 });
            dic.Add(17, new List<double>() { 3.13, 10.71, 41.07, 180.71, 948.75, 6325, 60087.5, 1081575 });
            dic.Add(18, new List<double>() { 3.57, 14.29, 65.71, 361.43, 2530, 25300, 480700 });
            dic.Add(19, new List<double>() { 4.17, 20, 115, 843.33, 8855, 177100 });
            dic.Add(20, new List<double>() { 5, 30, 230, 2530, 53130 });
            dic.Add(21, new List<double>() { 6.25, 50, 575, 12650 });
            dic.Add(22, new List<double>() { 8.33, 100, 2300 });
            dic.Add(23, new List<double>() { 12.5, 300 });
            dic.Add(24, new List<double>() { 25 });

            return dic;
        }

        

        #endregion
    }
}