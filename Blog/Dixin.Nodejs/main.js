const tumblr = require("./tumblr-batch");

(async () => {
    try {
        const client = await tumblr.getClientAsync({
            // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
            consumerKey: "",
            consumerSecret: "",
            cookie: ""
        });
        await client.downloadAllLikesAndUnlikeAsync({
            directory: "D:\\User\\Downloads\\Tumblr"
        });
        await client.downloadAllLikesFromHtmlAndUnlikeAsync({
            directory: "D:\\User\\Downloads\\Tumblr"
        });
    } catch (error) {
        console.log(error);
    } 
})();
