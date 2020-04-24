
using Newtonsoft.Json;
using Project.DTO;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class ChangePassword : Form
    {
        string url = @"http://localhost:52593/";
        string pathAccount = @"api/accounts";
        HttpClient client;
        AccountDTO account = new AccountDTO();
        public string idStudent;
        string studentId = null;
        public ChangePassword()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            InitializeComponent();
        }

        private void ChangePassword_Load(object sender, EventArgs e)
        {
            studentId = idStudent;
            btnSave.Enabled = false;
        }

        private void tbPassChange_TextChanged(object sender, EventArgs e)
        {
            if(tbPassChange.Text.Length == 0)
            {
                btnSave.Enabled = false;
            }else
            {
                btnSave.Enabled = true;
            }
        }

        private async void btnSave_Click(object sender, EventArgs e)
        {
            var salf = BCrypt.Net.BCrypt.GenerateSalt(12);
            var pass = BCrypt.Net.BCrypt.HashPassword(tbPassChange.Text.Trim(), salf);

            account.username = studentId;
            account.salf = salf;
            account.password = pass;

            var jsonAccount = JsonConvert.SerializeObject(account);
            var content = new StringContent(jsonAccount, Encoding.UTF8, "application/json");
            var result = await client.PutAsync($@"{pathAccount}/{studentId}", content);

            this.Close();
            MessageBox.Show("Changed Password Successfully", "Message");
            StudentApp frStudent = new StudentApp();
            frStudent.studentId = studentId;           
            frStudent.Show();
        }
    }
}
