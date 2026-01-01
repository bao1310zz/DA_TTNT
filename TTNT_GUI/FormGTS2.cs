using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTNT_DAL;
using TTNT_DAL.Models;

namespace TTNT_GUI
{
    public partial class FormGTS2 : Form
    {
        // --- 1. KHAI BÁO BIẾN ---
        GraphRepository _repo = new GraphRepository();
        List<Dinh> _dsDinh = new List<Dinh>();
        List<Canh> _dsCanh = new List<Canh>();

        public FormGTS2()
        {
            InitializeComponent();
        }

        // --- 2. SỰ KIỆN NÚT CHỌN FILE ---
        private void btnChonFile_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";

            if (ofd.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    var data = _repo.DocDoThi(ofd.FileName);
                    _dsDinh = data.Item1;
                    _dsCanh = data.Item2;

                    // Reset màu
                    foreach (var d in _dsDinh) d.Mau = 0;

                    // Đổ dữ liệu vào ComboBox Start
                    PopulateComboBox();

                    picGraph.Invalidate();
                    MessageBox.Show("Đã load xong đồ thị!", "Thông báo");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi đọc file: " + ex.Message);
                }
            }
        }

        // Hàm điền dữ liệu vào ComboBox Start
        private void PopulateComboBox()
        {
            var cmbStart = this.Controls.Find("cmbStart", true).FirstOrDefault() as ComboBox;
            if (cmbStart == null) return;

            // Tạo danh sách hiển thị tên (1, 2, 3...)
            var items = _dsDinh.Select(d => new { d.Id, TenHienThi = d.Ten }).ToList();

            cmbStart.DataSource = new List<object>(items);
            cmbStart.DisplayMember = "TenHienThi";
            cmbStart.ValueMember = "Id";

            if (_dsDinh.Count > 0) cmbStart.SelectedIndex = 0;
        }

        // --- 3. SỰ KIỆN VẼ (Paint) ---
        private void picGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            if (_dsDinh == null || _dsDinh.Count == 0) return;

            // A. VẼ CẠNH
            Pen penCanh = new Pen(Color.Black, 1);
            foreach (var c in _dsCanh)
            {
                var u = _dsDinh.FirstOrDefault(d => d.Id == c.TuDinh);
                var v = _dsDinh.FirstOrDefault(d => d.Id == c.DenDinh);
                if (u != null && v != null)
                {
                    e.Graphics.DrawLine(penCanh, u.X, u.Y, v.X, v.Y);

                    // Vẽ trọng số lên giữa cạnh (để dễ kiểm tra)
                    // string w = c.TrongSo.ToString();
                    // e.Graphics.DrawString(w, this.Font, Brushes.Blue, (u.X + v.X)/2, (u.Y + v.Y)/2);
                }
            }

            // B. VẼ ĐỈNH
            int banKinh = 18;
            Font fontChu = new Font("Arial", 11, FontStyle.Bold);

            foreach (var d in _dsDinh)
            {
                Brush brushToMau = (d.Mau == 2) ? Brushes.OrangeRed : Brushes.LightGreen;

                int drawX = d.X - banKinh;
                int drawY = d.Y - banKinh;

                e.Graphics.FillEllipse(brushToMau, drawX, drawY, banKinh * 2, banKinh * 2);
                e.Graphics.DrawEllipse(Pens.Black, drawX, drawY, banKinh * 2, banKinh * 2);

                string text = d.Ten ?? d.Id.ToString();
                SizeF sizeChu = e.Graphics.MeasureString(text, fontChu);
                e.Graphics.DrawString(text, fontChu, Brushes.White, d.X - sizeChu.Width / 2, d.Y - sizeChu.Height / 2);
            }
        }

        // --- 4. THUẬT TOÁN GTS2 (TSP - Nearest Neighbor) ---
        private async void btnChay_Click(object sender, EventArgs e)
        {
            if (_dsDinh == null || _dsDinh.Count == 0)
            {
                MessageBox.Show("Chưa có dữ liệu đồ thị!");
                return;
            }

            btnChay.Enabled = false;
            btnChonFile.Enabled = false;

            if (lstLog != null) lstLog.Items.Clear();
            foreach (var d in _dsDinh) d.Mau = 0;
            picGraph.Invalidate();

            Log("=== BẮT ĐẦU GTS2 (TSP - NEAREST NEIGHBOR) ===");

            try
            {
                List<int> hanhTrinhIds = new List<int>();
                int tongChiPhi = 0;

                // --- BƯỚC 1: LẤY ĐIỂM XUẤT PHÁT TỪ COMBOBOX ---
                int startId = 0;
                var cmbStart = this.Controls.Find("cmbStart", true).FirstOrDefault() as ComboBox;
                if (cmbStart != null && cmbStart.SelectedValue != null)
                {
                    startId = (int)cmbStart.SelectedValue;
                }

                Dinh current = _dsDinh.FirstOrDefault(d => d.Id == startId);
                if (current == null) current = _dsDinh[0];

                // Đánh dấu điểm đầu
                current.Mau = 2;
                hanhTrinhIds.Add(current.Id);

                Log($"-> Xuất phát tại: {current.Ten}");
                picGraph.Invalidate();
                await NghiGiaiLao();

                // --- BƯỚC 2: ĐI QUA TẤT CẢ CÁC ĐIỂM CÒN LẠI ---
                while (hanhTrinhIds.Count < _dsDinh.Count)
                {
                    int minW = int.MaxValue;
                    Dinh nextNode = null;

                    // Tìm hàng xóm CHƯA ĐI có chi phí thấp nhất
                    foreach (var c in _dsCanh)
                    {
                        if (c.TuDinh == current.Id)
                        {
                            bool daDi = hanhTrinhIds.Contains(c.DenDinh);
                            if (!daDi && c.TrongSo < minW)
                            {
                                minW = c.TrongSo;
                                nextNode = _dsDinh.Find(d => d.Id == c.DenDinh);
                            }
                        }
                    }

                    if (nextNode != null)
                    {
                        tongChiPhi += minW;
                        Log($"   Từ {current.Ten} -> {nextNode.Ten} (chi phí: {minW})");

                        current = nextNode;
                        current.Mau = 2;
                        hanhTrinhIds.Add(current.Id);

                        picGraph.Invalidate();
                        await NghiGiaiLao();
                    }
                    else
                    {
                        Log("[LỖI] Bế tắc! Không tìm thấy đường đi tiếp.");
                        MessageBox.Show("Không tìm thấy đường đi tiếp (Đồ thị không liên thông).");
                        return;
                    }
                }

                // --- BƯỚC 3: QUAY VỀ ĐIỂM XUẤT PHÁT ---
                var nodeStart = _dsDinh.First(x => x.Id == startId);
                Log($"-> Đã đi hết, quay về {nodeStart.Ten}...");

                var canhVe = _dsCanh.FirstOrDefault(c => c.TuDinh == current.Id && c.DenDinh == startId);

                if (canhVe != null)
                {
                    tongChiPhi += canhVe.TrongSo;
                    Log($"   Từ {current.Ten} -> {nodeStart.Ten} (quay về: {canhVe.TrongSo})");
                    hanhTrinhIds.Add(startId);

                    picGraph.Invalidate();
                    Log("------------------------------");
                    Log($"TỔNG CHI PHÍ: {tongChiPhi}");

                    // In hành trình ra Log cho đẹp
                    string pathStr = string.Join(" -> ", hanhTrinhIds.Select(id => _dsDinh.First(d => d.Id == id).Ten));
                    Log($"Hành trình: {pathStr}");

                    MessageBox.Show($"Hoàn tất! Tổng chi phí: {tongChiPhi}");
                }
                else
                {
                    Log($"[LỖI] Không có đường quay về {nodeStart.Ten}!");
                    MessageBox.Show("Đi hết đường nhưng không quay về được điểm xuất phát!");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
            finally
            {
                btnChay.Enabled = true;
                btnChonFile.Enabled = true;
            }
        }

        // --- CÁC HÀM PHỤ TRỢ ---
        private void Log(string s)
        {
            if (lstLog != null)
            {
                lstLog.Items.Add(s);
                lstLog.TopIndex = lstLog.Items.Count - 1;
            }
        }

        private async Task NghiGiaiLao()
        {
            int delay = 500;
            if (trackBarTocDo != null)
            {
                delay = (trackBarTocDo.Maximum + trackBarTocDo.Minimum) - trackBarTocDo.Value;
                if (delay < 10) delay = 10;
            }
            await Task.Delay(delay);
        }
    }
}