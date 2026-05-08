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
using System.Data;
using System.IO;

namespace FoodOrderingSystem
{
    public partial class frmReports : Form
    {

       string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";
        public frmReports()
        {
            InitializeComponent();
        }

        private void frmReports_Load(object sender, EventArgs e)
        {
            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;

            // 1. Set default dates (From the 1st of the current month, to Today)
            dtpFromDate.Value = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            dtpToDate.Value = DateTime.Now;

            // 2. Setup the Dropdown
            cboReportType.Items.Clear();
            cboReportType.Items.Add("Customer Order Report");
            // cboReportType.Items.Add("Restaurant Sales Report"); // We can wire this one up later!
            cboReportType.SelectedIndex = 0;
        }

        private void btnLoadReport_Click(object sender, EventArgs e)
        {
            // 1. Check if the user actually selected a report from the drop-down
            if (cboReportType.SelectedItem == null)
            {
                MessageBox.Show("Please select a report type first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string selectedReport = cboReportType.SelectedItem.ToString();
            string query = "";

            // 2. Decide which MySQL View to look at based on the drop-down choice
            if (selectedReport == "Customer Order Report")
            {
                query = "SELECT * FROM customer_order_report";
            }
            else if (selectedReport == "Restaurant Sales Report")
            {
                query = "SELECT * FROM restaurant_sales_report";
            }
            else if (selectedReport == "Best Selling Items")
            {
                query = "SELECT * FROM best_selling_items";
            }

            // 3. Connect to the database and grab the data
           string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";

            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // We use a DataAdapter here because it is the fastest way to pour SQL data into a C# Grid
                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable); // Fills our temporary table with the MySQL View data

                        // Attach the data to the UI grid
                        dgvReport.DataSource = dataTable;

                        // Optional: Make the grid look a bit nicer by auto-sizing the columns
                        dgvReport.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading report: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            // 1. Check if there is actually data to export
            if (dgvReport.Rows.Count == 0 || dgvReport.DataSource == null)
            {
                MessageBox.Show("There is no data to export. Please load a report first.", "Notice", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Open a Save Dialog so the user can choose where to save the file
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV File|*.csv";
            saveDialog.Title = "Save Report as Excel CSV";
            saveDialog.FileName = "FoodOrderingReport.csv";

            if (saveDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(saveDialog.FileName))
                    {
                        // 3. Write the Column Headers
                        for (int i = 0; i < dgvReport.Columns.Count; i++)
                        {
                            sw.Write(dgvReport.Columns[i].HeaderText);
                            if (i < dgvReport.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine(); // Move to the next line

                        // 4. Write the Rows of Data
                        foreach (DataGridViewRow row in dgvReport.Rows)
                        {
                            if (!row.IsNewRow) // Skip the empty row at the very bottom
                            {
                                for (int i = 0; i < dgvReport.Columns.Count; i++)
                                {
                                    sw.Write(row.Cells[i].Value?.ToString().Replace(",", "")); // Replace commas in data to prevent shifting
                                    if (i < dgvReport.Columns.Count - 1) sw.Write(",");
                                }
                                sw.WriteLine();
                            }
                        }
                    }

                    MessageBox.Show("Report successfully exported!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error saving file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dtpFromDate_ValueChanged(object sender, EventArgs e)
        {

        }

        private void button1_Click(object sender, EventArgs e)
        {
            LoadReport();

        }

        private void LoadReport()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();
                    string query = "";

                    // Check which report the user wants to see
                    if (cboReportType.Text == "Customer Order Report")
                    {
                        // Query the MySQL VIEW you already built!
                        // DATE_FORMAT cleans up the timestamp to look like your mockup
                        query = @"SELECT full_name AS 'Full Name',
                                         order_id AS 'Order ID',
                                         DATE_FORMAT(order_date, '%Y-%m-%d %H:%i') AS 'Order Date',
                                         status AS 'Status',
                                         total_amount AS 'Total Amount'
                                  FROM customer_order_report
                                  WHERE DATE(order_date) >= @fromDate AND DATE(order_date) <= @toDate
                                  ORDER BY order_date DESC";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        // Use .Date to ignore the specific time of day and just search by calendar day
                        cmd.Parameters.AddWithValue("@fromDate", dtpFromDate.Value.Date);
                        cmd.Parameters.AddWithValue("@toDate", dtpToDate.Value.Date);

                        using (MySqlDataAdapter da = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            da.Fill(dt);
                            dgvReport.DataSource = dt;

                            // Call a helper method to calculate the totals at the bottom!
                            UpdateTotals(dt);
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error generating report: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void UpdateTotals(DataTable dt)
        {
            int recordCount = dt.Rows.Count;
            decimal grandTotal = 0;

            // Loop through the data we just downloaded to add up the money
            foreach (DataRow row in dt.Rows)
            {
                if (row["Total Amount"] != DBNull.Value)
                {
                    grandTotal += Convert.ToDecimal(row["Total Amount"]);
                }
            }

            // Update your UI labels
            lblRecordCount.Text = $"Showing {recordCount} records";
            lblGrandTotal.Text = "Grand Total: ₱" + grandTotal.ToString("0.00");
        }

        private void btnExportExcel_Click_1(object sender, EventArgs e)
        {
            // 1. Check if the grid is empty before trying to export
            if (dgvReport.Rows.Count == 0 || (dgvReport.Rows.Count == 1 && dgvReport.Rows[0].IsNewRow))
            {
                MessageBox.Show("There is no data to export.", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 2. Open a "Save As" window so the user can choose where to save the file
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "Excel CSV File|*.csv";
            sfd.FileName = "CustomerOrderReport_" + DateTime.Now.ToString("yyyyMMdd") + ".csv";

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // 3. Create the file!
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        // STEP A: Write the Column Headers at the top of the Excel file
                        for (int i = 0; i < dgvReport.Columns.Count; i++)
                        {
                            sw.Write(dgvReport.Columns[i].HeaderText);
                            if (i < dgvReport.Columns.Count - 1) sw.Write(",");
                        }
                        sw.WriteLine(); // Move to the next row

                        // STEP B: Loop through the grid and write every single row of data
                        foreach (DataGridViewRow row in dgvReport.Rows)
                        {
                            if (!row.IsNewRow) // Skip the empty blank row at the bottom
                            {
                                for (int i = 0; i < dgvReport.Columns.Count; i++)
                                {
                                    string cellValue = row.Cells[i].Value?.ToString() ?? "";

                                    // Pro-trick: Wrap data in quotes just in case a name has a comma in it!
                                    sw.Write("\"" + cellValue.Replace("\"", "\"\"") + "\"");

                                    if (i < dgvReport.Columns.Count - 1) sw.Write(",");
                                }
                                sw.WriteLine(); // Move to the next row
                            }
                        }
                    }

                    MessageBox.Show("Report exported successfully!", "Export Complete", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error exporting data. Make sure the file isn't already open in Excel. Error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void dgvReport_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void txtSearch_TextChanged(object sender, EventArgs e)
        {
            // Make sure there is actually data in the grid to filter
            if (dgvReport.DataSource is DataTable dt)
            {
                // Grab what the user typed and clean it up so it doesn't break SQL syntax
                string keyword = txtSearch.Text.Trim().Replace("'", "''");

                // Instantly filter the existing downloaded data!
                // We are searching inside the Full Name OR the Status columns
                dt.DefaultView.RowFilter = $"[Full Name] LIKE '%{keyword}%' OR [Status] LIKE '%{keyword}%'";

                // Update the Totals at the bottom so they only calculate the visible filtered rows!
                UpdateTotals(dt.DefaultView.ToTable());
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

        private void button2_Click(object sender, EventArgs e)
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

        private void button11_Click(object sender, EventArgs e)
        {
            frmAbout ordersWindow = new frmAbout();

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

        private void button3_Click(object sender, EventArgs e)
        {
            frmUserManagement ordersWindow = new frmUserManagement();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }
    }
    
}
