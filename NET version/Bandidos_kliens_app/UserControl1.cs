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
using Hotcakes.CommerceDTO.v1;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;

//1-7d29439f-b04e-413b-9c28-30717808fb20

namespace Bandidos_kliens_app
{
    public partial class UserControl1 : UserControl
    {
        private readonly BindingSource bindingSource = new BindingSource();
        //v1 - private readonly string apiUrl = "http://rendfejl10006.northeurope.cloudapp.azure.com:8080/DesktopModules/Hotcakes/API/rest/v1/products?key=1-7d29439f-b04e-413b-9c28-30717808fb20";

        public UserControl1()
        {
            InitializeComponent();
            dataGridView1.DataSource = bindingSource1;
            ConfigureDataGridView();
            LoadData();
        }

        private void ConfigureDataGridView()
        {
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.AllowUserToAddRows = false; // Új sorok hozzáadásának tiltása
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter; // Szerkesztés engedélyezése azonnal
            dataGridView1.Columns.Clear();

            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SKU",
                HeaderText = "Termékkód",
                Name = "SKU",
                ReadOnly = true
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Terméknév",
                Name = "Name",
                ReadOnly = true
            });
            dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "QuantityOnHand",
                HeaderText = "Készlet",
                Name = "QuantityOnHand",
                ReadOnly = false // Explicit szerkeszthetővé tétel
            });
        }

        private static Api CreateApiClient()
        {
            string url = "http://rendfejl10006.northeurope.cloudapp.azure.com:8080";
            string key = "1-7d29439f-b04e-413b-9c28-30717808fb20";
            return new Api(url, key);
        }


        private void LoadData()
        {
            try
            {
                var proxy = CreateApiClient();
                var productsResponse = proxy.ProductsFindAll();

                if (productsResponse.Errors.Any())
                {
                    MessageBox.Show("Nem sikerült a termékek lekérdezése: " + string.Join(", ", productsResponse.Errors));
                    return;
                }

                var productList = new List<ProductData>();

                foreach (var product in productsResponse.Content)
                {
                    var inventoryResponse = proxy.ProductInventoryFindForProduct(product.Bvin);

                    int qty = 0;
                    if (!inventoryResponse.Errors.Any() && inventoryResponse.Content != null && inventoryResponse.Content.Any())
                    {
                        qty = inventoryResponse.Content.First().QuantityOnHand;
                    }

                    productList.Add(new ProductData
                    {
                        SKU = product.Sku,
                        Name = product.ProductName,
                        QuantityOnHand = qty,
                        ProductBvin = product.Bvin
                    });
                }

                bindingSource1.DataSource = productList;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt az adatok betöltése közben: " + ex.Message);
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            try
            {
                var proxy = CreateApiClient();

                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.IsNewRow) continue;

                    var product = (ProductData)row.DataBoundItem;
                    string sku = product.SKU;
                    string productBvin = product.ProductBvin;
                    if (!int.TryParse(row.Cells["QuantityOnHand"].Value?.ToString(), out int quantity))
                    {
                        MessageBox.Show($"Érvénytelen készlet érték: {sku}");
                        continue;
                    }

                    var inventoryResponse = proxy.ProductInventoryFindForProduct(productBvin);
                    if (inventoryResponse.Errors.Any() || inventoryResponse.Content == null || !inventoryResponse.Content.Any())
                    {
                        MessageBox.Show($"Nem található készletadat: {sku}");
                        continue;
                    }

                    var inventory = inventoryResponse.Content.First();
                    inventory.QuantityOnHand = quantity;

                    var updateResponse = proxy.ProductInventoryUpdate(inventory);
                    if (updateResponse.Errors.Any())
                    {
                        MessageBox.Show($"Nem sikerült frissíteni a készletet: {sku}. Hiba: {string.Join(", ", updateResponse.Errors)}");
                    }
                }

                MessageBox.Show("Készletfrissítés sikeres!");
                LoadData(); // Frissítjük az adatokat a mentés után
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt frissítés közben: " + ex.Message);
            }
        }

        // Segédosztály az adatok tárolására
        public class ProductData
        {
            public string SKU { get; set; }
            public string Name { get; set; }
            public int QuantityOnHand { get; set; }
            public string ProductBvin { get; set; }
        }
    }
}
