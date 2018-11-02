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

namespace ADO_3
{
    public partial class Sell : Form
    {
        private DataSet set = null;
        private int summ = 0;
        public Sell(DataSet dataSet)
        {
            InitializeComponent();
            set = dataSet;
            checkedListBox_Fridges.SelectedIndexChanged += CheckedListBox_Fridges_SelectedIndexChanged;
            button_Calculate.Click += Button_Calculate_Click;
            comboBox_Seller.SelectedIndexChanged += ComboBox_Seller_SelectedIndexChanged;
            button_Sell.Click += Button_Sell_Click;
            button_Sell.Visible = false;
           Show_Check();
        }

        private void Button_Sell_Click(object sender, EventArgs e)
        {
            //If Consultant NOT Choosen
            if (this.comboBox_Seller.SelectedIndex == -1)
            {
                MessageBox.Show("Choose consultant", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //If Buyer NOT Choosen
            if (comboBox_Buyer.SelectedIndex == -1)
            {
                MessageBox.Show("Choose buyer", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            //If Buyer Have not enough money
            if (Convert.ToInt32(label_BuyerVallet.Text) < 0)
            {
                MessageBox.Show("Sorry! Buyer have little money", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            //Begin Transaction
            SqlConnection connection = new SqlConnection(@"Data Source = KEFIR4IK; Initial Catalog=Market;Integrated Security=true;");
            SqlCommand command = connection.CreateCommand();
            SqlTransaction trans = null;
            try
            {
                connection.Open();
                trans = connection.BeginTransaction();
                command.Transaction = trans;
                DataRow row = set.Tables[2].NewRow();
                row[1] = DateTime.Today.ToString("d");
                for (int i = 0; i < set.Tables[0].Rows.Count; i++)
                {
                    foreach (var SelectedFridge in panel_Quantity.Controls)
                    {
                        if (SelectedFridge is ComboBox)
                        {
                            if (set.Tables[0].Rows[i][1].Equals((SelectedFridge as ComboBox).Name))
                            {
                                command.CommandText =
                                    $"UPDATE Checks SET TotalCount={(Convert.ToInt32(set.Tables[0].Rows[i][2]) - Convert.ToInt32((SelectedFridge as ComboBox).SelectedItem))} WHERE Id={set.Tables[0].Rows[i][0]}";
                                command.ExecuteNonQuery();
                            }
                        }
                    }
                }

                //Working with Buyers Table
                int count = 0;
                int idBuyer = 0;
                for (int i = 0; i < set.Tables[1].Rows.Count; i++)
                {
                    if ($"{set.Tables[1].Rows[i][1]} {set.Tables[1].Rows[i][2]}".Equals(comboBox_Buyer.SelectedItem))
                    {
                        idBuyer = Convert.ToInt32(set.Tables[1].Rows[i][0]);
                        count = 0;
                        foreach (var VARIABLE in panel_Quantity.Controls)
                        {
                            if (VARIABLE is ComboBox)
                            {
                                count += Convert.ToInt32((VARIABLE as ComboBox).SelectedItem);
                            }
                        }
                        command.CommandText =
                            $"UPDATE Buyers SET Purchases={(Convert.ToInt32(set.Tables[1].Rows[i][4]) + count)} WHERE Id={set.Tables[1].Rows[i][0]}";
                        command.ExecuteNonQuery();
                        command.CommandText =
                            $"UPDATE Buyers SET Account={Convert.ToDecimal(label_BuyerVallet.Text)} WHERE Id={set.Tables[1].Rows[i][0]}";
                        command.ExecuteNonQuery();
                    }
                }

                //Working with Sellers Table
                int idSeller = 0;
                for (int i = 0; i < set.Tables[3].Rows.Count; i++)
                {
                    if ($"{set.Tables[3].Rows[i][1]} {set.Tables[3].Rows[i][2]}".Equals(
                        comboBox_Seller.SelectedItem))
                    {
                        idSeller = Convert.ToInt32(set.Tables[3].Rows[i][0]);
                    }
                }

                string[] Fridges = new string[set.Tables[0].Rows.Count];
                int x = 0;
                foreach (var VARIABLE in panel_Quantity.Controls)
                {
                    if (VARIABLE is Label)
                    {
                        if ((VARIABLE as Label).Visible)
                        {
                            Fridges[x++] = (VARIABLE as Label).Text;
                        }
                    }
                }

                string Buyed_Fridges = String.Join(",", Fridges);
                int Count = count;
                decimal TotalSumm = Convert.ToDecimal(label_TotalSumm.Text);
                command.CommandText =
                    $"insert into Checks values(('{DateTime.Now.Date}'), ({idBuyer}), ({idSeller}), ('{Buyed_Fridges}'), ({Count}), ({TotalSumm}));";
                command.ExecuteNonQuery();
                trans.Commit();
                DialogResult = DialogResult.OK;

            }
            catch (Exception)
            {
                MessageBox.Show("Sorry! Something goes wrong", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                trans.Rollback();
                connection?.Close();
            }
        }

        private int Vallet;
        private void ComboBox_Seller_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_Sell.Visible = false;
            Vallet = 0;
            for (int i = 0; i < set.Tables[1].Rows.Count; i++)
            {
                if ($"{set.Tables[1].Rows[i][1]} {set.Tables[1].Rows[i][2]}".Equals(comboBox_Buyer.SelectedItem))
                {
                    Vallet = Convert.ToInt32(set.Tables[1].Rows[i][3]);
                }
            }
            label_BuyerVallet.Text = Vallet.ToString();
        }

        private void Button_Calculate_Click(object sender, EventArgs e)
        {
            if (comboBox_Buyer.SelectedIndex == -1)
            {
                MessageBox.Show("Choose Buyer!", "Information", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            summ = 0;
            for (int i = 0; i < set.Tables[0].Rows.Count; i++)
            {
                foreach (var VARIABLE in panel_Quantity.Controls)
                {
                    if (VARIABLE is ComboBox)
                    {
                        if (set.Tables[0].Rows[i][1].Equals((VARIABLE as ComboBox).Name))
                        {
                            summ += (Convert.ToInt32(set.Tables[0].Rows[i][3]) * 
                                     Convert.ToInt32((VARIABLE as ComboBox).SelectedItem));
                        }
                    }
                }
            }
            label_TotalSumm.Text = summ.ToString();
            Vallet -= summ;
            label_BuyerVallet.Text = Vallet.ToString();
            button_Sell.Visible = true;
        }

        private void CheckedListBox_Fridges_SelectedIndexChanged(object sender, EventArgs e)
        {
            for (int i = 0; i < checkedListBox_Fridges.Items.Count; i++)
            {
                if (checkedListBox_Fridges.GetItemChecked(i))
                {
                    foreach (var VARIABLE in panel_Quantity.Controls)
                    {
                        if (VARIABLE is Label)
                        {
                            if (checkedListBox_Fridges.Items[i].Equals((VARIABLE as Label).Text))
                            {
                                (VARIABLE as Label).Visible = true;
                            }
                        }
                        if (VARIABLE is ComboBox)
                        {
                            if (checkedListBox_Fridges.Items[i].Equals((VARIABLE as ComboBox).Name))
                            {
                                (VARIABLE as ComboBox).Visible = true;
                            }
                        }
                    }
                }
                else if (!checkedListBox_Fridges.GetItemChecked(i))
                {
                    foreach (var VARIABLE in panel_Quantity.Controls)
                    {
                        if (VARIABLE is Label)
                        {
                            if (checkedListBox_Fridges.Items[i].Equals((VARIABLE as Label).Text))
                            {
                                (VARIABLE as Label).Visible = false;
                            }
                        }
                        if (VARIABLE is ComboBox)
                        {
                            if (checkedListBox_Fridges.Items[i].Equals((VARIABLE as ComboBox).Name))
                            {
                                (VARIABLE as ComboBox).Visible = false;
                                (VARIABLE as ComboBox).SelectedIndex = 0;
                            }
                        }
                    }
                }
            }
        }

        private void Show_Check()
        {
            int x = 4, y = 5, z = 123;
            for (int i = 0; i < set.Tables[3].Rows.Count; i++)
            {
                comboBox_Seller.Items.Add($"{set.Tables[3].Rows[i][1]} {set.Tables[3].Rows[i][2]}");
            }

            for (int i = 0; i < set.Tables[0].Rows.Count; i++)
            {
                checkedListBox_Fridges.Items.Add($"{set.Tables[0].Rows[i][1]}");
                ComboBox box = new ComboBox();
                Label label = new Label();
                label.ForeColor = Color.Black;
                label.Text = set.Tables[0].Rows[i][1].ToString();
                label.Visible = false;
                label.Location = new Point(x, y);
                label.AutoSize = true;
                box.ForeColor = Color.Black;
                box.BackColor = Color.White;
                box.Name = (set.Tables[0].Rows[i][1]).ToString();
                box.Width = 40;
                box.Location = new Point(z, y);
                box.SelectedIndexChanged += Box_SelectedIndexChanged;
                y += 21;
                for (int j = 0; j <= Convert.ToInt32(set.Tables[0].Rows[i][2]); j++)
                {
                    box.Items.Add((j).ToString());
                }
                box.SelectedIndex = 0;
                box.Visible = false;
                panel_Quantity.Controls.Add(label);
                panel_Quantity.Controls.Add(box);
            }

            for (int i = 0; i < set.Tables[1].Rows.Count; i++)
            {
                comboBox_Buyer.Items.Add($"{set.Tables[1].Rows[i][1]} {set.Tables[1].Rows[i][2]}");
            }
        }

        private void Box_SelectedIndexChanged(object sender, EventArgs e)
        {
            button_Sell.Visible = false;
            Vallet = 0;
            for (int i = 0; i < set.Tables[1].Rows.Count; i++)
            {
                if ($"{set.Tables[1].Rows[i][1]} {set.Tables[1].Rows[i][2]}".Equals(comboBox_Buyer.SelectedItem))
                {
                    Vallet = Convert.ToInt32(set.Tables[1].Rows[i][3]);
                }
            }
            label_BuyerVallet.Text = Vallet.ToString();
        }
    }
}
