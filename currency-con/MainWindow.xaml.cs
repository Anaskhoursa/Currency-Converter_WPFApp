using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
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
using Newtonsoft.Json;

namespace currency_con
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        Root val = new Root();

        public class Root
        {
            public Rate rates { get; set; }
        }

        public class Rate
        {
            public double INR { get; set; }
            public double JPY { get; set; }
            public double USD { get; set; }
            public double NZD { get; set; }
            public double EUR { get; set; }
            public double CAD { get; set; }
            public double ISK { get; set; }
            public double PHP { get; set; }
            public double DKK { get; set; }
            public double CZK { get; set; }
        }
        SqlConnection con = new SqlConnection();
        SqlCommand cmd = new SqlCommand();
        SqlDataAdapter da = new SqlDataAdapter();

        private int CurrencyID = 0;
        
        public MainWindow()
        {
            InitializeComponent();
            ClearControls();
            GetData();
            GetValue();
            
        }

        public void mycon()
        {
            string Conn = ConfigurationManager.ConnectionStrings["MyConnectionString"].ConnectionString;
            con = new SqlConnection(Conn);
            con.Open();
        }

        private async void GetValue()
        {
            val = await GetDataGetMethod<Root>("https://openexchangerates.org/api/latest.json?app_id=873b699a90744041af9517620c1982ec"); //API Link
            BindCurrency();
        }

        public static async Task<Root> GetDataGetMethod<T>(string url)
        {
            var ss = new Root();
            try
            {
                using (var client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromMinutes(1);

                    HttpResponseMessage response = await client.GetAsync(url);

                    if (response.StatusCode == System.Net.HttpStatusCode.OK)
                    {
                        var ResponceString = await response.Content.ReadAsStringAsync();


                        var ResponceObject = JsonConvert.DeserializeObject<Root>(ResponceString);
                        return ResponceObject;  
                    }
                    return ss;
                }
            }
            catch
            {
                return ss;
            }
        }
        private void BindCurrency()
        {
            mycon();
            DataTable dtCurrency = new DataTable();
            cmd = new SqlCommand("select id, Amount, CurrencyName from Currency_Master", con);
            da = new SqlDataAdapter(cmd);
            da.Fill(dtCurrency);
            DataRow NewRow = dtCurrency.NewRow();
            NewRow["id"] = 0;
            NewRow["CurrencyName"] = "--SELECT--";
            dtCurrency.Rows.InsertAt(NewRow, 0);

            if (dtCurrency != null && dtCurrency.Rows.Count > 0)
            {

                cmbFromCurrency.ItemsSource = dtCurrency.DefaultView;
                cmbToCurrency.ItemsSource = dtCurrency.DefaultView;
            }
            con.Close();
            cmbFromCurrency.DisplayMemberPath = "CurrencyName";
            cmbFromCurrency.SelectedValuePath = "Amount";
            cmbFromCurrency.SelectedIndex = 0;


            cmbToCurrency.DisplayMemberPath = "CurrencyName";
            cmbToCurrency.SelectedValuePath = "Amount";
            cmbToCurrency.SelectedIndex = 0;
            
            

        }
        private void ClearControls()
        {
            txtCurrency.Text = string.Empty;
            if (cmbFromCurrency.Items.Count > 0)
                cmbFromCurrency.SelectedIndex = 0;
            if (cmbToCurrency.Items.Count > 0)
                cmbToCurrency.SelectedIndex = 0;
            lblCurrency.Text = "";
            txtCurrency.Focus();
        }


        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new Regex("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private void Convert_Click(object sender, RoutedEventArgs e)
        {
            double ConvertedValue;

            if (txtCurrency.Text == null || txtCurrency.Text.Trim() == "")
            {
                MessageBox.Show("Please Enter Currency", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                txtCurrency.Focus();
                return;
            }
            else if (cmbFromCurrency.SelectedValue == null || cmbFromCurrency.SelectedIndex == 0 || cmbFromCurrency.Text == "--SELECT--")
            {
                MessageBox.Show("Please Select Currency From", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                cmbFromCurrency.Focus();
                return;
            }
            else if (cmbToCurrency.SelectedValue == null || cmbToCurrency.SelectedIndex == 0 || cmbToCurrency.Text == "--SELECT--")
            {
                MessageBox.Show("Please Select Currency To", "Information", MessageBoxButton.OK, MessageBoxImage.Information);

                cmbToCurrency.Focus();
                return;
            }

            if (cmbFromCurrency.Text == cmbToCurrency.Text)
            {
                ConvertedValue = double.Parse(txtCurrency.Text);

                lblCurrency.Text = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
            else
            {
                
                ConvertedValue = (double.Parse(cmbToCurrency.SelectedValue.ToString()) * double.Parse(txtCurrency.Text)) / double.Parse(cmbFromCurrency.SelectedValue.ToString());

                lblCurrency.Text = cmbToCurrency.Text + " " + ConvertedValue.ToString("N3");
            }
        }
        private void Clear_Click(object sender, RoutedEventArgs e)
        {
            ClearControls();
        }
        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ClearMaster();
            }
            catch (Exception ee)
            {
                MessageBox.Show(ee.Message);
            }
        }

        private void dgvCurrency_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            try
            {
                DataGrid grd = (DataGrid)sender;
                DataRowView row_selected = grd.CurrentItem as DataRowView;

                if (row_selected != null)
                {

                    if (dgvCurrency.Items.Count > 0)
                    {
                        if (grd.SelectedCells.Count > 0)
                        {

                            CurrencyID = Int32.Parse(row_selected["Id"].ToString());

                            if (grd.SelectedCells[0].Column.DisplayIndex == 0)
                            {

                                txtAmount.Text = row_selected["Amount"].ToString();

                                txtCurrencyName.Text = row_selected["CurrencyName"].ToString();

                                btnSave.Content = "Update";
                            }

                            if (grd.SelectedCells[0].Column.DisplayIndex == 1)
                            {
                                if (MessageBox.Show("Are you sure you want to delete ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                                {
                                    mycon();
                                    DataTable dtCurrency = new DataTable();

                                    cmd = new SqlCommand("DELETE FROM Currency_Master WHERE Id = @Id", con);
                                    cmd.CommandType = CommandType.Text;

                                    cmd.Parameters.AddWithValue("@Id", CurrencyID);
                                    cmd.ExecuteNonQuery();
                                    con.Close();

                                    MessageBox.Show("Data deleted successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ClearMaster();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (txtAmount.Text == null || txtAmount.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter amount", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtAmount.Focus();
                    return;
                }
                else if (txtCurrencyName.Text == null || txtCurrencyName.Text.Trim() == "")
                {
                    MessageBox.Show("Please enter currency name", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                    txtCurrencyName.Focus();

                }
                else
                {
                    if (CurrencyID != 0 && CurrencyID > 0)
                    {
                        if (MessageBox.Show("Are you sure you want to Update ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                        {
                            mycon();
                            cmd = new SqlCommand("UPDATE Currency_Master SET Amount = @Amount, CurrencyName=@CurrencyName WHERE Id= @Id", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Id", CurrencyID);

                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();
                        }

                    }
                    else
                    {
                        if (MessageBox.Show("Are you sure you want to Save ?", "Information", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)

                        {
                            mycon();
                            cmd = new SqlCommand("INSERT INTO Currency_Master (Amount, CurrencyName) VALUES (@Amount, @CurrencyName)", con);
                            cmd.CommandType = CommandType.Text;
                            cmd.Parameters.AddWithValue("@Amount", txtAmount.Text);
                            cmd.Parameters.AddWithValue("@CurrencyName", txtCurrencyName.Text);
                            cmd.ExecuteNonQuery();
                            con.Close();

                            MessageBox.Show("Data saved successfully", "Information", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                    }
                    ClearMaster();

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void ClearMaster()
        {
            try
            {
                txtAmount.Text = string.Empty;
                txtCurrencyName.Text = string.Empty;
                btnSave.Content = "Save";
                GetData();
                CurrencyID = 0;
                BindCurrency();
                txtAmount.Focus();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private async Task GetData()
        {

            val = await GetDataGetMethod<Root>("https://openexchangerates.org/api/latest.json?app_id=d77f014deaf44000aa0ecd0b7a60a077");
            mycon();
                DataTable dtCurrency = new DataTable();
            foreach (var property in typeof(Rate).GetProperties())
            {
                // Get the property name and value
                string propertyName = property.Name;
                double propertyValue = (double)property.GetValue(val.rates);
                SqlCommand cmd1 = new SqlCommand("DELETE FROM Currency_Master WHERE CurrencyName = @CurrencyName", con);
                cmd1.CommandType = CommandType.Text;
                
                cmd1.Parameters.AddWithValue("@CurrencyName", propertyName);
                cmd1.ExecuteNonQuery();
            }
            foreach (var property in typeof(Rate).GetProperties())
            {
                // Get the property name and value
                string propertyName = property.Name;
                double propertyValue = (double)property.GetValue(val.rates);
                SqlCommand cmd1 = new SqlCommand("INSERT INTO Currency_Master (Amount, CurrencyName) VALUES (@Amount, @CurrencyName)", con);
                cmd1.CommandType = CommandType.Text;
                cmd1.Parameters.AddWithValue("@Amount", propertyValue);
                cmd1.Parameters.AddWithValue("@CurrencyName", propertyName);
                cmd1.ExecuteNonQuery();
            }

            cmd = new SqlCommand("SELECT * FROM Currency_Master", con);
                SqlDataAdapter da = new SqlDataAdapter(cmd);
                da.Fill(dtCurrency);
                if (dtCurrency != null && dtCurrency.Rows.Count > 0)
                {
                    dgvCurrency.ItemsSource = dtCurrency.DefaultView;
                }
                else
                {
                    dgvCurrency.ItemsSource = null;
                }


                con.Close();
            }
            
            
        }



    
}
