"use strict";

var tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    userEmail: "userEmail",
    userPassword: "userPassword",
    // register an application in tumblr: https://www.tumblr.com/oauth/apps.
    appConsumerKey: "appConsumerKey",
    appSecretKey: "appSecretKey",
    offset: 5,
    limit: 51,
    directory: "D:\\Tumblr",
    after: 1
});
