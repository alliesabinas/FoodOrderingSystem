using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;

namespace FoodOrderingSystem
{
    public partial class frmLogin : Form
    {
        public frmLogin()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            // 1. Define your connection string (Replace 'root' and '' with your actual MySQL username/password if different)
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

            // 2. Write the SQL query to check the email and password
            // We use parameters (@email, @pass) to prevent SQL Injection hackers!
            string query = "SELECT full_name FROM customers WHERE email = @email AND password = @pass";

            // 3. Connect to the database and execute
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Attach the text from your textboxes to the query
                        cmd.Parameters.AddWithValue("@email", txtEmail.Text.Trim());
                        cmd.Parameters.AddWithValue("@pass", txtPassword.Text);

                        // ExecuteScalar grabs the very first column of the very first row found
                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            // Login Success!
                            string loggedInName = result.ToString();
                            MessageBox.Show("Welcome back, " + loggedInName + "!", "Login Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                            frmDashboard dashboard = new frmDashboard();
                            dashboard.Show();
                            this.Hide();

                        }
                        else
                        {
                            // Login Failed
                            MessageBox.Show("Invalid email or password. Please try again.", "Login Failed", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // If the database is offline or the connection string is wrong, this will tell us why.
                    MessageBox.Show("Database Connection Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void txtPassword_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmLogin_Load(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            frmRecovery recoveryForm = new frmRecovery();
            recoveryForm.Show();
            this.Hide();
        }
    }
}
