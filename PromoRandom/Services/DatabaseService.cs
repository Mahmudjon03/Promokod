using MySql.Data.MySqlClient;
using PromoRandom.Models;
using PromoRandom.ViewModels;
using System.Data;


namespace PromoRandom.Services
{
    public class DatabaseService
    {
        private readonly string _connectionString;

        // Fix for CS8862 and CS8618: Added 'this' initializer and ensured '_connectionString' is initialized.
        public DatabaseService(IConfiguration configuration)
        {
            ArgumentNullException.ThrowIfNull(configuration);

            _connectionString = configuration.GetConnectionString("DefaultConnection")
                                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
        }

        public async Task<List<string>> GetPromoCodesByUserAsync(string date)
        {
                var codes = new List<string>();
                using var connection = new MySqlConnection(_connectionString);
                await connection.OpenAsync();
                var query = @"
                SELECT p.code
                FROM promo_codes p
                JOIN users u ON p.user_id = u.id 
                  where p.issued_at > @date limit 0,25;";
                using var cmd = new MySqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@date", date);
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


        public async Task UpdatePrizeAsync(AddPrizeUserModel prize)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = @"UPDATE prizes
                        SET  promo_code_id = (select id from promo_codes where code = @promocode) 
                        WHERE id = @id;
                        update promo_codes 
                        set state = 1 
                        where code = @promocode;";

            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", prize.PrizeId);
            cmd.Parameters.AddWithValue("@promocode", prize.Promocode);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<List<Prize>> GetPrizes()
        {
            var list = new List<Prize>();

            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM `prizes` WHERE promo_code_id = 0";
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
        public async Task<Prize> GetPrizeById(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            var query = "SELECT * FROM `prizes` WHERE id = @id";
            using var cmd = new MySqlCommand(query, connection);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = await cmd.ExecuteReaderAsync();
            await reader.ReadAsync();

            return new Prize
            {
                Id = reader.GetInt16(0),
                Name = reader.GetString(1),
            };
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

        public async Task<List<WinnerStatModel>> GetWinnerStatisticsAsync()
        {
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();

            // Corrected SQL query with proper table relationships
            var query = @"
                SELECT 
                    g.id AS GiveawayId,
                    g.name AS GiveawayName,
                    g.start_date,
                    g.end_date,
                    p.id AS PrizeId,
                    p.name AS PrizeName,
                    u.name AS WinnerName
                FROM gives g
                LEFT JOIN prizes p ON p.give_id = g.id
                LEFT JOIN winners w ON w.prize_id = p.id
                LEFT JOIN promo_codes pc ON pc.id = w.promo_code_id
                LEFT JOIN users u ON u.chat_id = pc.user_chat_id
                ORDER BY g.start_date DESC, g.id, p.id;
            ";

            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();

            var giveawayDict = new Dictionary<int, WinnerStatModel>();

            while (await reader.ReadAsync())
            {
                var giveawayId = reader.GetInt32("GiveawayId");

                if (!giveawayDict.TryGetValue(giveawayId, out var giveaway))
                {
                    giveaway = new WinnerStatModel
                    {
                        GiveawayName = reader.GetString("GiveawayName"),
                        StartDate = reader.GetDateTime("start_date"),
                        EndDate = reader.GetDateTime("end_date"),
                        Winners = []
                    };
                    giveawayDict[giveawayId] = giveaway;
                }

                // Only add prize information if PrizeName exists
                if (!reader.IsDBNull(reader.GetOrdinal("PrizeName")))
                {
                    var prizeName = reader.GetString("PrizeName");
                    var winnerName = reader.IsDBNull(reader.GetOrdinal("WinnerName"))
                        ? "-"
                        : reader.GetString("WinnerName");

                    giveaway.Winners.Add(new PrizeWinner
                    {
                        PrizeName = prizeName,
                        WinnerName = winnerName
                    });
                }
            }

            return [.. giveawayDict.Values];
        }
    }
}