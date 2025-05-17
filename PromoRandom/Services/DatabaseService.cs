using MySql.Data.MySqlClient;
using System.Data;


namespace AksiyaBot
{
    public class DatabaseService()
    {
        private readonly string _connectionString = "Server=localhost;Database=promokod_db;Uid=root;Pwd=;";


        public async Task<bool> IsUserRegisteredAsync(long chatId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT COUNT(*) FROM users WHERE chat_id = @chatId", conn);
            cmd.Parameters.AddWithValue("@chatId", chatId);
            var count = Convert.ToInt32(await cmd.ExecuteScalarAsync());
            return count > 0;
        }

        public async Task RegisterUserAsync(string name, string phone, long chatId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("INSERT INTO users (name, phone, role, chat_id) VALUES (@name, @phone, 'client', @chatId)", conn);
            cmd.Parameters.AddWithValue("@name", name);
            cmd.Parameters.AddWithValue("@phone", phone);
            cmd.Parameters.AddWithValue("@chatId", chatId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> SaveCheckAsync(long chatId, string fileId, int messageId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(@"
                INSERT INTO checks (user_id, image_file_id, message_id)
                VALUES ((SELECT id FROM users WHERE chat_id = @chatId), @fileId, @messageId);
                SELECT LAST_INSERT_ID();", conn);

            cmd.Parameters.AddWithValue("@chatId", chatId);
            cmd.Parameters.AddWithValue("@fileId", fileId);
            cmd.Parameters.AddWithValue("@messageId", messageId);
            return Convert.ToInt32(await cmd.ExecuteScalarAsync());
        }


        public async Task IssuePromoCodeAsync(int checkId, string promoCode)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand(@"
            UPDATE checks SET status = 'approved' WHERE id = @checkId;
            INSERT INTO promo_codes (code, user_id)
            SELECT @promoCode, c.user_id FROM checks c WHERE c.id = @checkId;", conn);
            cmd.Parameters.AddWithValue("@checkId", checkId);
            cmd.Parameters.AddWithValue("@promoCode", promoCode);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RejectCheckAsync(int checkId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("UPDATE checks SET status = 'rejected' WHERE id = @checkId", conn);
            cmd.Parameters.AddWithValue("@checkId", checkId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task<long?> GetUserChatIdByCheckId(int checkId)
        {
            using var conn = new MySqlConnection(_connectionString);
            await conn.OpenAsync();
            using var cmd = new MySqlCommand("SELECT u.chat_id FROM users u JOIN checks c ON u.id = c.user_id WHERE c.id = @checkId", conn);
            cmd.Parameters.AddWithValue("@checkId", checkId);
            var result = await cmd.ExecuteScalarAsync();
            return result == DBNull.Value ? null : Convert.ToInt64(result);
        }

        public async Task<List<string>> GetPromoCodesByUserAsync()
        {   var codes = new List<string>();
            using var connection = new MySqlConnection(_connectionString);
            await connection.OpenAsync();
            var query = @"
                SELECT p.code
                FROM promo_codes p
                JOIN users u ON p.user_id = u.id
               ";
            using var cmd = new MySqlCommand(query, connection);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                codes.Add(reader.GetString("code"));
            }

            return codes;
        }

    }
}