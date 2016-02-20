"use strict";

var path = require("path"),
    util = require("util"),
    Q = require("q"),
    tumblr = require("tumblr-auto-auth"),
    common = require("./common"),

    getClient = function (options) {
        var deferred = Q.defer();
        tumblr.interactiveAuthorization(options, function (error) {
            if (error) {
                deferred.reject(error);
            }
            tumblr.getAuthorizedClient({
                userEmail: options.userEmail,
                userPassword: options.userPassword,
                appConsumerKey: options.appConsumerKey,
                appSecretKey: options.appSecretKey,
                debug: options.debug
            }, function(error, client) {
                if (error) {
                    deferred.reject(error);
                } else {
                    options.client = client;
                    deferred.resolve(options);
                }
            });
        });
        return deferred.promise;
    },

    getLikes = function (options) {
        var deferred = Q.defer();
        options.client.likes({
            limit: options.limit,
            after: options.after
        }, function (error, data) {
            if (error) {
                console.log(error);
                deferred.reject(error);
            } else {
                console.log("Likes: " + data.liked_count);
                options.posts = data.liked_posts;
                options.likesCount = data.liked_count;
                deferred.resolve(options);
            }
        });
        return deferred.promise;
    },

    downloadPost = function (post, directory, getFileName) {
        var downloads = [];
        console.log("Processing " + post.post_url);
        if (post.photos) { // Post has pictures.
            post.photos.forEach(function (photo, index) {
                var url = photo.original_size.url;
                var file = path.join(directory, getFileName(post, url, index));
                downloads.push(common.download(url, file).thenResolve({
                    post: post,
                    url: url,
                    file: file,
                    type: "photo"
                }));
            });
        }
        if (post.video_url) { // Post has videos.
            var url = post.video_url;
            var file = path.join(directory, getFileName(post, url));
            downloads.push(common.download(url, file).thenResolve({
                post: post,
                url: url,
                file: file,
                type: "video"
            }));
        }
        return Q.all(downloads);
    },

    getFileName = function (post, url, index) {
        var summary = post.summary ? common.removeReservedCharactersFromFileName(post.summary).trim() : "",
            extension = url.split(".").pop();
        summary = summary ? " " + summary.substring(0, 30) : "";
        index = index || 0;
        // return `${post.id} ${index}${summary}.${extension}`;
        return post.id + " " + index + summary + "." + extension;
    },

    unlikePost = function (options) {
        var deferred = Q.defer();
        console.log("Unliking post " + options.post.post_url);
        options.client.unlike(options.post.id, options.post.reblog_key, function (error) {
            if (error) {
                deferred.reject(error);
            } else {
                deferred.resolve(options);
            }
        });
        return deferred.promise;
    },

    downloadAllAndUnlike = function (options) {
        if (options.fiddler) {
            common.fiddler();
        }

        getClient(options)// Get tumblr client.
            .then(getLikes)// Get tumblr liked post.
            .then(function (options) {
                if (options.likesCount > 0 && options.posts && options.posts.length > 0) {
                    // If there is any liked post.
                    Q.all(options.posts.map(function (post) { // Download each liked post.
                        return downloadPost(post, options.directory, getFileName).then(function (download) {
                            return unlikePost({ // After downloading all files of the tumblr post, unlike it
                                client: options.client,
                                post: post
                            }).thenResolve(download);
                        });
                    })).then(function (posts) { // After downloading and unliking all tumblr post, log them.
                        if (util.isArray(posts)) {
                            posts.forEach(console.log);
                        } else {
                            console.log(posts);
                        }
                    }, function (errors) { // If there is error, log it.
                        if (util.isArray(errors)) {
                            errors.forEach(console.error);
                        } else {
                            console.error(errors);
                        }
                    }).then(function () {
                        downloadAllAndUnlike(options); // Download gain, recursively.
                    });
                }
                // If there is not any liked post, stop. Recursion terminates.
            });
    };

module.exports = {
    downloadAllAndUnlike: downloadAllAndUnlike
};
