var UIToastr = function () {
    return {
        //main function to initiate the module
        init: function () {
            toastr.options = {
                "closeButton": true,
                "debug": false,
                "positionClass": "toast-top-right",
                "onclick": null,
                "showDuration": "1000",
                "hideDuration": "1000",
                "timeOut": "5000",
                "extendedTimeOut": "1000",
                "showEasing": "swing",
                "hideEasing": "linear",
                "showMethod": "fadeIn",
                "hideMethod": "fadeOut"
            };
        }
    };
}();

jQuery(document).ready(function () {
    UIToastr.init();
});

//example   alertInfo("Test Message", "Test Title", { timeOut: 100 });
var alertInfo = function (msg, title, option) {
    toastr["info"](msg, title == null ? "通知" : title, option);
}
var alertSuccess = function (msg, title, option) {
    toastr["success"](msg, title == null ? "成功" : title, option);
}
var alertWarn = function (msg, title, option) {
    toastr["warning"](msg, title == null ? "警告" : title, option);
}
var alertError = function (msg, title, option) {
    toastr["error"](msg, title == null ? "错误" : title, option);
}