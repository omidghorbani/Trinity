namespace OfficeSoft.Data.Crud
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Data;
    using System.Data.Common;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Reflection;

    public class SqlServerDataManager<T> : BaseDataManager<SqlModelCommand<T>>, IModelCommand<T>
        where T : class
    {
        public ModelConfiguration<T> ModelConfiguration { get; set; }
        public SqlServerDataManager(string connectionString)
            : this(connectionString, "System.Data.SqlClient")
        {
        }

        public SqlServerDataManager(string connectionString, string providerName)
            : base(connectionString, providerName)
        {
            if (BaseDataContext.TableMaps == null)
            {
                this.TableMaps = new Dictionary<string, TableMap>();
                BaseDataContext.TableMaps = this.TableMaps;
            }
            else
            {
                this.TableMaps = BaseDataContext.TableMaps;
            }
            TableMapFromDatabase = true;
        }

        public IDataCommand<T> Track(T model)
        {
            var dataCommand = this.Update();
            dataCommand.Model = model;
            dataCommand.Track = true;
            var errorInfo = model as IDataErrorInfo;
            if (errorInfo != null)
            {
                dataCommand.OnSetValidation(new ModelCommandValidationEventArgs<T> { ModelCommand = dataCommand });
            }
            var item = model as INotifyPropertyChanged;
            if (item != null)
            {
                item.PropertyChanged += (e, r) =>
                {
                    var property = item.GetType().GetProperty(r.PropertyName);
                    dataCommand.AddPropertyChange(property);
                };
            }
            return dataCommand;
        }
        public IDataCommand<T> Insert(T model)
        {
            var dataCommand = this.Insert();
            dataCommand.Model = model;
            var errorInfo = model;
            if (errorInfo != null)
            {
                dataCommand.OnSetValidation(new ModelCommandValidationEventArgs<T> { ModelCommand = dataCommand });
            }
            return dataCommand;
        }
        public IDataCommand<T> Update(T model)
        {
            var dataCommand = this.Update();
            dataCommand.Model = model;
            var errorInfo = model;
            if (errorInfo != null)
            {
                dataCommand.OnSetValidation(new ModelCommandValidationEventArgs<T> { ModelCommand = dataCommand });
            }
            return dataCommand;
        }

        protected override ICommandResult ExecuteDeleteCommand(SqlModelCommand<T> dataCommand, IDbCommand command)
        {
            var result = new ModelCommandResult<T>();
            if (TableMapFromDatabase)
                dataCommand.GetTableMap();

            //TODO change the where has to be set to parameters before execute
            //if (string.IsNullOrEmpty(dataCommand.WhereText) == false)
            //    if (dataCommand.WhereText.Contains("@"))
            //        dataCommand.BuildKeys();

            dataCommand.BuildSqlParameters(command);
            dataCommand.BuildSqlCommand();
            command.CommandText = dataCommand.SqlCommandText;



            if (command.CommandText.ToUpper().Contains("WHERE"))
            {
                int records = command.ExecuteNonQuery();
                result.RecordsAffected = records;
                result.AddMessage(string.Format("{0} executed with {1} rows affected", dataCommand.SqlCommandText, records));
                dataCommand.ResetCommand();
            }
            else
            {
                result.AddError(ErrorType.Information, "No where in delete " + command.CommandText);
            }
            result.DataCommand = dataCommand;

            return result;
        }
        protected override ICommandResult ExecuteUpdateCommand(SqlModelCommand<T> dataCommand, IDbCommand command)
        {

            var result = new ModelCommandResult<T>();
            var item = dataCommand.Model as IModelBase;

            if (item != null)
            {

                if (item.Error == null)
                    if (item.HasErrors())
                    {
                        return result;
                    }
            }

            if (TableMapFromDatabase)
                dataCommand.GetTableMap();


            if (dataCommand.TableMap.TableType == "VIEW")
            {
                
                if (item.Configuration == null)
                {
                    result.AddMessage(string.Format("The command is a off type View and has no merge configuration"));
                    return result;
                }
                






            }
            else
            {
                if (dataCommand.Track == false)
                {
                    dataCommand.AddChanges();
                }
                dataCommand.BuildKeys();
                foreach (var change in dataCommand.Changes)
                {
                    if (dataCommand.TableMap != null)
                    {
                        var column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.PropertyName == change);

                        if (column == null)
                            column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.ColumnName == change);


                        if (column != null)
                        {
                            if (column.IsIdentity == false)
                            {
                                dataCommand.Value(column.ColumnName, dataCommand.GetValue(change));
                            }
                        }
                    }
                    else
                    {
                        dataCommand.Value(change, dataCommand.GetValue(change));
                    }
                }
                if (dataCommand.Columns.Count > 0 && dataCommand.Changes.Count > 0)
                {
                    dataCommand.BuildSqlParameters(command);
                    dataCommand.BuildSqlCommand();
                    command.CommandText = dataCommand.SqlCommandText;
                    if (command.CommandText.Contains("WHERE"))
                    {
                        int resultIndex = command.ExecuteNonQuery();
                        result.RecordsAffected = resultIndex;
                        if (resultIndex == 0)
                        {
                            result.AddError(ErrorType.Information, "No rows affected");
                        }
                        else
                        {
                            dataCommand.ResetCommand();
                        }
                    }
                    else
                    {
                        result.AddError(ErrorType.Information, "No where in update");
                    }
                    result.AddMessage(string.Format("{0} executed with {1} rows affected", dataCommand.SqlCommandText, result.RecordsAffected));
                }
                result.DataCommand = dataCommand;
               
            }
            return result;

        }
        protected override ICommandResult ExecuteInsertCommand(SqlModelCommand<T> dataCommand, IDbCommand command)
        {
            var result = new ModelCommandResult<T>();
            var item = dataCommand.Model as IModelBase;

            if (item != null)
            {
                if (item.Errors != null)
                    if (item.HasErrors())
                    {
                        return result;
                    }
            }
            if (TableMapFromDatabase)
                dataCommand.GetTableMap();

            var select = string.Empty;
            var where = string.Empty;
            bool identity = false;
            var retunColums = new Dictionary<string, string>();
            dataCommand.AddChanges();
            foreach (var key in dataCommand.PrimaryKeys)
            {
                var column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.PropertyName == key);

                if (column == null)
                    column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.ColumnName == key);


                if (column != null)
                {
                    if (column.IsIdentity)
                    {
                        select += string.Format(" {0},", column.ColumnName);
                        where = string.Format("{0} = @@IDENTITY", column.ColumnName);
                        identity = true;
                        retunColums.Add(column.ColumnName, column.PropertyName);
                        if (dataCommand.Changes.Contains(column.PropertyName))
                        {
                            dataCommand.Changes.Remove(column.PropertyName);
                        }
                    }
                }
            }

            var guidIdColumns =
                dataCommand.TableMap.ColumnMaps.Where(map => !string.IsNullOrEmpty(map.Default))
                    .Where(
                        map =>
                            map.Default.ToUpper().Contains("NEWSEQUENTIALID")
                            || map.Default.ToUpper().ToUpper().Contains("NEWID"))
                    .ToList();
            foreach (var guidIdColumn in guidIdColumns)
            {
                select += string.Format(" {0},", guidIdColumn.ColumnName);
                retunColums.Add(guidIdColumn.ColumnName, guidIdColumn.PropertyName);
            }

            if (identity)
            {
                select = select.Remove(select.Length - 1);
                dataCommand.WhereText = string.Format(
                    " SELECT {0} FROM [{1}] WHERE {2}",
                    select,
                    dataCommand.TabelName,
                    where);
            }


            foreach (var change in dataCommand.Changes)
            {
                var column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.PropertyName == change);
                if (column != null)
                {
                    dataCommand.Value(column.ColumnName, dataCommand.GetValue(change));
                }
            }

            dataCommand.BuildSqlParameters(command);
            dataCommand.BuildSqlCommand();
            command.CommandText = dataCommand.SqlCommandText;

            using (var dataReader = command.ExecuteReader())
            {
                result.RecordsAffected = dataReader.RecordsAffected;
                if (dataReader.RecordsAffected == 0)
                {
                    result.AddError(ErrorType.Information, "No rows affected");
                }
                else
                {

                    dataCommand.WhereText = string.Empty;
                    dataCommand.CommandType = DataCommandType.Update;


                }
                while (dataReader.Read())
                {

                    for (int i = 0; i < 1; ++i)
                    {
                        var name = dataReader.GetName(i);
                        var property = retunColums[name];
                        dataCommand.SetValue(property, dataReader.GetValue(i));
                    }
                }

            }
            result.AddMessage(string.Format("{0} executed with {1} rows affected", dataCommand.SqlCommandText, result.RecordsAffected));
            dataCommand.ResetCommand();
            result.DataCommand = dataCommand;
            return result;
        }
        protected override ICommandResult ExecuteSelectCommand(SqlModelCommand<T> dataCommand, IDbCommand command)
        {
            var result = new ModelCommandResult<T>();
            if (TableMapFromDatabase)
                dataCommand.GetTableMap();

            dataCommand.BuildSqlCommand();
            dataCommand.BuildSqlParameters(command);
            command.CommandText = dataCommand.SqlCommandText;
            var items = new List<T>();
            var pd = CommandResult.ForType(typeof(T));
            try
            {
                int rowsIndex = 0;
                using (var r = command.ExecuteReader())
                {

                    this.OnExecutedCommand(command);
                    Type objectType = typeof(T);
                    while (r.Read())
                    {
                        bool userDataManager = false;
                        var newObject = (T)Activator.CreateInstance(objectType);
                        var dataManager = newObject as IObjectDataManager;

                        if (dataManager != null)
                            userDataManager = true;

                        if (dataCommand.TabelName != objectType.Name)
                            userDataManager = false;
                        if (userDataManager)
                        {
                            dataManager.SetData(r);
                        }
                        else
                        {
                            int counter = r.FieldCount;
                            for (int i = 0; i < counter; i++)
                            {
                                var name = r.GetName(i);
                                try
                                {
                                    var fieldType = r.GetFieldType(i);
                                    var value = r.GetValue(i);
                                    if (dataCommand.TableMap != null)
                                    {
                                        var column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.ColumnName == name);

                                        if (column != null)
                                        {
                                            if (!string.IsNullOrEmpty(column.PropertyName))
                                            {
                                                var prop = objectType.GetProperty(column.PropertyName);
                                                if (prop != null)
                                                {
                                                    if (value != DBNull.Value)
                                                    {
                                                        prop.SetValue(newObject, value, null);
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                TrySetValue(dataCommand, newObject, objectType, name, value);
                                            }

                                        }
                                        else
                                        {
                                            TrySetValue(dataCommand, newObject, objectType, name, value);
                                        }
                                    }
                                    else
                                    {
                                        var prop = objectType.GetProperty(name);
                                        if (prop != null)
                                        {
                                            if (value != DBNull.Value)
                                            {
                                                prop.SetValue(newObject, value, null);
                                            }
                                        }
                                    }
                                }
                                catch (Exception ex)
                                {
                                    result.AddError(ErrorType.Error, string.Format("{1} {0} ", name, dataCommand.TabelName), ex);
                                }
                            }


                        }
                        items.Add(newObject);

                        rowsIndex++;
                    }
                }
                result.RecordsAffected = rowsIndex;
            }
            catch (Exception exception)
            {
                result.AddError(ErrorType.Error, dataCommand.TabelName + " " + typeof(T).FullName, exception);
            }

            result.Data = items;
            result.AddMessage(string.Format("{0} executed with {1} rows affected", dataCommand.SqlCommandText, result.RecordsAffected));
            dataCommand.OnCommandExecuted(new ModelCommandExecutedEventArgs<T> { Result = result });
            dataCommand.ResetCommand();
            result.DataCommand = dataCommand;
            return result;
        }



        private void TrySetValue(IDataCommand dataCommand, object newObject, Type objectType, string name, object value)
        {
            var prop = objectType.GetProperty(name);
            if (prop != null)
            {
                if (value != DBNull.Value)
                {
                    prop.SetValue(newObject, value, null);
                }
            }
            else
            {
                PropertyInfo[] properties = objectType.GetProperties();
                var index = properties.Count();
                for (int j = 0; j < index; j++)
                {
                    var props = properties[j];
                    if (props.CanWrite)
                    {
                        var propName = props.Name.ToLower();
                        var columnName = name.ToLower();
                        if (propName == columnName)
                        {

                            if (value != DBNull.Value)
                                props.SetValue(newObject, value, null);

                            var column = dataCommand.TableMap.ColumnMaps.FirstOrDefault(m => m.ColumnName == name);
                            if (column == null)
                            {
                                dataCommand.TableMap.ColumnMaps.Add(new SqlColumnMap()
                                {
                                    ColumnName = name,
                                    PropertyName = props.Name
                                });
                            }

                            else
                            {
                                column.PropertyName = props.Name;
                            }
                            break;
                        }
                    }
                }
            }
        }

        public IEnumerable<IDataCommand> GetCommands()
        {
            return Commands;
        }
    }
}