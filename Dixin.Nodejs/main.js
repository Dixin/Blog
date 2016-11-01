"use strict";

const tumblr = require("./tumblr");

tumblr.downloadAllAndUnlike({
    // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
    appConsumerKey: "",
    appSecretKey: "",
    cookie: "",
    offset: 0,
    limit: 50,
    directory: "D:\\Dixin\\Downloads\\Tumblr",
    after: 0,
    debug: true,
    fiddler: false
});
