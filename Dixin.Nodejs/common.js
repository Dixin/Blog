"use strict";

var http = require("http"),
    https = require("https"),
    fs = require("fs"),
    url = require("url"),
    Q = require("q"),

    getHttpModule = function (options) {
        if (typeof options === "string") { // options can be URL string.
            options = url.parse(options);
        }
        return options.protocol && options.protocol.toLowerCase() === "https:" ? https : http;
    },

    download = function (options, path) {
        var deferred = Q.defer(),
            file = fs.createWriteStream(path, {
                flags: "w"
            }),
            httpModule = getHttpModule(options);

        console.log("Downloading " + url.format(options) + " to " + path);
        httpModule.request(options, function (response) {
            response.pipe(file);
            file.on("finish", function () {
                file.close(deferred.resolve);
            });
        }).on("error", function (error) {
            fs.unlink(path);
            deferred.reject(error);
        }).end();
        return deferred.promise;
    },

    downloadString = function(options) {
        var deferred = Q.defer(),
            httpModule = getHttpModule(options);
        console.log("Downloading " + url.format(options) + " as string.");
        httpModule.request(options, function (response) {
            var strings = [];
            response.setEncoding('utf8');
            response.on('data', function (string) {
                strings.push(string);
            });
            response.on('end', function () {
                deferred.resolve(strings.join());
            });
        }).on("error", function (error) {
            deferred.reject(error);
        }).end();
        return deferred.promise;
    },

    removeReservedCharactersFromFileName = function (fileName) {
        // https://en.wikipedia.org/wiki/Filename#Reserved_characters_and_words
        // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247.aspx#file_and_directory_names
        return fileName.replace(/[<>:"/\\|?*\x00-\x1F\r\n\t]/g, "");
    },

    exists = function (path) {
        var deferred = Q.defer();
        fs.access(path, fs.F_OK, function (error) {
            if (error) {
                deferred.reject(error);
            } else {
                deferred.resolve(path);
            }
        });
        return deferred.promise;
    };

module.exports = {
    download: download,
    downloadString: downloadString,
    removeReservedCharactersFromFileName: removeReservedCharactersFromFileName,
    exists: exists
};
