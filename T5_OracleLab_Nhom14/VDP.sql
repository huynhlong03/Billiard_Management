-- Connect tới user ql_Billiard
CONN QL_Billiard/123

--------- VPD
 CREATE OR REPLACE FUNCTION account_access_policy (
    schema_var IN VARCHAR2,
    table_var IN VARCHAR2
) RETURN VARCHAR2 AS
    v_predicate VARCHAR2(1000);
BEGIN
    v_predicate := '1=1';

 IF SYS_CONTEXT('USERENV', 'SESSION_USER') IN ('QL_BILLIARD', 'SYS') THEN
        v_predicate := '1=1'; -- Cho phép quản trị viên và người dùng QL_BILLIARD xem tất cả
    ELSE
      v_predicate := 'QuanLy = 1'; -- Chỉ cho phép xem nếu QuanLy = 1
    END IF;

  RETURN v_predicate;
END;
/


BEGIN
    DBMS_RLS.ADD_POLICY (
        object_schema     => 'QL_BILLIARD',
        object_name       => 'Account',
        policy_name       => 'account_access_policy',
        function_schema   => 'QL_BILLIARD', -- Thay YOUR_SCHEMA bằng schema chứa hàm policy
        policy_function   => 'account_access_policy',
        statement_types   => 'SELECT',
        sec_relevant_cols   => 'taikhoan,matkhau',
        sec_relevant_cols_opt => DBMS_RLS.ALL_ROWS,
        update_check      => FALSE,
        enable            => TRUE
    );
END;
/

--- xóa policy nếu cần thiét
--BEGIN
--    DBMS_RLS.DROP_POLICY(
--        object_schema => 'QL_BILLIARD',
--        object_name => 'Account',
--        policy_name => 'account_access_policy'
--    );
--END;



COMMIT