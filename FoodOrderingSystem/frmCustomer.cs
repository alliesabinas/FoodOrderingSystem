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
    public partial class frmCustomer : Form
    {
       string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
        public frmCustomer()
        {
            InitializeComponent();

        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            txtCustomerId.Clear(); // Empties the ID so the system knows it's a NEW customer
            txtFullName.Clear();
            txtEmail.Clear();
            txtPhone.Clear();
            txtAddress.Clear();

            txtFullName.Focus();
        }

        private void frmCustomer_Load(object sender, EventArgs e)
        {
            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;
            LoadCustomers();
        }
        private void LoadCustomers()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Assuming your table columns are: customer_id, full_name, email, phone, address, registration_date
                    string query = @"SELECT customer_id AS 'ID', 
                                            full_name AS 'Full Name', 
                                            email AS 'Email', 
                                            phone AS 'Phone', 
                                            address AS 'Address' 
                                     FROM customers 
                                     ORDER BY customer_id ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvCustomers.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading customers: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvCustomers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            // Make sure they clicked a real row, not the header at the top
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvCustomers.Rows[e.RowIndex];

                // The column names in the brackets MUST match the 'AS' names from your SELECT query exactly
                txtCustomerId.Text = row.Cells["ID"].Value.ToString();
                txtFullName.Text = row.Cells["Full Name"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPhone.Text = row.Cells["Phone"].Value.ToString();
                txtAddress.Text = row.Cells["Address"].Value.ToString();
            }
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // Basic validation so we don't save blank names
            if (string.IsNullOrWhiteSpace(txtFullName.Text))
            {
                MessageBox.Show("Please enter a Full Name.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    // IF THE ID IS BLANK -> CREATE NEW
                    if (string.IsNullOrEmpty(txtCustomerId.Text))
                    {
                        // FIXED: Changed registration_date to created_at, and used NOW() for the exact time.
                        // Added default values for password and favorite_food to match your table structure!
                        query = "INSERT INTO customers (full_name, email, phone, address, created_at, password, favorite_food) VALUES (@name, @email, @phone, @address, NOW(), '12345', 'N/A')";
                    }
                    // IF THE ID HAS A NUMBER -> UPDATE EXISTING
                    else
                    {
                        query = "UPDATE customers SET full_name=@name, email=@email, phone=@phone, address=@address WHERE customer_id=@id";
                        cmd.Parameters.AddWithValue("@id", txtCustomerId.Text);
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@name", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@phone", txtPhone.Text);
                    cmd.Parameters.AddWithValue("@address", txtAddress.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Customer saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    // Reload the grid and wipe the textboxes clean!
                    LoadCustomers();
                    btnClear.PerformClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving customer: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // Don't do anything if no customer is selected
            if (string.IsNullOrEmpty(txtCustomerId.Text)) return;

            // ALWAYS ask for confirmation before deleting from a database!
            DialogResult result = MessageBox.Show("Are you sure you want to permanently delete this customer?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM customers WHERE customer_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtCustomerId.Text);
                            cmd.ExecuteNonQuery();

                            MessageBox.Show("Customer deleted.", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                            LoadCustomers();
                            btnClear.PerformClick();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error deleting customer: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {
            string keyword = txtSearch.Text.Trim();

            // 1. If the search box is empty, just reload all customers and stop.
            if (string.IsNullOrEmpty(keyword))
            {
                LoadCustomers();
                return;
            }

            // 2. Otherwise, search the database!
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // We use the exact same SELECT statement as LoadCustomers, but add a WHERE clause
                    string query = @"SELECT customer_id AS 'ID', 
                                    full_name AS 'Full Name', 
                                    email AS 'Email', 
                                    phone AS 'Phone', 
                                    address AS 'Address' 
                             FROM customers 
                             WHERE full_name LIKE @keyword 
                                OR email LIKE @keyword 
                                OR phone LIKE @keyword 
                             ORDER BY customer_id ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // The '%' symbol tells MySQL to look for the keyword ANYWHERE inside the text
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvCustomers.DataSource = dt; // Update the grid with the search results!
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error searching: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {
            frmDashboard ordersWindow = new frmDashboard();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void btnOrders_Click(object sender, EventArgs e)
        {
            frmOrders ordersWindow = new frmOrders();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            frmNewOrderForm ordersWindow = new frmNewOrderForm();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmCustomer ordersWindow = new frmCustomer();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            frmRestaurants ordersWindow = new frmRestaurants();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button8_Click(object sender, EventArgs e)
        {
            frmMenuItems ordersWindow = new frmMenuItems();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void panel3_Paint(object sender, PaintEventArgs e)
        {
            frmReports ordersWindow = new frmReports();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button11_Click(object sender, EventArgs e)
        {
            frmAbout ordersWindow = new frmAbout();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            frmUserManagement ordersWindow = new frmUserManagement();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void txtFullName_TextChanged(object sender, EventArgs e)
        {

        }
    }
}

