
using Shell32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace MusicPlayer
{
    public partial class FormPlayer : Form
    {
        private int currentRowIndex;
        private string filePath;
        public FormPlayer()
        {
            InitializeComponent();
            filePath = Directory.GetCurrentDirectory()+ "\\music\\data.json";
        }


        private void FormPlayer_Shown(object sender, EventArgs e)
        {
            if (!File.Exists(filePath))
                InitContent();
            var content = File.ReadAllText(filePath);
            var result = Newtonsoft.Json.JsonConvert.DeserializeObject<List<MusicFileInfo>>(content);
            this.dgvList.Rows.Add(result.Count);
            for (int i = 0; i < result.Count; i++)
            {
                var row = this.dgvList.Rows[i];
                var musicFile = result[i];
                row.Cells["colName"].Value = musicFile.Name;
                row.Cells["colSize"].Value = musicFile.Size;
                row.Cells["colPath"].Value = musicFile.Path;
                row.Cells["colTime"].Value = musicFile.Time;
            }
        }

        private void InitContent() {
            var folder = Directory.CreateDirectory("music").FullName;
            var files = Directory.GetFiles(folder, "*.mp3", SearchOption.AllDirectories);
            if (files.Length == 0)
                return;
            List<MusicFileInfo> list = new List<MusicFileInfo>();
            ShellClass shellClass = new ShellClass();
            for (int i = 0; i < files.Length; i++)
            {
                var filePath = files[i];
                var dirName = Path.GetDirectoryName(filePath);
                var fileName = Path.GetFileName(filePath);
                var space = shellClass.NameSpace(dirName);
                FolderItem item = space.ParseName(fileName);
                var time = Regex.Match(space.GetDetailsOf(item, -1), "\\d{2}:\\d{2}:\\d{2}").Value;
                MusicFileInfo musicFile = new MusicFileInfo();
                musicFile.Size = (new FileInfo(filePath).Length / 1024 / 1024.0d).ToString("#,#.00") + "M";
                musicFile.Name = fileName;
                
                musicFile.Path = @"\music\"+fileName;
                musicFile.Time = time;
                list.Add(musicFile);
            }
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(list);
            File.WriteAllText(filePath, json);
        }
        private void dgvList_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            currentRowIndex = e.RowIndex;
            this.PlayMusic();
        }

        private void btnStop_Click(object sender, EventArgs e)
        {
            PlayerUtils.Stop();
        }

        private void btnPlayer_Click(object sender, EventArgs e)
        {
            this.PlayMusic();
        }

        private void PlayMusic()
        {
            if (currentRowIndex < 0)
                currentRowIndex = this.dgvList.Rows.Count - 1;
            else if (currentRowIndex >= this.dgvList.Rows.Count)
                currentRowIndex = 0;

            this.dgvList.Rows[currentRowIndex].Selected = true;
            this.dgvList.CurrentCell = this.dgvList.Rows[currentRowIndex].Cells["colPath"];

            var path = Directory.GetCurrentDirectory() + this.dgvList.Rows[currentRowIndex].Cells["colPath"].Value.ToString();
            if (!PlayerUtils.Play(path, this.Handle))
            {
                this.btnNext_Click(null, null);
            }
            notifyIcon1.Text = PlayerUtils.CurrentMusic;
        }

        private void btnPause_Click(object sender, EventArgs e)
        {
            PlayerUtils.Pause();
        }

        private void btnList_Click(object sender, EventArgs e)
        {
            this.PlayMusic();
        }

        private void btnResume_Click(object sender, EventArgs e)
        {
            PlayerUtils.Resume();
        }

        private void btnBefore_Click(object sender, EventArgs e)
        {
            currentRowIndex--;
            this.PlayMusic();
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            currentRowIndex++;
            this.PlayMusic();
        }
        private const int MM_MCINOTIFY = 0x03b9;
        private const int MCI_NOTIFY_SUCCESS = 0x01;
        private const int MCI_NOTIFY_SUPERSEDED = 0x02;
        private const int MCI_NOTIFY_ABORTED = 0x04;
        private const int MCI_NOTIFY_FAILURE = 0x08;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == MM_MCINOTIFY)
            {
                switch (m.WParam.ToInt32())
                {
                    case MCI_NOTIFY_SUCCESS:
                        this.btnNext_Click(null, null);
                        break;
                    case MCI_NOTIFY_SUPERSEDED:
                        // superseded handling
                        break;
                    case MCI_NOTIFY_ABORTED:
                        // abort handling
                        break;
                    case MCI_NOTIFY_FAILURE:
                        this.btnNext_Click(null, null);
                        break;
                    default:
                        // haha
                        break;
                }
            }
            base.WndProc(ref m);
        }

        private void btnExit_Click(object sender, EventArgs e)
        {
            this.Close();
            Application.Exit();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            PlayerUtils.GetMusicLength();
        }
        private void FormPlayer_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                e.Cancel = true;    //取消"关闭窗口"事件
                this.WindowState = FormWindowState.Minimized;    //使关闭时窗口向右下角缩小的效果
                notifyIcon1.Visible = true;
                this.Hide();
                return;
            }
        }

        private void notifyIcon1_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            this.WindowState = FormWindowState.Maximized;
            this.Visible = true;
        }

        private void tsmiStart_Click(object sender, EventArgs e)
        {
            this.btnPlayer_Click(sender, e);
        }
    }
}
