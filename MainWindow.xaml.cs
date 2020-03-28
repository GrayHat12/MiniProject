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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Npgsql;

namespace MiniProject
{

    public partial class MainWindow : Window
    {
        private string customerid;
        private string customername;
        private string managerid;
        private string managerpass;
        private string credent;
        private NpgsqlConnection connection;

        public MainWindow()
        {
            InitializeComponent();
            this.customerid = "";
            this.customername = "";
            this.managerid = "";
            this.managerpass = "";
            this.credent = "Host=localhost;Database=miniproject;Username=postgres;Password=root;";
            this.connection = new NpgsqlConnection(this.credent);
            this.connection.Open();
        }

        private void CustomerID_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.customerid = ((PasswordBox)sender).Password;
        }

        private void CustId_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.customername = ((TextBox)sender).Text;
        }

        private void CustLogin_Click(object sender, RoutedEventArgs e)
        {
            string command = "SELECT * FROM CUSTOMER WHERE custname='"+this.customername+"' AND custid="+this.customerid+";";
            NpgsqlCommand comm = new NpgsqlCommand(command, this.connection);
            //Console.WriteLine();
            try
            {
                string outp = comm.ExecuteScalar().ToString();
                comm.Dispose();
                Window1 w1 = new Window1(this.customerid,this.customername,this.connection);
                Close();
                w1.ShowDialog();
                //MessageBoxResult mbr = MessageBox.Show(this, outp);
            }
            catch(Exception ex)
            {
                MessageBox.Show(this, "CUSTOMER NOT FOUND");
            }
        }

        private void ManagerID_TextChanged(object sender, TextChangedEventArgs e)
        {
            this.managerid = ((TextBox)sender).Text;
        }

        private void ManagerPass_PasswordChanged(object sender, RoutedEventArgs e)
        {
            this.managerpass = ((PasswordBox)sender).Password;
        }

        private void ManagerLogin_Click(object sender, RoutedEventArgs e)
        {
            if(this.managerid=="grayhat" && this.managerpass == "sammsammer")
            {
                Window2 w2 = new Window2(this.managerid, this.managerpass, this.connection);
                this.Close();
                w2.ShowDialog();
            }
            else
            {
                //Console.WriteLine("Invalid Cred");
                MessageBox.Show(this, "INVALID CRED");
            }
        }

        private void CreateItem_Click(object sender, RoutedEventArgs e)
        {
            ItemPage w = new ItemPage(this.connection);
            this.Close();
            w.ShowDialog();
        }

        private void CreateRetailer_Click(object sender, RoutedEventArgs e)
        {
            RetailerPage w = new RetailerPage(this.connection);
            this.Close();
            w.ShowDialog();
        }

        private void CreateCustomer_Click(object sender, RoutedEventArgs e)
        {
            CustomerPage w = new CustomerPage(this.connection);
            this.Close();
            w.ShowDialog();
        }
    }
}
