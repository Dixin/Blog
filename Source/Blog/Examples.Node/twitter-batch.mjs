import Twit from 'twit';
import http from "http";
import https from "https";
import fs from "fs";
import urlModule from "url";
import path from "path";

const getHttpModule = parsedUrl => parsedUrl.protocol && parsedUrl.protocol.toLowerCase().endsWith("s:") ? https : http,

    getClientAsync = async options => {
        const twit = new Twit({
            consumer_key: options.consumer_key,
            consumer_secret: options.consumer_secret,
            access_token: options.access_token,
            access_token_secret: options.access_token_secret,
            timeout_ms: 60 * 1000,
            strictSSL: true
        });

        const user = await twit.get('account/verify_credentials', { skip_status: true });
        console.log(user.data.name);

        return Object.assign(twit, {
            downloadAllAndUnlikeAsync,
            downloadAllFromHtmlAndUnlikeAsync
        }, options);
    },

    unlikeAsync = async (twit, id) =>
        await twit.post("favorites/destroy", { id: id }),

    // https://en.wikipedia.org/wiki/Filename#Reserved_characters_and_words
    // https://msdn.microsoft.com/en-us/library/windows/desktop/aa365247.aspx#file_and_directory_names
    removeReservedCharactersFromFileName = fileName => fileName.replace(/[<>:"/\\|?*\x00-\x1F\r\n\t]/g, ""),

    downloadAsync = async (twit, like) => {
        if (like.extended_entities && like.extended_entities.media && like.extended_entities.media.length > 0) {
            for (const [index, media] of like.extended_entities.media.entries()) {
                switch (media.type) {
                    case "video":
                        {
                            if (media.video_info && media.video_info.variants) {
                                const videos = media.video_info.variants
                                    .filter(variant => variant.content_type === "video/mp4")
                                    .sort((variant1, variant2) => variant2.bitrate - variant1.bitrate);
                                if (videos.length > 0) {
                                    await downloadFileAsync(videos[0].url, getPath(index, like, videos[0].url, twit.directory));
                                }
                            }
                            return true;
                        }
                    case "photo":
                        {
                            await downloadFileAsync(media.media_url, getPath(index, like, media.media_url, twit.directory));
                            return true;
                        }
                    default:
                        return false;
                }
            }
        }
    },

    getPath = (index, like, url, directory, count = 150) => {
        const summary = removeReservedCharactersFromFileName(like.text.replace("https://t.co/", ""));
        const extension = url.split(".").pop().replace(/\?.*/, "");
        const fileName = `${like.user.screen_name} ${summary.substring(0, count)}.${extension}`;
        return path.join(directory, fileName);
    },

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

    getLikesAsync = async (twit, errors) => {
        console.log("Calling Twitter API favorites/list.");
        const likes = (await twit.get('favorites/list')).data;
        const validLikes = likes.filter(like => !(like.id_str in errors));
        console.log(`Got ${likes.length} likes, ${validLikes.length} likes are valid.`);
        return validLikes;
    },

    downloadAllAndUnlikeAsync = async function () {
        let likes = [];
        const errors = {};
        for (likes = await getLikesAsync(this, errors); likes.length > 0; likes = await getLikesAsync(this, errors)) {
            for (const like of likes) {
                try {
                    if (await downloadAsync(this, like)) {
                        await unlikeAsync(this, like.id_str);
                    } else {
                        errors[like.id_str] = null;
                    }
                }
                catch (error) {
                    console.log(error);
                }
            }
        }
    },

    getLikesFromHtmlAsync = async (twit, errors) => {

    },

    downloadAllFromHtmlAndUnlikeAsync = async function () {
        let likes = [];
        const errors = {};
        for (likes = await getLikesAsync(this, errors); likes.length > 0; likes = await getLikesAsync(this, errors)) {
            for (const like of likes) {
                try {
                    if (await downloadAsync(this, like)) {
                        await unlikeAsync(this, like.id_str);
                    } else {
                        errors[like.id_str] = null;
                    }
                }
                catch (error) {
                    console.log(error);
                }
            }
        }
    };

export default {
    getClientAsync
};