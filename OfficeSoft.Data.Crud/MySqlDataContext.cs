﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SqlServer.Server;
using MySql.Data.MySqlClient;

namespace OfficeSoft.Data.Crud
{
    public class MySqlDataContext
    {
        private readonly string _connectionsString;
        private readonly string _providerName;

        public MySqlDataContext(string connectionsString, string providerName)
        {
            _connectionsString = connectionsString;
            _providerName = providerName;
        }

        public void GetTableMaps()
        {

            //var manager = new BaseDataManager(_connectionsString, _providerName);
            //var sql = "SELECT * FROM INFORMATION_SCHEMA.TABLES where TABLE_TYPE = 'BASE TABLE'";

            //using (var conn = new MySqlConnection(manager.ConnectionString))
            //{
            //    conn.Open();


            //}


        }
    }
}
