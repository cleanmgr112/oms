using System;
using System.Collections.Generic;
using System.Text;

namespace OMS.Model.Grid
{
    public class SearchWHStockResultModel:SearchResultModel
    {
        //总库存
        public int AllStock { get; set; }
        //总锁定库存
        public int AllLockStock { get; set; }
        //总可用库存
        public int AllAvailableStock { get; set; }

        //当前页总库存
        public int SumStock { get; set; }
        //当前页总锁定库存
        public int SumLockStock { get; set; }
        //当前页总可用库存
        public int SumAvailableStock { get; set; }
    }
}
