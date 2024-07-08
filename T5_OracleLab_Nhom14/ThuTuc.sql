-- Connect tới user ql_Billiard
CONN QL_Billiard/123


--- Tính tổng số bàn có trong db
 CREATE OR REPLACE FUNCTION TinhTongBan
RETURN NUMBER
IS
    tongBan NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO tongBan
    FROM Ban;
    
    RETURN tongBan;
END;

GRANT EXECUTE ON QL_BILLIARD.TinhTongBan TO PUBLIC;
--- tính tổng số thành viên đang có
CREATE OR REPLACE FUNCTION TinhTongThanhVien
RETURN NUMBER
IS
    tongTv NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO tongTv
    FROM KhachHang;
    
    RETURN tongTv;
END;

GRANT EXECUTE ON QL_BILLIARD.TinhTongThanhVien TO PUBLIC;

-- tìm kiếm khách hàng theo tên or phone
CREATE OR REPLACE PROCEDURE TimKiemThanhVien(
    searchString IN VARCHAR2,
    cur OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN cur FOR
        SELECT * FROM KhachHang
        WHERE Ten LIKE '%' || searchString || '%' OR Phone LIKE '%' || searchString || '%';
END TimKiemThanhVien;


 
CREATE OR REPLACE PROCEDURE ThemThanhVien(
    Ten NVARCHAR2,
    Phone CHAR,
    LoaiThanhVien NUMBER
)
IS
BEGIN
    -- Thêm dữ liệu mới vào bảng KhachHang
    INSERT INTO KhachHang (Ten, Phone, LoaiThanhVien) VALUES (Ten, Phone, LoaiThanhVien);
    
    -- Hiển thị thông báo thành công
    DBMS_OUTPUT.PUT_LINE('Thêm thành viên thành công!');
EXCEPTION
    WHEN OTHERS THEN
        -- Hiển thị thông báo lỗi nếu có
        DBMS_OUTPUT.PUT_LINE('Đã xảy ra lỗi: ' || SQLERRM);
END ThemThanhVien;

GRANT EXECUTE ON QL_BILLIARD.ThemThanhVien TO PUBLIC;



--- Tìm kiếm hóa đơn theo ngày lập, trạng thái và giá tiền
CREATE OR REPLACE PROCEDURE TimKiemHoaDon(
    searchGiaTu IN NUMBER,
    searchGiaDen IN NUMBER,
    searchTrangthai IN VARCHAR2,
    searchNgayBatDau IN DATE,
    searchNgayKetThuc IN DATE,
    cur OUT SYS_REFCURSOR
)
IS
BEGIN
    OPEN cur FOR
        SELECT * FROM Hoadon
        WHERE (searchGiaTu IS NULL OR TongTien >= searchGiaTu)
          AND (searchGiaDen IS NULL OR TongTien <= searchGiaDen)
          AND (searchTrangthai IS NULL OR Trangthaithanhtoan = searchTrangthai)
          AND (searchNgayBatDau IS NULL OR Giobatdau >= searchNgayBatDau)
          AND (searchNgayKetThuc IS NULL OR Giobatdau <= searchNgayKetThuc)
        ORDER BY Giobatdau DESC;
END TimKiemHoaDon;

GRANT EXECUTE ON QL_BILLIARD.TimKiemHoaDon TO PUBLIC;

--- Tính tổng doanh thu
CREATE OR REPLACE FUNCTION TinhTongTienHoaDon RETURN NUMBER
IS
    totalTongTien NUMBER := 0;
BEGIN
    SELECT SUM(Tongtien) INTO totalTongTien FROM Hoadon;
    RETURN totalTongTien;
END;
/
GRANT EXECUTE ON QL_BILLIARD.TinhTongTienHoaDon TO PUBLIC;


--- Thống kê bàn khách chơi
CREATE OR REPLACE PROCEDURE ThongKeBanKhachChoi(
    SoLuotChoi OUT INT,
    cur OUT SYS_REFCURSOR
)
IS
BEGIN
    SELECT COUNT(h.Mahoadon) INTO SoLuotChoi
    FROM Ql_Billiard.Hoadon h
    JOIN Ql_Billiard.Ban b ON h.Maban = b.Maban;

    OPEN cur FOR
        SELECT b.Loaiban, COUNT(h.Mahoadon) AS SoLuotChoi, SUM(h.Tongtien) AS DoanhThu
        FROM Ql_Billiard.Hoadon h
        JOIN Ql_Billiard.Ban b ON h.Maban = b.Maban
        GROUP BY b.Loaiban;
END ThongKeBanKhachChoi;

GRANT EXECUTE ON QL_BILLIARD.ThongKeBanKhachChoi TO PUBLIC;

--------- VPD

COMMIT


--Trigger này sẽ tự động cập nhật trạng thái của Ban khi một hóa đơn mới được tạo:
CREATE OR REPLACE TRIGGER trg_CapNhatTrangThaiBan
AFTER INSERT ON HoaDon
FOR EACH ROW
BEGIN
    UPDATE Ban
    SET TrangThai = 2 -- 2: Đang sử dụng
    WHERE MaBan = :NEW.MaBan;
END;
/

-- Trigger này sẽ tự động tính toán và cập nhật tổng tiền của hóa đơn khi thêm chi tiết hóa đơn:
CREATE OR REPLACE TRIGGER trg_CapNhatTongTienHoaDon
AFTER INSERT OR UPDATE ON ChiTietHoaDon
FOR EACH ROW
DECLARE
    v_TongTien FLOAT;
BEGIN
    SELECT SUM(cthd.SoLuongDat * td.Gia)
    INTO v_TongTien
    FROM ChiTietHoaDon cthd
    JOIN ThucDon td ON cthd.MaThucDon = td.MaThucDon
    WHERE cthd.MaHoaDon = :NEW.MaHoaDon;
    
    UPDATE HoaDon
    SET TongTien = v_TongTien
    WHERE MaHoaDon = :NEW.MaHoaDon;
END;
/

-- Trigger này sẽ cấm việc xóa một tài khoản nếu tài khoản đó đang được sử dụng trong bảng 
CREATE OR REPLACE TRIGGER trg_KiemTraXoaAccount
BEFORE DELETE ON Account
FOR EACH ROW
DECLARE
    v_Count NUMBER;
BEGIN
    SELECT COUNT(*)
    INTO v_Count
    FROM HoaDon
    WHERE TaiKhoan = :OLD.TaiKhoan;
    
    IF v_Count > 0 THEN
        RAISE_APPLICATION_ERROR(-20001, 'Không thể xóa tài khoản đang được sử dụng trong hóa đơn.');
    END IF;
END;
/

-- Trigger này sẽ tự động cập nhật trạng thái hóa đơn và tính tổng tiền khi bàn chuyển từ trạng thái "Đang sử dụng" sang trạng thái "Trống":
CREATE OR REPLACE TRIGGER trg_CapNhatHoaDonKhiBanTrong
AFTER UPDATE OF TrangThai ON Ban
FOR EACH ROW
DECLARE
    v_MaHoaDon HoaDon.MaHoaDon%TYPE;
    v_TongTien FLOAT;
BEGIN
    IF :OLD.TrangThai = 2 AND :NEW.TrangThai = 1 THEN
        SELECT MaHoaDon INTO v_MaHoaDon
        FROM HoaDon
        WHERE MaBan = :NEW.MaBan
        AND TrangThaiThanhToan = 'Chưa Thanh Toán';
        
        SELECT SUM(cthd.SoLuongDat * td.Gia)
        INTO v_TongTien
        FROM ChiTietHoaDon cthd
        JOIN ThucDon td ON cthd.MaThucDon = td.MaThucDon
        WHERE cthd.MaHoaDon = v_MaHoaDon;
        
        UPDATE HoaDon
        SET TongTien = v_TongTien,
            TrangThaiThanhToan = 'Đã Thanh Toán'
        WHERE MaHoaDon = v_MaHoaDon;
    END IF;
END;
/


-- Function dùng tổng tiền hóa đơn đã thanh toán
CREATE OR REPLACE FUNCTION tongTienHoaDon 
RETURN NUMBER 
IS
    TongTien NUMBER;
BEGIN
    SELECT SUM(TongTien)
    INTO TongTien
    FROM HoaDon 
    WHERE TrangThaiThanhToan = 'Đã Thanh Toán';
    RETURN TongTien;
END;


-- Function này sẽ tính tổng tiền của một hóa đơn dựa trên mã hóa đơn:
CREATE OR REPLACE FUNCTION TinhTongTienHoaDon (p_MaHoaDon NUMBER)
RETURN FLOAT
IS
    v_TongTien FLOAT;
BEGIN
    SELECT SUM(cthd.SoLuongDat * td.Gia)
    INTO v_TongTien
    FROM ChiTietHoaDon cthd
    JOIN ThucDon td ON cthd.MaThucDon = td.MaThucDon
    WHERE cthd.MaHoaDon = p_MaHoaDon;
    
    RETURN v_TongTien;
END;
/

-- Function này sẽ trả về danh sách thực đơn hiện tại:
CREATE OR REPLACE FUNCTION HienThiThucDon
RETURN SYS_REFCURSOR
IS
    v_ThucDon SYS_REFCURSOR;
BEGIN
    OPEN v_ThucDon FOR
    SELECT MaThucDon, TenThucDon, DonViTinh, Gia, GhiChu
    FROM ThucDon;
    
    RETURN v_ThucDon;
END;
/

-- Function này sẽ thêm một món mới vào thực đơn:
CREATE OR REPLACE FUNCTION ThemMonMoi (
    p_MaThucDon CHAR,
    p_TenThucDon NVARCHAR2,
    p_DonViTinh NVARCHAR2,
    p_Gia FLOAT,
    p_Hinh NVARCHAR2,
    p_GhiChu NVARCHAR2
)
RETURN VARCHAR2
IS
BEGIN
    INSERT INTO ThucDon (MaThucDon, TenThucDon, DonViTinh, Gia, Hinh, GhiChu)
    VALUES (p_MaThucDon, p_TenThucDon, p_DonViTinh, p_Gia, p_Hinh, p_GhiChu);
    
    RETURN 'Món ăn đã được thêm thành công';
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        RETURN 'Mã thực đơn đã tồn tại';
    WHEN OTHERS THEN
        RETURN 'Có lỗi xảy ra khi thêm món ăn';
END;
/

-- Function này sẽ thêm một thành viên mới vào hệ thống:
CREATE OR REPLACE FUNCTION ThemThanhVien (
    p_Ten NVARCHAR2,
    p_Phone CHAR,
    p_LoaiThanhVien NUMBER
)
RETURN VARCHAR2
IS
BEGIN
    INSERT INTO KhachHang (Ten, Phone, LoaiThanhVien)
    VALUES (p_Ten, p_Phone, p_LoaiThanhVien);
    
    RETURN 'Thành viên đã được thêm thành công';
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        RETURN 'Số điện thoại đã tồn tại';
    WHEN OTHERS THEN
        RETURN 'Có lỗi xảy ra khi thêm thành viên';
END;
/

--Function này sẽ kiểm tra trạng thái của một tài khoản:
CREATE OR REPLACE FUNCTION KiemTraTrangThaiTaiKhoan (p_TaiKhoan VARCHAR2)
RETURN NVARCHAR2
IS
    v_TinhTrang NVARCHAR2(50);
BEGIN
    SELECT TinhTrang
    INTO v_TinhTrang
    FROM Account
    WHERE TaiKhoan = p_TaiKhoan;
    
    RETURN v_TinhTrang;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        RETURN 'Tài khoản không tồn tại';
    WHEN OTHERS THEN
        RETURN 'Có lỗi xảy ra khi kiểm tra trạng thái tài khoản';
END;
/

-- Thủ tục này sẽ thêm một món mới vào thực đơn:
CREATE OR REPLACE PROCEDURE ThemMonMoiV2 (
    p_MaThucDon IN CHAR,
    p_TenThucDon IN NVARCHAR2,
    p_DonViTinh IN NVARCHAR2,
    p_Gia IN FLOAT,
    p_Hinh IN NVARCHAR2,
    p_GhiChu IN NVARCHAR2
) 
IS
BEGIN
    INSERT INTO ThucDon (MaThucDon, TenThucDon, DonViTinh, Gia, Hinh, GhiChu)
    VALUES (p_MaThucDon, p_TenThucDon, p_DonViTinh, p_Gia, p_Hinh, p_GhiChu);
    
    COMMIT;
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        DBMS_OUTPUT.PUT_LINE('Mã thực đơn đã tồn tại');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Có lỗi xảy ra khi thêm món ăn');
END ThemMonMoiV2;
/

-- Thủ tục này sẽ thêm một thành viên mới vào hệ thống:
CREATE OR REPLACE PROCEDURE ThemThanhVienV2 (
    p_Ten IN NVARCHAR2,
    p_Phone IN CHAR,
    p_LoaiThanhVien IN NUMBER
)
IS
BEGIN
    INSERT INTO KhachHang (Ten, Phone, LoaiThanhVien)
    VALUES (p_Ten, p_Phone, p_LoaiThanhVien);
    
    COMMIT;
EXCEPTION
    WHEN DUP_VAL_ON_INDEX THEN
        DBMS_OUTPUT.PUT_LINE('Số điện thoại đã tồn tại');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Có lỗi xảy ra khi thêm thành viên');
END ThemThanhVienV2;
/

-- Thủ tục này sẽ cập nhật trạng thái thanh toán của hóa đơn khi bàn chuyển từ trạng thái "đang sử dụng" sang "trống":

CREATE OR REPLACE PROCEDURE CapNhatTrangThaiHoaDon (
    p_MaBan IN CHAR
)
IS
    v_MaHoaDon HoaDon.MaHoaDon%TYPE;
    v_ThanhToan FLOAT;
BEGIN
    -- Lấy mã hóa đơn của bàn đó
    SELECT MaHoaDon INTO v_MaHoaDon
    FROM HoaDon
    WHERE MaBan = p_MaBan
    AND TrangThaiThanhToan = 'Chưa Thanh Toán';
    
    -- Tính tổng tiền từ bảng ChiTietHoaDon
    SELECT SUM(cthd.SoLuongDat * td.Gia)
    INTO v_ThanhToan
    FROM ChiTietHoaDon cthd
    JOIN ThucDon td ON cthd.MaThucDon = td.MaThucDon
    WHERE cthd.MaHoaDon = v_MaHoaDon;
    
    -- Cập nhật tổng tiền và trạng thái của hóa đơn
    UPDATE HoaDon
    SET TongTien = v_ThanhToan,
        TrangThaiThanhToan = 'Đã Thanh Toán'
    WHERE MaHoaDon = v_MaHoaDon;
    
    COMMIT;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Không tìm thấy hóa đơn chưa thanh toán cho bàn này');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Có lỗi xảy ra khi cập nhật trạng thái hóa đơn');
END CapNhatTrangThaiHoaDon;
/

-- Thủ tục này sẽ hiển thị danh sách thực đơn hiện tại:
CREATE OR REPLACE PROCEDURE HienThiThucDonV2
IS
    CURSOR cur_ThucDon IS
    SELECT MaThucDon, TenThucDon, DonViTinh, Gia, GhiChu
    FROM ThucDon;
BEGIN
    FOR rec IN cur_ThucDon LOOP
        DBMS_OUTPUT.PUT_LINE('Mã Thực Đơn: ' || rec.MaThucDon);
        DBMS_OUTPUT.PUT_LINE('Tên Thực Đơn: ' || rec.TenThucDon);
        DBMS_OUTPUT.PUT_LINE('Đơn Vị Tính: ' || rec.DonViTinh);
        DBMS_OUTPUT.PUT_LINE('Giá: ' || rec.Gia);
        DBMS_OUTPUT.PUT_LINE('Ghi Chú: ' || rec.GhiChu);
        DBMS_OUTPUT.PUT_LINE('-----------------------');
    END LOOP;
END HienThiThucDonV2;
/

-- Thủ tục này sẽ kiểm tra trạng thái của một tài khoản và hiển thị kết quả:
CREATE OR REPLACE PROCEDURE KiemTraTrangThaiTaiKhoanV2 (
    p_TaiKhoan IN VARCHAR2
)
IS
    v_TinhTrang NVARCHAR2(50);
BEGIN
    SELECT TinhTrang
    INTO v_TinhTrang
    FROM Account
    WHERE TaiKhoan = p_TaiKhoan;
    
    DBMS_OUTPUT.PUT_LINE('Trạng thái tài khoản: ' || v_TinhTrang);
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        DBMS_OUTPUT.PUT_LINE('Tài khoản không tồn tại');
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Có lỗi xảy ra khi kiểm tra trạng thái tài khoản');
END KiemTraTrangThaiTaiKhoanV2;
/

-- tổng số lượng món muốn tìm trong hóa đơn
CREATE OR REPLACE PROCEDURE TongSoLuongMonTrongHoaDon (
    tenMon IN NVARCHAR2,
    tongSoLuong OUT NUMBER
)
IS
BEGIN
    SELECT SUM(cthd.SoLuongDat)
    INTO tongSoLuong
    FROM ChiTietHoaDon cthd
    JOIN ThucDon td ON cthd.MaThucDon = td.MaThucDon
    WHERE td.TenThucDon = tenMon;

    -- Handle case when no rows are found
    IF tongSoLuong IS NULL THEN
        tongSoLuong := 0;
    END IF;
EXCEPTION
    WHEN NO_DATA_FOUND THEN
        tongSoLuong := 0;
    WHEN OTHERS THEN
        DBMS_OUTPUT.PUT_LINE('Error: ' || SQLERRM);
END;
/

COMMIT