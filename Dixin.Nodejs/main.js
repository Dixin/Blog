"use strict";

var tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    userEmail: "userEmail",
    userPassword: "userPassword",
    // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
    appConsumerKey: "appConsumerKey",
    appSecretKey: "appSecretKey",
    offset: 5,
    limit: 51,
    directory: "D:\\Dixin\\Downloads\\Tumblr",
    after: 1,
    debug: true,
    fiddler: false
});
