using Microsoft.EntityFrameworkCore;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Interface;
using OMS.Model;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OMS.Services
{
    public class WareHouseService : ServiceBase, IWareHouseService
    {
        #region ctor
        public WareHouseService(IDbAccessor omsAccessor, IWorkContext workContext)
            : base(omsAccessor, workContext)
        {

        }
        #endregion
        public WareHouse GetById(int id)
        {
            return _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }
        public bool GetCountByName(string name, string code)
        {
            IQueryable<WareHouse> count = _omsAccessor.Get<WareHouse>().Where(x => x.Name.Equals(name) || x.Code.Equals(code));
            if (count.Count() > 0)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public IQueryable<WareHouse> GetCountByCode(string code)
        {
            IQueryable<WareHouse> result = _omsAccessor.Get<WareHouse>().Where(x => x.Code.Trim() == code.Trim() && x.Isvalid);
            return result;
        }

        public bool Add(WareHouse wareHouse)
        {
            if (wareHouse == null)
                throw new ArgumentException("WareHouse");
            wareHouse.Isvalid = true;
            wareHouse.CreatedBy = _workContext.CurrentUser.Id;
            wareHouse.Code = wareHouse.Code.Trim();
            wareHouse.Name = wareHouse.Name.Trim();
            wareHouse.ModifiedTime = DateTime.Now;
            wareHouse.CreatedTime = DateTime.Now;
            _omsAccessor.Insert(wareHouse);
            _omsAccessor.SaveChanges();
            return true;
        }
        public List<WareHouse> GetAllWareHouseList()
        {
            return _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.WareHouseType == WareHouseTypeEnum.Normal).ToList();
        }
        public List<WareHouse> GetWareHouses() {
            return _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid).ToList();
        }
        public void UpdateWareHouse(WareHouse wareHouses)
        {
            if (wareHouses == null)
                throw new ArgumentException("wareHouses");
            var data = _omsAccessor.Get<WareHouse>().Where(r => r.Id == wareHouses.Id).FirstOrDefault();
            data.Code = wareHouses.Code.Trim();
            data.Name = wareHouses.Name.Trim();
            data.IsSyncStock = wareHouses.IsSyncStock;
            data.WareHouseType = wareHouses.WareHouseType;
            wareHouses.ModifiedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Update(data);
            _omsAccessor.SaveChanges();
        }
        public bool DelWareHouseById(int id)
        {
            var delData = _omsAccessor.Get<WareHouse>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
            if (delData == null)
                throw new ArgumentException("WareHouse");
            else
                _omsAccessor.DeleteById<WareHouse>(id);
            _omsAccessor.SaveChanges();
            return true;
        }

        #region WareHouseArea
        /// <summary>
        /// 通过ID获取仓库区域
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WareHouseArea GetWareHouseAreaById(int Id)
        {
            return _omsAccessor.Get<WareHouseArea>().Where(x => x.Isvalid && x.Id == Id).FirstOrDefault();
        }
        /// <summary>
        /// 新增仓库区域
        /// </summary>
        /// <param name="wareHouseArea"></param>
        public void AddWareHouseArea(WareHouseArea wareHouseArea)
        {
            if (wareHouseArea == null)
                throw new ArgumentException("WareHouseArea");
            else
                wareHouseArea.CreatedBy = _workContext.CurrentUser.Id;

            wareHouseArea.CreatedTime = DateTime.Now;
            _omsAccessor.Insert(wareHouseArea);
            _omsAccessor.SaveChanges();


        }
        /// <summary>
        /// 更新仓库区域
        /// </summary>
        /// <param name="wareHouseArea"></param>
        public void UpdateWareHouseArea(WareHouseArea wareHouseArea)
        {
            if (wareHouseArea == null)
                throw new ArgumentException("WareHouseArea");
            else
                wareHouseArea.ModifiedBy = _workContext.CurrentUser.Id;
            wareHouseArea.ModifiedTime = DateTime.Now;
            _omsAccessor.Update(wareHouseArea);
            _omsAccessor.SaveChanges();
        }

        public WareHouseArea GetWareHouseAreaByName(string name)
        {
            return _omsAccessor.Get<WareHouseArea>().Where(r => r.Isvalid && r.AreaName.Equals(name)).FirstOrDefault();
        }
        #endregion

        #region WareHouseAreaRanks
        /// <summary>
        /// 通过ID获取仓库区域优先级
        /// </summary>
        /// <param name="Id"></param>
        /// <returns></returns>
        public WareHouseAreaRanks GetWHARanksById(int Id) {
            return _omsAccessor.Get<WareHouseAreaRanks>().Where(x => x.Isvalid && x.Id == Id).FirstOrDefault();
        }

        /// <summary>
        /// 通过区域ID获取区域对应的仓库列表
        /// </summary>
        public IList<WareHouseAreaRanks> GetWareHouseAreaRanksByWhaId(int Id) {
            return _omsAccessor.Get<WareHouseAreaRanks>().Where(x=>x.Isvalid && x.WhAId ==Id).OrderByDescending(x=>x.Rank).ToList();
        }
        /// <summary>
        /// 新增仓库区域优先级
        /// </summary>
        public void AddWareHouseAreaRanks(WareHouseAreaRanks wareHouseAreaRanks) {
            if (wareHouseAreaRanks == null)
                throw new ArgumentException("WareHouseAreaRanks");
            else
                wareHouseAreaRanks.CreatedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Insert(wareHouseAreaRanks);
            _omsAccessor.SaveChanges();
        }
        /// <summary>
        /// 更新仓库区域优先级
        /// </summary>
        /// <param name="wareHouseAreaRanks"></param>
        public void UpdateWareHouseAreaRanks(WareHouseAreaRanks wareHouseAreaRanks) {
            if (wareHouseAreaRanks == null)
                throw new ArgumentException("WareHouseAreaRanks");
            else
                wareHouseAreaRanks.ModifiedBy = _workContext.CurrentUser.Id;
            _omsAccessor.Update(wareHouseAreaRanks);
            _omsAccessor.SaveChanges();
        }

        /// <summary>
        /// 获取仓库区域列表（分页获取）
        /// </summary>
        /// <param name="searchWareHouseAreaContext"></param>
        /// <returns></returns>
        public IPageList<WareHouseAreaViewModel> GetWareHouseAreaViewModels(SearchWareHouseAreaContext searchWareHouseAreaContext)
        {

            var wareHouseAreaList = new List<WareHouseArea>();
            wareHouseAreaList = _omsAccessor.Get<WareHouseArea>().Where(x => x.Isvalid).ToList();
            if (!string.IsNullOrEmpty(searchWareHouseAreaContext.AreaName))
            {
                wareHouseAreaList = wareHouseAreaList.Where(x => x.AreaName.Contains(searchWareHouseAreaContext.AreaName)).ToList();
            }
            if (!string.IsNullOrEmpty(searchWareHouseAreaContext.AreaCode))
            {
                wareHouseAreaList = wareHouseAreaList.Where(x => x.AreaCode.Contains(searchWareHouseAreaContext.AreaCode)).ToList();
            }

            var wareHouseAreaViewModels = new List<WareHouseAreaViewModel>();
            foreach (var itemWareHouseArea in wareHouseAreaList)
            {
                var wareHouseAreaViewModel = new WareHouseAreaViewModel
                {
                    Id = itemWareHouseArea.Id,
                    AreaName = itemWareHouseArea.AreaName,
                    AreaCode = itemWareHouseArea.AreaCode,
                    MainWareHouse=(itemWareHouseArea.WhId.HasValue==false || itemWareHouseArea.WhId==0)?"":_omsAccessor.GetById<WareHouse>(itemWareHouseArea.WhId).Name
                };
                //var  mainWareHouseAreaRank =GetWareHouseAreaRanksByWhaId(itemWareHouseArea.Id).FirstOrDefault();
                //if (mainWareHouseAreaRank != null) {
                //    wareHouseAreaViewModel.MainWareHouse = GetById(itemWareHouseArea.Id) == null ? "" : GetById(itemWareHouseArea.Id).Name;   
                //}

                wareHouseAreaViewModels.Add(wareHouseAreaViewModel);
            }

            return new PageList<WareHouseAreaViewModel>(wareHouseAreaViewModels,searchWareHouseAreaContext.PageIndex,searchWareHouseAreaContext.PageSize);
        }

        public WareHouseAreaDetailModel GetWareHouseAreaDetailModelById(int id)
        {
            var result = from wa in _omsAccessor.Get<WareHouseArea>()
                         where wa.Id == id && wa.Isvalid
                         select new WareHouseAreaDetailModel
                         {
                             Id = wa.Id,
                             AreaName = wa.AreaName,
                             AreaCode = wa.AreaCode,
                             WareHouseRankModels = (from wr in _omsAccessor.Get<WareHouseAreaRanks>()
                                                    where wr.Isvalid && wr.WhAId == wa.Id
                                                    select new WareHouseRankModel
                                                    {
                                                        Id = wr.Id,
                                                        WareHouseAreaId = wr.WhAId,
                                                        WareHouseId = wr.WhId,
                                                        Rank = wr.Rank
                                                    }).ToList()
                         };
            return result.FirstOrDefault();
        }

        /// <summary>
        /// 通过仓库名获取仓库
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public WareHouse GetWareHouseByName(string name)
        {
            return _omsAccessor.Get<WareHouse>().Where(r => r.Isvalid && r.Name.Contains(name)).FirstOrDefault();
        }
        #endregion
    }
}
