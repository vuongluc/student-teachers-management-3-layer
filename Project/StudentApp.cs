
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
    public partial class StudentApp : Form
    {
        string url = @"http://localhost:52593/";

        string pathModule = @"api/modules";
        string pathStudent = @"api/students";
        string pathTeacher = @"api/teachers";
        string pathClass = @"api/classes";
        string pathEnroll = @"api/enrolls";
        string pathEvalua = @"api/evaluates";
        static HttpClient client;


        StudentDTO student = new StudentDTO();
        EnrollDTO enroll = new EnrollDTO();
        ClassDTO classes = new ClassDTO();
        EvaluateDTO evaluate = new EvaluateDTO();
        string studentIdDefault = null;
        public string studentId;
        List<string> list_classId = null;
        public StudentApp()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            this.MinimumSize = new Size(300, 400);            
            InitializeComponent();
            
        }
        private async void RegisterStudent_Load(object sender, EventArgs e)
        {

            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseEvalua = await client.GetAsync($"{pathEvalua}");
            var resultEvalua = await responseEvalua.Content.ReadAsStringAsync();
            var dataEvalua = JsonConvert.DeserializeObject<List<EvaluateDTO>>(resultEvalua);

            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var dataModule = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);


            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var dataTeacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);


            HttpResponseMessage responseStudent = await client.GetAsync($"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var dataStudent = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            studentIdDefault = studentId;

            list_classId = dataEnroll.Where(m => m.StudentId == studentIdDefault).Select(m => m.ClassId).ToList();
            var list_fullClass = dataClass.Select(m => m.ClassId).ToList();

            foreach (var item in list_classId)
            {
                list_fullClass = list_fullClass.Where(m => m != item).ToList();
                var statusId = dataClass.Where(m => m.ClassId == item).FirstOrDefault().StatusId;
                if (statusId != "CE")
                {
                    list_classId = list_classId.Where(m => m != item).ToList();
                }
                if (dataEvalua.Where(m => m.StudentId + m.ClassId == studentIdDefault + item).FirstOrDefault() != null)
                {
                    list_classId = list_classId.Where(m => m != item).ToList();
                }
            }
            // Register Class
            cbClass.DataSource = list_fullClass;
            lbErrorStudent.Text = "Class " + cbClass.Text + " currently " + (24 - dataEnroll.Where(m => m.ClassId == cbClass.Text).ToList().Count) + " seats available";

            if (cbClass.Items.Count == 0)
            {
                lbErrorStudent.Text = "";
                btnSubmit.Enabled = false;
            }

            cbClassEvalua.DataSource = list_classId;
            if (cbClassEvalua.Items.Count == 0)
            {
                btnSubmitEvaluate.Enabled = false;
            }
            dgvStudyRecords.DataSource = dataEnroll.Where(m => m.StudentId == studentIdDefault).Select(m => new
            {
                ClassId = m.ClassId,
                ModuleId = dataModule.Where(a => a.ModuleId == dataClass.Where(x => x.ClassId == m.ClassId).FirstOrDefault().ModuleId).FirstOrDefault().ModuleName,
                TeacherId = dataTeacher.Where(a => a.TeacherId == dataClass.Where(x => x.ClassId == m.ClassId).FirstOrDefault().TeacherId).FirstOrDefault().FullName,
                Hw1Grade = m.Hw1Grade,
                Hw2Grade = m.Hw2Grade,
                Hw3Grade = m.Hw3Grade,
                Hw4Grade = m.Hw4Grade,
                Hw5Grade = m.Hw5Grade,
                Passed = m.Passed
            }).ToList();
            tbFirstName.Text = dataStudent.Where(m => m.StudentId == studentIdDefault).FirstOrDefault().FirstName;
            tbLastName.Text = dataStudent.Where(m => m.StudentId == studentIdDefault).FirstOrDefault().LastName;
            dtpBirthDate.Text = Convert.ToDateTime(dataStudent.Where(m => m.StudentId == studentIdDefault).FirstOrDefault().BirthDate).ToShortDateString();
            tbContact.Text = dataStudent.Where(m => m.StudentId == studentIdDefault).FirstOrDefault().Contact;

        }

        private void RegisterStudent_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult closeApp = MessageBox.Show("Are you sure you want to exit the program?", "Notification", MessageBoxButtons.YesNo);
            if (closeApp == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        private void RegisterStudent_SizeChanged(object sender, EventArgs e)
        {
            lbTitle.Left = (this.ClientSize.Width - lbTitle.Size.Width) / 2;
        }

       
        private async void btnSubmit_Click(object sender, EventArgs e)
        {

            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);


            enroll.StudentId = studentIdDefault;
            enroll.ClassId = cbClass.Text;
            enroll.Passed = 0;

            var jsonEnroll = JsonConvert.SerializeObject(enroll);
            var contentEnroll = new StringContent(jsonEnroll, Encoding.UTF8, "application/json");
            await client.PostAsync($@"{pathEnroll}", contentEnroll);

            if (dataEnroll.Where(m => m.ClassId == cbClass.Text).ToList().Count == 24)
            {
                var classCureent = dataClass.Where(x => x.ClassId == cbClass.Text).FirstOrDefault();
                classes.ClassId = cbClass.Text;
                classes.ModuleId = classCureent.ModuleId;
                classes.TeacherId = classCureent.TeacherId;
                classes.TypeId = classCureent.TypeId;
                classes.StatusId = "CA";

                var jsonClass = JsonConvert.SerializeObject(classes);
                var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
                await client.PutAsync($@"{pathClass}/{cbClass.Text}", content);
            }
            var class_register = dataEnroll.Where(m => m.StudentId == studentIdDefault).Select(m => m.ClassId).ToList();
            var list_fullClass = dataClass.Select(m => m.ClassId).ToList();

            foreach (string item in class_register)
            {
                list_fullClass = list_fullClass.Where(m => m != item).ToList();
            }
            cbClass.DataSource = list_fullClass;
            if (cbClass.Items.Count == 0)
            {
                lbErrorStudent.Text = "";
                btnSubmit.Enabled = false;
            }
            MessageBox.Show("Register Successfully", "Message");


        }


        void ClearEvalua()
        {
            tbUnderstand.Value = tbTeaching.Value = tbSupport.Value = tbPunctuality.Value = 0;
        }

        private async void btnSubmitEvaluate_Click(object sender, EventArgs e)
        {

            HttpResponseMessage responseEvalua = await client.GetAsync($"{pathEvalua}");
            var resultEvalua = await responseEvalua.Content.ReadAsStringAsync();
            var dataEvalua = JsonConvert.DeserializeObject<List<EvaluateDTO>>(resultEvalua);

            evaluate.ClassId = cbClassEvalua.Text;
            evaluate.StudentId = studentIdDefault;
            evaluate.Understand = tbUnderstand.Text.Trim();
            evaluate.Punctuality = tbPunctuality.Text.Trim();
            evaluate.Support = tbSupport.Text.Trim();
            evaluate.Teaching = tbTeaching.Text.Trim();

            var jsonEvalua = JsonConvert.SerializeObject(evaluate);
            var contentEvalua = new StringContent(jsonEvalua, Encoding.UTF8, "application/json");
            await client.PostAsync($@"{pathEvalua}", contentEvalua);

            ClearEvalua();
            MessageBox.Show("Thanks for your evaluation!", "Message");
            var evalua = dataEvalua.Where(m => m.StudentId + m.ClassId == studentIdDefault + cbClassEvalua.Text).FirstOrDefault();
            if (evalua != null)
            {
                list_classId = list_classId.Where(m => m != cbClassEvalua.Text).ToList();
            }
            cbClassEvalua.DataSource = list_classId;
            if (cbClassEvalua.Items.Count == 0)
            {
                btnSubmitEvaluate.Enabled = false;
            }
        }

        private async void cbClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            lbErrorStudent.Text = "Class " + cbClass.Text + " currently " + (24 - dataEnroll.Where(m => m.ClassId == cbClass.Text).ToList().Count) + " seats available";
        }

        private void StudentApp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
        }

        private void btnChangePass_Click(object sender, EventArgs e)
        {
            ChangePassword frChange = new ChangePassword();
            frChange.idStudent = studentIdDefault;
            this.Hide();
            frChange.ShowDialog();
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClearProfile()
        {
            btnChangeProfile.Text = "Change Profile";
            tbFirstName.ReadOnly = true;
            tbContact.ReadOnly = true;
            dtpBirthDate.Enabled = false;
            tbLastName.ReadOnly = true;
        }
        int i = 0;
        private async void btnChangeProfile_Click(object sender, EventArgs e)
        {   
                     
            i++;
            if(i % 2 == 1)
            {
                btnChangeProfile.Text = "Save";
                tbFirstName.ReadOnly = false;
                tbContact.ReadOnly = false;
                dtpBirthDate.Enabled = true;
                tbLastName.ReadOnly = false;
            }else
            {
                student.StudentId = studentIdDefault;
                student.FirstName = tbFirstName.Text.Trim();
                student.LastName = tbLastName.Text.Trim();
                student.Contact = tbContact.Text.Trim();
                student.BirthDate = Convert.ToDateTime(dtpBirthDate.Value.ToShortDateString());

                var jsonStudent = JsonConvert.SerializeObject(student);
                var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");
                await client.PutAsync($@"{pathStudent}/{studentIdDefault}", content);

                ClearProfile();
                MessageBox.Show("Update Successfully", "Message");
            }
            
        }
    }
}
