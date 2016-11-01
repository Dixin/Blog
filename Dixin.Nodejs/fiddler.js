"use strict";

const url = require("url"),
    http = require("http"),

    env = process.env,

    proxy = {
        protocol: "http:",
        hostname: "127.0.0.1",
        port: 8888,
    },

    proxyRequests = () => {
        env.http_proxy = env.https_proxy = url.format(proxy);
        env.NODE_TLS_REJECT_UNAUTHORIZED = 0;
    },

    unproxyRequests = () => {
        env.http_proxy = env.https_proxy = "";
        env.NODE_TLS_REJECT_UNAUTHORIZED = "";
    },

    setProxy = options => {
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

    request = (options, callback) => http.request(setProxy(options), callback),
    
    get = (options, callback) => http.get(setProxy(options), callback);

module.exports = {
    proxy: proxy,
    proxyRequests: proxyRequests,
    unproxyRequests: unproxyRequests,
    setProxy: setProxy,
    request: request,
    get: get
};
