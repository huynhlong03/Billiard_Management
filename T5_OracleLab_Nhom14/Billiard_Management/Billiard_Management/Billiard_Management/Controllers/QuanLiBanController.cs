using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using Billiard_Management.Models;
using Billiard_Management.Models.ViewModel;

namespace Billiard_Management.Controllers
{
    [Authorize]
    public class QuanLiBanController : Controller
    {
        private readonly IWebHostEnvironment _environment;

        public QuanLiBanController(IWebHostEnvironment environment)
        {
            _environment = environment;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();

                    var dsBan = new List<Ban>();

                    string query = "SELECT b.Maban, b.Tenban, b.Hinhanh, b.Loaiban, b.Trangthai, b.Gia, lb.TenLoai, tt.TenTrangThai FROM Ql_Billiard.Ban b JOIN Ql_Billiard.LoaiBan lb ON b.Loaiban = lb.Maloai JOIN Ql_Billiard.TrangThai tt ON b.Trangthai = tt.MaTrangThai ORDER BY b.Maban";

                    using (var command = new OracleCommand(query, connection))
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            dsBan.Add(new Ban
                            {
                                Maban = reader.GetString(0),
                                Tenban = reader.GetString(1),
                                Hinhanh = reader.GetString(2),
                                Loaiban = reader.GetString(3),
                                Trangthai = reader.GetDecimal(4),
                                Gia = reader.GetDecimal(5),
                                LoaibanNavigation = new LoaiBan { TenLoai = reader.GetString(6) },
                                TrangthaiNavigation = new Trangthai { Tentrangthai = reader.GetString(7) }
                            });
                        }
                    }

                    return View(dsBan);
                }
            }

            return Unauthorized();
        }
        [Authorize]
        [HttpGet]
        public IActionResult ThemBan()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ThemBan(Ban model, IFormFile fileUpLoad)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string checkQuery = "SELECT COUNT(*) FROM Ql_Billiard.Ban WHERE Maban = :Maban";
                        using (var checkCommand = new OracleCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.Add("Maban", OracleDbType.Varchar2).Value = model.Maban;
                            int existingBanCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                            if (existingBanCount > 0)
                            {
                                ViewBag.Notice = "Mã bàn đã tồn tại!";
                                return View(model);
                            }
                        }

                        var imgDirectoryName = "img"; // Tên thư mục lưu trữ ảnh
                        var imgDirectoryPath = Path.Combine(_environment.WebRootPath, "assets", imgDirectoryName);

                        var fileName = Path.GetFileName(fileUpLoad.FileName);
                        var filePath = Path.Combine(imgDirectoryPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            fileUpLoad.CopyTo(stream);
                        }

                        string insertQuery = "INSERT INTO Ql_Billiard.Ban (Maban, Tenban, Hinhanh, Loaiban, Trangthai, Gia) VALUES (:Maban, :Tenban, :Hinhanh, :Loaiban, :Trangthai, :Gia)";
                        using (var command = new OracleCommand(insertQuery, connection))
                        {
                            command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = model.Maban;
                            command.Parameters.Add("Tenban", OracleDbType.Varchar2).Value = model.Tenban;
                            command.Parameters.Add("Hinhanh", OracleDbType.Varchar2).Value = fileName;
                            command.Parameters.Add("Loaiban", OracleDbType.Varchar2).Value = model.Loaiban;
                            command.Parameters.Add("Trangthai", OracleDbType.Decimal).Value = model.Trangthai;
                            command.Parameters.Add("Gia", OracleDbType.Decimal).Value = model.Gia;

                            await command.ExecuteNonQueryAsync();
                        }

                        string commitQuery = "COMMIT";
                        using (var commitCommand = new OracleCommand(commitQuery, connection))
                        {
                            await commitCommand.ExecuteNonQueryAsync();
                        }

                        ViewBag.Notice = "Bàn bida đã được thêm thành công!";
                        
                    }
                }
                
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
                
            }
            return View(model);


        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SuaBan(string id)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT Maban, Tenban, Hinhanh, Loaiban, Trangthai, Gia FROM Ql_Billiard.Ban WHERE Maban = :Maban";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = id;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    var ban = new Ban
                                    {
                                        Maban = reader.GetString(0),
                                        Tenban = reader.GetString(1),
                                        Hinhanh = reader.GetString(2),
                                        Loaiban = reader.GetString(3),
                                        Trangthai = reader.GetDecimal(4),
                                        Gia = reader.GetDecimal(5)
                                    };

                                    return View(ban);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            ViewBag.Notice = "Không tìm thấy bàn bida!";
            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SuaBan(Ban model, string id, IFormFile fileUpLoad)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                    {
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();

                            var imgDirectoryName = "img"; // Tên thư mục lưu trữ ảnh
                            var imgDirectoryPath = Path.Combine(_environment.WebRootPath, "assets", imgDirectoryName);

                            var fileName = Path.GetFileName(fileUpLoad.FileName);
                            var filePath = Path.Combine(imgDirectoryPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                fileUpLoad.CopyTo(stream);
                            }

                            string updateQuery = "UPDATE Ql_Billiard.Ban SET Tenban = :Tenban, Hinhanh = :Hinhanh, Loaiban = :Loaiban, Trangthai = :Trangthai, Gia = :Gia WHERE Maban = :Maban";
                            using (var command = new OracleCommand(updateQuery, connection))
                            {
                              
                                command.Parameters.Add(new OracleParameter("Tenban", model.Tenban));
                                command.Parameters.Add(new OracleParameter("Hinhanh", fileName));
                                command.Parameters.Add(new OracleParameter("Loaiban", model.Loaiban));
                                command.Parameters.Add(new OracleParameter("Trangthai", model.Trangthai));
                                command.Parameters.Add(new OracleParameter("Gia", model.Gia));
                                command.Parameters.Add(new OracleParameter("Maban", id));

                                await command.ExecuteNonQueryAsync();
                            }

                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }

                           
                            // Hoặc chuyển hướng đến một action/view khác
                        }
                    }

                    ViewBag.Notice = "Cập nhật thông tin bàn bida thành công!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            return View(model);
        }

        //public async Task<IActionResult> XoaBan(string id)
        //{
        //    if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
        //    {
        //        using (var connection = new OracleConnection(connectionString))
        //        {
        //            await connection.OpenAsync();

        //            string query = "SELECT COUNT(*) FROM Ql_Billiard.Ban WHERE Maban = :Maban";
        //            using (var command = new OracleCommand(query, connection))
        //            {
        //                command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = id;
        //                int existingBanCount = Convert.ToInt32(await command.ExecuteScalarAsync());

        //                if (existingBanCount == 0)
        //                {
        //                    ViewBag.Notice = "Không tìm thấy bàn bida!";
        //                    return RedirectToAction("Index"); // Hoặc trả về một view khác
        //                }
        //            }

        //            try
        //            {
        //                string deleteQuery = "DELETE FROM Ql_Billiard.Ban WHERE Maban = :Maban";
        //                using (var command = new OracleCommand(deleteQuery, connection))
        //                {
        //                    command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = id;
        //                    await command.ExecuteNonQueryAsync();
        //                }

        //                string commitQuery = "COMMIT";
        //                using (var commitCommand = new OracleCommand(commitQuery, connection))
        //                {
        //                    await commitCommand.ExecuteNonQueryAsync();
        //                }

        //                ViewBag.Notice = "Xóa bàn bida thành công!";
        //            }
        //            catch (Exception ex)
        //            {
        //                ViewBag.Notice = $"Lỗi: {ex.Message}";
        //            }

        //            return RedirectToAction("Index"); // Hoặc chuyển hướng đến một action/view khác
        //        }
        //    }

        //    return Unauthorized();
        //}
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> TimKiemBan(string searchString)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        var bans = new List<Ban>();
                        string query;

                        if (string.IsNullOrEmpty(searchString))
                        {
                            query = "SELECT b.Maban, b.Tenban, b.Hinhanh, b.Loaiban, b.Trangthai, b.Gia, lb.TenLoai, tt.TenTrangThai FROM Ql_Billiard.Ban b JOIN Ql_Billiard.LoaiBan lb ON b.Loaiban = lb.Maloai JOIN Ql_Billiard.TrangThai tt ON b.Trangthai = tt.MaTrangThai ORDER BY b.Maban";
                        }
                        else
                        {
                            query = "SELECT b.Maban, b.Tenban, b.Hinhanh, b.Loaiban, b.Trangthai, b.Gia, lb.TenLoai, tt.TenTrangThai FROM Ql_Billiard.Ban b JOIN Ql_Billiard.LoaiBan lb ON b.Loaiban = lb.Maloai JOIN Ql_Billiard.TrangThai tt ON b.Trangthai = tt.MaTrangThai WHERE LOWER(b.Tenban) LIKE '%' || :SearchString || '%' OR LOWER(lb.TenLoai) LIKE '%' || :SearchString || '%'";
                        }

                        using (var command = new OracleCommand(query, connection))
                        {
                            if (!string.IsNullOrEmpty(searchString))
                            {
                                command.Parameters.Add("SearchString", OracleDbType.Varchar2).Value = searchString.ToLower();
                            }

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    bans.Add(new Ban
                                    {
                                        Maban = reader.GetString(0),
                                        Tenban = reader.GetString(1),
                                        Hinhanh = reader.GetString(2),
                                        Loaiban = reader.GetString(3),
                                        Trangthai = reader.GetDecimal(4),
                                        Gia = reader.GetDecimal(5),
                                        LoaibanNavigation = new LoaiBan { TenLoai = reader.GetString(6) },
                                        TrangthaiNavigation = new Trangthai { Tentrangthai = reader.GetString(7) }
                                    });
                                }
                            }
                        }

                        return View(bans);
                    }
                }
            }
             catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            return Unauthorized();
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> XemChiTiet(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            ChiTietBanVM viewModel = new ChiTietBanVM();

            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Lấy thông tin bàn bida
                        string banQuery = "SELECT Maban, Tenban, Hinhanh, Loaiban, Trangthai, Gia FROM Ql_Billiard.Ban WHERE Maban = :Maban";
                        using (var command = new OracleCommand(banQuery, connection))
                        {
                            command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = id;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    viewModel.Ban = new Ban
                                    {
                                        Maban = reader.GetString(0),
                                        Tenban = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        Hinhanh = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Loaiban = reader.IsDBNull(3) ? null : reader.GetString(3),
                                        Trangthai = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                                        Gia = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5)
                                    };
                                }
                                else
                                {
                                    return NotFound();
                                }
                            }
                        }

                        // Lấy thông tin hóa đơn chưa thanh toán
                        string hoadonQuery = @" SELECT 
                        h.Mahoadon, h.Maban, h.Khachhang, h.Giobatdau, h.Gioketthuc, 
                        h.Thoigianchoi, h.Thanhtoan, h.Tongtien, h.Trangthaithanhtoan, 
                        h.Taikhoan, h.Ghichu, k.Ten AS KhachhangNavigation 
                    FROM 
                        Ql_Billiard.Hoadon h 
                    LEFT JOIN 
                        Ql_Billiard.Khachhang k ON h.Khachhang = k.Makh
                                       WHERE h.Maban = :Maban AND h.Trangthaithanhtoan = 'Chưa Thanh Toán'";
                        using (var command = new OracleCommand(hoadonQuery, connection))
                        {
                            command.Parameters.Add("Maban", OracleDbType.Varchar2).Value = id;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    viewModel.HoadonChuaThanhToan = new Hoadon
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
                                        KhachhangNavigation = new Khachhang { Makh = reader.GetDecimal(2), Ten = reader.GetString(11) } 
                                    };
                                }
                            }
                        }

                        // Lấy chi tiết hóa đơn nếu có hóa đơn chưa thanh toán
                        if (viewModel.HoadonChuaThanhToan != null)
                        {
                            viewModel.HoadonChuaThanhToan.Chitiethoadons = new List<Chitiethoadon>();

                            string chitiethoadonQuery = @"SELECT Mahoadon, Mathucdon, Soluong
                                                  FROM Ql_Billiard.Chitiethoadon
                                                  WHERE Mahoadon = :Mahoadon";
                            using (var command = new OracleCommand(chitiethoadonQuery, connection))
                            {
                                command.Parameters.Add("Mahoadon", OracleDbType.Varchar2).Value = viewModel.HoadonChuaThanhToan.Mahoadon;
                                using (var reader = await command.ExecuteReaderAsync())
                                {
                                    while (await reader.ReadAsync())
                                    {
                                        var chitiethoadon = new Chitiethoadon
                                        {
                                            Mahoadon = reader.GetDecimal(0),
                                            Mathucdon = reader.IsDBNull(1) ? null : reader.GetString(1),
                                            Soluongdat = reader.GetInt32(2),
                                           
                                        };

                                        viewModel.HoadonChuaThanhToan.Chitiethoadons.Add(chitiethoadon);
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
                return View(viewModel);
            }

            return View(viewModel);
        }

    }
}
