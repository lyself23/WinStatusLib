using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Transactions;
using DataBaseInfo;

namespace WinStatusLib.DataBase
{
    public class DataBaseSQL
    {
        private static SqlCommand _SqlCommand = null;
        private static SqlConnection _SqlConnection = null;
        private static string ConnString = "";

        /// <summary>
        /// DB 연결
        /// </summary>
        private static void SetConnectionString()
        {
            try
            {
                ConnString = string.Format("Server={0};Database={1};Uid={2};Pwd={3}", new object[] { DBConnInfo.ServerIP + ", " + DBConnInfo.DataBasePort, DBConnInfo.DataBaseName, DBConnInfo.DataBaseID, DBConnInfo.DataBasePassword });
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터베이스 연결
        /// </summary>
        private static void DataBaseConnect()
        {
            try
            {
                SetConnectionString();
                SqlConnection connection = new SqlConnection
                {
                    ConnectionString = ConnString
                };
                _SqlConnection = connection;
                if (_SqlConnection.State == ConnectionState.Open)
                {
                    _SqlConnection.Close();
                }
                _SqlConnection.Open();
            }

            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 데이터베이스 연결 해제
        /// </summary>
        private static void DataBaseDisConnect()
        {
            try
            {
                if (_SqlConnection.State == ConnectionState.Open)
                {
                    _SqlConnection.Close();
                    _SqlConnection.Dispose();
                }
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// SQL문 실행(데이터테이블반환)
        /// </summary>
        /// <param name="sql">작성한 SQL문</param>
        /// <returns>실행한 SQL문 데이터테이블 반환</returns>
        public static DataTable ExecuteSQLReturnDataTable(string sql)
        {
            DataTable dataTable = new DataTable();

            try
            {
                DataBaseConnect();

                _SqlCommand = new SqlCommand(sql, _SqlConnection);
                new SqlDataAdapter(_SqlCommand).Fill(dataTable);

                return dataTable;
            }
            catch
            {
                throw;
            }
            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// SQL문 실행 (데이터셋반환)
        /// </summary>
        /// <param name="sql">작성한 SQL문</param>
        /// <returns>실행한 SQL 데이터셋반환 </returns>
        public static DataSet ExecuteSQLReturnDataSet(string sql)
        {
            DataSet dataSet = new DataSet();

            try
            {
                DataBaseConnect();
                _SqlCommand = new SqlCommand(sql, _SqlConnection);

                new SqlDataAdapter(_SqlCommand).Fill(dataSet);
                return dataSet;
            }
            catch
            {
                throw;
            }
            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 트랜잭션 포함한 SQL문 실행
        /// </summary>
        /// <param name="sql">작성한 SQL문</param>
        public static void ExecuteTransactionSql(string sql)
        {
            SqlTransaction transaction = null;

            try
            {
                DataBaseConnect();
                transaction = _SqlConnection.BeginTransaction();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.Text;
                _SqlCommand.CommandTimeout = 60;
                _SqlCommand.Transaction = transaction;
                _SqlCommand.CommandText = sql;

                _SqlCommand.ExecuteNonQuery();

                transaction.Commit();
            }

            catch (SqlException)
            {
                transaction.Rollback();
                throw;
            }
            catch
            {
                throw;
            }
            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// SQL문 실행
        /// </summary>
        /// <param name="sql">작성한 SQL문</param>
        public static void ExecuteSql(string sql)
        {
            SqlTransaction transaction = null;

            try
            {
                DataBaseConnect();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.Text;
                _SqlCommand.CommandTimeout = 60;
                _SqlCommand.CommandText = sql;

                _SqlCommand.ExecuteNonQuery();
            }

            catch (SqlException)
            {
                transaction.Rollback();
                throw;
            }
            catch
            {
                throw;
            }
            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 분산트랜잭션 포함한 SQL문 실행
        /// </summary>
        /// <param name="sql">작성한 SQL문</param>
        public static void ExecuteTransactionScopeSql(string sql)
        {
            try
            {
                using (TransactionScope scope = new TransactionScope(TransactionScopeOption.RequiresNew))
                {
                    try
                    {
                        DataBaseConnect();

                        _SqlCommand = new SqlCommand();
                        _SqlCommand.Connection = _SqlConnection;
                        _SqlCommand.CommandType = CommandType.Text;
                        _SqlCommand.CommandTimeout = 60;
                        _SqlCommand.CommandText = sql;

                        _SqlCommand.ExecuteNonQuery();
                    }
                    catch (SqlException)
                    {
                        throw;
                    }

                    scope.Complete();
                }
            }
            catch (TransactionAbortedException)
            {
                throw;
            }
        }

        /// <summary>
        /// 트랜잭션 포함한 저장 프로시저 실행
        /// </summary>
        /// <param name="ProcName">저장프로시저 이름</param>
        /// <param name="ParamValue">저장프로시저 매개변수 값</param>
        public static void ExecuteTransactionProcedure(string ProcName, string[] ParamValue)
        {
            SqlTransaction transaction = null;
            DataTable paramTable = new DataTable();
            try
            {
                int num;
                string sql = "SELECT	PROC_NM     = object_name(PRM.object_id)," + "\r\n" +
                             "         PARAM_NM    = PRM.name ,  " + "\r\n" +
                             "         PARAM_TYPE	= TYP.name  +" + "\r\n" +
                             "                       CASE	WHEN TYP.name IN ('nvarchar','nchar','varchar','char','varbinary','binary')" + "\r\n" +
                             "                             THEN" + "\r\n" +
                             "                                 CASE WHEN PRM.max_length = -1" + "\r\n" +
                             "                                      THEN '(max)'" + "\r\n" +
                             "                                      WHEN TYP.name IN ('nvarchar','nchar')" +
                             "                                      THEN '(' + CAST(PRM.max_length/2 AS varchar(25)) + ')'" + "\r\n" +
                             "                                      ELSE '(' + CAST(PRM.max_length   AS varchar(25)) + ')'" + "\r\n" +
                             "                                 END" + "\r\n" +
                             "                             WHEN TYP.name IN ('decimal','numeric')" + "\r\n" +
                             "                             THEN       '('+CAST(PRM.precision AS varchar(25))+','+CAST(PRM.scale AS varchar(25))+')'" + "\r\n" +
                             "                             ELSE   ''" + "\r\n" +
                             "                       END ,   " + "\r\n" +
                             "         PARAM_ORDER = PRM.parameter_id" + "\r\n" +
                             "FROM	sys.parameters AS PRM" + "\r\n" +
                             "INNER JOIN   sys.types AS TYP ON PRM.system_type_id = TYP.system_type_id" + "\r\n" +
                             $"WHERE object_name(PRM.object_id) = {ProcName}" + "\r\n" +
                             "ORDER BY	object_name(PRM.object_id),    PRM.parameter_id";

                paramTable = ExecuteSQLReturnDataTable(sql);
                Dictionary<string, object> args = new Dictionary<string, object>();
                for (num = 0; num < paramTable.Rows.Count; num++)
                {
                    args.Add(paramTable.Rows[num]["PARAM_NM"].ToString(), ParamValue[num]);
                }
                SqlParameter[] sqlServerParameter = GetSqlServerParameter(args, ProcName);


                DataBaseConnect();
                transaction = _SqlConnection.BeginTransaction();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.StoredProcedure;
                _SqlCommand.CommandText = ProcName;
                _SqlCommand.CommandTimeout = 60;
                _SqlCommand.Transaction = transaction;

                foreach (SqlParameter parameter in sqlServerParameter)
                {
                    _SqlCommand.Parameters.Add(parameter);
                }

                _SqlCommand.ExecuteNonQuery();
                transaction.Commit();
            }
            catch (SqlException)
            {
                transaction.Rollback();
                throw;
            }
            catch
            {
                throw;
            }

            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 저장프로시저 실행 (데이터테이블반환)
        /// </summary>
        /// <param name="ProcName">저장프로시저 이름</param>
        /// <param name="ParamValue">저장프로시저 매개변수 값</param>
        /// <returns></returns>
        public static DataTable ExecuteProcedureReturnDataTable(string ProcName, string[] ParamValue)
        {
            DataTable dataTable = new DataTable();
            DataTable paramTable = new DataTable();
            try
            {
                int num;
                string sql = "SELECT	PROC_NM     = object_name(PRM.object_id)," + "\r\n" +
                             "         PARAM_NM    = PRM.name ,  " + "\r\n" +
                             "         PARAM_TYPE	= TYP.name  +" + "\r\n" +
                             "                       CASE	WHEN TYP.name IN ('nvarchar','nchar','varchar','char','varbinary','binary')" + "\r\n" +
                             "                             THEN" + "\r\n" +
                             "                                 CASE WHEN PRM.max_length = -1" + "\r\n" +
                             "                                      THEN '(max)'" + "\r\n" +
                             "                                      WHEN TYP.name IN ('nvarchar','nchar')" +
                             "                                      THEN '(' + CAST(PRM.max_length/2 AS varchar(25)) + ')'" + "\r\n" +
                             "                                      ELSE '(' + CAST(PRM.max_length   AS varchar(25)) + ')'" + "\r\n" +
                             "                                 END" + "\r\n" +
                             "                             WHEN TYP.name IN ('decimal','numeric')" + "\r\n" +
                             "                             THEN       '('+CAST(PRM.precision AS varchar(25))+','+CAST(PRM.scale AS varchar(25))+')'" + "\r\n" +
                             "                             ELSE   ''" + "\r\n" +
                             "                       END ,   " + "\r\n" +
                             "         PARAM_ORDER = PRM.parameter_id" + "\r\n" +
                             "FROM	sys.parameters AS PRM" + "\r\n" +
                             "INNER JOIN   sys.types AS TYP ON PRM.system_type_id = TYP.system_type_id" + "\r\n" +
                             $"WHERE object_name(PRM.object_id) = {ProcName}" + "\r\n" +
                             "ORDER BY	object_name(PRM.object_id),    PRM.parameter_id";

                paramTable = ExecuteSQLReturnDataTable(sql);
                Dictionary<string, object> args = new Dictionary<string, object>();
                for (num = 0; num < paramTable.Rows.Count; num++)
                {
                    args.Add(paramTable.Rows[num]["PARAM_NM"].ToString(), ParamValue[num]);
                }
                SqlParameter[] sqlServerParameter = GetSqlServerParameter(args, ProcName);


                DataBaseConnect();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.StoredProcedure;
                _SqlCommand.CommandText = ProcName;
                _SqlCommand.CommandTimeout = 60;

                foreach (SqlParameter parameter in sqlServerParameter)
                {
                    _SqlCommand.Parameters.Add(parameter);
                }

                new SqlDataAdapter(_SqlCommand).Fill(dataTable);
                return dataTable;
            }

            catch
            {
                throw;
            }

            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 저장 프로시저 실행
        /// </summary>
        /// <param name="ProcName">저장프로시저 이름</param>
        /// <param name="ParamValue">저장프로시저 매개변수 값</param>
        public static void ExecuteProcedure(string ProcName, string[] ParamValue)
        {
            DataTable dataTable = new DataTable();
            DataTable paramTable = new DataTable();
            try
            {
                int num;
                string sql = "SELECT	PROC_NM     = object_name(PRM.object_id)," + "\r\n" +
                              "         PARAM_NM    = PRM.name ,  " + "\r\n" +
                              "         PARAM_TYPE	= TYP.name  +" + "\r\n" +
                              "                       CASE	WHEN TYP.name IN ('nvarchar','nchar','varchar','char','varbinary','binary')" + "\r\n" +
                              "                             THEN" + "\r\n" +
                              "                                 CASE WHEN PRM.max_length = -1" + "\r\n" +
                              "                                      THEN '(max)'" + "\r\n" +
                              "                                      WHEN TYP.name IN ('nvarchar','nchar')" +
                              "                                      THEN '(' + CAST(PRM.max_length/2 AS varchar(25)) + ')'" + "\r\n" +
                              "                                      ELSE '(' + CAST(PRM.max_length   AS varchar(25)) + ')'" + "\r\n" +
                              "                                 END" + "\r\n" +
                              "                             WHEN TYP.name IN ('decimal','numeric')" + "\r\n" +
                              "                             THEN       '('+CAST(PRM.precision AS varchar(25))+','+CAST(PRM.scale AS varchar(25))+')'" + "\r\n" +
                              "                             ELSE   ''" + "\r\n" +
                              "                       END ,   " + "\r\n" +
                              "         PARAM_ORDER = PRM.parameter_id" + "\r\n" +
                              "FROM	sys.parameters AS PRM" + "\r\n" +
                              "INNER JOIN   sys.types AS TYP ON PRM.system_type_id = TYP.system_type_id" + "\r\n" +
                              $"WHERE object_name(PRM.object_id) = {ProcName}" + "\r\n" +
                              "ORDER BY	object_name(PRM.object_id),    PRM.parameter_id";

                paramTable = ExecuteSQLReturnDataTable(sql);
                Dictionary<string, object> args = new Dictionary<string, object>();
                for (num = 0; num < paramTable.Rows.Count; num++)
                {
                    args.Add(paramTable.Rows[num]["PARAM_NM"].ToString(), ParamValue[num]);
                }
                SqlParameter[] sqlServerParameter = GetSqlServerParameter(args, ProcName);


                DataBaseConnect();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.StoredProcedure;
                _SqlCommand.CommandText = ProcName;
                _SqlCommand.CommandTimeout = 60;

                foreach (SqlParameter parameter in sqlServerParameter)
                {
                    _SqlCommand.Parameters.Add(parameter);
                }
                _SqlCommand.ExecuteNonQuery();

            }
            catch
            {
                throw;
            }

            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 저장 프로시저 실행 (데이터셋반환)
        /// </summary>
        /// <param name="ProcName">저장프로시저 이름</param>
        /// <param name="ParamValue">저장프로시저 매개변수 값</param>
        /// <returns>저장 프로시저 실행결과(데이터셋)</returns>
        public static DataSet ExecuteProcedureReturnDataSet(string ProcName, string[] ParamValue)
        {
            DataSet dataSet = new DataSet();
            DataTable paramTable = new DataTable();
            try
            {
                int num;
                string sql = "SELECT	PROC_NM     = object_name(PRM.object_id)," + "\r\n" +
                              "         PARAM_NM    = PRM.name ,  " + "\r\n" +
                              "         PARAM_TYPE	= TYP.name  +" + "\r\n" +
                              "                       CASE	WHEN TYP.name IN ('nvarchar','nchar','varchar','char','varbinary','binary')" + "\r\n" +
                              "                             THEN" + "\r\n" +
                              "                                 CASE WHEN PRM.max_length = -1" + "\r\n" +
                              "                                      THEN '(max)'" + "\r\n" +
                              "                                      WHEN TYP.name IN ('nvarchar','nchar')" +
                              "                                      THEN '(' + CAST(PRM.max_length/2 AS varchar(25)) + ')'" + "\r\n" +
                              "                                      ELSE '(' + CAST(PRM.max_length   AS varchar(25)) + ')'" + "\r\n" +
                              "                                 END" + "\r\n" +
                              "                             WHEN TYP.name IN ('decimal','numeric')" + "\r\n" +
                              "                             THEN       '('+CAST(PRM.precision AS varchar(25))+','+CAST(PRM.scale AS varchar(25))+')'" + "\r\n" +
                              "                             ELSE   ''" + "\r\n" +
                              "                       END ,   " + "\r\n" +
                              "         PARAM_ORDER = PRM.parameter_id" + "\r\n" +
                              "FROM	sys.parameters AS PRM" + "\r\n" +
                              "INNER JOIN   sys.types AS TYP ON PRM.system_type_id = TYP.system_type_id" + "\r\n" +
                              $"WHERE object_name(PRM.object_id) = {ProcName}" + "\r\n" +
                              "ORDER BY	object_name(PRM.object_id),    PRM.parameter_id";

                paramTable = ExecuteSQLReturnDataTable(sql);
                Dictionary<string, object> args = new Dictionary<string, object>();
                for (num = 0; num < paramTable.Rows.Count; num++)
                {
                    args.Add(paramTable.Rows[num]["PARAM_NM"].ToString(), ParamValue[num]);
                }
                SqlParameter[] sqlServerParameter = GetSqlServerParameter(args, ProcName);

                DataBaseConnect();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.StoredProcedure;
                _SqlCommand.CommandText = ProcName;
                _SqlCommand.CommandTimeout = 60;

                foreach (SqlParameter parameter in sqlServerParameter)
                {
                    _SqlCommand.Parameters.Add(parameter);
                }

                new SqlDataAdapter(_SqlCommand).Fill(dataSet);

                return dataSet;

            }
            catch
            {
                throw;
            }

            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 저장 프로시저 실행 (데이터셋 반환)
        /// </summary>
        /// <param name="ProcName">저장프로시저 이름</param>
        /// <param name="ParamName">매개변수 이름</param>
        /// <param name="ParamValue">매개변수 값</param>
        /// <returns>저장프로시저 실행 결과 (데이터셋 반환)</returns>
        public static DataSet ExecuteProcedureReturnDataSet(string ProcName, ArrayList ParamName, ArrayList ParamValue)
        {
            DataSet dataSet = new DataSet();
            try
            {
                int num;
                Dictionary<string, object> args = new Dictionary<string, object>();
                for (num = 0; num < ParamName.Count; num++)
                {
                    args.Add(ParamName[num].ToString(), ParamValue[num]);
                }
                SqlParameter[] sqlServerParameter = GetSqlServerParameter(args, ProcName);

                DataBaseConnect();

                _SqlCommand = new SqlCommand();
                _SqlCommand.Connection = _SqlConnection;
                _SqlCommand.CommandType = CommandType.StoredProcedure;
                _SqlCommand.CommandText = ProcName;
                _SqlCommand.CommandTimeout = 60;

                foreach (SqlParameter parameter in sqlServerParameter)
                {
                    _SqlCommand.Parameters.Add(parameter);
                }
                new SqlDataAdapter(_SqlCommand).Fill(dataSet);
                return dataSet;
            }
            catch
            {
                throw;
            }

            finally
            {
                DataBaseDisConnect();
            }
        }

        /// <summary>
        /// 저장프로시저 SQL문 생성
        /// </summary>
        /// <param name="ProcedureName">저장프로시저 이름</param>
        /// <param name="value">저장프로시저 매개변수 값</param>
        /// <returns>SQL문</returns>
        public static string MakeProcedureQuery(string ProcedureName, string[] value)
        {
            string sql = "EXEC " + ProcedureName + " ";

            for (int i = 0; i < value.Length; i++)
            {
                sql += "'" + value[i] + "'";

                if (i != value.Length - 1)
                {
                    sql += ",";
                }
            }

            return sql;
        }

        /// <summary>
        /// 저장프로시저 매개변수 타입 찾기
        /// </summary>
        /// <param name="args">매개변수 타입</param>
        /// <param name="paramName">매개변수 이름</param>
        /// <returns></returns>
        private static object GetArgs(Dictionary<string, object> args, string paramName)
        {
            object obj2 = new object();
            try
            {
                foreach (string str in args.Keys)
                {
                    if (str == paramName)
                    {
                        return args[str];
                    }
                }
                return obj2;
            }
            catch
            {
                throw;
            }
        }

        /// <summary>
        /// 저장프로시저 매개변수 타입 가져오기
        /// </summary>
        /// <param name="args">매개변수 타입</param>
        /// <param name="procName">저장프로시저 이름</param>
        /// <returns>SQL 매개변수</returns>
        private static SqlParameter[] GetSqlServerParameter(Dictionary<string, object> args, string procName)
        {
            SqlParameter[] parameterArray = new SqlParameter[1];
            string str = "";
            int index = 0;
            str = "SELECT B.name AS PARAM_NAME,\r\n" +
                   "      C.name AS PARAM_TYPE,\r\n" +
                   "      B.max_length AS PARAM_SIZE,\r\n" +
                   "      (CASE WHEN B.is_output = '0' THEN 'INPUT' ELSE 'OUTPUT' END) AS PARAM_DIRECTION\r\n" +
                   "  FROM sys.objects A INNER JOIN\r\n" +
                   "       sys.all_parameters B ON(A.object_id = B.object_id) INNER JOIN\r\n" +
                   "       sys.types C ON(B.system_type_id = C.system_type_id)\r\n" +
                   " WHERE A.name = '" + procName + "' \r\n" +
                   "ORDER BY B.parameter_id \r\n";
            try
            {
                DataTable dataTable = new DataTable();
                dataTable = ExecuteSQLReturnDataTable(str);

                parameterArray = new SqlParameter[dataTable.Rows.Count];
                foreach (DataRow row in dataTable.Rows)
                {
                    parameterArray[index] = new SqlParameter();
                    parameterArray[index].ParameterName = row["PARAM_NAME"].ToString();
                    parameterArray[index].IsNullable = true;
                    switch (row["PARAM_TYPE"].ToString())
                    {
                        case "nvarchar":
                            parameterArray[index].SqlDbType = SqlDbType.NVarChar;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = GetArgs(args, row["PARAM_NAME"].ToString()).ToString();
                            }
                            parameterArray[index].Size = int.Parse(row["PARAM_SIZE"].ToString());
                            break;

                        case "varchar":
                            parameterArray[index].SqlDbType = SqlDbType.VarChar;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = GetArgs(args, row["PARAM_NAME"].ToString()).ToString();
                            }
                            parameterArray[index].Size = int.Parse(row["PARAM_SIZE"].ToString());
                            break;

                        case "char":
                            parameterArray[index].SqlDbType = SqlDbType.Char;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = GetArgs(args, row["PARAM_NAME"].ToString()).ToString();
                            }
                            parameterArray[index].Size = int.Parse(row["PARAM_SIZE"].ToString());
                            break;

                        case "date":
                            parameterArray[index].SqlDbType = SqlDbType.Date;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = GetArgs(args, row["PARAM_NAME"].ToString()).ToString();
                            }
                            parameterArray[index].Size = int.Parse(row["PARAM_SIZE"].ToString());
                            break;

                        case "datetime":
                            parameterArray[index].SqlDbType = SqlDbType.DateTime;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = GetArgs(args, row["PARAM_NAME"].ToString()).ToString();
                            }
                            parameterArray[index].Size = int.Parse(row["PARAM_SIZE"].ToString());
                            break;

                        case "numeric":
                            parameterArray[index].SqlDbType = SqlDbType.Decimal;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = decimal.Parse(GetArgs(args, row["PARAM_NAME"].ToString()).ToString());
                            }
                            break;

                        case "decimal":
                            parameterArray[index].SqlDbType = SqlDbType.Decimal;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = decimal.Parse(GetArgs(args, row["PARAM_NAME"].ToString()).ToString());
                            }
                            break;

                        case "int":
                            parameterArray[index].SqlDbType = SqlDbType.Int;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = int.Parse(GetArgs(args, row["PARAM_NAME"].ToString()).ToString());
                            }
                            break;

                        case "integer":
                            parameterArray[index].SqlDbType = SqlDbType.Int;
                            if (row["PARAM_DIRECTION"].ToString() == "INPUT")
                            {
                                parameterArray[index].Value = int.Parse(GetArgs(args, row["PARAM_NAME"].ToString()).ToString());
                            }
                            break;
                    }
                    if (row["PARAM_DIRECTION"].ToString() == "OUTPUT")
                    {
                        parameterArray[index].Direction = ParameterDirection.Output;
                    }
                    else
                    {
                        parameterArray[index].Direction = ParameterDirection.Input;
                        index++;
                    }
                }
                return parameterArray;
            }
            catch
            {
                throw;
            }
        }
    }
}
