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
    public partial class frmDashboard : Form
    {
        public frmDashboard()
        {
            InitializeComponent();
        }

        private void frmDashboard_Load(object sender, EventArgs e)
        {
            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;

          
            // 1. Your connection string (Don't forget to put your actual MySQL password here!)
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // --- 1. THE SUMMARY CARDS ---
                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders", conn))
                        lblTotalOrders.Text = cmd.ExecuteScalar().ToString();

                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE status = 'Completed'", conn))
                        lblCompleted.Text = cmd.ExecuteScalar().ToString();

                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE status = 'Pending'", conn))
                        lblPending.Text = cmd.ExecuteScalar().ToString();

                    using (MySqlCommand cmd = new MySqlCommand("SELECT COUNT(*) FROM orders WHERE status = 'Cancelled'", conn))
                        lblCancelled.Text = cmd.ExecuteScalar().ToString();

                    // --- 2. BEST SELLING ITEMS (This already works!) ---
                    string bestSellingQuery = "SELECT * FROM best_selling_items LIMIT 5";
                    using (MySqlDataAdapter adapterBestSellers = new MySqlDataAdapter(bestSellingQuery, conn))
                    {
                        DataTable dtBestSellers = new DataTable();
                        adapterBestSellers.Fill(dtBestSellers);
                        dgvBestSellers.DataSource = dtBestSellers;
                    }

                    // --- 3. RECENT ORDERS (Fixed to match your normalized DB) ---
                    string recentOrdersQuery = @"
                SELECT o.order_id AS '#', c.full_name AS 'Customer', o.total_amount AS 'Amount', o.status AS 'Status'
                FROM orders o
                JOIN customers c ON o.customer_id = c.customer_id
                ORDER BY o.order_id DESC LIMIT 5";

                    using (MySqlDataAdapter adapterRecent = new MySqlDataAdapter(recentOrdersQuery, conn))
                    {
                        DataTable dtRecent = new DataTable();
                        adapterRecent.Fill(dtRecent);
                        // Make sure your grid on the left is named exactly dgvRecentOrders in its Properties!
                        dgvRecentOrders.DataSource = dtRecent;
                    }

                    // --- 4. RESTAURANT SALES (Using your custom function) ---
                    string salesQuery = "SELECT restaurant_name AS 'Restaurant', GetRestaurantTotalSales(restaurant_id) AS 'Total Sales' FROM restaurants";

                    using (MySqlDataAdapter adapterSales = new MySqlDataAdapter(salesQuery, conn))
                    {
                        DataTable dtSales = new DataTable();
                        adapterSales.Fill(dtSales);
                        // Make sure your bottom grid is named exactly dgvSales in its Properties!
                        dgvSales.DataSource = dtSales;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading dashboard data: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
        }



        private void lblTotalOrders_Click(object sender, EventArgs e)
        {

        }

        private void lblTotalCustomers_Click(object sender, EventArgs e)
        {
               
        }

        private void button3_Click(object sender, EventArgs e)
        {
            frmReports reportsWindow = new frmReports();
            reportsWindow.ShowDialog();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            frmAbout aboutWindow = new frmAbout();
            aboutWindow.ShowDialog();
        }

        private void button5_Click(object sender, EventArgs e)
        {
            Application.Restart();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            frmMenuItems reportsWindow = new frmMenuItems();
            reportsWindow.ShowDialog();
        }

        private void statusStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        private void menuToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {

        }

        private void button6_Click(object sender, EventArgs e)
        {
            frmOrders ordersWindow = new frmOrders();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void fileToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void reporToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void button1_Click_1(object sender, EventArgs e)
        {
            frmNewOrderForm ordersWindow = new frmNewOrderForm();

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

        private void label8_Click(object sender, EventArgs e)
        {

        }

        private void panel4_Paint(object sender, PaintEventArgs e)
        {

        }

        private void button6_Click_1(object sender, EventArgs e)
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
    }
}
