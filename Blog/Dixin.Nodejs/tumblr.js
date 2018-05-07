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
        oAuthClient.getOAuthRequestToken((error, token, tokenSecret) => {
            if (error) {
                reject(error);
            } else {
                const server = http.createServer((request, response) => {
                    const requestUrl = url.parse(request.url);
                    if (requestUrl.pathname === parsedCallbackUrl.pathname) {
                        const query = queryString.parse(requestUrl.query);
                        oAuthClient.getOAuthAccessToken(token, tokenSecret, query.oauth_verifier, (error, accessToken, acessTokenSecret) => {
                            if (error) {
                                reject(error);
                            } else {
                                const client = Promise.promisifyAll(new tumblr.Client({
                                    consumer_key: options.consumerKey,
                                    consumer_secret: options.consumerSecret,
                                    token: accessToken,
                                    token_secret: acessTokenSecret
                                }));
                                if (!client.userInfoAsync) {
                                    reject(new Error("Failed to promisify client."));
                                }
                                client.userInfoAsync().then(data => {
                                    client.accessToken = accessToken;
                                    client.acessTokenSecret = acessTokenSecret;
                                    client.consumerKey = options.consumerKey;
                                    client.consumerSecret = options.consumerSecret;
                                    client.downloadLikesAndUnlikeAsync = downloadLikesAndUnlikeAsync;
                                    console.log(`Auth is done for ${data.user.name}.`);
                                    response.end(`Auth is done for ${data.user.name}.`);
                                    resolve(client);
                                }, reject);
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

    downloadLikesAndUnlikeAsync = async function(options) {
        const delay = options.delay || 1000;
        let likes = await this.userLikesAsync();
        do {
            for (const post of likes.liked_posts) {
                await setTimeoutAsync(delay); // Tumblr has a request rate limit
                await downloadPostMediaAsync(post, options.directory);
                await setTimeoutAsync(delay);
                await this.unlikePostAsync(post.id, post.reblog_key);
            }
            await setTimeoutAsync(delay);
            likes = await this.userLikesAsync();
        } while (likes.liked_posts.length > 0);
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

    getFileName = (post, url, index = 0, count = 150) => {
        const summary = post.summary ? common.removeReservedCharactersFromFileName(post.summary).trim() : "",
            extension = url.split(".").pop();
        return `${post.blog_name} ${post.id} ${index} ${summary ? ` ${summary.substring(0, count)}` : ""}.${extension}`;
    };

module.exports = {
    getClientAsync
};
