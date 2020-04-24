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
    public partial class Register : Form
    {

        string url = @"http://localhost:52593/";
        string pathAccount = @"api/accounts";
        string pathStudent = @"api/students";
        string pathTeacher = @"api/teachers";
        HttpClient client;
       
        TeacherDTO teacher = new TeacherDTO();
        StudentDTO student = new StudentDTO();
        AccountDTO account = new AccountDTO();
        List<string> listIdStudent = null;
        List<string> listIdTeacher = null;
        public Register()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            InitializeComponent();
        }

        private async void Register_Load(object sender, EventArgs e)
        {
            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var dataTeacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            HttpResponseMessage responseStudent = await client.GetAsync($"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var dataStudent = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            cbRoles.SelectedIndex = 0;
            listIdStudent = dataStudent.Select(m => m.StudentId).ToList();
            listIdTeacher = dataTeacher.Select(m => m.TeacherId).ToList();
        }

        private void Register_FormClosing(object sender, FormClosingEventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
        }

        private async void button1_Click(object sender, EventArgs e)
        {
            if (validateRegister())
            {
                if(cbRoles.SelectedIndex == 0)
                {
                    
                    string code = null;
                    string lastFourNumbers = null;
                    string monthYear = null;
                    for (var i = 0; i < ++i; i++)
                    {
                        List<string> list = new List<string> { "00", "01", "11", "10" };
                        Random rnd = new Random();
                        int index = rnd.Next(0, list.Count);
                        code = list[index];
                        lastFourNumbers = "";
                        for (var j = 0; j < 4; j++)
                        {
                            lastFourNumbers += rnd.Next(0, 9).ToString();
                        }
                        monthYear = Convert.ToDateTime(dtpBirthDate.Value).Year.ToString().Substring(2, 2) + Convert.ToDateTime(dtpBirthDate.Value).Month.ToString("00");
                        if (listIdStudent.Contains("S" + code + monthYear + lastFourNumbers))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    student.StudentId = "S" + code + monthYear + lastFourNumbers;
                    student.FirstName = tbSFirstName.Text.Trim();
                    student.LastName = tbSLastName.Text.Trim();
                    student.Contact = tbSContact.Text.Trim();
                    student.BirthDate = Convert.ToDateTime(dtpBirthDate.Value.ToShortDateString());
                    student.StatusId = "SA";

                    var jsonStudent = JsonConvert.SerializeObject(student);
                    var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathStudent, content);

                    var salf = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var pass = BCrypt.Net.BCrypt.HashPassword(tbPassword.Text.Trim(), salf);

                    account.username = "S" + code + monthYear + lastFourNumbers;
                    account.salf = salf;
                    account.password = pass;
                    account.email = tbEmail.Text.Trim();

                    var jsonAccount = JsonConvert.SerializeObject(account);
                    var contentAccount = new StringContent(jsonAccount, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathAccount, contentAccount);

                }
                else if(cbRoles.SelectedIndex == 1)
                {
                    
                    string code = null;
                    string lastFourNumbers = null;
                    string monthYear = null;
                    for (var i = 0; i < ++i; i++)
                    {
                        List<string> list = new List<string> { "00", "01", "11", "10" };
                        Random rnd = new Random();
                        int index = rnd.Next(0, list.Count);
                        code = list[index];
                        lastFourNumbers = "";
                        for (var j = 0; j < 4; j++)
                        {
                            lastFourNumbers += rnd.Next(0, 9).ToString();
                        }
                        monthYear = Convert.ToDateTime(dtpBirthDate.Value).Year.ToString().Substring(2, 2) + Convert.ToDateTime(dtpBirthDate.Value).Month.ToString("00");
                        if (listIdTeacher.Contains("T" + code + monthYear + lastFourNumbers))
                        {
                            continue;
                        }
                        else
                        {
                            break;
                        }
                    }
                    teacher.TeacherId = "T" + code + monthYear + lastFourNumbers;
                    teacher.FirstName = tbSFirstName.Text.Trim();
                    teacher.LastName = tbSLastName.Text.Trim();
                    teacher.Contact = tbSContact.Text.Trim();
                    teacher.BirthDate = Convert.ToDateTime(dtpBirthDate.Value.ToShortDateString());
                    teacher.StatusId = "TA";

                    var jsonTeacher = JsonConvert.SerializeObject(teacher);
                    var contentTeacher = new StringContent(jsonTeacher, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathTeacher, contentTeacher);

                    var salf = BCrypt.Net.BCrypt.GenerateSalt(12);
                    var pass = BCrypt.Net.BCrypt.HashPassword(tbPassword.Text.Trim(), salf);

                    account.username = "T" + code + monthYear + lastFourNumbers;
                    account.salf = salf;
                    account.password = pass;
                    account.email = tbEmail.Text.Trim();

                    var jsonAccount = JsonConvert.SerializeObject(account);
                    var contentAccount = new StringContent(jsonAccount, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathAccount, contentAccount);
                }

                MessageBox.Show("Register Successfully", "Message");
                this.Close();
            }
        }

        private bool validateRegister()
        {
            var check = true;
            if (tbSFirstName.Text == "")
            {
                errorProvider.SetError(tbSFirstName, "Please enter first name");
                lbErrorStudent.Text = "Please enter first name";
                check = false;
            }
            else if (tbSLastName.Text == "")
            {
                errorProvider.Clear();
                errorProvider.SetError(tbSLastName, "Please enter last name");
                lbErrorStudent.Text = "Please enter last name";
                check = false;
            }
            else if (tbSContact.Text == "")
            {
                errorProvider.Clear();
                errorProvider.SetError(tbSContact, "Please enter contact");
                lbErrorStudent.Text = "Please enter contact";
                check = false;
            }else if(tbPassword.Text == "")
            {
                errorProvider.Clear();
                errorProvider.SetError(tbPassword, "Please enter password");
                lbErrorStudent.Text = "Please enter password";
                check = false;
            }
            else if (tbEmail.Text == "")
            {
                errorProvider.Clear();
                errorProvider.SetError(tbPassword, "Please enter email address");
                lbErrorStudent.Text = "Please enter email address";
                check = false;
            }
            else
            {
                errorProvider.Clear();
                lbErrorStudent.Text = "";

            }
            return check;
        }

        
    }
}
