using Billiard_Management.Models;
using Billiard_Management.Models.ConnectionUser;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using System.Data;
using System.Security.Principal;

namespace Billiard_Management.Controllers
{
    public class QuanLiTaiKhoanController : Controller
    {
        private readonly OracleConnectionManager _connectionManager;
        public QuanLiTaiKhoanController(OracleConnectionManager connectionManager)
        {
            _connectionManager = connectionManager;
        }
        [Authorize]
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy connection string từ cookies
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    // Mở kết nối đến cơ sở dữ liệu
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Truy vấn danh sách tài khoản
                        string query = "SELECT * FROM Ql_billiard.Account";
                        using (var command = new OracleCommand(query, connection))
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            var accounts = new List<Account>();
                            while (reader.Read())
                            {
                                var account = new Account
                                {
                                    TaiKhoan = reader.IsDBNull("TaiKhoan") ? null : reader.GetString(reader.GetOrdinal("TaiKhoan")),
                                    MatKhau = reader.IsDBNull("MatKhau") ? null : reader.GetString(reader.GetOrdinal("MatKhau")),
                                    HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                                    SDT = reader.GetString(reader.GetOrdinal("SDT")),
                                    TinhTrang = reader.GetString(reader.GetOrdinal("TinhTrang")),
                                    QuanLy = reader.GetInt32(reader.GetOrdinal("QuanLy"))
                                };
                                accounts.Add(account);
                            }

                            return View(accounts);
                        }
                    }
                }
                else
                {
                    ModelState.AddModelError("", "Không thể lấy thông tin kết nối.");
                    return View(new List<Account>());
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = "Đã xảy ra lỗi: " + ex.Message;
                return View(new List<Account>());
            }
        }

        [Authorize]
        [HttpGet]
        public IActionResult ThemTaiKhoan()
        {
            return View();
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ThemTaiKhoan(Account model)
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
                            // Set Oracle script mode
                            string alterSessionQuery = "ALTER SESSION SET \"_oracle_script\"=true";
                            using (var alterCommand = new OracleCommand(alterSessionQuery, connection))
                            {
                                await alterCommand.ExecuteNonQueryAsync();
                            }
                            string insertQuery = "INSERT INTO Ql_Billiard.Account (TaiKhoan, MatKhau, HoTen, SDT, TinhTrang, QuanLy) VALUES (:TaiKhoan, :MatKhau, :HoTen, :SDT, :TinhTrang, :QuanLy)";
                            using (var command = new OracleCommand(insertQuery, connection))
                            {
                                command.Parameters.Add("TaiKhoan", OracleDbType.Varchar2).Value = model.TaiKhoan;
                                command.Parameters.Add("MatKhau", OracleDbType.Varchar2).Value = model.MatKhau;
                                command.Parameters.Add("HoTen", OracleDbType.NVarchar2).Value = model.HoTen;
                                command.Parameters.Add("SDT", OracleDbType.Char).Value = model.SDT;
                                command.Parameters.Add("TinhTrang", OracleDbType.NVarchar2).Value = model.TinhTrang;
                                command.Parameters.Add("QuanLy", OracleDbType.Int32).Value = model.QuanLy;

                                await command.ExecuteNonQueryAsync();
                            }
                            // Commit transaction
                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }
                        }

                        ViewBag.Notice = "Thêm thành công.";
                    }
                    else
                    {
                        ViewBag.Notice = "Không thể lấy thông tin kết nối.";
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
        public async Task<IActionResult> SuaTaiKhoan(string id)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        string query = "SELECT TaiKhoan, MatKhau, HoTen, SDT, TinhTrang, QuanLy FROM Ql_Billiard.Account WHERE TaiKhoan = :TaiKhoan";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add("TaiKhoan", OracleDbType.Varchar2).Value = id;
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    var account = new Account
                                    {
                                        TaiKhoan = reader.GetString(reader.GetOrdinal("TaiKhoan")),
                                        MatKhau = reader.GetString(reader.GetOrdinal("MatKhau")),
                                        HoTen = reader.GetString(reader.GetOrdinal("HoTen")),
                                        SDT = reader.GetString(reader.GetOrdinal("SDT")),
                                        TinhTrang = reader.GetString(reader.GetOrdinal("TinhTrang")),
                                        QuanLy = reader.GetInt32(reader.GetOrdinal("QuanLy"))
                                    };

                                    return View(account);
                                }
                            }
                        }
                    }

                    ViewBag.Notice = "Không thể lấy thông tin tài khoản.";
                }
                else
                {
                    ViewBag.Notice = "Không thể lấy thông tin kết nối.";
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
        public async Task<IActionResult> SuaTaiKhoan(Account model)
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

                            string updateQuery = "UPDATE Ql_Billiard.Account SET MatKhau = :MatKhau, HoTen = :HoTen, SDT = :SDT, TinhTrang = :TinhTrang, QuanLy = :QuanLy WHERE TaiKhoan = :TaiKhoan";
                            using (var command = new OracleCommand(updateQuery, connection))
                            {
                                command.Parameters.Add("MatKhau", OracleDbType.Varchar2).Value = model.MatKhau;
                                command.Parameters.Add("HoTen", OracleDbType.NVarchar2).Value = model.HoTen;
                                command.Parameters.Add("SDT", OracleDbType.Char).Value = model.SDT;
                                command.Parameters.Add("TinhTrang", OracleDbType.NVarchar2).Value = model.TinhTrang;
                                command.Parameters.Add("QuanLy", OracleDbType.Int32).Value = model.QuanLy;
                                command.Parameters.Add("TaiKhoan", OracleDbType.Varchar2).Value = model.TaiKhoan;

                                await command.ExecuteNonQueryAsync();
                            }
                        }

                        ViewBag.Notice = "Sửa thành công.";
                    }
                    else
                    {
                        ViewBag.Notice = "Không thể lấy thông tin kết nối.";
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
        
        public async Task<IActionResult> XoaTaiKhoan(string id)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();

                        // Kiểm tra điều kiện trước khi xóa
                        string checkQuery = "SELECT QuanLy, TinhTrang FROM Ql_Billiard.Account WHERE TaiKhoan = :TaiKhoan";
                        using (var checkCommand = new OracleCommand(checkQuery, connection))
                        {
                            checkCommand.Parameters.Add("TaiKhoan", OracleDbType.Varchar2).Value = id;
                            using (var reader = await checkCommand.ExecuteReaderAsync())
                            {
                                if (reader.Read())
                                {
                                    int isQuanLy = reader.GetInt32(reader.GetOrdinal("QuanLy"));
                                    string tinhTrang = reader.GetString(reader.GetOrdinal("TinhTrang"));

                                    if (isQuanLy == 1 && tinhTrang == "Đang làm việc")
                                    {
                                        ViewBag.Notice = "Không thể xóa tài khoản có quản lý và đang làm việc.";
                                        return RedirectToAction("Index");
                                    }
                                }
                            }
                        }

                        // Xóa tài khoản
                        string deleteQuery = "DELETE FROM Ql_Billiard.Account WHERE TaiKhoan = :TaiKhoan";
                        using (var command = new OracleCommand(deleteQuery, connection))
                        {
                            command.Parameters.Add("TaiKhoan", OracleDbType.Varchar2).Value = id;
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    ViewBag.Notice = "Xóa thành công!";
                }
                else
                {
                    ViewBag.Notice = "Không thể lấy thông tin kết nối.";
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Đã xảy ra lỗi: " + ex.Message;
            }

            return RedirectToAction("Index");
        }

    }
}
