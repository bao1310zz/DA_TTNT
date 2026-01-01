using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using TTNT_BUS;         // Gọi Project BUS
using TTNT_DAL.Models;  // Gọi Project DAL

namespace TTNT_GUI
{
    public partial class Form1 : Form
    {
        // 1. KHAI BÁO BIẾN (Dùng mô hình 3 lớp)
        private XuLyDoThi _xuLy = new XuLyDoThi();
        private List<Dinh> _dsDinh = new List<Dinh>();
        private List<Canh> _dsCanh = new List<Canh>();

        private FrmLog _frmLog = new FrmLog();

        // Bảng màu (Lấy từ code cũ của bạn)
        private Color[] _colors = {
            Color.White, Color.Red, Color.Lime, Color.Blue,
            Color.Yellow, Color.Orange, Color.Purple, Color.Cyan,
            Color.Magenta, Color.Brown, Color.Pink, Color.Teal
        };

        public Form1()
        {
            InitializeComponent();

            // CẤU HÌNH SỰ KIỆN (Để đảm bảo nút bấm luôn chạy)


            // Nút xem log (Nếu có)
            if (Controls.Find("btnXemLog", true).Length > 0)
                Controls["btnXemLog"].Click += (s, e) => { _frmLog.Show(); _frmLog.BringToFront(); };

            // Cấu hình GridView cho đẹp (Giống code cũ)
            dgvMaTran.AllowUserToAddRows = false;
            dgvMaTran.ReadOnly = true;
            dgvMaTran.RowHeadersWidth = 50;
        }

        // ==========================================================
        // PHẦN 1: NHẬP FILE (Load Graph)
        // ==========================================================
        private void BtnLoad_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog { Filter = "Text Files (*.txt)|*.txt" };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    // Reset dữ liệu
                    _dsDinh.Clear(); _dsCanh.Clear();
                    dgvMaTran.Rows.Clear(); dgvMaTran.Columns.Clear();

                    string[] lines = File.ReadAllLines(dlg.FileName);
                    int n = int.Parse(lines[0].Trim());

                    // Tạo cột cho DataGridView (Giống code cũ của bạn)
                    for (int i = 0; i < n; i++)
                        dgvMaTran.Columns.Add($"Col{i}", (i + 1).ToString());

                    // Tính tọa độ vẽ
                    int cx = picGraph.Width / 2, cy = picGraph.Height / 2;
                    int r = Math.Min(cx, cy) - 50;

                    for (int i = 0; i < n; i++)
                    {
                        // 1. Tạo Đỉnh (Dinh)
                        double angle = 2 * Math.PI * i / n;
                        _dsDinh.Add(new Dinh
                        {
                            Id = i + 1,
                            Ten = (i + 1).ToString(),
                            X = cx + (int)(r * Math.Cos(angle)),
                            Y = cy + (int)(r * Math.Sin(angle)),
                            Mau = 0
                        });

                        // 2. Xử lý dòng ma trận
                        string[] row = lines[i + 1].Trim().Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
                        dgvMaTran.Rows.Add(row);
                        dgvMaTran.Rows[i].HeaderCell.Value = (i + 1).ToString(); // Đánh số hàng

                        // 3. Tạo Cạnh (Canh)
                        for (int j = i + 1; j < n; j++)
                        {
                            if (row[j] == "1")
                                _dsCanh.Add(new Canh { TuDinh = i + 1, DenDinh = j + 1 });
                        }
                    }
                    picGraph.Invalidate();
                    MessageBox.Show("Đọc file thành công!");
                }
                catch (Exception ex) { MessageBox.Show("Lỗi: " + ex.Message); }
            }
        }

        // ==========================================================
        // PHẦN 2: XỬ LÝ NÚT GIẢI (Logic Lai ghép)
        // ==========================================================
        private async void BtnGiai_Click(object sender, EventArgs e)
        {
            // 1. KIỂM TRA ĐẦU VÀO
            if (_dsDinh.Count == 0)
            {
                MessageBox.Show("Chưa có đồ thị! Vui lòng Load File trước.");
                return;
            }

            // 2. DỌN DẸP TRẠNG THÁI CŨ
            _frmLog.XoaLog();
            _frmLog.Hide(); // Ẩn bảng log đi cho gọn

            foreach (var d in _dsDinh) d.Mau = 0; // Reset màu về trắng
            picGraph.Invalidate(); // Vẽ lại hình trắng

            // 3. XỬ LÝ THEO CHẾ ĐỘ CHỌN
            if (chkStep.Checked)
            {
                // ========================================================
                // TRƯỜNG HỢP A: CHẠY MÔ PHỎNG (VISUAL)
                // ========================================================
                btnGiai.Enabled = false;
                _frmLog.GhiLog(">>> BẮT ĐẦU MÔ PHỎNG (VISUAL BACKTRACKING)...");

                // Gọi hàm đệ quy chạy trực tiếp trên Form
                // Số 4 ở đây là số màu thử nghiệm (m), bạn có thể tăng lên nếu cần
                bool solved = await SolveBacktrackingVisual(4, new int[_dsDinh.Count], 0);

                btnGiai.Enabled = true;

                if (solved)
                {
                    _frmLog.GhiLog(">>> THÀNH CÔNG: Đã tìm ra cách tô màu.");
                    MessageBox.Show("Mô phỏng hoàn tất!");
                }
                else
                {
                    _frmLog.GhiLog(">>> THẤT BẠI: Không đủ màu để tô.");
                    MessageBox.Show("Không tìm được giải pháp với số màu hiện tại (Thử tăng m lên).");
                }
            }
            else
            {
                // ========================================================
                // TRƯỜNG HỢP B: CHẠY HỆ THỐNG (GỌI BUS & DATABASE)
                // ========================================================
                try
                {
                    btnGiai.Enabled = false;
                    Cursor.Current = Cursors.WaitCursor; // Hiện con chuột xoay xoay

                    _frmLog.GhiLog(">>> ĐANG CHẠY THUẬT TOÁN TỐI ƯU (BACKTRACKING)...");

                    // BƯỚC 1: Lưu xuống DB (Chạy luồng phụ để không đơ máy)
                    string tenBai = "ToiUu_" + DateTime.Now.Ticks;
                    await Task.Run(() => _xuLy.LuuDuLieu(tenBai, _dsDinh, _dsCanh));

                    // Lấy ID bài vừa lưu
                    var baiToan = _xuLy.LayHetBaiToan().FirstOrDefault();
                    int idBai = baiToan.Id;

                    // BƯỚC 2: Gọi thuật toán TỐI ƯU (Backtracking)
                    bool ketQua = false;

                    // Vì thuật toán tối ưu chạy lâu, đẩy ra luồng background
                    await Task.Run(() =>
                    {
                        ketQua = _xuLy.ChayToiUu(idBai);
                    });

                    // BƯỚC 3: Xử lý kết quả trả về
                    if (ketQua)
                    {
                        // Load lại dữ liệu đã tô màu từ DB
                        var res = _xuLy.LayMotBai(idBai);
                        _dsDinh = res.DanhSachDinh.ToList();
                        _dsCanh = res.DanhSachCanh.ToList();

                        picGraph.Invalidate(); // Vẽ lại hình lên màn hình

                        // --- GHI LỜI GIẢI CHI TIẾT VÀO LOG (Để xem sau) ---
                        _frmLog.GhiLog(">>> KẾT QUẢ CHI TIẾT:");
                        foreach (var d in _dsDinh)
                        {
                            string tenMau = GetColorName(d.Mau);
                            _frmLog.GhiLog($" - Đỉnh {d.Ten}: {tenMau}");
                        }

                        int maxColor = _dsDinh.Max(d => d.Mau);
                        _frmLog.GhiLog("----------------------------------");
                        _frmLog.GhiLog($"=> TỔNG KẾT: Số màu tối ưu là {maxColor}");

                        MessageBox.Show(
                            $"Đã giải xong theo cách Tối Ưu!\nSố màu ít nhất cần dùng: {maxColor}\n(Bấm 'Xem Lời Giải' để xem chi tiết)",
                            "Thành công",
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Information
                        );
                    }
                    else
                    {
                        _frmLog.GhiLog(">>> THẤT BẠI: Thuật toán không tìm ra lời giải.");
                        MessageBox.Show("Không tìm được phương án tô màu.");
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Lỗi hệ thống: " + ex.Message);
                }
                finally
                {
                    btnGiai.Enabled = true;
                    Cursor.Current = Cursors.Default;
                }
            }
        }
        private void BtnXemLog_Click(object sender, EventArgs e)
        {
            // Hiện Form Log lên
            _frmLog.Show();

            // Đưa nó lên trên cùng (đề phòng bị Form chính che mất)
            _frmLog.BringToFront();
        }

        // ==========================================================
        // PHẦN 3: THUẬT TOÁN VISUAL (Code cũ chuyển sang Dinh/Canh)
        // ==========================================================
        private async Task<bool> SolveBacktrackingVisual(int m, int[] colors, int vIndex)
        {
            if (vIndex == _dsDinh.Count) return true; // Đã xong

            int currentId = _dsDinh[vIndex].Id;
            _frmLog.GhiLog($"--- Xét Đỉnh {currentId} ---");

            for (int c = 1; c <= m; c++)
            {
                // HIỆU ỨNG: Tô thử màu
                _dsDinh[vIndex].Mau = c;
                picGraph.Invalidate(); // Vẽ lại ngay

                _frmLog.GhiLog($"   > Đỉnh {currentId}: Thử màu {GetColorName(c)}...");
                await Task.Delay(400); // <-- CHỜ 0.4s NHƯ CŨ

                if (IsSafeVisual(currentId, c, colors))
                {
                    colors[vIndex] = c;
                    _frmLog.GhiLog("   => Hợp lệ! Đi tiếp...");

                    if (await SolveBacktrackingVisual(m, colors, vIndex + 1)) return true;

                    // BACKTRACK
                    colors[vIndex] = 0;
                    _dsDinh[vIndex].Mau = 0;
                    picGraph.Invalidate();
                    _frmLog.GhiLog($"   <! QUAY LUI: Đỉnh {currentId} hủy màu.");
                    await Task.Delay(300);
                }
            }
            return false;
        }

        // Hàm kiểm tra trùng màu (Viết lại theo Model mới)
        private bool IsSafeVisual(int nodeId, int c, int[] colors)
        {
            for (int i = 0; i < _dsDinh.Count; i++)
            {
                if (colors[i] == c) // Nếu đỉnh i đang dùng màu c
                {
                    // Kiểm tra có cạnh nối không (Dùng danh sách Canh)
                    bool connected = _dsCanh.Any(edge =>
                        (edge.TuDinh == nodeId && edge.DenDinh == _dsDinh[i].Id) ||
                        (edge.DenDinh == nodeId && edge.TuDinh == _dsDinh[i].Id));

                    if (connected) return false;
                }
            }
            return true;
        }

        private string GetColorName(int c)
        {
            string[] names = { "Trắng", "Đỏ", "Lá", "Dương", "Vàng", "Cam", "Tím", "Cyan", "Magenta" };
            return (c < names.Length) ? names[c] : "Màu " + c;
        }

        // ==========================================================
        // PHẦN 4: VẼ HÌNH (Lấy nguyên logic vẽ đẹp từ code cũ)
        // ==========================================================
        private void PicGraph_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
            e.Graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;

            // 1. Vẽ cạnh
            foreach (var edge in _dsCanh)
            {
                var n1 = _dsDinh.FirstOrDefault(d => d.Id == edge.TuDinh);
                var n2 = _dsDinh.FirstOrDefault(d => d.Id == edge.DenDinh);
                if (n1 != null && n2 != null)
                {
                    using (Pen p = new Pen(Color.Gray, 2))
                        e.Graphics.DrawLine(p, n1.X, n1.Y, n2.X, n2.Y);
                }
            }

            // 2. Vẽ đỉnh (Bóng đổ + Viền + Chữ)
            foreach (var d in _dsDinh)
            {
                Color c = (d.Mau > 0 && d.Mau < _colors.Length) ? _colors[d.Mau] : Color.White;

                // Vẽ bóng đổ (Silver)
                e.Graphics.FillEllipse(Brushes.Silver, d.X - 18, d.Y - 18, 40, 40);

                // Vẽ hình tròn màu
                using (Brush b = new SolidBrush(c))
                    e.Graphics.FillEllipse(b, d.X - 20, d.Y - 20, 40, 40);

                // Vẽ viền đen
                using (Pen p = new Pen(Color.Black, 2))
                    e.Graphics.DrawEllipse(p, d.X - 20, d.Y - 20, 40, 40);

                // Vẽ số (Chọn màu chữ tương phản)
                // Màu tối (Xanh, Tím..) thì chữ Trắng, còn lại chữ Đen
                Brush textBrush = (d.Mau == 3 || d.Mau == 6 || d.Mau == 9) ? Brushes.White : Brushes.Black;

                var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
                e.Graphics.DrawString(d.Ten, new Font("Arial", 12, FontStyle.Bold), textBrush, d.X, d.Y + 1, sf);
            }
        }
    }
}