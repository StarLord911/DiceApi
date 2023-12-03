using DiceApi.Data.ApiReqRes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DiceApi.DataAcces.Repositoryes
{
    /// <summary>
    /// Репозиторий для работы с заявками о сотрудничестве.
    /// </summary>
    public interface ICooperationRequestRepository
    {
        Task CreateCooperationRequest(CooperationRequest request);

        Task<IEnumerable<CooperationRequest>> GetAllCooperationRequests();
    }
}
