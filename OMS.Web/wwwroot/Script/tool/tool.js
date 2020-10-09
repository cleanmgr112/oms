//Begin:string扩展
(function ($) { if (!$) return; $.extend(String.prototype, { 'toBoolean': function () { return (this == 'false' || this == '' || this == '0') ? false : true; }, 'toNumber': function () { return (!isNaN(this)) ? Number(this) : this.toString(); }, 'toRealValue': function () { return (this == 'true' || this == 'false') ? this.toBoolean() : this.toNumber(); }, 'trim': function () { return this.replace(/(^\s*)|(\s*$)/g, ''); }, 'ltrim': function () { return this.replace(/(^\s*)/g, ''); }, 'rtrim': function () { return this.replace(/(\s*$)/g, ''); }, 'trimAll': function () { return this.replace(/\s/g, ''); }, 'left': function (len) { return this.substring(0, len); }, 'right': function (len) { return (this.length <= len) ? this.toString() : this.substring(this.length - len, this.length); }, 'reverse': function () { return this.split('').reverse().join(''); }, 'startWith': function (start, noCase) { return !(noCase ? this.toLowerCase().indexOf(start.toLowerCase()) : this.indexOf(start)); }, 'endWith': function (end, noCase) { return noCase ? (new RegExp(end.toLowerCase() + "$").test(this.toLowerCase().trim())) : (new RegExp(end + "$").test(this.trim())); }, 'sliceAfter': function (str) { return (this.indexOf(str) >= 0) ? this.substring(this.indexOf(str) + str.length, this.length) : this.toString(); }, 'sliceBefore': function (str) { return (this.indexOf(str) >= 0) ? this.substring(0, this.indexOf(str)) : this.toString(); }, 'getByteLength': function () { return this.replace(/[^\x00-\xff]/ig, 'xx').length; }, 'subByte': function (len, s) { if (len < 0 || this.getByteLength() <= len) { return this.toString(); } var str = this; str = str.substr(0, len).replace(/([^\x00-\xff])/g, "\x241 ").substr(0, len).replace(/[^\x00-\xff]$/, "").replace(/([^\x00-\xff]) /g, "\x241"); return str + (s || ''); }, 'textToHtml': function () { return this.replace(/</ig, '&lt;').replace(/>/ig, '&gt;').replace(/\r\n/ig, '<br>').replace(/\n/ig, '<br>'); }, 'htmlToText': function () { return this.replace(/<br>/ig, '\r\n'); }, 'htmlEncode': function () { var text = this, re = { '<': '&lt;', '>': '&gt;', '&': '&amp;', '"': '&quot;' }; for (i in re) { text = text.replace(new RegExp(i, 'g'), re[i]); } return text; }, 'htmlDecode': function () { var text = this, re = { '&lt;': '<', '&gt;': '>', '&amp;': '&', '&quot;': '"' }; for (i in re) { text = text.replace(new RegExp(i, 'g'), re[i]); } return text; }, 'stripHtml': function () { return this.replace(/(<\/?[^>\/]*)\/?>/ig, ''); }, 'stripScript': function () { return this.replace(/<script(.|\n)*\/script>\s*/ig, '').replace(/on[a-z]*?\s*?=".*?"/ig, ''); }, 'replaceAll': function (os, ns) { return this.replace(new RegExp(os, "gm"), ns); }, 'escapeReg': function () { return this.replace(new RegExp("([.*+?^=!:\x24{}()|[\\]\/\\\\])", "g"), '\\\x241'); }, 'addQueryValue': function (name, value) { var url = this.getPathName(); var param = this.getQueryJson(); if (!param[name]) param[name] = value; return url + '?' + $.param(param); }, 'getQueryValue': function (name) { var reg = new RegExp("(^|&|\\?|#)" + name.escapeReg() + "=([^&]*)(&|\x24)", ""); var match = this.match(reg); return (match) ? match[2] : ''; }, 'getQueryJson': function () { if (this.indexOf('?') < 0) return {}; var query = this.substr(this.indexOf('?') + 1), params = query.split('&'), len = params.length, result = {}, key, value, item, param; for (var i = 0; i < len; i++) { param = params[i].split('='); key = param[0]; value = param[1]; item = result[key]; if ('undefined' == typeof item) { result[key] = value; } else if (Object.prototype.toString.call(item) == '[object Array]') { item.push(value); } else { result[key] = [item, value]; } } return result; }, 'getDomain': function () { if (this.startWith('http://')) return this.split('/')[2]; return ''; }, 'getPathName': function () { return (this.lastIndexOf('?') == -1) ? this.toString() : this.substring(0, this.lastIndexOf('?')); }, 'getFilePath': function () { return this.substring(0, this.lastIndexOf('/') + 1); }, 'getFileName': function () { return this.substring(this.lastIndexOf('/') + 1); }, 'getFileExt': function () { return this.substring(this.lastIndexOf('.') + 1); }, 'parseDate': function () { return (new Date()).parse(this.toString()); }, 'parseJSON': function () { return (new Function("return " + this.toString()))(); }, 'parseAttrJSON': function () { var d = {}, a = this.toString().split(';'); for (var i = 0; i < a.length; i++) { if (a[i].trim() == '' || a[i].indexOf(':') < 1) continue; var item = a[i].sliceBefore(':').trim(), val = a[i].sliceAfter(':').trim(); if (item != '' && val != '') d[item.toCamelCase()] = val.toRealValue(); } return d; }, '_pad': function (width, ch, side) { var str = [side ? '' : this, side ? this : '']; while (str[side].length < (width ? width : 0) && (str[side] = str[1] + (ch || ' ') + str[0])); return str[side]; }, 'padLeft': function (width, ch) { if (this.length >= width) return this.toString(); return this._pad(width, ch, 0); }, 'padRight': function (width, ch) { if (this.length >= width) return this.toString(); return this._pad(width, ch, 1); }, 'toHalfWidth': function () { return this.replace(/[\uFF01-\uFF5E]/g, function (c) { return String.fromCharCode(c.charCodeAt(0) - 65248); }).replace(/\u3000/g, " "); }, 'toCamelCase': function () { if (this.indexOf('-') < 0 && this.indexOf('_') < 0) { return this.toString(); } return this.replace(/[-_][^-_]/g, function (match) { return match.charAt(1).toUpperCase(); }); }, 'format': function () { var result = this; if (arguments.length > 0) { var parameters = (arguments.length == 1 && $.isArray(arguments[0])) ? arguments[0] : $.makeArray(arguments); $.each(parameters, function (i, n) { result = result.replace(new RegExp("\\{" + i + "\\}", "g"), n); }); } return result; }, 'substitute': function (data) { if (data && typeof (data) == 'object') { return this.replace(/\{([^{}]+)\}/g, function (match, key) { var value = data[key]; return (value !== undefined) ? '' + value : ''; }); } else { return this.toString(); } }, 'extractValues': function (pattern, options) { var str = this.toString(); options = options || {}; var delimiters = options.delimiters || ["{", "}"]; var lowercase = options.lowercase; var whitespace = options.whitespace; var special_chars_regex = /[\\\^\$\*\+\.\?\(\)]/g; var token_regex = new RegExp(delimiters[0] + "([^" + delimiters.join("") + "\t\r\n]+)" + delimiters[1], "g"); var tokens = pattern.match(token_regex); var pattern_regex = new RegExp(pattern.replace(special_chars_regex, "\\$&").replace(token_regex, "(\.+)")); if (lowercase) { str = str.toLowerCase(); } if (whitespace) { str = str.replace(/\s+/g, function (match) { var whitespace_str = ""; for (var i = 0; i < whitespace; i++) { whitespace_str = whitespace_str + match.charAt(0); }; return whitespace_str; }); }; var matches = str.match(pattern_regex); if (!matches) { return null; } matches = matches.splice(1); var output = {}; for (var i = 0; i < tokens.length; i++) { output[tokens[i].replace(new RegExp(delimiters[0] + "|" + delimiters[1], "g"), "")] = matches[i]; } return output; } }); })(jQuery);
//End

//Begin:公用方法
function priceFormat(p) {
    return p && p.toFixed(2).replace(/(\d{1,3})(?=(\d{3})+(?:\.))/g, '$1,');
}
//End

(function ($) {
    if (window.$oms) return;
    var $oms = [];
    //将当前 url 的参数值
    $oms.getQueryParam = function (name) {
        if (!name)
            return "";
        name = name.toLowerCase();
        var se = decodeURIComponent(window.location.search);
        if (!se) {
            return '';
        }
        var arr = se.slice(1).split("&");
        for (var i = 0; i < arr.length; i++) {
            var ar = arr[i].split("=");
            if (ar[0].toLowerCase() == name) {
                return ar[1];
            }
        }
        return "";
    }
    // 将当前 url 参数转成一个 js 对象
    $oms.getQueryParams = function (url) {
        var params = {};
        var se;
        if (url) {
            var a = document.createElement('a');
            a.href = url;
            se = decodeURIComponent(a.search);
        } else {
            se = decodeURIComponent(window.location.search);
        }

        if (!se) {
            return params;
        }

        var arr = se.slice(1).split("&");
        for (var i = 0; i < arr.length; i++) {
            var index = arr[i].indexOf("=");
            if (index >= 0) {
                var key = arr[i].substring(0, index);
                var value = arr[i].substring(index, arr[i].length);
                params[key] = value.replace('=', '');
            }

        }
        return params;
    }
    //validate
    if ($.validator) {
        $.extend($.validator.messages, {
            required: "必填",
            remote: "请修正此字段",
            email: "请输入有效的电子邮件地址",
            url: "请输入有效的网址",
            date: "请输入有效的日期",
            dateISO: "请输入有效的日期",
            number: "请输入有效的数字",
            digits: "只能输入数字",
            creditcard: "请输入有效的信用卡号码",
            equalTo: "您的输入不相同",
            extension: "请输入有效的后缀",
            maxlength: $.validator.format("最多可以输入 {0} 个字符"),
            minlength: $.validator.format("最少要输入 {0} 个字符"),
            rangelength: $.validator.format("请输入长度在 {0} 到 {1} 之间的字符串"),
            range: $.validator.format("请输入范围在 {0} 到 {1} 之间的数值"),
            max: $.validator.format("请输入不大于 {0} 的数值"),
            min: $.validator.format("请输入不小于 {0} 的数值")
        });
    }
    //ajax
    $oms.ajax = function (options) {
        $.ajax({
            url: options.url,
            data: options.data,
            type: options.type || 'post',
            success: function (res) {
                if (res.code == 403) {//未登录
                    //...
                }
                if (res.code == 401) {//未授权
                    //...
                }
                typeof options.success == 'function' && options.success(res);
            }
        })
    }
    //分页
    $oms.paginator = function (options) {
        if (!options.data) {
            options.data = { pageIndex: 1, pageSize: 10 }
        } else {
            options.data.pageIndex = options.data.pageIndex || 1;
            options.data.pageSize = options.data.pageSize || 10;
        }
        $oms.ajax({
            url: options.url,
            data: options.data,
            success: function (data) {
                if (data.isSucc) {
                    typeof options.success == 'function' && options.success(data.data);
                    var start = (options.data.pageIndex - 1) * options.data.pageSize + 1;
                    var end = (options.data.pageIndex * options.data.pageSize);
                    if (end > data.totalCount) { end = data.totalCount; }
                    var info = "当前显示 " + start + " 到 " + end + " 条，共 " + data.totalCount + " 条记录";
                    if (data.data.length == 0) {
                        info = "未找到任何数据";
                        $('#' + options.pageLimitId).hide();
                        $('#' + options.gotoPage).hide();
                    }
                    else {
                        $('#' + options.pageLimitId).show();
                        $('#' + options.gotoPage).show();
                    }
                    $("#p_" + options.pageLimitId).remove();
                    $("<p id='p_" + options.pageLimitId + "' style='float:left;margin-top:0px;'>" + info + "</p>").insertAfter($('#' + options.pageLimitId));
                    $('#' + options.pageLimitId).bootstrapPaginator({
                        //currentPage: 1,
                        currentPage: $("#pageNumber").val(),//表格下方列表页导航显示的选中的当前页面index
                        totalPages: data.totalPages,
                        size: "normal",
                        bootstrapMajorVersion: 3,
                        alignment: "right",
                        numberOfPages: 9,
                        onPageClicked: function (event, originalEvent, type, page) {//给每个页眉绑定一个事件，其实就是ajax请求，其中page变量为当前点击的页上的数字。
                            if (page < 1 || page > data.totalPages) {
                                return false;
                            }
                            options.data.pageIndex = page;
                            $oms.ajax({
                                url: options.url,
                                data: options.data,
                                dataType: 'JSON',
                                success: function (data) {
                                    if (data.isSucc) {
                                        typeof options.success == 'function' && options.success(data.data);
                                        var start = (options.data.pageIndex - 1) * options.data.pageSize + 1;
                                        var end = (options.data.pageIndex * options.data.pageSize);
                                        if (end > data.totalCount) { end = data.totalCount; }
                                        var info = "当前显示 " + start + " 到 " + end + " 条，共 " + data.totalCount + " 条记录";
                                        $("#p_" + options.pageLimitId).remove();
                                        $("<p id='p_" + options.pageLimitId + "' style='float:left;margin-top:0px;'>" + info + "</p>").insertAfter($('#' + options.pageLimitId));

                                        $("#pageNumber").val(page);//把当前选中的页面数赋值给页面上的pageNumber
                                    }
                                    else {
                                        alertError(data.code);
                                    }
                                }
                            });
                        },
                        onPageChanged: function (event, oldPage, newPage) {
                            $("#redirect").val(newPage);
                        }
                    });
                    if (data.data.length > 0) {
                    }
                }
                else {
                    alertError(data.code);
                }
            }
        })
    }
    //jquery fn extend
    $.fn.extend({
        $validate: function (options) {//表单验证
            var params = {
                errorElement: 'span', //default input error message container
                errorClass: 'help-block', // default input error message class
                focusInvalid: false, // do not focus the last invalid input
                highlight: function (element) { // hightlight error inputs
                    $(element).closest('.vgroup').addClass('has-error'); // set error class to the control group
                },
                success: function (label) {
                    label.closest('.vgroup').removeClass('has-error');
                    label.remove();
                },
                errorPlacement: function (error, element) {
                    error.insertAfter(element.closest('.vinput'));
                },
                submitHandler: function () {
                    submitForm();
                }
            };
            $.extend(params, options || {});
            $(this).validate(params);
            return $(this);
        },
    })

    window.$oms = $oms;
})($)