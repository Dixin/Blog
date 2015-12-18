"use strict";

var tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    userEmail: "dixinyan@live.com",
    userPassword: "ftSq1@zure",
    appConsumerKey: "UIH25zu5hM4i0Mf24qngIwJFvMXH1PehEyJCfhmgAfdTIhbVTj",
    appSecretKey: "jMD4THr9EUfJNkAOvyrNOhNqYJim0fMFakYzNaToG7nAj5ZiQb",
    offset: 5,
    limit: 51,
    directory: "D:\\Tumblr",
    after: 1
});
