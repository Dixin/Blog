"use strict";

const tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
    appConsumerKey: "UIH25zu5hM4i0Mf24qngIwJFvMXH1PehEyJCfhmgAfdTIhbVTj",
    appSecretKey: "jMD4THr9EUfJNkAOvyrNOhNqYJim0fMFakYzNaToG7nAj5ZiQb",
    cookie: "",
    offset: 0,
    limit: 50,
    directory: "D:\\Dixin\\Downloads\\Tumblr",
    after: 0,
    debug: true,
    fiddler: false
});
