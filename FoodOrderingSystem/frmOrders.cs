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
    public partial class frmOrders : Form
    {
        public frmOrders()
        {
            InitializeComponent();
        }

        private void frmOrders_Load(object sender, EventArgs e)
        {
            btnOrders.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            LoadAllOrders();
            LoadCustomerDropdown();
        }

        private void LoadAllOrders()
        {
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // We use JOINs so we see names, not just ID numbers!
                    string query = @"
                        SELECT o.order_id AS 'Order ID', c.full_name AS 'Customer', 
                               r.restaurant_name AS 'Restaurant', o.order_date AS 'Order Date', 
                               o.status AS 'Status', o.total_amount AS 'Total Amount'
                        FROM orders o
                        JOIN customers c ON o.customer_id = c.customer_id
                        JOIN restaurants r ON o.restaurant_id = r.restaurant_id
                        ORDER BY o.order_id ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvOrders.DataSource = dt;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading orders: " + ex.Message); }
            }
        }

        private void LoadCustomerDropdown()
        {
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "SELECT customer_id, full_name FROM customers";
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);

                        // This is a pro-trick: We show the Name, but hide the ID behind the scenes!
                        cboCustomer.DataSource = dt;
                        cboCustomer.DisplayMember = "full_name"; // What the user sees
                        cboCustomer.ValueMember = "customer_id"; // The hidden ID we need for the database
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading customers: " + ex.Message); }
            }
        }

        private void reporToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void frmOrders_Load_1(object sender, EventArgs e)
        {
            
            LoadAllOrders();
            LoadCustomerDropdown();
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void btnCallProc_Click(object sender, EventArgs e)
        {
            if (cboCustomer.SelectedValue == null) return;

            // Grab that hidden customer_id from the dropdown
            int selectedCustomerId = Convert.ToInt32(cboCustomer.SelectedValue);
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // Call your specific Stored Procedure!
                    using (MySqlCommand cmd = new MySqlCommand("GetCustomerOrders", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.Parameters.AddWithValue("cust_id", selectedCustomerId);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvOrders.DataSource = dt; // Update the grid with ONLY their orders
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error calling procedure: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            // 1. Reset the Customer dropdown so nothing is selected
            cboCustomer.SelectedIndex = -1;

            // 2. Reset the Status dropdown to the first option ("All Status")
            if (cboStatus.Items.Count > 0)
            {
                cboStatus.SelectedIndex = 0;
            }

            // 3. Reload the original, unfiltered grid!
            LoadAllOrders();
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            // If they picked "All Status" or left it blank, just load everything
            if (cboStatus.SelectedIndex <= 0 || cboStatus.Text == "")
            {
                LoadAllOrders();
                return;
            }

            // Otherwise, filter the database by the exact status word they chose
            string selectedStatus = cboStatus.Text;
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    // We use the exact same JOIN query, but add a WHERE clause at the end
                    string query = @"
                SELECT o.order_id AS 'Order ID', c.full_name AS 'Customer', 
                       r.restaurant_name AS 'Restaurant', o.order_date AS 'Order Date', 
                       o.status AS 'Status', o.total_amount AS 'Total Amount'
                FROM orders o
                JOIN customers c ON o.customer_id = c.customer_id
                JOIN restaurants r ON o.restaurant_id = r.restaurant_id
                WHERE o.status = @status
                ORDER BY o.order_id ASC";

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@status", selectedStatus);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvOrders.DataSource = dt; // Update the grid with filtered data
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error filtering status: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnOrders_Click(object sender, EventArgs e)
        {
            frmOrders ordersWindow = new frmOrders();

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
        
