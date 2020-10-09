var UINestable = function () {
    var data = "";
    var updateOutput = function (e) {
        var list = e.length ? e : $(e.target);
        var sort;
        var arr = [];
            var data = list.nestable('serialize');
            for (var i = 0; i < data.length; i++) {
                arr.push(data[i].id);
            }
            sort = arr.join(',');
            var id = parseInt(list.find("sign").text());
            $oms.ajax({
                url: "/B2BOrder/UpdateAPDetailSort",
                data: {
                    id: id,
                    sort: sort
                },
            });
    };
    return {
        //main function to initiate the module
        init: function () {
                // activate Nestable for list 1
                $('.dd').nestable({
                    group:1,
                    maxDepth: 1
                }).on('change', updateOutput);
            }
    };

}();
jQuery(document).ready(function () {
    UINestable.init();
});