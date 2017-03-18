"use strict";

const path = require("path"),
    url = require("url"),
    util = require("util"),
    http = require("http"),
    queryString = require("querystring"),
    Q = require("q"),
    cheerio = require("cheerio"),
    oAuth = require("oauth"),
    tumblr = require("tumblr.js"),
    common = require("./common"),

    getClient = options => {
        const deferred = Q.defer(),
            callbackUrl = "http://127.0.0.1:34946/tumblr",
            parsedCallbackUrl = url.parse(callbackUrl),
            oAuthClient = new oAuth.OAuth(
                "https://www.tumblr.com/oauth/request_token",
                "https://www.tumblr.com/oauth/access_token",
                options.appConsumerKey,
                options.appSecretKey,
                "1.0A",
                callbackUrl,
                "HMAC-SHA1");
        oAuthClient.getOAuthRequestToken((error, token, tokenSecret) => {
            if (error) {
                deferred.reject(error);
            } else {
                const server = http.createServer((request, response) => {
                    const requestUrl = url.parse(request.url);
                    if (requestUrl.pathname === parsedCallbackUrl.pathname) {
                        const query = queryString.parse(requestUrl.query);
                        oAuthClient.getOAuthAccessToken(token, tokenSecret, query.oauth_verifier, (error, accessToken, acessTokenSecret) => {
                            if (error) {
                                deferred.reject(error);
                            } else {
                                const tumblrClient = new tumblr.Client({
                                    consumer_key: options.appConsumerKey,
                                    consumer_secret: options.appSecretKey,
                                    token: accessToken,
                                    token_secret: acessTokenSecret
                                });
                                tumblrClient.userInfo((error, data) => {
                                    if (error) {
                                        deferred.reject(error);
                                    } else {
                                        console.log(`Auth is done for ${data.user.name}.`);
                                        options.accessToken = accessToken;
                                        options.acessTokenSecret = acessTokenSecret;
                                        options.client = tumblrClient;
                                        deferred.resolve(options);
                                        response.end("Auth is done.");
                                    }
                                });
                            }
                        });
                    } else {
                        response.end("Auth fails.");
                        deferred.reject(new Error("Auth fails."));
                    }
                });

                server.listen(parsedCallbackUrl.port, parsedCallbackUrl.hostname, () => console.log("Waiting for auth."));

                console.log(`Auth URL: http://www.tumblr.com/oauth/authorize?oauth_token=${token}`);
            }
        });
        return deferred.promise;
    },

    getLikes = options => {
        const deferred = Q.defer();
        options.client.userLikes({
            limit: options.limit,
            after: options.after
        }, (error, data) => {
            if (error) {
                console.log(error);
                deferred.reject(error);
            } else {
                console.log(`Likes: ${data.liked_count}`);
                options.posts = data.liked_posts;
                options.likesCount = data.liked_count;
                deferred.resolve(options);
            }
        });
        return deferred.promise;
    },

    downloadPost = (post, directory, getFileName) => {
        const downloads = [];
        console.log(`Processing ${post.post_url}`);
        if (post.photos) { // Post has pictures.
            post.photos.forEach((photo, index) => {
                const url = photo.original_size.url,
                    file = path.join(directory, getFileName(post, url, index));
                downloads.push(common.download(url, file).thenResolve({
                    post: post,
                    url: url,
                    file: file,
                    type: "photo"
                }));
            });
        }
        if (post.video_url) { // Post has videos.
            const url = post.video_url,
                file = path.join(directory, getFileName(post, url));
            downloads.push(common.download(url, file).thenResolve({
                post: post,
                url: url,
                file: file,
                type: "video"
            }));
        }
        return Q.all(downloads);
    },

    getFileName = (post, url, index) => {
        const summary = post.summary ? common.removeReservedCharactersFromFileName(post.summary).trim() : "",
            extension = url.split(".").pop();
        return `${post.blog_name} ${post.id} ${index || 0} ${summary ? ` ${summary.substring(0, 30)}` : ""}.${extension}`;
    },

    unlikePost = options => {
        const deferred = Q.defer();
        console.log(`Unliking post ${options.post.post_url}`);
        options.client.unlikePost(options.post.id, options.post.reblog_key, error => {
            if (error) {
                deferred.reject(error);
            } else {
                deferred.resolve(options);
            }
        });
        return deferred.promise;
    },

    downloadAndUnlike = options => {
        return getLikes(options) // Get tumblr liked post.
            .then(options => {
                if (options.likesCount > 0 && options.posts && options.posts.length > 0) {
                    // If there is any liked post.
                    return Q.all(options.posts.map(post => { // Download each liked post.
                        return downloadPost(post, options.directory, getFileName).then(download => unlikePost({ // After downloading all files of the tumblr post, unlike it
                            client: options.client,
                            post: post
                        }).thenResolve(download));
                    })).then(posts => { // After downloading and unliking all tumblr post, log them.
                        if (util.isArray(posts)) {
                            posts.forEach(console.log);
                        } else {
                            console.log(posts);
                        }
                    }, errors => { // If there is error, log it.
                        if (util.isArray(errors)) {
                            errors.forEach(console.error);
                        } else {
                            console.error(errors);
                        }
                    }).then(() => downloadAndUnlike(options)); // Download gain, recursively.
                } else {
                    return options;
                }
                // If there is not any liked post, stop. Recursion terminates.
            });
    },

    getLikedPostsFromHtml = (options, likedPosts, likesPath, deferred) => {
        likedPosts = likedPosts || [];
        deferred = deferred || Q.defer();
        const likesUrl = `https://www.tumblr.com${likesPath || "/likes"}`;
        const requestOptions = url.parse(likesUrl);
        requestOptions.headers = {
            "accept": 'text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,*/*;q=0.8',
            "accept-language": "en-US,en;q=0.8,zh-CN;q=0.6,zh-TW;q=0.4",
            "cache-control": "max-age=0",
            "cookie": options.cookie,
            "dnt": "1",
            "upgrade-insecure-requests": "1"
        };
        common.downloadString(requestOptions)
            .then(html => {
                const $ = cheerio.load(html);
                $("li.post_container div.post").each(() => {
                    likedPosts.push({
                        blog: $(this).data("tumblelog"),
                        post: $(this).data("post-id")
                    });
                });
                const nextPage = $("#next_page_link");
                if (nextPage.length > 0) {
                    getLikedPostsFromHtml(options, likedPosts, nextPage.prop("href"), deferred);
                } else {
                    deferred.resolve(likedPosts);
                }
            }, deferred.reject);
        return deferred.promise;
    },

    downloadLikesFromHtml = options => {
        return getLikedPostsFromHtml(options).then(likedPosts => {
            console.log(`Posts from HTML: ${likedPosts.length}`);
            Q.all(likedPosts.map(post => {
                const deferred = Q.defer();
                options.client.blogPosts(post.blog + ".tumblr.com", null, { id: post.post }, function (error, data) {
                    if (error) {
                        deferred.reject(error);
                    } else {
                        downloadPost(data.posts[0], options.directory, getFileName).then(function (download) {
                            return unlikePost({ // After downloading all files of the tumblr post, unlike it
                                client: options.client,
                                post: data.posts[0]
                            }).thenResolve(download);
                        }).then(() => deferred.resolve(data.posts[0]), deferred.reject);
                    }
                });
                return deferred.promise;
            })).then(posts => { // After downloading and unliking all tumblr post, log them.
                if (util.isArray(posts)) {
                    posts.forEach(console.log);
                } else {
                    console.log(posts);
                }
            }, errors => { // If there is error, log it.
                if (util.isArray(errors)) {
                    errors.forEach(console.error);
                } else {
                    console.error(errors);
                }
            });
        });
    },

    downloadAllAndUnlike = options => {
        if (options.fiddler) {
            common.fiddler();
        }
        getClient(options)
            .then(() => downloadAndUnlike(options))
            .then(() => downloadLikesFromHtml(options));
    };

module.exports = {
    downloadAllAndUnlike: downloadAllAndUnlike
};
