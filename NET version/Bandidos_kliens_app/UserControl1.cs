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

namespace Bandidos_kliens_app
{
    public partial class UserControl1 : UserControl
    {
        private readonly HttpClient client;
        private readonly string apiUrl = "http://rendfejl10006.northeurope.cloudapp.azure.com:8080/";
        private readonly string apiKey = "1-7d29439f-b04e-413b-9c28-30717808fb20"; // Cseréld a Hotcakes API kulcsra
        private DataTable dataTable;
        public UserControl1()
        {
            InitializeComponent();
        }
    }
}
