import http from "http";
import https from "https";
import fs from "fs";
import urlModule from "url";

const getHttpModule = parsedUrl => parsedUrl.protocol && parsedUrl.protocol.toLowerCase().endsWith("s:") ? https : http,

    downloadFileAsync = (url, path) => new Promise((resolve, reject) => {
        if (typeof url === "string") {
            url = urlModule.parse(url);
        }
        const file = fs.createWriteStream(path, {
            flags: "w"
        }),
            httpModule = getHttpModule(url);
        console.log(`Downloading ${urlModule.format(url)} to ${path}.`);
        httpModule.request(url, response => {
            response.pipe(file);
            file.on("finish", () => {
                console.log(`Downloaded ${urlModule.format(url)} to ${path}.`);
                return file.close(resolve);
            });
        }).on("error", error => {
            fs.unlink(path);
            console.log(`Failed to download ${urlModule.format(url)} to ${path}.`);
            reject(error);
        }).end();
    }),

    downloadStringAsync = url => new Promise((resolve, reject) => {
        if (typeof url === "string") {
            url = urlModule.parse(url);
        }
        const httpModule = getHttpModule(url);
        console.log(`Downloading ${urlModule.format(url)} as string.`);
        httpModule.request(url, response => {
            const strings = [];
            response.setEncoding("utf8");
            response.on("data", string => strings.push(string));
            response.on("end", () => {
                console.log(`Downloaded ${urlModule.format(url)} as string.`);
                return resolve(strings.join());
            });
        }).on("error", error => {
            console.log(`Failed to download ${urlModule.format(url)} as string.`);
            return reject(error);
        }).end();
    }),

    // https://en.wikipedia.org/wiki/Filename#Reserved_characters_and_words
    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247.aspx#file_and_directory_names
    removeReservedCharactersFromFileName = fileName => fileName.replace(/[<>:"/\\|?*\x00-\x1F\r\n\t]/g, ""),

    existsAsync = path => new Promise((resolve, reject) => {
        fs.access(path, fs.F_OK, error => {
            if (error) {
                reject(error);
            } else {
                resolve(path);
            }
        });
    });

export default {
    downloadFileAsync,
    downloadStringAsync,
    removeReservedCharactersFromFileName,
    existsAsync
};
