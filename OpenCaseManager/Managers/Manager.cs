using System;
using System.Collections.Generic;
using System.Text;
using OpenCaseManager.Models;
using Newtonsoft.Json;
using Repository;
using System.Data;
using OpenCaseManager.Commons;

namespace OpenCaseManager.Managers
{
    public class Manager : IManager
    {
        private IDBManager _dbManager;
        public Manager(IDBManager dbManager)
        {
            _dbManager = dbManager;
        }

        /// <summary>
        /// Make SQL Command to select data
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public DataTable SelectData(DataModel dataModel)
        {
            try
            {
                var command = new StringBuilder();

                command.Append("SELECT ");

                // columns
                command.Append(GetColumnsToSelect(dataModel) + " FROM " + dataModel.Entity + "");

                // where clause
                command.Append(Where(dataModel));

                // order by clause

                command.Append(OrderBy(dataModel));
                var sqlCommand = Common.ReplaceKeyWithResponsible(command.ToString());
                var dataTable = _dbManager.SelectData(sqlCommand);
                return dataTable;
            }
            catch (Exception ex)
            {
                LogSQLModel(dataModel, ex, "SelectData");
                throw ex;
            }
        }

        /// <summary>
        /// Make sql command to update data
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        public DataTable UpdateData(DataModel dataModel)
        {
            try
            {
                var updateCommand = new StringBuilder();

                updateCommand.Append("UPDATE [" + dataModel.Entity + "]");

                updateCommand.Append(GetParametersUpdate(dataModel));

                // where clause
                updateCommand.Append(Where(dataModel));

                var sqlCommand = updateCommand.ToString();
                var dataTable = _dbManager.UpdateData(sqlCommand);
                return dataTable;
            }
            catch (Exception ex)
            {
                LogSQLModel(dataModel, ex, "UpdateData");
                throw ex;
            }
        }

        /// <summary>
        /// Make sql command to insert data
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable InsertData(DataModel dataModel)
        {
            try
            {
                var insertCommand = new StringBuilder();

                insertCommand.Append("INSERT INTO [" + dataModel.Entity + "]");
                insertCommand.Append(GetParametersInsert(dataModel));

                var sqlCommand = insertCommand.ToString();
                var dataTable = _dbManager.InsertData(sqlCommand);
                return dataTable;
            }
            catch (Exception ex)
            {
                try
                {
                    LogSQLModel(dataModel, ex, "InsertData");
                    throw ex;
                }
                catch (Exception e)
                {
                    // Log into file
                    throw e;
                }
            }
        }

        /// <summary>
        /// Make sql command to execute a stored procedure
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public DataTable ExecuteStoredProcedure(DataModel model)
        {
            try
            {
                var parameters = new Dictionary<string, dynamic>();

                foreach (var param in model.Parameters)
                {
                    dynamic value = string.Empty;
                    switch (param.ColumnType)
                    {
                        case "int":
                            value = int.Parse(param.ColumnValue);
                            break;
                        case "decimal":
                            value = float.Parse(param.ColumnValue);
                            break;
                        case "boolean":
                            value = bool.Parse(param.ColumnValue);
                            break;
                        case "datetime":
                            value = DateTime.Parse(param.ColumnValue);
                            break;
                        case "xml":
                        case "string":
                        default:
                            value = param.ColumnValue;
                            break;
                    }
                    parameters.Add(param.ColumnName, value);
                }

                var dataTable = _dbManager.ExecuteStoreProcedure(model.Entity, parameters);
                return dataTable;
            }
            catch (Exception ex)
            {
                LogSQLModel(model, ex, "ExecuteStoreProcedure");
                throw ex;
            }
        }

        /// <summary>
        /// Get where clause for command
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private string Where(DataModel dataModel)
        {
            var where = new StringBuilder();
            if (dataModel.Filters != null && dataModel.Filters.Count > 0)
            {
                where.Append(" WHERE ");
                foreach (var param in dataModel.Filters)
                {
                    where.Append(param.Column);

                    switch (param.Operator)
                    {
                        case "equal":
                            where.Append(" = ");
                            break;
                        case "not_equal":
                            where.Append(" <> ");
                            break;
                        case "greater_than":
                            where.Append(" > ");
                            break;
                        case "less_than":
                            where.Append(" < ");
                            break;
                        case "equal_greater_than":
                            where.Append(" >= ");
                            break;
                        case "equal_less_than":
                            where.Append(" <= ");
                            break;
                        case "like":
                            where.Append(" LIKE ");
                            break;
                        case "_null":
                            where.Append(" IS NULL ");
                            break;
                        case "_not_null":
                            where.Append(" IS NOT NULL ");
                            break;
                    }

                    if (param.Value != null)
                    {
                        dynamic value = string.Empty;
                        switch (param.ValueType)
                        {
                            case "int":
                                value = int.Parse(param.Value);
                                break;
                            case "decimal":
                                value = float.Parse(param.Value);
                                break;
                            case "boolean":
                                value = bool.Parse(param.Value) == true ? 1 : 0;
                                break;
                            case "datetime":
                                value = DateTime.Parse(param.Value);
                                value = value.ToString("yyyy-MM-dd HH:mm:ss.fffffff");
                                value = "\'" + value + "\'";
                                break;
                            case "string":
                            default:
                                value = "\'" + param.Value + "\'";
                                break;

                        }
                        where.Append("  " + value + " ");
                    }

                    if (dataModel.Filters.IndexOf(param) != (dataModel.Filters.Count - 1))
                    {
                        where.Append(" " + param.LogicalOperator + " ");
                    }
                }
            }
            return where.ToString();
        }

        /// <summary>
        /// Get column list for selection
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private string GetColumnsToSelect(DataModel dataModel)
        {
            var columnName = new StringBuilder();
            foreach (var param in dataModel.ResultSet)
            {
                columnName.Append(param);
                if (dataModel.ResultSet.IndexOf(param) != (dataModel.ResultSet.Count - 1))
                {
                    columnName.Append(",");
                }
            }
            return columnName.ToString();
        }

        /// <summary>
        /// Get orderby clause
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private string OrderBy(DataModel dataModel)
        {
            var orderBy = new StringBuilder();
            if (dataModel.Order != null && dataModel.Order.Count > 0)
            {
                orderBy.Append(" ORDER BY ");
                foreach (var param in dataModel.Order)
                {
                    orderBy.Append(param.Column);
                    if (param.Descending)
                    {
                        orderBy.Append(" DESC ");
                    }
                    if (dataModel.Order.IndexOf(param) != (dataModel.Order.Count - 1))
                    {
                        orderBy.Append(",");
                    }
                }
            }
            return orderBy.ToString();
        }

        /// <summary>
        /// Parametes and values for update command
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private string GetParametersUpdate(DataModel dataModel)
        {
            var values = new StringBuilder(" SET ");

            foreach (var param in dataModel.Parameters)
            {
                dynamic value = string.Empty;
                switch (param.ColumnType)
                {
                    case "int":
                        value = int.Parse(param.ColumnValue);
                        values.Append(param.ColumnName + " = " + value);
                        break;
                    case "decimal":
                        value = float.Parse(param.ColumnValue);
                        values.Append(param.ColumnName + " = " + value);
                        break;
                    case "boolean":
                        value = bool.Parse(param.ColumnValue);
                        values.Append(param.ColumnName + " = " + (value == true ? 1 : 0));
                        break;
                    case "datetime":
                        value = Common.GetSqlDateTimeFormat(DateTime.Parse(param.ColumnValue));
                        values.Append(param.ColumnName + " = " + "\'" + value + "\'");
                        break;
                    case "xml":
                    case "string":
                    default:
                        value = param.ColumnValue;
                        value = value.Replace("'", "''");
                        values.Append(param.ColumnName + " = " + "\'" + value + "\'");
                        break;

                }
                if (dataModel.Parameters.IndexOf(param) != (dataModel.Parameters.Count - 1))
                {
                    values.Append(",");
                }
            }
            return values.ToString();
        }

        /// <summary>
        /// Parametes and values for insert command
        /// </summary>
        /// <param name="dataModel"></param>
        /// <returns></returns>
        private string GetParametersInsert(DataModel dataModel)
        {
            var columnName = new StringBuilder(" ( ");
            var values = new StringBuilder(" VALUES (");

            foreach (var param in dataModel.Parameters)
            {
                dynamic value = string.Empty;
                switch (param.ColumnType)
                {
                    case "int":
                        value = int.Parse(param.ColumnValue);
                        values.Append(value);
                        break;
                    case "decimal":
                        value = float.Parse(param.ColumnValue);
                        values.Append(value);
                        break;
                    case "boolean":
                        value = bool.Parse(param.ColumnValue);
                        values.Append(value == true ? 1 : 0);
                        break;
                    case "datetime":
                        value = Common.GetSqlDateTimeFormat(DateTime.Parse(param.ColumnValue));
                        values.Append("\'" + value + "\'");
                        break;
                    case "xml":
                    case "string":
                    default:
                        value = param.ColumnValue;
                        value = value.Replace("'", "''");
                        values.Append("\'" + value + "\'");
                        break;

                }
                columnName.Append(param.ColumnName);
                if (dataModel.Parameters.IndexOf(param) != (dataModel.Parameters.Count - 1))
                {
                    values.Append(",");
                    columnName.Append(",");
                }
            }
            columnName.Append(")");
            values.Append(")");
            columnName.Append(values);
            return columnName.ToString();
        }

        /// <summary>
        /// Log sql model
        /// </summary>
        /// <param name="dataModel"></param>
        public void LogSQLModel(DataModel dataModel, Exception ex, string methodName)
        {
            try
            {
                var jsonModel = JsonConvert.SerializeObject(dataModel);
                var logDataModel = new DataModel()
                {
                    Type = "INSERT",
                    Entity = "Log",
                    Parameters = new List<Parameter>()
                    {
                       new Parameter(){ ColumnName = "Message", ColumnType ="string", ColumnValue = jsonModel},
                       new Parameter(){ ColumnName = "Logged", ColumnType ="datetime", ColumnValue = DateTime.Now.ToString()},
                       new Parameter(){ ColumnName = "Level", ColumnType ="string", ColumnValue = "Info"},
                       new Parameter(){ ColumnName = "Exception", ColumnType ="string", ColumnValue = methodName + " : " + ex.StackTrace}
                    }
                };
                InsertData(logDataModel);
            }
            catch (Exception e)
            {
                // Log into Log file
            }
        }
    }
}