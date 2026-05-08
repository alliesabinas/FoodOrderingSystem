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
using Org.BouncyCastle.Asn1.Cmp;

namespace FoodOrderingSystem
{
    public partial class frmRestaurants : Form
    {
       string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
        public frmRestaurants()
        {
            InitializeComponent();
        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            SearchAndFilter();
        }

        private void btnSearch_Click(object sender, EventArgs e)
        {

        }

        private void groupBox2_Enter(object sender, EventArgs e)
        {

        }

        private void frmRestaurants_Load(object sender, EventArgs e)
        {

            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;


            // Set default dropdown values so they aren't blank
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0; // "All Status"
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0; // "Open"

            LoadRestaurants();
        }

        private void SearchAndFilter()
        {
            string keyword = txtSearch.Text.Trim();
            string statusFilter = cboStatus.Text;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Start with the base query
                    string query = @"SELECT restaurant_id AS 'ID', 
                                    restaurant_name AS 'Restaurant Name', 
                                    location AS 'Location', 
                                    contact_number AS 'Contact', 
                                    status AS 'Status',
                                    GetRestaurantTotalSales(restaurant_id) AS 'Total Sales'
                             FROM restaurants 
                             WHERE (restaurant_name LIKE @keyword OR location LIKE @keyword)";

                    // Dynamically add the Status filter ONLY if they didn't pick 'All Status'
                    if (statusFilter != "All Status" && !string.IsNullOrEmpty(statusFilter))
                    {
                        query += " AND status = @status";
                    }

                    query += " ORDER BY restaurant_id ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");

                        if (statusFilter != "All Status" && !string.IsNullOrEmpty(statusFilter))
                        {
                            cmd.Parameters.AddWithValue("@status", statusFilter);
                        }

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvRestaurants.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Search Error: " + ex.Message); }
            }
        }
        private void LoadRestaurants()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // Notice how we call your GetRestaurantTotalSales function right in the query!
                    // Note: If your table columns are named slightly differently (like contact_number instead of contact), update them here.
                    string query = @"SELECT restaurant_id AS 'ID', 
                        restaurant_name AS 'Restaurant Name', 
                        location AS 'Location', 
                        contact_number AS 'Contact', 
                        status AS 'Status',
                        GetRestaurantTotalSales(restaurant_id) AS 'Total Sales'
                 FROM restaurants 
                 ORDER BY restaurant_id ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvRestaurants.DataSource = dt;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading restaurants: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvRestaurants_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvRestaurants.Rows[e.RowIndex];

                txtRestaurantId.Text = row.Cells["ID"].Value.ToString();
                txtName.Text = row.Cells["Restaurant Name"].Value.ToString();
                txtLocation.Text = row.Cells["Location"].Value.ToString();
                txtContact.Text = row.Cells["Contact"].Value.ToString();
                cboStatus.Text = row.Cells["Status"].Value.ToString();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtRestaurantId.Clear(); // Empties ID for a new insert!
            txtName.Clear();
            txtLocation.Clear();
            txtContact.Clear();
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;

            txtName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                MessageBox.Show("Please enter a Restaurant Name.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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

                    if (string.IsNullOrEmpty(txtRestaurantId.Text))
                    {
                        // INSERT NEW
                        query = "INSERT INTO restaurants (restaurant_name, location, contact_number, status) VALUES (@name, @loc, @contact, @status)";
                    }
                    else
                    {
                        // UPDATE EXISTING
                        query = "UPDATE restaurants SET restaurant_name=@name, location=@loc, contact_number=@contact, status=@status WHERE restaurant_id=@id";
                        cmd.Parameters.AddWithValue("@id", txtRestaurantId.Text);
                    }

                    cmd.CommandText = query;
                    cmd.Parameters.AddWithValue("@name", txtName.Text);
                    cmd.Parameters.AddWithValue("@loc", txtLocation.Text);
                    cmd.Parameters.AddWithValue("@contact", txtContact.Text);
                    cmd.Parameters.AddWithValue("@status", cboStatus.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Restaurant saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadRestaurants();
                    btnClear.PerformClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtRestaurantId.Text)) return;

            if (MessageBox.Show("Delete this restaurant?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        string query = "DELETE FROM restaurants WHERE restaurant_id = @id";
                        using (MySqlCommand cmd = new MySqlCommand(query, conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtRestaurantId.Text);
                            cmd.ExecuteNonQuery();
                            LoadRestaurants();
                            btnClear.PerformClick();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Error deleting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            SearchAndFilter();
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

        private void button10_Click(object sender, EventArgs e)
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

        private void button2_Click(object sender, EventArgs e)
        {
            frmUserManagement ordersWindow = new frmUserManagement();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }
    }
}

