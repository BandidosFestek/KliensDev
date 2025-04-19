using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Hotcakes.CommerceDTO.v1;
using Hotcakes.CommerceDTO.v1.Catalog;
using Hotcakes.CommerceDTO.v1.Client;

namespace Bandidos_kliens_app
{
    public partial class UserControl2 : UserControl
    {
        private List<ProductStats> productList = new List<ProductStats>();
        private List<ProductStats> originalProductList = new List<ProductStats>();

        public UserControl2()
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
            dataGridView1.ReadOnly = false;
            dataGridView1.EditMode = DataGridViewEditMode.EditOnEnter;
            dataGridView1.Columns.Clear();

            // Kiválasztósor szélességének csökkentése
            dataGridView1.RowHeadersWidth = 20;

            // SKU oszlop (fix szélesség, csak olvasható)
            var skuColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SKU",
                HeaderText = "SKU",
                Name = "SKU",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 80
            };
            dataGridView1.Columns.Add(skuColumn);

            // Terméknév oszlop (kitölti a maradék helyet, csak olvasható)
            var nameColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "Name",
                HeaderText = "Terméknév",
                Name = "Name",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            dataGridView1.Columns.Add(nameColumn);

            // Kiemelt termék oszlop (logikai, checkbox, szerkeszthető)
            var featuredColumn = new DataGridViewCheckBoxColumn
            {
                DataPropertyName = "IsFeatured",
                HeaderText = "Kiemelt",
                Name = "IsFeatured",
                ReadOnly = false,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 30
            };
            dataGridView1.Columns.Add(featuredColumn);

            // Legtöbbet megtekintett oszlop (szám, csak olvasható)
            var viewsColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "ViewCount",
                HeaderText = "Legtöbbet megtekintett",
                Name = "ViewCount",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 30
            };
            dataGridView1.Columns.Add(viewsColumn);

            // Legtöbbet eladott oszlop (szám, csak olvasható)
            var soldColumn = new DataGridViewTextBoxColumn
            {
                DataPropertyName = "SoldCount",
                HeaderText = "Legtöbbet eladott",
                Name = "SoldCount",
                ReadOnly = true,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.None,
                Width = 30
            };
            dataGridView1.Columns.Add(soldColumn);

            // AutoSizeColumnsMode beállítása
            dataGridView1.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None;
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

                productList = new List<ProductStats>();

                foreach (var product in productsResponse.Content)
                {
                    if (string.IsNullOrEmpty(product.Bvin))
                    {
                        MessageBox.Show($"Hiányzó Bvin a termékhez: {product.Sku ?? "Ismeretlen SKU"}");
                        continue;
                    }

                    productList.Add(new ProductStats
                    {
                        SKU = product.Sku ?? string.Empty,
                        Name = product.ProductName ?? string.Empty,
                        IsFeatured = product.Featured,
                        ViewCount = 0,
                        SoldCount = 0,
                        ProductBvin = product.Bvin
                    });
                }

                // Mély másolat készítése az originalProductList számára
                originalProductList = productList.Select(p => new ProductStats
                {
                    SKU = p.SKU,
                    Name = p.Name,
                    IsFeatured = p.IsFeatured,
                    ViewCount = p.ViewCount,
                    SoldCount = p.SoldCount,
                    ProductBvin = p.ProductBvin
                }).ToList();

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
                int updatedCount = 0;

                // A DataGridView látható sorain iterálunk a szűrés miatt
                foreach (DataGridViewRow row in dataGridView1.Rows)
                {
                    if (row.DataBoundItem is ProductStats currentProduct)
                    {
                        // Keresünk az eredeti listában a megfelelő terméket
                        var originalProduct = originalProductList.FirstOrDefault(p => p.ProductBvin == currentProduct.ProductBvin);
                        if (originalProduct == null)
                        {
                            MessageBox.Show($"Nem található eredeti termék: {currentProduct.SKU}");
                            continue;
                        }

                        // Ellenőrizzük, hogy a "Kiemelt" értéke változott-e
                        if (currentProduct.IsFeatured != originalProduct.IsFeatured)
                        {
                            // Termék lekérdezése a Hotcakes API-ból
                            var productResponse = proxy.ProductsFind(currentProduct.ProductBvin);
                            if (productResponse.Errors.Any() || productResponse.Content == null)
                            {
                                MessageBox.Show($"Nem sikerült lekérdezni a terméket: {currentProduct.SKU}");
                                continue;
                            }

                            var productToUpdate = productResponse.Content;
                            productToUpdate.Featured = currentProduct.IsFeatured;

                            // Termék frissítése a Hotcakes API-n keresztül
                            var updateResponse = proxy.ProductsUpdate(productToUpdate);
                            if (updateResponse.Errors.Any())
                            {
                                MessageBox.Show($"Nem sikerült frissíteni a terméket: {currentProduct.SKU}. Hiba: {string.Join(", ", updateResponse.Errors)}");
                                continue;
                            }

                            // Frissítjük az originalProductList-ben az eredeti állapotot
                            originalProduct.IsFeatured = currentProduct.IsFeatured;
                            updatedCount++;
                        }
                    }
                }

                MessageBox.Show($"Frissítés sikeres! {updatedCount} termék frissítve.");
                bindingSource1.ResetBindings(false);
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Hiba történt a frissítés közben: " + ex.Message);
            }
        }

        

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            try
            {
                string filterText = textBox1.Text.Trim();
                if (string.IsNullOrEmpty(filterText))
                {
                    productList = originalProductList.Select(p => new ProductStats
                    {
                        SKU = p.SKU,
                        Name = p.Name,
                        IsFeatured = p.IsFeatured,
                        ViewCount = p.ViewCount,
                        SoldCount = p.SoldCount,
                        ProductBvin = p.ProductBvin
                    }).ToList();
                }
                else
                {
                    productList = originalProductList
                        .Where(p => (p.SKU ?? string.Empty).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0 ||
                                    (p.Name ?? string.Empty).IndexOf(filterText, StringComparison.OrdinalIgnoreCase) >= 0)
                        .Select(p => new ProductStats
                        {
                            SKU = p.SKU,
                            Name = p.Name,
                            IsFeatured = p.IsFeatured,
                            ViewCount = p.ViewCount,
                            SoldCount = p.SoldCount,
                            ProductBvin = p.ProductBvin
                        })
                        .ToList();
                }

                bindingSource1.DataSource = null;
                bindingSource1.DataSource = productList;
                bindingSource1.ResetBindings(false);
                dataGridView1.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Hiba történt a szűrés során: {ex.Message}\nAdatforrás: {(bindingSource1.DataSource != null ? "Létezik" : "Null")}\nSzűrt elemek száma: {productList.Count}",
                                "Hiba", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // Segédosztály az adatok tárolására
        public class ProductStats
        {
            public string SKU { get; set; }
            public string Name { get; set; }
            public bool IsFeatured { get; set; }
            public int ViewCount { get; set; }
            public int SoldCount { get; set; }
            public string ProductBvin { get; set; } // Bvin a frissítéshez
        }
    }
}
