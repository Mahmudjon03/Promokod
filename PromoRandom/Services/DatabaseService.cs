using MySql.Data.MySqlClient;
using PromoRandom.Models;
using System.Data;


namespace PromoRandom.Services
{
    public class DatabaseService()
    {
        private readonly string _connectionString = "Server=localhost;Database=imkon_aksiya;Uid=root;Pwd=;";

        public async Task<List<string>> GetPromoCodesByUserAsync()
        {
            var codes = new List<string>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
                SELECT p.code
                FROM promo_codes p
                JOIN users u ON p.user_id = u.id LIMIT 0, 25;";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                codes.Add(reader.GetString("code"));
            }

            return codes;
        }

        public async Task<string> GetUserByPromokod(string promokod)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"select name from users 
                            where id = (select user_id from promo_codes 
                                where code = @promokod);";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@promokod", promokod);
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            return reader.GetString(0);
        }

        public async Task<User?> GetUserByPromokodAsync(string promokod)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, language, name, phone, chat_id 
                FROM users 
                WHERE id = (SELECT user_id FROM promo_codes WHERE code = @promokod);";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@promokod", promokod);

            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new User
                {
                    Id = reader.GetInt32("id"),
                    Language = reader["language"] as string,
                    Name = reader["name"] as string,
                    Phone = reader["phone"] as string,
                    ChatId = reader.GetInt64("chat_id")
                };
            }
            return null; // Если промокод не найден
        }
    }
}