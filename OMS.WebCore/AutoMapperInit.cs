using AutoMapper;
using OMS.Data.Domain;
using OMS.Data.Domain.Permissions;
using OMS.Model.Account;
using OMS.Model;
using OMS.Model.Menu;
using OMS.Model.B2B;
using OMS.Model.Role;
using System.Linq;
using OMS.Model.Permission;
using OMS.Model.Customer;
using OMS.Model.StockRemind;
using OMS.Services.StockRemid.StockRemindDto;

namespace OMS.WebCore
{
    public class AutoMapperInit
    {
        public static void Init()
        {
            Mapper.Initialize(config =>
            {
                #region entity To Model 
                config.CreateMap<User, UserModel>();
                config.CreateMap<User,UserViewModel>();
                config.CreateMap<Menu, MenuModel>();
                config.CreateMap<Product, ProductModel>();
                config.CreateMap<PlatformProduct, PlatformProductModel>();
                config.CreateMap<Role, RoleModel>();
                config.CreateMap<Permission, PermissionModel>();
                config.CreateMap<ApprovalProcess, ApprovalProcessModel>()
                .AfterMap((a, b) => b.ApprovalProcessDetailModel = a.ApprovalProcessDetail.Select(x => { return x.ToModel(); }).ToList());
                config.CreateMap<ApprovalProcessDetail, ApprovalProcessDetailModel>()
                .AfterMap((a, b) => b.UserName = a.User != null ? a.User.Name : "");
                config.CreateMap<SaleProductPrice, SaleProductPriceModel>();
                config.CreateMap<SaleProduct, SaleProductModel>()
               .AfterMap((a, b) => b.SaleProductPriceModel = a.SaleProductPrice?.Select(x => { return x.ToModel(); }).ToList());
                config.CreateMap<OrderProduct, OrderProductModel>()
                //获取订单商品表，商品名 ，code
                .AfterMap((a, b) => b.ProductName = a.SaleProduct?.Product?.Name)
                .AfterMap((a, b) => b.ProductCode = a.SaleProduct?.Product?.Code)
                .AfterMap((a, b) => b.SaleProductModel = a.SaleProduct.ToModel());
                config.CreateMap<Order, OrderModel>()
                //获取当前审核人
                .AfterMap((a, b) => b.Reviewer = a.OrderApproval != null ?
                (a.OrderApproval.Where(x => x.State == OrderApprovalState.Unaudited).OrderBy(x => x.Sort).FirstOrDefault() != null ?
                a.OrderApproval.Where(x => x.State == OrderApprovalState.Unaudited).OrderBy(x => x.Sort).FirstOrDefault().UserId : 0) : 0)
                //获取发票信息
                .AfterMap((a, b) => b.InvoiceInfoModel = a.InvoiceInfo?.ToModel());
                config.CreateMap<InvoiceInfo, InvoiceInfoModel>();


                config.CreateMap<Customers, CustomerModel>();
                #endregion
                #region Model To entity
                config.CreateMap<ProductModel, Product>();
                config.CreateMap<PlatformProductModel, PlatformProduct>();
                config.CreateMap<SaleProductModel, SaleProduct>();
                config.CreateMap<SaleProductPriceModel, SaleProductPrice>();
                config.CreateMap<CategoryModel, Category>();
                config.CreateMap<MenuModel, Menu>();
                config.CreateMap<RoleModel, Role>();
                config.CreateMap<OrderProductModel, OrderProduct>();
                config.CreateMap<PermissionModel, Permission>();

                //提醒
                config.CreateMap<RemindTemplateModel, RemindTemplateDto>().ReverseMap().ForMember(c=>c.TemplateCode,c=>c.Ignore()).ForMember(c=>c.TemplateId,c=>c.Ignore());
                config.CreateMap<RemindProduct, RemindProductdto>().ReverseMap();
                config.CreateMap<RemindTitleModel, RemindTitleDto>().ReverseMap();
                config.CreateMap<RemindTemplateModel, TitleSearchDto>().ReverseMap();
                #endregion

            });
        }
    }
}
