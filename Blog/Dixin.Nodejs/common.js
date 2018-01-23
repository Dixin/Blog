"use strict";

const http = require("http"),
    https = require("https"),
    fs = require("fs"),
    url = require("url"),
    Q = require("q"),

    getHttpModule = options => {
        if (typeof options === "string") { // options can be URL string.
            options = url.parse(options);
        }
        return options.protocol && options.protocol.toLowerCase() === "https:" ? https : http;
    },

    download = (options, path) => {
        const deferred = Q.defer(),
            file = fs.createWriteStream(path, {
                flags: "w"
            }),
            httpModule = getHttpModule(options);

        console.log(`Downloading ${url.format(options)} to ${path}`);
        httpModule.request(options, response => {
            response.pipe(file);
            file.on("finish", () => file.close(deferred.resolve));
        }).on("error", error => {
            fs.unlink(path);
            deferred.reject(error);
        }).end();
        return deferred.promise;
    },

    downloadString = options => {
        const deferred = Q.defer(),
            httpModule = getHttpModule(options);
        console.log(`Downloading ${url.format(options)} as string.`);
        httpModule.request(options, response => {
            const strings = [];
            response.setEncoding('utf8');
            response.on('data', string => strings.push(string));
            response.on('end', () => deferred.resolve(strings.join()));
        }).on("error", error => deferred.reject(error)).end();
        return deferred.promise;
    },

    // https://en.wikipedia.org/wiki/Filename#Reserved_characters_and_words
    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247.aspx#file_and_directory_names
    removeReservedCharactersFromFileName = fileName => fileName.replace(/[<>:"/\\|?*\x00-\x1F\r\n\t]/g, ""),

    exists = path => {
        const deferred = Q.defer();
        fs.access(path, fs.F_OK, error => {
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
