using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    public interface IMinesRepository
    {
        Task AddMinesGame(MinesGame minesGame);

        Task<List<MinesGame>> GetMinesGamesByUserId(long userId);
    }
}
