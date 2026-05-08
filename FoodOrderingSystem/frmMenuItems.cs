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
    public partial class frmMenuItems : Form
    {

       string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
        public frmMenuItems()
        {
            InitializeComponent();
        }

        private void frmMenuItems_Load(object sender, EventArgs e)
        {
            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;

            // 1. Set default static dropdown values
            if (cboFilterAvailability.Items.Count > 0) cboFilterAvailability.SelectedIndex = 0; // "All"
            if (cboAvailability.Items.Count > 0) cboAvailability.SelectedIndex = 0; // "Available"

            // 2. Load the dynamic Restaurant dropdowns FIRST
            LoadRestaurantDropdowns();

            // 3. Then load the grid data
            LoadMenuItems();
        }

        // --- LOAD THE FOREIGN KEYS (RESTAURANTS) ---
        private void LoadRestaurantDropdowns()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT restaurant_id, restaurant_name FROM restaurants";

                    using (MySqlDataAdapter da = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        da.Fill(dt);

                        // Bind to the bottom input dropdown
                        // Bind to the bottom input dropdown (NOTICE DATASOURCE IS LAST)
                        cboRestaurant.ValueMember = "restaurant_id";     // Hidden ID for saving
                        cboRestaurant.DisplayMember = "restaurant_name"; // What user sees
                        cboRestaurant.DataSource = dt;

                        // Bind a CLONE of the data to the top filter dropdown
                        DataTable dtFilter = dt.Copy();

                        // Add an "All Restaurants" option to the top filter
                        DataRow allRow = dtFilter.NewRow();
                        allRow["restaurant_id"] = 0; // Fake ID
                        allRow["restaurant_name"] = "— All Restaurants —";
                        dtFilter.Rows.InsertAt(allRow, 0);

                        // Bind filter dropdown (NOTICE DATASOURCE IS LAST)
                        cboFilterRestaurant.ValueMember = "restaurant_id";
                        cboFilterRestaurant.DisplayMember = "restaurant_name";
                        cboFilterRestaurant.DataSource = dtFilter;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading restaurants: " + ex.Message); }
            }
        }

        private void LoadMenuItems()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // We JOIN the restaurants table so we can show the name, not just the ID!
                    string query = @"SELECT m.item_id AS 'Item ID', 
                                            r.restaurant_name AS 'Restaurant', 
                                            m.item_name AS 'Item Name', 
                                            m.price AS 'Price', 
                                            m.availability AS 'Availability',
                                            m.restaurant_id AS 'HiddenRestID' 
                                     FROM menu_items m
                                     JOIN restaurants r ON m.restaurant_id = r.restaurant_id
                                     ORDER BY m.item_id ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvMenuItems.DataSource = dt;

                        // Hide the specific restaurant ID column from the user, but keep it in the grid so we can use it for editing later!
                        if (dgvMenuItems.Columns["HiddenRestID"] != null)
                        {
                            dgvMenuItems.Columns["HiddenRestID"].Visible = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading menu items: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }


        

        private void cboRestaurant_TextChanged(object sender, EventArgs e)
        {

        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtItemId.Clear();
            txtItemName.Clear();
            txtPrice.Clear();

            // Reset dropdowns to their first options
            if (cboRestaurant.Items.Count > 0) cboRestaurant.SelectedIndex = 0;
            if (cboAvailability.Items.Count > 0) cboAvailability.SelectedIndex = 0;

            txtItemName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            // 1. Validation
            if (string.IsNullOrWhiteSpace(txtItemName.Text) || string.IsNullOrWhiteSpace(txtPrice.Text))
            {
                MessageBox.Show("Please enter an Item Name and Price.", "Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Ensure the price is actually a valid number/decimal before trying to save it
            if (!decimal.TryParse(txtPrice.Text, out decimal priceValue))
            {
                MessageBox.Show("Please enter a valid numeric price.", "Invalid Input", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. The Upsert Logic
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "";
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    if (string.IsNullOrEmpty(txtItemId.Text))
                    {
                        // INSERT NEW
                        query = "INSERT INTO menu_items (restaurant_id, item_name, price, availability) VALUES (@restId, @name, @price, @avail)";
                    }
                    else
                    {
                        // UPDATE EXISTING
                        query = "UPDATE menu_items SET restaurant_id=@restId, item_name=@name, price=@price, availability=@avail WHERE item_id=@id";
                        cmd.Parameters.AddWithValue("@id", txtItemId.Text);
                    }

                    cmd.CommandText = query;

                    // Grab the hidden ID of the selected restaurant
                    cmd.Parameters.AddWithValue("@restId", cboRestaurant.SelectedValue);

                    cmd.Parameters.AddWithValue("@name", txtItemName.Text);
                    cmd.Parameters.AddWithValue("@price", priceValue);
                    cmd.Parameters.AddWithValue("@avail", cboAvailability.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("Menu item saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadMenuItems();
                    btnClear.PerformClick();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving item: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnDelete_Click_1(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtItemId.Text)) return;

            if (MessageBox.Show("Are you sure you want to delete this menu item?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM menu_items WHERE item_id = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtItemId.Text);
                            cmd.ExecuteNonQuery();
                            LoadMenuItems();
                            btnClear.PerformClick();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Error deleting: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error); }
                }
            }
        }

        private void cboFilterRestaurant_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterMenuItems();
        }
        private void FilterMenuItems()
        {
            // Stop the code from crashing if it tries to filter before the form finishes loading
            if (cboFilterRestaurant.SelectedValue == null || string.IsNullOrEmpty(cboFilterAvailability.Text)) return;

            string restId = cboFilterRestaurant.SelectedValue.ToString();
            string availStatus = cboFilterAvailability.Text;

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT m.item_id AS 'Item ID', 
                                    r.restaurant_name AS 'Restaurant', 
                                    m.item_name AS 'Item Name', 
                                    m.price AS 'Price', 
                                    m.availability AS 'Availability',
                                    m.restaurant_id AS 'HiddenRestID' 
                             FROM menu_items m
                             JOIN restaurants r ON m.restaurant_id = r.restaurant_id
                             WHERE 1=1"; // Base clause to make dynamic appending easy!

                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    // If they didn't pick "All Restaurants" (which we gave an ID of 0)
                    if (restId != "0")
                    {
                        query += " AND m.restaurant_id = @restId";
                        cmd.Parameters.AddWithValue("@restId", restId);
                    }

                    // If they didn't pick "All" availability
                    if (availStatus != "All")
                    {
                        query += " AND m.availability = @avail";
                        cmd.Parameters.AddWithValue("@avail", availStatus);
                    }

                    query += " ORDER BY m.item_id ASC";
                    cmd.CommandText = query;

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvMenuItems.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Filter Error: " + ex.Message); }
            }
        }

        private void cboFilterAvailability_SelectedIndexChanged(object sender, EventArgs e)
        {
            FilterMenuItems();
        }

        private void dgvMenuItems_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)

            {

                DataGridViewRow row = dgvMenuItems.Rows[e.RowIndex];



                txtItemId.Text = row.Cells["Item ID"].Value.ToString();

                txtItemName.Text = row.Cells["Item Name"].Value.ToString();



                // Clean the "₱" sign out of the price if it's there so we can edit the raw number

                txtPrice.Text = row.Cells["Price"].Value.ToString().Replace("₱", "");



                cboAvailability.Text = row.Cells["Availability"].Value.ToString();



                // The Pro Move: Use the hidden ID to select the exact restaurant in the dropdown!

                if (row.Cells["HiddenRestID"].Value != DBNull.Value)

                {

                    cboRestaurant.SelectedValue = row.Cells["HiddenRestID"].Value;

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

        private void button7_Click(object sender, EventArgs e)
        {
            frmRestaurants ordersWindow = new frmRestaurants();

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
