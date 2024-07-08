using Billiard_Management.Models;
using Billiard_Management.Models.ViewModel;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models.ConnectionUser;
using Billiard_Management.Models.ExcuteQuery;
using Microsoft.AspNetCore.Http;

namespace Billiard_Management.Controllers
{
    public class HomeController : Controller
    {
        private readonly OracleConnectionManager _connectionManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public HomeController(OracleConnectionManager connectionManager, IHttpContextAccessor httpContextAccessor)
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
            string connectionString = GetConnectionStringFromCookies();
            using (var _executeQueryFromDB = new ExecuteQueryFromDB(connectionString))
            {
                ViewBag.ThongKeSoBan = await _executeQueryFromDB.TinhTongBanAsync();
                ViewBag.ThongKeThanhVien = await _executeQueryFromDB.TinhTongThanhVienAsync();
                ViewBag.ThongKeThucDon = await _executeQueryFromDB.TinhTongThucDonAsync();
                ViewBag.ThongKeHoaDon = await _executeQueryFromDB.TinhTongSoHoaDonAsync();
                ViewBag.ThongKeDoanhThu = await _executeQueryFromDB.TinhTongTienHoaDonAsync();
                ViewBag.ThongKeLoaiBan = await _executeQueryFromDB.TinhTongLoaiBanAsync();
                ViewBag.ThongKeTaiKhoan = await _executeQueryFromDB.TinhTongTaiKhoanAsync();
                ViewBag.ThongKeBanChoi = await _executeQueryFromDB.ThongKeBanKhachChoi();
            }

            return View();
        }
        public IActionResult DangNhap()
        {
            return View();
        }
        [HttpPost]

        public async Task<IActionResult> DangNhap(LoginVM model)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo connection string từ thông tin đăng nhập
                    string connectionString = $"User Id={model.TaiKhoan};Password={model.MatKhau};Data Source=(DESCRIPTION=(ADDRESS=(PROTOCOL=TCP)(HOST=localhost)(PORT=1521))(CONNECT_DATA=(SERVICE_NAME=orcl)))";

                    try
                    {
                        // Mở kết nối để kiểm tra thông tin đăng nhập
                        using (var connection = new OracleConnection(connectionString))
                        {
                            await connection.OpenAsync();
                            // Kiểm tra thông tin đăng nhập
                            string query = "SELECT COUNT(*) FROM Ql_billiard.Account WHERE TaiKhoan = :taiKhoan AND MatKhau = :matKhau";
                            using (var command = new OracleCommand(query, connection))
                            {
                                command.Parameters.Add("taiKhoan", OracleDbType.Varchar2).Value = model.TaiKhoan;
                                command.Parameters.Add("matKhau", OracleDbType.Varchar2).Value = model.MatKhau;

                                int count = Convert.ToInt32(await command.ExecuteScalarAsync());
                                if (count == 1)
                                {
                                    // Tạo claim cho người dùng
                                    var claims = new[] { new Claim(ClaimTypes.Name, model.TaiKhoan) };
                                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                                    // Tạo token và đăng nhập người dùng
                                    var principal = new ClaimsPrincipal(identity);
                                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

                                    // Lưu connection string vào cookies
                                    HttpContext.Response.Cookies.Append("ConnectionString", connectionString, new CookieOptions { HttpOnly = true, Secure = true });

                                    return RedirectToAction("Index", "QuanLiTaiKhoan");
                                }
                            }
                        }

                        ViewBag.Notice = "Thông tin đăng nhập không chính xác.";
                    }
                    catch (OracleException)
                    {
                        ViewBag.Notice = "Thông tin đăng nhập không chính xác.";
                    }
                }

                return View(model);
            }
            catch (Exception ex)
            {
                ViewBag.Notice = ex.Message;
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DangXuat()
        {
            await _connectionManager.CloseConnectionAsync();
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
