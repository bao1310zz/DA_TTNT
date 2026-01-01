using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTNT_BUS;         // Gọi lớp BUS
using TTNT_DAL.Models;  // Gọi lớp Model

namespace TTNT_GUI // <-- Đổi tên namespace này trùng với project của bạn
{
    public partial class FormDFS : Form
    {
        // Khai báo biến
        private List<Dinh> _dsDinh = new List<Dinh>();
        private List<Canh> _dsCanh = new List<Canh>();
        private XuLyDoThi _bus = new XuLyDoThi();
        private List<Canh> _duongDi = new List<Canh>();

        public FormDFS()
        {
            InitializeComponent();
        }

        // --- 1. NÚT CHỌN FILE ĐỒ THỊ ---
        private void btnChonFile_Click(object sender, EventArgs e) // Check tên nút này
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Text Files|*.txt" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _duongDi.Clear();
                DocFileVaTaoToaDo(ofd.FileName);
                picGraph.Invalidate(); // Vẽ lại (Nhớ đặt tên PictureBox là picGraph)
                MessageBox.Show("Đã tải xong! Sẵn sàng chạy.");
            }
        }

        // Hàm hỗ trợ đọc file (GUI tự xử lý việc hiển thị)
        private void DocFileVaTaoToaDo(string path)
        {
            _dsDinh.Clear(); _dsCanh.Clear(); lstLog.Items.Clear();
            var lines = File.ReadAllLines(path);
            int n = int.Parse(lines[0].Trim());

            // Xếp đỉnh thành vòng tròn
            int w = picGraph.Width, h = picGraph.Height;
            int r = Math.Min(w, h) / 2 - 40;

            for (int i = 0; i < n; i++)
            {
                double rad = 2 * Math.PI * i / n;
                _dsDinh.Add(new Dinh
                {
                    Id = i + 1,
                    Ten = (i + 1).ToString(),
                    X = (int)(w / 2 + r * Math.Cos(rad)),
                    Y = (int)(h / 2 + r * Math.Sin(rad)),
                    Mau = 0
                });
            }

            // Đọc ma trận kề
            for (int i = 0; i < n; i++)
            {
                var row = lines[i + 1].Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int j = 0; j < n; j++)
                    if (int.Parse(row[j]) > 0)
                        _dsCanh.Add(new Canh { TuDinh = i + 1, DenDinh = j + 1 });
            }
        }

        // --- 2. NÚT CHẠY DFS (GỌI BUS) ---
        private async void btnChayDFS_Click(object sender, EventArgs e)
        {
            if (_dsDinh.Count == 0) return;

            // 1. RESET TRẠNG THÁI
            lstLog.Items.Clear();
            _duongDi.Clear();
            foreach (var d in _dsDinh) d.Mau = 0;
            picGraph.Invalidate();

            // 2. GỌI BUS LẤY KỊCH BẢN
            int startId = _dsDinh[0].Id;
            var kichBan = _bus.ChayDFS(_dsDinh, _dsCanh, startId);

            // --- [MỚI] XỬ LÝ HIỆN ĐÁP ÁN ---

            // Tạo chuỗi kết quả: "1 -> 2 -> 5 -> 4 -> 3"
            string dapAn = string.Join(" -> ", kichBan.Select(b => b.DinhId));

            // Yêu cầu 1: Hiện lên MessageBox
            MessageBox.Show("Thứ tự duyệt DFS tìm được là:\n\n" + dapAn, "Đáp án");

            // Yêu cầu 2: Ghi vào ListBox (Ghi lên đầu cho dễ thấy)
            lstLog.Items.Add("=== KẾT QUẢ: " + dapAn + " ===");
            lstLog.Items.Add("--------------------------------");
            // -------------------------------

            // 3. DIỄN HOẠT (VẼ TỪNG BƯỚC)
            foreach (var buoc in kichBan)
            {
                // Xử lý vẽ đường nối đỏ (Cha -> Con)
                if (buoc.TuDinh != -1)
                {
                    var canhNoi = _dsCanh.FirstOrDefault(c =>
                        (c.TuDinh == buoc.TuDinh && c.DenDinh == buoc.DinhId) ||
                        (c.DenDinh == buoc.TuDinh && c.TuDinh == buoc.DinhId));

                    if (canhNoi != null) _duongDi.Add(canhNoi);
                }

                // Tô màu đỉnh
                var d = _dsDinh.First(x => x.Id == buoc.DinhId);
                d.Mau = 2; // Đỏ

                // Ghi log chi tiết từng bước
                lstLog.Items.Add(buoc.ThongBao);
                lstLog.SelectedIndex = lstLog.Items.Count - 1; // Tự cuộn xuống

                picGraph.Invalidate(); // Vẽ lại

                // Tốc độ chạy (500ms)
                await Task.Delay(500);
            }
        }

        // 2. HÀM VẼ (PAINT)
        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (_dsDinh.Count == 0) return;

            // Vẽ tất cả cạnh mờ (xám)
            using (Pen pXam = new Pen(Color.LightGray, 2))
            {
                foreach (var c in _dsCanh)
                {
                    var u = _dsDinh.First(d => d.Id == c.TuDinh);
                    var v = _dsDinh.First(d => d.Id == c.DenDinh);
                    e.Graphics.DrawLine(pXam, u.X, u.Y, v.X, v.Y);
                }
            }

            // Vẽ ĐƯỜNG CHẠY (Màu Đỏ) đè lên
            using (Pen pDo = new Pen(Color.Red, 3))
            {
                foreach (var c in _duongDi.ToList())
                {
                    var u = _dsDinh.First(d => d.Id == c.TuDinh);
                    var v = _dsDinh.First(d => d.Id == c.DenDinh);
                    e.Graphics.DrawLine(pDo, u.X, u.Y, v.X, v.Y);
                }
            }

            // Vẽ đỉnh
            foreach (var d in _dsDinh)
            {
                Brush b = (d.Mau == 2) ? Brushes.Red : Brushes.White;
                e.Graphics.FillEllipse(b, d.X - 15, d.Y - 15, 30, 30);
                e.Graphics.DrawEllipse(Pens.Black, d.X - 15, d.Y - 15, 30, 30);
                e.Graphics.DrawString(d.Ten, this.Font, Brushes.Black, d.X - 5, d.Y - 5);
            }
        }
    }
}