using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileArchiver
{
    public partial class Form1 : Form
    {
        private BindingList<FileToMove> _files = new BindingList<FileToMove>();

        public Form1()
        {
            InitializeComponent();

            backgroundWorker1.WorkerReportsProgress = true;
            backgroundWorker1.WorkerSupportsCancellation = true;
            backgroundWorker1.DoWork += new DoWorkEventHandler(backgroundWorker1_DoWork);
            backgroundWorker1.ProgressChanged += new ProgressChangedEventHandler(backgroundWorker1_ProgressChanged);
            backgroundWorker1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(backgroundWorker1_RunWorkerCompleted);

            fileToMoveBindingSource.SuspendBinding();
            fileToMoveBindingSource.DataSource = _files;
            fileToMoveBindingSource.ResumeBinding();
        }



        private void butRun_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync();
        }

        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            var topLevel = new System.IO.DirectoryInfo(txtFolder.Text);
            WalkDirectoryTree(topLevel);
        }

        private void backgroundWorker1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

        }

        private void WalkDirectoryTree(System.IO.DirectoryInfo root)
        {
            System.IO.FileInfo[] files = null;
            System.IO.DirectoryInfo[] subDirs = null;

            // First, process all the files directly under this folder
            try
            {
                files = root.GetFiles("*.*");
            }
            // This is thrown if even one of the files requires permissions greater
            // than the application provides.
            catch (UnauthorizedAccessException e)
            {
                // This code just writes out the message and continues to recurse.
                // You may decide to do something different here. For example, you
                // can try to elevate your privileges and access the file again.
                AddLogMessage(e.Message);
            }

            catch (System.IO.DirectoryNotFoundException e)
            {
                Console.WriteLine(e.Message);
            }

            if (files != null)
            {
                foreach (System.IO.FileInfo fi in files)
                {
                    // In this example, we only access the existing FileInfo object. If we
                    // want to open, delete or modify the file, then
                    // a try-catch block is required here to handle the case
                    // where the file has been deleted since the call to TraverseTree().
                    Console.WriteLine(fi.FullName);
                    var age = DateTime.Now - fi.LastWriteTime;
                    if (age.TotalDays > Convert.ToDouble(numDaysBack.Value))
                    {
                        var toMove = new FileToMove(fi.FullName, PathType.File, fi.Length / (1024 * 1024), age.TotalDays, Status.Found);
                        AddFile(toMove);

                        if (chkMove.Checked)
                        {
                            // add the trailing backslash for removal
                            var relativePath = fi.FullName.Replace(txtFolder.Text + "\\", "");
                            var topLevel = txtDestination.Text;
                            var dest = System.IO.Path.Combine(topLevel, relativePath);
                            try
                            {
                                new System.IO.FileInfo(dest).Directory.Create();
                                fi.MoveTo(dest);
                                toMove.status = Status.Success;
                            }
                            catch (Exception e)
                            {
                                AddLogMessage(dest + ": " + e.Message);
                            }

                        }

                    }

                }

                // Now find all the subdirectories under this directory.
                subDirs = root.GetDirectories();

                foreach (System.IO.DirectoryInfo dirInfo in subDirs)
                {
                    // Resursive call for each subdirectory.
                    WalkDirectoryTree(dirInfo);
                }
            }
        }

        private void AddLogMessage(string Message)
        {
            if (txtLog.InvokeRequired)
            {
                // This is how you force your logic to be called on the main
                // application thread
                txtLog.Invoke(new DoUIWorkHandler(_AddLogMessage), Message);
            }
            else
            {
                _AddLogMessage(Message);
            }
        }

        private delegate void DoUIWorkHandler(string Message);
        private void _AddLogMessage(string Message)
        {
            txtLog.AppendText(System.Environment.NewLine + Message);
        }



        private void AddFile(FileToMove ToMove)
        {
            if (dataGridView1.InvokeRequired)
            {
                // This is how you force your logic to be called on the main
                // application thread
                dataGridView1.Invoke(new DoUIWorkHandler2(_AddFile), ToMove);
            }
            else
            {
                _AddFile(ToMove);
            }
        }

        private delegate void DoUIWorkHandler2(FileToMove ToMove);
        private void _AddFile(FileToMove ToMove)
        {
            _files.Add(ToMove);
        }








        private void butBrowse_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtFolder.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtFolder.Text = folderBrowserDialog1.SelectedPath;
            }
        }
        private void butDestination_Click(object sender, EventArgs e)
        {
            folderBrowserDialog1.SelectedPath = txtDestination.Text;
            if (folderBrowserDialog1.ShowDialog() == DialogResult.OK)
            {
                txtDestination.Text = folderBrowserDialog1.SelectedPath;
            }
        }
    }



}
