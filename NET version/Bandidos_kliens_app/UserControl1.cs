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
        //private readonly BindingSource bindingSource = new BindingSource();
        private List<ProductData> productList = new List<ProductData>();
        private List<ProductData> originalProductList = new List<ProductData>();
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
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView1.ReadOnly = false; // Biztosítjuk, hogy a DataGridView szerkeszthető
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
                ReadOnly = false
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

                productList = new List<ProductData>();

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
                        ProductBvin = product.Bvin,
                        OriginalQuantity = qty
                    });
                }

                bindingSource1.DataSource = productList;
                ConfigureDataGridView();
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
                int updatedCount = 0;

                foreach (ProductData product in productList)
                {
                    if (product.QuantityOnHand != product.OriginalQuantity)
                    {
                        if (product.QuantityOnHand < 0)
                        {
                            MessageBox.Show($"Negatív készlet nem megengedett: {product.SKU}", "Érvénytelen bevitel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                            continue;
                        }

                        var inventoryResponse = proxy.ProductInventoryFindForProduct(product.ProductBvin);
                        if (inventoryResponse.Errors.Any() || inventoryResponse.Content == null || !inventoryResponse.Content.Any())
                        {
                            MessageBox.Show($"Nem található készletadat: {product.SKU}");
                            continue;
                        }

                        var inventory = inventoryResponse.Content.First();
                        inventory.QuantityOnHand = product.QuantityOnHand;

                        var updateResponse = proxy.ProductInventoryUpdate(inventory);
                        if (updateResponse.Errors.Any())
                        {
                            MessageBox.Show($"Nem sikerült frissíteni a készletet: {product.SKU}. Hiba: {string.Join(", ", updateResponse.Errors)}");
                            continue;
                        }

                        product.OriginalQuantity = product.QuantityOnHand;
                        updatedCount++;
                    }
                }

                MessageBox.Show($"Készletfrissítés sikeres! {updatedCount} termék frissítve.");
                bindingSource1.ResetBindings(false);
                ConfigureDataGridView();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt frissítés közben: " + ex.Message);
            }
        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string filterText = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(filterText))
                {
                    productList = new List<ProductData>(originalProductList);
                }
                else
                {
                    // LINQ-alapú szűrés, kis- és nagybetű-érzéketlen, IndexOf használatával
                    productList = originalProductList
                        .Where(p => p.SKU.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    p.Name.IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .ToList();
                }

                bindingSource1.DataSource = null; // Először leválasztjuk az adatforrást
                bindingSource1.DataSource = productList; // Újrakötjük a szűrt listát
                bindingSource1.ResetBindings(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a szűrés során: {ex.Message}\nAdatforrás: {(bindingSource1.DataSource != null ? "Létezik" : "Null")}\nSzűrt elemek száma: {productList.Count}",
                                "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            if (e.ColumnIndex == dataGridView1.Columns.IndexOf(dataGridView1.Columns["QuantityOnHand"]) && e.Exception is FormatException)
            {
                MessageBox.Show("Kérjük, csak számokat adjon meg a készlet mezőben!", "Érvénytelen bevitel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                e.Cancel = true;
            }
            else
            {
                MessageBox.Show("Hiba történt az adatok szerkesztése közben: " + e.Exception.Message, "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void dataGridView1_CellValidating(object sender, DataGridViewCellValidatingEventArgs e)
        {

            if (dataGridView1.Columns[e.ColumnIndex].Name == "QuantityOnHand")
            {
                string input = e.FormattedValue?.ToString();
                if (!int.TryParse(input, out int value) || value < 0)
                {
                    MessageBox.Show("Kérjük, adjon meg egy pozitív egész számot a készlethez!", "Érvénytelen bevitel", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    e.Cancel = true;
                }
            }

        }


        // Segédosztály az adatok tárolására
        public class ProductData
        {
            public string SKU { get; set; }
            public string Name { get; set; }
            public int QuantityOnHand { get; set; }
            public string ProductBvin { get; set; }
            public int OriginalQuantity { get; set; }
        }
    }
}
