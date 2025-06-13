using MySql.Data.MySqlClient;
using PromoRandom.Models;
using PromoRandom.ViewModels;
using System.Data;

namespace PromoRandom.Services;

public class DatabaseService
{
    private readonly string _connectionString;

    public DatabaseService(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    }

    // Проверка регистрации пользователя
    public async Task<bool> IsUserRegisteredAsync(long chatId)
    {
        const string sql = "SELECT COUNT(*) FROM users WHERE chat_id = @chatId";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return count > 0;
    }

    public async Task<bool> IsUserAdminAsync(long chatId)
    {
        const string sql = "SELECT COUNT(*) FROM users WHERE chat_id = @chatId AND role = 'admin'";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        int count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
        return count > 0;
    }

    // Добавление нового пользователя
    public async Task RegisterUserAsync(string name, string phone, long chatId)
    {
        const string sql = @"
            INSERT INTO users (name, phone, role, chat_id)
            VALUES (@name, @phone, 'client', @chatId)";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@phone", phone);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<User?> GetUserByChatIdAsync(long chatId)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        var cmd = new MySqlCommand("SELECT * FROM users WHERE chat_id = @ChatId", conn);
        cmd.Parameters.AddWithValue("@ChatId", chatId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                ChatId = reader.GetInt64("chat_id"),
                Language = reader.GetString("language"),
                Name = reader.GetString("name"),
                Phone = reader.GetString("phone"),
            };
        }
        return null;
    }

    public async Task<User?> GetUserByIdAsync(int userId)
    {
        const string sql = @"
                SELECT id, name, phone, language, chat_id 
                FROM users 
                WHERE id = @userId";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userId", userId);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new User
            {
                Name = reader["name"] as string ?? string.Empty,
                Phone = reader["phone"] as string,
                Language = reader["language"] as string ?? "tj",
                ChatId = reader.GetInt64("chat_id")
            };
        }

        return null;
    }

    // Обновление данных пользователя
    public async Task UpdateUserAsync(string name, string phone, long chatId)
    {
        const string sql = @"
            UPDATE users
            SET name = @name, phone = @phone
            WHERE chat_id = @chatId";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@phone", phone);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        await cmd.ExecuteNonQueryAsync();
    }

    // Получение языка пользователя
    public async Task<string> GetUserLanguageAsync(long chatId)
    {
        const string sql = "SELECT language FROM users WHERE chat_id = @chatId";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        return (string?)await cmd.ExecuteScalarAsync() ?? "tj";
    }

    // Установка языка пользователя
    public async Task SetUserLanguageAsync(long chatId, string lang)
    {
        const string sql = @"
            INSERT INTO users (chat_id, language)
            VALUES (@chatId, @lang)
            ON DUPLICATE KEY UPDATE language = @lang";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        cmd.Parameters.AddWithValue("@lang", lang);
        await cmd.ExecuteNonQueryAsync();
    }

    // Создание заказа и возврат его ID
    public async Task<int> CreateOrderAsync(double amount)
    {
        const string sql = @"
            INSERT INTO orders (amount)
            VALUES (@amount);
            SELECT LAST_INSERT_ID();";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@amount", amount);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    // Добавление промокода
    public async Task AddPromoCodeAsync(long userChatId, int orderId, string code, int giveId)
    {
        const string sql = @"
            INSERT INTO promo_codes (code, user_chat_id, order_id, give_id)
            VALUES (@code, @userChatId, @orderId, @giveId)";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", code);
        cmd.Parameters.AddWithValue("@userChatId", userChatId);
        cmd.Parameters.AddWithValue("@orderId", orderId);
        cmd.Parameters.AddWithValue("@giveId", giveId);

        await cmd.ExecuteNonQueryAsync();
    }


    public async Task<PromoCode?> GetPromoCodeByCodeAsync(string code)
    {
        const string sql = @"
                SELECT * FROM promo_codes WHERE code = @code";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@code", code);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                UserChatId = reader.GetInt32("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            };
        }

        return null;
    }

    // Получение списка промокодов пользователя
    public async Task<List<PromoCode>> GetAllPromoCodesForUserAsync(long chatId)
    {
        var list = new List<PromoCode>();
        const string sql = @"
                SELECT p.id, p.code, p.user_chat_id, p.order_id, p.give_id, p.status, p.submitted_at
                FROM promo_codes p
                WHERE p.user_chat_id = @chatId";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                UserChatId = reader.GetInt32("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            });
        }

        return list;
    }

    public async Task<List<PromoCode>> GetPromoCodesByGiveIdAsync(int giveId, string status = "active")
    {
        var promoCodes = new List<PromoCode>();
        const string sql = @"
        SELECT id, code, order_id, give_id, status, submitted_at, user_chat_id 
        FROM promo_codes 
        WHERE give_id = @giveId AND status = @status";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", giveId);
        cmd.Parameters.AddWithValue("@status", status);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            promoCodes.Add(new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at"),
                UserChatId = reader.GetInt64("user_chat_id")
            });
        }

        return promoCodes;
    }

    public async Task<List<UserPromoCodeDto>> GetPromoCodeDisplayListAsync(long chatId)
    {
        const string sql = @"
                SELECT 
                    pc.code AS Code,
                    pc.submitted_at AS IssuedAt,
                    o.amount AS OrderPrice
                FROM promo_codes pc
                INNER JOIN users u ON pc.user_chat_id = u.chat_id
                INNER JOIN orders o ON pc.order_id = o.id
                WHERE u.chat_id = @chatId
                ORDER BY pc.submitted_at DESC";


        var result = new List<UserPromoCodeDto>();

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            result.Add(new UserPromoCodeDto
            {
                Code = reader.GetString("Code"),
                IssuedAt = reader.GetDateTime("IssuedAt"),
                OrderPrice = reader.GetDouble("OrderPrice")
            });
        }

        return result;
    }


    // Получение настроек промо
    public async Task<int> GetMinimumAmountAsync()
    {
        const string sql = "SELECT minimum_amount FROM promo_settings LIMIT 1";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task UpdateMinimumAmountAsync(int newAmount)
    {
        const string sql = "UPDATE promo_settings SET minimum_amount = @amount LIMIT 1";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@amount", newAmount);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<Give?> GetFirstIncompleteGiveAsync()
    {
        const string sql = @"
                SELECT * FROM gives 
                WHERE is_completed = 0 
                ORDER BY id ASC 
                LIMIT 1";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Give
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date"),
                IsCompleted = reader.GetBoolean("is_completed")
            };
        }

        return null;
    }

    public async Task<int?> GetFirstIncompleteGiveIdAsync()
    {
        const string sql = @"
                SELECT id FROM gives 
                WHERE is_completed = 0 
                ORDER BY id ASC 
                LIMIT 1";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return reader.GetInt32("id");
        }

        return null;
    }

    // Добавить приз
    public async Task AddPrizeAsync(Prize prize)
    {
        const string sql = @"INSERT INTO prizes (give_id, name) VALUES (@giveId, @name)";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", prize.GiveId);
        cmd.Parameters.AddWithValue("@name", prize.Name);
        await cmd.ExecuteNonQueryAsync();
    }

    // Получить все призы
    public async Task<List<Prize>> GetAllPrizesAsync()
    {
        const string sql = @"SELECT id, give_id, name FROM prizes";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        var prizes = new List<Prize>();
        while (await reader.ReadAsync())
        {
            prizes.Add(new Prize
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                GiveId = reader.GetInt32(reader.GetOrdinal("give_id")),
                Name = reader.GetString(reader.GetOrdinal("name"))
            });
        }
        return prizes;
    }

    // Обновить название приза
    public async Task UpdatePrizeAsync(Prize prize)
    {
        const string sql = @"UPDATE prizes SET name = @name, give_id = @giveId WHERE id = @id";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", prize.Id);
        cmd.Parameters.AddWithValue("@giveId", prize.GiveId);
        cmd.Parameters.AddWithValue("@name", prize.Name);
        await cmd.ExecuteNonQueryAsync();
    }

    // Удалить приз
    public async Task DeletePrizeAsync(int id)
    {
        const string sql = @"DELETE FROM prizes WHERE id = @id";
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }


    //    public async Task<List<string>> GetPromoCodesByUserAsync(string date)
    //    {
    //            var codes = new List<string>();
    //            using var connection = new MySqlConnection(_connectionString);
    //            await connection.OpenAsync();
    //            var query = @"
    //                SELECT p.code
    //                FROM promo_codes p
    //                JOIN users u ON p.user_id = u.id 
    //                  where p.issued_at > @date limit 0,25;";
    //            using var cmd = new MySqlCommand(query, connection);
    //            cmd.Parameters.AddWithValue("@date", date);
    //            using var reader = await cmd.ExecuteReaderAsync();
    //            while (await reader.ReadAsync())
    //            {
    //                codes.Add(reader.GetString("code"));
    //            }

    //            return codes;

    //    }

    //    public async Task<string> GetUserByPromokod(string promokod)
    //    {
    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();
    //        var query = @"select name from users 
    //                            where id = (select user_id from promo_codes 
    //                                where code = @promokod);";
    //        using var cmd = new MySqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@promokod", promokod);
    //        using var reader = await cmd.ExecuteReaderAsync();
    //        await reader.ReadAsync();
    //        return reader.GetString(0);
    //    }

    //    public async Task AddPrizeAsync(Prize prize)

    //    {
    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();

    //        var query = @"INSERT INTO prizes (name) VALUES (@name);";

    //        using var cmd = new MySqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@name", prize.Name);

    //        await cmd.ExecuteNonQueryAsync();
    //    }

    //    public async Task DeletePrizeAsync(int prizeId)
    //    {
    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();

    //        var query = @"DELETE FROM prizes WHERE id = @id;";

    //        using var cmd = new MySqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@id", prizeId);

    //        await cmd.ExecuteNonQueryAsync();
    //    }


    //    //public async Task UpdatePrizeAsync(AddPrizeUserModel prize)
    //    //{
    //    //    using var connection = new MySqlConnection(_connectionString);
    //    //    await connection.OpenAsync();

    //    //    var query = @"UPDATE prizes
    //    //                    SET  promo_code_id = (select id from promo_codes where code = @promocode) 
    //    //                    WHERE id = @id;
    //    //                    update promo_codes 
    //    //                    set state = 1 
    //    //                    where code = @promocode;";

    //    //    using var cmd = new MySqlCommand(query, connection);
    //    //    cmd.Parameters.AddWithValue("@id", prize.PrizeId);
    //    //    cmd.Parameters.AddWithValue("@promocode", prize.Promocode);
    //    //    await cmd.ExecuteNonQueryAsync();
    //    //}

    //    public async Task<List<Prize>> GetPrizes()
    //    {
    //        var list = new List<Prize>();

    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();

    //        var query = "SELECT * FROM `prizes` WHERE promo_code_id = 0";
    //        using var cmd = new MySqlCommand(query, connection);
    //        using var reader = await cmd.ExecuteReaderAsync();
    //        while (await reader.ReadAsync())
    //        {
    //            var prize = new Prize
    //            {
    //                Id = reader.GetInt16(0),
    //                Name = reader.GetString(1),
    //            };
    //            list.Add(prize);
    //        }

    //        return list;
    //    }
    //    public async Task<Prize> GetPrizeById(int id)
    //    {
    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();

    //        var query = "SELECT * FROM `prizes` WHERE id = @id";
    //        using var cmd = new MySqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@id", id);
    //        using var reader = await cmd.ExecuteReaderAsync();
    //        await reader.ReadAsync();

    //        return new Prize
    //        {
    //            Id = reader.GetInt16(0),
    //            Name = reader.GetString(1),
    //        };
    //    }

    //    public async Task<User> GetUserByPromokodAsync(string promoCode)
    //    {
    //        using var connection = new MySqlConnection(_connectionString);
    //        await connection.OpenAsync();

    //        var query = @"
    //                SELECT id, language, name, phone, chat_id 
    //                FROM users 
    //                WHERE id = (SELECT user_id FROM promo_codes WHERE code = @promokod);";

    //        using var cmd = new MySqlCommand(query, connection);
    //        cmd.Parameters.AddWithValue("@promokod", promoCode);

    //        using var reader = await cmd.ExecuteReaderAsync();
    //        if (await reader.ReadAsync())
    //        {
    //            return new User
    //            {
    //                Id = reader.GetInt32("id"),
    //                Language = reader["language"] as string,
    //                Name = reader["name"] as string,
    //                Phone = reader["phone"] as string,
    //                ChatId = reader.GetInt64("chat_id")
    //            };
    //        }
    //        return null; // Если промокод не найден
    //    }

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