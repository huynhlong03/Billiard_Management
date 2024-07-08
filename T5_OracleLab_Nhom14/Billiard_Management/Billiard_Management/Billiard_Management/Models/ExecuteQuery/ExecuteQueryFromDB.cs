using Billiard_Management.Models;
using Microsoft.AspNetCore.Http;
using Oracle.ManagedDataAccess.Client;
using Billiard_Management.Models.ViewModel;
using System;
using System.Collections.Generic;
using System.Data;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Types;
using Microsoft.AspNetCore.Mvc;

namespace Billiard_Management.Models.ExcuteQuery
{
    public class ExecuteQueryFromDB : IDisposable
    {
        private readonly OracleConnection _connection;

        public ExecuteQueryFromDB(string connectionString)
        {
            _connection = new OracleConnection(connectionString);
            _connection.Open();
        }

        public async Task<int> TinhTongBanAsync()
        {
            int totalBan = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT Ql_billiard.TinhTongBan() FROM dual";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    totalBan = reader.GetInt32(0);
                }
            }

            return totalBan;
        }

        public async Task<int> TinhTongThanhVienAsync()
        {
            int totalThanhVien = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT Ql_billiard.TinhTongThanhVien() FROM dual";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    totalThanhVien = reader.GetInt32(0);
                }
            }

            return totalThanhVien;
        }
        public async Task<int> TinhTongSoHoaDonAsync()
        {
            int total= 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Ql_billiard.HoaDon";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }
            }

            return total;
        }
        public async Task<int> TinhTongLoaiBanAsync()
        {
            int total = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Ql_billiard.LoaiBan";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }
            }

            return total;
        }
        public async Task<int> TinhTongTaiKhoanAsync()
        {
            int total = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Ql_billiard.Account";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }
            }

            return total;
        }
        public async Task<decimal> TinhTongTienHoaDonAsync()
        {
            decimal totalTongTien = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "Ql_Billiard.TinhTongTienHoaDon";
                command.CommandType = CommandType.StoredProcedure;

                OracleParameter returnValue = new OracleParameter();
                returnValue.ParameterName = "ReturnValue";
                returnValue.OracleDbType = OracleDbType.Decimal;
                returnValue.Direction = ParameterDirection.ReturnValue;
                command.Parameters.Add(returnValue);

                await command.ExecuteNonQueryAsync();

                OracleDecimal oracleDecimalValue = (OracleDecimal)returnValue.Value;
                if (!oracleDecimalValue.IsNull)
                {
                    totalTongTien = oracleDecimalValue.Value;
                }
            }

            return totalTongTien;
        }


        public async Task ThemThanhVienAsync(string ten, string phone, int loaiThanhVien)
        {
            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "Ql_Billiard.ThemThanhVien";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("Ten", OracleDbType.NVarchar2).Value = ten;
                command.Parameters.Add("Phone", OracleDbType.Char).Value = phone;
                command.Parameters.Add("LoaiThanhVien", OracleDbType.Int32).Value = loaiThanhVien;

                await command.ExecuteNonQueryAsync();
            }
        }

        public async Task<int> TinhTongThucDonAsync()
        {
            int total = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT COUNT(*) FROM Ql_billiard.ThucDon";
                OracleDataReader reader = (OracleDataReader)await command.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    total = reader.GetInt32(0);
                }
            }

            return total;
        }
        public async Task<int> ThongKeBanKhachChoi()
        {
            int soLuotChoi = 0;

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandType = CommandType.StoredProcedure;
                command.CommandText = "Ql_Billiard.ThongKeBanKhachChoi";

                // Tạo tham số đầu ra cho số lượt chơi
                OracleParameter soLuotChoiParam = new OracleParameter();
                soLuotChoiParam.ParameterName = "SoLuotChoi";
                soLuotChoiParam.OracleDbType = OracleDbType.Int32;
                soLuotChoiParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(soLuotChoiParam);

                // Tạo tham số đầu ra cho con trỏ
                OracleParameter cursorParam = new OracleParameter();
                cursorParam.ParameterName = "cur";
                cursorParam.OracleDbType = OracleDbType.RefCursor;
                cursorParam.Direction = ParameterDirection.Output;
                command.Parameters.Add(cursorParam);

                await command.ExecuteNonQueryAsync();

                // Lấy giá trị số lượt chơi từ tham số đầu ra
                soLuotChoi = ((OracleDecimal)soLuotChoiParam.Value).ToInt32();
            }

            return soLuotChoi;
        }

        public async Task<List<Hoadon>> TimKiemHoaDonAsync(decimal? searchGiaTu, decimal? searchGiaDen, string searchTrangthai, DateTime? searchNgayBatDau, DateTime? searchNgayKetThuc)
        {
            List<Hoadon> result = new List<Hoadon>();

            using (OracleCommand command = _connection.CreateCommand())
            {
                command.CommandText = "Ql_Billiard.TimKiemHoaDon";
                command.CommandType = CommandType.StoredProcedure;

                command.Parameters.Add("searchGiaTu", OracleDbType.Decimal).Value = searchGiaTu ?? (object)DBNull.Value;
                command.Parameters.Add("searchGiaDen", OracleDbType.Decimal).Value = searchGiaDen ?? (object)DBNull.Value;
                command.Parameters.Add("searchTrangthai", OracleDbType.Varchar2).Value = searchTrangthai ?? (object)DBNull.Value;
                command.Parameters.Add("searchNgayBatDau", OracleDbType.Date).Value = searchNgayBatDau ?? (object)DBNull.Value;
                command.Parameters.Add("searchNgayKetThuc", OracleDbType.Date).Value = searchNgayKetThuc ?? (object)DBNull.Value;
                command.Parameters.Add("cur", OracleDbType.RefCursor).Direction = ParameterDirection.Output;

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        Hoadon hoadon = new Hoadon
                        {
                            Mahoadon = reader.GetDecimal(0),
                            Maban = reader.GetString(1),
                            Giobatdau = reader.IsDBNull(2) ? (DateTime?)null : reader.GetDateTime(2),
                            Gioketthuc = reader.IsDBNull(3) ? (DateTime?)null : reader.GetDateTime(3),
                            Thoigianchoi = reader.IsDBNull(4) ? (decimal?)null : reader.GetDecimal(4),
                            Thanhtoan = reader.IsDBNull(5) ? (decimal?)null : reader.GetDecimal(5),
                            Khachhang = reader.GetDecimal(6),
                            Tongtien = reader.IsDBNull(7) ? (decimal?)null : reader.GetDecimal(7),
                            Ghichu = reader.IsDBNull(8) ? null : reader.GetString(8),
                            Trangthaithanhtoan = reader.IsDBNull(9) ? null : reader.GetString(9),
                            Taikhoan = reader.GetString(10)
                        };

                        result.Add(hoadon);
                    }
                }
            }

            return result;
        }

        public void Dispose()
        {
            if (_connection != null && _connection.State == ConnectionState.Open)
            {
                _connection.Close();
                _connection.Dispose();
            }
        }
    }
}
