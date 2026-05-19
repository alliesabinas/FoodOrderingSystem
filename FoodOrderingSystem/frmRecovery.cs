using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using MySql.Data.MySqlClient;


namespace FoodOrderingSystem
{
    public partial class frmRecovery : Form
    {
        public frmRecovery()
        {
            InitializeComponent();
        }

        private void txtEmail_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmRecovery_Load(object sender, EventArgs e)
        {

        }

        private void txtAnswer_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnRecover_Click(object sender, EventArgs e)
        {
            // 1. Grab what the user typed
            string emailInput = txtEmail.Text.Trim();
            string answerInput = txtAnswer.Text.Trim();

            if (emailInput == "" || answerInput == "")
            {
                MessageBox.Show("Please fell in both fields.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Connect to Database (Don't forget your password here!)
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // 3. Ask MySQL if the email and favorite food match
                    string query = "SELECT password FROM customers WHERE email = @email AND favorite_food = @food";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@email", emailInput);
                        cmd.Parameters.AddWithValue("@food", answerInput);

                        object result = cmd.ExecuteScalar();

                        if (result != null)
                        {
                            // Match found! Show the password in green.
                            lblResult.Text = "Success! Your password is: " + result.ToString();
                            lblResult.ForeColor = System.Drawing.Color.Green;
                        }
                        else
                        {
                            // No match found. Show error in red.
                            lblResult.Text = "Incorrect email or security answer.";
                            lblResult.ForeColor = System.Drawing.Color.Red;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Database Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            frmLogin login = new frmLogin();
            login.Show();
            this.Close();
        }
    }
}
