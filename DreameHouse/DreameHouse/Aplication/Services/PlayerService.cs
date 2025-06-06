using DreameHouse.Domain.Entities;
using DreameHouse.Infrastructure.Repositories;

namespace DreameHouse.Aplication.Services
{
    public class PlayerService 
    {
        private readonly PlayerRepository _playerRepository;

        public PlayerService(PlayerRepository playerRepository)
        {
            _playerRepository = playerRepository;
        }

        public async Task<Player> CreatePlayerAsync(string email, string password)
        {
            var existingPlayer = await _playerRepository.FindByEmailAndPasswordAsync(email, password);
            if (existingPlayer != null)
            {
                throw new InvalidOperationException("Игрок с таким email уже существует");
            }

            var newPlayer = new Player
            {
                email = email,
                password = password,
                level = 1,
                bitcoin = 0,
                task = 1
            };

            await _playerRepository.CreateAsync(newPlayer);
            return newPlayer;
        }

        public async Task<Player?> GetPlayerByEmailAndPasswordAsync(string email, string password)
        {
            return await _playerRepository.FindByEmailAndPasswordAsync(email, password);
        }

        public async Task<Player?> GetPlayerByIdAsync(int id)
        {
            return await _playerRepository.ReadByIdAsync(id);
        }

        public async Task<bool> UpdatePlayerLevelAsync(int playerId, int newLevel)
        {
            var player = await _playerRepository.ReadByIdAsync(playerId);
            if (player == null) return false;

            player.level = newLevel;
            await _playerRepository.UpdateAsync(player);
            return true;
        }

        public async Task<bool> UpdatePlayerBitcoinAsync(int playerId, int colBitcoin)
        {
            var player = await _playerRepository.ReadByIdAsync(playerId);
            if (player == null) return false;

            player.bitcoin += colBitcoin;
            await _playerRepository.UpdateAsync(player);
            return true;
        }

        public async Task UpdatePlayerAsync(Player player)
        {
            await _playerRepository.UpdateAsync(player);
        }
    }
}
