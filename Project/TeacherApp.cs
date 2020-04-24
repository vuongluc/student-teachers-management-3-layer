
using Newtonsoft.Json;
using Project.DTO;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.AccessControl;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Project
{
    public partial class TeacherApp : Form
    {
        string url = @"http://localhost:52593/";
        string pathModule = @"api/modules";
        string pathStudent = @"api/students";
        string pathClass = @"api/classes";
        string pathEnroll = @"api/enrolls";
        string pathEvalua = @"api/evaluates";
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
        string teacherIdDefault = null;
        public string teacherId; 
        public TeacherApp()
        {
            client = new HttpClient();
            client.BaseAddress = new Uri(url);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));
            InitializeComponent();
        }
        private void TeacherApp_FormClosing(object sender, FormClosingEventArgs e)
        {
            DialogResult closeApp = MessageBox.Show("Are you sure you want to exit the program?", "Notification", MessageBoxButtons.YesNo);
            if (closeApp == DialogResult.No)
            {
                e.Cancel = true;
            }
        }
        private async void TeacherApp_Load(object sender, EventArgs e)
        {

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            HttpResponseMessage responseEvalua = await client.GetAsync($"{pathEvalua}");
            var resultEvalua = await responseEvalua.Content.ReadAsStringAsync();
            var dataEvalua = JsonConvert.DeserializeObject<List<EvaluateDTO>>(resultEvalua);

            teacherIdDefault = teacherId;
            // Class
           
            DisplayData();

            dgvGrade.Columns[0].ReadOnly = true;
            dgvGrade.Columns[1].ReadOnly = true;

            dgvClass.Columns[0].ReadOnly = true;
            dgvClass.Columns[2].ReadOnly = true;
            dgvClass.Columns[3].ReadOnly = true;
            dgvClass.Columns[4].ReadOnly = true;
            dgvClass.Columns[5].ReadOnly = true;
            btnUpdateTeachingHour.Enabled = false;
            // Grade
            cbClassGrade.DataSource = dataClass.Where(m => m.TeacherId == teacherIdDefault).Select(m => m.ClassId).ToList();
            //DataEnroll();
            btnSaveGrade.Enabled = false;
            dgvEvaluate.Columns[1].Visible = false;
            // Evaluate
            var list_class = dataClass.Where(m => m.TeacherId != teacherIdDefault).Select(m => m.ClassId).ToList();
            var result = dataEvalua;
            foreach (var item in list_class)
            {
                result = result.Where(m => m.ClassId != item).ToList();
            }
            dgvEvaluate.DataSource = result.GroupBy(m => m.ClassId).Select(g => new
            {               
                ClassId = g.Key,
                Understand = g.Average(m => Convert.ToInt32(m.Understand)),
                Punctuality = g.Average(m => Convert.ToInt32(m.Punctuality)),
                Support = g.Average(m => Convert.ToInt32(m.Support)),
                Teaching = g.Average(m => Convert.ToInt32(m.Teaching)),
            });

            var list_combobox = dataClass.Where(m => m.TeacherId == teacherIdDefault && m.StatusId == "CE").Select(m => m.ClassId).ToList();
            cbEvalua.Items.Add("All");
            foreach(var item in list_combobox)
            {
                cbEvalua.Items.Add(item);
            }
            cbEvalua.SelectedIndex = 0;
           

        }


       

        // Class
        async void DisplayData()
        {

            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            dgvClass.DataSource = dataClass.Where(m => m.TeacherId == teacherIdDefault).ToList();
        }
        private async void btnUpdateTeachingHour_Click(object sender, EventArgs e)
        {
            if (lbErrorTeachingHour.Text == "")
            {
                HttpResponseMessage responseModuel = await client.GetAsync($"{pathModule}");
                var resultModule = await responseModuel.Content.ReadAsStringAsync();
                var dataModule = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

                HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
                var resultClass = await responseClass.Content.ReadAsStringAsync();
                var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

                foreach (DataGridViewRow row in dgvClass.Rows)
                {
                    
                    string moduleId = row.Cells["ModuleId"].Value.ToString();
                    string ModuleName = dataModule.Where(m=>m.ModuleId == moduleId).FirstOrDefault().ModuleName;
                    int duration = dataModule.Where(m => m.ModuleId == moduleId).FirstOrDefault().Duration;
                    string classId = Convert.ToString(row.Cells["ClassId"].Value);
                    var classCurrent = dataClass.Where(m=>m.ClassId == classId).FirstOrDefault();
                    if(Convert.ToString(row.Cells["TeachingHour"].Value) != "")
                    {
                        int teacherHour = Convert.ToInt32(row.Cells["TeachingHour"].Value.ToString());
                        classes.ClassId = Convert.ToString(row.Cells["ClassId"].Value);
                        classes.TeachingHour = Convert.ToInt32(row.Cells["TeachingHour"].Value.ToString());
                        classes.ModuleId = Convert.ToString(row.Cells["ModuleId"].Value);
                        classes.StatusId = Convert.ToString(row.Cells["StatusId"].Value);
                        classes.TeacherId = Convert.ToString(row.Cells["TeacherId"].Value);
                        classes.TypeId = Convert.ToString(row.Cells["TypeId"].Value);
                        if (Convert.ToInt32(row.Cells["TeachingHour"].Value.ToString()) == duration)
                        {
                            classes.ClassId = Convert.ToString(row.Cells["ClassId"].Value);
                            classes.TeachingHour = Convert.ToInt32(row.Cells["TeachingHour"].Value.ToString());
                            classes.ModuleId = Convert.ToString(row.Cells["ModuleId"].Value);
                            classes.StatusId = "CE";
                            classes.TeacherId = Convert.ToString(row.Cells["TeacherId"].Value);
                            classes.TypeId = Convert.ToString(row.Cells["TypeId"].Value);
                           
                        }
                    }
                    else
                    {
                        classes.ClassId = classCurrent.ClassId;
                        classes.TeachingHour = classCurrent.TeachingHour;
                        classes.ModuleId = classCurrent.ModuleId;
                        classes.StatusId = classCurrent.StatusId;
                        classes.TeacherId = classCurrent.TeacherId;
                        classes.TypeId = classCurrent.TypeId;
                    }

                    var jsonClass = JsonConvert.SerializeObject(classes);
                    var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
                    await client.PutAsync($@"{pathClass}/{classCurrent.ClassId}", content);
                }
                //bizClass.updateClass(DTOEFMapper.GetDtoFromEntity(classes));
                MessageBox.Show("Update Sucessfully", "Message");   
                DisplayData();             
            }
        }
        private void dgvClass_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            dgvClass.ClearSelection();
        }
        
        private void dgvClass_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;
            tb.KeyPress += new KeyPressEventHandler(dgvClass_KeyPress);

            e.Control.KeyPress += new KeyPressEventHandler(dgvClass_KeyPress);
        }
        private void dgvClass_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (Char.IsDigit(e.KeyChar) == false)
            {
                lbErrorTeachingHour.Text = "Please enter a number";
            }
            else
            {
                btnUpdateTeachingHour.Enabled = true;
                lbErrorTeachingHour.Text = "";
            }
        }
        private void dgvClass_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }
        private async void dgvClass_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            HttpResponseMessage responseModuel = await client.GetAsync($"{pathModule}");
            var resultModule = await responseModuel.Content.ReadAsStringAsync();
            var dataModule = JsonConvert.DeserializeObject<List<ModuleDTO>>(resultModule);

            DataGridViewRow indexRow = ((DataGridView)sender).Rows[e.RowIndex];
            int teacherHour = Convert.ToInt32(indexRow.Cells["TeachingHour"].Value.ToString());
            string moduleId = indexRow.Cells["ModuleId"].Value.ToString();
            string ModuleName = dataModule.Where(m => m.ModuleId == moduleId).FirstOrDefault().ModuleName;
            int duration = dataModule.Where(m => m.ModuleId == moduleId).FirstOrDefault().Duration;
            if (teacherHour > duration)
            {
                indexRow.ErrorText = ModuleName + " maximum is " + duration;
                lbErrorTeachingHour.Text = ModuleName + " maximum is " + duration;
            }
            else
            {
                btnUpdateTeachingHour.Enabled = true;
                indexRow.ErrorText = "";
                lbErrorTeachingHour.Text = "";
            }
        }
        private void dgvClass_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            MessageBox.Show(e.ColumnIndex.ToString());
        }
        private async void btnCancelEditTeachingHour_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);

            lbErrorTeachingHour.Text = "";
            dgvClass.DataSource = dataClass.Where(m => m.TeacherId == teacherIdDefault).ToList();
        }


        ////////////////////////////////////////////////////////////////////////////
        // Grade

        async void DataEnroll()
        {
            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            dgvGrade.AutoGenerateColumns = false;
            // Edit trực tiếp được trên datagirdview
            dgvGrade.DataSource = dataEnroll.Where(m => m.ClassId == cbClassGrade.Text).ToList();
           

        }       
        private async void cbClassGrade_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            dgvGrade.DataSource = dataEnroll.Where(m => m.ClassId == cbClassGrade.Text).ToList();
            
        }

       
       
        private async void tbSearchStudent_TextChanged(object sender, EventArgs e)
        {
            if (tbSearchStudent.Text != "")
            {
                pbCloseSearchStudent.Visible = true;
            }
            else
            {
                pbCloseSearchStudent.Visible = false;
            }

            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}/{tbSearchStudent.Text}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

            dgvGrade.DataSource = dataEnroll.Where(m => m.ClassId == cbClassGrade.Text).ToList();
            //foreach (DataGridViewRow row in dgvGrade.Rows)
            //{
            //    string studentId = row.Cells["StudentId"].Value.ToString();
            //    dgvGrade.Rows[row.Index].Cells["StudentFullName"].Value = bizStudent.findById(studentId).FullName;
            //}
        }
        private void pbCloseSearchStudent_Click(object sender, EventArgs e)
        {
            tbSearchStudent.Text = ""; 
        }
  
       
        /////////////////// validate
       
        private void dgvGrade_EditingControlShowing(object sender, DataGridViewEditingControlShowingEventArgs e)
        {
            DataGridViewTextBoxEditingControl tb = (DataGridViewTextBoxEditingControl)e.Control;
            tb.KeyPress += new KeyPressEventHandler(dgvGrade_KeyPress);

            e.Control.KeyPress += new KeyPressEventHandler(dgvGrade_KeyPress);
        }
        private void dgvGrade_KeyPress(object sender, KeyPressEventArgs e)
        {

            if (Char.IsDigit(e.KeyChar) == false)
            {
                lbError.Text = "Please enter a number";
            }
            else
            {
                btnSaveGrade.Enabled = true;
                lbError.Text = "";
            }
        } 
        private void dgvGrade_DataError(object sender, DataGridViewDataErrorEventArgs e)
        {
            e.Cancel = true;
        }
        private void dgvGrade_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            DataGridViewRow indexRow = ((DataGridView)sender).Rows[e.RowIndex];
            Regex pattern = new Regex("^([0-9]){1,3}$");
            if (Convert.ToString(dgvGrade.Rows[e.RowIndex].Cells[8].Value) != "" && Convert.ToString(dgvGrade.Rows[e.RowIndex].Cells[8].Value).Contains("%") == false)
            {
                if (pattern.IsMatch(Convert.ToString(dgvGrade.Rows[e.RowIndex].Cells[8].Value)) == false)
                {
                    indexRow.ErrorText = "Please enter a number";
                    lbError.Text = "Please enter a number";
                }
                else if (Convert.ToInt32(Convert.ToString(dgvGrade.Rows[e.RowIndex].Cells[8].Value)) > 100 || Convert.ToInt32(Convert.ToString(dgvGrade.Rows[e.RowIndex].Cells[8].Value)) < 0)
                {
                    indexRow.ErrorText = "ExamGrade range from 0 to 100";
                    lbError.Text = "ExamGrade range from 0 to 100";
                }
                else
                {
                    indexRow.ErrorText = "";
                    lbError.Text = "";
                }
            }
            else if (Convert.ToInt32(dgvGrade[e.ColumnIndex, e.RowIndex].Value) < 0 || Convert.ToInt32(dgvGrade[e.ColumnIndex, e.RowIndex].Value) > 10)
            {
                indexRow.ErrorText = "Please enter the correct number format 0 - 10";
                lbError.Text = "Please enter the correct number format 0 - 10";
            }
            else
            {
                btnSaveGrade.Enabled = true;
                indexRow.ErrorText = "";
                lbError.Text = "";
            }
        }

        //////////////// 
        // Evaluate
        private async void cbEvalua_SelectedIndexChanged(object sender, EventArgs e)
        {
            HttpResponseMessage responseClass = await client.GetAsync($"{pathClass}");
            var resultClass = await responseClass.Content.ReadAsStringAsync();
            var dataClass = JsonConvert.DeserializeObject<List<ClassDTO>>(resultClass);


            HttpResponseMessage responseEvalua = await client.GetAsync($"{pathEvalua}");
            var resultEvalua = await responseEvalua.Content.ReadAsStringAsync();
            var dataEvalua = JsonConvert.DeserializeObject<List<EvaluateDTO>>(resultEvalua);

            if (cbEvalua.SelectedIndex == 0)
            {
                dgvEvaluate.DataSource = dataEvalua.Join(dataClass, eva => eva.ClassId, clas => clas.ClassId, (eva, clas) => new
                {
                    ClassId = eva.ClassId,
                    Punctuality = eva.Punctuality,
                    Understand = eva.Understand,
                    Support = eva.Support,
                    Teaching = eva.Teaching,
                    StatusId = clas.StatusId
                }).Where(m => m.StatusId == "CE").GroupBy(m => m.ClassId).Select(g => new
                {
                    ClassId = g.Key,
                    Understand = g.Average(m => Convert.ToInt32(m.Understand)),
                    Punctuality = g.Average(m => Convert.ToInt32(m.Punctuality)),
                    Support = g.Average(m => Convert.ToInt32(m.Support)),
                    Teaching = g.Average(m => Convert.ToInt32(m.Teaching)),
                }).ToList();
            }else
            {
                dgvEvaluate.DataSource = dataEvalua.Join(dataClass, eva => eva.ClassId, clas => clas.ClassId, (eva, clas) => new
                {
                    ClassId = eva.ClassId,
                    Punctuality = eva.Punctuality,
                    Understand = eva.Understand,
                    Support = eva.Support,
                    Teaching = eva.Teaching,
                    StatusId = clas.StatusId
                }).Where(m => m.ClassId == cbEvalua.Text && m.StatusId == "CE").GroupBy(m => m.ClassId).Select(g => new
                {
                    ClassId = g.Key,
                    Understand = g.Average(m => Convert.ToInt32(m.Understand)),
                    Punctuality = g.Average(m => Convert.ToInt32(m.Punctuality)),
                    Support = g.Average(m => Convert.ToInt32(m.Support)),
                    Teaching = g.Average(m => Convert.ToInt32(m.Teaching)),
                }).ToList();
            }
            

           
        }
            
        private void dgvGrade_ColumnHeaderMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            MessageBox.Show(e.ColumnIndex.ToString());
        }
        private async void btnSaveGrade_Click(object sender, EventArgs e)
        {
            tbSearchStudent.Text = "";
            if(lbError.Text == "")
            {
                HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
                var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
                var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);

                foreach (DataGridViewRow row in dgvGrade.Rows)
                {
                    string fullNameStudent = Convert.ToString(row.Cells["StudentId"].Value);
                    string classId = Convert.ToString(row.Cells["Class"].Value);
                    //string studentId = bizStudent.findAllStudent().Where(m => (m.FirstName + " " + m.LastName) == fullNameStudent).FirstOrDefault().StudentId;
                    var enrolls = dataEnroll.Where(m => m.StudentId + m.ClassId == fullNameStudent + classId).FirstOrDefault();
                    enroll.StudentId = fullNameStudent;
                    enroll.ClassId = classId;
                    if (Convert.ToString(row.Cells["Hw1Grade"].Value) != "")
                    {
                        enroll.Hw1Grade = Convert.ToDouble(row.Cells["Hw1Grade"].Value.ToString());
                    }
                    else
                    {
                        enroll.Hw1Grade = enrolls.Hw1Grade;
                    }
                    if (Convert.ToString(row.Cells["Hw2Grade"].Value) != "")
                    {
                        enroll.Hw2Grade = Convert.ToDouble(row.Cells["Hw2Grade"].Value.ToString());
                    }
                    else
                    {
                        enroll.Hw2Grade = enrolls.Hw2Grade;
                    }
                    if (Convert.ToString(row.Cells["Hw3Grade"].Value) != "")
                    {
                        enroll.Hw3Grade = Convert.ToDouble(row.Cells["Hw3Grade"].Value.ToString());
                    }
                    else
                    {
                        enroll.Hw3Grade = enrolls.Hw3Grade;
                    }
                    if (Convert.ToString(row.Cells["Hw4Grade"].Value) != "")
                    {
                        enroll.Hw4Grade = Convert.ToDouble(row.Cells["Hw4Grade"].Value.ToString());
                    }
                    else
                    {
                        enroll.Hw4Grade = enrolls.Hw4Grade;
                    }
                    if (Convert.ToString(row.Cells["Hw5Grade"].Value) != "")
                    {
                        enroll.Hw5Grade = Convert.ToDouble(row.Cells["Hw5Grade"].Value.ToString());
                    }
                    else
                    {
                        enroll.Hw5Grade = enrolls.Hw5Grade;
                    }

                    if (Convert.ToString(row.Cells["ExamGrade"].Value) != "")
                    {
                        if (Convert.ToString(row.Cells["ExamGrade"].Value).Contains("%"))
                        {
                            enroll.ExamGrade = Convert.ToString(row.Cells["ExamGrade"].Value);
                            if (Convert.ToString(row.Cells["ExamGrade"].Value).Length == 2)
                            {
                                if (Convert.ToInt32(row.Cells["ExamGrade"].Value.ToString().Substring(0, 1)) >= 40)
                                {
                                    enroll.Passed = 1;
                                }
                                else
                                {
                                    enroll.Passed = 0;
                                }
                            }
                            else if (Convert.ToString(row.Cells["ExamGrade"].Value).Length == 3)
                            {
                                if (Convert.ToInt32(row.Cells["ExamGrade"].Value.ToString().Substring(0, 2)) >= 40)
                                {
                                    enroll.Passed = 1;
                                }
                                else
                                {
                                    enroll.Passed = 0;
                                }
                            }
                            else
                            {
                                enroll.Passed = 1;

                            }

                        }                        
                        else
                        {
                            enroll.ExamGrade = Convert.ToString(row.Cells["ExamGrade"].Value) + "%";
                            if (Convert.ToString(row.Cells["ExamGrade"].Value).Length == 1)
                            {
                                if (Convert.ToInt32(row.Cells["ExamGrade"].Value.ToString().Substring(0, 1)) >= 40)
                                {
                                    enroll.Passed = 1;
                                }
                                else
                                {
                                    enroll.Passed = 0;
                                }
                            }
                            else if (Convert.ToString(row.Cells["ExamGrade"].Value).Length == 2)
                            {
                                if (Convert.ToInt32(row.Cells["ExamGrade"].Value.ToString().Substring(0, 2)) >= 40)
                                {
                                    enroll.Passed = 1;
                                }
                                else
                                {
                                    enroll.Passed = 0;
                                }
                            }
                            else
                            {
                                enroll.Passed = 1;
                            }
                        }
                        
                        
                        
                    }
                    else
                    {
                        enroll.Passed = enrolls.Passed;
                        enroll.ExamGrade = enrolls.ExamGrade;
                    }

                    var jsonClass = JsonConvert.SerializeObject(enroll);
                    var content = new StringContent(jsonClass, Encoding.UTF8, "application/json");
                    await client.PutAsync($@"{pathEnroll}/{enrolls.StudentId}{enrolls.ClassId}", content);
                }
                DataEnroll();
                MessageBox.Show("Update Successfully", "Message");
            }
        }
        private async void btnCancelEdit_Click(object sender, EventArgs e)
        {
            HttpResponseMessage responseEnroll = await client.GetAsync($"{pathEnroll}");
            var resultEnroll = await responseEnroll.Content.ReadAsStringAsync();
            var dataEnroll = JsonConvert.DeserializeObject<List<EnrollDTO>>(resultEnroll);


            HttpResponseMessage responseStudent = await client.GetAsync($"{pathStudent}");
            var resultStudent = await responseStudent.Content.ReadAsStringAsync();
            var dataStudent = JsonConvert.DeserializeObject<List<StudentDTO>>(resultStudent);

            dgvGrade.DataSource = dataEnroll.Where(m => m.ClassId == cbClassGrade.Text).ToList();
         
            //foreach (DataGridViewRow row in dgvGrade.Rows)
            //{
            //    string studentId = row.Cells["StudentId"].Value.ToString();
            //    dgvGrade.Rows[row.Index].Cells["StudentFullName"].Value = dataStudent.Where(m=> m.StudentId == studentId).FirstOrDefault().FullName;
            //}
            btnSaveGrade.Enabled = false;
            lbError.Text = "";
        }

        private void TeacherApp_FormClosed(object sender, FormClosedEventArgs e)
        {
            Login frLogin = new Login();
            frLogin.Show();
        }
    }

}
