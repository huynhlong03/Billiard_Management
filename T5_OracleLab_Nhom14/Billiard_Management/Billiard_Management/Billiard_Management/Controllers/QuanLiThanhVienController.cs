using Microsoft.AspNetCore.Mvc;
using Billiard_Management.Models;

using Billiard_Management.Models.ViewModel;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using Billiard_Management.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Billiard_Management.Models.ExcuteQuery;
using Billiard_Management.Models.ConnectionUser;

namespace QuanLi_Billiard.Controllers
{
    [Authorize]
    public class QuanLiThanhVienController : Controller
    {
        private readonly OracleConnectionManager _connectionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public QuanLiThanhVienController(OracleConnectionManager connectionManager, IHttpContextAccessor httpContextAccessor)
        {
            _connectionManager = connectionManager;
            _httpContextAccessor = httpContextAccessor;
        }
        private string GetConnectionStringFromCookies()
        {
            if (_httpContextAccessor.HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                return connectionString;
            }
            throw new Exception("Connection string not found in cookies.");
        }
        public async Task<IActionResult> Index()
        {
            var dsThanhVien = new List<Khachhang>();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = @"SELECT kh.Makh, kh.Ten, kh.Phone, kh.Loaithanhvien, ltv.Tenloaitv FROM Ql_Billiard.Khachhang kh JOIN Ql_Billiard.Loaithanhvien ltv ON kh.Loaithanhvien = ltv.Maloaitv";

                        using (var command = new OracleCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var tv = new Khachhang
                                    {
                                        Makh = reader.GetDecimal(0),
                                        Ten = reader.GetString(1),
                                        Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Loaithanhvien =  reader.GetInt32(3),
                                        LoaithanhvienNavigation = new Loaithanhvien
                                        {
                                            Maloaitv = reader.GetInt32(3),
                                            Tenloaitv = reader.GetString(4)
                                        }
                                    };
                                    dsThanhVien.Add(tv);
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

            return View(dsThanhVien);
        }

        public IActionResult ThemThanhVien()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ThemThanhVien(string ten, string phone, int loaiThanhVien)
        {
            try
            {
                string connectionString = GetConnectionStringFromCookies();
                using (var _executeQueryFromDB = new ExecuteQueryFromDB(connectionString))
                {
                    await _executeQueryFromDB.ThemThanhVienAsync(ten, phone, loaiThanhVien);
                    ViewBag.Notice = "Thêm thành viên thành công.";
                    return View();
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi khi thêm thành viên." + ex.Message;
               
                return View();
            }

           
        }

        //[HttpPost]
        //public async Task<IActionResult> ThemThanhVien(string ten, string phone, int loaiThanhVien)
        //{
        //    try
        //    {
        //        if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
        //        {
        //            using (var connection = new OracleConnection(connectionString))
        //            {
        //                await connection.OpenAsync();
        //                var query = "INSERT INTO Ql_Billiard.Khachhang (Ten, Phone, Loaithanhvien) VALUES (:Ten, :Phone, :LoaiThanhVien)";
        //                using (var command = new OracleCommand(query, connection))
        //                {
        //                    command.Parameters.Add(new OracleParameter("Ten", ten));
        //                    command.Parameters.Add(new OracleParameter("Phone", phone));
        //                    command.Parameters.Add(new OracleParameter("LoaiThanhVien", loaiThanhVien));
        //                    await command.ExecuteNonQueryAsync();
        //                }
        //                string commitQuery = "COMMIT";
        //                using (var commitCommand = new OracleCommand(commitQuery, connection))
        //                {
        //                    await commitCommand.ExecuteNonQueryAsync();
        //                }
        //                ViewBag.Notice = "Thêm thành viên thành công!";
        //            }

        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        ViewBag.Notice = $"Đã xảy ra lỗi: {ex.Message}";
        //    }

        //    return View();
        //}

        public async Task<IActionResult> SuaThanhVien(decimal id)
        {
            var tv = new Khachhang();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = "SELECT Makh, Ten, Phone, Loaithanhvien FROM Ql_Billiard.Khachhang WHERE Makh = :Makh";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add(new OracleParameter("Makh", id));
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {
                                    tv.Makh = reader.GetDecimal(0);
                                    tv.Ten = reader.GetString(1);
                                    tv.Phone = reader.IsDBNull(2) ? null : reader.GetString(2);
                                    tv.Loaithanhvien = reader.GetInt32(3);
                                }
                                else
                                {
                                    ViewBag.Notice = "Không tìm thấy thành viên!";
                                    return View();
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
            var model = new Khachhang
            {
                Makh = tv.Makh,
                Ten = tv.Ten,
                Phone = tv.Phone,
                Loaithanhvien = tv.Loaithanhvien
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SuaThanhVien(Khachhang model, decimal id)
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
                            var query = "UPDATE Ql_Billiard.Khachhang SET Ten = :Ten, Phone = :Phone, Loaithanhvien = :LoaiThanhVien WHERE Makh = :Makh";
                            using (var command = new OracleCommand(query, connection))
                            {
                                command.Parameters.Add(new OracleParameter("Ten", model.Ten));
                                command.Parameters.Add(new OracleParameter("Phone", model.Phone));
                                command.Parameters.Add(new OracleParameter("LoaiThanhVien", model.Loaithanhvien));
                                command.Parameters.Add(new OracleParameter("Makh", id));
                                await command.ExecuteNonQueryAsync();
                            }
                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }
                        }
                    }
                    ViewBag.Notice = "Đã sửa thành viên!";
                   
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = $"Lỗi: {ex.Message}";
                    return View(model);
                }
            }
            return View(model);
        }
        public async Task<IActionResult> XoaThanhVien(decimal id)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = "DELETE FROM Ql_Billiard.Khachhang WHERE Makh = :Makh";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add(new OracleParameter("Makh", id));
                            await command.ExecuteNonQueryAsync();
                        }
                        string commitQuery = "COMMIT";
                        using (var commitCommand = new OracleCommand(commitQuery, connection))
                        {
                            await commitCommand.ExecuteNonQueryAsync();
                        }
                    }
                }

                ViewBag.Notice = "Đã xóa thành viên!";
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
            }
            return RedirectToAction("Index");
        }
        [HttpGet]
        public async Task<IActionResult> TimKiemThanhVien(string searchString)
        {
            var dsThanhVien = new List<Khachhang>();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                       
                        string query;

                        if (string.IsNullOrEmpty(searchString))
                        {
                            query = "SELECT kh.Makh, kh.Ten, kh.Phone, kh.Loaithanhvien, ltv.Tenloaitv FROM Ql_Billiard.Khachhang kh JOIN Ql_Billiard.Loaithanhvien ltv ON kh.Loaithanhvien = ltv.Maloaitv ORDER BY kh.Makh";
                        }
                        else
                        {
                            query = "SELECT kh.Makh, kh.Ten, kh.Phone, kh.Loaithanhvien, ltv.Tenloaitv FROM Ql_Billiard.Khachhang kh JOIN Ql_Billiard.Loaithanhvien ltv ON kh.Loaithanhvien = ltv.Maloaitv WHERE LOWER(kh.Phone) LIKE '%' || :SearchString || '%' OR LOWER(kh.Ten) LIKE '%' || :SearchString || '%' ORDER BY kh.Makh";
                        }
                       
                        using (var command = new OracleCommand(query, connection))
                        {
                            if (!string.IsNullOrEmpty(searchString))
                            {
                                command.Parameters.Add(new OracleParameter("SearchString", "%" + searchString.ToLower() + "%"));
                            }
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    var tv = new Khachhang
                                    {
                                        Makh = reader.GetDecimal(0),
                                        Ten = reader.GetString(1),
                                        Phone = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Loaithanhvien = reader.IsDBNull(3) ? null : reader.GetInt32(3),
                                        LoaithanhvienNavigation = new Loaithanhvien
                                        {
                                            Maloaitv = reader.GetInt32(3),
                                            Tenloaitv = reader.GetString(4)
                                        }
                                    };
                                    dsThanhVien.Add(tv);
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
            return View(dsThanhVien);
        }
    }
}
