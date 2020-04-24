using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Net.Http;
using System.Net.Http.Headers;
using ConsumerWebAPI.Controllers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Threading;
using Project.DTO;

namespace Project
{
    public partial class Admin : Form
    {
        string url = @"http://localhost:52593/";
        string pathClassType = @"api/classtypes";
        string pathStatus = @"api/status";
        string pathModule = @"api/modules";
        string pathStudent = @"api/students";
        string pathTeacher = @"api/teachers";
        string pathClass = @"api/classes";
        string pathEnroll = @"api/enrolls";
        string pathEvalua = @"api/evaluates";
        string pathCapable = @"api/capables";
        static HttpClient client;

        
        ClassTypesDTO classType = new ClassTypesDTO();
        StatusDTO status = new StatusDTO();
        ModuleDTO module = new ModuleDTO();
        TeacherDTO teacher = new TeacherDTO();
        StudentDTO student = new StudentDTO();
        ClassDTO classes = new ClassDTO();
        EnrollDTO enroll = new EnrollDTO();
        CapableDTO capable = new CapableDTO();
        EvaluateDTO evaluate = new EvaluateDTO();
        List<StudentDTO> list_student = null;
        public Admin()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            InitializeComponent();
        }
        private async void Form1_Load(object sender, EventArgs e)
        {

            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);


            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);


            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var list_Ernoll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            cbModuleName.DataSource = list_module.Select(m => m.ModuleName).ToList();

            chart1.Series["Percent"].XValueMember = "ClassId";
            chart1.Series["Percent"].YValueMembers = "Passed";
            chart1.DataSource = list_Class.Join(list_Ernoll, clas => clas.ClassId, enrol => enrol.ClassId, (clas, enrol) => new
            {
                ClassId = clas.ClassId,
                Passed = enrol.Passed,
                ModuleId = clas.ModuleId
            }).Where(m => list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName == cbModuleName.Text).GroupBy(m => m.ClassId).Select(g => new
            {
                ClassId = g.Key,
                Passed = g.Average(m => Convert.ToInt32(m.Passed)) * 100
            }).ToList();
            chart1.DataBind();
            pbCloseSearchClassType.Visible = false;
            //ClassType
            Clear();
            DisplayDataGirdView();
            //Status
            ClearStatus();
            DataStatus();
            //Module
            ClearModule();
            DataModule();
            //Teacher   


            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var list_status = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);



            cbStatusId.DataSource = list_status.Select(m => m.StatusName).Where(x => x.StartsWith("Teacher")).ToList();

            cbSStatusId.DataSource = list_status.Select(m => m.StatusName).Where(x => x.StartsWith("Student")).ToList();

            cbStatusClass.DataSource = list_status.Select(m => m.StatusName).Where(x => x.StartsWith("Class")).ToList();

            lbModule.ClearSelected();
            ClearTeacher();
            DataTeacher();
            lbModule.DataSource = list_module.Select(x => x.ModuleName).ToList();
            //Student
            HttpResponseMessage response = await client.GetAsync($"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            list_student = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
            ClearStudent();
            DataStudent();
            pageTotal();


            // Class

            HttpResponseMessage responses = await client.GetAsync($"{pathClassType}");
            var results = await responses.Content.ReadAsStringAsync();

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var list_teacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);


            cbModuleClass.DataSource = list_module.Select(m => m.ModuleName).ToList();
            cbTeacherClass.DataSource = list_teacher.Select(x => x.FullName).ToList();
            cbTypeClass.DataSource = JsonConvert.DeserializeObject<List<ClassTypesDTO>>(results).Select(x => x.TypeId).ToList();
            ClearClass();
            DataClass();
            cbStatusClass.SelectedIndex = 3;
            // Enroll
            lbStudent.DataSource = list_student.Select(x => x.FullName).ToList();
            cbClassEnroll.DataSource = list_Class.Select(m => m.ClassId).ToList();
            ClearEnroll();
            DataEnroll();

            // Evaluate

            cbClassEvaluate.DataSource = list_Class.Where(m => m.StatusId == "CE").Select(m => m.ClassId).ToList();
            var list_IdStudent = list_Ernoll.Where(m => m.ClassId == cbClassEnroll.Text).Select(m => m.StudentId).ToList();
            List<string> student = new List<string>();
            foreach (var item in list_IdStudent)
            {
                var name = list_student.Where(x => x.StudentId == item).FirstOrDefault().FullName;
                student.Add(name);
            }
            cbStudentEvaluate.DataSource = student;
            ClearEvalueate();
            DataEvaluate();
            chart1.Titles.Add("Pass percentage of modules");
            chart2.Titles.Add("Average exam grades of modules");
            chart3.Titles.Add("The number of students of modules");
        }
        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult closeApp = MessageBox.Show("Are you sure you want to exit the program?", "Notification", MessageBoxButtons.YesNo);
            if (closeApp == DialogResult.No)
            {
                e.Cancel = true;
            }

        }
        private void Admin_FormClosed(object sender, FormClosedEventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
        }
        /*
         * "S01#{year(field("BirthDate")) % 100}#{month(field("BirthDate"), true)}#{random(0, 9)}#{random(0, 9)}#{random(0, 9)}#{random(0, 9)}"      
        */

        ////////////////////////////////////////////////////////////////////////////
        // Class Type
        // Reuse method
        void Clear()
        {
            tbTypeId.Text = tbTeachingTime.Text = "";
            btnDelete.Enabled = false;
            btnUpdate.Enabled = false;
            btnCreate.Enabled = true;
            btnNew.Text = "Reset";
        }
        private async void DisplayDataGirdView()
        {
            dgwClassType.AutoGenerateColumns = false;

            HttpResponseMessage response = await client.GetAsync($"{pathClassType}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<ClassTypesDTO>>(result);
            dgwClassType.DataSource = results.ToList();
        }
        void dataBidingsClassType()
        {
            errorTypeId.Clear();
            errorTypeId.SetError(tbTypeId, null);
            errorTeachingTime.Clear();
            errorTeachingTime.SetError(tbTeachingTime, null);
            tbTypeId.DataBindings.Clear();
            tbTypeId.DataBindings.Add("Text", dgwClassType.DataSource, "TypeId");
            tbTeachingTime.DataBindings.Clear();
            tbTeachingTime.DataBindings.Add("Text", dgwClassType.DataSource, "TeachingTime");
            btnCreate.Enabled = false;
            btnDelete.Enabled = true;
            btnUpdate.Enabled = true;
            tbTypeId.Enabled = false;
        }
        //Form method
        private async void btnCreate_Click(object sender, EventArgs e)
        {
            if (validateTypeId() && validateTeachingTime())
            {

                errorTypeId.Clear();
                errorTeachingTime.Clear();
                classType.TypeId = tbTypeId.Text.Trim().ToUpper();
                classType.TeachingTime = tbTeachingTime.Text.Trim();

                var jsonClassType = JsonConvert.SerializeObject(classType);
                var content = new StringContent(jsonClassType, Encoding.UTF8, "application/json");
                await client.PostAsync(pathClassType, content);

                Clear();
                DisplayDataGirdView();
                MessageBox.Show("Create Successfully", "Message");


            }

        }
        private async void btnUpdate_Click(object sender, EventArgs e)
        {
            if (validateTeachingTime())
            {

                errorTypeId.Clear();
                errorTeachingTime.Clear();
                classType.TypeId = tbTypeId.Text.Trim();
                classType.TeachingTime = tbTeachingTime.Text.Trim();

                var jsonClassType = JsonConvert.SerializeObject(classType);
                var content = new StringContent(jsonClassType, Encoding.UTF8, "application/json");
                var result = await client.PutAsync($@"{pathClassType}/{tbTypeId.Text}", content);

                Clear();
                DisplayDataGirdView();
                tbTypeId.Enabled = true;
                btnCreate.Enabled = true;
                MessageBox.Show("Update Successfully", "Message");
            }

        }
        private async void btnDelete_Click(object sender, EventArgs e)
        {
            string typeId = dgwClassType.Rows[dgwClassType.CurrentRow.Index].Cells["TypeId"].Value.ToString();

            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await client.DeleteAsync($@"{pathClassType}/{tbTypeId.Text}");

                Clear();
                DisplayDataGirdView();
                tbTypeId.Enabled = true;
                btnCreate.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //Clear();
                //DisplayDataGirdView();
                //tbTypeId.Enabled = true;
                //btnCreate.Enabled = true;
            }
        }
        private void dgwClassType_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            errorTypeId.Clear();
            errorTeachingTime.Clear();
            lbError.Text = "";
            if ((tbTypeId.Text != "" || tbTeachingTime.Text != "") && btnUpdate.Enabled == false)
            {
                DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
                if (mess == DialogResult.OK)
                {
                    btnNew.Text = "Cancel";
                    dataBidingsClassType();
                }
            }
            else
            {
                btnNew.Text = "Cancel";
                dataBidingsClassType();
            }
        }
        private void btnNew_Click(object sender, EventArgs e)
        {
            Clear();
            DisplayDataGirdView();
            errorTypeId.Clear();
            errorTypeId.SetError(tbTypeId, null);
            errorTeachingTime.Clear();
            errorTypeId.SetError(tbTeachingTime, null);
            tbTypeId.Enabled = true;
        }
        private void tbSearch_TextChanged(object sender, EventArgs e)
        {

            if (tbSearch.Text != "")
            {
                pbCloseSearchClassType.Visible = true;
            }
            else
            {
                pbCloseSearchClassType.Visible = false;
            }
            HttpResponseMessage response = client.GetAsync($@"{pathClassType}/{tbSearch.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<ClassTypesDTO>>(result);
            dgwClassType.DataSource = results.ToList();
        }
        private void pbClose_Click(object sender, EventArgs e)
        {
            tbSearch.Text = "";
            pbCloseSearchClassType.Visible = false;
        }
        private void dgwClassType_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgwClassType.ClearSelection();
        }
        //Validate Class Type
        private bool validateTypeId()
        {
            HttpResponseMessage response = client.GetAsync($"{pathClassType}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<ClassTypesDTO>>(result);
            var listID = results.Select(m => m.TypeId).ToList();
            var check = true;
            if (tbTypeId.Text == "")
            {
                errorTypeId.SetError(tbTypeId, "Please enter Type ID");
                lbError.Text = "Please enter Type ID";
                check = false;
            }
            else if (new Regex(@"^([a-zA-Z]){1}$").IsMatch(tbTypeId.Text) == false)
            {
                errorTypeId.SetError(tbTypeId, "Type ID only 1 letter can be entered");
                lbError.Text = "Type ID only 1 letter can be entered";
                check = false;
            }
            else if (listID.Contains(tbTypeId.Text.ToUpper()))
            {
                errorTypeId.SetError(tbTypeId, "Type ID already exist please enter again");
                lbError.Text = "Type ID already exist please enter again";
                check = false;
            }
            else
            {
                errorTypeId.Clear();
                errorTypeId.SetError(tbTypeId, null);
                lbError.Text = "";

            }
            return check;
        }
        private bool validateTeachingTime()
        {
            var check = true;
            if (tbTeachingTime.Text == "")
            {
                errorTeachingTime.SetError(tbTeachingTime, "Please enter Teaching Time");
                lbError.Text = "Please enter Teaching Time";
                check = false;
            }
            else if (new Regex(@"([0-9]{4})(\s-\s)([0-9]{4})$").IsMatch(tbTeachingTime.Text) == false)
            {
                errorTeachingTime.SetError(tbTeachingTime, "Please enter the correct format. Eg 8000 - 1200");
                lbError.Text = "Please enter the correct format. Eg 8000 - 1200";
                check = false;
            }
            else
            {
                errorTeachingTime.Clear();
                errorTypeId.SetError(tbTeachingTime, null);
                lbError.Text = "";
            }
            return check;
        }





        ////////////////////////////////////////////////////////////////////////////       
        // Status
        // Reuse method
        async void DataStatus()
        {
            dgvStatus.AutoGenerateColumns = false;
            HttpResponseMessage response = await client.GetAsync($"{pathStatus}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<StatusDTO>>(result);
            dgvStatus.DataSource = results.ToList();
        }
        void ClearStatus()
        {
            tbStatusId.Text = tbDescription.Text = tbStatusName.Text = "";
            btnUpdateStatus.Enabled = false;
            btnDeleteStatus.Enabled = false;
            btnNewStatus.Text = "Reset";
        }
        void dataBindingsStatus()
        {
            tbStatusId.DataBindings.Clear();
            tbStatusId.DataBindings.Add("Text", dgvStatus.DataSource, "StatusId");
            tbDescription.DataBindings.Clear();
            tbDescription.DataBindings.Add("Text", dgvStatus.DataSource, "Description");
            tbStatusName.DataBindings.Clear();
            tbStatusName.DataBindings.Add("Text", dgvStatus.DataSource, "StatusName");
            btnCreateStatus.Enabled = false;
            btnDeleteStatus.Enabled = true;
            btnUpdateStatus.Enabled = true;
            tbStatusId.Enabled = false;
        }
        // Form method
        private async void btnCreateStatus_Click(object sender, EventArgs e)
        {
            if (validateStatusId() && validateStatusName())
            {
                errorStatusId.Clear();
                errorStatusName.Clear();
                status.StatusId = tbStatusId.Text.Trim().ToUpper();
                status.Description = tbDescription.Text.Trim();
                status.StatusName = tbStatusName.Text.Trim();

                var jsonStatus = JsonConvert.SerializeObject(status);
                var content = new StringContent(jsonStatus, Encoding.UTF8, "application/json");
                await client.PostAsync(pathStatus, content);

                ClearStatus();
                DataStatus();
                MessageBox.Show("Create Successfully", "Message");
            }

        }
        private async void btnUpdateStatus_Click(object sender, EventArgs e)
        {
            if (validateStatusName())
            {
                errorStatusId.Clear();
                errorStatusName.Clear();
                status.StatusId = tbStatusId.Text.Trim();
                status.Description = tbDescription.Text.Trim();
                status.StatusName = tbStatusName.Text.Trim();

                var jsonClassType = JsonConvert.SerializeObject(status);
                var content = new StringContent(jsonClassType, Encoding.UTF8, "application/json");
                var result = await client.PutAsync($"{pathStatus}/{tbStatusId.Text}", content);

                ClearStatus();
                DataStatus();
                tbStatusId.Enabled = true;
                btnCreateStatus.Enabled = true;
                MessageBox.Show("Update Successfully", "Message");
            }

        }
        private async void btnDeleteStatus_Click(object sender, EventArgs e)
        {
            string statusId = dgvStatus.Rows[dgvStatus.CurrentRow.Index].Cells["StatusId"].Value.ToString();

            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {

                await client.DeleteAsync($@"{pathStatus}/{tbStatusId.Text}");

                ClearStatus();
                DataStatus();
                tbStatusId.Enabled = true;
                btnCreateStatus.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //ClearStatus();
                //DataStatus();
            }
            //tbStatusId.Enabled = true;
            //btnCreateStatus.Enabled = true;
        }
        private void dgvStatus_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            errorStatusId.Clear();
            errorStatusName.Clear();
            lbErrorStatus.Text = "";
            if ((tbStatusId.Text != "" || tbStatusName.Text != "" || tbDescription.Text != "") && btnUpdateStatus.Enabled == false)
            {
                DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
                if (mess == DialogResult.OK)
                {
                    btnNewStatus.Text = "Cancel";
                    dataBindingsStatus();
                }
            }
            else
            {
                btnNewStatus.Text = "Cancel";
                dataBindingsStatus();
            }
        }
        private void btnNewStatus_Click(object sender, EventArgs e)
        {
            ClearStatus();
            DataStatus();
            tbStatusId.Enabled = true;
            btnCreateStatus.Enabled = true;
            errorImportStatus.Clear();
        }
        private void tbSearchStatus_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchStatus.Text != "")
            {
                pbCloseSearchStatus.Visible = true;
            }
            else
            {
                pbCloseSearchStatus.Visible = false;
            }
            HttpResponseMessage response = client.GetAsync($@"{pathStatus}/{tbSearchStatus.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<StatusDTO>>(result);
            dgvStatus.DataSource = results.ToList();
        }
        private void pbSClose_Click(object sender, EventArgs e)
        {
            tbSearchStatus.Text = "";
        }
        private void dgvStatus_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvStatus.ClearSelection();
        }
        // Validate Status
        private bool validateStatusId()
        {
            HttpResponseMessage response =  client.GetAsync($"{pathStatus}").Result;
            var result =  response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<StatusDTO>>(result);
            var listID = results.Select(m => m.StatusId).ToList();
            var check = true;
            if (tbStatusId.Text == "")
            {
                errorStatusId.SetError(tbStatusId, "Please enter Status ID");
                lbErrorStatus.Text = "Please enter Status ID";
                check = false;
            }
            else if (new Regex(@"^([a-zA-Z]){2}$").IsMatch(tbStatusId.Text) == false)
            {
                errorStatusId.SetError(tbStatusId, "Status ID can only enter 2 uppercase and lowercase letters");
                lbErrorStatus.Text = "Status ID can only enter 2 uppercase and lowercase letters";
                check = false;
            }
            else if (listID.Contains(tbStatusId.Text.ToUpper()))
            {
                errorStatusId.SetError(tbStatusId, "Status ID already exist please enter again");
                lbErrorStatus.Text = "Status ID already exist please enter again";
                check = false;
            }
            else
            {
                errorStatusId.Clear();
                errorStatusId.SetError(tbStatusId, null);
                lbErrorStatus.Text = "";
            }
            return check;
        }
        private bool validateStatusName()
        {
            var check = true;
            if (tbStatusName.Text == "")
            {
                errorStatusName.SetError(tbStatusName, "Please enter Status Name");
                lbErrorStatus.Text = "Please enter Status Name";
                check = false;
            }
            else
            {
                errorStatusName.Clear();
                errorStatusName.SetError(tbStatusName, null);
                lbErrorStatus.Text = "";
            }
            return check;
        }


        ////////////////////////////////////////////////////////////////////////////  
        //Module
        // Reuse method
        async void DataModule()
        {
            dgvModule.AutoGenerateColumns = false;
            HttpResponseMessage response = await client.GetAsync($"{pathModule}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<ModuleDTO>>(result);
            dgvModule.DataSource = results.ToList();
        }
        void ClearModule()
        {
            tbModuleId.Text = tbDuration.Text = tbModuleName.Text = tbHomeWork.Text = "";
            btnUpdateModule.Enabled = false;
            btnDeleteModule.Enabled = false;
            btnNewModule.Text = "Reset";
            lbModule.SelectedIndex = -1;
        }
        void databindingsModule()
        {
            tbModuleId.DataBindings.Clear();
            tbModuleId.DataBindings.Add("Text", dgvModule.DataSource, "ModuleId");

            tbDuration.DataBindings.Clear();
            string duration = dgvModule.Rows[dgvModule.CurrentRow.Index].Cells["Duration"].Value.ToString();
            //tbDuration.DataBindings.Add("Text", dgvModule.DataSource, "Duration");
            tbDuration.Text = duration;

            tbModuleName.DataBindings.Clear();
            tbModuleName.DataBindings.Add("Text", dgvModule.DataSource, "ModuleName");

            tbHomeWork.DataBindings.Clear();
            string homeWork = dgvModule.Rows[dgvModule.CurrentRow.Index].Cells["HomeWork"].Value.ToString();
            //tbHomeWork.DataBindings.Add("Text", dgvModule.DataSource, "Homework");
            tbHomeWork.Text = homeWork;

            btnCreateModule.Enabled = false;
            btnDeleteModule.Enabled = true;
            btnUpdateModule.Enabled = true;
            tbModuleId.Enabled = false;
        }
        // Form method
        private async void btnCreateModule_Click(object sender, EventArgs e)
        {
            if (validateModuleId() && validateModule())
            {
                errorModuleId.Clear();
                errorModuleName.Clear();
                errorDuration.Clear();
                errorHomeWork.Clear();
                module.ModuleId = tbModuleId.Text.Trim().ToUpper();
                module.Duration = Convert.ToByte(tbDuration.Text.Trim());
                module.ModuleName = tbModuleName.Text.Trim();
                module.Homework = Convert.ToByte(tbHomeWork.Text.Trim());

                var jsonModule = JsonConvert.SerializeObject(module);
                var content = new StringContent(jsonModule, Encoding.UTF8, "application/json");
                await client.PostAsync(pathModule, content);

                ClearModule();
                DataModule();
                MessageBox.Show("Create Successfully", "Message");
            }

        }
        private async void btnUpdateModule_Click(object sender, EventArgs e)
        {
            if (validateModule())
            {
                errorHomeWork.Clear();
                errorModuleId.Clear();
                errorModuleName.Clear();
                errorDuration.Clear();
                module.ModuleId = tbModuleId.Text.Trim();
                module.Duration = Convert.ToByte(tbDuration.Text);
                module.ModuleName = tbModuleName.Text.Trim();
                module.Homework = Convert.ToByte(tbHomeWork.Text);

                var jsonModule = JsonConvert.SerializeObject(module);
                var content = new StringContent(jsonModule, Encoding.UTF8, "application/json");
                await client.PutAsync($@"{pathModule}/{tbModuleId.Text}", content);

                ClearModule();
                DataModule();
                tbModuleId.Enabled = true;
                btnCreateModule.Enabled = true;
                MessageBox.Show("Update Successfully", "Message");
            }

        }
        private async void btnDeleteModule_Click(object sender, EventArgs e)
        {
            string moduleId = dgvModule.Rows[dgvModule.CurrentRow.Index].Cells["ModuleId"].Value.ToString();

            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await client.DeleteAsync($@"{pathModule}/{tbModuleId.Text}");

                ClearModule();
                DataModule();
                tbModuleId.Enabled = true;
                btnCreateModule.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //ClearModule();
                //DataModule();
            }
            //tbStatusId.Enabled = true;
            //btnCreateModule.Enabled = true;
        }
        private void dgvModule_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            errorModuleId.Clear();
            errorModuleName.Clear();
            errorDuration.Clear();
            errorHomeWork.Clear();
            if ((tbModuleId.Text != "" || tbModuleName.Text != "" || tbDuration.Text != "" || tbHomeWork.Text != "") && btnUpdateModule.Enabled == false)
            {
                DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
                if (mess == DialogResult.OK)
                {
                    btnNewModule.Text = "Cancel";
                    databindingsModule();
                }
            }
            else
            {
                btnNewModule.Text = "Cancel";
                databindingsModule();
            }
        }
        private void btnNewModule_Click(object sender, EventArgs e)
        {
            ClearModule();
            DataModule();
            tbModuleId.Enabled = true;
            btnCreateModule.Enabled = true;
            errorImportModule.Clear();
        }
        private void pbCloseSearchModule_Click(object sender, EventArgs e)
        {
            tbSearchModule.Text = "";
        }
        private void tbSearchModule_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchModule.Text != "")
            {
                pbCloseSearchModule.Visible = true;
            }
            else
            {
                pbCloseSearchModule.Visible = false;
            }
            HttpResponseMessage response = client.GetAsync($@"{pathModule}/{tbSearchModule.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<ModuleDTO>>(result);
            dgvModule.DataSource = results.ToList();
        }
        private void dgvModule_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvModule.ClearSelection();
        }
        // Validate Module

        private bool validateModuleId()
        {
            HttpResponseMessage responseModule = client.GetAsync($"{pathModule}").Result;
            var resultModule = responseModule.Content.ReadAsStringAsync().Result;
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            var listID = list_module.Select(m => m.ModuleId).ToList();
            var check = true;
            if (tbModuleId.Text == "")
            {
                errorModuleId.SetError(tbModuleId, "Please enter Module ID");
                lbErrorModule.Text = "Please enter Module ID";
                check = false;
            }
            else if (new Regex(@"^([a-zA-Z]){1,5}$").IsMatch(tbModuleId.Text) == false)
            {
                errorModuleId.SetError(tbModuleId, "Module ID can only enter up to 5 uppercase and lowercase letters");
                lbErrorModule.Text = "Module ID can only enter up to 5 uppercase and lowercase letters";
                check = false;
            }
            else if (listID.Contains(tbModuleId.Text.ToUpper()))
            {
                errorModuleId.SetError(tbModuleId, "Module ID already exist please enter again");
                lbErrorModule.Text = "Module ID already exist please enter again";
                check = false;
            }
            else
            {
                errorModuleId.Clear();
                errorModuleId.SetError(tbModuleId, null);
                lbErrorModule.Text = "P";

            }
            return check;
        }
        private bool validateModule()
        {
            var check = true;
            if (tbDuration.Text == "")
            {
                errorDuration.SetError(tbDuration, "Please enter Duration");
                lbErrorModule.Text = "Please enter Duration";
                check = false;
            }
            else if (new Regex(@"^([0-9]){2}$").IsMatch(tbDuration.Text) == false)
            {
                errorDuration.SetError(tbDuration, "Duration only 2 digits can be entered");
                lbErrorModule.Text = "Duration only 2 digits can be entered";
                check = false;
            }
            else if (tbModuleName.Text == "")
            {
                errorDuration.Clear();
                errorModuleName.SetError(tbModuleName, "Please enter Module Name");
                lbErrorModule.Text = "Please enter Module Name";
                check = false;
            }
            else if (tbHomeWork.Text == "")
            {
                errorModuleName.Clear();
                errorHomeWork.SetError(tbHomeWork, "Please enter Home Work");
                lbErrorModule.Text = "Please enter Home Work";
                check = false;
            }
            else if (new Regex(@"^([0-9]){1,2}$").IsMatch(tbHomeWork.Text) == false)
            {
                errorHomeWork.SetError(tbHomeWork, "HomeWork only enter up to 2 letters");
                lbErrorModule.Text = "HomeWork only enter up to 2 letters";
                check = false;
            }
            else
            {
                errorDuration.Clear();
                errorDuration.Clear();
                errorHomeWork.Clear();
                lbErrorModule.Text = "";
            }
            return check;
        }




        ////////////////////////////////////////////////////////////////////////////  
        // Teacher
        // Reuse mothod
        void ClearTeacher()
        {
            tbTeacherId.Text = tbLastName.Text = tbFirstName.Text = tbContact.Text = "";
            dtpBirthDate.Text = DateTime.Now.ToShortDateString();
            cbStatusId.SelectedIndex = 0;
            btnUpdateTeacher.Enabled = false;
            btnDeleteTeacher.Enabled = false;
            btnImportTeacher.Enabled = false;
            btnNewTeacher.Text = "Reset";
            lbModule.SelectedIndex = -1;

        }
        async void DataTeacher()
        {
            dgvTeacher.AutoGenerateColumns = false;
            HttpResponseMessage response = await client.GetAsync($"{pathTeacher}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<TeacherDTO>>(result);
            dgvTeacher.DataSource = results.ToList();
        }
        async void databindingsTeacher()
        {
            HttpResponseMessage response = await client.GetAsync($"{pathStatus}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<StatusDTO>>(result);

            tbTeacherId.DataBindings.Clear();
            tbTeacherId.DataBindings.Add("Text", dgvTeacher.DataSource, "TeacherId");
            tbFirstName.DataBindings.Clear();
            tbFirstName.DataBindings.Add("Text", dgvTeacher.DataSource, "FirstName");
            tbLastName.DataBindings.Clear();
            tbLastName.DataBindings.Add("Text", dgvTeacher.DataSource, "LastName");
            tbContact.DataBindings.Clear();
            tbContact.DataBindings.Add("Text", dgvTeacher.DataSource, "Contact");
            dtpBirthDate.DataBindings.Clear();
            dtpBirthDate.DataBindings.Add("Text", dgvTeacher.DataSource, "BirthDate");
            cbStatusId.DataBindings.Clear();
            string StatusId = dgvTeacher.Rows[dgvTeacher.CurrentRow.Index].Cells["StatusIDs"].Value.ToString();
            cbStatusId.DataBindings.Add("Text", results.Where(m => m.StatusId == StatusId).FirstOrDefault(), "StatusName");

            btnCreateTeacher.Enabled = false;
            btnDeleteTeacher.Enabled = true;
            btnUpdateTeacher.Enabled = true;
            tbTeacherId.Enabled = false;
        }
        // Form method
        private async void btnCreateTeacher_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            List<string> modules = new List<string>();
            foreach (var module in lbModule.SelectedItems)
            {
                modules.Add(module.ToString());
            }
            if (validateTeacherId() && validateTeacher())
            {
                teacher.TeacherId = tbTeacherId.Text.Trim();
                teacher.FirstName = tbFirstName.Text.Trim();
                teacher.LastName = tbLastName.Text.Trim();
                teacher.Contact = tbContact.Text.Trim();
                teacher.BirthDate = Convert.ToDateTime(dtpBirthDate.Value.ToShortDateString());

                var teachers = dataStatus.Where(x => x.StatusName == cbStatusId.Text).FirstOrDefault();
                if (teachers != null)
                {
                    teacher.StatusId = teachers.StatusId;
                }


                var jsonTeacher = JsonConvert.SerializeObject(teacher);
                var content = new StringContent(jsonTeacher, Encoding.UTF8, "application/json");
                await client.PostAsync(pathTeacher, content);


                HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
                var resultModule = await responseModule.Content.ReadAsStringAsync();
                var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

                foreach (string module in modules)
                {
                    var moduleId = list_module.Where(m => m.ModuleName == module).FirstOrDefault();
                    capable.ModuleId = moduleId.ModuleId;
                    capable.TeacherId = tbTeacherId.Text;

                    var jsonCapable = JsonConvert.SerializeObject(capable);
                    var contents = new StringContent(jsonCapable, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathCapable, contents);

                }
                //teacher.StatusId = cbStatusId.Text.Trim();

                ClearTeacher();
                DataTeacher();
                MessageBox.Show("Create Successfully", "Message");
            }

        }
        private async void btnUpdateTeacher_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            List<string> modules = new List<string>();
            foreach (object module in lbModule.SelectedItems)
            {
                modules.Add(module.ToString());
            }

            await client.DeleteAsync($@"{pathCapable}/{tbTeacherId.Text}");
            await client.DeleteAsync($@"{pathTeacher}/{tbTeacherId.Text}");

            if (validateTeacher())
            {
                teacher.TeacherId = tbTeacherId.Text.Trim();
                teacher.FirstName = tbFirstName.Text.Trim();
                teacher.LastName = tbLastName.Text.Trim();
                teacher.Contact = tbContact.Text.Trim();
                teacher.BirthDate = Convert.ToDateTime(dtpBirthDate.Value.ToShortDateString());

                var teachers = dataStatus.Where(x => x.StatusName == cbStatusId.Text).FirstOrDefault();
                if (teachers != null)
                {
                    teacher.StatusId = teachers.StatusId;
                }


                var jsonTeacher = JsonConvert.SerializeObject(teacher);
                var content = new StringContent(jsonTeacher, Encoding.UTF8, "application/json");
                await client.PostAsync(pathTeacher, content);

                HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
                var resultModule = await responseModule.Content.ReadAsStringAsync();
                var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

                foreach (string module in modules)
                {
                    var moduleId = list_module.Where(m => m.ModuleName == module).FirstOrDefault();
                    capable.ModuleId = moduleId.ModuleId;
                    capable.TeacherId = tbTeacherId.Text;
                    var jsonCapable = JsonConvert.SerializeObject(capable);
                    var contents = new StringContent(jsonCapable, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathCapable, contents);

                }
                lbModule.SelectedIndex = -1;
                ClearTeacher();
                DataTeacher();
                tbTeacherId.Enabled = true;
                btnCreateTeacher.Enabled = true;
                MessageBox.Show("Update Successfully", "Message");
            }

        }
        private async void btnDeleteTeacher_Click(object sender, EventArgs e)
        {
            string teacherID = dgvTeacher.Rows[dgvTeacher.CurrentRow.Index].Cells["TeacherId"].Value.ToString();
            HttpResponseMessage responseCapable = await client.GetAsync($"{pathCapable}");
            var resultCapable = await responseCapable.Content.ReadAsStringAsync();
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);


            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                var capableId = dataCapable.Where(m => m.TeacherId == tbTeacherId.Text).FirstOrDefault();
                //var classId = bizClass.findAllClass().Where(m => m.TeacherId == teacherID).FirstOrDefault().ClassId;
                //if(classId != null)
                //{

                //}
                await client.DeleteAsync($@"{pathCapable}/{tbTeacherId.Text}");
                await client.DeleteAsync($@"{pathTeacher}/{teacherID}");
                ClearTeacher();
                DataTeacher();
                tbTeacherId.Enabled = true;
                btnCreateTeacher.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //ClearTeacher();
                //DataTeacher();
            }
            //tbTeacherId.Enabled = true;
            //btnCreateTeacher.Enabled = true;
        }
        private async void dgvTeacher_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            string teacherId = dgvTeacher.Rows[dgvTeacher.CurrentRow.Index].Cells["TeacherId"].Value.ToString();


            HttpResponseMessage responseCapable = await client.GetAsync($"{pathCapable}/{teacherId}");
            var resultCapable = await responseCapable.Content.ReadAsStringAsync();
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);

            lbModule.SelectedIndex = -1;
            var list = dataCapable;
            if (list != null)
            {
                HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
                var resultModule = await responseModule.Content.ReadAsStringAsync();
                var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

                List<string> module_name = new List<string>();
                foreach (var item in list)
                {
                    var moduleName = list_module.Where(x => x.ModuleId == item.ModuleId).FirstOrDefault().ModuleName;
                    module_name.Add(moduleName);
                }
                for (int i = 0; i < lbModule.Items.Count; i++)
                {
                    for (int j = 0; j < module_name.Count; j++)
                    {
                        if (lbModule.Items[i].ToString() == module_name[j])
                        {
                            lbModule.SetSelected(i, true);
                        }
                    }
                }
            }

            errorTeacherId.Clear();
            errorFirstNameTeacher.Clear();
            errorLastNameTeacher.Clear();
            errorContactTeacher.Clear();
            lbErrorTeacher.Text = "";
            if ((tbTeacherId.Text != "" || tbLastName.Text != "" || tbFirstName.Text != "" || tbContact.Text != "") && btnUpdateTeacher.Enabled == false)
            {
                DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
                if (mess == DialogResult.OK)
                {
                    btnNewTeacher.Text = "Cancel";
                    databindingsTeacher();

                }
            }
            else
            {
                btnNewTeacher.Text = "Cancel";
                databindingsTeacher();
            }
        }
        private void btnNewTeacher_Click(object sender, EventArgs e)
        {
            ClearTeacher();
            DataTeacher();
            tbTeacherId.Enabled = true;
            btnCreateTeacher.Enabled = true;
            errorImportTeacher.Clear();
            tbFileTeacher.Text = "";
            lbModule.SelectedIndex = -1;
        }
        private void pbCloseSearchTeacher_Click(object sender, EventArgs e)
        {
            tbSearchTeacher.Text = "";
        }
        private void tbSearchTeacher_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchTeacher.Text != "")
            {
                pbCloseSearchTeacher.Visible = true;
            }
            else
            {
                pbCloseSearchTeacher.Visible = false;
            }
            HttpResponseMessage response = client.GetAsync($@"{pathTeacher}/{tbSearchTeacher.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<List<TeacherDTO>>(result);
            dgvTeacher.DataSource = results.ToList();
        }
        private void dgvTeacher_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvTeacher.ClearSelection();
        }
        // Validate Teacher
        private void btnOpenFileTeacher_Click(object sender, EventArgs e)
        {
            openFileTeacher.Filter = "csv files (*.csv)|*.csv";
            openFileTeacher.ShowDialog();
            var filename = Path.GetFileName(openFileTeacher.FileName);
            tbFileTeacher.Text = filename;
            if (tbFileTeacher.Text != "")
            {
                btnImportTeacher.Enabled = true;
            }
            else
            {
                btnImportTeacher.Enabled = false;
            }
        }
        private bool validateImportTeacher()
        {
            HttpResponseMessage responseTeacher = client.GetAsync($"{pathTeacher}").Result;
            var resultTeacher = responseTeacher.Content.ReadAsStringAsync().Result;
            var list_teacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            var check = true;
            var filename = Path.GetFileName(openFileTeacher.FileName);
            var path = Path.GetDirectoryName(openFileTeacher.FileName);
            var fullPath = path + "\\" + filename;
            var listId = list_teacher.Select(m => m.TeacherId).ToList();

            StreamReader streamCsv = new StreamReader(fullPath);

            string csvDataLine = "";
            string[] data = null;
            var lineHeader = streamCsv.ReadLine();
            int count = 0;
            while ((csvDataLine = streamCsv.ReadLine()) != null)
            {
                data = csvDataLine.Split(',');
                if (listId.Contains(data[0]))
                {
                    count = 1;
                }
            }
            if (count == 1)
            {
                errorImportTeacher.SetError(tbFileTeacher, "File there is data TeacherId already exists");
                check = false;
            }
            else if (Path.GetExtension(tbFileTeacher.Text) != ".csv")
            {
                errorImportTeacher.SetError(tbFileTeacher, "Please select the file with the .csv extension");
                check = false;
            }
            return check;

        }
        private void btnImportTeacher_Click(object sender, EventArgs e)
        {
            if (validateImportTeacher())
            {
                errorImportTeacher.Clear();
                var filename = Path.GetFileName(openFileTeacher.FileName);
                var path = Path.GetDirectoryName(openFileTeacher.FileName);
                var fullPath = path + "\\" + filename;
                //bizTeacher.importData(fullPath);
                tbFileTeacher.Text = "";
                DataTeacher();
                MessageBox.Show("Import successfully", "Message", MessageBoxButtons.OK);
                btnImportTeacher.Enabled = false;
            }
        }
        private bool validateTeacherId()
        {
            HttpResponseMessage responseTeacher = client.GetAsync($"{pathTeacher}").Result;
            var resultTeacher = responseTeacher.Content.ReadAsStringAsync().Result;
            var list_teacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            var year = dtpBirthDate.Value.Year.ToString().Substring(2, 2);
            var month = "";
            if (dtpBirthDate.Value.Month < 10)
            {
                month += "0" + dtpBirthDate.Value.Month;
            }
            else
            {
                month = dtpBirthDate.Value.Month.ToString();
            }
            var listID = list_teacher.Select(m => m.TeacherId).ToList();
            var check = true;
            if (tbTeacherId.Text == "")
            {
                errorTeacherId.SetError(tbTeacherId, "Please enter Teacher ID");
                lbErrorTeacher.Text = "Please enter Teacher ID";
                check = false;
            }
            else if (new Regex("^[T]([0-1]{2})(" + year + month + ")([0-9]{4})$").IsMatch(tbTeacherId.Text) == false)
            {
                errorTeacherId.SetError(tbTeacherId, "Teacher Id must start with the letter 'T' then 01 or 00 or 11 or 10 or 2 digits of the year and 2 digits of the month and 4 random digits");
                lbErrorTeacher.Text = "Teacher Id must start with the letter 'T' then 01 or 00 or 11 or 10 or 2 digits of the year and 2 digits of the month and 4 random digits";
                check = false;
            }
            else if (listID.Contains(tbTeacherId.Text))
            {
                errorTeacherId.SetError(tbModuleId, "Teacher ID already exist please enter again");
                lbErrorTeacher.Text = "Teacher ID already exist please enter again";
                check = false;
            }
            else
            {
                errorTeacherId.Clear();
                lbErrorTeacher.Text = "";

            }
            return check;
        }
        private bool validateTeacher()
        {
            var check = true;
            if (tbFirstName.Text == "")
            {
                errorFirstNameTeacher.SetError(tbFirstName, "Please enter first name");
                lbErrorTeacher.Text = "Please enter first name";
                check = false;
            }
            else if (tbLastName.Text == "")
            {
                errorFirstNameTeacher.Clear();
                errorLastNameTeacher.SetError(tbLastName, "Please enter last name");
                lbErrorTeacher.Text = "Please enter last name";
                check = false;
            }
            else if (tbContact.Text == "")
            {
                errorLastNameTeacher.Clear();
                errorContactTeacher.SetError(tbContact, "Please enter contact");
                lbErrorTeacher.Text = "Please enter contact";
                check = false;
            }
            else if (lbModule.SelectedItems.Count < 1)
            {
                errorContactTeacher.Clear();
                errorContactTeacher.SetError(lbModule, "Please select a minimum of 1 module");
                lbErrorTeacher.Text = "Please select a minimum of 1 module";
                check = false;
            }
            else
            {
                errorTeacherId.Clear();
                errorFirstNameTeacher.Clear();
                errorLastNameTeacher.Clear();
                errorContactTeacher.Clear();
                lbErrorTeacher.Text = "";

            }
            return check;
        }




        ////////////////////////////////////////////////////////////////////////////  
        // Student
        // Reuse moethod

        // Paging
        int currentPageIndex = 1;
        int pageSize = 10; //Số dòng hiển thị lên lưới
        int pageNumber = 0; //Số trang
        int rows; //Số dòng được trả về từ câu truy vấn trong formLoad
        int column = 0; // Biến đếm để sorting
        int countFistName = 0;
        int countLastName = 0;
        int countStudentId = 0;
        int countContact = 0;
        int countBirthDate = 0;
        int countStatus = 0;


        async void pageTotal()
        {
            HttpResponseMessage response = await client.GetAsync($"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            rows = results.Count();
            pageNumber = rows % pageSize != 0 ? rows / pageSize + 1 : rows / pageSize;
            lbTotalPage.Text = " / " + pageNumber.ToString();
            cbPage.Items.Clear();
            for (int i = 1; i <= pageNumber; i++)
            {
                cbPage.Items.Add(i + "");
            }
            cbPage.SelectedIndex = 0;
        }
        void ClearStudent()
        {
            tbStudentId.Text = tbSLastName.Text = tbSFirstName.Text = tbSContact.Text = "";
            dtpSBirthDate.Text = DateTime.Now.ToShortDateString();
            cbSStatusId.SelectedIndex = 0;
            btnUpdateStudent.Enabled = false;
            btnDeleteStudent.Enabled = false;
            btnImportStudent.Enabled = false;
            btnNewStudent.Text = "Reset";
        }
        async void DataStudent()
        {
            pageTotal();
            dgvStudent.AutoGenerateColumns = false;

            HttpResponseMessage response = await client.GetAsync($"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
            dgvStudent.DataSource = results.ToList();
            dgvStudent.DataSource = results.Skip(0).Take(10).ToList();
        }
        async void databindingsStudent()
        {

            HttpResponseMessage response = await client.GetAsync($"{pathStatus}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<StatusDTO>>(result);

            tbStudentId.DataBindings.Clear();
            tbStudentId.DataBindings.Add("Text", dgvStudent.DataSource, "StudentId");
            tbSFirstName.DataBindings.Clear();
            tbSFirstName.DataBindings.Add("Text", dgvStudent.DataSource, "FirstName");
            tbSLastName.DataBindings.Clear();
            tbSLastName.DataBindings.Add("Text", dgvStudent.DataSource, "LastName");
            tbSContact.DataBindings.Clear();
            tbSContact.DataBindings.Add("Text", dgvStudent.DataSource, "Contact");
            dtpSBirthDate.DataBindings.Clear();
            dtpSBirthDate.DataBindings.Add("Text", dgvStudent.DataSource, "BirthDate");
            cbSStatusId.DataBindings.Clear();
            string StatusId = dgvStudent.Rows[dgvStudent.CurrentRow.Index].Cells["SStatusId"].Value.ToString();
            cbSStatusId.DataBindings.Add("Text", results.Where(m => m.StatusId == StatusId).FirstOrDefault(), "StatusName");
            btnCreateStudent.Enabled = false;
            btnDeleteStudent.Enabled = true;
            btnUpdateStudent.Enabled = true;
            tbStudentId.Enabled = false;
        }
        // Form method
        private async void btnCreateStudent_Click(object sender, EventArgs e)
        {
            if (validateStudentId() && validateStudent())
            {
                HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
                var resultStatus = await responseStatus.Content.ReadAsStringAsync();
                var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

                pageTotal();
                student.StudentId = tbStudentId.Text.Trim();
                student.FirstName = tbSFirstName.Text.Trim();
                student.LastName = tbSLastName.Text.Trim();
                student.Contact = tbSContact.Text.Trim();
                student.BirthDate = Convert.ToDateTime(dtpSBirthDate.Value.ToShortDateString());

                var students = dataStatus.Where(x => x.StatusName == cbSStatusId.Text).FirstOrDefault();
                if (students != null)
                {
                    student.StatusId = students.StatusId;
                }


                var jsonStudent = JsonConvert.SerializeObject(student);
                var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");
                await client.PostAsync(pathStudent, content);

                ClearStudent();
                DataStudent();
                MessageBox.Show("Create Successfully", "Message");
            }
        }
        private async void btnUpdateStudent_Click(object sender, EventArgs e)
        {
            if (validateStudent())
            {
                HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
                var resultStatus = await responseStatus.Content.ReadAsStringAsync();
                var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

                pageTotal();
                student.StudentId = tbStudentId.Text.Trim();
                student.FirstName = tbSFirstName.Text.Trim();
                student.LastName = tbSLastName.Text.Trim();
                student.Contact = tbSContact.Text.Trim();
                student.BirthDate = Convert.ToDateTime(dtpSBirthDate.Value.ToShortDateString());

                var status = dataStatus.Where(x => x.StatusName == cbSStatusId.Text).FirstOrDefault();
                if (status != null)
                {
                    student.StatusId = status.StatusId;
                }

                var jsonStudent = JsonConvert.SerializeObject(student);
                var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");
                await client.PutAsync($@"{pathStudent}/{tbStudentId.Text}", content);

                ClearStudent();
                DataStudent();
                tbStudentId.Enabled = true;
                btnCreateStudent.Enabled = true;
                MessageBox.Show("Update Successfully", "Message");
            }

        }
        private void dgvStudent_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            errorStudentId.Clear();
            errorFistNameStudent.Clear();
            errorLastNameStudent.Clear();
            errorContactStudent.Clear();
            lbErrorStudent.Text = "";
            if ((tbStatusId.Text != "" || tbSLastName.Text != "" || tbSFirstName.Text != "" || tbSContact.Text != "") && btnUpdateStudent.Enabled == false)
            {
                DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
                if (mess == DialogResult.OK)
                {
                    btnNewStudent.Text = "Cancel";
                    databindingsStudent();
                }
            }
            else
            {
                btnNewStudent.Text = "Cancel";
                databindingsStudent();
            }
        }
        private async void btnDeleteStudent_Click(object sender, EventArgs e)
        {
            string StudentID = dgvStudent.Rows[dgvStudent.CurrentRow.Index].Cells["StudentId"].Value.ToString();
            await client.DeleteAsync($@"{pathStudent}/{StudentID}");
            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                pageTotal();

                ClearStudent();
                DataStudent();
                tbStudentId.Enabled = true;
                btnCreateStudent.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //ClearStudent();
                //DataStudent();
            }
            //tbStudentId.Enabled = true;
            //btnCreateStudent.Enabled = true;
        }
        private void btnNewStudent_Click(object sender, EventArgs e)
        {
            ClearStudent();
            DataStudent();
            tbStudentId.Enabled = true;
            btnCreateStudent.Enabled = true;
            errorImportStudent.Clear();
            tbFileStudent.Text = "";
        }
        private void pbCloseSearchStudent_Click(object sender, EventArgs e)
        {
            tbSearchStudent.Text = "";
        }
        private async void tbSearchStudent_TextChanged(object sender, EventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync($@"{pathStudent}/{tbSearchStudent.Text}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
            if (tbSearchStudent.Text != "")
            {
                if (data.Count == 0)
                {
                    rows = 0;
                    cbPage.Items.Clear();
                    HttpResponseMessage responses = await client.GetAsync($@"{pathStudent}/{tbSearchStudent.Text}");
                    var results = await response.Content.ReadAsStringAsync();
                    dgvStudent.DataSource = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
                    lbTotalPage.Text = "/ 0";
                    cbPage.Items.Add("0");
                    cbPage.SelectedIndex = 0;
                }
                else
                {
                    rows = data.Count();
                    pageNumber = rows % pageSize != 0 ? rows / pageSize + 1 : rows / pageSize;
                    lbTotalPage.Text = " / " + pageNumber.ToString();
                    cbPage.Items.Clear();
                    for (int i = 1; i <= pageNumber; i++)
                    {
                        cbPage.Items.Add(i + "");
                    }
                    cbPage.SelectedIndex = 0;
                    currentPageIndex = Convert.ToInt32(cbPage.Text);
                    pbCloseSearchStudent.Visible = true;
                    dgvStudent.DataSource = data.Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                }

            }
            else
            {
                HttpResponseMessage responses = await client.GetAsync($@"{pathStudent}/{tbSearchStudent.Text}");
                var results = await response.Content.ReadAsStringAsync();
                var datas = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
                rows = datas.Count();
                pageNumber = rows % pageSize != 0 ? rows / pageSize + 1 : rows / pageSize;
                lbTotalPage.Text = " / " + pageNumber.ToString();
                cbPage.Items.Clear();
                for (int i = 1; i <= pageNumber; i++)
                {
                    cbPage.Items.Add(i + "");
                }
                cbPage.Text = "1";
                currentPageIndex = Convert.ToInt32(cbPage.Text);
                cbPage.SelectedIndex = 0;
                dgvStudent.DataSource = datas.Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                pbCloseSearchStudent.Visible = false;
            }

        }
        private void dgvStudent_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvStudent.ClearSelection();
        }
        // Validate Student
        private void btnOpenFileStudent_Click(object sender, EventArgs e)
        {
            openFileStudent.Filter = "csv files (*.csv)|*.csv";
            openFileStudent.ShowDialog();
            var filename = Path.GetFileName(openFileStudent.FileName);
            tbFileStudent.Text = filename;
            if (tbFileStudent.Text != "")
            {
                btnImportStudent.Enabled = true;
            }
            else
            {
                btnImportStudent.Enabled = false;
            }
        }
        private bool validateImportStudent()
        {
            HttpResponseMessage response = client.GetAsync($"{pathStudent}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            list_student = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
            var check = true;
            var filename = Path.GetFileName(openFileStudent.FileName);
            var path = Path.GetDirectoryName(openFileStudent.FileName);
            var fullPath = path + "\\" + filename;
            var listId = list_student.Select(x => x.StudentId).ToList();

            StreamReader streamCsv = new StreamReader(fullPath);

            string csvDataLine = "";
            string[] data = null;
            var lineHeader = streamCsv.ReadLine();
            int count = 0;
            while ((csvDataLine = streamCsv.ReadLine()) != null)
            {
                data = csvDataLine.Split(',');
                if (listId.Contains(data[0]))
                {
                    count = 1;
                }
            }
            if (count == 1)
            {
                errorImportStudent.SetError(tbFileStudent, "File there is data StudentId already exists");
                check = false;
            }
            else if (Path.GetExtension(tbFileStudent.Text) != ".csv")
            {
                errorImportStudent.SetError(tbFileStudent, "Please select the file with the .csv extension");
                check = false;
            }
            return check;

        }
        private async void btnImportStudent_Click(object sender, EventArgs e)
        {
            if (validateImportStudent())
            {
                errorImportStudent.Clear();
                var filename = Path.GetFileName(openFileStudent.FileName);
                var path = Path.GetDirectoryName(openFileStudent.FileName);
                var fullPath = path + "\\" + filename;


                StreamReader streamCsv = new StreamReader(path);

                string csvDataLine = "";
                string[] data = null;
                var lineHeader = streamCsv.ReadLine();
                while ((csvDataLine = streamCsv.ReadLine()) != null)
                {
                    data = csvDataLine.Split(',');
                    var newStudent = new StudentDTO
                    {
                        StudentId = data[0],
                        LastName = data[1],
                        FirstName = data[2],
                        Contact = data[3],
                        BirthDate = Convert.ToDateTime(data[4]),
                        StatusId = data[5]

                    };
                    var jsonStudent = JsonConvert.SerializeObject(newStudent);
                    var content = new StringContent(jsonStudent, Encoding.UTF8, "application/json");
                    await client.PostAsync(pathStudent, content);
                }
                tbFileStudent.Text = "";
                DataStudent();
                MessageBox.Show("Import successfully", "Message", MessageBoxButtons.OK);
                btnImportStudent.Enabled = false;
            }
        }
        private bool validateStudentId()
        {
            HttpResponseMessage response = client.GetAsync($"{pathStudent}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            list_student = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            var year = dtpSBirthDate.Value.Year.ToString().Substring(2, 2);
            var month = "";
            if (dtpSBirthDate.Value.Month < 10)
            {
                month += "0" + dtpSBirthDate.Value.Month;
            }
            else
            {
                month = dtpSBirthDate.Value.Month.ToString();
            }
            var listID = list_student.Select(m => m.StudentId).ToList();
            var check = true;
            if (tbStudentId.Text == "")
            {
                errorStudentId.SetError(tbStudentId, "Please enter Student ID");
                lbErrorStudent.Text = "Please enter Student ID";
                check = false;
            }
            else if (new Regex("^[S]([0-1]{2})(" + year + month + ")([0-9]{4})$").IsMatch(tbStudentId.Text) == false)
            {
                errorStudentId.SetError(tbStudentId, "Student Id must start with the letter 'S' then 01 or 00 or 11 or 10 or 2 digits of the year and 2 digits of the month and 4 random digits");
                lbErrorStudent.Text = "Student Id must start with the letter 'S' then 01 or 00 or 11 or 10 or 2 digits of the year and 2 digits of the month and 4 random digits";
                check = false;
            }
            else if (listID.Contains(tbStudentId.Text))
            {
                errorStudentId.SetError(tbModuleId, "Student ID already exist please enter again");
                lbErrorStudent.Text = "Student ID already exist please enter again";
                check = false;
            }
            else
            {
                errorStudentId.Clear();
                lbErrorStudent.Text = "";

            }
            return check;
        }
        private bool validateStudent()
        {
            var check = true;
            if (tbSFirstName.Text == "")
            {
                errorFistNameStudent.SetError(tbSFirstName, "Please enter first name");
                lbErrorStudent.Text = "Please enter first name";
                check = false;
            }
            else if (tbSLastName.Text == "")
            {
                errorFistNameStudent.Clear();
                errorLastNameStudent.SetError(tbSLastName, "Please enter last name");
                lbErrorStudent.Text = "Please enter last name";
                check = false;
            }
            else if (tbSContact.Text == "")
            {
                errorLastNameStudent.Clear();
                errorContactStudent.SetError(tbSContact, "Please enter contact");
                lbErrorStudent.Text = "Please enter contact";
                check = false;
            }
            else
            {
                errorFistNameStudent.Clear();
                errorLastNameStudent.Clear();
                errorContactStudent.Clear();
                lbErrorStudent.Text = "";

            }
            return check;
        }

        // Paging and Sorting 
        private async void cbPage_SelectedIndexChanged(object sender, EventArgs e)
        {

            currentPageIndex = Convert.ToInt32(cbPage.Text);
            HttpResponseMessage response = await client.GetAsync($"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var list_student = JsonConvert.DeserializeObject<List<StudentDTO>>(result);
            if (tbSearchStudent.Text == "")
            {
                if (countFistName == 0 && countLastName == 0)
                {
                    dgvStudent.DataSource = list_student.Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                }
                else if (column == 0)
                {
                    if (countStudentId % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else if (column == 1)
                {
                    if (countFistName % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else if (column == 2)
                {
                    if (countLastName % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else if (column == 3)
                {
                    if (countContact % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else if (column == 4)
                {
                    if (countBirthDate % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countStatus % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
            }
            else
            {
                HttpResponseMessage responseStudent = await client.GetAsync($@"{pathStudent}/{tbSearchStudent.Text}");
                var resultStudent = await responseStudent.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);
                if (countFistName == 0 && countLastName == 0)
                {
                    dgvStudent.DataSource = data.Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                }
                else if (column == 0)
                {
                    if (countStudentId % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else if (column == 1)
                {
                    if (countFistName % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else if (column == 2)
                {
                    if (countLastName % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else if (column == 3)
                {
                    if (countContact % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else if (column == 4)
                {
                    if (countBirthDate % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countStatus % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
            }


        }
        private async void dgvStudent_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            ClearStudent();
            tbStudentId.Enabled = true;
            btnCreateStudent.Enabled = true;
            column = e.ColumnIndex;
            currentPageIndex = Convert.ToInt32(cbPage.Text);

            HttpResponseMessage response = await client.GetAsync($@"{pathStudent}/{tbSearchStudent.Text}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            if (e.ColumnIndex == 0)
            {
                countStudentId++;
                if (tbSearchStudent.Text != "")
                {
                    if (countStudentId % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else
                {
                    if (countStudentId % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.StudentId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
            }
            else if (e.ColumnIndex == 1)
            {
                countFistName++;
                if (tbSearchStudent.Text != "")
                {
                    if (countFistName % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }
                else
                {
                    if (countFistName % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.FirstName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                }


            }
            else if (e.ColumnIndex == 2)
            {
                countLastName++;
                if (tbSearchStudent.Text != "")
                {
                    if (countLastName % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countLastName % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.LastName).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }

            }
            else if (e.ColumnIndex == 3)
            {
                countContact++;
                if (tbSearchStudent.Text != "")
                {
                    if (countContact % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countContact % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.Contact).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
            }
            else if (e.ColumnIndex == 4)
            {
                countBirthDate++;
                if (tbSearchStudent.Text != "")
                {
                    if (countBirthDate % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countBirthDate % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.BirthDate).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
            }
            else
            {
                countStatus++;
                if (tbSearchStudent.Text != "")
                {
                    if (countStatus % 2 == 0)
                    {
                        dgvStudent.DataSource = data.OrderByDescending(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = data.OrderBy(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
                else
                {
                    if (countStatus % 2 == 0)
                    {
                        dgvStudent.DataSource = list_student.OrderByDescending(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();
                    }
                    else
                    {
                        dgvStudent.DataSource = list_student.OrderBy(c => c.StatusId).Skip(currentPageIndex * pageSize - pageSize).Take(pageSize).ToList();

                    }
                }
            }
        }
        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {

            dgwClassType.ClearSelection();
            dgvStatus.ClearSelection();
            dgvModule.ClearSelection();
            dgvTeacher.ClearSelection();
            dgvStudent.ClearSelection();
            dgvClass.ClearSelection();
        }


        //////////////////////////////////////////////////////////////////////////// 
        // Class

        // Reuse method
        void ClearClass()
        {
            tbClassId.Text = "";
            cbModuleClass.SelectedIndex = cbTeacherClass.SelectedIndex = cbTypeClass.SelectedIndex = 0;
            cbStatusClass.SelectedIndex = 3;
            btnUpdateClass.Enabled = false;
            btnImportClass.Enabled = false;
            btnResetClass.Text = "Reset";
        }
        async void DataClass()
        {
            dgvClass.AutoGenerateColumns = false;

            HttpResponseMessage response = await client.GetAsync($"{pathClass}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<ClassDTO>>(result);

            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var resultsStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var list_teacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);


            dgvClass.DataSource = results.Select(m => new
            {
                ClassId = m.ClassId,
                TeachingHour = m.TeachingHour,
                ModuleId = list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName,
                StatusId = resultsStatus.Where(e => e.StatusId == m.StatusId).FirstOrDefault().StatusName,
                TeacherId = list_teacher.Where(x => x.TeacherId == m.TeacherId).FirstOrDefault().FullName,
                m.TypeId
            }).ToList();
        }
        void databindingsClass()
        {
            tbClassId.DataBindings.Clear();
            tbClassId.DataBindings.Add("Text", dgvClass.DataSource, "ClassId");


            cbModuleClass.DataBindings.Clear();

            cbModuleClass.DataBindings.Add("Text", dgvClass.DataSource, "ModuleId");

            cbStatusClass.DataBindings.Clear();

            cbStatusClass.DataBindings.Add("Text", dgvClass.DataSource, "StatusId");

            cbTeacherClass.DataBindings.Clear();

            cbTeacherClass.DataBindings.Add("Text", dgvClass.DataSource, "TeacherId");

            cbTypeClass.DataBindings.Clear();
            cbTypeClass.DataBindings.Add("Text", dgvClass.DataSource, "TypeId");

            btnCreateClass.Enabled = false;
            btnUpdateClass.Enabled = true;
            tbClassId.Enabled = false;
        }
        // Form method
        string TypeCurrent = null;
        private async void btnCreateClass_Click(object sender, EventArgs e)
        {

            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var dataTeacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            var moduleName = cbModuleClass.Text;
            var moduleId = list_module.Where(m => m.ModuleName == moduleName).FirstOrDefault().ModuleId;
            for (var i = 1; i < 30; i++)
            {
                var classID = classes.ClassId = "C" + DateTime.Now.Year.ToString().Substring(2, 2) + DateTime.Now.Month.ToString("00") + moduleId + "_" + i;
                if (list_Class.Select(m => m.ClassId).ToList().Contains(classID))
                {
                    continue;
                }
                else
                {
                    break;
                }
            }

            classes.ModuleId = moduleId;
            classes.TypeId = cbTypeClass.Text;

            var teacher = dataTeacher.Where(x => x.FirstName + " " + x.LastName == cbTeacherClass.Text.Trim()).FirstOrDefault();
            if (teacher != null)
            {
                classes.TeacherId = teacher.TeacherId;
            }
            var status = dataStatus.Where(x => x.StatusName == cbStatusClass.Text).FirstOrDefault();
            if (status != null)
            {
                classes.StatusId = status.StatusId;
            }


            var jsonClass = JsonConvert.SerializeObject(classes);
            var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
            await client.PostAsync(pathClass, content);

            ClearClass();
            DataClass();
            MessageBox.Show("Create Successfully", "Message");

        }
        private async void btnUpdateClass_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var dataStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var dataTeacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            var moduleName = cbModuleClass.Text;
            var moduleId = list_module.Where(m => m.ModuleName == moduleName).FirstOrDefault().ModuleId;

            classes.ClassId = tbClassId.Text.Trim();
            classes.ModuleId = moduleId;
            classes.TypeId = cbTypeClass.Text;

            var teacher = dataTeacher.Where(x => x.FirstName + " " + x.LastName == cbTeacherClass.Text.Trim()).FirstOrDefault();
            if (teacher != null)
            {
                classes.TeacherId = teacher.TeacherId;
            }
            var status = dataStatus.Where(x => x.StatusName == cbStatusClass.Text).FirstOrDefault();
            if (status != null)
            {
                classes.StatusId = status.StatusId;
            }

            var jsonClass = JsonConvert.SerializeObject(classes);
            var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
            await client.PutAsync($@"{pathClass}/{tbClassId.Text}", content);

            ClearClass();
            DataClass();
            tbClassId.Enabled = true;
            btnCreateClass.Enabled = true;
            MessageBox.Show("Update Successfully", "Message");


        }
        private async void dgvClass_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var list_teachers = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseCapable = await client.GetAsync($"{pathCapable}");
            var resultCapable = await responseCapable.Content.ReadAsStringAsync();
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);

            TypeCurrent = dgvClass.Rows[dgvClass.CurrentRow.Index].Cells["TypeClass"].Value.ToString();
            var moduleId = list_module.Where(m => m.ModuleName == cbModuleClass.Text).FirstOrDefault().ModuleId;

            var teacher = list_Class.Where(m => m.ModuleId == moduleId && m.TypeId == cbTypeClass.Text).Select(m => m.TeacherId).ToList();
            var list_teacher = list_teachers.Select(m => m.FullName).ToList();
            foreach (var item in teacher)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == item).FirstOrDefault().FullName;
                list_teacher = list_teacher.Where(m => m != teacherName).ToList();
            }
            cbTeacherClass.DataSource = list_teacher;



            var teacherId = dataCapable.Where(m => m.ModuleId == moduleId).ToList();
            List<string> teacherModule = new List<string>();
            for (var i = 0; i < teacherId.Count; i++)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == teacherId[i].TeacherId).FirstOrDefault().FullName;
                teacherModule.Add(teacherName.ToString());
            }
            cbTeacherClass.DataSource = teacherModule;
            dgvClass.Refresh();
            errorStudentId.Clear();
            errorDuration.Clear();
            lbErrorClass.Text = "";

            btnResetClass.Text = "Cancel";
            databindingsClass();

        }
        private void btnResetClass_Click(object sender, EventArgs e)
        {
            ClearClass();
            DataClass();
            tbClassId.Enabled = true;
            btnCreateClass.Enabled = true;
            lbErrorClass.Text = "";
            errorStudentId.Clear();
            errorDuration.Clear();
        }
        private void dgvClass_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvClass.ClearSelection();
        }
        private async void tbSearchClass_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchClass.Text != "")
            {
                pbCloseSearchClass.Visible = true;
            }
            else
            {
                pbCloseSearchClass.Visible = false;
            }
            HttpResponseMessage response = await client.GetAsync($@"{pathClass}/{tbSearchClass.Text}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<ClassDTO>>(result);

            HttpResponseMessage responseStatus = await client.GetAsync($"{pathStatus}");
            var resultStatus = await responseStatus.Content.ReadAsStringAsync();
            var resultsStatus = JsonConvert.DeserializeObject<List<StatusDTO>>(resultStatus);

            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var list_teacher = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            dgvClass.DataSource = results.Select(m => new
            {
                ClassId = m.ClassId,
                TeachingHour = m.TeachingHour,
                ModuleId = list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName,
                StatusId = resultsStatus.Where(c => c.StatusId == m.StatusId).FirstOrDefault().StatusName,
                TeacherId = list_teacher.Where(a => a.TeacherId == m.TeacherId).FirstOrDefault().FullName,
                m.TypeId
            }).ToList();
        }
        private void pbCloseSearchClass_Click(object sender, EventArgs e)
        {
            tbSearchClass.Text = "";
        }
        // Validate Class
        private bool validateClassId()
        {
            HttpResponseMessage responseClass = client.GetAsync($"{pathClass}").Result;
            var resultClass = responseClass.Content.ReadAsStringAsync().Result;
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            var year = DateTime.Now.Year.ToString().Substring(2, 2);
            var month = DateTime.Now.Month.ToString("00");
            var listID = list_Class.Select(m => m.ClassId).ToList();
            var check = true;
            if (tbClassId.Text == "")
            {
                errorStudentId.SetError(tbClassId, "Please enter Class ID");
                lbErrorClass.Text = "Please enter Class ID";
                check = false;
            }
            else if (new Regex("^[C](" + year + month + cbModuleClass.Text + "_" + ")([0-9]{1})$").IsMatch(tbClassId.Text) == false)
            {
                errorStudentId.SetError(tbClassId, "The Class ID must begin with the letter 'C' followed by the last 2 digits of the 2nd year of the year of the code year of the character '_' and finally 1 random number");
                lbErrorClass.Text = "The Class ID must begin with the letter 'C' followed by the last 2 digits of the 2nd year of the year of the code year of the character '_' and finally 1 random number";
                check = false;
            }
            else if (listID.Contains(tbClassId.Text))
            {
                errorStudentId.SetError(tbClassId, "Class ID already exist please enter again");
                lbErrorClass.Text = "Class ID already exist please enter again";
                check = false;
            }
            else
            {
                errorStudentId.Clear();
                lbErrorClass.Text = "";

            }
            return check;
        }
        private async void finbyTeacher()
        {
            HttpResponseMessage responseModule = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModule.Content.ReadAsStringAsync();
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = await client.GetAsync($"{pathTeacher}");
            var resultTeacher = await responseTeacher.Content.ReadAsStringAsync();
            var list_teachers = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseCapable = await client.GetAsync($"{pathCapable}");
            var resultCapable = await responseCapable.Content.ReadAsStringAsync();
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);

            var moduleId = list_module.Where(m => m.ModuleName == cbModuleClass.Text).FirstOrDefault().ModuleId;

            var teacher = list_Class.Where(m => m.ModuleId == moduleId && m.TypeId == cbTypeClass.Text).Select(m => m.TeacherId).ToList();
            var list_teacher = list_teachers.Select(m => m.FullName).ToList();

            foreach (var item in teacher)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == item).FirstOrDefault().FullName;
                list_teacher = list_teacher.Where(m => m != teacherName).ToList();
            }
            cbTeacherClass.DataSource = list_teacher;



            var teacherId = dataCapable.Where(m => m.ModuleId == moduleId).ToList();
            List<string> teacherModule = new List<string>();
            for (var i = 0; i < teacherId.Count; i++)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == teacherId[i].TeacherId).FirstOrDefault().FullName;
                teacherModule.Add(teacherName.ToString());
            }
            cbTeacherClass.DataSource = teacherModule;
        }
        private void cbModuleClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseModule = client.GetAsync($"{pathModule}").Result;
            var resultModule = responseModule.Content.ReadAsStringAsync().Result;
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = client.GetAsync($"{pathTeacher}").Result;
            var resultTeacher = responseTeacher.Content.ReadAsStringAsync().Result;
            var list_teachers = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            HttpResponseMessage responseClass = client.GetAsync($"{pathClass}").Result;
            var resultClass = responseClass.Content.ReadAsStringAsync().Result;
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseCapable = client.GetAsync($"{pathCapable}").Result;
            var resultCapable = responseCapable.Content.ReadAsStringAsync().Result;
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);

            var moduleId = list_module.Where(m => m.ModuleName == cbModuleClass.Text).FirstOrDefault().ModuleId;

            var teacher = list_Class.Where(m => m.ModuleId == moduleId && m.TypeId == cbTypeClass.Text).Select(m => m.TeacherId).ToList();
            var list_teacher = list_teachers.Select(m => m.FullName).ToList();
            foreach (var item in teacher)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == item).FirstOrDefault().FullName;
                list_teacher = list_teacher.Where(m => m != teacherName).ToList();
            }
            cbTeacherClass.DataSource = list_teacher;



            var teacherId = dataCapable.Where(m => m.ModuleId == moduleId).ToList();
            List<string> teacherModule = new List<string>();
            for (var i = 0; i < teacherId.Count; i++)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == teacherId[i].TeacherId).FirstOrDefault().FullName;
                teacherModule.Add(teacherName.ToString());
            }
            cbTeacherClass.DataSource = teacherModule;
        }
        private void cbTypeClass_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseModule = client.GetAsync($"{pathModule}").Result;
            var resultModule = responseModule.Content.ReadAsStringAsync().Result;
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseTeacher = client.GetAsync($"{pathTeacher}").Result;
            var resultTeacher = responseTeacher.Content.ReadAsStringAsync().Result;
            var list_teachers = JsonConvert.DeserializeObject<List<TeacherDTO>>(resultTeacher);

            HttpResponseMessage responseClass = client.GetAsync($"{pathClass}").Result;
            var resultClass = responseClass.Content.ReadAsStringAsync().Result;
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseCapable = client.GetAsync($"{pathCapable}").Result;
            var resultCapable = responseCapable.Content.ReadAsStringAsync().Result;
            var dataCapable = JsonConvert.DeserializeObject<List<CapableDTO>>(resultCapable);

            var moduleId = list_module.Where(m => m.ModuleName == cbModuleClass.Text).FirstOrDefault().ModuleId;

            var teacher = list_Class.Where(m => m.ModuleId == moduleId && m.TypeId == cbTypeClass.Text).Select(m => m.TeacherId).ToList();
            var list_teacher = list_teachers.Select(m => m.FullName).ToList();

            foreach (var item in teacher)
            {
                var teacherName = list_teachers.Where(t => t.TeacherId == item).FirstOrDefault().FullName;
                list_teacher = list_teacher.Where(m => m != teacherName).ToList();
            }
            cbTeacherClass.DataSource = list_teacher;



            var teacherId = dataCapable.Where(m => m.ModuleId == moduleId).ToList();
            List<string> teacherModule = new List<string>();
            for (var i = 0; i < teacherId.Count; i++)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == teacherId[i].TeacherId).FirstOrDefault().FullName;
                teacherModule.Add(teacherName.ToString());
            }
            cbTeacherClass.DataSource = teacherModule;

            foreach (var item in teacher)
            {
                var teacherName = list_teachers.Where(x => x.TeacherId == item).FirstOrDefault().FullName;
                teacherModule = teacherModule.Where(m => m != teacherName).ToList();
            }
            cbTeacherClass.DataSource = teacherModule;

            if (cbTypeClass.Text == TypeCurrent)
            {
                finbyTeacher();
            }
        }





        //////////////////////////////////////////////////////////////////////////// 
        // Enroll

        // Reuse method
        void ClearEnroll()
        {
            cbClassEnroll.SelectedIndex = 0;
            lbStudent.SelectedIndex = -1;

            btnUpdateEnroll.Enabled = false;
            btnDeleteEnroll.Enabled = false;
            btnResetEnroll.Text = "Reset";
            cbClassEnroll.Enabled = true;
        }
        async void DataEnroll()
        {
            dgvEnroll.AutoGenerateColumns = false;
            HttpResponseMessage response = await client.GetAsync($"{pathEnroll}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<EnrollDTO>>(result);

            HttpResponseMessage responseStudent = await client.GetAsync($@"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            dgvEnroll.DataSource = results.Select(m => new
            {
                StudentId = data.Where(x => x.StudentId == m.StudentId).FirstOrDefault().FullName,
                ClassId = m.ClassId,
                Hw1Grade = m.Hw1Grade,
                Hw2Grade = m.Hw2Grade,
                Hw3Grade = m.Hw3Grade,
                Hw4Grade = m.Hw4Grade,
                Hw5Grade = m.Hw5Grade,
                Passed = m.Passed,
                ExamGrade = m.ExamGrade
            }).ToList();
        }
        void databindingsEnroll()
        {
            HttpResponseMessage response = client.GetAsync($@"{pathStudent}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            string studentId = dgvEnroll.Rows[dgvEnroll.CurrentCell.RowIndex].Cells["StudentEnroll"].Value.ToString();
            lbStudent.DataBindings.Clear();
            lbStudent.DataBindings.Add("Text", data.Where(m => m.FullName == studentId).FirstOrDefault(), "FullName");

            cbClassEnroll.DataBindings.Clear();
            cbClassEnroll.DataBindings.Add("Text", dgvEnroll.DataSource, "ClassId");


            btnCreateEnroll.Enabled = false;
            btnUpdateEnroll.Enabled = true;
            cbClassEnroll.Enabled = false;
        }

        // Form method

        private async void btnCreateEnroll_Click(object sender, EventArgs e)
        {
            List<string> students = new List<string>();
            foreach (var student in lbStudent.SelectedItems)
            {
                //await Task.Run(() => students.Add(student.ToString()));
                students.Add(student.ToString());
            }
            if (validateEnrollId() && validateEnroll())
            {
                HttpResponseMessage response = await client.GetAsync($@"{pathStudent}");
                var result = await response.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

                foreach (string student in students)
                {
                    var studentId = data.Where(m => m.FullName == student).FirstOrDefault();
                    enroll.ClassId = cbClassEnroll.Text;
                    enroll.StudentId = studentId.StudentId;
                    enroll.Passed = 0;

                    var jsonEnroll = JsonConvert.SerializeObject(enroll);
                    var content = new StringContent(jsonEnroll, Encoding.UTF8, "application/json");
                    await client.PostAsync($@"{pathEnroll}", content);

                }

                ClearEnroll();
                DataEnroll();
                MessageBox.Show("Create Successfully", "Message");
            }
        }
        private async void btnUpdateEnroll_Click(object sender, EventArgs e)
        {

            HttpResponseMessage responseStudent = await client.GetAsync($"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var dataStudent = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            enroll.ClassId = cbClassEnroll.Text;
            string studentID = "";

            var student = dataStudent.Where(x => x.FirstName + " " + x.LastName == lbStudent.Text.Trim()).FirstOrDefault();
            if (student != null)
            {
                studentID = student.StudentId;
                enroll.StudentId = student.StudentId;
            }


            var jsonClass = JsonConvert.SerializeObject(enroll);
            var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
            await client.PutAsync($@"{pathEnroll}/{studentID}{tbClassId.Text}", content);

            ClearEnroll();
            DataEnroll();
            //tbStudentId.Enabled = true;
            btnCreateEnroll.Enabled = true;
            MessageBox.Show("Update Successfully", "Message");

        }
        private async void btnDeleteEnroll_Click(object sender, EventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync($@"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            string enrollId = data.Where(m => m.FullName == dgvEnroll.Rows[dgvEnroll.CurrentRow.Index].Cells["StudentEnroll"].Value.ToString()).FirstOrDefault().StudentId + dgvEnroll.Rows[dgvEnroll.CurrentRow.Index].Cells["ClassEnroll"].Value.ToString();

            if (MessageBox.Show("Are you sure to delete record?", "Delete", MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                await client.DeleteAsync($@"{pathEnroll}/{enrollId}");
                ClearEnroll();
                DataEnroll();
                //tbStudentId.Enabled = true;
                btnCreateEnroll.Enabled = true;
                MessageBox.Show("Delete Successfully", "Message");
            }
            else
            {
                //ClearEnroll();
                //DataEnroll();
            }
            //tbStudentId.Enabled = true;
            //btnCreateStudent.Enabled = true;
        }
        private void dgvEnroll_CellClick(object sender, DataGridViewCellEventArgs e)
        {

            lbStudent.SelectedIndex = -1;
            errorEnroll.Clear();
            lbErrorEnroll.Text = "";
            btnDeleteEnroll.Enabled = true;
            //if (btnUpdateEnroll.Enabled == false)
            //{
            //    DialogResult mess = MessageBox.Show("The data you have entered will be lost. do you want to continue?", "Message", MessageBoxButtons.OKCancel);
            //    if (mess == DialogResult.OK)
            //    {
            btnResetEnroll.Text = "Cancel";
            databindingsEnroll();

            //    }
            //}
            //else
            //{
            //    btnResetEnroll.Text = "Cancel";
            //    databindingsEnroll();
            //}
        }
        private void btnResetEnroll_Click(object sender, EventArgs e)
        {
            btnCreateEnroll.Enabled = true;
            ClearEnroll();
            DataEnroll();
            lbErrorEnroll.Text = "";
            errorEnroll.Clear();
        }
        private void pbCloseSearchEnroll_Click(object sender, EventArgs e)
        {
            tbSearchEnroll.Text = "";
        }
        private async void tbSearchEnroll_TextChanged(object sender, EventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync($@"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);


            HttpResponseMessage responseEnroll = await client.GetAsync($@"{pathEnroll}/{tbSearchEnroll.Text}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            if (tbSearchEnroll.Text != "")
            {
                pbCloseSearchEnroll.Visible = true;
            }
            else
            {
                pbCloseSearchEnroll.Visible = false;
            }
            dgvEnroll.DataSource = dataEnroll.Select(m => new
            {
                StudentId = data.Where(x => x.StudentId == m.StudentId).FirstOrDefault().FullName,
                ClassId = m.ClassId,
                Hw1Grade = m.Hw1Grade,
                Hw2Grade = m.Hw2Grade,
                Hw3Grade = m.Hw3Grade,
                Hw4Grade = m.Hw4Grade,
                Hw5Grade = m.Hw5Grade,
                Passed = m.Passed,
                ExamGrade = m.ExamGrade
            }).ToList();
        }
        private void dgvEnroll_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvEnroll.ClearSelection();
        }
        // Validate Enroll
        // ^([1-9]{1,9}[05]+)|5$
        private bool validateEnrollId()
        {
            HttpResponseMessage responseEnroll = client.GetAsync($@"{pathEnroll}").Result;
            var resultEnroll = responseEnroll.Content.ReadAsStringAsync().Result;
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            HttpResponseMessage responseStudent = client.GetAsync($"{pathStudent}").Result;
            var resultStudent = responseStudent.Content.ReadAsStringAsync().Result;
            var dataStudent = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            string studentId = "";

            var student = dataStudent.Where(x => x.FirstName + " " + x.LastName == lbStudent.Text.Trim()).FirstOrDefault();
            if (teacher != null)
            {
                studentId = student.StudentId;
            }

            string enrollId = studentId + cbClassEnroll.Text;
            var listID = dataEnroll.Select(m => m.StudentId + m.ClassId).ToList();
            var check = true;
            if (listID.Contains(enrollId))
            {
                errorEnroll.SetError(lbStudent, "This student is already in class");
                lbErrorEnroll.Text = "This student is already in class";
                check = false;
            }
            else
            {
                errorEnroll.Clear();
                lbErrorEnroll.Text = "";

            }
            return check;
        }
        private bool validateEnroll()
        {

            HttpResponseMessage responseEnroll = client.GetAsync($@"{pathEnroll}").Result;
            var resultEnroll = responseEnroll.Content.ReadAsStringAsync().Result;
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            var check = true;
            if (dataEnroll.Where(m => m.ClassId == cbClassEnroll.Text).ToList().Count >= 24)
            {
                errorEnroll.SetError(lbStudent, "Class enough. Please choose another class");
                lbErrorEnroll.Text = "Class enough. Please choose another class";
                check = false;
            }
            else
            {
                errorEnroll.Clear();
                lbErrorEnroll.Text = "";

            }
            return check;
        }
        private async void cbClassEnroll_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage response = await client.GetAsync($@"{pathStudent}");
            var result = await response.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(result);

            HttpResponseMessage responseEnroll = await client.GetAsync($@"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            lbStudent.SelectedIndex = -1;
            var studentsId = dataEnroll.Select(m => m.StudentId).ToList();
            var list_student = data.Select(m => m.FullName).ToList();

            foreach (var student in studentsId)
            {
                var FullNameStudent = data.Where(x => x.StudentId == student).FirstOrDefault().FullName;
                list_student = list_student.Where(m => m != FullNameStudent).ToList();
            }

            var studentNotPass = dataEnroll.Where(m => m.Passed == 0).Select(m => m.StudentId).ToList();
            foreach (var item in studentNotPass)
            {
                var FullNameStudent = data.Where(x => x.StudentId == item).FirstOrDefault().FullName;
                list_student.Add(FullNameStudent);
            }
            lbStudent.DataSource = list_student;
        }



        //////////////////////////////////////////////////////////////////////////////
        // Evaluate

        void ClearEvalueate()
        {

            tbUnderstand.Text = tbSupport.Text = tbTeaching.Text = tbPunctuality.Text = "";
            cbClassEvaluate.SelectedIndex = 0;
            cbStudentEvaluate.SelectedIndex = 0;
            btnUpdateEvaluate.Enabled = false;
            btnResetEvaluate.Text = "Reset";
            btnCreateEvaluate.Enabled = true;
            cbClassEvaluate.Enabled = true;
            cbStudentEvaluate.Enabled = true;
            DataEvaluate();
        }
        async void DataEvaluate()
        {
            dgvEvaluate.AutoGenerateColumns = false;
            HttpResponseMessage response = await client.GetAsync($"{pathEvalua}");
            var result = await response.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<List<EvaluateDTO>>(result);

            HttpResponseMessage responseStudent = await client.GetAsync($@"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            dgvEvaluate.DataSource = results.Select(m => new
            {
                StudentId = data.Where(x => x.StudentId == m.StudentId).FirstOrDefault().FullName,
                ClassId = m.ClassId,
                Understand = m.Understand,
                Punctuality = m.Punctuality,
                Support = m.Support,
                Teaching = m.Teaching
            }).ToList();
        }
        void bindingsEvaluate()
        {

            string studentId = dgvEvaluate.Rows[dgvEvaluate.CurrentRow.Index].Cells["StudentEvalua"].Value.ToString();

            cbClassEvaluate.DataBindings.Clear();
            cbClassEvaluate.DataBindings.Add("Text", dgvEvaluate.DataSource, "ClassId", true, DataSourceUpdateMode.OnPropertyChanged);

            cbStudentEvaluate.DataBindings.Clear();
            cbStudentEvaluate.DataBindings.Add("Text", dgvEvaluate.DataSource, "StudentId", true, DataSourceUpdateMode.OnPropertyChanged);

            tbUnderstand.DataBindings.Clear();
            tbUnderstand.DataBindings.Add("Text", dgvEvaluate.DataSource, "Understand", true, DataSourceUpdateMode.OnPropertyChanged);

            tbPunctuality.DataBindings.Clear();
            tbPunctuality.DataBindings.Add("Text", dgvEvaluate.DataSource, "Punctuality", true, DataSourceUpdateMode.OnPropertyChanged);

            tbSupport.DataBindings.Clear();
            tbSupport.DataBindings.Add("Text", dgvEvaluate.DataSource, "Support", true, DataSourceUpdateMode.OnPropertyChanged);

            tbTeaching.DataBindings.Clear();
            tbTeaching.DataBindings.Add("Text", dgvEvaluate.DataSource, "Teaching", true, DataSourceUpdateMode.OnPropertyChanged);
            btnCreateEvaluate.Enabled = false;
            btnUpdateEvaluate.Enabled = true;
            btnResetEvaluate.Text = "Cancel";
            cbClassEvaluate.Enabled = false;
            cbStudentEvaluate.Enabled = false;
        }
        private void cbClassEvaluate_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseStudent = client.GetAsync($@"{pathStudent}").Result;
            var resultStudent = responseStudent.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            HttpResponseMessage responseEnroll = client.GetAsync($@"{pathEnroll}").Result;
            var resultEnroll = responseEnroll.Content.ReadAsStringAsync().Result;
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            List<string> student = new List<string>();
            var list_studentId = dataEnroll.Where(m => m.ClassId == cbClassEvaluate.Text).Select(m => m.StudentId).ToList();
            foreach (var item in list_studentId)
            {
                student.Add(data.Where(m => m.StudentId == item).FirstOrDefault().FullName);
            }
            cbStudentEvaluate.DataSource = student;
        }
        private async void btnCreateEvaluate_Click(object sender, EventArgs e)
        {
            if (validateEvaluate())
            {
                HttpResponseMessage responseStudent = await client.GetAsync($@"{pathStudent}");
                var resultStudent = await responseStudent.Content.ReadAsStringAsync();
                var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

                string studentId = data.Where(m => m.FullName == cbStudentEvaluate.Text).FirstOrDefault().StudentId;
                evaluate.StudentId = studentId;
                evaluate.ClassId = cbClassEvaluate.Text;
                evaluate.Understand = tbUnderstand.Text.Trim();
                evaluate.Punctuality = tbPunctuality.Text.Trim();
                evaluate.Support = tbSupport.Text.Trim();
                evaluate.Teaching = tbTeaching.Text.Trim();

                var jsonEvalua = JsonConvert.SerializeObject(evaluate);
                var content = new StringContent(jsonEvalua, Encoding.UTF8, "application/json");
                await client.PostAsync($@"{pathEvalua}", content);

                ClearEvalueate();
                DataEvaluate();
                MessageBox.Show("Create Successfully", "Message");
            }

        }
        private void dgvEvaluate_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            bindingsEvaluate();
        }
        private void btnResetEvaluate_Click(object sender, EventArgs e)
        {
            lbErrorEvaluate.Text = "";
            ClearEvalueate();
        }
        private async void btnUpdateEvaluate_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseStudent = await client.GetAsync($@"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            string studentId = data.Where(m => m.FullName == cbStudentEvaluate.Text).FirstOrDefault().StudentId;
            evaluate.StudentId = studentId;
            evaluate.ClassId = cbClassEvaluate.Text;
            evaluate.Understand = tbUnderstand.Text.Trim();
            evaluate.Punctuality = tbPunctuality.Text.Trim();
            evaluate.Support = tbSupport.Text.Trim();
            evaluate.Teaching = tbTeaching.Text.Trim();

            var jsonEvalua = JsonConvert.SerializeObject(evaluate);
            var content = new StringContent(jsonEvalua, Encoding.UTF8, "application/json");
            await client.PutAsync($@"{pathEvalua}/{studentId}{cbClassEvaluate.Text}", content);

            ClearEvalueate();
            DataEvaluate();
            MessageBox.Show("Update Successfully", "Message");

        }
        private void dgvEvaluate_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvEvaluate.ClearSelection();
        }
        private bool validateEvaluate()
        {
            HttpResponseMessage responseStudent = client.GetAsync($@"{pathStudent}").Result;
            var resultStudent = responseStudent.Content.ReadAsStringAsync().Result;
            var data = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);


            string studentId = data.Where(m => m.FullName == cbStudentEvaluate.Text).FirstOrDefault().StudentId;

            HttpResponseMessage response = client.GetAsync($"{pathEvalua}/{studentId + cbClassEvaluate.Text}").Result;
            var result = response.Content.ReadAsStringAsync().Result;
            var results = JsonConvert.DeserializeObject<EvaluateDTO>(result);

            var check = true;
            if (results != null)
            {
                lbErrorEvaluate.Text = "Students had an evaluate in this class";
                check = false;
            }
            else
            {
                lbErrorEnroll.Text = "";
            }
            return check;
        }

        private void cbModuleName_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseModule = client.GetAsync($"{pathModule}").Result;
            var resultModule = responseModule.Content.ReadAsStringAsync().Result;
            var list_module = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            HttpResponseMessage responseClass = client.GetAsync($"{pathClass}").Result;
            var resultClass = responseClass.Content.ReadAsStringAsync().Result;
            var list_Class = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseEnroll = client.GetAsync($@"{pathEnroll}").Result;
            var resultEnroll = responseEnroll.Content.ReadAsStringAsync().Result;
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);
            // Chart 1
            chart1.ChartAreas[0].AxisX.Title = "Class";
            chart1.ChartAreas[0].AxisY.Title = "Passed Percent";
            chart1.Series["Percent"].XValueMember = "ClassId";
            chart1.Series["Percent"].YValueMembers = "Passed";
            chart1.DataSource = list_Class.Join(dataEnroll, clas => clas.ClassId, enrol => enrol.ClassId, (clas, enrol) => new
            {
                ClassId = clas.ClassId,
                Passed = enrol.Passed,
                ModuleId = clas.ModuleId
            }).Where(m => list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName == cbModuleName.Text).GroupBy(m => m.ClassId).Select(g => new
            {
                ClassId = g.Key,
                Passed = g.Count(m => m.Passed == 1) * 1.0 / (g.Count() * 1.0) * 100.0
                //Passed = g.Average(m => m.Passed) * 100
            }).ToList();
            chart1.DataBind();
            // Chart 2
            chart2.Series["AvgExam"].XValueMember = "ClassId";
            chart2.Series["AvgExam"].YValueMembers = "ExamGrade";
            chart2.DataSource = list_Class.Join(dataEnroll, clas => clas.ClassId, enrol => enrol.ClassId, (clas, enrol) => new
            {
                ClassId = clas.ClassId,
                ExamGrade = Convert.ToString(enrol.ExamGrade.Split('%')[0]),
                ModuleId = clas.ModuleId
            }).Where(m => list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName == cbModuleName.Text).GroupBy(m => m.ClassId).Select(g => new
            {
                ClassId = g.Key,
                ExamGrade = g.Average(m => Convert.ToInt32(m.ExamGrade))
            }).ToList();
            chart2.DataBind();
            // Chart 3           
            chart3.Series["Student"].XValueMember = "ClassId";
            chart3.Series["Student"].YValueMembers = "StudentId";
            chart3.DataSource = list_Class.Join(dataEnroll, clas => clas.ClassId, enrol => enrol.ClassId, (clas, enrol) => new
            {
                ClassId = clas.ClassId,
                StudentId = enrol.StudentId,
                ModuleId = clas.ModuleId
            }).Where(m => list_module.Where(x => x.ModuleId == m.ModuleId).FirstOrDefault().ModuleName == cbModuleName.Text).GroupBy(m => m.ClassId).Select(g => new
            {
                ClassId = g.Key,
                StudentId = g.Count()
            }).ToList();
            chart3.DataBind();


        }


    }
}
