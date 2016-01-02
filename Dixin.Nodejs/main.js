"use strict";

var tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    userEmail: "dixinyan@live.com",
    userPassword: "ftSq1@zure",
    appConsumerKey: "...",
    appSecretKey: "...",
    offset: 5,
    limit: 51,
    directory: "D:\\Tumblr",
    after: 1
});
