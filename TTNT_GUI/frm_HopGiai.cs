using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using TTNT_BUS;
using TTNT_DAL.Models;

namespace TTNT_GUI
{
    public partial class frm_HopGiai : Form
    {
        private GiaiThuatHopGiai _bus = new GiaiThuatHopGiai();

        public frm_HopGiai()
        {
            InitializeComponent();
            // Không còn SetupUI() nữa
        }

        // --- CÁC HÀM SỰ KIỆN (Giữ nguyên) ---
        private void btnMau_Click(object sender, EventArgs e)
        {
            txtInput.Text = "A v B\r\n-A v C\r\n-B v D\r\n-C\r\n-D";
        }

        private void btnXoa_Click(object sender, EventArgs e)
        {
            txtInput.Clear();
            dgvKetQua.Rows.Clear();
        }

        private void btnGiai_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtInput.Text)) return;

            dgvKetQua.Rows.Clear();
            List<string> lines = txtInput.Lines.ToList();

            try
            {
                // Gọi BUS để tính toán
                var ketQua = _bus.ThucHienHopGiai(lines);

                // Đổ dữ liệu ra bảng
                foreach (var step in ketQua.CacBuoc)
                {
                    int idx = dgvKetQua.Rows.Add();
                    var row = dgvKetQua.Rows[idx];

                    row.Cells[0].Value = step.STT;
                    row.Cells[1].Value = step.HanhDong;
                    row.Cells[2].Value = step.KetQua;

                    // Nếu tìm thấy rỗng [] thì tô màu đỏ
                    if (step.IsRong)
                    {
                        row.DefaultCellStyle.BackColor = Color.MistyRose;
                        row.DefaultCellStyle.ForeColor = Color.Red;
                        row.DefaultCellStyle.Font = new Font(dgvKetQua.Font, FontStyle.Bold);
                    }
                }

                if (ketQua.ThanhCong)
                    MessageBox.Show("THÀNH CÔNG! Đã chứng minh được (Ra mệnh đề rỗng).", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show("THẤT BẠI! Không tìm thấy mâu thuẫn.", "Kết quả", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Lỗi: " + ex.Message);
            }
        }
    }
}
