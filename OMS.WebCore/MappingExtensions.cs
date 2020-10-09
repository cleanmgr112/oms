using AutoMapper;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Model.Account;
using OMS.Model.Customer;
using OMS.Model;
using OMS.Model.Menu;
using OMS.Model.B2B;
using OMS.Model.Role;
using OMS.Model.Permission;

namespace OMS.WebCore
{
    public static class MappingExtensions
    {
        public static UserModel ToModel(this User entity)
        {
            return Mapper.Map<User, UserModel>(entity);
        }
        public static UserViewModel ToViewModel(this User entity)
        {
            return Mapper.Map<User, UserViewModel>(entity);
        }
        public static CustomerModel ToModel(this Customers entity)
        {
            return Mapper.Map<Customers, CustomerModel>(entity);
        }

        public static MenuModel ToModel(this Menu entity)
        {
            return Mapper.Map<Menu, MenuModel>(entity);
        }
        public static Menu ToModel(this MenuModel entity)
        {
            return Mapper.Map<MenuModel, Menu>(entity);
        }
        public static ApprovalProcessModel ToModel(this ApprovalProcess entity)
        {
            return Mapper.Map<ApprovalProcess, ApprovalProcessModel>(entity);
        }
        public static ApprovalProcessDetailModel ToModel(this ApprovalProcessDetail entity)
        {
            return Mapper.Map<ApprovalProcessDetail, ApprovalProcessDetailModel>(entity);
        }
        public static SaleProductPriceModel ToModel(this SaleProductPrice entity)
        {
            return Mapper.Map<SaleProductPrice, SaleProductPriceModel>(entity);
        }
        public static SaleProductModel ToModel(this SaleProduct entity)
        {
            return Mapper.Map<SaleProduct, SaleProductModel>(entity);
        }
        public static OrderProductModel ToModel(this OrderProduct entity)
        {
            return Mapper.Map<OrderProduct, OrderProductModel>(entity);
        }
        public static OrderModel ToModel(this Order entity)
        {
            return Mapper.Map<Order, OrderModel>(entity);
        }
        public static InvoiceInfoModel ToModel(this InvoiceInfo entity)
        {
            return Mapper.Map<InvoiceInfo, InvoiceInfoModel>(entity);
        }
        public static Role ToModel(this RoleModel entity)
        {
            return Mapper.Map<RoleModel, Role>(entity);
        }
        public static RoleModel ToModel(this Role entity)
        {
            return Mapper.Map<Role, RoleModel>(entity);
        }
        public static Permission ToModel(this PermissionModel entity)
        {
            return Mapper.Map<PermissionModel, Permission>(entity);
        }
        public static PermissionModel ToModel(this Permission entity)
        {
            return Mapper.Map<Permission, PermissionModel>(entity);
        }
        public static ProductModel ToModel(this Product entity)
        {
            return Mapper.Map<Product, ProductModel>(entity);
        }
        public static Product ToEntity(this ProductModel model)
        {
            return Mapper.Map<ProductModel, Product>(model);
        }
        public static SaleProduct ToEntity(this SaleProductModel model)
        {
            return Mapper.Map<SaleProductModel, SaleProduct>(model);
        }
        public static PlatformProduct ToEntity(this PlatformProductModel model)
        {
            return Mapper.Map<PlatformProductModel, PlatformProduct>(model);
        }
        public static SaleProductPrice ToEntity(this SaleProductPriceModel model)
        {
            return Mapper.Map<SaleProductPriceModel, SaleProductPrice>(model);
        }
        public static Category ToEntity(this CategoryModel model)
        {
            return Mapper.Map<CategoryModel, Category>(model);
        }
        public static OrderProduct ToEntity(this OrderProductModel model)
        {
            return Mapper.Map<OrderProductModel, OrderProduct>(model);
        }

        public static ScheduleTask ToEntity(this ScheduleTaskModel model) {
            return Mapper.Map<ScheduleTaskModel,ScheduleTask>(model);
        }
    }
}
