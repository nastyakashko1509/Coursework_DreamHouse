using DreameHouse.Domain.Abstarctions;
using DreameHouse.Domain.Entities;
using System.Threading.Tasks;

namespace DreameHouse.Aplication.Services
{
    public class TaskService 
    {
        private readonly ICrudOperations<Tasks> _taskRepository;
        private readonly PlayerService _playerService;

        public TaskService(ICrudOperations<Tasks> taskRepository, PlayerService playerService)
        {
            _taskRepository = taskRepository;
            _playerService = playerService;
        }

        public async Task<Tasks> GetCurrentTaskAsync(int taskId)
        {
            return await _taskRepository.ReadByIdAsync(taskId);
        }

        public async Task<bool> CompleteTaskAsync(Player player, Tasks task)
        {
            if (player.bitcoin >= task.PriceBitcoin)
            {
                player.bitcoin -= task.PriceBitcoin;
                player.task = task.Id + 1;
                await _playerService.UpdatePlayerAsync(player);
                return true;
            }
            return false;
        }
    }
}
