using System;
using System.Windows.Forms;

namespace TTNT_GUI // <--- Quan trọng: Namespace đúng theo dự án mới
{
    public partial class FrmLog : Form
    {
        public FrmLog()
        {
            InitializeComponent();
            // Khi bấm tắt (X) thì chỉ ẩn đi
            this.FormClosing += (s, e) => {
                if (e.CloseReason == CloseReason.UserClosing)
                {
                    e.Cancel = true;
                    this.Hide();
                }
            };
        }

        // Hàm ghi log (Giống code cũ của bạn)
        public void GhiLog(string message)
        {
            if (!IsHandleCreated) CreateHandle();

            // Thêm thời gian vào trước
            lstLog.Items.Add($"[{DateTime.Now:HH:mm:ss}] {message}");

            // Tự cuộn xuống dưới cùng
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        public void XoaLog() => lstLog.Items.Clear();
    }
}