using MySql.Data.MySqlClient;
using System.Data;


namespace AksiyaBot
{
    public class DatabaseService()
    {
        private readonly string _connectionString = "Server=localhost;Database=promokod_db;Uid=root;Pwd=;";

        public async Task<List<string>> GetPromoCodesByUserAsync()
        {
            var codes = new List<string>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
                SELECT p.code
                FROM promo_codes p
                JOIN users u ON p.user_id = u.id LIMIT 0, 25;
               ";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                codes.Add(reader.GetString("code"));
            }

            return codes;
        }

        public async Task<string> GetUserViePromokod(string promokod)
        {
            var codes = new List<string>();
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

    }
}