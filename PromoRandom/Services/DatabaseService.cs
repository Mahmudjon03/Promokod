using MySql.Data.MySqlClient;
using PromoRandom.Models;
using System.Data;


namespace PromoRandom.Services
{
    public class DatabaseService()
    {
        private readonly string _connectionString = "Server=localhost;Database=imkon_db;Uid=root;Pwd=;";

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


        public async Task AddPrizeAsync(Prize prize)

        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"INSERT INTO prizes (name) VALUES (@name);";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@name", prize.Name);

            await cmd.ExecuteNonQueryAsync();
        }

        public async Task DeletePrizeAsync(int prizeId)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"DELETE FROM prizes WHERE id = @id;";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", prizeId);

            await cmd.ExecuteNonQueryAsync();
        }


        public async Task UpdatePrizeAsync(AddPrizUserModel prize)
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();

                var query = @"UPDATE prizes
                        SET  promokod_id = (select id from promo_codes where code = @promocode) 
                        WHERE id = @id;
                        update promo_codes 
                        set state = 0 
                        where code = @promocode;";

                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@id", prize.PrizId);
                cmd.Parameters.AddWithValue("@promocode", prize.Promocod);
                await cmd.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                string error = ex.Message;
            }
        }

        public async Task<List<Prize>> GetPrizes()
        {
            var list = new List<Prize>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM prizes";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            while (await reader.ReadAsync())
            {
                var prize = new Prize
                {
                    Id = reader.GetInt16(0),
                    Name = reader.GetString(1),
                };
                list.Add(prize);
            }

            return list;
        }
        public async Task<Prize> GetPrize()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"SELECT * FROM `prizes` WHERE promokod_id = 0 LIMIT 1;";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();
            var priz = new Prize()
            {
                Id = reader.GetInt16(0),
                Name = reader.GetString(1)
            };
            return priz;
        }

        public async Task<User> GetUserByPromokodAsync(string promoCode)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"
                SELECT id, language, name, phone, chat_id 
                FROM users 
                WHERE id = (SELECT user_id FROM promo_codes WHERE code = @promokod);";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@promokod", promoCode);

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