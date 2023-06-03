using Newtonsoft.Json;
using PEMSoft.Web.AppServer.Core.Common;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Web.Services;
using XGDbUtility;

namespace GeneralSQLAPI
{
    /// <summary>
    /// GeneralAPI 的摘要说明
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // 若要允许使用 ASP.NET AJAX 从脚本中调用此 Web 服务，请取消注释以下行。 
     [System.Web.Script.Services.ScriptService]
    public class GeneralAPI : System.Web.Services.WebService
    {
        [WebMethod]
        public R<string> AddUpdate(string MainBill, string MainPrimaryKeys, string DetailBill, string DetailPrimaryKeys, string JsonData)
        {
            try 
            {
                //解析字符串
                dynamic data = JsonConvert.DeserializeObject(JsonData);

                //首先判断是单表还是主从表
                if (!string.IsNullOrEmpty(MainBill) && !string.IsNullOrEmpty(MainPrimaryKeys))
                {
                    //主表不为空，从表不为空的情况
                    if (!string.IsNullOrEmpty(DetailBill) && !string.IsNullOrEmpty(DetailPrimaryKeys))
                    {
                        //处理主从表,解析Json字符串
                        if (data.MainTable != null && data.DetailTable != null)
                        {
                            //先查主表从表有没有数据，进行更新或者新增
                            // 处理主表数据
                            var mainTableData = data.MainTable;
                            if (mainTableData.Count == 0)
                            {
                                throw new Exception("传输数据异常");
                            }

                            // 处理从表数据
                            var detailTableData = data.DetailTable;
                            if (detailTableData.Count == 0)
                            {
                                throw new Exception("传输数据异常");
                            }

                            bool mainTableDealResult= GenerateTableSql("AddUpdate", MainBill, MainPrimaryKeys, mainTableData);

                            bool detailTableDealResult = GenerateTableSql("AddUpdate",DetailBill, DetailPrimaryKeys, detailTableData);
                            
                            //判断是否执行成功
                            if (mainTableDealResult && detailTableDealResult)
                            {
                                return R<string>.Success("数据更新成功！");
                            }
                            else
                            {
                                throw new Exception("数据更新失败！");
                            }
                        }
                        else 
                        {
                            throw new Exception("传输数据异常！");
                        }
                    }

                    //处理只处理单表
                    var mainTableDataOne = data.MainTable;
                    if (mainTableDataOne.Count == 0)
                    {
                        throw new Exception("传输数据异常");
                    }

                    bool mainTableDataOneDealResult = GenerateTableSql("AddUpdate", MainBill, MainPrimaryKeys, mainTableDataOne);

                    //判断是否执行成功
                    if (mainTableDataOneDealResult)
                    {
                        return R<string>.Success("数据更新成功！");
                    }
                    else
                    {
                        throw new Exception("数据更新失败！");
                    }
                }
                else 
                {
                    throw new Exception("传输数据异常！");
                }

                throw new Exception("服务器异常，更新数据失败！");
            }
            catch (Exception ex)
            {
                return R<string>.Error("服务器异常，" + ex.Message + "更新数据失败！");
            }
        }

        [WebMethod]
        public R<string> Delete(string MainBill, string MainPrimaryKeys, string DetailBill, string DetailPrimaryKeys, string JsonData) 
        {
            try
            {
                //解析字符串
                dynamic data = JsonConvert.DeserializeObject(JsonData);

                //首先判断是单表还是主从表
                if (!string.IsNullOrEmpty(MainBill) && !string.IsNullOrEmpty(MainPrimaryKeys)) 
                {
                    //主表不为空，从表不为空的情况
                    if (!string.IsNullOrEmpty(DetailBill) && !string.IsNullOrEmpty(DetailPrimaryKeys))
                    {
                        //处理主从表,解析Json字符串
                        if (data.MainTable != null && data.DetailTable != null)
                        {
                            // 处理主表数据
                            var mainTableData = data.MainTable;
                            if (mainTableData.Count == 0)
                            {
                                throw new Exception("传输数据异常");
                            }

                            // 处理从表数据
                            var detailTableData = data.DetailTable;
                            if (detailTableData.Count == 0)
                            {
                                throw new Exception("传输数据异常");
                            }

                            bool mainTableDealResult = GenerateTableSql("Delete", MainBill, MainPrimaryKeys, mainTableData);

                            bool detailTableDealResult = GenerateTableSql("Delete", DetailBill, DetailPrimaryKeys, detailTableData);

                            //判断是否执行成功
                            if (mainTableDealResult && detailTableDealResult)
                            {
                                return R<string>.Success("数据删除成功！");
                            }
                            else
                            {
                                throw new Exception("数据删除失败！");
                            }
                        }
                        else
                        {
                            throw new Exception("传输数据异常！");
                        }
                    }

                    //处理只处理单表
                    var mainTableDataOne = data.MainTable;
                    if (mainTableDataOne.Count == 0)
                    {
                        throw new Exception("传输数据异常");
                    }

                    bool mainTableDataOneDealResult = GenerateTableSql("Delete", MainBill, MainPrimaryKeys, mainTableDataOne);

                    //判断是否执行成功
                    if (mainTableDataOneDealResult)
                    {
                        return R<string>.Success("数据删除成功！");
                    }
                    else
                    {
                        throw new Exception("数据删除失败！");
                    }
                }

                throw new Exception("服务器异常，删除数据失败！");
            }
            catch (Exception ex) 
            {
                return R<string>.Error("服务器异常，" + ex.Message + "删除数据失败！");
            }
            
        }

        [WebMethod]
        public R<string> Select(string sql, string Json) 
        {
            try
            {
                string rowJson = Json.ToString();

                // 提取键值对
                Dictionary<string, object> Jsons = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowJson);

                // 遍历键值对，替换查询语句中的参数值
                foreach (var kvp in Jsons)
                {
                    string paramName = $"@{kvp.Key}";
                    string paramValue = $"'{kvp.Value.ToString()}'";
                    sql = sql.Replace(paramName, paramValue);
                }

                SqlHelper db = SqlManager.GetDbCjgl();

                DataTable dt = db.GetDataTable(sql);

                string result = JsonConvert.SerializeObject(dt);

                return R<string>.Success(result);
            }
            catch (Exception ex)
            {
                return R<string>.Error("服务器异常，" + ex.Message + "获取数据失败！");
            }
        }


        /// <summary>
        /// 处理SQL语句
        /// </summary>
        /// <param name="deal">处理逻辑</param>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeys">主键</param>
        /// <param name="data">JSON数据</param>
        /// <returns></returns>
        private bool GenerateTableSql(string deal,string tableName, string primaryKeys, dynamic data)
        {
            Dictionary< string, List<SqlParameter>> sqldic = new Dictionary<string, List<SqlParameter>>();

            int count = 0;

            foreach (var row in data)
            {
                // 查询数据库数据是否存在
                bool recordExists = CheckTableRecordExists(tableName, primaryKeys, row);

                if (recordExists && deal.Equals("AddUpdate"))
                {
                    //如果主键存在，对比其他属性是否需要更新


                    // 获取更新语句
                    sqldic = GenerateUpdateTableSql(tableName, primaryKeys, row);

                    //执行sql语句
                    if (sqldic.Count != 0)
                    {
                        //执行数据库操作
                        SqlHelper db = SqlManager.GetDbCjgl();

                        count += db.ExecuteSql(sqldic.First().Key, sqldic.First().Value.ToArray());
                    }
                }
                else if (deal.Equals("AddUpdate"))
                {
                    // 获取插入语句
                    sqldic = GenerateInsertTableSql(tableName, row);

                    //执行sql语句
                    if (sqldic.Count != 0)
                    {
                        //执行数据库操作
                        SqlHelper db = SqlManager.GetDbCjgl();

                        count += db.ExecuteSql(sqldic.First().Key, sqldic.First().Value.ToArray());
                    }
                }
                else if (recordExists && deal.Equals("Delete"))
                {
                    //获取删除语句
                    sqldic = GenerateDeleteTableSql(tableName, primaryKeys, row);

                    //执行sql语句
                    if (sqldic.Count != 0)
                    {
                        //执行数据库操作
                        SqlHelper db = SqlManager.GetDbCjgl();

                        count += db.ExecuteSql(sqldic.First().Key, sqldic.First().Value.ToArray());
                    }
                }
                else 
                {
                    return false;      
                }
            }

            return count > 0;
        }


        /// <summary>
        /// 根据主键查询是否有记录
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeys">主键</param>
        /// <param name="row">行数据</param>
        /// <returns></returns>
        private bool CheckTableRecordExists(string tableName, string primaryKeys, dynamic row)
        {
            string[] primaryKeyArray = primaryKeys.Split(',');

            // 拼接 WHERE 子句，根据主键查询数据库数据
            string whereClause = "";
            foreach (var primaryKey in primaryKeyArray)
            {
                if (row[primaryKey] != null)
                {
                    whereClause += $"{primaryKey} = '{row[primaryKey]}' AND ";
                }
            }
            whereClause = whereClause.TrimEnd(new char[] { ' ', 'A', 'N', 'D' });

            // 根据表名称和 WHERE 子句查询数据库数据
            // 返回数据是否存在的布尔值
            SqlHelper db = SqlManager.GetDbCjgl();
            string querySql = $"SELECT COUNT(*) FROM {tableName} WHERE {whereClause}";
            int count = 0;
            // 执行 querySql 语句，获取结果 count
            DataTable dt = db.GetDataTable(querySql);
            count = Convert.ToInt32(dt.Rows[0][0].ToString());

            return count > 0;
        }

        /// <summary>
        /// 根据主键删除数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeys">主键</param>
        /// <param name="row">行数据</param>
        /// <returns></returns>
        private Dictionary<string, List<SqlParameter>> GenerateDeleteTableSql(string tableName, string primaryKeys, dynamic row)
        {
            string rowJson = row.ToString();

            // 提取键值对
            Dictionary<string, object> rows = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowJson);

            string[] primaryKeyArray = primaryKeys.Split(',');

            // 拼接 WHERE 子句
            string whereClause = "";
            List<SqlParameter> parameters = new List<SqlParameter>();

            // 检查主键是否缺少
            foreach (var primaryKey in primaryKeyArray)
            {
                if (!rows.ContainsKey(primaryKey))
                {
                    throw new ArgumentException($"Missing primary key: {primaryKey}");
                }
            }

            foreach (var primaryKey in primaryKeyArray)
            {
                object primaryKeyValue = rows[primaryKey];
                if (primaryKeyValue == null)
                {
                    throw new ArgumentException($"Primary key value cannot be null: {primaryKey}");
                }

                whereClause += $"{primaryKey} = @{primaryKey} AND ";

                // 创建参数
                SqlParameter parameter = new SqlParameter($"@{primaryKey}", primaryKeyValue);
                parameters.Add(parameter);
            }

            whereClause = whereClause.TrimEnd(' ', 'A', 'N', 'D');

            // 生成删除操作的 SQL 语句
            string sql = $"DELETE FROM {tableName} WHERE {whereClause}";

            Dictionary<string, List<SqlParameter>> sqldic = new Dictionary<string, List<SqlParameter>>();
            sqldic.Add(sql, parameters);

            return sqldic;
        }

        /// <summary>
        /// 根据主键更新数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="primaryKeys">主键</param>
        /// <param name="row">行数据</param>
        /// <returns></returns>
        private Dictionary<string, List<SqlParameter>> GenerateUpdateTableSql(string tableName, string primaryKeys, dynamic row)
        {
            string rowJson = row.ToString();

            // 提取键值对
            Dictionary<string, object> rows = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowJson);

            string[] primaryKeyArray = primaryKeys.Split(',');

            // 拼接 SET 子句
            string setClause = "";

            List<SqlParameter> parameters = new List<SqlParameter>();

            foreach (var keyValuePair in rows)
            {
                string columnName = keyValuePair.Key;
                object columnValue = keyValuePair.Value;

                if (!primaryKeyArray.Contains(columnName))
                {
                    setClause += $"{columnName} = @{columnName}, ";

                    // 创建参数
                    SqlParameter parameter;
                    if (columnValue != null)
                    {
                        parameter = new SqlParameter($"@{columnName}", columnValue);
                    }
                    else
                    {
                        parameter = new SqlParameter($"@{columnName}", DBNull.Value);
                    }
                    parameters.Add(parameter);
                }
            }
            setClause = setClause.TrimEnd(',', ' ');

            // 拼接 WHERE 子句
            string whereClause = "";
            foreach (var primaryKey in primaryKeyArray)
            {
                if (rows.ContainsKey(primaryKey))
                {
                    object primaryKeyValue = rows[primaryKey];
                    if (primaryKeyValue == null)
                    {
                        throw new ArgumentException($"Primary key value cannot be null: {primaryKey}");
                    }

                    whereClause += $"{primaryKey} = @{primaryKey} AND ";

                    // 创建参数
                    SqlParameter parameter = new SqlParameter($"@{primaryKey}", primaryKeyValue);
                    parameters.Add(parameter);
                }
                else
                {
                    throw new ArgumentException($"Missing primary key: {primaryKey}");
                }
            }

            whereClause = whereClause.TrimEnd(' ', 'A', 'N', 'D');

            // 生成更新操作的 SQL 语句
            string sql = $"UPDATE {tableName} SET {setClause} WHERE {whereClause}";
            Dictionary<string, List<SqlParameter>> sqldic = new Dictionary<string, List<SqlParameter>>();
            sqldic.Add(sql, parameters);

            return sqldic;
        }

        /// <summary>
        /// 根据主键新增数据
        /// </summary>
        /// <param name="tableName">表名称</param>
        /// <param name="row">行数据</param>
        /// <returns></returns>
        private Dictionary<string, List<SqlParameter>> GenerateInsertTableSql(string tableName, dynamic row)
        {
            string rowJson = row.ToString();

            // 提取键值对
            Dictionary<string, object> rows = JsonConvert.DeserializeObject<Dictionary<string, object>>(rowJson);

            // 拼接列名和列值
            string columnNames = "";
            string columnValues = "";
            List<SqlParameter> parameters = new List<SqlParameter>();

            foreach (var keyValuePair in rows)
            {
                string columnName = keyValuePair.Key;
                string columnValue = Convert.ToString(keyValuePair.Value);
                columnNames += columnName + ", ";
                columnValues += $"@{columnName}, ";

                // 创建参数
                SqlParameter parameter;
                if (columnValue != null)
                {
                    parameter = new SqlParameter($"@{columnName}", columnValue);
                }
                else
                {
                    parameter = new SqlParameter($"@{columnName}", DBNull.Value);
                }
                parameters.Add(parameter);
            }

            columnNames = columnNames.TrimEnd(',', ' ');
            columnValues = columnValues.TrimEnd(',', ' ');

            // 生成新增操作的 SQL 语句
            string sql = $"INSERT INTO {tableName} ({columnNames}) VALUES ({columnValues})";
            Dictionary<string, List<SqlParameter>> sqldic = new Dictionary<string, List<SqlParameter>>();
            sqldic.Add(sql, parameters);

            return sqldic;
        }

    }
}
