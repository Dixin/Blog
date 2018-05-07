const http = require("http"),
    https = require("https"),
    fs = require("fs"),
    url = require("url"),

    getHttpModule = options => {
        if (typeof options === "string") { // options can be URL string.
            options = url.parse(options);
        }
        return options.protocol && options.protocol.toLowerCase() === "https:" ? https : http;
    },

    downloadFileAsync = (options, path) => new Promise((resolve, reject) => {
        const file = fs.createWriteStream(path, {
            flags: "w"
        }),
            httpModule = getHttpModule(options);
        console.log(`Downloading ${url.format(options)} to ${path}.`);
        httpModule.request(options, response => {
            response.pipe(file);
            file.on("finish", () => {
                console.log(`Downloaded ${url.format(options)} to ${path}.`);
                return file.close(resolve);
            });
        }).on("error", error => {
            fs.unlink(path);
            console.log(`Failed to download ${url.format(options)} to ${path}.`);
            reject(error);
        }).end();
    }),

    downloadStringAsync = options => new Promise((resolve, reject) => {
        const httpModule = getHttpModule(options);
        console.log(`Downloading ${url.format(options)} as string.`);
        httpModule.request(options, response => {
            const strings = [];
            response.setEncoding("utf8");
            response.on("data", string => strings.push(string));
            response.on("end", () => {
                console.log(`Downloaded ${url.format(options)} as string.`);
                return resolve(strings.join());
            });
        }).on("error", error => {
            console.log(`Failed to download ${url.format(options)} as string.`);
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

module.exports = {
    downloadAsync: downloadFileAsync,
    downloadStringAsync,
    removeReservedCharactersFromFileName,
    existsAsync
};
