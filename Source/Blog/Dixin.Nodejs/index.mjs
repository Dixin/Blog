import tumblr from "./tumblr-batch";
import twitter from "./twitter-batch";

(async () => {
    try {
        // const tumblrClient = await tumblr.getClientAsync({
        //     // Register an application in tumblr: https://www.tumblr.com/oauth/apps.
        //     consumerKey: "j6l07pCFuT9aOTWbFncXFi3qgxY2fL8VGyYLfbRIaYzd1WjxjI",
        //     consumerSecret: "ZFJvpPphkvBrXVc30Tbvy3x5DbIi30n8LqztcRmUUUacYfwQSb",
        //     cookie: "rxx=27z95fi0ll5.141azcuh&v=1; _ga=GA1.2.789437853.1525537405; __utmc=189990958; OUTFOX_SEARCH_USER_ID_NCOO=1679916652.7974026; language=%2Cen_US; tmgioct=5b35ff7d04a1510701254600; __utmz=189990958.1530394891.19.11.utmcsr=sony.com|utmccn=(referral)|utmcmd=referral|utmcct=/electronics/cyber-shot-compact-cameras/dsc-rx100m6; pfl=YWExNjgxZTRiNDlkODRjODVmNmZiZjI2NjhjMjU5MmNmMTc4MDM5NDhlN2JjNTA2MzcwZTFmMTcyZTFkOTJmOSw3NG4zMmJkNGI3N3JodmZzYzV1ZTVzemFkYmJoZjVoZiwxNTM0Mjk1NDkx; _gid=GA1.2.1359478114.1534295491; __utma=189990958.789437853.1525537405.1530394891.1534295491.20; __utmb=189990958.0.10.1534295491; yx=21tzb91qrdu4p%26o%3D4%26q%3Dq8evuPIcLRV4p72ZzqPvGwgm4Dx-%26f%3Dab%26v%3DAgbal8BmevmpibpyB3mP; ___rl__test__cookies=1534295497807; pfp=RJcJ0sTObCkdl8ZiTiJZNhSy30N7XtFde5l57xc9; pfs=FEkrb8pU3AJZIQ8IyvBWa7xfEI0; pfe=1542071523; pfu=137297581; pfx=19109f75ff00bd09ae26c3f7382fba4b22ec95930c350fb161e3ea5c19f376ea%230%232561823437; logged_in=1; sid=al4E1yy9B9xtr6X8JPmXqlYnmNsI2NWrvX1DVApQsxTTeJNtLA.as3ZaPe0xxy3fauEoG1sDW1HnRJCk8Rpgz4G6qoEfAGCtJ4Xkb; pfg=4b42932aacd3f619973f720fd1a2f7f3079ae5f72f00f0cab19df9a63a6ce4b7%23%7B%22gdpr_is_acceptable_age%22%3A1%2C%22exp%22%3A1565831539%2C%22vc%22%3A%22%22%7D%238998613076; nts=true; devicePixelRatio=1.5; documentWidth=1008; capture=HCgpR2vYoo2QONcS0cRLv59q5Vc"
        // });
        // await tumblrClient.downloadAllLikesAndUnlikeAsync({
        //     directory: "D:\\User\\Downloads\\Tumblr"
        // });
        // await tumblrClient.downloadAllLikesFromHtmlAndUnlikeAsync({
        //     directory: "D:\\User\\Downloads\\Tumblr"
        // });

        const twitterClient = await twitter.getClientAsync({
            consumer_key: 'kwcoGCCOwjXo7EuZd2HaN7xpO',
            consumer_secret: '7gBNvULu20CNMET8IRduA4unP5l9ByrOJo4PwDqc4LIu5thO3H',
            access_token: '4291725083-vlw1k8SyW44yScwCOLkBZY9QWaeriDv2XkRlSO3',
            access_token_secret: '9HMO6KetJoT9e7pyFoVRsOjA9PdIZmw3NdLIgEwXhDhrv',
            directory: "D:\\User\\Downloads\\Twitter"
        });
        await twitterClient.downloadAllAndUnlikeAsync();
    } catch (error) {
        console.log(error);
    }
})();
