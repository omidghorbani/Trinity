﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OfficeSoft.Data.Crud
{
    public class BaseDataContext
    {
        public static Dictionary<string, TableMap> TableMaps { get; set; }

        public static List<IDataManager> DataManagers { get; set; }

        public static void AddDataManger(IDataManager radDataManger)
        {
            DataManagers.Add(radDataManger);
        }



        public TableMap GetDataTabel(string name)
        {
            if (BaseDataContext.TableMaps.ContainsKey(name))
            {
                return BaseDataContext.TableMaps[name];
            }

            return null;
        }
    }
}
