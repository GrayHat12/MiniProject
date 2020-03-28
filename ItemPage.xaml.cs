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
    public partial class ItemPage : Window
    {
        private NpgsqlConnection connection;
        public ItemPage(NpgsqlConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(itemName.Text.Length>0 && itemCost.Text.Length>0 && itemSell.Text.Length > 0)
            {
                try
                {
                    int cost = Int32.Parse(itemCost.Text);
                    int sell = Int32.Parse(itemSell.Text);
                    string cmd = "INSERT INTO STOCK(stockname,stockquant,costprice,sellprice) VALUES('"+itemName.Text+"',0,"+cost+","+sell+");";
                    NpgsqlCommand command = new NpgsqlCommand(cmd, this.connection);
                    int o=command.ExecuteNonQuery();
                    if (o <= 0)
                    {
                        MessageBox.Show("Some Error Occured", "Error");
                    }
                }catch(Exception ex)
                {
                    MessageBox.Show("Error : " + ex.Message, "Error");
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
