var UIModals = function () {
    return {
        //main function to initiate the module
        init: function () {
            var func_ok;
            var func_cancel;
            var modals=''+
            '<div class="modal fade" id="basic" tabindex="-1" role="basic" aria-hidden="true">' +
                '<div class="modal-dialog">'+
                    '<div class="modal-content">'+
                        '<div class="modal-header">'+
                            '<button type="button" class="close" data-dismiss="modal" aria-hidden="true"></button>'+
                            '<h4 class="modal-title">信息提示</h4>'+
                        '</div>'+
                        '<div class="modal-body" id="body"">是否继续操作</div>'+
                        '<div class="modal-footer">'+
                            '<button type="button" class="btn btn-outline" onclick="cancel1()" id="cancel">取消</button>'+
                           '<button type="button" class="btn " onclick="continue1()" id="ok">继续</button>'+
                        '</div>'+
                    '</div>'+
                '</div>'+
                '</div > ';
            $("body").append(modals);
        }
    };
}();

jQuery(document).ready(function () {
    UIModals.init();
});
var isContinue = function (continueFunc, cancelFunc, body, cancel, ok,cancel_color,ok_color) {
    $("#basic").modal();
    var body_content = body || "是否继续操作?";
    var cancel_content = cancel || "取消";
    var ok_content = ok || "继续?";
    var cancel_color_content = cancel_color  || "dark";
    var ok_color_content = ok_color || "green";
    $("#body").text(body_content);
    $("#ok").text(ok_content);
    $("#cancel").text(cancel_content);
    $("#ok").addClass(ok_color_content);
    $("#cancel").addClass(cancel_color_content);
    func_ok = continueFunc;
    func_cancel = cancelFunc;
}
var continue1 = function () {
    typeof func_ok == 'function' && func_ok();
    $("#basic").modal("hide");
}
var cancel1 = function () {
    typeof func_cancel == 'function' && func_cancel();
    $("#basic").modal("hide");
}