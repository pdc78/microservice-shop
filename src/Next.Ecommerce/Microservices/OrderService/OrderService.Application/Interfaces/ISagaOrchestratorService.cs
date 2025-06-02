using System.Threading.Tasks;
using OrderService.Domain.DTOs;

namespace OrderService.Application.Interfaces;

// SAGA orchestrator interface for managing order processing
public interface ISagaOrchestratorService
{
    Task<string> StartSagaAsync(BasketDto basket);
}
