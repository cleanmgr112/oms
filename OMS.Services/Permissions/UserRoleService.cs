using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OMS.Core;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Data.Interface;

namespace OMS.Services.Permissions
{
    public class UserRoleService : ServiceBase, IUserRoleService
    {

        public UserRoleService(IDbAccessor omsAccessor, IWorkContext workContext) : base(omsAccessor, workContext)
        {
        }

        public void AddUserRoleRange(List<UserRole> list)
        {
            _omsAccessor.InsertRange<UserRole>(list);
            _omsAccessor.SaveChanges();
        }

        public void DeleteUserRoleRange(List<UserRole> list)
        {
            _omsAccessor.DeleteRange<UserRole>(list);
            _omsAccessor.SaveChanges();
        }

        public UserRole GetUserRole(int id)
        {
            return _omsAccessor.Get<UserRole>().Where(x => x.Isvalid && x.Id == id).FirstOrDefault();
        }
        public UserRole GetUserRole(int userId, int roleId)
        {
            return _omsAccessor.Get<UserRole>().Where(x => x.Isvalid && x.UserId == userId && x.RoleId == roleId).FirstOrDefault(); 

        }
        public IQueryable<UserRole> GetUserRolesByUserId(int userId)
        {
            return _omsAccessor.Get<UserRole>().Where(x => x.Isvalid && x.UserId == userId);
        }
    }
}
