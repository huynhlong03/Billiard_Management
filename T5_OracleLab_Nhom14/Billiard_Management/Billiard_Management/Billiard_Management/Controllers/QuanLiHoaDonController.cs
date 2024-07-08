using Billiard_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models;
using Billiard_Management.Models.ViewModel;
using System.Data;
using Microsoft.AspNetCore.Authorization;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;
using NuGet.Protocol.Plugins;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Billiard_Management.Models.ExcuteQuery;
using Billiard_Management.Models.ConnectionUser;

namespace Billiard_Management.Controllers
{
    [Authorize]
    public class QuanLiHoaDonController : Controller
    {
        private readonly OracleConnectionManager _connectionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public QuanLiHoaDonController(OracleConnectionManager connectionManager, IHttpContextAccessor httpContextAccessor)
        {
            _connectionManager = connectionManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<IActionResult> Index()
        {
            var dsHoaDon = new List<Hoadon>();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = @"
                    SELECT 
                        h.Mahoadon, h.Maban, h.Khachhang, h.Giobatdau, h.Gioketthuc, 
                        h.Thoigianchoi, h.Thanhtoan, h.Tongtien, h.Trangthaithanhtoan, 
                        h.Taikhoan, h.Ghichu, k.Ten AS KhachhangNavigation 
                    FROM 
                        Ql_Billiard.Hoadon h 
                    LEFT JOIN 
                        Ql_Billiard.Khachhang k ON h.Khachhang = k.Makh ORDER BY h.Giobatdau DESC";

                        using (var command = new OracleCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var hoadon = new Hoadon
                                    {
                                        Mahoadon = reader.GetDecimal(0),
                                        Maban = reader.GetString(1),
                                        Khachhang = reader.GetDecimal(2),
                                        Giobatdau = reader.GetDateTime(3),
                                        Gioketthuc = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                        Thoigianchoi = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                                        Thanhtoan = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                                        Tongtien = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                        Trangthaithanhtoan = reader.GetString(8),
                                        Taikhoan = reader.GetString(9),
                                        Ghichu = reader.IsDBNull(10) ? null : reader.GetString(10),
                                        KhachhangNavigation = new Khachhang{Makh = reader.GetDecimal(2), Ten = reader.GetString(11) } // Đọc giá trị KhachhangNavigation
                                    };
                                    dsHoaDon.Add(hoadon);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
            }
            return View(dsHoaDon);
        }
        public IActionResult ThemHoaDon(string maban)
        {
            var model = new HoaDonVM
            {
                Maban = maban
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ThemHoaDon(HoaDonVM model)
        {
            var taikhoan = User.Identity.Name;

            if (ModelState.IsValid)
            {
                try
                {
                    if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                    {
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            string insertQuery = "INSERT INTO Ql_Billiard.Hoadon (Maban, Khachhang, Giobatdau, Trangthaithanhtoan, Taikhoan, Ghichu) " +
                                                 "VALUES (:Maban, :Khachhang, :Giobatdau, :Trangthaithanhtoan, :Taikhoan, :Ghichu)";
                            using (var command = new OracleCommand(insertQuery, connection))
                            {
                                command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = model.Maban;
                                command.Parameters.Add("Khachhang", OracleDbType.Decimal).Value = model.Khachhang;
                                command.Parameters.Add("Giobatdau", OracleDbType.Date).Value = DateTime.Now;
                                command.Parameters.Add("Trangthaithanhtoan", OracleDbType.Varchar2).Value = "Chưa Thanh Toán";
                                command.Parameters.Add("Taikhoan", OracleDbType.Varchar2).Value = taikhoan;
                                command.Parameters.Add("Ghichu", OracleDbType.Varchar2).Value = model.Ghichu;

                                await command.ExecuteNonQueryAsync();
                            }

                            string updateBanQuery = "UPDATE Ql_Billiard.Ban SET Trangthai = 2 WHERE Maban = :Maban";
                            using (var updateCommand = new OracleCommand(updateBanQuery, connection))
                            {
                                updateCommand.Parameters.Add("Maban", OracleDbType.Varchar2).Value = model.Maban;
                                await updateCommand.ExecuteNonQueryAsync();
                            }

                            ViewBag.Notice = "Đã thêm hóa đơn thành công!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = $"Lỗi: {ex.Message}";
                    return View(model);
                }
            }

            return View(model);
        }
        public async Task<IActionResult> SuaHoaDon(decimal id)
        {
            var hoadon = new Hoadon();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = @"
                    SELECT 
                        Mahoadon, Maban, Khachhang, Giobatdau, Gioketthuc, 
                        Thoigianchoi, Thanhtoan, Tongtien, Trangthaithanhtoan, 
                        Taikhoan, Ghichu 
                    FROM 
                        Ql_Billiard.Hoadon 
                    WHERE 
                        Mahoadon = :id";

                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add(new OracleParameter("id", id));

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    hoadon = new Hoadon
                                    {
                                        Mahoadon = reader.GetDecimal(0),
                                        Maban = reader.GetString(1),
                                        Khachhang = reader.GetDecimal(2),
                                        Giobatdau = reader.GetDateTime(3),
                                        Gioketthuc = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                        Thoigianchoi = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                                        Thanhtoan = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                                        Tongtien = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                        Trangthaithanhtoan = reader.GetString(8),
                                        Taikhoan = reader.GetString(9),
                                        Ghichu = reader.IsDBNull(10) ? null : reader.GetString(10)
                                    };
                                }
                                else
                                {
                                    // Nếu không tìm thấy hóa đơn, trả về một thông báo lỗi
                                    ViewBag.Notice = "Không tìm thấy hóa đơn!";
                                    return View(); // Hoặc trả về một view khác
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
            }

            var model = new HoaDonVM
            {
                Maban = hoadon.Maban,
                Khachhang = hoadon.Khachhang,
                Ghichu = hoadon.Ghichu
            };

            return View(model);
        }


        [HttpPost]
        public async Task<IActionResult> SuaHoaDon(HoaDonVM model, decimal id)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                    {
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            string updateQuery = "UPDATE Ql_Billiard.Hoadon SET Maban = :Maban, Khachhang = :Khachhang, Ghichu = :Ghichu WHERE Mahoadon = :Mahoadon";
                            using (var command = new OracleCommand(updateQuery, connection))
                            {
                                command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = model.Maban;
                                command.Parameters.Add("Khachhang", OracleDbType.Decimal).Value = model.Khachhang;
                                command.Parameters.Add("Ghichu", OracleDbType.Varchar2).Value = model.Ghichu;
                                command.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = id;

                                await command.ExecuteNonQueryAsync();
                            }

                            ViewBag.Notice = "Đã sửa hóa đơn!";
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = $"Lỗi: {ex.Message}";
                    return View(model);
                }
            }

            return View(model);
        }

        public IActionResult XemHoaDon(decimal id)
        {
            var hoaDon = new Hoadon();
            var chiTietHoaDon = new List<Chitiethoadon>();
            var thucDon = new List<Thucdon>();
            var khachHang = new Khachhang();
            var ban = new Ban();

            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        connection.Open();

                        string hoaDonQuery = @"
                    SELECT h.Mahoadon, h.Maban, h.Khachhang, h.Giobatdau, h.Gioketthuc, 
                           h.Thoigianchoi, h.Thanhtoan, h.Tongtien, h.Trangthaithanhtoan, 
                           h.Taikhoan, h.Ghichu, k.Ten AS KhachhangTen, b.Tenban AS BanTen
                    FROM Ql_Billiard.Hoadon h
                    LEFT JOIN Ql_Billiard.Khachhang k ON h.Khachhang = k.Makh
                    LEFT JOIN Ql_Billiard.Ban b ON h.Maban = b.Maban
                    WHERE h.Mahoadon = :id";
                        using (var hoaDonCommand = new OracleCommand(hoaDonQuery, connection))
                        {
                            hoaDonCommand.Parameters.Add(new OracleParameter("id", id));

                            using (var reader = hoaDonCommand.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    hoaDon = new Hoadon
                                    {
                                        Mahoadon = reader.GetDecimal(0),
                                        Maban = reader.GetString(1),
                                        Khachhang = reader.GetDecimal(2),
                                        Giobatdau = reader.GetDateTime(3),
                                        Gioketthuc = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                        Thoigianchoi = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                                        Thanhtoan = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                                        Tongtien = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                        Trangthaithanhtoan = reader.GetString(8),
                                        Taikhoan = reader.GetString(9),
                                        Ghichu = reader.IsDBNull(10) ? null : reader.GetString(10)
                                        // Lấy các cột khác tương ứng
                                    };
                                }
                            }
                        }

                        string chiTietHoaDonQuery = @"
                    SELECT ct.Machitiethoadon, ct.Mahoadon, ct.Mathucdon, ct.Soluongdat, m.TenThucDon AS ThucDonTen, m.Gia
                    FROM Ql_Billiard.Chitiethoadon ct
                    LEFT JOIN Ql_Billiard.Thucdon m ON ct.Mathucdon = m.Mathucdon
                    WHERE ct.Mahoadon = :id";
                        using (var chiTietHoaDonCommand = new OracleCommand(chiTietHoaDonQuery, connection))
                        {
                            chiTietHoaDonCommand.Parameters.Add(new OracleParameter("id", id));

                            using (var reader = chiTietHoaDonCommand.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    var chitiethoadon = new Chitiethoadon
                                    {
                                        Machitiethoadon = reader.GetDecimal(0),
                                        Mahoadon = reader.GetDecimal(1),
                                        Mathucdon = reader.GetString(2),
                                        Soluongdat = reader.GetDecimal(3),
                                        MathucdonNavigation = new Thucdon
                                        {
                                            Tenthucdon = reader.GetString(4),
                                            Gia = reader.GetDecimal(5)
                                        }
                                    };
                                    chiTietHoaDon.Add(chitiethoadon);
                                }
                            }
                        }

                        // Lấy thông tin thực đơn, khách hàng và bàn tương ứng nếu cần
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
            }

            var viewModel = new ChiTietHoaDonVM
            {
                HoaDon = hoaDon,
                Chitiethoadon = chiTietHoaDon,
                newChiTietHoaDon = new Chitiethoadon { Mahoadon = id },
                Ban = ban,
                ThucDon = thucDon,
                KhachHang = khachHang
            };

            return View(viewModel);
        }
        private async Task<Hoadon> GetHoaDonWithDetails(decimal id)
        {
            Hoadon hoaDon = null;
            if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string query = "SELECT h.*, b.*, k.*, l.*, ct.*, td.* " +
                                   "FROM Hoadon h " +
                                   "LEFT JOIN Ban b ON h.Maban = b.Maban " +
                                   "LEFT JOIN Khachhang k ON h.Khachhang = k.Makh " +
                                   "LEFT JOIN Loaithanhvien l ON k.Loaithanhvien = l.Maloaithanhvien " +
                                   "LEFT JOIN Chitiethoadon ct ON h.Mahoadon = ct.Mahoadon " +
                                   "LEFT JOIN Thucdon td ON ct.Mathucdon = td.Mathucdon " +
                                   "WHERE h.Mahoadon = :Mahoadon";

                    using (var command = new OracleCommand(query, connection))
                    {
                        command.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = id;
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                hoaDon = new Hoadon
                                {
                                    Mahoadon = reader.GetDecimal(0),
                                    Maban = reader.GetString(1),
                                    Khachhang = reader.GetDecimal(2),
                                    Giobatdau = reader.GetDateTime(3),
                                    Gioketthuc = reader.IsDBNull(4) ? (DateTime?)null : reader.GetDateTime(4),
                                    Thoigianchoi = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                                    Thanhtoan = reader.IsDBNull(6) ? (decimal?)null : reader.GetDecimal(6),
                                    Tongtien = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                                    Trangthaithanhtoan = reader.GetString(8),
                                    Ghichu = reader.IsDBNull(9) ? null : reader.GetString(9),
                                    KhachhangNavigation = new Khachhang
                                    {
                                        Makh = reader.GetDecimal(10),
                                        Ten = reader.GetString(11),
                                        LoaithanhvienNavigation = new Loaithanhvien
                                        {
                                            Maloaitv = reader.GetDecimal(12),
                                            Tenloaitv = reader.GetString(13),
                                            Giamgia = reader.GetDecimal(14)
                                        }
                                    },
                                    MabanNavigation = new Ban
                                    {
                                        Maban = reader.GetString(15),
                                        Tenban = reader.GetString(16),
                                        Gia = reader.GetDecimal(17)
                                    }
                                };

                                var chiTietHoaDonList = new List<Chitiethoadon>();
                                do
                                {
                                    var chiTietHoaDon = new Chitiethoadon
                                    {
                                        Machitiethoadon = reader.GetDecimal(18),
                                        Mahoadon = reader.GetDecimal(19),
                                        Mathucdon = reader.GetString(20),
                                        Soluongdat = reader.GetDecimal(21),
                                        MathucdonNavigation = new Thucdon
                                        {
                                            Mathucdon = reader.GetString(20),
                                            Tenthucdon = reader.GetString(22),
                                            Gia = reader.GetDecimal(23)
                                        }
                                    };
                                    chiTietHoaDonList.Add(chiTietHoaDon);
                                } while (await reader.ReadAsync());

                                hoaDon.Chitiethoadons = chiTietHoaDonList;
                            }
                        }
                    }
                }
            }
            return hoaDon;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThemChiTietHoaDon(decimal Mahoadon, string Mathucdon, decimal Soluongdat)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                    {
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            string insertQuery = "INSERT INTO Ql_Billiard.Chitiethoadon (Mahoadon, Mathucdon, Soluongdat) " +
                                                 "VALUES (:Mahoadon, :Mathucdon, :Soluongdat)";
                            using (var command = new OracleCommand(insertQuery, connection))
                            {
                                command.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = Mahoadon;
                                command.Parameters.Add("Mathucdon", OracleDbType.Varchar2).Value = Mathucdon;
                                command.Parameters.Add("Soluongdat", OracleDbType.Decimal).Value = Soluongdat;

                                await command.ExecuteNonQueryAsync();
                            }

                            ViewBag.Notice = "Thêm món thành công!";
                            return RedirectToAction("XemHoaDon", new { id = Mahoadon });
                        }
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = $"Lỗi: {ex.Message}";
                }
            }

            var hoaDon = await GetHoaDonWithDetails(Mahoadon);
            if (hoaDon == null)
            {
                return NotFound();
            }

            var viewModel = new ChiTietHoaDonVM
            {
                HoaDon = hoaDon,
                Chitiethoadon = hoaDon.Chitiethoadons,
                newChiTietHoaDon = new Chitiethoadon { Mahoadon = Mahoadon }
            };

            return View("XemHoaDon", viewModel);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ThanhToan(decimal Mahoadon)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT h.Mahoadon, h.Maban, h.Khachhang, h.Giobatdau, h.Gioketthuc, h.Trangthaithanhtoan, h.Thanhtoan, h.Tongtien, " +
                                       "k.Makh, k.Ten, k.Loaithanhvien, l.Tenloaitv, l.Giamgia, b.Gia " +
                                       "FROM Ql_Billiard.Hoadon h " +
                                       "JOIN Ql_Billiard.Ban b ON h.Maban = b.Maban " +
                                       "LEFT JOIN Ql_Billiard.Khachhang k ON h.Khachhang = k.Makh " +
                                       "LEFT JOIN Ql_Billiard.Loaithanhvien l ON k.Loaithanhvien = l.Maloaitv " +
                                       "WHERE h.Mahoadon = :Mahoadon";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = Mahoadon;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    var hoaDon = new Hoadon
                                    {
                                        Mahoadon = reader.GetDecimal(reader.GetOrdinal("Mahoadon")),
                                        Maban = reader.GetString(reader.GetOrdinal("Maban")),
                                        Khachhang = reader.GetDecimal(reader.GetOrdinal("Khachhang")),
                                        Giobatdau = reader.GetDateTime(reader.GetOrdinal("Giobatdau")),
                                        Gioketthuc = DateTime.Now,
                                        Trangthaithanhtoan = reader.GetString(reader.GetOrdinal("Trangthaithanhtoan")),
                                        Thanhtoan = reader.IsDBNull(reader.GetOrdinal("Thanhtoan")) ? null : reader.GetDecimal(reader.GetOrdinal("Thanhtoan")),
                                        Tongtien = reader.IsDBNull(reader.GetOrdinal("Tongtien")) ? null : reader.GetDecimal(reader.GetOrdinal("Tongtien")),
                                        KhachhangNavigation = new Khachhang
                                        {
                                            Makh = reader.GetDecimal(reader.GetOrdinal("Makh")),
                                            Ten = reader.GetString(reader.GetOrdinal("Ten")),
                                            LoaithanhvienNavigation = new Loaithanhvien
                                            {
                                                Maloaitv = reader.GetDecimal(reader.GetOrdinal("Loaithanhvien")),
                                                Tenloaitv = reader.GetString(reader.GetOrdinal("Tenloaitv")),
                                                Giamgia = reader.IsDBNull(reader.GetOrdinal("Giamgia")) ? 0 : reader.GetDecimal(reader.GetOrdinal("Giamgia"))
                                            }
                                        },
                                        MabanNavigation = new Ban
                                        {
                                            Maban = reader.GetString(reader.GetOrdinal("Maban")),
                                            Gia = reader.GetDecimal(reader.GetOrdinal("Gia"))
                                        }
                                    };

                                    hoaDon.Thoigianchoi = (decimal)(hoaDon.Gioketthuc.Value - hoaDon.Giobatdau.Value).TotalHours;
                                    var giaBan = hoaDon.MabanNavigation.Gia;
                                    var tongTienThucDon = await GetTongTienThucDon(Mahoadon, connection);

                                    hoaDon.Thanhtoan = (decimal)(hoaDon.Thoigianchoi * giaBan + tongTienThucDon);
                                    hoaDon.Trangthaithanhtoan = "Đã Thanh Toán";
                                    var giamGia = hoaDon.KhachhangNavigation?.LoaithanhvienNavigation?.Giamgia ?? 0;
                                    hoaDon.Tongtien = (decimal)(hoaDon.Thanhtoan - (hoaDon.Thanhtoan * giamGia));
                                    hoaDon.MabanNavigation.Trangthai = 1;

                                    string updateQuery = "UPDATE Ql_Billiard.Hoadon SET Gioketthuc = :Gioketthuc, Thoigianchoi = :Thoigianchoi, Thanhtoan = :Thanhtoan, Tongtien = :Tongtien, Trangthaithanhtoan = :Trangthaithanhtoan WHERE Mahoadon = :Mahoadon";
                                    using (var updateCommand = new OracleCommand(updateQuery, connection))
                                    {
                                        updateCommand.Parameters.Add("Gioketthuc", OracleDbType.Date).Value = hoaDon.Gioketthuc;
                                        updateCommand.Parameters.Add("Thoigianchoi", OracleDbType.Decimal).Value = hoaDon.Thoigianchoi;
                                        updateCommand.Parameters.Add("Thanhtoan", OracleDbType.Decimal).Value = hoaDon.Thanhtoan;
                                        updateCommand.Parameters.Add("Tongtien", OracleDbType.Decimal).Value = hoaDon.Tongtien;
                                        updateCommand.Parameters.Add("Trangthaithanhtoan", OracleDbType.Varchar2).Value = hoaDon.Trangthaithanhtoan;
                                        updateCommand.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = Mahoadon;

                                        await updateCommand.ExecuteNonQueryAsync();
                                    }

                                    string updateBanQuery = "UPDATE Ql_Billiard.Ban SET Trangthai = 1 WHERE Maban = :Maban";
                                    using (var updateBanCommand = new OracleCommand(updateBanQuery, connection))
                                    {
                                        updateBanCommand.Parameters.Add("Maban", OracleDbType.Varchar2).Value = hoaDon.Maban;
                                        await updateBanCommand.ExecuteNonQueryAsync();
                                    }

                                    return RedirectToAction(nameof(XemHoaDon), new { id = Mahoadon });
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Xử lý lỗi tại đây
                ViewBag.Notice = $"Lỗi: {ex.Message}";
            }

            return RedirectToAction(nameof(XemHoaDon), new { id = Mahoadon });
        }

        public async Task<IActionResult> XoaChiTietHoaDon(decimal machitiethoadon)
        {
            if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();

                    string deleteQuery = "DELETE FROM Ql_Billiard.Chitiethoadon WHERE Machitiethoadon = :Machitiethoadon";
                    using (var command = new OracleCommand(deleteQuery, connection))
                    {
                        command.Parameters.Add("Machitiethoadon", OracleDbType.Decimal).Value = machitiethoadon;

                        await command.ExecuteNonQueryAsync();
                    }

                    ViewBag.Notice = "Xóa món thành công!";
                    // Chuyển hướng người dùng trở lại trang xem hóa đơn
                    return RedirectToAction("XemHoaDon", new { id = machitiethoadon });
                }
            }

            return RedirectToAction("XemHoaDon", new { id = machitiethoadon });
        }

        private async Task<decimal> GetTongTienThucDon(decimal mahoadon, OracleConnection connection)
        {
            decimal tongTienThucDon = 0;

            string query = "SELECT SUM(ct.Soluongdat * td.Gia) AS TongTienThucDon " +
                           "FROM Ql_Billiard.Chitiethoadon ct " +
                           "JOIN Ql_Billiard.Thucdon td ON ct.Mathucdon = td.Mathucdon " +
                           "WHERE ct.Mahoadon = :Mahoadon " +
                           "GROUP BY ct.Mahoadon";

            using (var command = new OracleCommand(query, connection))
            {
                command.Parameters.Add("Mahoadon", OracleDbType.Decimal).Value = mahoadon;
                using (var reader = await command.ExecuteReaderAsync())
                {
                    if (await reader.ReadAsync())
                    {
                        if (!reader.IsDBNull(reader.GetOrdinal("TongTienThucDon")))
                        {
                            tongTienThucDon = reader.GetDecimal(reader.GetOrdinal("TongTienThucDon"));
                        }
                    }
                }
            }

            return tongTienThucDon;
        }

        [HttpGet]
        public async Task<IActionResult> TimKiemHoaDon(decimal? searchGiaTu, decimal? searchGiaDen, string searchTrangthai, DateTime? searchNgayBatDau, DateTime? searchNgayKetThuc)
        {
            try
            {
                // Lấy connection string từ cookie
                string connectionString = GetConnectionStringFromCookies();

                // Khởi tạo đối tượng ExecuteQueryFromDB
                using (var executeQueryFromDB = new ExecuteQueryFromDB(connectionString))
                {
                    // Gọi phương thức TimKiemHoaDonAsync từ ExecuteQueryFromDB
                    var hoadons = await executeQueryFromDB.TimKiemHoaDonAsync(searchGiaTu, searchGiaDen, searchTrangthai, searchNgayBatDau, searchNgayKetThuc);

                    // Trả về view với danh sách hóa đơn tìm được
                    return View(hoadons);
                }
            }
            catch (Exception ex)
            {
                // Xử lý exception tại đây nếu cần
                ViewBag.Notice = ex.Message;
                return RedirectToAction("Index", "QuanLiHoaDon");
            }
        }

        private string GetConnectionStringFromCookies()
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                return connectionString;
            }
            throw new Exception("Connection string not found in cookies.");
        }
    }
}