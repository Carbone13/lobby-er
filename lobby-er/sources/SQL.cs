using System;
using System.Security.Claims;
using Bonebreaker.Net;
using MySqlConnector;

namespace LobbyEr
{
    public class SQL
    {
        private const string SERVER_ADDRESS = "localhost";
        private const string DATABASE = "bonebreaker";
        private const string SHEET = "users";
        private const string UID = "root";
        private const string PWD = "piratebomb";
        
        private static SQL sg;

        private MySqlConnection connection;
        
        public SQL ()
        {
            sg = this;
            
            const string connectionString = "SERVER=" + SERVER_ADDRESS + ";" + "DATABASE=" + 
                                            DATABASE + ";" + "UID=" + UID + ";" + "PASSWORD=" + PWD + ";";

            connection = new MySqlConnection(connectionString);
        }

        private Account QueryAccount (string query)
        {
            if(OpenConnection())
            {
                //Create Command
                MySqlCommand cmd = new (query, connection);
                //Create a data reader and Execute the command
                MySqlDataReader dataReader = cmd.ExecuteReader();
        
                //Read the data and store them in the list
                while (dataReader.Read())
                {
                    Account account = Account.FromDatabaseEntry(dataReader);
                    dataReader.Close();
                    CloseConnection();

                    return account;
                }
                CloseConnection();
            }

            return null;
        }

        public static Account QueryAccountByID (int accountID)
        {
            string query = "SELECT * FROM " + SHEET + " AS u WHERE u.user_id ='" + accountID + "'";
            return sg.QueryAccount(query);
        }

        public static Account QueryAccountByUsername (string username)
        {
            string query = "SELECT * FROM " + SHEET + " AS u WHERE u.username='" + username + "'";
            return sg.QueryAccount(query);
        }

        public static Account QueryAccountByEmail (string email)
        {
            string query = "SELECT * FROM " + SHEET + " AS u WHERE u.email='" + email + "'";
            return sg.QueryAccount(query);
        }
        
        public static bool CheckIfEmailIsAlreadyInUse (string email)
        {
            string query = "SELECT COUNT(1) FROM " + SHEET + " AS u WHERE u.email = '" + email + "'";
            int count = -1;
            
            if (sg.OpenConnection())
            {
                MySqlCommand cmd = new(query, sg.connection);
                count = int.Parse(cmd.ExecuteScalar() + "");
                
                sg.CloseConnection();
            }

            return count > 0;
        }

        public static bool CheckIfUsernameIsAlreadyInUse (string username)
        {
            string query = "SELECT COUNT(1) FROM " + SHEET + " AS u WHERE u.username = '" + username + "'";
            int count = -1;
            
            if (sg.OpenConnection())
            {
                MySqlCommand cmd = new(query, sg.connection);
                count = int.Parse(cmd.ExecuteScalar() + "");
                
                sg.CloseConnection();
            }

            return count > 0;
        }

        public static void RegisterAccount (Account account)
        {
            if (CheckIfEmailIsAlreadyInUse(account.Email))
            {
                Console.WriteLine("Email already registered");
                return;
            }

            if (CheckIfUsernameIsAlreadyInUse(account.Username))
            {
                Console.WriteLine("Username already registered");
                return;
            }
            
            string query = "INSERT INTO " + SHEET + " (username, email, password) VALUES('" + account.Username + "','" +
                           account.Email + "', '" + account.Password + "')";

            if (sg.OpenConnection())
            {
                MySqlCommand cmd = new(query, sg.connection);
                cmd.ExecuteNonQuery();
                
                sg.CloseConnection();
            }
        }

        public static void UpdateAccount (Account account)
        {
            if (account.ID == -1) return;
            
            string query = "UPDATE " + SHEET + " SET username=" + account.Username + ", email=" + account.Email + ", password=" + account.Password + " WHERE id=" + account.ID;

            if (sg.OpenConnection())
            {
                MySqlCommand cmd = new(query, sg.connection);
                cmd.ExecuteNonQuery();
                
                sg.CloseConnection();
            }
        }
        
        public static void DeleteAccount (Account account)
        {
            if (account.ID == -1) return;
            
            string query = "DELETE FROM " + SHEET + " WHERE id=" + account.ID;

            if (sg.OpenConnection())
            {
                MySqlCommand cmd = new(query, sg.connection);
                cmd.ExecuteNonQuery();
                
                sg.CloseConnection();
            }
        }
        
        private bool OpenConnection()
        {
            try
            {
                connection.Open();
                return true;
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex);
            }

            return false;
        }

        private void CloseConnection ()
        {
            try
            {
                connection.Close();
            }
            catch (MySqlException ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
    }
}