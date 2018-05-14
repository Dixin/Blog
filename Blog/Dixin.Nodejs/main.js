const tumblr = require("./tumblr");

(async () => {
    try {
        const client = await tumblr.getClientAsync({
            // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
            consumerKey: "j6l07pCFuT9aOTWbFncXFi3qgxY2fL8VGyYLfbRIaYzd1WjxjI",
            consumerSecret: "ZFJvpPphkvBrXVc30Tbvy3x5DbIi30n8LqztcRmUUUacYfwQSb"
        });
        await client.downloadLikesAndUnlikeAsync({
            directory: "D:\\User\\Downloads\\Tumblr",
            delay: 1000
        });
        const following = await client.getAllFollowingAsync();
        console.log(following.blogs);
    } catch (error) {
        console.log(error);
    } 
})();
