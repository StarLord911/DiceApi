using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.Services
{
    public class FakeActiveService : IFakeActiveService
    {
        public FakeActiveService()
        {

        }

        public Task StartFakeGamesActive()
        {
            throw new NotImplementedException();
        }

        public Task StartFakeOnlineActive()
        {
            throw new NotImplementedException();
        }
    }
}
