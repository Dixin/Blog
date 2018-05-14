const path = require("path"),
    url = require("url"),
    util = require("util"),
    http = require("http"),
    queryString = require("querystring"),
    cheerio = require("cheerio"),
    oAuth = require("oauth"),
    tumblr = require("tumblr.js"),
    common = require("./common"),
    opn = require("opn"),
    Promise = require("bluebird"),

    setTimeoutAsync = util.promisify(setTimeout),

    getClientAsync = options => new Promise((resolve, reject) => {
        if (options.accessToken && options.acessTokenSecret) {
            buildClient(options, resolve, reject);
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
                                buildClient(options, resolve, reject).then(data => response.end(`Auth is done for ${data.user.name}.`));
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

    buildClient = (options, resolve, reject) => {
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
            client.accessToken = options.accessToken;
            client.acessTokenSecret = options.acessTokenSecret;
            client.consumerKey = options.consumerKey;
            client.consumerSecret = options.consumerSecret;
            client.downloadAllLikesAndUnlikeAsync = downloadAllLikesAndUnlikeAsync;
            client.getAllFollowingAsync = getAllFollowingAsync;
            client.followAllAsync = followAllAsync;
            console.log(`Auth is done for ${data.user.name}.`);
            resolve(client);
            return data;
        }, reject);
    },

    downloadAllLikesAndUnlikeAsync = async function (options) {
        const delay = options.delay || 1000;
        for (let likes = await this.userLikesAsync(); likes.liked_posts.length > 0; likes = await this.userLikesAsync()) {
            for (const post of likes.liked_posts) {
                await setTimeoutAsync(delay); // Tumblr has a request rate limit
                await downloadPostMediaAsync(post, options.directory);
                await setTimeoutAsync(delay);
                await this.unlikePostAsync(post.id, post.reblog_key);
            }
            await setTimeoutAsync(delay);
        }
    },

    getFileName = (post, url, index = 0, count = 150) => {
        const summary = post.summary ? common.removeReservedCharactersFromFileName(post.summary).trim() : "",
            extension = url.split(".").pop();
        return `${post.blog_name} ${post.id} ${index} ${summary ? ` ${summary.substring(0, count)}` : ""}.${extension}`;
    },

    downloadPostMediaAsync = async (post, directory) => {
        console.log(`Processing ${post.post_url}`);
        if (post.photos) { // Post has pictures.
            for (const [index, photo] of post.photos.entries()) {
                const url = photo.original_size.url,
                    file = path.join(directory, getFileName(post, url, index));
                await common.downloadAsync(url, file);
            }
        }
        if (post.video_url) { // Post has videos.
            const url = post.video_url,
                file = path.join(directory, getFileName(post, url));
            await common.downloadAsync(url, file);
        }
        if (post.body) { // Post has HTML.
            const $ = cheerio.load(post.body),
                $images = $("img");
            for (let index = 0; index < $images.length; index++) {
                const image = $images[index],
                    url = image.attribs.src,
                    file = path.join(directory, getFileName(post, url, index));
                await common.downloadAsync(url, file);
            }
        }
    },

    getAllFollowingAsync = async function (options) {
        options = options || {};
        const delay = options.delay || 1000,
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
        const delay = options.delay || 1000,
            blogs = options.blogs || [];
        for (const [index, blog] of blogs.entries()) {
            console.log(`Following ${index} ${blog.name} ${blog.url}.`);
            try {
                this.followBlogAsync({ url: blog.name }).then(() => console.log(`Followed ${index} ${blog.name} ${blog.url}.`), console.log);
                //await this.followBlogAsync({ url: blog.name });
                //await setTimeoutAsync(delay);
            } catch (error) {
                console.log(error);
            }
            await setTimeoutAsync(delay);
        }
    };

module.exports = {
    getClientAsync
};
