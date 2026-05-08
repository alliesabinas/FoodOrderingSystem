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
    public partial class frmUserManagement : Form
    {
        public frmUserManagement()
        {
            InitializeComponent();
        }

        private void txtFullName_TextChanged(object sender, EventArgs e)
        {

        }

        private void frmUserManagement_Load(object sender, EventArgs e)
        {
            if (cboFilterRole.Items.Count > 0) cboFilterRole.SelectedIndex = 0;
            if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 1; // Default Staff
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0; // Default Active

            LoadUsers();
        }
        private void LoadUsers()
        {
            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    // We grab the password but keep it hidden in the grid so we can put it in the textbox when editing
                    string query = @"SELECT user_id AS 'HiddenID',
                                            CONCAT('USR-', LPAD(user_id, 3, '0')) AS 'User ID',
                                            full_name AS 'Full Name',
                                            email AS 'Email',
                                            password AS 'HiddenPassword', 
                                            role AS 'Role',
                                            status AS 'Status',
                                            DATE_FORMAT(created_at, '%Y-%m-%d') AS 'Created At'
                                     FROM users 
                                     ORDER BY user_id ASC";

                    using (MySqlDataAdapter adapter = new MySqlDataAdapter(query, conn))
                    {
                        DataTable dt = new DataTable();
                        adapter.Fill(dt);
                        dgvUsers.DataSource = dt;

                        if (dgvUsers.Columns["HiddenID"] != null) dgvUsers.Columns["HiddenID"].Visible = false;
                        if (dgvUsers.Columns["HiddenPassword"] != null) dgvUsers.Columns["HiddenPassword"].Visible = false;
                    }
                }
                catch (Exception ex) { MessageBox.Show("Error loading users: " + ex.Message); }
            }
        }

        private void dgvUsers_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {

        }

        private void dgvUsers_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0)
            {
                DataGridViewRow row = dgvUsers.Rows[e.RowIndex];

                txtUserId.Text = row.Cells["HiddenID"].Value.ToString();
                txtDisplayId.Text = row.Cells["User ID"].Value.ToString();
                txtFullName.Text = row.Cells["Full Name"].Value.ToString();
                txtEmail.Text = row.Cells["Email"].Value.ToString();
                txtPassword.Text = row.Cells["HiddenPassword"].Value.ToString();
                cboRole.Text = row.Cells["Role"].Value.ToString();
                cboStatus.Text = row.Cells["Status"].Value.ToString();
            }
        }

        private void btnClear_Click(object sender, EventArgs e)
        {
            txtUserId.Clear();
            txtDisplayId.Clear();
            txtFullName.Clear();
            txtEmail.Clear();
            txtPassword.Clear();
            if (cboRole.Items.Count > 0) cboRole.SelectedIndex = 1;
            if (cboStatus.Items.Count > 0) cboStatus.SelectedIndex = 0;
            txtFullName.Focus();
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFullName.Text) || string.IsNullOrWhiteSpace(txtEmail.Text) || string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                MessageBox.Show("Name, Email, and Password are required.", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    MySqlCommand cmd = new MySqlCommand();
                    cmd.Connection = conn;

                    if (string.IsNullOrEmpty(txtUserId.Text))
                    {
                        // INSERT NEW USER
                        cmd.CommandText = "INSERT INTO users (full_name, email, password, role, status) VALUES (@name, @email, @pass, @role, @status)";
                    }
                    else
                    {
                        // UPDATE EXISTING USER
                        cmd.CommandText = "UPDATE users SET full_name=@name, email=@email, password=@pass, role=@role, status=@status WHERE user_id=@id";
                        cmd.Parameters.AddWithValue("@id", txtUserId.Text);
                    }

                    cmd.Parameters.AddWithValue("@name", txtFullName.Text);
                    cmd.Parameters.AddWithValue("@email", txtEmail.Text);
                    cmd.Parameters.AddWithValue("@pass", txtPassword.Text);
                    cmd.Parameters.AddWithValue("@role", cboRole.Text);
                    cmd.Parameters.AddWithValue("@status", cboStatus.Text);

                    cmd.ExecuteNonQuery();
                    MessageBox.Show("User saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    LoadUsers();
                    btnClear.PerformClick();
                }
                catch (Exception ex) { MessageBox.Show("Error saving user: " + ex.Message); }
            }
        }

        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtUserId.Text)) return;

            if (MessageBox.Show("Delete this user permanently?", "Confirm", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                using (MySqlConnection conn = DatabaseHelper.GetConnection())
                {
                    try
                    {
                        conn.Open();
                        using (MySqlCommand cmd = new MySqlCommand("DELETE FROM users WHERE user_id = @id", conn))
                        {
                            cmd.Parameters.AddWithValue("@id", txtUserId.Text);
                            cmd.ExecuteNonQuery();
                            LoadUsers();
                            btnClear.PerformClick();
                        }
                    }
                    catch (Exception ex) { MessageBox.Show("Error deleting: " + ex.Message); }
                }
            }
        }

        private void SearchUsers()
        {
            if (cboFilterRole.SelectedItem == null) return;

            string keyword = txtSearch.Text.Trim();
            string roleFilter = cboFilterRole.Text;

            using (MySqlConnection conn = DatabaseHelper.GetConnection())
            {
                try
                {
                    conn.Open();
                    string query = @"SELECT user_id AS 'HiddenID',
                                            CONCAT('USR-', LPAD(user_id, 3, '0')) AS 'User ID',
                                            full_name AS 'Full Name',
                                            email AS 'Email',
                                            password AS 'HiddenPassword',
                                            role AS 'Role',
                                            status AS 'Status',
                                            DATE_FORMAT(created_at, '%Y-%m-%d') AS 'Created At'
                                     FROM users 
                                     WHERE (full_name LIKE @keyword OR email LIKE @keyword) ";

                    if (roleFilter != "All Roles")
                    {
                        query += " AND role = @role";
                    }

                    using (MySqlCommand cmd = new MySqlCommand(query, conn))
                    {
                        cmd.Parameters.AddWithValue("@keyword", "%" + keyword + "%");
                        if (roleFilter != "All Roles") cmd.Parameters.AddWithValue("@role", roleFilter);

                        using (MySqlDataAdapter adapter = new MySqlDataAdapter(cmd))
                        {
                            DataTable dt = new DataTable();
                            adapter.Fill(dt);
                            dgvUsers.DataSource = dt;
                        }
                    }
                }
                catch (Exception ex) { MessageBox.Show("Search Error: " + ex.Message); }
            }
        }
        private void txtSearch_TextChanged(object sender, EventArgs e) { SearchUsers(); }
        private void cboFilterRole_SelectedIndexChanged(object sender, EventArgs e) { SearchUsers(); }

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

        private void btnOrders_Click(object sender, EventArgs e)
        {
            frmOrders ordersWindow = new frmOrders();

            // 2. Show the Orders form on the screen
            ordersWindow.Show();

            // 3. Hide the current Dashboard so they don't overlap
            this.Hide();
        }

        private void button1_Click_1(object sender, EventArgs e)
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
    }
}
