-- Connect tới user SYSDBA
CONN SYS/123 AS SYSDBA


alter session set "_oracle_script"=true;

---- Create schema in Oracle
CREATE USER Ql_Billiard IDENTIFIED BY 123;
GRANT CONNECT, RESOURCE TO Ql_Billiard;
ALTER USER Ql_Billiard DEFAULT TABLESPACE users;
ALTER USER Ql_Billiard QUOTA UNLIMITED ON users;
--

GRANT CREATE ANY CONTEXT TO Ql_Billiard;
GRANT EXECUTE ON DBMS_SESSION TO Ql_Billiard;
GRANT EXECUTE ON DBMS_RLS TO Ql_Billiard;
GRANT CREATE TRIGGER TO Ql_Billiard;


-- Create profile for admin
CREATE PROFILE admin_profile LIMIT
    SESSIONS_PER_USER UNLIMITED
    CPU_PER_SESSION UNLIMITED
    CPU_PER_CALL UNLIMITED
    CONNECT_TIME UNLIMITED
    IDLE_TIME UNLIMITED
    LOGICAL_READS_PER_SESSION UNLIMITED
    LOGICAL_READS_PER_CALL UNLIMITED
    COMPOSITE_LIMIT UNLIMITED
    PRIVATE_SGA UNLIMITED
    FAILED_LOGIN_ATTEMPTS 5
    PASSWORD_LIFE_TIME 90
    PASSWORD_REUSE_TIME 365
    PASSWORD_REUSE_MAX 10
    PASSWORD_VERIFY_FUNCTION NULL
    PASSWORD_LOCK_TIME 1
    PASSWORD_GRACE_TIME 7;


-- Create profile for staff
CREATE PROFILE staff_profile LIMIT
    SESSIONS_PER_USER UNLIMITED
    CPU_PER_SESSION UNLIMITED
    CPU_PER_CALL UNLIMITED
    CONNECT_TIME UNLIMITED
    IDLE_TIME UNLIMITED
    LOGICAL_READS_PER_SESSION UNLIMITED
    LOGICAL_READS_PER_CALL UNLIMITED
    COMPOSITE_LIMIT UNLIMITED
    PRIVATE_SGA UNLIMITED
    FAILED_LOGIN_ATTEMPTS 5
    PASSWORD_LIFE_TIME 90
    PASSWORD_REUSE_TIME 365
    PASSWORD_REUSE_MAX 10
    PASSWORD_VERIFY_FUNCTION NULL
    PASSWORD_LOCK_TIME 1
    PASSWORD_GRACE_TIME 7;


-- Create role for admin and grant necessary privileges
CREATE ROLE admin_role;
GRANT CONNECT TO admin_role;

GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.LoaiBan TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.LoaiThanhVien TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.TrangThai TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.Account TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.Ban TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.HoaDon TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.ThucDon TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.KhachHang TO admin_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.ChiTietHoaDon TO admin_role;

-- Create role for staff and grant necessary privileges
CREATE ROLE staff_role;
GRANT CONNECT TO staff_role;

GRANT SELECT ON Ql_Billiard.LoaiBan TO staff_role;
GRANT SELECT ON Ql_Billiard.LoaiThanhVien TO staff_role;
GRANT SELECT ON Ql_Billiard.TrangThai TO staff_role;
GRANT SELECT ON Ql_Billiard.Account TO staff_role;
GRANT SELECT ON Ql_Billiard.Ban TO staff_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.HoaDon TO staff_role;
GRANT SELECT ON Ql_Billiard.ThucDon TO staff_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.KhachHang TO staff_role;
GRANT SELECT, INSERT, UPDATE, DELETE ON Ql_Billiard.ChiTietHoaDon TO staff_role;

-- Create or replace package account_management_pkg
CREATE OR REPLACE PACKAGE UserManagement AS
    PROCEDURE CreateUser(p_username VARCHAR2, p_password VARCHAR2, p_quanly INT); 
END UserManagement;
/

CREATE OR REPLACE PACKAGE BODY UserManagement AS
    PROCEDURE CreateUser(p_username VARCHAR2, p_password VARCHAR2, p_quanly INT) IS
        PRAGMA AUTONOMOUS_TRANSACTION;
    BEGIN
        IF p_quanly = 0 THEN
            EXECUTE IMMEDIATE 'CREATE USER ' || p_username || ' IDENTIFIED BY ' || p_password || ' PROFILE staff_profile';
            EXECUTE IMMEDIATE 'GRANT CONNECT, RESOURCE TO ' || p_username;
            EXECUTE IMMEDIATE 'GRANT staff_role TO ' || p_username;
        ELSIF p_quanly = 1 THEN
            EXECUTE IMMEDIATE 'CREATE USER ' || p_username || ' IDENTIFIED BY ' || p_password || ' PROFILE admin_profile';
            EXECUTE IMMEDIATE 'GRANT CONNECT, RESOURCE TO ' || p_username;
            EXECUTE IMMEDIATE 'GRANT admin_role TO ' || p_username;
        ELSE 
            RAISE_APPLICATION_ERROR(-20001, 'Invalid role specified: ' || p_quanly);
        END IF;
        COMMIT;
    END CreateUser;
END UserManagement;
/


CREATE OR REPLACE TRIGGER CreateUserTrigger
AFTER INSERT ON ql_billiard.Account
FOR EACH ROW
BEGIN
        UserManagement.CreateUser(:new.TaiKhoan, :new.MatKhau, :new.QuanLy);
END;
/

--GRANT SYSDBA TO ql_billiard CONTAINER=ALL;


COMMIT