using LiteNetLib.Utils;
using MySqlConnector;

namespace Bonebreaker.Net
{
    public class Account : INetSerializable
    {
        public int ID = -1;
        public string Username, Email, Password;

        public static Account FromDatabaseEntry (MySqlDataReader reader)
        {
            return new Account()
            {
                ID = (int) reader["user_id"],
                Username = (string) reader["username"],
                Email = (string) reader["email"],
            };

        }
        
        public void Serialize (NetDataWriter writer)
        {
            writer.Put(ID);
            writer.Put(Username, 50);
            writer.Put(Email, 100);
            writer.Put(Password, 100);
        }

        public void Deserialize (NetDataReader reader)
        {
            ID = reader.GetInt();
            Username = reader.GetString(50);
            Email = reader.GetString(100);
            Password = reader.GetString(100);
        }

        public override string ToString ()
        {
            return "id: " + ID + " username: " + Username + " pwd: " + Password + " email: " + Email;
        }
    }
}