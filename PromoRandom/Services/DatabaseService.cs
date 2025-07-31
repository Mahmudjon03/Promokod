using MySql.Data.MySqlClient;
using PromoRandom.Models;
using PromoRandom.ViewModels;
using System.Data;
using System.Text;

namespace PromoRandom.Services;

public class DatabaseService
{
    static readonly string _connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING")
        ?? throw new InvalidOperationException("Переменная DB_CONNECTION_STRING не установлена");

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

    public async Task<List<long>> GetAllUserChatIdsAsync()
    {
        var chatIds = new List<long>();
        const string sql = "SELECT DISTINCT chat_id FROM users";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            chatIds.Add(reader.GetInt64("chat_id"));
        }

        return chatIds;
    }

    public async Task<List<long>> GetUserChatIdsByLanguageAsync(string language)
    {
        var chatIds = new List<long>();
        const string sql = "SELECT DISTINCT chat_id FROM users WHERE language = @language";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@language", language);

        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            chatIds.Add(reader.GetInt64("chat_id"));
        }

        return chatIds;
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
                Phone = reader.IsDBNull(reader.GetOrdinal("phone"))
                    ? string.Empty
                    : reader.GetString("phone"),
                PersonalPhone = reader.IsDBNull(reader.GetOrdinal("personal_phone"))
                    ? string.Empty
                    : reader.GetString("personal_phone")
            };
        }
        return null;
    }

    // Обновление данных пользователя
    public async Task UpdateUserAsync(string name, string phone, string? personalPhone, long chatId)
    {
        const string sql = @"
            UPDATE users
            SET name = @name,
                phone = @phone,
                personal_phone = @personalPhone
            WHERE chat_id = @chatId";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", name);
        cmd.Parameters.AddWithValue("@phone", phone);
        cmd.Parameters.AddWithValue("@personalPhone", personalPhone ?? (object)DBNull.Value);
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

    // Создание заказа
    public async Task CreateOrderAsync(int id, double amount)
    {
        const string sql = @"
            INSERT INTO orders (id, amount)
            VALUES (@id, @amount);";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        cmd.Parameters.AddWithValue("@amount", amount);
        await cmd.ExecuteNonQueryAsync();
    }


    public async Task<Order?> GetOrderByIdAsync(int id)
    {
        const string sql = @"
            SELECT id, order_id, amount, date
            FROM orders
            WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Order
            {
                Id = reader.GetInt32("id"),
                OrderId = reader.GetInt32("order_id"),
                Amount = reader.GetDouble("amount"),
                Date = reader.GetDateTime("date")
            };
        }

        return null;
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
                UserChatId = reader.GetInt64("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            };
        }

        return null;
    }

    public async Task<PromoCode?> GetPromoCodeByIdAsync(int id)
    {
        const string sql = @"
            SELECT 
                id, code, user_chat_id, order_id, 
                give_id, status, submitted_at
            FROM promo_codes
            WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                UserChatId = reader.GetInt64("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            };
        }

        return null;
    }

    public async Task UpdatePromoCodeAsync(PromoCode promoCode)
    {
        const string sql = @"
            UPDATE promo_codes 
            SET 
                status = @status
            WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);

        cmd.Parameters.AddWithValue("@id", promoCode.Id);
        cmd.Parameters.AddWithValue("@status", promoCode.Status);

        await cmd.ExecuteNonQueryAsync();
    }

    // Получение списка промокодов пользователя
    public async Task<List<PromoCode>> GetAllPromoCodesForUserAsync(long chatId, string? status = null)
    {
        var promoCodes = new List<PromoCode>();
        var sql = new StringBuilder(@"
            SELECT p.id, p.code, p.user_chat_id, p.order_id, p.give_id, p.status, p.submitted_at
            FROM promo_codes p
            WHERE p.user_chat_id = @chatId");

        if (!string.IsNullOrWhiteSpace(status))
        {
            sql.Append(" AND p.status = @status");
        }

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        await using var cmd = new MySqlCommand(sql.ToString(), conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);

        if (!string.IsNullOrWhiteSpace(status))
        {
            cmd.Parameters.AddWithValue("@status", status);
        }

        await using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            promoCodes.Add(new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                UserChatId = reader.GetInt64("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            });
        }

        return promoCodes;
    }


    public async Task<List<PromoCode>> GetAllPromoCodesAsync()
    {
        var list = new List<PromoCode>();
        const string sql = @"SELECT * FROM promo_codes;";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            list.Add(new PromoCode
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                UserChatId = reader.GetInt64("user_chat_id"),
                OrderId = reader.GetInt32("order_id"),
                GiveId = reader.GetInt32("give_id"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at")
            });
        }

        return list;
    }

    public async Task<int> GetUserPromoCodeCountAsync(long userChatId, int giveId)
    {
        const string sql = @"
        SELECT COUNT(*) 
        FROM promo_codes 
        WHERE user_chat_id = @userChatId AND give_id = @giveId AND status = 'active'";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@userChatId", userChatId);
        cmd.Parameters.AddWithValue("@giveId", giveId);

        return Convert.ToInt32(await cmd.ExecuteScalarAsync());
    }

    public async Task<List<PromoCode>> GetPromoCodesByGiveIdAsync(int giveId, int requiredPromoCodeCount, string status = "active")
    {
        var promoCodes = new List<PromoCode>();
        const string sql = @"
        WITH UserPromoCounts AS (
            SELECT 
                user_chat_id,
                COUNT(*) as promo_count
            FROM promo_codes
            WHERE give_id = @giveId AND status = @status
            GROUP BY user_chat_id
            HAVING COUNT(*) >= @requiredCount
        )
        SELECT 
            pc.id, pc.code, pc.order_id, pc.give_id, 
            pc.status, pc.submitted_at, pc.user_chat_id
        FROM promo_codes pc
        JOIN UserPromoCounts upc ON pc.user_chat_id = upc.user_chat_id
        WHERE pc.give_id = @giveId AND pc.status = @status";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", giveId);
        cmd.Parameters.AddWithValue("@status", status);
        cmd.Parameters.AddWithValue("@requiredCount", requiredPromoCodeCount);

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

    public async Task<PromoCodeResult> GetFilteredPromoCodesAsync(PromoCodeFilter filter)
    {
        var result = new PromoCodeResult();
        var promoCodes = new List<PromoCodeViewModel>();

        var sql = new StringBuilder(@"
        SELECT 
            pc.id, 
            pc.code, 
            pc.status, 
            pc.submitted_at,
            u.name AS user_name,
            g.name AS give_name,
            o.order_id,
            p.name AS prize_name
        FROM promo_codes pc
        LEFT JOIN users u ON pc.user_chat_id = u.chat_id
        LEFT JOIN gives g ON pc.give_id = g.id
        LEFT JOIN orders o ON o.id = pc.order_id
        LEFT JOIN winners w ON w.promo_code_id = pc.id
        LEFT JOIN prizes p ON w.prize_id = p.id
        WHERE 1=1");

        var parameters = new List<MySqlParameter>();

        // Условия фильтрации
        if (!string.IsNullOrEmpty(filter.Search))
        {
            sql.Append(" AND pc.code LIKE @search");
            parameters.Add(new MySqlParameter("@search", $"%{filter.Search}%"));
        }

        if (filter.UserChatId.HasValue)
        {
            sql.Append(" AND u.chat_id = @userId");
            parameters.Add(new MySqlParameter("@userId", filter.UserChatId.Value));
        }

        if (filter.OrderId.HasValue)
        {
            sql.Append(" AND o.order_id = @order_id");
            parameters.Add(new MySqlParameter("@order_id", filter.OrderId.Value));
        }

        if (filter.GiveId.HasValue)
        {
            sql.Append(" AND pc.give_id = @give_id");
            parameters.Add(new MySqlParameter("@give_id", filter.GiveId.Value));
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            sql.Append(" AND pc.status = @status");
            parameters.Add(new MySqlParameter("@status", filter.Status));
        }

        if (!string.IsNullOrEmpty(filter.PrizeName))
        {
            sql.Append(" AND p.name = @prize_name");
            parameters.Add(new MySqlParameter("@prize_name", filter.PrizeName));
        }

        // Подсчёт общего количества
        var countSql = new StringBuilder(@"
        SELECT COUNT(DISTINCT pc.id)
        FROM promo_codes pc
        LEFT JOIN users u ON pc.user_chat_id = u.chat_id
        LEFT JOIN gives g ON pc.give_id = g.id
        LEFT JOIN orders o ON o.id = pc.order_id
        LEFT JOIN winners w ON w.promo_code_id = pc.id
        LEFT JOIN prizes p ON w.prize_id = p.id
        WHERE 1=1");

        if (!string.IsNullOrEmpty(filter.Search))
        {
            countSql.Append(" AND pc.code LIKE @search");
        }

        if (filter.UserChatId.HasValue)
        {
            countSql.Append(" AND u.chat_id = @userId");
        }

        if (filter.OrderId.HasValue)
        {
            countSql.Append(" AND o.order_id = @order_id");
        }

        if (filter.GiveId.HasValue)
        {
            countSql.Append(" AND pc.give_id = @give_id");
        }

        if (!string.IsNullOrEmpty(filter.Status))
        {
            countSql.Append(" AND pc.status = @status");
        }

        if (!string.IsNullOrEmpty(filter.PrizeName))
        {
            countSql.Append(" AND p.name = @prize_name");
        }

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        // Выполняем подсчёт количества
        using (var countCmd = new MySqlCommand(countSql.ToString(), conn))
        {
            if (!string.IsNullOrEmpty(filter.Search))
                countCmd.Parameters.AddWithValue("@search", $"%{filter.Search}%");

            if (filter.UserChatId.HasValue)
                countCmd.Parameters.AddWithValue("@userId", filter.UserChatId.Value);

            if (filter.OrderId.HasValue)
                countCmd.Parameters.AddWithValue("@order_id", filter.OrderId.Value);

            if (filter.GiveId.HasValue)
                countCmd.Parameters.AddWithValue("@give_id", filter.GiveId.Value);

            if (!string.IsNullOrEmpty(filter.Status))
                countCmd.Parameters.AddWithValue("@status", filter.Status);

            if (!string.IsNullOrEmpty(filter.PrizeName))
                countCmd.Parameters.AddWithValue("@prize_name", filter.PrizeName);

            result.TotalCount = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        }

        // Сортировка и пагинация
        sql.Append(" GROUP BY pc.id, pc.code, pc.status, pc.submitted_at, u.name, g.name, o.order_id, p.name");
        sql.Append(" ORDER BY pc.submitted_at DESC");
        sql.Append(" LIMIT @offset, @page_size");

        parameters.Add(new MySqlParameter("@offset", (filter.Page - 1) * filter.PageSize));
        parameters.Add(new MySqlParameter("@page_size", filter.PageSize));

        // Выполняем основной запрос
        using (var cmd = new MySqlCommand(sql.ToString(), conn))
        {
            cmd.Parameters.AddRange(parameters.ToArray());

            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                promoCodes.Add(new PromoCodeViewModel
                {
                    Id = reader.GetInt32("id"),
                    Code = reader.GetString("code"),
                    OrderId = reader.IsDBNull(reader.GetOrdinal("order_id")) ? 0 : reader.GetInt32("order_id"),
                    UserName = reader.IsDBNull(reader.GetOrdinal("user_name")) ? "" : reader.GetString("user_name"),
                    GiveName = reader.IsDBNull(reader.GetOrdinal("give_name")) ? "" : reader.GetString("give_name"),
                    Status = reader.GetString("status"),
                    SubmittedAt = reader.GetDateTime("submitted_at"),
                    PrizeName = reader.IsDBNull(reader.GetOrdinal("prize_name")) ? "" : reader.GetString("prize_name")
                });
            }
        }

        result.PromoCodes = promoCodes;
        return result;
    }

    public async Task<List<PromoCodeViewModel>> GetAllPromoCodesViewModelAsync()
    {
        var promoCodes = new List<PromoCodeViewModel>();

        var sql = @"
            SELECT 
                pc.id, pc.code, pc.status, pc.submitted_at,
                u.name AS user_name,
                g.name AS give_name,
                o.order_id,
                (
                    SELECT p.name 
                    FROM winners w
                    JOIN prizes p ON w.prize_id = p.id
                    WHERE w.promo_code_id = pc.id
                    LIMIT 1
                ) AS prize_name
            FROM promo_codes pc
            LEFT JOIN users u ON pc.user_chat_id = u.chat_id
            LEFT JOIN gives g ON pc.give_id = g.id
            LEFT JOIN orders o ON o.id = pc.order_id
            ORDER BY pc.submitted_at DESC";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            promoCodes.Add(new PromoCodeViewModel
            {
                Id = reader.GetInt32("id"),
                Code = reader.GetString("code"),
                OrderId = reader.GetInt32("order_id"),
                UserName = reader.IsDBNull(reader.GetOrdinal("user_name")) ? "" : reader.GetString("user_name"),
                GiveName = reader.IsDBNull(reader.GetOrdinal("give_name")) ? "" : reader.GetString("give_name"),
                Status = reader.GetString("status"),
                SubmittedAt = reader.GetDateTime("submitted_at"),
                PrizeName = reader.IsDBNull(reader.GetOrdinal("prize_name")) ? "" : reader.GetString("prize_name")
            });
        }

        return promoCodes;
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

    // Получение всех раздач (Give)
    public async Task<List<Give>> GetAllGivesAsync()
    {
        var gives = new List<Give>();
        const string sql = @"SELECT * FROM gives";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            gives.Add(new Give
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date"),
                IsCompleted = reader.GetBoolean("is_completed")
            });
        }

        return gives;
    }

    public async Task<List<Give>> GetActiveGivesAsync()
    {
        var gives = new List<Give>();
        const string sql = @"SELECT * FROM gives WHERE is_completed = 0;";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            gives.Add(new Give
            {
                Id = reader.GetInt32("id"),
                Name = reader.GetString("name"),
                StartDate = reader.GetDateTime("start_date"),
                EndDate = reader.GetDateTime("end_date"),
                IsCompleted = reader.GetBoolean("is_completed")
            });
        }

        return gives;
    }

    // Добавление новой раздачи (Give)
    public async Task AddGiveAsync(Give give)
    {
        const string sql = @"
        INSERT INTO gives (name, start_date, end_date, is_completed) 
        VALUES (@name, @startDate, @endDate, @isCompleted)";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@name", give.Name);
        cmd.Parameters.AddWithValue("@startDate", give.StartDate);
        cmd.Parameters.AddWithValue("@endDate", give.EndDate);
        cmd.Parameters.AddWithValue("@isCompleted", give.IsCompleted);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<long>> GetAllAdminChatIdsAsync()
    {
        var adminChatIds = new List<long>();
        const string sql = "SELECT chat_id FROM users WHERE role = 'admin'";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            if (!reader.IsDBNull(0))
            {
                adminChatIds.Add(reader.GetInt64(0));
            }
        }

        return adminChatIds;
    }

    public async Task<Give?> GetRelevantGiveAsync()
    {
        const string sql = @"
        SELECT id, name, start_date, end_date, is_completed
        FROM gives 
        WHERE is_completed = 0
        ORDER BY 
            CASE 
                WHEN start_date <= NOW() AND end_date >= NOW() THEN 1  -- активные
                WHEN start_date > NOW() THEN 2  -- будущие
                ELSE 3  -- прошедшие (но не завершенные)
            END,
            start_date ASC
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

    public async Task<Give?> GetFirstActiveGiveAsync()
    {
        const string sql = @"
            SELECT id, name, start_date, end_date, is_completed
            FROM gives 
            WHERE start_date <= NOW() AND end_date >= NOW() AND is_completed = 0
            ORDER BY start_date ASC
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

    public async Task<int> ExpirePromoCodesByGiveIdAsync(int giveId)
    {
        const string sql = @"
        UPDATE promo_codes
        SET status = 'expired'
        WHERE give_id = @GiveId AND status = 'active'";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@GiveId", giveId);

        int rowsAffected = await cmd.ExecuteNonQueryAsync();
        return rowsAffected;
    }

    public async Task<int?> GetFirstActiveGiveIdAsync()
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
    public async Task<int> AddPrizeAsync(Prize prize)
    {
        const string sql = @"
            INSERT INTO prizes 
                (give_id, name, image_path, quantity, required_promo_code_count, priority, spin_duration) 
            VALUES 
                (@giveId, @name, @imagePath, @quantity, @requiredPromoCodeCount, @priority, @spinDuration);
            SELECT LAST_INSERT_ID();";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", prize.GiveId);
        cmd.Parameters.AddWithValue("@name", prize.Name);
        cmd.Parameters.AddWithValue("@imagePath", prize.ImagePath);
        cmd.Parameters.AddWithValue("@quantity", prize.Quantity);
        cmd.Parameters.AddWithValue("@requiredPromoCodeCount", prize.RequiredPromoCodeCount);
        cmd.Parameters.AddWithValue("@priority", prize.Priority);
        cmd.Parameters.AddWithValue("@spinDuration", prize.SpinDuration);

        var id = await cmd.ExecuteScalarAsync();
        return Convert.ToInt32(id);
    }

    // Получить все призы
    public async Task<List<Prize>> GetAllPrizesAsync()
    {
        const string sql = @"
        SELECT p.*, 
               (SELECT COUNT(*) FROM winners w WHERE w.prize_id = p.id) AS WinnerCount
        FROM prizes p";

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
                Name = reader.GetString(reader.GetOrdinal("name")),
                ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                RequiredPromoCodeCount = reader.GetInt32(reader.GetOrdinal("required_promo_code_count")),
                Priority = reader.GetInt32(reader.GetOrdinal("priority")),
                SpinDuration = reader.GetInt32(reader.GetOrdinal("spin_duration")),
                WinnerCount = reader.GetInt32(reader.GetOrdinal("WinnerCount"))
            });
        }
        return prizes;
    }

    // Обновить название приза
    public async Task UpdatePrizeAsync(Prize prize)
    {
        const string sql = @"
        UPDATE prizes 
        SET 
            give_id = @giveId, 
            name = @name, 
            image_path = @imagePath,
            priority = @priority,
            spin_duration = @spinDuration,
            quantity = @quantity,
            required_promo_code_count = @requiredPromoCodeCount
        WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", prize.Id);
        cmd.Parameters.AddWithValue("@giveId", prize.GiveId);
        cmd.Parameters.AddWithValue("@name", prize.Name);
        cmd.Parameters.AddWithValue("@imagePath", prize.ImagePath);
        cmd.Parameters.AddWithValue("@priority", prize.Priority);
        cmd.Parameters.AddWithValue("@spinDuration", prize.SpinDuration);
        cmd.Parameters.AddWithValue("@quantity", prize.Quantity);
        cmd.Parameters.AddWithValue("@requiredPromoCodeCount", prize.RequiredPromoCodeCount);

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

    public async Task<List<PrizeViewModel>> GetPrizesWithWinnersAsync(int giveId)
    {
        var prizes = new List<PrizeViewModel>();

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        // Получаем призы
        const string prizeSql = "SELECT * FROM prizes WHERE give_id = @GiveId";
        using var prizeCmd = new MySqlCommand(prizeSql, conn);
        prizeCmd.Parameters.AddWithValue("@GiveId", giveId);

        using var prizeReader = await prizeCmd.ExecuteReaderAsync();
        while (await prizeReader.ReadAsync())
        {
            prizes.Add(new PrizeViewModel
            {
                Id = prizeReader.GetInt32("id"),
                Name = prizeReader.GetString("name"),
                ImagePath = prizeReader.GetString("image_path"),
                Priority = prizeReader.GetInt32("priority"),
                SpinDuration = prizeReader.GetInt32("spin_duration"),
                Quantity = prizeReader.GetInt32("quantity"),
                RequiredPromoCodeCount = prizeReader.GetInt32("required_promo_code_count"),
                WonCount = 0
            });
        }
        await prizeReader.CloseAsync();

        // Для каждого приза получаем победителей и их промокоды
        foreach (var prize in prizes)
        {
            const string winnerSql = @"
                SELECT u.name, pc.code 
                FROM winners w
                JOIN promo_codes pc ON w.promo_code_id = pc.id
                JOIN users u ON pc.user_chat_id = u.chat_id
                WHERE w.prize_id = @PrizeId";

            using var winnerCmd = new MySqlCommand(winnerSql, conn);
            winnerCmd.Parameters.AddWithValue("@PrizeId", prize.Id);

            using var winnerReader = await winnerCmd.ExecuteReaderAsync();
            while (await winnerReader.ReadAsync())
            {
                prize.WinnerNames.Add(winnerReader.GetString("name"));
                prize.WinnerPromoCodes.Add(winnerReader.GetString("code")); // Добавляем промокод
            }
            prize.WonCount = prize.WinnerNames.Count;
            await winnerReader.CloseAsync();
        }

        return prizes;
    }

    public async Task AddWinnerAsync(int promoCodeId, int prizeId)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            INSERT INTO winners (promo_code_id, prize_id)
            VALUES (@promoCodeId, @prizeId)";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@promoCodeId", promoCodeId);
        cmd.Parameters.AddWithValue("@prizeId", prizeId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdatePromoCodeStatusAsync(int promoCodeId, string status)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            UPDATE promo_codes 
            SET status = @status 
            WHERE id = @promoCodeId";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@promoCodeId", promoCodeId);
        cmd.Parameters.AddWithValue("@status", status);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task IncrementPrizeWinCountAsync(int prizeId)
    {
        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        const string sql = @"
            UPDATE prizes 
            SET win_count = win_count + 1 
            WHERE id = @prizeId";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@prizeId", prizeId);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<WinnerStatModel>> GetWinnerStatisticsAsync()
    {
        using var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync();

        var query = @"
                    SELECT 
            g.id AS GiveawayId,
            g.name AS GiveawayName,
            g.start_date,
            g.end_date,
            g.is_completed,
            p.id AS PrizeId,
            p.name AS PrizeName,
            p.image_path,
            pc.code AS PromoCode,
            u.name AS WinnerName
        FROM gives g
        LEFT JOIN prizes p ON p.give_id = g.id
        LEFT JOIN winners w ON w.prize_id = p.id
        LEFT JOIN promo_codes pc ON pc.id = w.promo_code_id
        LEFT JOIN users u ON u.chat_id = pc.user_chat_id
        ORDER BY g.start_date DESC, g.id, p.id;";

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
                    IsCompleted = reader.GetBoolean("is_completed"),
                    Winners = []
                };
                giveawayDict[giveawayId] = giveaway;
            }

            // Проверяем, есть ли информация о призе
            if (!reader.IsDBNull(reader.GetOrdinal("PrizeName")))
            {
                var prizeName = reader.GetString("PrizeName");
                var promoCode = reader.IsDBNull(reader.GetOrdinal("PromoCode"))
                    ? "-"
                    : reader.GetString("PromoCode");
                var imagePath = reader.IsDBNull(reader.GetOrdinal("image_path"))
                    ? ""
                    : reader.GetString("image_path");
                var winnerName = reader.IsDBNull(reader.GetOrdinal("WinnerName"))
                    ? "-"
                    : reader.GetString("WinnerName");

                giveaway.Winners.Add(new PrizeWinner
                {
                    PrizeName = prizeName,
                    WinnerName = winnerName,
                    PromoCode = promoCode,
                    ImagePath = imagePath
                });
            }
        }

        return [.. giveawayDict.Values];
    }

    public async Task<Prize?> GetPrizeByIdAsync(int id)
    {
        const string sql = @"SELECT * FROM prizes WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Prize
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                GiveId = reader.GetInt32(reader.GetOrdinal("give_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                Priority = reader.GetInt32(reader.GetOrdinal("priority")),
                SpinDuration = reader.GetInt32(reader.GetOrdinal("spin_duration")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                RequiredPromoCodeCount = reader.GetInt32(reader.GetOrdinal("required_promo_code_count")),
                WinCount = reader.IsDBNull(reader.GetOrdinal("win_count")) ? 0 : reader.GetInt32(reader.GetOrdinal("win_count"))
            };
        }

        return null;
    }

    public async Task<Prize?> GetPrizeByPromoCodeIdAsync(int promoCodeId)
    {
        const string sql = @"
        SELECT p.* 
        FROM winners w
        JOIN prizes p ON w.prize_id = p.id
        WHERE w.promo_code_id = @promoCodeId
        LIMIT 1";

        await using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        await using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@promoCodeId", promoCodeId);

        await using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Prize
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                GiveId = reader.GetInt32(reader.GetOrdinal("give_id")),
                Name = reader.GetString(reader.GetOrdinal("name")),
                ImagePath = reader.GetString(reader.GetOrdinal("image_path")),
                Priority = reader.GetInt32(reader.GetOrdinal("priority")),
                SpinDuration = reader.GetInt32(reader.GetOrdinal("spin_duration")),
                Quantity = reader.GetInt32(reader.GetOrdinal("quantity")),
                RequiredPromoCodeCount = reader.GetInt32(reader.GetOrdinal("required_promo_code_count")),
                WinCount = reader.IsDBNull(reader.GetOrdinal("win_count")) ? 0 : reader.GetInt32(reader.GetOrdinal("win_count"))
            };
        }

        return null;
    }

    public async Task<Give?> GetGiveByIdAsync(int id)
    {
        const string sql = @"
        SELECT id, name, start_date, end_date, is_completed
        FROM gives
        WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

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

    public async Task UpdateGiveAsync(Give give)
    {
        const string sql = @"
        UPDATE gives 
        SET name = @name, 
            start_date = @startDate, 
            end_date = @endDate, 
            is_completed = @isCompleted
        WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", give.Id);
        cmd.Parameters.AddWithValue("@name", give.Name);
        cmd.Parameters.AddWithValue("@startDate", give.StartDate);
        cmd.Parameters.AddWithValue("@endDate", give.EndDate);
        cmd.Parameters.AddWithValue("@isCompleted", give.IsCompleted);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteGiveAsync(int id)
    {
        const string sql = @"DELETE FROM gives WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);
        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<Prize>> GetPrizesByGiveIdAsync(int giveId)
    {
        var prizes = new List<Prize>();
        const string sql = @"
        SELECT 
            id, give_id, name, image_path, 
            priority, spin_duration, 
            quantity, required_promo_code_count
        FROM prizes
        WHERE give_id = @giveId";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", giveId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prizes.Add(new Prize
            {
                Id = reader.GetInt32("id"),
                GiveId = reader.GetInt32("give_id"),
                Name = reader.GetString("name"),
                ImagePath = reader.GetString("image_path"),
                Priority = reader.GetInt32("priority"),
                SpinDuration = reader.GetInt32("spin_duration"),
                Quantity = reader.GetInt32("quantity"),
                RequiredPromoCodeCount = reader.GetInt32("required_promo_code_count")
            });
        }

        return prizes;
    }


    public async Task<List<Prize>> GetAvailablePrizesByGiveIdAsync(int giveId)
    {
        var prizes = new List<Prize>();
        const string sql = @"
        SELECT 
            p.id, p.give_id, p.name, p.image_path, 
            p.priority, p.spin_duration,
            p.required_promo_code_count, p.quantity,
            COUNT(w.id) as win_count
        FROM prizes p
        LEFT JOIN winners w ON p.id = w.prize_id
        WHERE p.give_id = @giveId
        GROUP BY p.id
        HAVING COUNT(w.id) < p.quantity OR COUNT(w.id) IS NULL";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();
        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@giveId", giveId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            prizes.Add(new Prize
            {
                Id = reader.GetInt32("id"),
                GiveId = reader.GetInt32("give_id"),
                Name = reader.GetString("name"),
                ImagePath = reader.GetString("image_path"),
                Priority = reader.GetInt32("priority"),
                SpinDuration = reader.GetInt32("spin_duration"),
                Quantity = reader.GetInt32("quantity"),
                WinCount = reader.IsDBNull("win_count") ? 0 : reader.GetInt32("win_count"),
                RequiredPromoCodeCount = reader.GetInt32("required_promo_code_count")
            });
        }

        return prizes;
    }

    public async Task<List<Article>> GetAllArticlesAsync()
    {
        var articles = new List<Article>();
        const string sql = @"
        SELECT id, title, content, image_path, created_at, updated_at, views 
        FROM articles 
        ORDER BY created_at DESC";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            articles.Add(new Article
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Content = reader.GetString("content"),
                ImagePath = reader.IsDBNull("image_path") ? null : reader.GetString("image_path"),
                CreatedAt = reader.GetDateTime("created_at"),
                UpdatedAt = reader.IsDBNull("updated_at") ? null : reader.GetDateTime("updated_at"),
                Views = reader.GetInt32("views")
            });
        }

        return articles;
    }

    public async Task<Article?> GetArticleByIdAsync(int id)
    {
        const string sql = @"
        SELECT id, title, content, image_path, created_at, updated_at, views 
        FROM articles 
        WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        using var reader = await cmd.ExecuteReaderAsync();
        if (await reader.ReadAsync())
        {
            return new Article
            {
                Id = reader.GetInt32("id"),
                Title = reader.GetString("title"),
                Content = reader.GetString("content"),
                ImagePath = reader.IsDBNull("image_path") ? null : reader.GetString("image_path"),
                CreatedAt = reader.GetDateTime("created_at"),
                UpdatedAt = reader.IsDBNull("updated_at") ? null : reader.GetDateTime("updated_at"),
                Views = reader.GetInt32("views")
            };
        }

        return null;
    }

    public async Task CreateArticleAsync(Article article)
    {
        const string sql = @"
        INSERT INTO articles (title, content, image_path, created_at, views)
        VALUES (@title, @content, @imagePath, @createdAt, @views)";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@title", article.Title);
        cmd.Parameters.AddWithValue("@content", article.Content);
        cmd.Parameters.AddWithValue("@imagePath", article.ImagePath ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@createdAt", article.CreatedAt);
        cmd.Parameters.AddWithValue("@views", article.Views);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task UpdateArticleAsync(Article article)
    {
        const string sql = @"
        UPDATE articles 
        SET title = @title, 
            content = @content, 
            image_path = @imagePath, 
            updated_at = @updatedAt,
            views = @views
        WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", article.Id);
        cmd.Parameters.AddWithValue("@title", article.Title);
        cmd.Parameters.AddWithValue("@content", article.Content);
        cmd.Parameters.AddWithValue("@imagePath", article.ImagePath ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@updatedAt", DateTime.Now);
        cmd.Parameters.AddWithValue("@views", article.Views);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task DeleteArticleAsync(int id)
    {
        const string sql = "DELETE FROM articles WHERE id = @id";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@id", id);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task SaveBroadcastLogAsync(BroadcastLog log)
    {
        const string sql = @"
        INSERT INTO broadcast_logs (message, photo_url, success_count, total_count, sent_at, language)
        VALUES (@message, @photoUrl, @successCount, @totalCount, @sentAt, @language)";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@message", log.Message);
        cmd.Parameters.AddWithValue("@photoUrl", (object?)log.PhotoUrl ?? DBNull.Value);
        cmd.Parameters.AddWithValue("@successCount", log.SuccessCount);
        cmd.Parameters.AddWithValue("@totalCount", log.TotalCount);
        cmd.Parameters.AddWithValue("@sentAt", log.SentAt);
        cmd.Parameters.AddWithValue("@language", log.Language);

        await cmd.ExecuteNonQueryAsync();
    }

    public async Task<List<BroadcastLog>> GetBroadcastLogsAsync()
    {
        const string sql = "SELECT * FROM broadcast_logs ORDER BY sent_at DESC";
        var logs = new List<BroadcastLog>();

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            logs.Add(new BroadcastLog
            {
                Id = reader.GetInt32("id"),
                Message = reader.GetString("message"),
                PhotoUrl = reader["photo_url"] as string,
                SuccessCount = reader.GetInt32("success_count"),
                TotalCount = reader.GetInt32("total_count"),
                SentAt = reader.GetDateTime("sent_at"),
                Language = reader.GetString("language")
            });
        }

        return logs;
    }


    public async Task<List<UserPromoDetailsViewModel>> GetUserPromoDetailsAsync(long chatId)
    {
        const string sql = @"
        SELECT 
            pc.code AS Code,
            pc.status AS Status,
            pc.submitted_at AS SubmittedAt,
            o.order_id AS OrderNumber,
            o.amount AS OrderAmount,
            o.date AS OrderDate,
            g.name AS GiveName
        FROM promo_codes pc
        INNER JOIN orders o ON pc.order_id = o.id
        INNER JOIN gives g ON pc.give_id = g.id
        WHERE pc.user_chat_id = @chatId
        ORDER BY pc.submitted_at DESC";

        var list = new List<UserPromoDetailsViewModel>();

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            list.Add(new UserPromoDetailsViewModel
            {
                Code = reader.GetString("Code"),
                Status = reader.GetString("Status"),
                SubmittedAt = reader.GetDateTime("SubmittedAt"),
                OrderNumber = reader.GetInt32("OrderNumber"),
                OrderAmount = reader.GetDouble("OrderAmount"),
                OrderDate = reader.GetDateTime("OrderDate"),
                GiveName = reader.GetString("GiveName")
            });
        }

        return list;
    }

    public async Task<List<User>> SearchUsersAsync(string searchTerm, int limit = 20)
    {
        var users = new List<User>();
        const string sql = @"
        SELECT chat_id, name, phone
        FROM users
        WHERE name LIKE @search OR phone LIKE @search
        LIMIT @limit";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@search", $"%{searchTerm}%");
        cmd.Parameters.AddWithValue("@limit", limit);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            users.Add(new User
            {
                ChatId = reader.GetInt64("chat_id"),
                Name = reader.IsDBNull("name") ? null : reader.GetString("name"),
                Phone = reader.IsDBNull("phone") ? null : reader.GetString("phone")
            });
        }

        return users;
    }

    public async Task<(List<UserViewModel> Users, int TotalCount)> GetFilteredUserViewModelsAsync(UserFilter filter)
    {
        const string baseSql = @"
        WITH user_promocodes AS (
            SELECT 
                p.user_chat_id,
                COUNT(DISTINCT p.id) AS promo_code_count
            FROM promo_codes p
            GROUP BY p.user_chat_id
        ),
        user_orders AS (
            SELECT 
                p.user_chat_id,
                COUNT(DISTINCT o.id) AS order_count,
                SUM(DISTINCT o.amount) AS total_order_amount
            FROM orders o
            JOIN promo_codes p ON o.id = p.order_id
            GROUP BY p.user_chat_id
        )
        SELECT SQL_CALC_FOUND_ROWS
            u.chat_id,
            u.name,
            u.phone,
            u.personal_phone,
            u.language,
            u.registered_at,
            COALESCE(up.promo_code_count, 0) AS promo_code_count,
            COALESCE(uo.order_count, 0) AS order_count,
            COALESCE(uo.total_order_amount, 0) AS total_order_amount
        FROM users u
        LEFT JOIN user_promocodes up ON up.user_chat_id = u.chat_id
        LEFT JOIN user_orders uo ON uo.user_chat_id = u.chat_id
        WHERE 1=1";

        var sql = new StringBuilder(baseSql);

        if (!string.IsNullOrEmpty(filter.Search))
        {
            sql.Append(@"
            AND (u.name LIKE @search 
                OR u.phone LIKE @search 
                OR u.personal_phone LIKE @search)");
        }

        // Динамическая сортировка
        var safeSortColumns = new Dictionary<string, string>
    {
        { "order_count", "uo.order_count" },
        { "total_order_amount", "uo.total_order_amount" },
        { "promo_code_count", "up.promo_code_count" },
        { "chat_id", "u.chat_id" }
    };

        var sortColumn = safeSortColumns.ContainsKey(filter.SortBy)
            ? safeSortColumns[filter.SortBy]
            : "u.chat_id";

        var sortDir = filter.SortOrder.Equals("asc", StringComparison.OrdinalIgnoreCase)
            ? "ASC"
            : "DESC";

        sql.Append($@"
        ORDER BY {sortColumn} {sortDir}
        LIMIT @offset, @pageSize;
        SELECT FOUND_ROWS();");

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql.ToString(), conn);
        if (!string.IsNullOrEmpty(filter.Search))
            cmd.Parameters.AddWithValue("@search", $"%{filter.Search}%");

        cmd.Parameters.AddWithValue("@offset", (filter.Page - 1) * filter.PageSize);
        cmd.Parameters.AddWithValue("@pageSize", filter.PageSize);

        using var reader = await cmd.ExecuteReaderAsync();
        var users = new List<UserViewModel>();

        while (await reader.ReadAsync())
        {
            users.Add(new UserViewModel
            {
                ChatId = reader.GetInt64("chat_id"),
                Name = reader.GetString("name"),
                Phone = reader.GetString("phone"),
                PersonalPhone = reader.IsDBNull("personal_phone") ? null : reader.GetString("personal_phone"),
                Language = reader.GetString("language"),
                RegisteredAt = reader.GetDateTime("registered_at"),
                PromoCodeCount = reader.GetInt32("promo_code_count"),
                OrderCount = reader.GetInt32("order_count"),
                TotalOrderAmount = reader.GetDouble("total_order_amount")
            });
        }

        var totalCount = 0;
        if (await reader.NextResultAsync() && await reader.ReadAsync())
        {
            totalCount = reader.GetInt32(0);
        }

        return (users, totalCount);
    }

    public async Task<List<PromoCode>> GetPromoCodesByUserAsync(long chatId)
    {
        var promoCodes = new List<PromoCode>();

        const string sql = @"
            SELECT id, code, give_id, status, submitted_at, user_chat_id
            FROM promo_codes
            WHERE user_chat_id = @chatId
            ORDER BY submitted_at DESC";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            promoCodes.Add(new PromoCode
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Code = reader.GetString(reader.GetOrdinal("code")),
                GiveId = reader.GetInt32(reader.GetOrdinal("give_id")),
                Status = reader.GetString(reader.GetOrdinal("status")),
                SubmittedAt = reader.GetDateTime(reader.GetOrdinal("submitted_at")),
                UserChatId = reader.GetInt64(reader.GetOrdinal("user_chat_id"))
            });
        }

        return promoCodes;
    }


    public async Task<List<Order>> GetOrdersByUserAsync(long chatId)
    {
        var orders = new List<Order>();

        const string sql = @"
            SELECT DISTINCT o.id, o.order_id, o.amount, o.date
                FROM orders o
                INNER JOIN promo_codes p ON o.id = p.order_id
                WHERE p.user_chat_id = @chatId
                ORDER BY o.date DESC";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@chatId", chatId);

        using var reader = await cmd.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            var order = new Order
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                OrderId = reader.GetInt32(reader.GetOrdinal("order_id")),
                Amount = reader.GetDouble(reader.GetOrdinal("amount")),
                Date = reader.GetDateTime(reader.GetOrdinal("date"))
            };

            orders.Add(order);
        }

        return orders;
    }

    public async Task<List<string>> GetAllPrizeNamesAsync()
    {
        var prizeNames = new List<string>();

        const string sql = @"
        SELECT DISTINCT name 
        FROM prizes
        ORDER BY name";

        using var conn = new MySqlConnection(_connectionString);
        await conn.OpenAsync();

        using var cmd = new MySqlCommand(sql, conn);
        using var reader = await cmd.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            prizeNames.Add(reader.GetString("name"));
        }

        return prizeNames;
    }
}