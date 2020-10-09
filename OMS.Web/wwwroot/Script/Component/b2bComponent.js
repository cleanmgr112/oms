var productsComponent = function () {
   
    return {
        init: function () {
           // showProducts1();
        }
    }
}();
jQuery(document).ready(function () {
    // productsComponent.init();
    //showProducts1();
    //showOrderProducts();
});

/*增加、修改订单*/
var submit1 = function (ismotify) {
    var isMotify = ismotify || false;
    var billNo = $("#billNo").val();//单据编号
    var customerId = parseInt($("#customer").select2("data")[0].id);//客户
    var wareHousesId = parseInt($("#wareHouses option:selected").val());//仓库
    var deliveryId = parseInt($("#delivery").select2("data")[0].id);//物流方式
    var priceTypeId = parseInt($("#priceType").select2("data")[0].id);//价格类型
    var contact = $("#contact").val();//联系人
    var mobile = $("#mobile").val().trim();//电话
    var address = $("#address").val();//地址
    var descr = $("#descr").val();//描述
    var username = $("#username").val().trim();//描述
    //var invoiceTypeId = parseInt($("#invoiceType").select2("data")[0].id);//发票类型
    //var apId = parseInt($("#ap").select2("data")[0].id);
    var salesManId = parseInt($("#salesMan").select2("data")[0].id);//业务员
    var ssq = "";
    $('#Shengshiqu').children('select').each(function () {
        var selectValue = $(this).val();
        if (selectValue != null && selectValue != "" && selectValue != "undefined") {
            ssq += selectValue + " ";
        }
        //else {
        //    alertWarn("省市区必填");
        //    return false;
        //}
    });
    //if (address == null || address == "") {
    //    alertWarn("详细地址不能为空！");
    //    return false;
    //};
    address = ssq + address;

    var OrderModel = {
        SerialNumber: billNo,
        CustomerId: customerId,
        WarehouseId: wareHousesId,
        PriceTypeId: priceTypeId,
        CustomerName: contact,
        CustomerPhone: mobile,
        AddressDetail: address,
        CustomerMark: descr,
        UserName: username,
        //ApprovalProcessId: apId,
        DeliveryTypeId: deliveryId,
        //InvoiceType: invoiceTypeId,
        SalesManId: salesManId
    }
    if (!(customerId > 0)) {
        alertWarn("请选择客户！");
        return false;
    }
    if (!(wareHousesId > 0)) {
        alertWarn("请选择仓库！");
        return false;
    }
    if (!(deliveryId > 0)) {
        alertWarn("请选择物流方式！");
        return false;
    }
    if (!(priceTypeId > 0)) {
        alertWarn("请选择价格类型！");
        return false;
    }
    //if (contact == "") {
    //    alertWarn("联系人不能为空！");
    //    return false;
    //}
    //if (mobile == "") {
    //    alertWarn("电话不能为空！");
    //    return false;
    //}
    var reg = /^1[0-9]{10}$/;
    var reg2 = /^(0[0-9]{2,3}\-)([2-9][0-9]{6,7})+(\-[0-9]{1,4})?$/;
    if (mobile !== "" && !reg.test(mobile) && !reg2.test(mobile)) {
        alertWarn("电话有误请核实！");
        return false;
    }
    //if (address == "") {
    //    alertWarn("地址不能为空！");
    //    return false;
    //}
    //if (!(apId > 0)) {
    //    alertWarn("请选择审核流程");
    //    return false;
    //}
    //if (!(invoiceTypeId >= 0)) {
    //    alertWarn("请选择发票类型");
    //    return false;
    //}
    //if (!(salesManId > 0)) {
    //    alertWarn("业务员不能为空！");
    //    return false;
    //}
    //OrderModel.InvoiceType = invoiceTypeId;//发票类型0为不需要发票，这个插件不能使用为0的值，或者是读取的时候没办法判断value是默认还是0
    if (isMotify) {
        OrderModel.Id = parseInt($("#orderId").val());
        $oms.ajax({
            url: "/B2BOrder/UpdateB2BOrder",
            data: { OrderModel: OrderModel },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("修改成功");
                    edit("#submit_id");
                    window.location.reload();
                } else {
                    alertError(data.msg);
                }
            }
        });
    }
    else {
        $oms.ajax({
            url: "/B2BOrder/AddB2BOrder",
            data: { OrderModel: OrderModel },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("创建B2B订单成功！");
                    setTimeout(function () { window.location.reload()}, 1000);
                    linkToDetail(data.data.serialNumber, data.data.id);
                } else {
                    alertError(data.msg);
                }
            }
        });
    }
};
//跳转到订单详情
var linkToDetail = function (orderSerialNumber, id) {
    //是否有打开tab
    for (var i = 0; i < parent.$('.x-iframe').length; i++) {
        if (parent.$('.x-iframe').eq(i).attr('tab-id') == orderSerialNumber) {
            parent.$tap.tabChange(orderSerialNumber);
            event.stopPropagation();
            return;
        }
    };
    var url = "/B2BOrder/AddSalesBill?orderId=" + id;
    parent.$tap.tabAdd(orderSerialNumber, url, orderSerialNumber); // 新开一个tap页面
    parent.$tap.tabChange(orderSerialNumber);
    event.stopPropagation();
};

/*添加商品（模态框显示商品）*/
var selectProducts = function () {
    var searchKey = $("#searchKey").val();
    showProducts1(searchKey);
}
var showProducts1 = function (searchKey) {
    var priceType = $("#priceType").val();
    $oms.paginator({
        pageLimitId: "pageLimit",
        url: "/product/GetProducts",
        data: { search: searchKey, pageIndex: 1, pageSize: 10, priceType: priceType},
        success: function (data) {
            var html = "";
            if (data.length > 0) {
                for (var i = 0; i < data.length; i++) {
                    html += "<tr>" +
                        "<td>" + data[i].code + "</td >" +
                        "<td>" + data[i].name + "</td>" +
                        "<td>" + (data[i].nameEn == null ? "" : data[i].nameEn) + "</td>" +
                        "<td>" +
                        "<button class='btn btn-circle blue' onclick='getProductInfo(" + data[i].saleProductId +")'> 添加 </button>"+
                        //"<a class=\"delete\" href=\"javascript:;\" onclick=\"getProductInfo(" + data[i].saleProductId + ")\"> 添加 </a>" +
                        "</td>" +
                        "</tr >";
                }
            }
            $("#tbody").html(html);
        }
    });
}

/*显示订单商品*/
var showOrderProducts = function (Id) {
    var search = $("#search").val();
    var orderId = Id || $("#orderId").val();
    var isUpload = $("#isUpload").val();
    $oms.paginator({
        pageLimitId: "pageLimit2",
        url: "/B2BOrder/GetOrderProducts",
        data: { search: search, orderId: orderId },//default 1,10
        success: function (data) {
            if (data.length > 0) {
                var html = "";
                for (var i = 0; i < data.length; i++) {
                    var optstr = "<div class=\"btn-group\">" +
                        "<button class=\"btn btn-xs green dropdown-toggle\" type=\"button\" data-toggle=\"dropdown\" aria-expanded=\"false\"> Actions" +
                        "<i class=\"fa fa-angle-down\"></i>" +
                        "</button>" +
                        "<ul class=\"dropdown-menu pull-left\" role=\"menu\">" +
                        "<li>" +
                        "<a  onclick=\"motifyOrderProduct('" + data[i].id + "')\">" +
                        "<i class=\"icon-pencil\"></i> 修改 </a>" +
                        "</li>" +
                        "<li>" +
                        "<a href=\"javascript:;\" onclick=\"deleteOrderProduct('" + data[i].id + "','" + data[i].productName + "'," + data[i].quantity + ")\">" +
                        "<i class=\"fa fa-remove\"></i> 删除 </a>" +
                        "</li>" +
                        "</ul>" +
                        "</div>";
                    if (isUpload === "false") {
                        optstr = "";
                    }
                    //计算商品折扣
                    var discount = 0;
                    if (data[i].price !== 0 && data[i].orginPrice !== 0) {
                        discount = (data[i].price / data[i].orginPrice).toFixed(2);
                    }
                    var classStr = "";
                    if (data[i].isLackStock) {
                        classStr = "danger";
                    }

                    if (data[i].refundQuantity !== 0) {
                        classStr = "warning";
                    }
                    html += "<tr class='" + classStr + "'>" +
                        "<td>" + data[i].productName + "</td >" +
                        "<td>" + data[i].productCode + "</td>" +
                        "<td>" + data[i].orginPrice + "</td>" +
                        "<td>" + data[i].price + "</td>" +
                        "<td>" + discount + "</td>" +
                        "<td>" + data[i].quantity + "<span style='color:red;'>（" + data[i].lockNum + "）</span></td>" +
                        "<td>" + data[i].refundQuantity + "</td>"+
                        "<td><a onclick='getProductStock(" + data[i].productId + ")' style='color: blue; text-decoration: underline'>" + data[i].totalQuantity + "</a></td>"+
                        "<td>" + data[i].sumPrice + "</td>" +
                        "<td>" + optstr+
                        "</td>" +
                        "</tr >";
                }
                $("#tbody2").html(html);
            }
            else {
                $("#tbody2").html("");
            }
        }
    });
}

/*获取商品信息（模态框添加商品）*/
var getProductInfo = function (id) {
    var productId = id;
    var orderId = parseInt($("#orderId").val());
    var priceTypeId = parseInt($("#priceType").select2("data")[0].id);
    var priceTypeName = $("#priceType").select2("data")[0].text;
    $oms.ajax({
        url: "/B2BOrder/CheckOrderProductCount",
        data: { orderId: orderId, productId: productId },
        success: function (data) {
            if (data.isSucc) {
                if (data.count == 0) {
                    $("#stack2").modal();
                    $oms.ajax({
                        url: "/Product/GetProductInfo",
                        data: { saleproductId: productId, priceTypeId: priceTypeId },
                        success: function (data) {
                            if (data.isSucc) {
                                if (data.data) {
                                    $("#pName").text(data.data.saleProduct.product.name);
                                    $("#code").text(data.data.saleProduct.product.code);
                                    $("#priceName").html(priceTypeName);
                                    $("#order_product_quantity").val(0);
                                    $("#order_product_sumprice").val(0);
                                }
                                if (data != null) {
                                    $("#availableStock").text(data.data.saleProduct.availableStock);
                                    $("#stock a").text(data.data.saleProduct.stock);
                                    $("#stock a").attr("onclick", "getProductStock(" + data.data.saleProduct.productId + ")");
                                    $("#lockStock").text(data.data.saleProduct.lockStock);
                                    $("#order_product_id").val(data.data.saleProductId);
                                    if (data.data.price) {
                                        $("#order_product_orginPrice").val(data.data.price);
                                        
                                        f = Math.round(parseFloat(parseFloat($("#order_product_orginPrice").val())*1.00) * 100) / 100;
                                        $("#order_product_price").val(f);
                                    }
                                    else {
                                        $("#order_product_orginPrice").val(0);
                                        $("#order_product_price").val(0);
                                        alertWarn("该商品" + priceTypeName +"价格为0或错误，请重新设置商品的改价格！");
                                    }
                                }
                                else {
                                    alertWarn("该商品未更新库存信息");
                                }

                            }
                        },
                        error: function (result) {
                            alertInfo("无法获取到该商品信息！" + result);
                        }
                    });
                }
                else {
                    alertWarn(data.data)
                }
            }
        }
    });
}

/*修改商品（模态框）*/
var motifyOrderProduct = function (id) {
    //var isCreator = $("#isCreator").val();
    //if (isCreator == "false") {
    //    alertWarn("当前账号不是创建人，没有权限修改订单商品");
    //    return false;
    //}
    $oms.ajax({
        url: "/B2BOrder/GetOrderProductInfo",
        data: { id: id },
        success: function (data) {
            if (data.isSucc) {
                $("#pName").text(data.data[0].productName);
                $("#code").text(data.data[0].productCode);
                $("#order_product_id").val(data.data[0].saleProductId);
                $("#availableStock").text(data.data[0].availableStock);
                $("#stock").text(data.data[0].stock).attr("onclick", "getProductStock(" + data.data[0].productId + ")").attr("style", "color:blue;text-decoration:underline");
                $("#lockStock").text(data.data[0].lockStock);
                $("#order_product_orginPrice").val(data.data[0].orginPrice);
                $("#order_product_price").val(data.data[0].price);
                $("#order_product_quantity").val(data.data[0].quantity);
                $("#order_product_sumprice").val(data.data[0].sumPrice);
                f = Math.round(parseFloat((data.data[0].price / data.data[0].orginPrice)) * 100) / 100;
                $("#discount").val(f);
                $("#stack2").modal();
                $("#order_product_ok").attr("onclick", "submitOrderProduct('" + id + "')");
            }
        }
    })
}

/*删除订单商品*/
var deleteOrderProduct = function (id, productName, quantity) {
    //var isCreator = $("#isCreator").val();
    var orderId = $("#orderId").val();
    //if (isCreator == "false") {
    //    alertWarn("当前账号不是创建人，没有权限删除订单商品");
    //    return false;
    //}
    isContinue(function () {
        $oms.ajax({
            url: "/B2BOrder/DeleteOrderProductById",
            data: { id: id, orderId: orderId, productName: productName, quantity: quantity},
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("删除成功");
                    setTimeout(function () { window.location.reload(); }, 1000);
                    showOrderProducts();
                } else {
                    alertError(data.msg);
                }
            }
        })
    }, null, "确定删除所选商品？")
}

/*修改、增加订单商品*/
var submitOrderProduct = function (id) {
    $("#order_product_ok").attr("disabled", true);
    var orderProductId = id || 0;
    var orderId = $("#orderId").val();
    var orderProductName = $('#pName').html();
    var orderProductPriceType = $('#priceName').html();
    var saleProductId = $("#order_product_id").val();
    var quantity = $("#order_product_quantity").val();
    var orginPrice = $("#order_product_orginPrice").val();
    var sumPrice = $("#order_product_sumprice").val();
    var price = $("#order_product_price").val();
    var OrderProductModel = {
        Id: (orderProductId == 0 ? null : orderProductId),
        OrderId: orderId,
        SaleProductId: saleProductId,
        ProductName: orderProductName,
        Quantity: quantity,
        OrginPrice: orginPrice,
        SumPrice: sumPrice,
        Price: price,
    }
    //if (orginPrice == 0) {
    //    alertWarn("请确认该商品的" + orderProductPriceType + "价格为0或者错误，请检查此价格后选择！");
    //    $("#order_product_ok").attr("disabled", false);
    //    return false;
    //}
    if (quantity==0 || quantity == "") {
        alertWarn("数量不能为空");
        $("#order_product_ok").attr("disabled", false);
        return false;
    }
    if (price == "") {
        alertWarn("价格不能为空");
        $("#order_product_ok").attr("disabled", false);
        return false;
    }
    if (/*sumPrice != price * quantity ||*/ sumPrice===0) {
        alertWarn("总金额错误");
        $("#order_product_ok").attr("disabled", false);
        return false;
    }
    //var availableStock = $("#availableStock").text();
    //if (availableStock == 0) {
    //    alertWarn("请确认商品剩余可售库存！");
    //    $("#order_product_ok").attr("disabled", false);
    //    return false;
    //}
    if (orderProductId == 0) {
        $oms.ajax({
            url: "/B2BOrder/AddB2BOrderProduct",
            data: { OrderProductModel: OrderProductModel },
            success: function (data) {
                $("#order_product_ok").attr("disabled", false);
                if (data.isSucc) {
                    alertSuccess("添加成功");
                    $("#stack2").modal("hide");
                    $("#productsModal").modal("hide");
                    $(".order-product").removeClass("display-hide");
                    showOrderProducts(orderId);
                    setTimeout(function () {
                        window.location.reload();
                    },200);
                } else {
                    alertError(data.msg);
                }
            }
        });
    }
    else {
        $oms.ajax({
            url: "/B2BOrder/UpdateB2BOrderProduct",
            data: { OrderProductModel: OrderProductModel},
            success: function (data) {
                $("#order_product_ok").attr("disabled", false);
                if (data.isSucc) {
                    alertSuccess("修改成功");
                    $("#stack2").modal("hide");
                    showOrderProducts(orderId);
                    setTimeout(function () {
                        window.location.reload();
                    }, 200);
                } else {
                    alertError(data.msg);
                }
            }
        });
    }
}

/*添加、修改商品计算、折扣商品价格*/
//数量
var calculationSumPrice = function (e) {
    var price = parseFloat($("#order_product_price").val().trim());
    if (price > 0) {
        var q = parseFloat($(e).val().trim());
        if ( String(q).indexOf(".") !== -1) {
            alertError("商品数量不能为小数请重新填写！");
            return false;
        }
        if (q > 0) {
            var sumPrice = price * q;
            //f = Math.round(sumPrice * 100) / 100;
            $("#order_product_sumprice").val(sumPrice.toFixed(2));
        } else {
            $("#order_product_sumprice").val(0);
        }
    }
};
//单价
var calculationPriceChange = function (e) {
    var price = $("#order_product_price").val();
    var originPrice = $("#order_product_orginPrice").val();
    var quantity = $("#order_product_quantity").val();
    $("#discount").val(Math.floor((price / originPrice) * 100) / 100);
    if (quantity > 0) {
        var sumPrice = price * quantity;
        $("#order_product_sumprice").val(sumPrice);
    }
};
//总金额
var calculationChange = function () {
    var sumPrice = $("#order_product_sumprice").val();
    var quantity = $("#order_product_quantity").val();
    var originPrice = $("#order_product_orginPrice").val();
    if (quantity != 0 && quantity != "") {
        //计算总金额
        var price = $("#order_product_price").val((sumPrice / quantity).toFixed(2));
        $("#discount").val(Math.floor((price.val() / originPrice) * 100) / 100);
    }
};
//折扣触发
function calculationDiscount() {
    var discount = $("#discount").val();
    var originPrice = $("#order_product_orginPrice").val();
    var quantity = $("#order_product_quantity").val();
    if (discount > 0) {
        var price = $("#order_product_price").val(Math.floor((discount * originPrice) * 100) / 100);
        if (quantity > 0) {
            var sumPrice = price.val() * quantity;
            $("#order_product_sumprice").val(sumPrice.toFixed(2));
        }
    }
};
//var approvalOrder = function (e) {
//    var orderId = $("#orderId").val();
//    var that = e;
//    isContinue(function () {
//        //通过执行方法
//        $oms.ajax({
//            url: "/B2BOrder/ApprovalOrder",
//            data: { orderId: orderId, state: "true" },
//            success: function (data) {
//                if (data.isSucc) {
//                    alertSuccess("审核成功");
//                    $(that).addClass("display-hide");
//                    window.location.reload();
//                }
//                else {
//                    alertError(data.msg);
//                }
//            }

//        })

//    }, function () {
//        //回退执行方法
//        $oms.ajax({
//            url: "/B2BOrder/ApprovalOrder",
//            data: { orderId: orderId, state: "false" },
//            success: function (data) {
//                if (data.isSucc) {
//                    alertSuccess("回退成功");
//                    $(that).addClass("display-hide");
//                }
//                else {
//                    alertError(data.msg);
//                }
//            }

//        })

//    }, "审核当前订单！", "退回", "通过", "red", "green");
//}

/*审核订单*/
var approvalOrder = function (e) {
    var orderId = $("#orderId").val();
    isContinue(function () {
        $oms.ajax({
            url: "/B2BOrder/ApprovalOrder",
            data: { orderId: orderId},
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("审核成功");
                    //$(that).addClass("display-hide");
                    window.location.reload();
                }
                else {
                    alertError(data.msg);
                }
            }

        })
    }, null,"确定审核吗？")
}

/*确认订单*/
var confirm = function (e) {
    var orderId = $("#orderId").val();
    var that = e;
    isContinue(function () {
        //通过执行方法
        $oms.ajax({
            url: "/B2BOrder/ConfirmOrder",
            data: { orderId: orderId, state: "true" },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("确认成功");
                    $(that).addClass("display-hide");
                    window.location.reload();
                }
                else {
                    alertError(data.msg);
                }
            }

        });
    }, null, "确认当前订单！", null, "确认", null, null);
};

/*设置订单无效*/
var setInvalid = function (e) {
    var orderId = $("#orderId").val();
    var that = e;
    isContinue(function () {
        $("#loading").modal("show");
        //设置无效执行方法
        $oms.ajax({
            url: "/B2BOrder/SetInvalid",
            data: { orderId: orderId },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("设置无效成功！");
                    $(that).addClass("display-hide");
                    setTimeout(function () { window.location.reload(); }, 1000);
                } else
                {
                    alertError(data.msg);
                }
            }
        });
    }, null, "确认设置当前订单为无效！", null, "确认", null, null);
};

/*审核订单*/
var submitApproval = function (e) {
    var orderId = $("#orderId").val();
    var that = e;
    isContinue(function () {
        //通过执行方法
        $oms.ajax({
            url: "/B2BOrder/SubmitApproval",
            data: { orderId: orderId},
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("提交成功");
                    $(that).addClass("display-hide");
                    window.location.reload();
                }
                else {
                    alertError(data.msg);
                }
            }

        })
    }, null, "确认提交审核！", null, "确认", null, null);
}

/*验收*/
var checkAccept = function () {
    var orderId = $("#orderId").val();
    isContinue(function () {
        //通过执行方法
        $oms.ajax({
            url: "/B2BOrder/CheckAcceptB2BOrder",
            type:"POST",
            data: { orderId: orderId },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("提交成功");
                    window.location.reload();
                }
                else {
                    alertError(data.msg);
                }
            }

        })
    }, null, "确认验收吗？", null, "确认", null, null);
}

/*记账*/
var bookKeeping = function () {
    var orderId = parseInt($("#orderId").val());
    var payType = parseInt($("#payType").select2("data")[0].id);
    var payMentType = parseInt($("#payMentType").select2("data")[0].id);
    var payPrice = parseFloat($("#payPrice").val());
    var isPay = $('#radio1').is(':checked');
    var OrderModel = {
        Id: orderId,
        PayType: payType,
        PayMentType: payMentType,
        PayPrice: payPrice,
        IsPayOrRefund: isPay
    }
    if (!(payType > 0)) {
        alertWarn("请选择支付方式");
        return false;
    }
    if (!(payMentType > 0)) {
        alertWarn("请选择汇款方式！");
        return false;
    }

    if (payPrice < 0) {
        alertWarn("支付金额不能小于0！");
        return false;
    }
    var sumPrice = parseFloat($("#sumPrice").val());
    var paiedPrice = parseFloat($("#paiedPrice").val());
    if (isPay) {
        if (payPrice + paiedPrice > sumPrice) {
            alertWarn("支付金额加上已支付金额大于订单总金额，请核实！");
            return false;
        }
    }
    else {
        if (paiedPrice < payPrice) {
            alertWarn("退款金额大于已支付金额，请核实！");
            return false;
        }
    }
    isContinue(function () {
        //通过执行方法
        $oms.ajax({
            url: "/B2BOrder/BookKeeping",
            data: { OrderModel: OrderModel },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("操作成功");
                    if (isPay) {
                        $("#paiedPrice").val(payPrice + paiedPrice);
                    }
                    else {
                        $("#paiedPrice").val(paiedPrice - payPrice);
                    }
                    setTimeout(function () { window.location.reload(); }, 800);
                }
                else {
                    alertError(data.msg);
                }
            }

        });
    }, null, "对当前订单进行记账！", null, "确认", null, null);
};

/*确认完成本单*/
var confirmComplete = function () {
    var orderId = parseInt($("#orderId").val());
    var sumPrice = parseInt($("#sumPrice").val());
    var paiedPrice = parseInt($("#paiedPrice").val());
    if (sumPrice != paiedPrice) {
        isContinue(function () {
            $oms.ajax({
                url: "/B2BOrder/ConfirmCompleteB2BOrder",
                type: "POST",
                data: { orderId: orderId },
                success: function (data) {
                    if (data.isSucc) {
                        alertSuccess("操作成功");
                        setTimeout(function () { window.location.reload(); }, 800);
                    }
                    else {
                        alertError(data.msg);
                    }
                }

            });
        }, null, "总金额与已支付金额不相等，确定完成吗？", null, "确认", null, null);
    }
    else {
        isContinue(function () {
            $oms.ajax({
                url: "/B2BOrder/ConfirmCompleteB2BOrder",
                type: "POST",
                data: { orderId: orderId },
                success: function (data) {
                    if (data.isSucc) {
                        alertSuccess("操作成功");
                        setTimeout(function () { window.location.reload(); }, 800);
                    }
                    else {
                        alertError(data.msg);
                    }
                }
            });
        }, null, "确认完成本单吗？", null, "确认", null, null);
    }
}

/*修改财务备注*/
var updateFinanceMark = function () {
    var mark = $('#caiwuMark').val();
    var orderId = $("#orderId").val();
    isContinue(function () {
        //通过执行方法
        $oms.ajax({
            url: "/B2BOrder/updateFinanceMark",
            type: "POST",
            data: { orderId: orderId, mark: mark },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("修改成功");
                    setTimeout(function () { window.location.reload(); }, 800);
                }
                else {
                    alertError(data.msg);
                }
            }

        })
    }, null, "确认修改财务备注吗？", null, "确认", null, null);
}

/*获取发票信息*/
var getDefaultInvoiceInfo = function (e) {
    var orderId = parseInt($("#orderId").val());
    var isDefault = $(e).is(':checked');
    if (isDefault) {
        $oms.ajax({
            url: "/B2BOrder/GetDefaultInvoiceInfo",
            data: { orderId: orderId },
            success: function (data) {
                if (data.isSucc) {
                    $("#e_mail").val(data.data.customerEmail);
                    $("#title").val(data.data.title);
                    $("#taxpayerID").val(data.data.taxpayerId);
                    $("#registerAddress").val(data.data.registerAddress);
                    $("#registerTel").val(data.data.registerTel);
                    $("#bankOfDeposit").val(data.data.bankOfDeposit);
                    $("#bankAccount").val(data.data.bankAccount);
                }
            }

        })
    }
}

/*提交发票信息*/
var submitOrderInvoiceInfo = function () {

    var invoiceType = $("#invoiceType option:selected").val();//发票类型
    if (invoiceType == 0) {
        $("#invoiceInfo").modal("hide");
        return false;
    }
    var customerEmail = $("#e_mail").val().trim();
    var title = $("#title").val().trim();
    var taxpayerId = $("#taxpayerID").val().trim();
    var registerAddress = $("#registerAddress").val().trim();
    var registerTel = $("#registerTel").val().trim();
    var bankOfDeposit = $("#bankOfDeposit").val().trim();
    var bankAccount = $("#bankAccount").val().trim();
    var invoiceNo = $("#invoiceNo").val().trim();
    var invoiceMode = $("#InoviceModes option:selected").val();
    //if (invoiceNo == null || invoiceNo == "") {
    //    alertWarn("发票号不能为空！");
    //    return false;
    //}

    if (invoiceMode == 0) {
        if ($.trim(customerEmail) == "") {
            alertWarn("邮箱不能为空！");
            return false;
        }
    }

    var reg_mail = /^([a-zA-Z0-9_-])+@([a-zA-Z0-9_-])+(.[a-zA-Z0-9_-])+/;
    if ($.trim(customerEmail) !== "" && !reg_mail.test($.trim(customerEmail))) {
        alertWarn("邮箱有误，请核实！");
        return false;
    }
    
    if ($.trim(title) == "") {
        alertWarn("发票抬头不能为空！");
        return false;
    }
    if (invoiceType != 1) {
        if (taxpayerId == "") {
            alertWarn("纳税人识别码不能为空！");
            return false;
        }
        if (invoiceType != 2) {
            if ($.trim(registerAddress) == "") {
                alertWarn("注册地址不能为空！");
                return false;
            }
            if ($.trim(registerTel) == "") {
                alertWarn("注册电话不能为空！");
                return false;
            }
            if ($.trim(bankOfDeposit) == "") {
                alertWarn("开户银行不能为空！");
                return false;
            }
            if ($.trim(bankAccount) == "") {
                alertWarn("银行账号不能为空！");
                return false;
            }
            var reg = /^(0|86|17951)?(13[0-9]|15[012356789]|17[01678]|18[0-9]|14[57])[0-9]{8}$/;
            var reg2 = /^(0[0-9]{2,3}\-)([2-9][0-9]{6,7})+(\-[0-9]{1,4})?$/;

            if (!reg.test($.trim(registerTel)) && !reg2.test($.trim(registerTel))) {
                alertWarn("电话有误请核实，查看有没有空格！");
                return false;
            }
        }

    }
    var InvoiceInfoModel = {
        CustomerEmail: customerEmail,
        Title: title,
        TaxpayerID: taxpayerId,
        RegisterAddress: registerAddress,
        RegisterTel: registerTel,
        BankOfDeposit: bankOfDeposit,
        BankAccount: bankAccount,
        InvoiceNo: invoiceNo
    }
    var orderId = parseInt($("#orderId").val());
    $oms.ajax({
        url: "/B2BOrder/SubmitOrderInvoiceInfo",
        data: { invoiceInfoModel: InvoiceInfoModel, orderId: orderId, invoiceMode: invoiceMode, invoiceType: invoiceType },
        success: function (data) {
            if (data.isSucc) {
                alertSuccess("保存成功");
                $("#invoiceInfo").modal("hide");
                window.location.reload();
            } else {
                alertError("保存失败");
                $("#invoiceInfo").modal("hide");
            }
        }

    })
}

/*复制订单*/
var copyOrder = function () {
    isContinue(function () {
        $.ajax({
            url: "/B2BOrder/CopyOrder",
            data: { orderId: $("#orderId").val() },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("复制订单成功！");
                    setTimeout(function () { window.location.reload() }, 1000);
                } else {
                    alertError(data.data);
                }
            },
            error: function (data) {
                alertError("复制订单错误！");
            }
        })
    }, null, "点击继续复制订单！")
}


/*缺货解锁*/
var unLockOrder = function () {
    isContinue(function () {
        $.ajax({
            url: "/B2BOrder/UnLockLackOrder",
            data: { orderId: $("#orderId").val() },
            success: function (data) {
                if (data.isSucc) {
                    alertSuccess("缺货解锁成功！");
                    setTimeout(function () { window.location.reload() }, 1000);
                } else {
                    alertError(data.msg);
                }
            },
            error: function (data) {
                alertError("缺货解锁错误！");
            }
        });
    }, null, "点击继续解锁订单缺货！")
}


/*补录商城购买帐号*/
var modifyUserName = function () {
    $("#userNameModel").modal();
}

/*上传订单及取消订单部分代码重复，已写在页面处*/

///*上传订单到WMS*/
//function uploadOrderToWMS(e) {
//    var orderId = parseInt($("#orderId").val());
//    var that = e;
//    isContinue(function () {
//        $oms.ajax({
//            url: "/B2BOrder/UploadB2BOrderToWMS",
//            data: { OrderId: orderId },
//            beforeSend: function () {
//                //防止重复提交
//                $("#upLoadOrder").attr("disabled", true).css("pointer-events", "none");
//                $('#upLoadOrder').css({ opacity: 0.2 });
//                $('#loading').modal('show');
//            },
//            success: function (data) {
//                $('#loading').modal('hide');
//                if (data.isSucc) {
//                    alertSuccess("上传成功！");
//                    $(that).addClass("display-hide");
//                    window.location.reload();
//                } else {
//                    alertError(data.msg);
//                }
//            },
//            error: function (res) {
//                $('#loading').modal('hide');
//                alertError("上传失败！");
//                $("#upLoadOrder").attr("disabled", false).css("pointer-events", "auto");
//                $('#upLoadOrder').css({ opacity: 1 });
//            }
//        });
//    }, null, "上传订单到WMS!", null, "确认", null, null);
//}

///*取消上传订单到WMS*/
//function cancelUploadOrderToWMS(e) {
//    var orderId = parseInt($("#orderId").val());
//    var that = e;
//    isContinue(function () {
//        $oms.ajax({
//            url: "/B2BOrder/CancelUploadB2BOrder",
//            data: { OrderId: orderId },
//            beforeSend: function () {
//                //防止重复提交
//                $("#cancelUploadOrder").attr("disabled", true).css("pointer-events", "none");
//                $('#cancelUploadOrder').css({ opacity: 0.2 });
//                $('#loading').modal('show');
//            },
//            success: function (data) {
//                $('#loading').modal('hide');
//                if (data.isSucc) {
//                    alertSuccess("订单取消成功！");
//                    $(that).addClass("display-hide");
//                    window.location.reload();
//                } else {
//                    alertError(data.msg);
//                }
//            },
//            error: function (res) {
//                $('#loading').modal('hide');
//                alertError("取消失败！");
//                $("#cancelUploadOrder").attr("disabled", false).css("pointer-events", "auto");
//                $('#cancelUploadOrder').css({ opacity: 1 });

//            }
//        });
//    }, null, "取消订单!", null, "确认", null, null);
//}