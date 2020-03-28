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
    public partial class CustomerPage : Window
    {
        private NpgsqlConnection connection;
        public CustomerPage(NpgsqlConnection connection)
        {
            InitializeComponent();
            this.connection = connection;
        }
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if (itemName.Text.Length > 0 && itemAddr.Text.Length>0)
            {
                try
                {
                    string cmd = "INSERT INTO CUSTOMER(custname,custaddr) VALUES('" + itemName.Text + "','" + itemAddr.Text + "');";
                    NpgsqlCommand command = new NpgsqlCommand(cmd, this.connection);
                    int i = command.ExecuteNonQuery();
                    if (i <= 0)
                    {
                        MessageBox.Show("Some error occured", "Error");
                        return;
                    }
                    command.Dispose();
                    string ncmd = "SELECT custid FROM CUSTOMER WHERE custname='" + itemName.Text + "' AND custaddr='" + itemAddr.Text + "';";
                    command = new NpgsqlCommand(ncmd,this.connection);
                    string id = ""+command.ExecuteScalar();
                    command.Dispose();
                    MessageBox.Show("Added CUSTOMER\nId="+id, "INFO");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Invalid Input " + ex.Message, "Error");
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
