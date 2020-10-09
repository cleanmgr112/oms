var Login = function () {

    var handleLogin = function () {
        $('.login-form').validate({
            errorElement: 'span', //default input error message container
            errorClass: 'help-block', // default input error message class
            focusInvalid: false, // do not focus the last invalid input
            rules: {
                username: {
                    required: true
                },
                password: {
                    required: true
                },
                verify: {
                    required: true
                }
            },

            invalidHandler: function (event, validator) { //display error alert on form submit   
                $('.alert-danger', $('.login-form')).show();
            },

            highlight: function (element) { // hightlight error inputs
                $(element)
                    .closest('.form-group').addClass('has-error'); // set error class to the control group
            },

            success: function (label) {
                label.closest('.form-group').removeClass('has-error');
                label.remove();
            },

            errorPlacement: function (error, element) {
                error.insertAfter(element.closest('.input-icon'));
            },

            submitHandler: function (form) {
                LoginPost();
            }
        });

        $('.login-form input').keypress(function (e) {
            if (e.which == 13) {
                if ($('.login-form').validate().form()) {


                    LoginPost();


                }
                return false;
            }
        });
    }

    var LoginPost = function () {
        var account = $.trim($('#userName').val());
        var password = $.trim($('#userPwd').val());
        var verify = $.trim($('#imgCode').val());
        if (!account) {
            $('.errInfo').empty().text('* 请输入用户名');
            $('#account').focus();
            return false;
        }
        if (!password) {
            $('.errInfo').empty().text('* 请输入密码');
            $('#password').focus();
            return false;
        }
        if (!verify) {
            $('.errInfo').empty().text('* 请输入验证码');
            $('#verify').focus();
            return false;
        }
        $.ajax({
            type: "POST",
            url: "/User/Login",
            data: {
                username: account,
                password: password,
                verify: verify,
                returnUrl: ''//$procurement.getQueryParam('returnUrl')
            },
            success: function (msg) {
                if (msg.isSucc) {
                    if (msg.data.returnUrl == '/') {
                        window.location.href = '/Home/Index';
                    } else {
                        window.location.href = returnUrl;
                    }
                } else {
                    $('.errInfo').empty().text("*" + msg.msg);
                    $('.imageCode-input').val("");
                    $('.userPwd').val("");
                    $('#imgcode').attr('src', '/User/VerifyCode?random=' + new Date().getTime());
                    setTimeout(function () {
                        $('.errInfo').empty();
                    }, 3000);
                }
            }

        });

    }

    return {
        //main function to initiate the module
        init: function () {
            handleLogin();
            // init background slide images
            $.backstretch([
                "../Metronic/assets/pages/media/bg/1.jpg",
                "../Metronic/assets/pages/media/bg/2.jpg",
                "../Metronic/assets/pages/media/bg/3.jpg"
            ], {
                    fade: 1000,
                    duration: 8000
                }
            );
        }
    };

}();

jQuery(document).ready(function () {
    Login.init();
});