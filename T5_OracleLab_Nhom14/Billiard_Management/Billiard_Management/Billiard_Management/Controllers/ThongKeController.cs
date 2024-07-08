using Microsoft.AspNetCore.Mvc;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models;
using Billiard_Management.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace QuanLi_Billiard.Controllers
{
    public class ThongKeController : Controller
    {
        public async Task<IActionResult> ThongKeBanKhachChoi()
        {
            var data = new List<ThongKeBanKhachChoiVM>();

            if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();
                    string query = "SELECT b.Loaiban, COUNT(h.Mahoadon) AS SoLuotChoi, SUM(h.Tongtien) AS DoanhThu " +
                                   "FROM Ql_Billiard.Hoadon h " +
                                   "JOIN Ql_Billiard.Ban b ON h.Maban = b.Maban " +
                                   "GROUP BY b.Loaiban";

                    using (var command = new OracleCommand(query, connection))
                    {
                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                data.Add(new ThongKeBanKhachChoiVM
                                {
                                    LoaiBan = reader.GetString(0),
                                    SoLuotChoi = reader.GetInt32(1),
                                    DoanhThu = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                                });
                            }
                        }
                    }
                }
            }

            return View(data);
        }

        public async Task<IActionResult> ThongKeDoanhThu()
        {
            var doanhThuTheoNgay = new List<ThongKeItem>();
            var doanhThuTheoThang = new List<ThongKeItem>();
            var doanhThuTheoNam = new List<ThongKeItem>();

            if (HttpContext.Request.Cookies.TryGetValue("ConnectionString", out string connectionString))
            {
                using (var connection = new OracleConnection(connectionString))
                {
                    await connection.OpenAsync();

                    // Thống kê doanh thu theo ngày
                    string queryNgay = "SELECT TRUNC(Gioketthuc) AS Ngay, SUM(Tongtien) AS Total " +
                                       "FROM Ql_Billiard.Hoadon " +
                                       "WHERE Gioketthuc IS NOT NULL " +
                                       "GROUP BY TRUNC(Gioketthuc) " +
                                       "ORDER BY TRUNC(Gioketthuc)";
                    using (var commandNgay = new OracleCommand(queryNgay, connection))
                    {
                        using (var reader = await commandNgay.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                doanhThuTheoNgay.Add(new ThongKeItem
                                {
                                    Label = reader.GetDateTime(0).ToString("dd/MM/yyyy"),
                                    Value = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                                });
                            }
                        }
                    }

                    // Thống kê doanh thu theo tháng
                    string queryThang = "SELECT EXTRACT(YEAR FROM Gioketthuc) AS Nam, EXTRACT(MONTH FROM Gioketthuc) AS Thang, SUM(Tongtien) AS Total " +
                                        "FROM Ql_Billiard.Hoadon " +
                                        "WHERE Gioketthuc IS NOT NULL " +
                                        "GROUP BY EXTRACT(YEAR FROM Gioketthuc), EXTRACT(MONTH FROM Gioketthuc) " +
                                        "ORDER BY EXTRACT(YEAR FROM Gioketthuc), EXTRACT(MONTH FROM Gioketthuc)";
                    using (var commandThang = new OracleCommand(queryThang, connection))
                    {
                        using (var reader = await commandThang.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                doanhThuTheoThang.Add(new ThongKeItem
                                {
                                    Label = $"{reader.GetInt32(1)}/{reader.GetInt32(0)}",
                                    Value = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2)
                                });
                            }
                        }
                    }

                    // Thống kê doanh thu theo năm
                    string queryNam = "SELECT EXTRACT(YEAR FROM Gioketthuc) AS Nam, SUM(Tongtien) AS Total " +
                                      "FROM Ql_Billiard.Hoadon " +
                                      "WHERE Gioketthuc IS NOT NULL " +
                                      "GROUP BY EXTRACT(YEAR FROM Gioketthuc) " +
                                      "ORDER BY EXTRACT(YEAR FROM Gioketthuc)";
                    using (var commandNam = new OracleCommand(queryNam, connection))
                    {
                        using (var reader = await commandNam.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                doanhThuTheoNam.Add(new ThongKeItem
                                {
                                    Label = reader.GetInt32(0).ToString(),
                                    Value = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1)
                                });
                            }
                        }
                    }
                }
            }

            var viewModel = new ThongKeDoanhThuVM
            {
                DoanhThuTheoNgay = doanhThuTheoNgay,
                DoanhThuTheoThang = doanhThuTheoThang,
                DoanhThuTheoNam = doanhThuTheoNam
            };

            return View(viewModel);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
