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

    public partial class Window1 : Window
    {
        private string custId;
        private string custName;
        private NpgsqlConnection connection;
        private List<StockItem> stockItems;
        private List<StockItem> cartItems;
        private StackPanel cartStack;
        private int cartCost;
        private NpgsqlCommand command;
        private StackPanel stack;
        public Window1(string custId, string custName, NpgsqlConnection connection)
        {
            InitializeComponent();
            this.custId = custId;
            this.custName = custName;
            this.Title = "Wholesale Management - " + this.custName;
            this.connection = connection;
            this.stockItems = new List<StockItem>();
            this.cartItems = new List<StockItem>();
            this.stack = new StackPanel();
            this.cartStack = new StackPanel();
            this.command = new NpgsqlCommand("",this.connection);
            this.cartCost = 0;
            this.GetStockData();
        }
        private void GetStockData()
        {
            string comnd = "SELECT * FROM STOCK;";
            this.command.CommandText = comnd;
            NpgsqlDataReader npgsqlDataReader = this.command.ExecuteReader();
            while (npgsqlDataReader.Read())
            {
                StockItem item = new StockItem((string)npgsqlDataReader[1], (int)npgsqlDataReader[0], (int)npgsqlDataReader[4], (int)npgsqlDataReader[2], (int)npgsqlDataReader[3]);
                this.stockItems.Add(item);
            }
            npgsqlDataReader.Close();
            this.FillProductScroll();
        }
        private void FillProductScroll()
        {
            stack.Orientation = Orientation.Vertical;
            foreach(StockItem item in this.stockItems)
            {
                item.SetLabelContent();
                item.label.FontSize = 16;
                item.label.Background = new SolidColorBrush(Color.FromRgb(51, 51, 55));
                item.label.Foreground = Brushes.White;
                item.label.Height = 50;
                item.label.Width = ScrollV.Width - 10;
                item.onClicked = this.OnClicked;

                stack.Children.Add(item.label);
            }
            ScrollV.Content = stack;
        }
        private void updateCost()
        {
            this.cartCost = 0;
            foreach(StockItem item in cartItems)
            {
                this.cartCost += item.sellp * item.quant;
            }
            Price.Content = "$ " + this.cartCost + ".00";
        }
        private bool OnRemoveFromCart(StockItem item)
        {
            StockItem sitem = null;
            foreach(StockItem itm in this.stockItems)
            {
                if(itm.id == item.id)
                {
                    sitem = itm;
                    break;
                }
            }
            if (sitem.quant == 0)
            {
                sitem.label.Visibility = Visibility.Visible;
            }
            sitem.quant += 1;
            sitem.SetLabelContent();
            this.updateCost();
            return true;
        }
        private bool OnClicked(StockItem item)
        {
            this.cartStack.Children.Clear();
            this.cartStack.Orientation = Orientation.Vertical;
            StockItem titem = null;
            foreach(StockItem citem in this.cartItems)
            {
                if(citem.id == item.id)
                {
                    titem = citem;
                    break;
                }
            }
            if (titem != null)
            {
                if (titem.quant == 0)
                {
                    titem.label.Visibility = Visibility.Visible;
                }
                titem.quant += 1;
                titem.SetLabelContent();
                this.SetViewCart();
                this.updateCost();
                return true;
            }
            titem = new StockItem(item.name, item.id, 1, item.costp, item.sellp);
            titem.label.FontSize = 16;
            titem.label.Background = new SolidColorBrush(Color.FromRgb(51, 51, 55));
            titem.label.Foreground = Brushes.White;
            titem.label.Height = 50;
            titem.label.Width = ScrollVI.Width - 10;
            titem.onClicked = this.OnRemoveFromCart;
            titem.SetLabelContent();
            this.cartItems.Add(titem);
            this.SetViewCart();
            this.updateCost();
            return true;
        }
        public void SetViewCart()
        {
            foreach(StockItem item in this.cartItems)
            {
                this.cartStack.Children.Add(item.label);
            }
            ScrollVI.Content = this.cartStack;
        }
        private void BUY(object sender, RoutedEventArgs e)
        {
            if (this.cartCost <= 0)
            {
                return;
            }
            foreach(StockItem item in this.cartItems)
            {
                string stcom = "UPDATE STOCK SET stockquant = stockquant - "+item.quant+" WHERE stockid = "+item.id+"; ";
                this.command.CommandText = stcom;
                int i=this.command.ExecuteNonQuery();
                if (i <= 0)
                {
                    MessageBox.Show(item.id + " failed to be purchased");
                    this.cartCost -= item.sellp;
                }
            }
            string stcom2 = "INSERT INTO CUST_PAYMENTS (paymentstatus,customerid,amount) VALUES(true, "+this.custId+", "+this.cartCost+"); ";
            this.command.CommandText = stcom2;
            int i2 = this.command.ExecuteNonQuery();
            MessageBox.Show(""+i2,"Purchase");
            this.DestroyItems();
            this.cartCost = 0;
            this.GetStockData();
        }
        private void DestroyItems()
        {
            this.stack.Children.Clear();
            this.cartStack.Children.Clear();
            this.cartItems.Clear();
            this.stockItems.Clear();
        }

        private void Logout(object sender, RoutedEventArgs e)
        {
            this.command.Dispose();
            this.connection.Close();
            MainWindow w1 = new MainWindow();
            this.Close();
            w1.ShowDialog();
        }
    }
    public class StockItem
    {
        public string name;
        public int id;
        public int quant;
        public int costp;
        public int sellp;
        public Label label;
        public Func<StockItem,bool> onClicked;

        public StockItem(string name,int id,int quant,int costp,int sellp)
        {
            this.name = name;
            this.id = id;
            this.quant = quant;
            this.costp = costp;
            this.sellp = sellp;
            this.label = new Label();
            this.label.MouseDown += new MouseButtonEventHandler(HandleClick);
        }
        public StockItem(string name, int id, int quant, int costp, int sellp,bool flag)
        {
            this.name = name;
            this.id = id;
            this.quant = quant;
            this.costp = costp;
            this.sellp = sellp;
            this.label = new Label();
        }
        private void HandleClick(Object sender, EventArgs e)
        {
            if (this.quant <= 0)
            {
                return;
            }
            this.quant -= 1;
            SetLabelContent();
            this.onClicked(this);
        }

        public void SetLabelContent()
        {
            if (this.quant == 0)
            {
                this.label.Visibility = Visibility.Collapsed;
            }
            this.label.Content = this.name + " - " + this.quant + "\n$" + this.sellp;
        }
    }
}
