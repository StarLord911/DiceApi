using DiceApi.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services.Contracts
{
    public interface IAntiMinusService
    {
        bool CheckMinesAntiMinus(ActiveMinesGame minesGame, Settings settings);
    }
}
