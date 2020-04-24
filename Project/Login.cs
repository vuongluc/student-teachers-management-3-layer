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
    public partial class Login : Form
    {
        string url = @"http://localhost:52593/";       
        string pathAccount = @"api/accounts";
        HttpClient client;
        public Login()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            InitializeComponent();
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Register frRegister = new Register();
            this.Hide();
            frRegister.ShowDialog();
        }

        private void Login_FormClosing(object sender, FormClosingEventArgs e)
        {
            Application.Exit();
        }

        private void btnLogin_Click(object sender, EventArgs e)
        {
            if (validateLogin())
            {

                if (tbUserName.Text.StartsWith("S"))
                {

                    StudentApp frSTudent = new StudentApp();
                    frSTudent.studentId = tbUserName.Text.Trim();
                    this.Hide();
                    frSTudent.ShowDialog();
                }
                else if (tbUserName.Text.StartsWith("T"))
                {
                    TeacherApp frTeacher = new TeacherApp();
                    frTeacher.teacherId = tbUserName.Text.Trim();
                    this.Hide();
                    frTeacher.ShowDialog();
                }
                else
                {
                    Admin frAdmin = new Admin();
                    this.Hide();
                    frAdmin.ShowDialog();
                }
            }
        }

        private bool validateLogin()
        {
           
            var check = true;
            if (tbUserName.Text == "")
            {
                errorProvider.SetError(tbUserName, "Please enter User Name");
                lbErrorStudent.Text = "Please enter User Name";
                check = false;
            }
            else if (tbPassWord.Text == "")
            {
                errorProvider.Clear();
                errorProvider.SetError(tbPassWord, "Please enter Password");
                lbErrorStudent.Text = "Please enter Password";
                check = false;
            }
            else
            {
                string username = tbUserName.Text.Trim();

                HttpResponseMessage response =  client.GetAsync($"{pathAccount}").Result;
                var result =  response.Content.ReadAsStringAsync().Result;
                var list_account = JsonConvert.DeserializeObject<List<AccountDTO>>(result);

                var account = list_account.Where(m => m.username == username).FirstOrDefault();
                if (account == null)
                {
                    errorProvider.Clear();
                    errorProvider.SetError(tbUserName, "Username incorrect");
                    lbErrorStudent.Text = "Username incorrect";
                    check = false;
                }
                else
                {
                    var pass = account.password;
                    var salf = account.salf;
                    if (BCrypt.Net.BCrypt.HashPassword(tbPassWord.Text, salf) != pass)
                    {
                        errorProvider.Clear();
                        errorProvider.SetError(tbPassWord, "Password incorrect");
                        lbErrorStudent.Text = "Password incorrect";
                        check = false;
                    }
                }
            }
            return check;
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ForgotPassword frForgot = new ForgotPassword();
            this.Hide();
            frForgot.ShowDialog();
        }
    }
}
