using OMS.Core;
using OMS.Data.Domain;
using OMS.Model;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Services
{
   public interface IWareHouseService
    {
        WareHouse GetById(int id);
        bool GetCountByName(string name, string code);
        IQueryable<WareHouse> GetCountByCode(string code);
        bool Add(WareHouse customer);
        List<WareHouse> GetAllWareHouseList();
        List<WareHouse> GetWareHouses();

        void UpdateWareHouse(WareHouse WareHouses);
        bool DelWareHouseById(int id);

        #region WareHouseArea
        /// <summary>
        /// 通过ID获取仓库区域
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        WareHouseArea GetWareHouseAreaById(int Id);
        /// <summary>
        /// 新增仓库区域
        /// </summary>
        /// <param name="wareHouseArea"></param>
        void AddWareHouseArea(WareHouseArea wareHouseArea);
        /// <summary>
        /// 更新仓库区域
        /// </summary>
        /// <param name="wareHouseArea"></param>
        void UpdateWareHouseArea(WareHouseArea wareHouseArea);

        WareHouseArea GetWareHouseAreaByName(string name);
        #endregion

        #region WareHouseAreaRanks
        /// <summary>
        /// 通过ID获取仓库区域优先级
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        WareHouseAreaRanks GetWHARanksById(int Id);

        /// <summary>
        /// 通过区域ID获取区域对应的仓库列表
        /// </summary>
        IList<WareHouseAreaRanks> GetWareHouseAreaRanksByWhaId(int Id); 
        /// <summary>
        /// 新增仓库区域优先级
        /// </summary>
        void AddWareHouseAreaRanks(WareHouseAreaRanks wareHouseAreaRanks);
        /// <summary>
        /// 更新仓库区域优先级
        /// </summary>
        /// <param name="wareHouseAreaRanks"></param>
        void UpdateWareHouseAreaRanks(WareHouseAreaRanks wareHouseAreaRanks);
        /// <summary>
        /// 获取仓库区域列表（分页获取）
        /// </summary>
        IPageList<WareHouseAreaViewModel> GetWareHouseAreaViewModels(SearchWareHouseAreaContext searchWareHouseAreaContext);

        /// <summary>
        /// 获取仓库区域详情Model
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        WareHouseAreaDetailModel GetWareHouseAreaDetailModelById(int id);
        /// <summary>
        /// 通过仓库名获取仓库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        WareHouse GetWareHouseByName(string name);
        #endregion
    }
}
