namespace Dixin.IO
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;

    using Dixin.Common;

    internal static class Music
    {
        private static readonly Dictionary<string, string> Translation = new Dictionary<string, string>()
        {
            ["Alternative-Rock"] = "另类摇滚",
            ["Anime"] = "儿歌",
            ["Classical"] = "古典",
            ["Dream Pop"] = "梦幻流行",
            ["Electronic"] = "电子",
            ["Instrumental"] = "乐器",
            ["Pop"] = "流行",
            ["Rock"] = "摇滚",
            ["Soundtrack"] = "电影原声",
            ["Trailer"] = "预告片",
            ["Indie"]= "独立",

            ["Coldplay"] = "酷玩",
            ["Oku Hanako"] = "奥华子",
            ["Sarah Brightman"] = "莎拉布莱曼",
            ["Secret Garden"] = "神秘园",
            ["Yanni"] = "雅尼",
            ["Andy Lau"] = "刘德华",
            ["Cheer Chen"] = "陈绮贞",
            ["Faye Wong"] = "王菲",
            ["Fiona Fung"] = "冯曦妤",
            ["Jacky Cheung"] = "张学友",
            ["Jay Chou"] = "周杰伦",
            ["Junlin Wu"] = "伍佰",
            ["Peggy Hsu"] = "许哲佩",
            ["The Eagles"] = "老鹰乐队",
            ["Westlife"] = "西城男孩",
            ["Yunpeng Zhou"] = "周云蓬",
            ["Adele"] = "阿黛尔",
            ["Shin"] = "信乐团",
            ["Tang Dynasty"] = "唐朝",
            ["Various"] = "群星",
            
            ["The Avengers"] = "复仇者联盟",
            ["Alien Vs Predator 2 Requiem"] = "异形大战铁血战士2",
            ["Iron Man 3"] = "钢铁侠3",
            ["The Terror Live"] = "恐怖直播",
            ["Resident Evil 3 Extinction"] = "生化危机3",
            ["Casino Royale"] = "007皇家赌场",
            ["Godzilla"] = "哥斯拉",
            ["Quantum Of Solace"] = "007量子危机",
            ["The Lion King"] = "狮子王",
            ["The Good, the Bad and the Ugly"] = "黄金三镖客",
            ["Saw"] = "电锯惊魂",
            ["The Dark Knight"] = "蝙蝠侠1",
            ["The Rock"] = "石破天惊",
            ["The Lion King 2 Rhythm Of The Pride Lands"] = "狮子王2",
            ["The Lion King 3"] = "狮子王3",
            ["Black Hawk Down"] = "黑鹰坠落",
            ["Gladiator"] = "角斗士",
            ["Pirates Of The Caribbean 2 Dead Man's Chest"] = "加勒比海盗2",
            ["The Da Vinci Code"] = "达芬奇密码",
            ["Pirates Of The Caribbean 3"] = "加勒比海盗3",
            ["Call Of Duty Modern Warfare 2 Complete Score"] = "使命召唤",
            ["Sherlock Holmes"] = "福尔摩斯",
            ["Call Of Duty Modern Warfare 2"] = "现代战争",
            ["Inception"] = "盗梦空间",
            ["Pirates Of The Caribbean 4 On Stranger Tides"] = "加勒比海盗4",
            ["The Dark Knight Rises"] = "蝙蝠侠3",
            ["Alien Vs Predator"] = "营销大战铁血战士",
            ["The Day After Tomorrow"] = "后天",
            ["Lord Of The Rings 1"] = "指环王1",
            ["Lord Of The Rings 2"] = "指环王2",
            ["Lord Of The Rings 3"] = "指环王3",
            ["Titanic"] = "泰坦尼克号",
            ["Avatar"] = "阿凡达",
            ["Blood Diamond"] = "血钻",
            ["Green Snake"] = "青蛇",
            ["World of Warcraft"] = "魔兽世界",
            ["Secret"] = "不能说的秘密",
            ["Iron Man 2"] = "钢铁侠2",
            ["The Bourne Identity"] = "谍影重重1",
            ["The Bourne Supremacy"] = "谍影重重2",
            ["The Bourne Ultimatum"] = "谍影重重3",
            ["Schindler's List"] = "辛德勒名单",
            ["Avalon"] = "阿瓦隆",
            ["Pirates Of The Caribbean"] = "加勒比海盗1",
            ["Infernal Affairs"] = "无间道",
            ["Bodyguards and Assassins"] = "十月围城",
            ["Breathe"] = "呼吸",
            ["Mission Impossible 2"] = "碟中谍2",
            ["Transformers The Album"] = "变形金刚1",
            ["Transformers 2 The Album"] = "变形金刚2",
            ["Transformers 3 The Album"] = "变形金刚3",
            ["The Legend of the Condor Heroes"] = "射雕英雄传",
            ["Oblivion"] = "遗落战境",
            ["Saw 2"] = "电锯惊魂2",
            ["Tropa de Elite"] = "精英部队",
            ["Mission Impossible 4 Ghost Protocol"] = "碟中谍4",
            ["Kill Bill Volume 1"] = "杀死比尔1",
            ["The Three Musketeers"] = "三个火枪手",
            ["Dredd"] = "特警判官",
            ["Prison Break"] = "越狱",
            ["Iron Man"] = "钢铁侠",
            ["Clash Of The Titans"] = "诸神之战",
            ["Pacific Rim"] = "环太平洋",
            ["Seediq Bale"] = "赛德克巴莱",
            ["The Last Emperor"] = "末代皇帝",
            ["Resident Evil"] = "生化危机",
            ["Resident Evil 2 Apocalypse"] = "生化危机2",
            ["The Island"] = "逃出克隆岛",
            ["Transformers Promotional Score"] = "变形金刚原声大碟",
            ["Transformers The Score"] = "变形金刚原声1",
            ["Transformers 2 The Score"] = "变形金刚原声2",
            ["Transformers 3 The Score"] = "变形金刚原声3",
            ["Transformers 4 The Score"] = "变形金刚原声4",
            ["Gravity"] = "地心引力",
            ["Crouching Tiger,Hidden Dragon"] = "卧虎藏龙",
            ["X-Men First Class"] = "X战警",
            ["Mad Max Fury Road"] = "疯狂的麦克斯",
            ["Naruto"] = "火影忍者",
            ["National Treasure"] = "国家宝藏",
            ["300"] = "斯巴达三百勇士",
            ["Gangs of New York"] = "纽约黑帮",
            ["Conquest Of Paradise"] = "哥伦布传",
            ["Chariots Of Fire"] = "烈火战车",
            ["Armageddon"] = "世界末日",
            ["Young And Dangerous"] = "古惑仔",
            ["Bad Company"] = "临时特工",
            ["Kill Bill Volume 2"] = "杀死比尔2",
            ["Need For Speed"] = "极品飞车",
            ["Saw 3"] = "电锯惊魂3",
            ["Cape No 7"] = "海角七号",
            ["The Great Gatsby"] = "了不起的盖茨比",
            ["Fast And Furious 7"] = "速度与激情7",
            ["Hotel Rwanda"] = "卢旺达饭店",
            ["All About Lily Chou-Chou"] = "关于莉莉周的一切",
            ["The Punisher"]="惩罚者"
        };

        private const string Separater = ".";

        private static readonly HashSet<string> Extensions = new HashSet<string>(
            new[] { ".mp3", ".m4a", ".wma" }, StringComparer.OrdinalIgnoreCase);

        internal static void RenameAlbum(string from, string to)
        {
            DirectoryInfo fromDirectory = new DirectoryInfo(from);
            DirectoryInfo toDirectory = new DirectoryInfo(to);

            bool hasError = false;

            fromDirectory
                .EnumerateFiles()
                .Where(song => IsMusicFile(song.Extension) && IsNotFormated(song.Name))
                .ForEach(song =>
                    {
                        Trace.WriteLine(song.Name);
                        hasError = true;
                    });

            if (hasError)
            {
                throw new OperationCanceledException();
            }

            fromDirectory
                .EnumerateFiles()
                .ForEach(song =>
                    {
                        if (!IsMusicFile(song.Extension))
                        {
                            return;
                        }

                        string[] names = song.Name.Split(Separater.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                        string year = names[0];
                        string albumName = names[1];
                        string artistName = names[3];
                        string genre = names[5];

                        string newAlbumName = $"{genre}{Separater}{artistName}{Separater}{year}{Separater}{albumName}";
                        DirectoryInfo newAlbum = new DirectoryInfo(Path.Combine(toDirectory.FullName, newAlbumName));
                        if (!newAlbum.Exists)
                        {
                            newAlbum.Create();
                        }

                        song.MoveTo(Path.Combine(newAlbum.FullName, song.Name));
                    });
        }

        internal static void RenameAllAlbums(string from, string to)
        {
            bool hasError = false;
            DirectoryInfo music = new DirectoryInfo(from);

            music
                .EnumerateDirectories()
                .SelectMany(artist => artist.EnumerateDirectories())
                .SelectMany(album => album.EnumerateFiles())
                .Where(song => IsMusicFile(song.Extension) && IsNotFormated(song.Name))
                .ForEach(song =>
                    {
                        Trace.WriteLine(song.Name);
                        hasError = true;
                    });

            if (hasError)
            {
                return;
            }

            music
                .EnumerateDirectories()
                .SelectMany(artist => artist.EnumerateDirectories())
                .ForEach(album =>
                    {
                        IEnumerable<FileInfo> songs = album.EnumerateFiles()
                                .Where(song => IsMusicFile(song.Extension));
                        if (songs.IsEmpty())
                        {
                            Trace.WriteLine(album.Name);
                        }
                        else
                        {
                            string[] names = songs.First().Name.Split(Separater.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                            string artistName = names[3];
                            string year = names[0];
                            string albumName = names[1];
                            string genre = names[5];
                            if (string.IsNullOrWhiteSpace(albumName) || string.IsNullOrWhiteSpace(year)
                                || string.IsNullOrWhiteSpace(artistName))
                            {
                                Trace.WriteLine(album.Name);
                            }
                            else
                            {
                                string newAlbumName = $"{genre}{Separater}{artistName}{Separater}{year}{Separater}{albumName}";
                                if (!album.Name.EqualsIgnoreCase(newAlbumName))
                                {
                                    album.Rename(newAlbumName);
                                }

                                DirectoryInfo newAlbum = new DirectoryInfo(Path.Combine(album.Parent.FullName, newAlbumName));
                                newAlbum.MoveTo(Path.Combine(to, newAlbumName));
                            }
                        }
                    });
        }

        internal static void Translate(string music)
        {
            Directory.EnumerateDirectories(music).ForEach(album =>
                {
                    string[] names = Path.GetFileName(album)
                        .Split(Separater.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
                    for (int index = 0; index < names.Length; index++)
                    {
                        string name = names[index];
                        if (Translation.ContainsKey(name))
                        {
                            string translation = Translation[name];
                            if (!string.IsNullOrWhiteSpace(translation))
                            {
                                names[index] = translation;
                            }
                        }
                    }
                    new DirectoryInfo(album).TryRename(string.Join(Separater, names));
                });
        }

        private static bool IsMusicFile(string extension) => Extensions.Contains(extension);

        private static bool IsNotFormated(string fileName)
        {
            string[] names = fileName.Split(Separater.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            return names.Length != 7
                || names[0].Length != 4
                || (names[2].Length != 2 && names[2].Length != 3 && names.Any(string.IsNullOrWhiteSpace));
        }
    }
}