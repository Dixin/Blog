"use strict";

var http = require("http"),
    fs = require("fs"),
    Q = require("q"),

    download = function (url, path) {
        var deferred = Q.defer(),
            file = fs.createWriteStream(path);
        console.log("Downloading " + url + "to " + path);
        http.get(url, function (response) {
            response.pipe(file);
            file.on("finish", function () {
                file.close(deferred.resolve);
            });
        }).on("error", function (error) {
            fs.unlink(path);
            deferred.reject(error);
        });
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
    removeReservedCharactersFromFileName: removeReservedCharactersFromFileName,
    exists: exists
};
