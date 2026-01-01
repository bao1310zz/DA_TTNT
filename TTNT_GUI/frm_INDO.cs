using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTNT_BUS;         // Gọi lớp BUS
using TTNT_DAL;
using TTNT_DAL.Models;

namespace TTNT_GUI
{
    public partial class frm_INDO : Form
    {
        // --- KHAI BÁO CÁC LỚP 3 TẦNG ---
        private GraphRepository _repo = new GraphRepository(); // DAL
        private XuLyDoThi _bus = new XuLyDoThi();            // BUS

        private List<Dinh> _dsDinh = new List<Dinh>();
        private List<Canh> _dsCanh = new List<Canh>();

        // (Giữ nguyên các biến màu sắc, font chữ như code trước để vẽ cho đẹp)
        private readonly Brush _brushNormal = new SolidBrush(Color.FromArgb(220, 240, 255));
        private readonly Brush _brushVisiting = new SolidBrush(Color.FromArgb(255, 200, 80));
        private readonly Brush _brushVisited = new SolidBrush(Color.FromArgb(100, 200, 100));
        private readonly Pen _penEdge = new Pen(Color.Gray, 1.5f);
        private readonly Font _fontNode = new Font("Segoe UI", 10, FontStyle.Bold);

        public frm_INDO()
        {
            InitializeComponent();

            this.btnChonFile.Click += new System.EventHandler(this.btnChonFile_Click);
            this.btnChay.Click += new System.EventHandler(this.btnChay_Click);
            this.picGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.picGraph_Paint);
        }

        // --- SỰ KIỆN CHỌN FILE (Giữ nguyên logic) ---
        private void btnChonFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var data = _repo.DocDoThi(ofd.FileName); // Gọi DAL
                    _dsDinh = data.Item1;
                    _dsCanh = data.Item2;
                    foreach (var d in _dsDinh) d.Mau = 0;
                    PopulateComboBoxes();
                    picGraph.Invalidate();
                    AddLog("--- Đã tải dữ liệu ---");
                }
                catch (Exception ex) { MessageBox.Show(ex.Message); }
            }
        }

        // --- SỰ KIỆN CHẠY INDO (GỌI BUS) ---
        private async void btnChay_Click(object sender, EventArgs e)
        {
            if (_dsDinh.Count == 0) return;

            // 1. Chuẩn bị giao diện
            btnChay.Enabled = false; btnChonFile.Enabled = false;
            lstLog.Items.Clear();
            AddLog("=== BẮT ĐẦU INDO (3-LAYER) ===");

            try
            {
                int startId = (int)cmbStart.SelectedValue;
                int endId = (int)cmbEnd.SelectedValue;

                // 2. GỌI BUS ĐỂ LẤY KỊCH BẢN (KHÔNG XỬ LÝ LOGIC Ở ĐÂY)
                // Đây là bước quan trọng nhất của mô hình 3 lớp
                List<BuocDuyet> kichBan = _bus.TimKiemINDO(_dsDinh, _dsCanh, startId, endId);

                // 3. DIỄN HOẠT (REPLAY) LẠI KỊCH BẢN TRÊN GIAO DIỆN
                foreach (var buoc in kichBan)
                {
                    // Xử lý logic Reset đặc biệt của INDO
                    if (buoc.IsReset)
                    {
                        foreach (var node in _dsDinh) node.Mau = 0;
                        AddLog(buoc.ThongBao);
                        picGraph.Invalidate();
                        await DelayStep(500); // Nghỉ lâu hơn xíu khi reset
                        continue;
                    }

                    // Cập nhật trạng thái đỉnh
                    var d = _dsDinh.FirstOrDefault(x => x.Id == buoc.DinhId);
                    if (d != null)
                    {
                        d.Mau = buoc.Mau;
                    }

                    // Ghi log nếu có
                    if (!string.IsNullOrEmpty(buoc.ThongBao))
                    {
                        AddLog(buoc.ThongBao);
                    }

                    // Vẽ lại và Delay
                    picGraph.Invalidate();
                    await DelayStep(300);
                }

                MessageBox.Show("Hoàn tất thuật toán!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                btnChay.Enabled = true; btnChonFile.Enabled = true;
            }
        }

        // --- HÀM VẼ (GIỮ NGUYÊN) ---
        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            if (_dsDinh == null || _dsDinh.Count == 0) return;

            // Vẽ cạnh
            foreach (var c in _dsCanh)
            {
                var u = _dsDinh.FirstOrDefault(d => d.Id == c.TuDinh);
                var v = _dsDinh.FirstOrDefault(d => d.Id == c.DenDinh);
                if (u != null && v != null) e.Graphics.DrawLine(_penEdge, u.X, u.Y, v.X, v.Y);
            }

            // Vẽ đỉnh
            int r = 18; int d_size = r * 2;
            foreach (var node in _dsDinh)
            {
                Brush currentBrush = _brushNormal;
                if (node.Mau == 1) currentBrush = _brushVisiting;
                if (node.Mau == 2) currentBrush = _brushVisited;

                e.Graphics.FillEllipse(currentBrush, node.X - r, node.Y - r, d_size, d_size);
                e.Graphics.DrawEllipse(Pens.DimGray, node.X - r, node.Y - r, d_size, d_size);

                string txt = node.Ten ?? node.Id.ToString();
                SizeF size = e.Graphics.MeasureString(txt, _fontNode);
                e.Graphics.DrawString(txt, _fontNode, Brushes.Black, node.X - size.Width / 2, node.Y - size.Height / 2);
            }
        }

        // --- HÀM PHỤ TRỢ (HELPER) ---
        private void PopulateComboBoxes()
        {
            if (cmbStart == null || cmbEnd == null) return;
            var items = _dsDinh.Select(d => new { d.Id, Ten = d.Ten }).ToList();
            cmbStart.DataSource = new List<object>(items); cmbStart.DisplayMember = "Ten"; cmbStart.ValueMember = "Id";
            cmbEnd.DataSource = new List<object>(items); cmbEnd.DisplayMember = "Ten"; cmbEnd.ValueMember = "Id";
            if (_dsDinh.Count > 0) cmbStart.SelectedIndex = 0;
            if (_dsDinh.Count > 1) cmbEnd.SelectedIndex = _dsDinh.Count - 1;
        }

        private void AddLog(string msg)
        {
            lstLog.Items.Add(msg);
            lstLog.TopIndex = lstLog.Items.Count - 1;
        }

        private async Task DelayStep(int baseDelay)
        {
            int delay = baseDelay;
            if (trackBarTocDo != null)
            {
                delay = (trackBarTocDo.Maximum + trackBarTocDo.Minimum) - trackBarTocDo.Value;
                if (delay < 10) delay = 10;
            }
            await Task.Delay(delay);
        }
    }
}