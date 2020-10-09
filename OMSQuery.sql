-------------------------------
-------[User] 用户表-----------
--------creator zxw-----------
-------------------------------
create table [User]
(
	Id int primary key identity(1,1) not null, 
	UserName nvarchar(32) not null, 
	UserPwd nvarchar(50) not null ,
	[Name] nvarchar(15) null,
	Email nvarchar(50) null,
	PhoneNumber nvarchar(15) null,
	Salt nvarchar(32) not null,
	LastLoginTime datetime2 null,
	LastLoginIp nvarchar(40) null,
	[State] char(1) default('1') not null, --'0' 未激活;'1'正常;'2'禁用
	
	[Isvalid] [bit] NULL,
	[ModifiedBy] [int] NULL,	
	[ModifiedTime] [datetime] NOT NULL,
	[CreatedTime] [datetime] NOT NULL,
	[CreatedBy] [int] NULL
)

----------------------------------
---------[Dictionary]  数据字典表
---------creator whf-------
---------------------------------
CREATE TABLE Dictionary
(
   Id              INT             NOT NULL       PRIMARY KEY     IDENTITY(1,1),
   [Type]          INT             NOT NULL ,
   Value           NVARCHAR(200)   NOT NULL,
   
   [Isvalid] [bit] NULL,
   [ModifiedBy] [int] NULL,	
   [ModifiedTime] [datetime] NOT NULL,
   [CreatedTime] [datetime] NOT NULL,
   [CreatedBy] [int] NULL      
)
GO
----------------------------------
---------[Product]  商品表
---------creator whf-------
---------------------------------
CREATE TABLE Product
(
  Id                 INT                NOT NULL        PRIMARY KEY     IDENTITY(1,1),
  Name               NVARCHAR(200)      NOT NULL,
  NameEn             NVARCHAR(200)      NULL,
  [Type]             INT                NOT NULL DEFAULT(0),
  Cover              NVARCHAR(200)      NULL,
  Country            INT                NULL DEFAULT(0),
  Area               INT                NULL DEFAULT(0),
  Grapes             NVARCHAR(100)      NULL,
  Capacity           INT                NULL DEFAULT(0),
  [Year]             NVARCHAR(100)      NULL,
  Packing            INT                NULL DEFAULT(0),
  CategoryId         INT                NULL,
  Code               NVARCHAR(100)      NOT NULL,

   [Isvalid] [bit] NULL,
   [ModifiedBy] [int] NULL,	
   [ModifiedTime] [datetime] NOT NULL,
   [CreatedTime] [datetime] NOT NULL,
   [CreatedBy] [int] NULL      
)
GO



--2018-7-25
alter table Product alter column [Type] int null



--2018-12-4
USE OMS

ALTER TABLE [Order] ADD [InvoiceMode] SMALLINT DEFAULT(0)
ALTER TABLE [OrderProduct] ADD [Type] INT NULL

--2019-11-14-- K3数据表
CREATE TABLE [dbo].[K3BaseData](
	[Id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[Type] [int] NOT NULL,
	[No] [int] NOT NULL,
	[FNumber] [nvarchar](100) NOT NULL,
	[FName] [nvarchar](100) NOT NULL,
	[Isvalid] [bit] NULL,
	[ModifiedBy] [int] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedTime] [datetime] NOT NULL,
	[CreatedTime] [datetime] NOT NULL)

--2019-11-14-- K3订单关联表
CREATE TABLE [dbo].[K3BillNoRelated](
	[Id] [int] PRIMARY KEY IDENTITY(1,1) NOT NULL,
	[K3BillNo] [nvarchar](30) NOT NULL,
	[OMSSeriNo] [nvarchar](20) NOT NULL,
	[Message] [nvarchar](300) NOT NULL,
	[Isvalid] [bit] NULL,
	[ModifiedBy] [int] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedTime] [datetime] NOT NULL,
	[CreatedTime] [datetime] NOT NULL)
--2019-11-14-- K3客户表
CREATE TABLE [dbo].[K3Customers](
	[Id] [int]  PRIMARY KEY  IDENTITY(1,1) NOT NULL,
	[Type] [int] NOT NULL,
	[Key] [nvarchar](50) NOT NULL,
	[Value] [nvarchar](50) NOT NULL,
	[Isvalid] [bit] NULL,
	[ModifiedBy] [int] NULL,
	[CreatedBy] [int] NULL,
	[ModifiedTime] [datetime] NOT NULL,
	[CreatedTime] [datetime] NOT NULL)


-- ======================================
-- Author: px
-- Date: 2020-02-20
-- ======================================
--PurchasingProducts表增加字段
ALTER TABLE [PurchasingProducts] ADD [FactReceivedNum] INT NULL


-- ======================================
-- Author: px
-- Date: 2020-04-15
-- ======================================
--Delivery表增加字段
ALTER TABLE [Delivery] ADD [ShopCode] NVARCHAR(50) NULL
-- ======================================
-- Author: px
-- Date: 2020-04-16
-- ======================================
--PurchasingProducts表修改字段
alter table PurchasingProducts alter column Price DECIMAL(18,2) not NULL

-- ======================================
-- Author: px
-- Date: 2020-04-20
-- ======================================
--Order表新增支付时间字段

ALTER TABLE [Order]  ADD [PayDate] DATETIME NULL 
-- ======================================
-- Author: px
-- Date: 2020-04-21
-- ======================================
--Order表新增附加类型字段

ALTER TABLE [Order]  ADD AppendType INT DEFAULT(0) NULL 
-- ======================================
-- Author: px
-- Date: 2020-04-23
-- ======================================
--[SaleProductWareHouseStock]表新增LockStock字段
ALTER TABLE [SaleProductWareHouseStock] ADD LockStock INT DEFAULT(0) NOT NULL
-- ======================================
-- Author: px
-- Date: 2020-04-29
-- ======================================
--[SaleProductWareHouseStock]表新增LockStock字段
ALTER TABLE [SaleProductLockedTrack] ADD OrderProductId  INT NOT NULL

-- ======================================
-- Author: px
-- Date: 2020-04-30
-- ======================================
--[SaleProductWareHouseStock]表新增LockStock字段
ALTER TABLE [Order] ADD IsLackStock  BIT DEFAULT(1) NOT NULL

--添加快递方式同步字段 px 20200525
ALTER TABLE [Delivery] ADD IsSynchronized  BIT DEFAULT(0) NOT NULL
--添加供应商同步字段 px  20200525
ALTER TABLE [Supplier] ADD IsSynchronized  BIT DEFAULT(0) NOT NULL

--添加客户同步字段 px  20200525
ALTER TABLE [Customer] ADD IsSynchronized  BIT DEFAULT(0) NOT NULL

--添加订单商品原始ID字段 px 20200603
ALTER TABLE [OrderProduct] ADD OrginId  INT DEFAULT(0) NULL

--添加采购单的OriginalOrderId px 20200717
ALTER TABLE [Purchasing] ADD OriginalOrderId INT DEFAULT(0) NULL

--添加采购单商品的OrginId px 20200717
ALTER TABLE [PurchasingProducts] ADD OrginId  INT DEFAULT(0) NULL

-- 修改发票邮箱字段为可空类型 px 20200806
ALTER table dbo.InvoiceInfo alter column CustomerEmail VARCHAR(100)  NULL

--新增发票信息里面的银行代码
ALTER TABLE dbo.InvoiceInfo ADD BankCode VARCHAR(50) NULL


--添加仓库类型字段WareHouseType
--px 20200813
ALTER TABLE dbo.WareHouse ADD WareHouseType SMALLINT NOT NULL DEFAULT(0) 
