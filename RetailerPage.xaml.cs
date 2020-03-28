using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Npgsql;

namespace MiniProject
{
    public partial class RetailerPage : Window
    {
        private NpgsqlConnection connection;
        private NpgsqlCommand command;
        private List<Item> items;
        public RetailerPage(NpgsqlConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
            this.command = new NpgsqlCommand("", this.connection);
            this.items = new List<Item>();
            this.GetStockData();
        }
        private void GetStockData()// this is currently unnecessary but can be beneficial if increasing features
        {
            this.items.Clear();
            string comnd = "SELECT * FROM STOCK;";
            this.command.CommandText = comnd;
            NpgsqlDataReader npgsqlDataReader = this.command.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                Item item = new Item((string)npgsqlDataReader[1], (int)npgsqlDataReader[0], (int)npgsqlDataReader[2], (int)npgsqlDataReader[3], (int)npgsqlDataReader[4]);
                this.items.Add(item);
            }
            npgsqlDataReader.Close();
            this.AddItems();
        }

        private void AddItems()
        {
            combo.Items.Clear();
            foreach(Item item in this.items)
            {
                combo.Items.Add(item.id + ":" + item.name);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (itemName.Text.Length > 0 && itemAddr.Text.Length > 0 && combo.SelectedItem.ToString().Length>0)
            {
                try
                {
                    string id = combo.SelectedItem.ToString();
                    id = id.Split(':')[0];
                    string cmd = "INSERT INTO RETAILERS(buyername,buyeraddr,buystockid) VALUES('" + itemName.Text + "','" + itemAddr.Text + "'," + id + ");";
                    this.command.CommandText = cmd;
                    int i=this.command.ExecuteNonQuery();
                    if (i <= 0)
                    {
                        MessageBox.Show("Some error occured", "Error");
                        return;
                    }
                    MessageBox.Show("Added Retailer", "INFO");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid Input "+ex.Message, "Error");
                    return;
                }
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            this.connection.Close();
            this.Close();
            mw.ShowDialog();
        }
    }
}
