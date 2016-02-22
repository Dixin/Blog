"use strict";

var url = require("url"),
    http = require("http"),

    env = process.env,

    proxy = {
        protocol: "http:",
        hostname: "127.0.0.1",
        port: 8888,
    },

    proxyRequests = function () {
        var proxyUrl = url.format(proxy);
        env.http_proxy = proxyUrl;
        env.https_proxy = proxyUrl;
        env.NODE_TLS_REJECT_UNAUTHORIZED = 0;
    },

    unproxyRequests = function () {
        env.http_proxy = "";
        env.https_proxy = "";
        env.NODE_TLS_REJECT_UNAUTHORIZED = "";
    },

    setProxy = function (options) {
        if (typeof options === "string") { // options can be URL string.
            options = url.parse(options);
        }
        if (!options.host && !options.hostname) {
            throw new Error("host or hostname must have value.");
        }
        options.path = url.format(options);
        options.headers = options.headers || {};
        options.headers.Host = options.host || url.format({
            hostname: options.hostname,
            port: options.port
        });
        options.protocol = proxy.protocol;
        options.hostname = proxy.hostname;
        options.port = proxy.port;
        options.href = null;
        options.host = null;
        return options;
    },

    request = function (options, callback) {
        options = setProxy(options);
        return http.request(options, callback);
    },
    
    get = function(options, callback) {
        options = setProxy(options);
        return http.get(options, callback);
    };

module.exports = {
    proxy: proxy,
    proxyRequests: proxyRequests,
    unproxyRequests: unproxyRequests,
    setProxy: setProxy,
    request: request,
    get: get
};
