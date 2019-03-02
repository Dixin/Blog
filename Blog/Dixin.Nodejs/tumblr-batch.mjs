import path from "path";
import url from "url";
import util from "util";
import http from "http";
import queryString from "querystring";
import cheerio from "cheerio";
import oAuth from "oauth";
import tumblr from "tumblr.js";
import io from "./io";
import opn from "opn";
import Promise from "bluebird";

const setTimeoutAsync = util.promisify(setTimeout),

    getClientAsync = options => new Promise((resolve, reject) => {
        if (options.accessToken && options.acessTokenSecret) {
            buildBatchClient(options, resolve, reject);
            return;
        }
        const callbackUrl = "http://127.0.0.1:34946/tumblr",
            parsedCallbackUrl = url.parse(callbackUrl),
            oAuthClient = new oAuth.OAuth(
                "https://www.tumblr.com/oauth/request_token",
                "https://www.tumblr.com/oauth/access_token",
                options.consumerKey,
                options.consumerSecret,
                "1.0A",
                callbackUrl,
                "HMAC-SHA1");
        console.log("Requesting OAuth request token.");
        oAuthClient.getOAuthRequestToken((error, token, tokenSecret) => {
            if (error) {
                reject(error);
            } else {
                const server = http.createServer((request, response) => {
                    const requestUrl = url.parse(request.url);
                    if (requestUrl.pathname === parsedCallbackUrl.pathname) {
                        const query = queryString.parse(requestUrl.query);
                        console.log(`Requested OAuth verifier ${query.oauth_verifier}.`);
                        console.log("Requesting OAuth access token.");
                        oAuthClient.getOAuthAccessToken(token, tokenSecret, query.oauth_verifier, (error, accessToken, acessTokenSecret) => {
                            if (error) {
                                reject(error);
                            } else {
                                console.log(`Requested OAuth access token ${accessToken}, secrete ${acessTokenSecret}.`);
                                options.accessToken = accessToken;
                                options.acessTokenSecret = acessTokenSecret;
                                buildBatchClient(options, resolve, reject).then(data => response.end(`Auth is done for ${data.user.name}.`));
                            }
                        });
                    } else {
                        response.end("Auth fails.");
                        reject(new Error("Auth fails."));
                    }
                });

                server.listen(parsedCallbackUrl.port, parsedCallbackUrl.hostname, () => console.log("Waiting for auth."));

                const authUrl = `http://www.tumblr.com/oauth/authorize?oauth_token=${token}`;
                console.log(`Auth URL: ${authUrl}`);
                opn(authUrl);
            }
        });
    }),

    buildBatchClient = (options, resolve, reject) => {
        const client = Promise.promisifyAll(new tumblr.Client({
            consumer_key: options.consumerKey,
            consumer_secret: options.consumerSecret,
            token: options.accessToken,
            token_secret: options.acessTokenSecret
        }));
        if (!client.userInfoAsync) {
            reject(new Error("Failed to promisify client."));
        }
        return client.userInfoAsync().then(data => {
            Object.assign(client, {
                getLikedPosts,
                downloadAllLikesAndUnlikeAsync,
                getAllFollowingAsync,
                followAllAsync,
                defaultDelay: 100,
                downloadAllLikesFromHtmlAndUnlikeAsync,
                getLikedPostsFromHtmlAsync
            }, options);
            console.log(`Auth is done for ${data.user.name}.`);
            resolve(client);
            return data;
        }, reject);
    },

    getLikedPosts = async function (errors) {
        return (await this.userLikesAsync()).liked_posts.filter(value => !(value.id in errors));
    },

    downloadAllLikesAndUnlikeAsync = async function (options) {
        const delay = options.delay || this.defaultDelay,
            errors = {};
        for (let posts = await this.getLikedPosts(errors); posts.length > 0; posts = await this.getLikedPosts(errors)) {
            for (const post of posts) {
                await setTimeoutAsync(delay); // Tumblr has a request rate limit.
                try {
                    await downloadPostMediaAsync(post, options.directory);
                    await this.unlikePostAsync(post.id, post.reblog_key);
                } catch (error) {
                    console.log(error);
                    errors[post.id] = null;
                }
            }
            await setTimeoutAsync(delay);
        }
    },

    getFileName = (post, url, index = 0, count = 150) => {
        const summary = post.summary ? io.removeReservedCharactersFromFileName(post.summary).trim() : "",
            extension = url.split(".").pop();
        return `${post.blog_name} ${post.id} ${index} ${summary ? ` ${summary.substring(0, count)}` : ""}.${extension}`;
    },

    downloadPostMediaAsync = async (post, directory) => {
        console.log(`Processing ${post.post_url}`);
        if (post.photos) { // Post has pictures.
            for (const [index, photo] of post.photos.entries()) {
                const url = photo.original_size.url,
                    file = path.join(directory, getFileName(post, url, index));
                await io.downloadFileAsync(url, file);
            }
        }
        if (post.video_url) { // Post has videos.
            const url = post.video_url,
                file = path.join(directory, getFileName(post, url));
            await io.downloadFileAsync(url, file);
        }
        if (post.body) { // Post has HTML.
            const $ = cheerio.load(post.body),
                $images = $("img");
            for (let index = 0; index < $images.length; index++) {
                const image = $images[index],
                    url = image.attribs.src,
                    file = path.join(directory, getFileName(post, url, index));
                await io.downloadFileAsync(url, file);
            }
        }
    },

    getAllFollowingAsync = async function (options) {
        options = options || {};
        const delay = options.delay || this.defaultDelay,
            offset = options.offset || 20;
        let index = 0,
            blogs = [];
        console.log(`Requesting folllowing blogs ${index} to ${index + offset}`);
        for (let following = await this.userFollowingAsync({ offset: index }); following.total_blogs >= index; index += offset, following = await this.userFollowingAsync({ offset: index })) {
            blogs = blogs.concat(following.blogs.map(blog => ({ name: blog.name, url: blog.url })));
            await setTimeoutAsync(delay);
            console.log(`Requested folllowing blogs ${index} to ${index + offset}`);
        }
        return blogs;
    },

    followAllAsync = async function (options) {
        options = options || {};
        const delay = options.delay || this.defaultDelay,
            blogs = options.blogs || [];
        for (const [index, blog] of blogs.entries()) {
            try {
                await this.followBlogAsync({ url: blog.name });
                console.log(`Followed ${index} ${blog.name} ${blog.url}.`);
                await setTimeoutAsync(delay);
            } catch (error) {
                console.log(error);
            }
            await setTimeoutAsync(delay);
        }
    },

    getLikedPostsFromHtmlAsync = async function (options) {
        const likedPosts = [],
            headers = {
                "accept": 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
                "accept-language": "en-US,en;q=0.8,zh-CN;q=0.6,zh-TW;q=0.4",
                "cache-control": "max-age=0",
                "cookie": options.cookie,
                "dnt": "1",
                "upgrade-insecure-requests": "1"
            },
            delay = options.delay || this.defaultDelay;
        let likesPath = "/likes";
        while (true) {
            const requestOptions = Object.assign({ headers }, url.parse(`https://www.tumblr.com${likesPath}`));
            let html;
            try {
                html = await io.downloadStringAsync(requestOptions);
            } catch (error) {
                console.log(`Failed to download likes from HtML: ${error}`);
                break;
            }
            const $ = cheerio.load(html),
                $nextPage = $("#next_page_link");
            $("ol li div.post").each((index, element) => {
                const $element = $(element);
                likedPosts.push({
                    tumblelog: $element.data("tumblelog"),
                    id: $element.data("id")
                });
            });
            if ($nextPage.length <= 0) {
                break;
            }
            likesPath = $nextPage.prop("href");
            await setTimeoutAsync(delay);
        }
        return likedPosts;
    },

    downloadAllLikesFromHtmlAndUnlikeAsync = async function (options) {
        options.cookie = this.cookie;
        const likedPosts = await this.getLikedPostsFromHtmlAsync(options);
        console.log(`Posts from HTML: ${likedPosts.length}`);

        for (const likedPost of likedPosts) {
            try {
                const data = await this.blogPostsAsync(likedPost.tumblelog + ".tumblr.com", null, { id: likedPost.id }),
                    post = data.posts[0];
                await downloadPostMediaAsync(post, options.directory);
                await this.unlikePostAsync(post.id, post.reblog_key);
                console.log(`Downloaded and unliked post ${likedPost.blog}.tumblr.com/posts/${likedPost.post}`);
            } catch (error) {
                console.error(error);
            }
        }
    };

export default {
    getClientAsync
};
