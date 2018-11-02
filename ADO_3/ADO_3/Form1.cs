using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace ADO_3
{
    public partial class Form1 : Form
    {
        private SqlConnection connection = null;
        private SqlDataAdapter dataAdapter = null;
        private DataSet set = null;
        private SqlCommandBuilder builder = null;
        public Form1()
        {
            InitializeComponent();
            pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
            button_Sell.Click += Button_Sell_Click;
            Show_Table();
        }

        private void Button_Sell_Click(object sender, EventArgs e)
        {
            Sell A = new Sell(set);
            if (A.ShowDialog() == DialogResult.OK)
            {
                Show_Table();
            }
        }

        private void Show_Table()
        {
            connection = new SqlConnection();
            connection.ConnectionString = ConfigurationManager.ConnectionStrings["MyConnString"].ConnectionString;
            //DB Request to fill in DGV Tables
            string Sql_command = @"SELECT * FROM Fridges;SELECT * FROM Buyers;SELECT
            Checks.DataSale AS'Продано',
            Buyers.FirstName + ' ' + Buyers.LastName AS 'Покупатель',
            Sellers.FirstName + ' ' + Sellers.LastName AS 'Продавец',
            Checks.Fridges AS 'Холодильники',
            Checks.TotalCount AS 'Количество',
            Checks.TotalCost AS 'Сумма'
            FROM Buyers INNER JOIN Checks ON Buyers.Id = Checks.IdBuyer
            INNER JOIN Sellers ON Checks.IdSeller = Sellers.Id; SELECT * FROM Sellers";
            set = new DataSet();
            dataAdapter = new SqlDataAdapter(Sql_command, connection);
            builder = new SqlCommandBuilder(dataAdapter);
            dataAdapter.Fill(set);
            dataGridView_Fridges.DataSource = null;
            dataGridView_Buyers.DataSource = null;
            dataGridView_Checks.DataSource = null;
            
            //Set Tables for DB Source Tables
            dataGridView_Fridges.DataSource = set.Tables[0];
            dataGridView_Buyers.DataSource = set.Tables[1];
            dataGridView_Checks.DataSource = set.Tables[2];
        }
    }
}
