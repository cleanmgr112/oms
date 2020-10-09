var connection = new signalR.HubConnectionBuilder().withUrl("/chatHub").build();


connection.start().then(function () {
    console.log("连接成功");
}).then(function () {
    $.ajax({
        url: '/remind/template/warn',
        type: 'get',
    });
}).catch(function (err) {
    return console.error(err.toString());
});


var app = new Vue({
    data() {
        return {
            notifyPromise: Promise.resolve()
        }
    },
    methods: {
        //通知，解决element-ui，同时调用notify时，通知重叠的问题
        notify(msg) {
            this.notifyPromise = this.notifyPromise.then(this.$nextTick).then(() => {
                this.$notify.info({
                    title: '通知',
                    message: msg,
                    duration: 0,
                    dangerouslyUseHTMLString: true
                });
            });
        }

    }
});

connection.on("SendMessage", function (title, message) {
    if (title == "message")
        app.notify(message);
    else if (title == "stock")
        try {
            demo.product();
        } catch (ex) {
            return;
        };

});




