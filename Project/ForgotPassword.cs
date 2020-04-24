
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using Project.DTO;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace Project
{
    public partial class ForgotPassword : Form
    {
        string url = @"http://localhost:52593/";
        string pathAccount = @"api/accounts";
        HttpClient clients;

        AccountDTO account = new AccountDTO();
        public ForgotPassword()
        {
            clients = new HttpClient();
            clients.BaseAddress = new Uri(url);
            clients.DefaultRequestHeaders.Accept.Clear();
            clients.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            InitializeComponent();
        }

        private void btnSend_Click(object sender, EventArgs e)
        {

            if (validate())
            {
                SendNewPassword();
            }
        }
        private async void SendNewPassword()
        {
            HttpResponseMessage response = await clients.GetAsync($"{pathAccount}/{tbUserNm.Text}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<AccountDTO>(result);

            string emailUser = results.email;
            string Newpass = "";
            Random rnd = new Random();
            for (var j = 0; j < 5; j++)
            {
                Newpass += Convert.ToChar(rnd.Next(97, 122));
            }
            string salf = BCrypt.Net.BCrypt.GenerateSalt(12);
            string pass = BCrypt.Net.BCrypt.HashPassword(Newpass, salf);
            account.username = tbUserNm.Text;
            account.salf = salf;
            account.password = pass;


            var jsonAccount = JsonConvert.SerializeObject(account);
            var content = new StringContent(jsonAccount, Encoding.UTF8, "application/json");
            await clients.PutAsync($@"{pathAccount}/{tbUserNm.Text}", content);

            MailMessage mess = new MailMessage("vuongluc2708@gmail.com", emailUser, "Provide a new password", $@"Your new password is: {Newpass} \r\n You can now use this password to log in to the application.");
            SmtpClient client = new SmtpClient("smtp.gmail.com", 587);
            client.EnableSsl = true;

            client.Credentials = new NetworkCredential("vuongluc2708@gmail.com", "lucbeo123");
            client.Send(mess);

            MessageBox.Show("New password has been sent to your email. Please check your email!", "Message");
            this.Close();
        }

        private bool validate()
        {

            HttpResponseMessage response = clients.GetAsync($"{pathAccount}/{tbUserNm.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<AccountDTO>(result);

            var check = true;
            if (tbUserNm.Text.Length == 0)
            {
                errorProvider1.SetError(tbUserNm, "Please enter User Name");
                lbError.Text = "Please enter User Name";
                check = false;
            }
            else if (results == null)
            {
                errorProvider1.Clear();
                errorProvider1.SetError(tbUserNm, "Username is incorrect");
                lbError.Text = "Username is incorrect";
                check = false;
            }
            else
            {
                errorProvider1.Clear();
                lbError.Text = "";

            }
            return check;
        }

        private void ForgotPassword_FormClosing(object sender, FormClosingEventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
        }
    }
}
