(function ($) {
    if (window.$tap) return;
    var $tap = [];

    $tap.tabAdd = function (title, url, id) {
        //新增一个Tab项
        element.tabAdd('xbs_tab', {
            title: title
            , content: '<iframe  name="ww-iframe" tab-id="' + id + '" frameborder="0" src="' + url + '" scrolling="yes" class="x-iframe"></iframe>'
            , id: id
        })
    }
    $tap.tabDelete = function (othis) {
        //删除指定Tab项
        element.tabDelete('xbs_tab', '44'); //删除：“商品管理”


        othis.addClass('layui-btn-disabled');
    }
    $tap.tabChange = function (id) {
        //切换到指定Tab项
        element.tabChange('xbs_tab', id); //切换到：用户管理
    }

    window.$tap = $tap;

})($)