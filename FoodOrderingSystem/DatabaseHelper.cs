using System;
using MySql.Data.MySqlClient;

namespace FoodOrderingSystem
{
    public class DatabaseHelper
    {
        // Your central connection string
        private static string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

        // A public method that any form can call to get a connection
        public static MySqlConnection GetConnection()
        {
            return new MySqlConnection(connectionString);
        }
    }
}