var select2Component = function () {
    var handle = function () {
        var placeholder = "--请选择--";

        $(".select2, .select2-multiple").select2({
            placeholder: placeholder,
            width: null
        });
        $(".select2-allow-clear").select2({
            allowClear: true,
            placeholder: placeholder,
            width: null
        });
        //下拉框多选=>有序记录
        $("select").on("select2:select", function (evt) {
            var element = evt.params.data.element;
            var $element = $(element);
            $element.detach();
            $(this).append($element);
            $(this).trigger("change");
        });
        //日期控件，最小视图到月份，可以选取到天数
        $(".form_datetime").datetimepicker({
            language: 'zh-CN',
            format: "yyyy-mm-dd",
            autoclose: true,
            todayBtn: true,
            initialDate: new Date(),
            minView: "month"
        });
    }
    return {
        init: function () {
            handle();
        }
    }
}();
jQuery(document).ready(function () {
    select2Component.init();
})