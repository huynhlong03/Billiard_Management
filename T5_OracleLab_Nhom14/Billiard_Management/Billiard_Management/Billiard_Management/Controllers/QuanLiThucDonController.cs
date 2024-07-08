using Billiard_Management.Models;
using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Humanizer;

namespace Billiard_Management.Controllers
{
    [Authorize]
    public class QuanLiThucDonController : Controller
    {
      
        private readonly IWebHostEnvironment _environment;

        public QuanLiThucDonController(IConfiguration configuration, IWebHostEnvironment environment)
        {
         
            _environment = environment;
        }

        public async Task<IActionResult> Index()
        {
            var dsThucDon = new List<Thucdon>();

            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = "SELECT Mathucdon, Tenthucdon, Donvitinh, Gia, Hinh, Ghichu FROM Ql_Billiard.Thucdon ORDER BY Mathucdon";
                        using (var command = new OracleCommand(query, connection))
                        {
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    dsThucDon.Add(new Thucdon
                                    {
                                        Mathucdon = reader.GetString(0),
                                        Tenthucdon = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        Donvitinh = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Gia = reader.GetDecimal(3),
                                        Hinh = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        Ghichu = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    });
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

            return View(dsThucDon);
        }

        public IActionResult ThemThucDon()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ThemThucDon(Thucdon model, IFormFile fileUpLoad)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var checkQuery = "SELECT COUNT(*) FROM Ql_Billiard.Thucdon WHERE Mathucdon = :Mathucdon";
                        using (var command = new OracleCommand(checkQuery, connection))
                        {
                            command.Parameters.Add(new OracleParameter("Mathucdon", model.Mathucdon));
                            var exists = (decimal)(await command.ExecuteScalarAsync()) > 0;

                            if (exists)
                            {
                                ViewBag.Notice = "Món đã tồn tại!";
                                return View(model);
                            }
                        }

                        var imgDirectoryName = "img";
                        var imgDirectoryPath = Path.Combine(_environment.WebRootPath, "assets", imgDirectoryName);
                        var fileName = Path.GetFileName(fileUpLoad.FileName);
                        var filePath = Path.Combine(imgDirectoryPath, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fileUpLoad.CopyToAsync(stream);
                        }

                        if (ModelState.IsValid)
                        {

                            var insertQuery = "INSERT INTO Ql_Billiard.Thucdon (Mathucdon, Tenthucdon, Donvitinh, Gia, Hinh, Ghichu) VALUES (:Mathucdon, :Tenthucdon, :Donvitinh, :Gia, :Hinh, :Ghichu)";
                            using (var command = new OracleCommand(insertQuery, connection))
                            {
                                command.Parameters.Add(new OracleParameter("Mathucdon", model.Mathucdon));
                                command.Parameters.Add(new OracleParameter("Tenthucdon", model.Tenthucdon));
                                command.Parameters.Add(new OracleParameter("Donvitinh", model.Donvitinh));
                                command.Parameters.Add(new OracleParameter("Gia", model.Gia));
                                command.Parameters.Add(new OracleParameter("Hinh", fileName));
                                command.Parameters.Add(new OracleParameter("Ghichu", model.Ghichu));

                                await command.ExecuteNonQueryAsync();
                            }
                            string commitQuery = "COMMIT";
                            using (var commitCommand = new OracleCommand(commitQuery, connection))
                            {
                                await commitCommand.ExecuteNonQueryAsync();
                            }
                            ViewBag.Notice = "Món mới đã được thêm thành công vào thực đơn!";

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
                return View(model);
            }
            return View(model);
        }


        public async Task<IActionResult> SuaThucDon(string id)
        {
            var tv = new Thucdon();
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = "SELECT Mathucdon, Tenthucdon, Donvitinh, Gia, Hinh, Ghichu FROM Ql_Billiard.Thucdon WHERE Mathucdon = :Mathucdon";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add(new OracleParameter("Mathucdon", id));
                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                if (await reader.ReadAsync())
                                {

                                    tv.Mathucdon = reader.GetString(0);
                                    tv.Tenthucdon = reader.IsDBNull(1) ? null : reader.GetString(1);
                                    tv.Donvitinh = reader.IsDBNull(2) ? null : reader.GetString(2);
                                    tv.Gia = reader.GetDecimal(3);
                                    tv.Hinh = reader.IsDBNull(4) ? null : reader.GetString(4);
                                    tv.Ghichu = reader.IsDBNull(5) ? null : reader.GetString(5);
                                   
                                }
                                else
                                {
                                    ViewBag.Notice = "Không tìm thấy món!";
                                    return View();
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
            var model = new Thucdon
            {
                Mathucdon = tv.Mathucdon,
                Tenthucdon = tv.Tenthucdon,
                Donvitinh = tv.Donvitinh,
                Gia = tv.Gia,
                Hinh = tv.Hinh,
                Ghichu = tv.Ghichu,
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SuaThucDon(Thucdon model, string id, IFormFile fileUpLoad)
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
                            
                            var imgDirectoryName = "img";
                            var imgDirectoryPath = Path.Combine(_environment.WebRootPath, "assets", imgDirectoryName);
                            var fileName = Path.GetFileName(fileUpLoad.FileName);
                            var filePath = Path.Combine(imgDirectoryPath, fileName);

                            using (var stream = new FileStream(filePath, FileMode.Create))
                            {
                                await fileUpLoad.CopyToAsync(stream);
                            }


                            var updateQuery = @"UPDATE Ql_Billiard.Thucdon SET Tenthucdon = :Tenthucdon, Donvitinh = :Donvitinh, Gia = :Gia, Hinh = :Hinh, Ghichu = :Ghichu WHERE Mathucdon = :Mathucdon";

                            using (var command = new OracleCommand(updateQuery, connection))
                            {
                              
                                command.Parameters.Add(new OracleParameter("Tenthucdon", model.Tenthucdon));
                                command.Parameters.Add(new OracleParameter("Donvitinh", model.Donvitinh));
                                command.Parameters.Add(new OracleParameter("Gia", model.Gia));
                                command.Parameters.Add(new OracleParameter("Hinh", fileName));
                                command.Parameters.Add(new OracleParameter("Ghichu", model.Ghichu));
                                command.Parameters.Add(new OracleParameter("Mathucdon", id));

                                var rowsAffected = await command.ExecuteNonQueryAsync();
                                string commitQuery = "COMMIT";
                                using (var commitCommand = new OracleCommand(commitQuery, connection))
                                {
                                    await commitCommand.ExecuteNonQueryAsync();
                                }
                                
                            }
 
                            
                        }
                    }
                    ViewBag.Notice = "Cập nhật thông tin món thành công!";
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
                return View(model);
            }
            return View(model);
        }

        public async Task<IActionResult> XoaThucDon(string id)
        {
            try
            {
                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {

                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = "SELECT COUNT(*) FROM Ql_Billiard.Thucdon WHERE Mathucdon = :Mathucdon";
                        using (var command = new OracleCommand(query, connection))
                        {
                            command.Parameters.Add(new OracleParameter("Mathucdon", id));
                            var exists = (decimal)(await command.ExecuteScalarAsync()) > 0;

                            if (!exists)
                            {
                                ViewBag.Notice = "Không tìm thấy món!";
                                return RedirectToAction("Index");
                            }
                        }

                        try
                        {
                            var deleteQuery = "DELETE FROM Ql_Billiard.Thucdon WHERE Mathucdon = :Mathucdon";
                            using (var command = new OracleCommand(deleteQuery, connection))
                            {
                                command.Parameters.Add(new OracleParameter("Mathucdon", id));
                                await command.ExecuteNonQueryAsync();
                            }

                            ViewBag.Notice = "Xóa món thành công!";
                        }
                        catch (Exception ex)
                        {
                            ViewBag.Notice = $"Lỗi: {ex.Message}";
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.Notice = $"Lỗi: {ex.Message}";
               
            }
            return RedirectToAction("Index");
        }


        public async Task<IActionResult> TimKiemThucDon(string searchString)
        {
            var thucdons = new List<Thucdon>();
            try
            {


                if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
                {
                    using (var connection = new OracleConnection(connectionString))
                    {
                        await connection.OpenAsync();
                        var query = string.IsNullOrEmpty(searchString)
                            ? "SELECT Mathucdon, Tenthucdon, Donvitinh, Gia, Hinh, Ghichu FROM Ql_Billiard.Thucdon"
                            : "SELECT Mathucdon, Tenthucdon, Donvitinh, Gia, Hinh, Ghichu FROM Ql_Billiard.Thucdon WHERE LOWER(Tenthucdon) LIKE '%' || :searchString || '%'";

                        using (var command = new OracleCommand(query, connection))
                        {
                            if (!string.IsNullOrEmpty(searchString))
                            {
                                command.Parameters.Add(new OracleParameter("searchString", searchString.ToLower()));
                            }

                            using (var reader = await command.ExecuteReaderAsync())
                            {
                                while (await reader.ReadAsync())
                                {
                                    thucdons.Add(new Thucdon
                                    {
                                        Mathucdon = reader.GetString(0),
                                        Tenthucdon = reader.IsDBNull(1) ? null : reader.GetString(1),
                                        Donvitinh = reader.IsDBNull(2) ? null : reader.GetString(2),
                                        Gia = reader.GetDecimal(3),
                                        Hinh = reader.IsDBNull(4) ? null : reader.GetString(4),
                                        Ghichu = reader.IsDBNull(5) ? null : reader.GetString(5),
                                    });
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
            return View(thucdons);
        }
    }
}

