using Google.Apis.Auth.OAuth2;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.Sockets;
using System.Runtime.Remoting.Messaging;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace DiscordBot.Database
{
    internal class DBEngine
    {
        private string connectionString = "Host=Host goes here;Port=Port Goes there; Username=username here;Password=*******;Database=and the database;";

        public bool isLevelledUp = false;

        public async Task<bool> StoreUserAsync(DUser user)
        {

            int userCount = 0;

            try
            {
                var totalUsers = await GetTotalUsersAsync();
                
                Console.WriteLine(totalUsers.ToString());

                if (totalUsers.Item1 != true ) {

                    throw new Exception();

                }
                else
                {
                    totalUsers.Item2++;
                }
            
                userCount = await CheckUserExistenceAsync(user.userName);
                Console.WriteLine(userCount.ToString());    

                if (userCount == 0)
                {
                    await InsertUserAsync(user, totalUsers.Item2);
                    return true;
                }
                else
                {
                    Console.WriteLine("User already exists");
                    return false;
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                return false;
            }



            ///////////////////////////////////////////////////////

        }

        private async Task<int> CheckUserExistenceAsync(string userName)
        {
            using (var conn = new NpgsqlConnection(connectionString))
            {
                try
                {
                    await conn.OpenAsync();

                string queryDoubleCheck = $"SELECT COUNT (*) FROM data.userlevel WHERE username = '{userName}';";

                
                    using (var cmddoubleCheck = new NpgsqlCommand(queryDoubleCheck, conn))
                    {
                        return Convert.ToInt32(await cmddoubleCheck.ExecuteScalarAsync());
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return 1;
                }
            }
            }

        private async Task InsertUserAsync(DUser user, long totalUserCount)
        {
            Console.WriteLine("Insert UserAsync");
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {

                    await conn.OpenAsync();

                    string query = "INSERT INTO data.userlevel ( userno, username, serverid, avatarurl, level, xp, xplimit)" +
                                      $"VALUES ('{totalUserCount}', '{user.userName}', '{user.serverID}', '{user.avatarURL}', '{user.level}', '{user.XP}', '{user.xplimit}');";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }

                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());

            }

            
            }

        public async Task<(bool, DUser)> GetUserAsync(string username, ulong serverid)
        {
            DUser result;
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "select u.userno, u.username ,u.serverid, u.avatarurl, u.level, u.xp, u.xplimit " +
                                   "from data.userlevel u " +
                                   $"where username = '{username}' and serverid = {serverid};";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var reader = await cmd.ExecuteReaderAsync();
                        await reader.ReadAsync();

                        result = new DUser
                        {
                            userName = reader.GetString(1),
                            serverID = (ulong)reader.GetInt64(2),
                            avatarURL = reader.GetString(3),
                            level = reader.GetInt32(4),
                            XP = reader.GetInt32(5),
                            xplimit = reader.GetInt32(6),
                        };

                        Console.WriteLine(username);
                        Console.WriteLine("^^^^^^^^");
                    }
                    return (true, result);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() );
                return (false, null);
            }
            
        }
        private async Task<(bool, long)> GetTotalUsersAsync()
        {
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "SELECT COUNT (*) FROM data.userlevel";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        var userCount = await cmd.ExecuteScalarAsync();

                        return (true, Convert.ToInt64(userCount));
                    }
                }
            }
            catch (Exception ex)
            {

                Console.WriteLine(ex.ToString());
                return (false, -1);
            }

        }

        public async Task <bool> AddXpAsync(string username, ulong serverID)
        {
            var XPAmounts = await DetermineXPAsync(username, serverID);
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "update data.userlevel " +
                                   $"SET xp = xp + {XPAmounts.Item1}, xplimit = {XPAmounts.Item2} " +
                                   $"where username = '{username}' AND serverid = {serverID}";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString() );
                return false;
            }
            
        }

        private async Task<(double, int)> DetermineXPAsync(string username, ulong serverid)
        {
            var user = await GetUserAsync(username, serverid);
            //Weitere Level hinzufügen 
            switch (user.Item2.level) { 
            
                case int level when level >= 1 && level <= 5 :
                    return (10.0, 100);
                case int level when level >= 6 && level <= 10 :
                    return (5.0, 200);
                case int level when level >= 11:
                    return (5.0, 300);

            }
            //Default
            return (10.0, 100);
        }

        public async Task<bool> LevelUpAsync(string username, ulong serverID)
        {
            isLevelledUp = false;
            var XPAmounts = await DetermineXPAsync(username, serverID);
            try
            {
                using (var conn = new NpgsqlConnection(connectionString))
                {
                    await conn.OpenAsync();

                    string query = "update data.userlevel " +
                                   $"set level = level + 1, xp = 0, xplimit = {XPAmounts.Item2} " +
                                   $"where username = '{username}' and serverid = {serverID}";

                    using (var cmd = new NpgsqlCommand(query, conn))
                    {
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                isLevelledUp = true;
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
            
        }

    }
}
