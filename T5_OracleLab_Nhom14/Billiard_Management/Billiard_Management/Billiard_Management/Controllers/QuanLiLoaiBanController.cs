using Billiard_Management.Models;
using Billiard_Management.Models.ViewModel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;

namespace Billiard_Management.Controllers
{
    public class QuanLiLoaiBanController : Controller
    {
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT * FROM Ql_Billiard.LoaiBan";
                        using (var command = new OracleCommand(query, connection))
                        {
                            var loaiBanList = new List<LoaiBan>();
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (reader.Read())
                                {
                                    loaiBanList.Add(new LoaiBan
                                    {
                                        MaLoai = reader.GetString(reader.GetOrdinal("MaLoai")),
                                        TenLoai = reader.GetString(reader.GetOrdinal("TenLoai"))
                                    });
                                }
                            }

                            return View(loaiBanList);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            return View(new List<LoaiBan>());
        }
        [Authorize]
        [HttpGet]
        public IActionResult ThemLoaiBan()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ThemLoaiBan(LoaiBan model)
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

                            string insertQuery = @"INSERT INTO Ql_Billiard.LoaiBan (MaLoai, TenLoai) VALUES (:MaLoai, :TenLoai)";
                            using (var command = new OracleCommand(insertQuery, connection))
                            {
                                command.Parameters.Add("MaLoai", OracleDbType.Char).Value = model.MaLoai;
                                command.Parameters.Add("TenLoai", OracleDbType.NVarchar2).Value = model.TenLoai;

                                await command.ExecuteNonQueryAsync();
                            }

                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }
                        }

                        ViewBag.Notice = "Thêm thành công";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
                }
            }

            return View(model);
        }
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> SuaLoaiBan(string maLoai)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT MaLoai, TenLoai FROM Ql_Billiard.LoaiBan WHERE MaLoai = :MaLoai";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add("MaLoai", OracleDbType.Char).Value = maLoai;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    var loaiBan = new LoaiBan
                                    {
                                        MaLoai = reader.GetString(reader.GetOrdinal("MaLoai")),
                                        TenLoai = reader.GetString(reader.GetOrdinal("TenLoai"))
                                    };

                                    return View(loaiBan);
                                }
                            }
                        }
                    }

                    ModelState.AddModelError("", "Không thể lấy thông tin loại bàn.");
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> SuaLoaiBan(LoaiBan model)
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

                            string updateQuery = @"UPDATE Ql_Billiard.LoaiBan SET TenLoai = :TenLoai WHERE MaLoai = :MaLoai";
                            using (var command = new OracleCommand(updateQuery, connection))
                            {
                                command.Parameters.Add("TenLoai", OracleDbType.NVarchar2).Value = model.TenLoai;
                                command.Parameters.Add("MaLoai", OracleDbType.Char).Value = model.MaLoai;

                                await command.ExecuteNonQueryAsync();
                            }

                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }
                        }

                        ViewBag.Notice = "Sửa thành công!";
                    }
                    else
                    {
                        ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                    }
                }
                catch (Exception ex)
                {
                    ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
                }
            }

            return View(model);
        }
        [Authorize]
       
        public async Task<IActionResult> XoaLoaiBan(string maLoai)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Kiểm tra xem MaLoai có được tham chiếu ở bảng Ban hay không
                        string checkQuery = "SELECT COUNT(*) FROM Ql_Billiard.Ban WHERE LoaiBan = :MaLoai";
                        using (var checkCommand = new OracleCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.Add("MaLoai", OracleDbType.Char).Value = maLoai;
                            int referenceCount = Convert.ToInt32(await checkCommand.ExecuteScalarAsync());

                            if (referenceCount > 0)
                            {
                                ModelState.AddModelError("", "Không thể xóa loại bàn này vì có bàn tham chiếu tới nó.");
                                return RedirectToAction("Index");
                            }
                        }

                        // Nếu không có ràng buộc, thực hiện xóa
                        string deleteQuery = "DELETE FROM Ql_Billiard.LoaiBan WHERE MaLoai = :MaLoai";
                        using (var command = new OracleCommand(deleteQuery, connection))
                        {
                            command.Parameters.Add("MaLoai", OracleDbType.Char).Value = maLoai;
                            await command.ExecuteNonQueryAsync();
                        }

                        string commitQuery = "COMMIT";
                        using (var commitCommand = new OracleCommand(commitQuery, connection))
                        {
                            await commitCommand.ExecuteNonQueryAsync();
                        }
                    }

                    ViewBag.Notice = "Xóa thành công!";
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
