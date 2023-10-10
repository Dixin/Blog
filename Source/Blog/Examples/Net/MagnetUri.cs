namespace Examples.Net;

using System.Collections.Immutable;
using System.Web;
using Examples.Common;

public record MagnetUri(string ExactTopic, string DisplayName, string[] Trackers) : ISimpleParsable<MagnetUri>
{
    private const int HashLength = 40;

    private static readonly Regex UriRegex = new(@$"magnet\:\?xt\=urn\:btih\:([A-Za-z0-9]{{{HashLength}}})(\&dn\=([^\&]+))?(\&tr\=([^\&]+))*", RegexOptions.IgnoreCase);

    private static readonly Regex TrackersRegex = new(@"&tr\=([^\&]+)", RegexOptions.IgnoreCase);

    public override string ToString() => $"magnet:?xt=urn:btih:{this.ExactTopic}{(this.DisplayName.IsNullOrWhiteSpace() ? string.Empty : $"&dn={HttpUtility.UrlEncode(this.DisplayName)}")}{string.Join(string.Empty, this.Trackers.Select(tracker => $"&tr={HttpUtility.UrlEncode(tracker)}"))}";

    public static MagnetUri Parse(string value) =>
        TryParse(value, out MagnetUri? result) ? result : throw new ArgumentOutOfRangeException(nameof(value), value, "Input is invalid.");

    public static bool TryParse([NotNullWhen(true)] string? value, [NotNullWhen(true)] out MagnetUri? result)
    {
        Match match = UriRegex.Match(value);
        if (!match.Success)
        {
            result = null;
            return false;
        }

        string[] trackers = TrackersRegex
            .Matches(value)
            .Where(trackerMatch => trackerMatch.Success)
            .Select(trackerMatch => HttpUtility.UrlDecode(trackerMatch.Groups[1].Value))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        result = new(match.Groups[1].Value.ToUpperInvariant(),  HttpUtility.UrlDecode(match.Groups[3].Value), trackers);
        return true;
    }

    public virtual bool Equals(MagnetUri? other) => this.ExactTopic.EqualsOrdinal(other?.ExactTopic);

    public override int GetHashCode() => this.ExactTopic.GetHashCode();

    public MagnetUri AddDefaultTrackers() => this with
    {
        Trackers = this.Trackers.Union(DefaultTrackers, StringComparer.OrdinalIgnoreCase).ToArray()
    };

    public static ImmutableArray<string> DefaultTrackers { get; } = ImmutableArray.Create(
        "https://1337.abcvg.info:443/announce",
        "http://1337.abcvg.info:80/announce",
        "udp://1c.premierzal.ru:6969/announce",
        "udp://6.pocketnet.app:6969/announce",
        "udp://6ahddutb1ucc3cp.ru:6969/announce",
        "*udp://9.rarbg.me:2710/announce",
        "*udp://9.rarbg.me:2720/announce",
        "*udp://9.rarbg.me:2730/announce",
        "*udp://9.rarbg.me:2740/announce",
        "*udp://9.rarbg.me:2770/announce",
        "*udp://9.rarbg.me:2790/announce",
        "*udp://9.rarbg.me:2930/announce",
        "*udp://9.rarbg.me:2980/announce",
        "*udp://9.rarbg.to:2710/announce",
        "*udp://9.rarbg.to:2720/announce",
        "*udp://9.rarbg.to:2730/announce",
        "*udp://9.rarbg.to:2740/announce",
        "*udp://9.rarbg.to:2770/announce",
        "*udp://9.rarbg.to:2790/announce",
        "*udp://9.rarbg.to:2930/announce",
        "*udp://9.rarbg.to:2980/announce",
        "udp://aarsen.me:6969/announce",
        "udp://acxx.de:6969/announce",
        "udp://aegir.sexy:6969/announce",
        "udp://boysbitte.be:6969/announce",
        "http://bt.endpot.com:80/announce",
        "udp://bt.ktrackers.com:6666/announce",
        "udp://bt1.archive.org:6969/announce",
        "udp://bt2.archive.org:6969/announce",
        "udp://carr.codes:6969/announce",
        "udp://concen.org:6969/announce",
        "udp://epider.me:6969/announce",
        "udp://exodus.desync.com:6969/announce",
        "udp://explodie.org:6969/announce",
        "udp://fh2.cmp-gaming.com:6969/announce",
        "udp://free.publictracker.xyz:6969/announce",
        "udp://freedomalternative.com:6969/announce",
        "udp://htz3.noho.st:6969/announce",
        "http://incine.ru:6969/announce",
        "udp://isk.richardsw.club:6969/announce",
        "udp://lloria.fr:6969/announce",
        "udp://mail.artixlinux.org:6969/announce",
        "udp://mail.segso.net:6969/announce",
        "udp://moonburrow.club:6969/announce",
        "udp://movies.zsw.ca:6969/announce",
        "udp://netmap.top:6969/announce",
        "udp://new-line.net:6969/announce",
        "udp://oh.fuuuuuck.com:6969/announce",
        "http://open.acgnxtracker.com:80/announce",
        "http://open.acgtracker.com:1096/announce",
        "udp://open.demonii.com:1337/announce",
        "udp://open.dstud.io:6969/announce",
        "udp://open.stealth.si:80/announce",
        "udp://open.u-p.pw:6969/announce",
        "udp://opentracker.i2p.rocks:6969/announce",
        "udp://opentracker.io:6969/announce",
        "udp://p4p.arenabg.com:1337/announce",
        "udp://private.anonseed.com:6969/announce",
        "udp://psyco.fr:6969/announce",
        "udp://public.publictracker.xyz:6969/announce",
        "udp://public-tracker.cf:6969/announce",
        "udp://retracker01-msk-virt.corbina.net:80/announce",
        "udp://run.publictracker.xyz:6969/announce",
        "udp://run-2.publictracker.xyz:6969/announce",
        "udp://ryjer.com:6969/announce",
        "udp://sanincode.com:6969/announce",
        "http://t.acg.rip:6699/announce",
        "https://t1.hloli.org:443/announce",
        "udp://tamas3.ynh.fr:6969/announce",
        "udp://thinking.duckdns.org:6969/announce",
        "udp://thouvenin.cloud:6969/announce",
        "udp://tk1.trackerservers.com:8080/announce",
        "udp://torrents.artixlinux.org:6969/announce",
        "https://tr.burnabyhighstar.com:443/announce",
        "udp://tracker.0x7c0.com:6969/announce",
        "udp://tracker.4.babico.name.tr:3131/announce",
        "udp://tracker.artixlinux.org:6969/announce",
        "http://tracker.bt4g.com:2095/announce",
        "udp://tracker.ccp.ovh:6969/announce",
        "https://tracker.cloudit.top:443/announce",
        "udp://tracker.cubonegro.lol:6969/announce",
        "udp://tracker.ddunlimited.net:6969/announce",
        "http://tracker.dler.org:6969/announce",
        "udp://tracker.dler.org:6969/announce",
        "https://tracker.expli.top:443/announce",
        "*udp://tracker.fatkhoala.org:13760/announce",
        "http://tracker.files.fm:6969/announce",
        "https://tracker.gbitt.info:443/announce",
        "http://tracker.gbitt.info:80/announce",
        "https://tracker.ipfsscan.io:443/announce",
        "udp://tracker.leech.ie:1337/announce",
        "https://tracker.lilithraws.org:443/announce",
        "https://tracker.loligirl.cn:443/announce",
        "https://tracker.moeblog.cn:443/announce",
        "http://tracker.mywaifu.best:6969/announce",
        "udp://tracker.openbittorrent.com:6969/announce",
        "http://tracker.openbittorrent.com:80/announce",
        "http://tracker.opentrackr.org:1337/announce",
        "udp://tracker.opentrackr.org:1337/announce",
        "udp://tracker.publictracker.xyz:6969/announce",
        "http://tracker.qu.ax:6969/announce",
        "udp://tracker.qu.ax:6969/announce",
        "https://tracker.renfei.net:443/announce",
        "http://tracker.renfei.net:8080/announce",
        "udp://tracker.srv00.com:6969/announce",
        "udp://tracker.swateam.org.uk:2710/announce",
        "*udp://tracker.tallpenguin.org:15720/announce",
        "https://tracker.tamersunion.org:443/announce",
        "udp://tracker.theoks.net:6969/announce",
        "udp://tracker.therarbg.com:6969/announce",
        "udp://tracker.tiny-vps.com:6969/announce",
        "udp://tracker.torrent.eu.org:451/announce",
        "udp://tracker.trackerfix.com:85/announce",
        "udp://tracker.t-rb.org:6969/announce",
        "http://tracker.zerobytes.xyz:1337/announce",
        "udp://tracker.zerobytes.xyz:1337/announce",
        "https://tracker1.520.jp:443/announce",
        "http://tracker1.bt.moack.co.kr:80/announce",
        "udp://tracker1.bt.moack.co.kr:80/announce",
        "http://tracker1.itzmx.com:8080/announce",
        "udp://tracker1.myporn.club:9337/announce",
        "http://tracker2.dler.org:80/announce",
        "udp://tracker2.dler.org:80/announce",
        "http://tracker2.itzmx.com:6961/announce",
        "udp://tracker-udp.gbitt.info:80/announce",
        "udp://ts.populargamers.co.za:6969/announce",
        "udp://uploads.gamecoast.net:6969/announce",
        "udp://v1046920.hosted-by-vdsina.ru:6969/announce",
        "udp://v2.iperson.xyz:6969/announce",
        "http://www.peckservers.com:9000/announce",
        "https://www.peckservers.com:9443/announce",
        "udp://yahor.of.by:6969/announce");
}
