using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

//1-7d29439f-b04e-413b-9c28-30717808fb20

namespace Bandidos_kliens_app
{
    public partial class UserControl1 : UserControl
    {
        private readonly BindingSource bindingSource = new BindingSource();
        private readonly string apiUrl = "http://rendfejl10006.northeurope.cloudapp.azure.com:8080/DesktopModules/Hotcakes/API/rest/v1/products?key=1-7d29439f-b04e-413b-9c28-30717808fb20";

        public UserControl1()
        {
            InitializeComponent();
            dataGridView1.DataSource = bindingSource;
            LoadData();
        }

        private async void LoadData()
        {
            await FrissitAdatokatAsync();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            await FrissitAdatokatAsync();
        }

        private async Task FrissitAdatokatAsync()
        {
            try
            {
                using (HttpClient client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    // Itt teljes URL-t használsz ahelyett, hogy beállítanád a BaseAddress-t
                    var response = await client.GetAsync(apiUrl);
                    response.EnsureSuccessStatusCode();

                    string json = await response.Content.ReadAsStringAsync();

                    var apiResponse = JsonConvert.DeserializeObject<ApiResponse>(json);

                    if (apiResponse != null && apiResponse.Content != null && apiResponse.Content.Products != null)
                    {
                        bindingSource.DataSource = apiResponse.Content.Products;
                    }
                    else
                    {
                        MessageBox.Show("Nem sikerült beolvasni a termékeket.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt: " + ex.Message);
            }
        }
    }

    public class ApiResponse
    {
        public ContentWrapper Content { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }

    public class ContentWrapper
    {
        public List<Product> Products { get; set; }
    }

    public class Product
    {
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public string SitePrice { get; set; }
        public string SitePriceOverrideText { get; set; }
        public bool IsAvailableForSale { get; set; }
        public bool Featured { get; set; }
    }

    // Típusok a JSON válaszhoz
    /*
    public class ProductViewModel
    {
        public string SKU { get; set; }
        public string Name { get; set; }
        public int QuantityOnHand { get; set; }
    }

    public class InventoryDTO
    {
        public int QuantityOnHand { get; set; }
    }

    public class ProductDTO
    {
        public string Sku { get; set; }
        public string ProductName { get; set; }
        public InventoryDTO Inventory { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public List<ApiMessage> Messages { get; set; }
        public T Content { get; set; }
    }

    public class ApiMessage
    {
        public string Description { get; set; }
    }
    */
}
