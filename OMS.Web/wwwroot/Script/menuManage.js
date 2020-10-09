var Menu = function () {

    var InitJsTree = function () {
        $('#container').jstree({
            'core': {
                'data': {
                    "url": "/Menu/GetJsonMenus",
                    "dataType": "json",// needed only if you do not supply JSON headers
                    "type": "POST"
                },
                "check_callback": true, // enable all modifications
            },
            //"plugins": ["contextmenu", "Search","State"]
        });
    }

    var SettingMenuForm = function () {
        $('#container').on("changed.jstree", function (e, data) {
            var selected = data.selected;
            if (selected.length > 1) {
                alertError("只能选择一个菜单！");
            }
            else if (selected.length == 1) {
                //ajax请求菜单信息
                $oms.ajax({
                    url: "/Menu/GetMenuInfo",
                    data: { menuId: selected[0] },
                    success: function (res) {
                        if (res.isSucc) {
                            SetMenuFormVal(res.data);
                        } else {
                            alertError("请求错误！");
                        }
                    }
                });
            }
        });
    }

    var IconSelect = function () {
        $('#Icon-hover').on('focus', function () {
            $.getJSON("/Script/FontIcon.json", function (icon) {
                var iconJson = icon.data;
                var iconList = '';
                var temple = ` <div class="fa-item col-md-3 col-sm-4" ondblclick="GetIconCode('{0}')"><i class="{1}"></i> {2} </div>`;

                for (var i = 0; i < iconJson.length; i++) {
                    var current = iconJson[i];
                    iconList += temple.format(current, 'fa fa-' + current, current);
                }
                $('#icon-content').empty().append(iconList);
            });
            $('#IconModal').modal();
        });
    }

    var ClearForm = function () {
        $('#addMenu').on('click', function () {
            SetMenuFormVal();
            $('#menuName').focus();
        });
    }

    var SubmitMenu = function () {
        $('#submitMenu').on('click', function () {
            var childUrl= $('#tags-input').val();
            var menuId = $('#menuId').val();
            var name = $('#menuName').val();
            var parentId = $('#parentModule').find('option:selected').data('menuid');
            var moduleName = $('#parentModule').val();
            var code = $('#sysCode').val();
            var url = $('#menuUrl').val();
            var icon = $('#Icon-hover').val();
            var state = $('#isNomal').find('input:checked').val();
            var sort = $("#sort-input").val();
            $oms.ajax({
                url: '/Menu/SettingMenu',
                type: 'POST',
                data: {
                    id: menuId,
                    name: name,
                    moduleName: moduleName,
                    parentId: parentId,
                    code: code,
                    url: url,
                    childUrl: childUrl,
                    icon: icon,
                    state: state,
                    sort: sort
                },
                success: function (res) {
                    if (!res.isSucc) {
                        alertError(res.msg);
                    } else {
                        alertSuccess("添加成功！")
                        setTimeout(function () {
                            window.location.reload();
                        }, 800)
                    }
                }
            });
        });
    }

    return {
        init: function () {
            InitJsTree();
            SettingMenuForm();
            IconSelect();
            ClearForm();
            SubmitMenu();
        }
    }
}();

jQuery(document).ready(function () {
    Menu.init();
});

function GetIconCode(icon) {
    $('#IconModal').modal('hide');
    var iconCode = 'fa fa-' + icon;
    $('#Icon-hover').val(iconCode);
}

function SetMenuFormVal(menu) {
    if (menu) {
        $('#menuId').val(menu.id);
        $('#menuName').val(menu.name);
        $("#sort-input").val(menu.sort);
        if (menu.parentId != null) {
            $('#parentModule').val(menu.moduleName);
        } else {
            $('#parentModule').val('无（根菜单）');
        }
        if (menu.state) {
            $('#stateTrue').prop("checked", "checked");
        } else {
            $('#stateFalse').prop("checked", "checked");
        }
        $('#sysCode').val(menu.code);
        $('#menuUrl').val(menu.url);
        $('#Icon-hover').val(menu.icon);
        $('input[data-role="tagsinput"]').tagsinput('removeAll');  
        $('input[data-role="tagsinput"]').tagsinput('add', menu.childUrl); 
    } else {
        $('#menuId').val('');
        $('#menuName').val('');
        $('#stateTrue').prop("checked", "checked");
        $('#parentModule').val('无（根菜单）');
        $('#sysCode').val('');
        $('#menuUrl').val('');
        $('input[data-role="tagsinput"]').tagsinput('removeAll');  
        $('#Icon-hover').val('');
        $("#sort-input").val('');
    }
}