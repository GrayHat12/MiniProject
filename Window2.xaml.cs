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
    public partial class Window2 : Window
    {
        private string username;
        private string password;
        private List<StockItem> stockItems;
        private Retailer currentRetailer;
        private List<Retailer> retailers;
        private NpgsqlConnection connection;
        private NpgsqlCommand command;
        private StackPanel stockPanel;
        private StackPanel retailerPanel;
        public Window2(string name, string password, NpgsqlConnection connection)
        {
            InitializeComponent();
            this.username = name;
            this.password = password;
            this.connection = connection;
            this.Title = "MANAGER - " + this.username.ToUpper();
            this.command = new NpgsqlCommand("", this.connection);
            this.stockItems = new List<StockItem>();
            this.stockPanel = new StackPanel();
            this.retailerPanel = new StackPanel();
            this.retailers = new List<Retailer>();
            this.currentRetailer = null;
            this.GetStockData();
            this.GetRetailerData();
        }
        private void GetStockData()
        {
            this.stockItems.Clear();
            string comnd = "SELECT * FROM STOCK;";
            this.command.CommandText = comnd;
            NpgsqlDataReader npgsqlDataReader = this.command.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                StockItem item = new StockItem((string)npgsqlDataReader[1], (int)npgsqlDataReader[0], (int)npgsqlDataReader[4], (int)npgsqlDataReader[2], (int)npgsqlDataReader[3],true);
                this.stockItems.Add(item);
            }
            npgsqlDataReader.Close();
            this.FillProductScroll();
        }
        private void GetRetailerData()
        {
            this.retailers.Clear();
            string comnd = "SELECT * from RETAILERS;";
            this.command.CommandText = comnd;
            NpgsqlDataReader npgsqlDataReader = this.command.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                Retailer retail;
                Item item;
                foreach(StockItem titem in this.stockItems)
                {
                    if(titem.id == (int)npgsqlDataReader[3])
                    {
                        item = new Item(titem.name, titem.id, titem.quant, titem.costp, titem.sellp);
                        retail = new Retailer((int)npgsqlDataReader[0], (string)npgsqlDataReader[1], (string)npgsqlDataReader[2], item);
                        this.retailers.Add(retail);
                        if (this.currentRetailer == null)
                        {
                            this.currentRetailer = retail;
                        }
                        break;
                    }
                }
            }
            npgsqlDataReader.Close();
            this.FillRetailerScroll();
        }
        private void FillProductScroll()
        {
            stockPanel.Children.Clear();
            stockPanel.Orientation = Orientation.Vertical;
            foreach (StockItem item in this.stockItems)
            {
                item.SetLabelContent();
                item.label.FontSize = 16;
                item.label.Background = new SolidColorBrush(Color.FromRgb(51, 51, 55));
                item.label.Foreground = Brushes.White;
                item.label.Height = 50;
                item.label.Width = ScrollStock.Width - 10;
                item.onClicked = this.OnClicked;

                stockPanel.Children.Add(item.label);
            }
            ScrollStock.Content = stockPanel;
        }
        private void FillRetailerScroll()
        {
            retailerPanel.Children.Clear();
            retailerPanel.Orientation = Orientation.Vertical;
            foreach (Retailer item in this.retailers)
            {
                item.SetLabelContent();
                item.label.FontSize = 16;
                item.label.Background = new SolidColorBrush(Color.FromRgb(51, 51, 55));
                item.label.Foreground = Brushes.White;
                item.label.Height = 50;
                item.label.Width = ScrollStock.Width - 10;
                item.onClicked = this.OnClicked;

                retailerPanel.Children.Add(item.label);
            }
            RetailerScrollV.Content = retailerPanel;
        }
        private bool OnClicked(Retailer item)
        {
            ItName.Content = item.stock.name.ToUpper();
            this.currentRetailer = item;
            CPLabel.Content = "CP : $" + item.stock.costp + ".00";
            SPLabel.Content = "SP : $" + item.stock.sellp + ".00";
            return true;
        }
        private bool OnClicked(StockItem item)
        {
            return true;
        }
        private void BuyClick(object sender, RoutedEventArgs e)
        {
            int quant = 0;
            try
            {
                quant = Int32.Parse(InputQuant.Text);
            }catch(Exception ex)
            {
                MessageBox.Show("Error", "Quantity Should be a Natural Number");
                return;
            }
            if (quant <= 0)
            {
                MessageBox.Show("Error", "Quantity Should be a Natural Number");
                return;
            }
            int cost = this.currentRetailer.stock.costp * quant;
            string stcom = "UPDATE STOCK SET stockquant = stockquant + " + quant + " WHERE stockid = " + this.currentRetailer.stock.id + "; ";
            this.command.CommandText = stcom;
            int i = this.command.ExecuteNonQuery();
            if (i <= 0)
            {
                MessageBox.Show(" failed to be purchased");
                return;
            }
            string stcom2 = "INSERT INTO RETAIL_PAYMENTS (paymentstatus,customerid,amount) VALUES(true, " + this.currentRetailer.buyerid + ", " + cost +"); ";
            this.command.CommandText = stcom2;
            int i2 = this.command.ExecuteNonQuery();
            MessageBox.Show("" + i2, "Purchased, Cost : "+cost);
            this.GetStockData();
        }
        private void LogoutClick(object sender, RoutedEventArgs e)
        {
            this.command.Dispose();
            this.connection.Close();
            MainWindow w1 = new MainWindow();
            this.Close();
            w1.ShowDialog();
        }
    }
    public class Retailer
    {
        public int buyerid;
        public string buyername;
        public string buyeraddr;
        public Item stock;
        public Label label;
        public Func<Retailer, bool> onClicked;
        public Retailer(int id,string name,string addr,Item s)
        {
            this.buyerid = id;
            this.buyeraddr = addr;
            this.buyername = name;
            this.stock = s;
            this.label = new Label();
            this.label.MouseDown += new MouseButtonEventHandler(HandleClick);
        }
        private void HandleClick(Object sender, EventArgs e)
        {
            this.onClicked(this);
        }

        public void SetLabelContent()
        {
            this.label.Content = this.buyername + " : " + this.stock.name + "\n" + this.buyeraddr;
        }
    }
    public class Item
    {
        public string name;
        public int id;
        public int quant;
        public int costp;
        public int sellp;

        public Item(string name, int id, int quant, int costp, int sellp)
        {
            this.name = name;
            this.id = id;
            this.quant = quant;
            this.costp = costp;
            this.sellp = sellp;
        }

    }
}
