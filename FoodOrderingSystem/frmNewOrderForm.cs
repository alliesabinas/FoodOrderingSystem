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
    public partial class frmNewOrderForm : Form
    {
        string connectionString = "Server=localhost;Database=online_food_ordering_db;Uid=root;Pwd=;";   
        public frmNewOrderForm()
        {
            InitializeComponent();
        }

        private void frmNewOrderForm_Load(object sender, EventArgs e)
        {

            btnDashboard.BackColor = System.Drawing.Color.FromArgb(0, 122, 204);
            btnDashboard.ForeColor = System.Drawing.Color.White;

            LoadDropdowns();

            // Set the Status to Pending by default
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;
        }
        private void LoadDropdowns()
        {
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                try
                {
                    conn.Open();

                    // 1. Load Customers
                    string custQuery = "SELECT customer_id, full_name FROM customers";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(custQuery, conn))
                    {
                        DataTable dtCust = new DataTable();
                        da.Fill(dtCust);
                        cboCustomer.DataSource = dtCust;
                        cboCustomer.DisplayMember = "full_name";
                        cboCustomer.ValueMember = "customer_id";
                    }

                    // 2. Load Restaurants
                    string restQuery = "SELECT restaurant_id, restaurant_name FROM restaurants";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(restQuery, conn))
                    {
                        DataTable dtRest = new DataTable();
                        da.Fill(dtRest);
                        cboRestaurant.DataSource = dtRest;
                        cboRestaurant.DisplayMember = "restaurant_name";
                        cboRestaurant.ValueMember = "restaurant_id";
                    }

                    // 3. Load Menu Items (We include price so we can auto-fill it later!)
                    string menuQuery = "SELECT item_id, item_name, price FROM menu_items WHERE availability = 'Available'";
                    using (MySqlDataAdapter da = new MySqlDataAdapter(menuQuery, conn))
                    {
                        DataTable dtMenu = new DataTable();
                        da.Fill(dtMenu);
                        cboMenuItem.DataSource = dtMenu;
                        cboMenuItem.DisplayMember = "item_name";
                        cboMenuItem.ValueMember = "item_id";
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading dropdowns: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void cboMenuItem_SelectedIndexChanged(object sender, EventArgs e)
        {
            // Make sure a real item is selected, not just the empty setup phase
            if (cboMenuItem.SelectedItem != null && cboMenuItem.DataSource != null)
            {
                // Grab the hidden "price" column we downloaded during Form_Load
                DataRowView selectedRow = (DataRowView)cboMenuItem.SelectedItem;
                string price = selectedRow["price"].ToString();

                // Put it in the textbox!
                txtUnitPrice.Text = price;

                // Auto-compute the temporary subtotal based on the current quantity
                ComputeTempSubtotal();
            }

        }
        private void ComputeTempSubtotal()
        {
            if (decimal.TryParse(txtUnitPrice.Text, out decimal price) &&
                int.TryParse(txtQuantity.Text, out int qty))
            {
                txtSubtotal.Text = (price * qty).ToString("0.00");
            }
        }

        private void groupBox1_Enter(object sender, EventArgs e)
        {

        }

        private void cboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        private void txtQuantity_TextChanged(object sender, EventArgs e)
        {
            ComputeTempSubtotal();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            // 1. Validation: Did they actually pick a menu item?
            if (cboMenuItem.SelectedIndex == -1 || string.IsNullOrEmpty(txtSubtotal.Text))
            {
                MessageBox.Show("Please select a menu item and ensure a valid quantity.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // 2. Grab the current values from the textboxes
            string itemId = cboMenuItem.SelectedValue.ToString();
            string itemName = cboMenuItem.Text;
            string unitPrice = txtUnitPrice.Text;
            string quantity = txtQuantity.Text;
            string subtotal = txtSubtotal.Text;

            // 3. PRO FEATURE: The Duplicate Checker
            // Loop through the grid to see if this item is already in the list
            foreach (DataGridViewRow row in dgvOrderItems.Rows)
            {
                // If we find a match using the hidden Item ID...
                if (row.Cells["colItemId"].Value != null && row.Cells["colItemId"].Value.ToString() == itemId)
                {
                    // Just do the math to update the existing row!
                    int existingQty = Convert.ToInt32(row.Cells["colQuantity"].Value);
                    int newQty = existingQty + Convert.ToInt32(quantity);
                    decimal price = Convert.ToDecimal(unitPrice);

                    row.Cells["colQuantity"].Value = newQty;
                    row.Cells["colSubtotal"].Value = (newQty * price).ToString("0.00");

                    // Update the grand total, reset the inputs, and STOP the method here.
                    UpdateGrandTotal();
                    ResetItemInputs();
                    return;
                }
            }

            // 4. If the code makes it here, it means it's a brand new item! Add the row.
            // NOTE: These match the exact column names we set up in the Smart Tag earlier
            dgvOrderItems.Rows.Add(itemId, itemName, unitPrice, quantity, subtotal);

            // 5. Update the grand total and clean up the form for the next item
            UpdateGrandTotal();
            ResetItemInputs();
        }
        // Helper 1: Recalculates the bottom Total label by adding up everything in the grid
        private void UpdateGrandTotal()
        {
            decimal grandTotal = 0;

            foreach (DataGridViewRow row in dgvOrderItems.Rows)
            {
                // Make sure the row isn't empty before trying to do math
                if (row.Cells["colSubtotal"].Value != null)
                {
                    grandTotal += Convert.ToDecimal(row.Cells["colSubtotal"].Value);
                }
            }

            // Update your label at the bottom of the screen
            lblGrandTotal.Text = "₱" + grandTotal.ToString("0.00");
        }

        // Helper 2: Clears the Add Item boxes so the user can quickly add the next dish
        private void ResetItemInputs()
        {
            cboMenuItem.SelectedIndex = -1; // Unselect the dropdown
            txtUnitPrice.Clear();
            txtQuantity.Text = "1";         // Reset quantity back to default
            txtSubtotal.Clear();

            // Put the blinking cursor back on the dropdown to speed up data entry!
            cboMenuItem.Focus();
        }

        private void ResetEntireForm()
        {
            // 1. Wipe out all the food items in the grid
            dgvOrderItems.Rows.Clear();

            // 2. Unselect the Customer and Restaurant dropdowns
            cboCustomer.SelectedIndex = -1;
            cboRestaurant.SelectedIndex = -1;

            // 3. Reset the status back to "Pending"
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;

            // 4. Reset the Grand Total back to zero
            lblGrandTotal.Text = "₱0.00";

            // 5. Run your existing helper method to clear the Add Item textboxes
            ResetItemInputs();
        }

        private void button2_Click_1(object sender, EventArgs e)
        {
            this.Close();
        }

        private void btnSaveOrder_Click(object sender, EventArgs e)
        {
            // --- 1. BASIC VALIDATION ---
            if (cboCustomer.SelectedIndex == -1 || cboRestaurant.SelectedIndex == -1)
            {
                MessageBox.Show("Please select both a Customer and a Restaurant.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Check if the grid has actual items (ignoring the empty placeholder row at the very bottom)
            if (dgvOrderItems.Rows.Count == 0 || (dgvOrderItems.Rows.Count == 1 && dgvOrderItems.Rows[0].IsNewRow))
            {
                MessageBox.Show("Please add at least one item to the order.", "Validation Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // Clean up the Grand Total text (remove the "₱" symbol so MySQL can read it as a decimal)
            string totalText = lblGrandTotal.Text.Replace("₱", "").Trim();
            decimal finalTotal = Convert.ToDecimal(totalText);

            // --- 2. THE DATABASE TRANSACTION ---
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                // Start the transaction guarantee!
                using (MySqlTransaction transaction = conn.BeginTransaction())
                {
                    try
                    {
                        // STEP A: Create the Main Order Record
                        string orderQuery = @"INSERT INTO orders (customer_id, restaurant_id, order_date, status, total_amount) 
                                      VALUES (@custId, @restId, NOW(), @status, @total)";

                        long newOrderId = 0; // We need to store the newly created ID!

                        using (MySqlCommand cmd = new MySqlCommand(orderQuery, conn, transaction))
                        {
                            cmd.Parameters.AddWithValue("@custId", cboCustomer.SelectedValue);
                            cmd.Parameters.AddWithValue("@restId", cboRestaurant.SelectedValue);
                            cmd.Parameters.AddWithValue("@status", cboStatus.Text);
                            cmd.Parameters.AddWithValue("@total", finalTotal);

                            cmd.ExecuteNonQuery();

                            // MAGIC TRICK: Grab the auto-increment ID that MySQL just generated for this order
                            newOrderId = cmd.LastInsertedId;
                        }

                        // STEP B: Loop through the DataGrid and save every food item
                        string detailsQuery = @"INSERT INTO order_details (order_id, item_id, quantity, subtotal) 
                                        VALUES (@orderId, @itemId, @qty, @subtotal)";

                        using (MySqlCommand cmdDetails = new MySqlCommand(detailsQuery, conn, transaction))
                        {
                            foreach (DataGridViewRow row in dgvOrderItems.Rows)
                            {
                                // Skip the empty blank row at the bottom of the grid
                                if (row.Cells["colItemId"].Value != null)
                                {
                                    cmdDetails.Parameters.Clear(); // Clear the previous row's data

                                    // Link this food item to the specific Order ID we just generated above
                                    cmdDetails.Parameters.AddWithValue("@orderId", newOrderId);

                                    cmdDetails.Parameters.AddWithValue("@itemId", row.Cells["colItemId"].Value);
                                    cmdDetails.Parameters.AddWithValue("@qty", row.Cells["colQuantity"].Value);

                                    // FIXED: Grab the subtotal from the grid instead of the unit price
                                    cmdDetails.Parameters.AddWithValue("@subtotal", row.Cells["colSubtotal"].Value);

                                    cmdDetails.ExecuteNonQuery();
                                }
                            }
                        }

                        // STEP C: If we made it this far without crashing, COMMIT the save permanently!
                        transaction.Commit();

                        MessageBox.Show("Order saved successfully! Order ID: " + newOrderId, "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        ResetEntireForm();
                    }
                    catch (Exception ex)
                    {
                        // STEP D: If literally anything fails, ROLLBACK (delete) the whole order 
                        // so we don't corrupt your database!
                        transaction.Rollback();
                        MessageBox.Show("Error saving order: " + ex.Message, "Database Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
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

        }

        private void btnDashboard_Click(object sender, EventArgs e)
        {

            frmDashboard ordersWindow = new frmDashboard();

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

        private void button2_Click_2(object sender, EventArgs e)
        {
            frmUserManagement ordersWindow = new frmUserManagement();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }
    }
}

