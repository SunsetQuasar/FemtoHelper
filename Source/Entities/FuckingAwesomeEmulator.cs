using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using FMOD.Studio;
using Microsoft.Xna.Framework.Graphics;

namespace Celeste.Mod.FemtoHelper.Entities;

[CustomEntity("FemtoHelper/FuckingAwesomeEmulator")]
public class FuckingAwesomeEmulator : Entity
{
    private const string MapData = "2331252548252532323232323300002425262425252631323232252628282824252525252525323328382828312525253232323233000000313232323232323232330000002432323233313232322525252525482525252525252526282824252548252525262828282824254825252526282828283132323225482525252525\r\n252331323232332900002829000000242526313232332828002824262a102824254825252526002a2828292810244825282828290000000028282900000000002810000000372829000000002a2831482525252525482525323232332828242525254825323338282a283132252548252628382828282a2a2831323232322525\r\n252523201028380000002a0000003d24252523201028292900282426003a382425252548253300002900002a0031252528382900003a676838280000000000003828393e003a2800000000000028002425253232323232332122222328282425252532332828282900002a283132252526282828282900002a28282838282448\r\n3232332828282900000000003f2020244825262828290000002a243300002a2425322525260000000000000000003125290000000021222328280000000000002a2828343536290000000000002839242526212223202123313232332828242548262b000000000000001c00003b242526282828000000000028282828282425\r\n2340283828293a2839000000343522252548262900000000000030000000002433003125333d3f00000000000000003100001c3a3a31252620283900000000000010282828290000000011113a2828313233242526103133202828282838242525262b000000000000000000003b2425262a2828670016002a28283828282425\r\n263a282828102829000000000000312525323300000000110000370000003e2400000037212223000000000000000000395868282828242628290000000000002a2828290000000000002123283828292828313233282829002a002a2828242525332b0c00000011110000000c3b314826112810000000006828282828282425\r\n252235353628280000000000003a282426003d003a3900270000000000002125001a000024252611111111000000002c28382828283831332800000017170000002a000000001111000024261028290028281b1b1b282800000000002a2125482628390000003b34362b000000002824252328283a67003a28282829002a3132\r\n25333828282900000000000000283824252320201029003039000000005824480000003a31323235353536675800003c282828281028212329000000000000000000000000003436003a2426282800003828390000002a29000000000031323226101000000000282839000000002a2425332828283800282828390000001700\r\n2600002a28000000003a283a2828282425252223283900372858390068283132000000282828282820202828283921222829002a28282426000000000000000000000000000020382828312523000000282828290000000000163a67682828003338280b00000010382800000b00003133282828282868282828280000001700\r\n330000002867580000281028283422252525482628286720282828382828212200003a283828102900002a28382824252a0000002838242600000017170000000000000000002728282a283133390000282900000000000000002a28282829002a2839000000002a282900000000000028282838282828282828290000000000\r\n0000003a2828383e3a2828283828242548252526002a282729002a28283432250000002a282828000000002810282425000000002a282426000000000000000000000000000037280000002a28283900280000003928390000000000282800000028290000002a2828000000000000002a282828281028282828675800000000\r\n0000002838282821232800002a28242532322526003a2830000000002a28282400000000002a281111111128282824480000003a28283133000000000000171700013f0000002029000000003828000028013a28281028580000003a28290000002a280c0000003a380c00000000000c00002a2828282828292828290000003a\r\n00013a2123282a313329001111112425002831263a3829300000000000002a310000000000002834222236292a0024253e013a3828292a00000000000000000035353536000020000000003d2a28671422222328282828283900582838283d00003a290000000028280000000000000000002a28282a29000058100012002a28\r\n22222225262900212311112122222525002a3837282900301111110000003a2800013f0000002a282426290000002425222222232900000000000000171700002a282039003a2000003a003435353535252525222222232828282810282821220b10000000000b28100000000b0000002c00002838000000002a283917000028\r\n2548252526111124252222252525482500012a2828673f242222230000003828222223000012002a24260000001224252525252600000000171700000000000000382028392827080028676820282828254825252525262a28282122222225253a28013d0000006828390000000000003c0168282800171717003a2800003a28\r\n25252525252222252525252525252525222222222222222525482667586828282548260000270000242600000021252525254826171700000000000000000000002a2028102830003a282828202828282525252548252600002a2425252548252821222300000028282800000000000022222223286700000000282839002838\r\n2532330000002432323232323232252525252628282828242532323232254825253232323232323225262828282448252525253300000000000000000000005225253232323233313232323233282900262829286700000000002828313232322525253233282800312525482525254825254826283828313232323232322548\r\n26282800000030402a282828282824252548262838282831333828290031322526280000163a28283133282838242525482526000000000000000000000000522526000016000000002a10282838390026281a3820393d000000002a3828282825252628282829003b2425323232323232323233282828282828102828203125\r\n3328390000003700002a3828002a2425252526282828282028292a0000002a313328111111282828000028002a312525252526000000000000000000000000522526000000001111000000292a28290026283a2820102011111121222328281025252628382800003b24262b002a2a38282828282829002a2800282838282831\r\n28281029000000000000282839002448252526282900282067000000000000003810212223283829003a1029002a242532323367000000000000000000004200252639000000212300000000002122222522222321222321222324482628282832323328282800003b31332b00000028102829000000000029002a2828282900\r\n2828280016000000162a2828280024252525262700002a2029000000000000002834252533292a0000002a00111124252223282800002c46472c00000042535325262800003a242600001600002425252525482631323331323324252620283822222328292867000028290000000000283800111100001200000028292a1600\r\n283828000000000000003a28290024254825263700000029000000000000003a293b2426283900000000003b212225252526382867003c56573c4243435363633233283900282426111111111124252525482526201b1b1b1b1b24252628282825252600002a28143a2900000000000028293b21230000170000112867000000\r\n2828286758000000586828380000313232323320000000000000000000272828003b2426290000000000003b312548252533282828392122222352535364000029002a28382831323535353522254825252525252300000000003132332810284825261111113435361111111100000000003b3133111111111127282900003b\r\n2828282810290000002a28286700002835353536111100000000000011302838003b3133000000000000002a28313225262a282810282425252662636400000000160028282829000000000031322525252525252667580000002000002a28282525323535352222222222353639000000003b34353535353536303800000017\r\n282900002a0000000000382a29003a282828283436200000000000002030282800002a29000011110000000028282831260029002a282448252523000000000039003a282900000000000000002831322525482526382900000017000058682832331028293b2448252526282828000000003b201b1b1b1b1b1b302800000017\r\n283a0000000000000000280000002828283810292a000000000000002a3710281111111111112136000000002a28380b2600000000212525252526001c0000002828281000000000001100002a382829252525252628000000001700002a212228282908003b242525482628282912000000001b00000000000030290000003b\r\n3829000000000000003a102900002838282828000000000000000000002a2828223535353535330000000000002828393300000000313225252533000000000028382829000000003b202b00682828003232323233290000000000000000312528280000003b3132322526382800170000000000000000110000370000000000\r\n290000000000000000002a000000282928292a0000000000000000000000282a332838282829000000000000001028280000000042434424252628390000000028002a0000110000001b002a2010292c1b1b1b1b0000000000000000000010312829160000001b1b1b313328106700000000001100003a2700001b0000000000\r\n00000100000011111100000000002a3a2a0000000000000000000000002a2800282829002a000000000000000028282800000000525354244826282800000000290000003b202b39000000002900003c000000000000000000000000000028282800000000000000001b1b2a2829000001000027390038300000000000000000\r\n1111201111112122230000001212002a00010000000000000000000000002900290000000000000000002a6768282900003f01005253542425262810673a3900013f0000002a3829001100000000002101000000000000003a67000000002a382867586800000100000000682800000021230037282928300000000000000000\r\n22222222222324482611111120201111002739000017170000001717000000000001000000001717000000282838393a0021222352535424253328282838290022232b00000828393b27000000001424230000001200000028290000000000282828102867001717171717282839000031333927101228370000000000000000\r\n254825252526242526212222222222223a303800000000000000000000000000001717000000000000003a28282828280024252652535424262828282828283925262b00003a28103b30000000212225260000002700003a28000000000000282838282828390000005868283828000022233830281728270000000000000000\r\n00000000000000008242525252528452339200001323232352232323232352230000000000000000b302000013232352526200a2828342525223232323232323\r\n00000000000000a20182920013232352363636462535353545550000005525355284525262b20000000000004252525262828282425284525252845252525252\r\n00000000000085868242845252525252b1006100b1b1b1b103b1b1b1b1b103b100000000000000111102000000a282425233000000a213233300009200008392\r\n000000000000110000a2000000a28213000000002636363646550000005525355252528462b2a300000000004252845262828382132323232323232352528452\r\n000000000000a201821323525284525200000000000000007300000000007300000000000000b343536300410000011362b2000000000000000000000000a200\r\n0000000000b302b2002100000000a282000000000000000000560000005526365252522333b28292001111024252525262019200829200000000a28213525252\r\n0000000000000000a2828242525252840000000000000000b10000000000b1000000000000000000b3435363930000b162273737373737373737374711000061\r\n000000110000b100b302b20000006182000000000000000000000000005600005252338282828201a31222225252525262820000a20011111100008283425252\r\n0000000000000093a382824252525252000061000011000000000011000000001100000000000000000000020182001152222222222222222222222232b20000\r\n0000b302b200000000b10000000000a200000000000000009300000000000000846282828283828282132323528452526292000000112434440000a282425284\r\n00000000000000a2828382428452525200000000b302b2936100b302b20061007293a30000000000000000b1a282931252845252525252232323232362b20000\r\n000000b10000001100000000000000000000000093000086820000a3000000005262828201a200a282829200132323236211111111243535450000b312525252\r\n00000000000000008282821323232323820000a300b1a382930000b100000000738283931100000000000011a382821323232323528462829200a20173b20061\r\n000000000000b302b2000061000000000000a385828286828282828293000000526283829200000000a20000000000005222222232263636460000b342525252\r\n00000011111111a3828201b1b1b1b1b182938282930082820000000000000000b100a282721100000000b372828283b122222232132333610000869200000000\r\n00100000000000b1000000000000000086938282828201920000a20182a37686526282829300000000000000000000005252845252328283920000b342845252\r\n00008612222232828382829300000000828282828283829200000000000061001100a382737200000000b373a2829211525284628382a2000000a20000000000\r\n00021111111111111111111111110061828282a28382820000000000828282825262829200000000000000000000000052525252526201a2000000b342525252\r\n00000113235252225353536300000000828300a282828201939300001100000072828292b1039300000000b100a282125223526292000000000000a300000000\r\n0043535353535353535353535363b2008282920082829200061600a3828382a28462000000000000000000000000000052845252526292000011111142525252\r\n0000a28282132362b1b1b1b1000000009200000000a28282828293b372b2000073820100110382a3000000110082821362101333610000000000008293000000\r\n0002828382828202828282828272b20083820000a282d3000717f38282920000526200000000000093000000000000005252525284620000b312223213528452\r\n000000828392b30300000000002100000000000000000082828282b303b20000b1a282837203820193000072a38292b162710000000000009300008382000000\r\n00b1a282820182b1a28283a28273b200828293000082122232122232820000a3233300000000000082920000000000002323232323330000b342525232135252\r\n000000a28200b37300000000a37200000010000000111111118283b373b200a30000828273039200828300738283001162930000000000008200008282920000\r\n0000009261a28200008261008282000001920000000213233342846282243434000000000000000082000085860000008382829200000000b342528452321323\r\n0000100082000082000000a2820300002222321111125353630182829200008300009200b1030000a28200008282001262829200000000a38292008282000000\r\n00858600008282a3828293008292610082001000001222222252525232253535000000f3100000a3820000a2010000008292000000009300b342525252522222\r\n0400122232b200839321008683039300528452222262c000a28282820000a38210000000a3738000008293008292001362820000000000828300a38201000000\r\n00a282828292a2828283828282000000343434344442528452525252622535350000001263000083829300008200c1008210d3e300a38200b342525252845252\r\n1232425262b28682827282820103820052525252846200000082829200008282320000008382930000a28201820000b162839300000000828200828282930000\r\n0000008382000000a28201820000000035353535454252525252528462253535000000032444008282820000829300002222223201828393b342525252525252\r\n525252525262b2b1b1b1132323526200845223232323232352522323233382825252525252525252525284522333b2822323232323526282820000b342525252\r\n52845252525252848452525262838242528452522333828292425223232352520000000000000000000000000000000000000000000000000000000000000000\r\n525252845262b2000000b1b1b142620023338276000000824233b2a282018283525252845252232323235262b1b10083921000a382426283920000b342232323\r\n2323232323232323232323526201821352522333b1b1018241133383828242840000000000000000000000000000000000000000000000000000000000000000\r\n525252525262b20000000000a242627682828392000011a273b200a382729200525252525233b1b1b1b11333000000825353536382426282410000b30382a2a2\r\na1829200a2828382820182426200a2835262b1b10000831232b2000080014252000000000000a300000000000000000000000000000000000000000000000000\r\n528452232333b20000001100824262928201a20000b3720092000000830300002323525262b200000000b3720000a382828283828242522232b200b373928000\r\n000100110092a2829211a2133300a3825262b2000000a21333b20000868242520000000000000100009300000000000000000000000000000000000000000000\r\n525262122232b200a37672b2a24262838292000000b30300000000a3820300002232132333b200000000b303829300a2838292019242845262b2000000000000\r\n00a2b302b2a36182b302b200110000825262b200000000b1b10000a283a2425200000000a30082000083000000000000000000000094a4b4c4d4e4f400000000\r\n525262428462b200a28303b2214262928300000000b3030000000000a203e3415252222232b200000000b30392000000829200000042525262b2000000000000\r\n000000b100a2828200b100b302b211a25262b200000000000000000092b3428400000000827682000001009300000000000000000095a5b5c5d5e5f500000000\r\n232333132362b221008203b2711333008293858693b3031111111111114222225252845262b200001100b303b2000000821111111142528462b2000000000000\r\n000000000000110176851100b1b3026184621111111100000061000000b3135200000000828382670082768200000000000000000096a6b6c6d6e6f600000000\r\n82000000a203117200a203b200010193828283824353235353535353535252845252525262b200b37200b303b2000000824353535323235262b2000011000000\r\n0000000000b30282828372b26100b100525232122232b200000000000000b14200000000a28282123282839200000000000000000097a7b7c7d7e7f700000000\r\n9200110000135362b2001353535353539200a2000001828282829200b34252522323232362b261b30300b3030000000092b1b1b1b1b1b34262b200b372b20000\r\n001100000000b1a2828273b200000000232333132333b200001111000000b342000000868382125252328293a300000000000000000000000000000000000000\r\n00b372b200a28303b2000000a28293b3000000000000a2828382827612525252b1b1b1b173b200b30393b30361000000000000000000b34262b271b303b20000\r\nb302b211000000110092b100000000a3b1b1b1b1b1b10011111232110000b342000000a282125284525232828386000000000000000000000000000000000000\r\n80b303b20000820311111111008283b311111111110000829200928242528452000000a3820000b30382b37300000000000000000000b3426211111103b20000\r\n00b1b302b200b372b200000000000082b21000000000b31222522363b200b3138585868292425252525262018282860000000000000000000000000000000000\r\n00b373b20000a21353535363008292b32222222232111102b20000a21323525200000001839200b3038282820000000011111111930011425222222233b20000\r\n100000b10000b303b200000000858682b27100000000b3425233b1b1000000b182018283001323525284629200a2820000000000000000000000000000000000\r\n9300b100000000b1b1b1b1b100a200b323232323235363b100000000b1b1135200000000820000b30382839200000000222222328283432323232333b2000000\r\n329300000000b373b200000000a20182111111110000b31333b100a30061000000a28293f3123242522333020000820000000000000000000000000000000000\r\n829200001000410000000000000000b39310d30000a28200000000000000824200000086827600b30300a282760000005252526200828200a30182a2006100a3\r\n62820000000000b100000093a382838222222232b20000b1b1000083000000860000122222526213331222328293827600000000000000000000000000000000\r\n017685a31222321111111111002100b322223293000182930000000080a301131000a383829200b373000083920000005284526200a282828283920000000082\r\n62839321000000000000a3828282820152845262b261000093000082a300a3821000135252845222225252523201838200000000000000000000000000000000\r\n828382824252522222222232007100b352526282a38283820000000000838282320001828200000083000082010000005252526271718283820000000000a382\r\n628201729300000000a282828382828252528462b20000a38300a382018283821222324252525252525284525222223200000000000000000000000000000000";

    //public Scene ReturnTo;

    private FuckingAwesomeClassic game;

    private int gameFrame;

    private bool gameActive;

    private float gameDelay;

    private Point bootLevel;

    private bool skipFrame;

    private EventInstance bgSfx;

    private VirtualRenderTarget buffer;

    private Color[] pixels;

    private Vector2 offset;

    private float pauseFade;

    private EventInstance snapshot;

    private MTexture picoBootLogo;

    private byte[] tilemap;

    private MTexture[] sprites;

    private byte[] mask;

    private Color[] colors;

    private Dictionary<Color, int> paletteSwap;

    private MTexture[] font;

    private string fontMap;

    private Effect effect;

    private bool booting => game == null;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public FuckingAwesomeEmulator(EntityData data, Vector2 offset) : base(data.Position + offset)
    {
        
        if (Everest.Content.TryGet($"Effects/FemtoHelper/retronew.cso", out var effectAsset, true))
        {
            effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset.Data);
        }
        else
        {
            Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", $"Failed getting effect: \"Effects/FemtoHelper/retronew.cso\"! Using default shader instead.");
            if(Everest.Content.TryGet($"Effects/FemtoHelper/DistortedParallax.cso", out var effectAsset2, true))
            {
                effect = new Effect(Engine.Graphics.GraphicsDevice, effectAsset2.Data);
            }
            else
            {
                Logger.Log(LogLevel.Error, "FemtoHelper/NewDistortedParallax", "Failed getting the default shader?? How??");
            }
        }
        
        EffectParameterCollection parameters = effect.Parameters;
        parameters["Dimensions"]?.SetValue(new Vector2(320f, 180f));
        
        Tag |= TagsExt.SubHUD;
        orig_ctor(0, 0);
        if (!Everest.Content.TryGet<AssetTypeText>("Pico8Tilemap", out var metadata))
        {
            return;
        }
        string input;
        using (StreamReader streamReader = new StreamReader(metadata.Stream))
        {
            input = streamReader.ReadToEnd();
        }
        input = Regex.Replace(input, "\\s+", "");
        tilemap = new byte[input.Length / 2];
        int length = input.Length;
        int num = length / 2;
        for (int i = 0; i < length; i += 2)
        {
            char c = input[i];
            char c2 = input[i + 1];
            byte[] array = tilemap;
            int num2 = i / 2;
            char reference;
            char reference2;
            string s;
            if (i >= num)
            {
                reference = c2;
                ReadOnlySpan<char> readOnlySpan = new ReadOnlySpan<char>(in reference);
                reference2 = c;
                s = string.Concat(readOnlySpan, new ReadOnlySpan<char>(in reference2));
            }
            else
            {
                reference2 = c;
                ReadOnlySpan<char> readOnlySpan2 = new ReadOnlySpan<char>(in reference2);
                reference = c2;
                s = string.Concat(readOnlySpan2, new ReadOnlySpan<char>(in reference));
            }
            array[num2] = (byte)int.Parse(s, NumberStyles.HexNumber);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Awake(Scene scene)
    {
        bgSfx = Audio.Play("event:/env/amb/03_pico8_closeup");
        base.Awake(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Removed(Scene scene)
    {
        buffer.Dispose();
        Audio.BusStopAll("bus:/gameplay_sfx");
        Audio.Stop(bgSfx);
        if (snapshot != null)
        {
            Audio.ReleaseSnapshot(snapshot);
        }
        snapshot = null;
        base.Removed(scene);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void ResetScreen()
    {
        //IL_0043: Unknown result type (might be due to invalid IL or missing references)
        //IL_0048: Unknown result type (might be due to invalid IL or missing references)
        Engine.Graphics.GraphicsDevice.Textures[0] = null;
        Engine.Graphics.GraphicsDevice.Textures[1] = null;
        for (int i = 0; i < 128; i++)
        {
            for (int j = 0; j < 128; j++)
            {
                pixels[i + j * 128] = Color.Black;
            }
        }
        buffer.Target.SetData<Color>(pixels);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Update()
    {
        //IL_054d: Unknown result type (might be due to invalid IL or missing references)
        //IL_058b: Unknown result type (might be due to invalid IL or missing references)
        //IL_05f7: Unknown result type (might be due to invalid IL or missing references)
        //IL_0615: Unknown result type (might be due to invalid IL or missing references)
        //IL_061a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0145: Unknown result type (might be due to invalid IL or missing references)
        //IL_014a: Unknown result type (might be due to invalid IL or missing references)
        //IL_01aa: Unknown result type (might be due to invalid IL or missing references)
        //IL_01af: Unknown result type (might be due to invalid IL or missing references)
        //IL_0218: Unknown result type (might be due to invalid IL or missing references)
        //IL_021d: Unknown result type (might be due to invalid IL or missing references)
        //IL_03e2: Unknown result type (might be due to invalid IL or missing references)
        //IL_0417: Unknown result type (might be due to invalid IL or missing references)
        //IL_0289: Unknown result type (might be due to invalid IL or missing references)
        //IL_028e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0357: Unknown result type (might be due to invalid IL or missing references)
        //IL_035c: Unknown result type (might be due to invalid IL or missing references)
        //IL_02f2: Unknown result type (might be due to invalid IL or missing references)
        //IL_02f7: Unknown result type (might be due to invalid IL or missing references)
        base.Update();
        skipFrame = !skipFrame;
        if (skipFrame)
        {
            return;
        }
        gameDelay -= Engine.DeltaTime;
        if (!gameActive || gameDelay > 0f)
        {
            return;
        }
        if (booting)
        {
            Engine.Graphics.GraphicsDevice.Textures[0] = null;
            Engine.Graphics.GraphicsDevice.Textures[1] = null;
            gameFrame++;
            int num = gameFrame - 20;
            if (num == 1)
            {
                for (int i = 0; i < 128; i++)
                {
                    for (int j = 2; j < 128; j += 8)
                    {
                        pixels[j + i * 128] = colors[Calc.Random.Next(4) + i / 32];
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 4)
            {
                for (int k = 0; k < 128; k += 2)
                {
                    for (int l = 0; l < 128; l += 4)
                    {
                        pixels[l + k * 128] = colors[6 + (((l + k) / 8) & 7)];
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 7)
            {
                for (int m = 0; m < 128; m += 3)
                {
                    for (int n = 2; n < 128; n += 4)
                    {
                        pixels[n + m * 128] = colors[10 + Calc.Random.Next(4)];
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 9)
            {
                for (int num2 = 0; num2 < 128; num2++)
                {
                    for (int num3 = 1; num3 < 127; num3 += 2)
                    {
                        pixels[num3 + num2 * 128] = pixels[num3 + 1 + num2 * 128];
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 12)
            {
                for (int num4 = 0; num4 < 128; num4++)
                {
                    if ((num4 & 3) > 0)
                    {
                        for (int num5 = 0; num5 < 128; num5++)
                        {
                            pixels[num5 + num4 * 128] = colors[0];
                        }
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 15)
            {
                for (int num6 = 0; num6 < 128; num6++)
                {
                    for (int num7 = 0; num7 < 128; num7++)
                    {
                        pixels[num7 + num6 * 128] = colors[0];
                    }
                }
                buffer.Target.SetData<Color>(pixels);
            }
            if (num == 30)
            {
                Audio.Play("event:/classic/pico8_boot");
            }
            if (num == 30 || num == 35 || num == 40)
            {
                Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
                Engine.Graphics.GraphicsDevice.Clear(colors[0]);
                Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone);
                picoBootLogo.Draw(new Vector2(1f, 1f));
                if (num >= 35)
                {
                    print("pico-8 0.1.9B", 1f, 18f, 6f);
                }
                if (num >= 40)
                {
                    print("(c) 2014-16 lexaloffle games llp", 1f, 24f, 6f);
                    print("booting cartridge..", 1f, 36f, 6f);
                }
                Draw.SpriteBatch.End();
                Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
            }
            if (num == 90)
            {
                gameFrame = 0;
                game = new FuckingAwesomeClassic();
                game.Init(this);
                if (bootLevel.X != 0 || bootLevel.Y != 0)
                {
                    game.load_room(bootLevel.X, bootLevel.Y);
                }
            }
            return;
        }
        gameFrame++;
        game.Update();
        if (game.freeze > 0)
        {
            return;
        }
        Engine.Graphics.GraphicsDevice.SetRenderTarget(buffer);
        Engine.Graphics.GraphicsDevice.Clear(colors[0]);
        Draw.SpriteBatch.Begin(0, BlendState.AlphaBlend, SamplerState.PointClamp, null, RasterizerState.CullNone, null, Matrix.CreateTranslation(0f - offset.X, 0f - offset.Y, 0f));
        game.Draw();
        Draw.SpriteBatch.End();
        Engine.Graphics.GraphicsDevice.SetRenderTarget(null);
        if (paletteSwap.Count <= 0)
        {
            return;
        }
        buffer.Target.GetData<Color>(pixels);
        for (int num8 = 0; num8 < pixels.Length; num8++)
        {
            int value = 0;
            if (paletteSwap.TryGetValue(pixels[num8], out value))
            {
                pixels[num8] = colors[value];
            }
        }
        buffer.Target.SetData<Color>(pixels);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public override void Render()
    {
        bool flag = SaveData.Instance != null && SaveData.Instance.Assists.MirrorMode;
        base.Render();
        GameplayRenderer.End();
        
        Viewport viewport = Engine.Graphics.GraphicsDevice.Viewport;
        Matrix projection = Matrix.CreateOrthographicOffCenter(0f, viewport.Width, viewport.Height, 0f, 0f, 1f);
        
        EffectParameterCollection parameters = effect.Parameters;
        parameters["Dimensions"]?.SetValue(new Vector2(128, 128));
        parameters["DeltaTime"]?.SetValue(Engine.DeltaTime);
        parameters["Time"]?.SetValue(Scene.TimeActive);
        parameters["CamPos"]?.SetValue(SceneAs<Level>().Camera.Position);
        parameters["TransformMatrix"]?.SetValue(projection);
        parameters["ViewMatrix"]?.SetValue(Matrix.Identity);
        
        Draw.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, effect);
        Draw.SpriteBatch.Draw(buffer, (Position - SceneAs<Level>().Camera.Position) * 6, new Rectangle(0, 0, buffer.Width, buffer.Height), Color.White, 0, Vector2.Zero, 6f, flag ? SpriteEffects.FlipHorizontally : SpriteEffects.None, 0);
        Draw.SpriteBatch.End();
        GameplayRenderer.Begin();
    }
    
    public void music(int index, int fade, int mask)
    {
        switch (index)
        {
            case -1:
                Audio.SetMusic(null);
                break;
            case 0:
                Audio.SetMusic("event:/classic/pico8_mus_01");
                break;
            case 10:
                Audio.SetMusic("event:/classic/pico8_mus_03");
                break;
            case 20:
                Audio.SetMusic("event:/classic/pico8_mus_02");
                break;
            case 30:
                Audio.SetMusic("event:/classic/sfx61");
                break;
            case 40:
                Audio.SetMusic("event:/classic/pico8_mus_00");
                break;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void sfx(int sfx)
    {
        Audio.Play("event:/classic/sfx" + sfx);
    }

    public float rnd(float max)
    {
        return Calc.Random.NextFloat(max);
    }

    public int flr(float value)
    {
        return (int)Math.Floor(value);
    }

    public int sign(float value)
    {
        return Math.Sign(value);
    }

    public float abs(float value)
    {
        return Math.Abs(value);
    }

    public float min(float a, float b)
    {
        return Math.Min(a, b);
    }

    public float max(float a, float b)
    {
        return Math.Max(a, b);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public float sin(float a)
    {
        return (float)Math.Sin((1f - a) * (MathF.PI * 2f));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public float cos(float a)
    {
        return (float)Math.Cos((1f - a) * (MathF.PI * 2f));
    }

    public float mod(float a, float b)
    {
        float num = a % b;
        if (!(num >= 0f))
        {
            return b + num;
        }
        return num;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool btn(int index)
    {
        //IL_001e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0030: Unknown result type (might be due to invalid IL or missing references)
        //IL_0042: Unknown result type (might be due to invalid IL or missing references)
        //IL_0054: Unknown result type (might be due to invalid IL or missing references)
        Vector2 val = new((float)Input.MoveX, (float)Input.MoveY);
        return index switch
        {
            0 => val.X < 0f, 
            1 => val.X > 0f, 
            2 => val.Y < 0f, 
            3 => val.Y > 0f, 
            4 => Input.Jump.Check, 
            5 => Input.Dash.Check, 
            _ => false, 
        };
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private Vector2 aimVector()
    {
        //IL_0005: Unknown result type (might be due to invalid IL or missing references)
        //IL_000a: Unknown result type (might be due to invalid IL or missing references)
        //IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_010c: Unknown result type (might be due to invalid IL or missing references)
        //IL_003f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0033: Unknown result type (might be due to invalid IL or missing references)
        //IL_0034: Unknown result type (might be due to invalid IL or missing references)
        //IL_0039: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f5: Unknown result type (might be due to invalid IL or missing references)
        //IL_0101: Unknown result type (might be due to invalid IL or missing references)
        //IL_0106: Unknown result type (might be due to invalid IL or missing references)
        //IL_010b: Unknown result type (might be due to invalid IL or missing references)
        Vector2 val = Input.Aim.Value;
        if (val != Vector2.Zero)
        {
            if (SaveData.Instance != null && SaveData.Instance.Assists.ThreeSixtyDashing)
            {
                val = val.SafeNormalize();
            }
            else
            {
                float num = val.Angle();
                int num2 = ((num < 0f) ? 1 : 0);
                float num3 = MathF.PI / 8f - num2 * 0.08726646f;
                if (Calc.AbsAngleDiff(num, 0f) < num3)
                {
                    val = new(1f, 0f);
                }
                else if (Calc.AbsAngleDiff(num, MathF.PI) < num3)
                {
                    val = new(-1f, 0f);
                }
                else if (Calc.AbsAngleDiff(num, -MathF.PI / 2f) < num3)
                {
                    val = new(0f, -1f);
                }
                else if (Calc.AbsAngleDiff(num, MathF.PI / 2f) < num3)
                {
                    val = new(0f, 1f);
                }
                else
                {
                    val = Calc.SafeNormalize(new Vector2(Math.Sign(val.X), Math.Sign(val.Y)));
                }
            }
        }
        return val;
    }

    public int dashDirectionX(int facing)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        return Math.Sign(aimVector().X);
    }

    public int dashDirectionY(int facing)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        return Math.Sign(aimVector().Y);
    }

    public int mget(int tx, int ty)
    {
        return tilemap[tx + ty * 128];
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public bool fget(int tile, int flag)
    {
        if (tile < mask.Length)
        {
            return (mask[tile] & (1 << flag)) != 0;
        }
        return false;
    }

    public void camera()
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        offset = Vector2.Zero;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void camera(float x, float y)
    {
        //IL_0013: Unknown result type (might be due to invalid IL or missing references)
        //IL_0018: Unknown result type (might be due to invalid IL or missing references)
        offset = new Vector2((int)Math.Round(x), (int)Math.Round(y));
    }

    public void pal()
    {
        paletteSwap.Clear();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void pal(int a, int b)
    {
        //IL_0007: Unknown result type (might be due to invalid IL or missing references)
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0013: Unknown result type (might be due to invalid IL or missing references)
        //IL_002f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0021: Unknown result type (might be due to invalid IL or missing references)
        Color key = colors[a];
        if (paletteSwap.ContainsKey(key))
        {
            paletteSwap[key] = b;
        }
        else
        {
            paletteSwap.Add(key, b);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void rectfill(float x, float y, float x2, float y2, float c)
    {
        //IL_0042: Unknown result type (might be due to invalid IL or missing references)
        float num = Math.Min(x, x2);
        float num2 = Math.Min(y, y2);
        float width = Math.Max(x, x2) - num + 1f;
        float height = Math.Max(y, y2) - num2 + 1f;
        Draw.Rect(num, num2, width, height, colors[(int)c % 16]);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void circfill(float x, float y, float r, float c)
    {
        //IL_000c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_002c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0044: Unknown result type (might be due to invalid IL or missing references)
        //IL_006b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0089: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ce: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ec: Unknown result type (might be due to invalid IL or missing references)
        Color color = colors[(int)c % 16];
        if (r <= 1f)
        {
            Draw.Rect(x - 1f, y, 3f, 1f, color);
            Draw.Rect(x, y - 1f, 1f, 3f, color);
        }
        else if (r <= 2f)
        {
            Draw.Rect(x - 2f, y - 1f, 5f, 3f, color);
            Draw.Rect(x - 1f, y - 2f, 3f, 5f, color);
        }
        else if (r <= 3f)
        {
            Draw.Rect(x - 3f, y - 1f, 7f, 3f, color);
            Draw.Rect(x - 1f, y - 3f, 3f, 7f, color);
            Draw.Rect(x - 2f, y - 2f, 5f, 5f, color);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void print(string str, float x, float y, float c)
    {
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0013: Unknown result type (might be due to invalid IL or missing references)
        //IL_0063: Unknown result type (might be due to invalid IL or missing references)
        //IL_0068: Unknown result type (might be due to invalid IL or missing references)
        //IL_006d: Unknown result type (might be due to invalid IL or missing references)
        float num = x;
        Color color = colors[(int)c % 16];
        foreach (char c2 in str)
        {
            int num2 = -1;
            for (int j = 0; j < fontMap.Length; j++)
            {
                if (fontMap[j] == c2)
                {
                    num2 = j;
                    break;
                }
            }
            if (num2 >= 0)
            {
                font[num2].Draw(new Vector2(num, y), Vector2.Zero, color);
            }
            num += 4f;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void map(int mx, int my, int tx, int ty, int mw, int mh, int mask = 0)
    {
        //IL_004c: Unknown result type (might be due to invalid IL or missing references)
        for (int i = 0; i < mw; i++)
        {
            for (int j = 0; j < mh; j++)
            {
                byte b = tilemap[i + mx + (j + my) * 128];
                if (b < sprites.Length && (mask == 0 || fget(b, mask)))
                {
                    sprites[b].Draw(new Vector2(tx + i * 8, ty + j * 8));
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void spr(float sprite, float x, float y, int columns = 1, int rows = 1, bool flipX = false, bool flipY = false)
    {
        //IL_0001: Unknown result type (might be due to invalid IL or missing references)
        //IL_0006: Unknown result type (might be due to invalid IL or missing references)
        //IL_0008: Unknown result type (might be due to invalid IL or missing references)
        //IL_0009: Unknown result type (might be due to invalid IL or missing references)
        //IL_000e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_0011: Unknown result type (might be due to invalid IL or missing references)
        //IL_0046: Unknown result type (might be due to invalid IL or missing references)
        //IL_004b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0050: Unknown result type (might be due to invalid IL or missing references)
        //IL_005f: Unknown result type (might be due to invalid IL or missing references)
        SpriteEffects val = SpriteEffects.None;
        if (flipX)
        {
            val |= SpriteEffects.FlipHorizontally;
        }
        if (flipY)
        {
            val |= SpriteEffects.FlipVertically;
        }
        for (int i = 0; i < columns; i++)
        {
            for (int j = 0; j < rows; j++)
            {
                sprites[(int)sprite + i + j * 16].Draw(new Vector2((int)Math.Floor(x + i * 8), (int)Math.Floor(y + j * 8)), Vector2.Zero, Color.White, 1f, 0f, val);
            }
        }
    }
    
    public void orig_ctor(int levelX = 0, int levelY = 0)
    {
        //IL_001f: Unknown result type (might be due to invalid IL or missing references)
        //IL_0024: Unknown result type (might be due to invalid IL or missing references)
        //IL_0053: Unknown result type (might be due to invalid IL or missing references)
        //IL_0058: Unknown result type (might be due to invalid IL or missing references)
        //IL_0064: Unknown result type (might be due to invalid IL or missing references)
        //IL_0069: Unknown result type (might be due to invalid IL or missing references)
        //IL_0075: Unknown result type (might be due to invalid IL or missing references)
        //IL_007a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0086: Unknown result type (might be due to invalid IL or missing references)
        //IL_008b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0097: Unknown result type (might be due to invalid IL or missing references)
        //IL_009c: Unknown result type (might be due to invalid IL or missing references)
        //IL_00a8: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ad: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b9: Unknown result type (might be due to invalid IL or missing references)
        //IL_00be: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ca: Unknown result type (might be due to invalid IL or missing references)
        //IL_00cf: Unknown result type (might be due to invalid IL or missing references)
        //IL_00db: Unknown result type (might be due to invalid IL or missing references)
        //IL_00e0: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ed: Unknown result type (might be due to invalid IL or missing references)
        //IL_00f2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00ff: Unknown result type (might be due to invalid IL or missing references)
        //IL_0104: Unknown result type (might be due to invalid IL or missing references)
        //IL_0111: Unknown result type (might be due to invalid IL or missing references)
        //IL_0116: Unknown result type (might be due to invalid IL or missing references)
        //IL_0123: Unknown result type (might be due to invalid IL or missing references)
        //IL_0128: Unknown result type (might be due to invalid IL or missing references)
        //IL_0135: Unknown result type (might be due to invalid IL or missing references)
        //IL_013a: Unknown result type (might be due to invalid IL or missing references)
        //IL_0147: Unknown result type (might be due to invalid IL or missing references)
        //IL_014c: Unknown result type (might be due to invalid IL or missing references)
        //IL_0159: Unknown result type (might be due to invalid IL or missing references)
        //IL_015e: Unknown result type (might be due to invalid IL or missing references)
        //IL_018e: Unknown result type (might be due to invalid IL or missing references)
        //IL_0193: Unknown result type (might be due to invalid IL or missing references)
        gameActive = true;
        skipFrame = true;
        pixels = new Color[16384];
        offset = Vector2.Zero;
        mask = new byte[128]
        {
            0, 0, 0, 0, 0, 0, 0, 0, 0, 0,
            0, 0, 0, 0, 0, 0, 4, 2, 0, 0,
            0, 0, 0, 0, 0, 0, 0, 2, 0, 0,
            0, 0, 3, 3, 3, 3, 3, 3, 3, 3,
            4, 4, 4, 2, 2, 0, 0, 0, 3, 3,
            3, 3, 3, 3, 3, 3, 4, 4, 4, 2,
            2, 2, 2, 2, 0, 0, 19, 19, 19, 19,
            2, 2, 3, 2, 2, 2, 2, 2, 2, 2,
            0, 0, 19, 19, 19, 19, 2, 2, 4, 2,
            2, 2, 2, 2, 2, 2, 0, 0, 19, 19,
            19, 19, 0, 4, 4, 2, 2, 2, 2, 2,
            2, 2, 0, 0, 19, 19, 19, 19, 0, 0,
            0, 2, 2, 2, 2, 2, 2, 2
        };
        colors = new Color[16]
        {
            Calc.HexToColor("000000"),
            Calc.HexToColor("1d2b53"),
            Calc.HexToColor("7e2553"),
            Calc.HexToColor("008751"),
            Calc.HexToColor("ab5236"),
            Calc.HexToColor("5f574f"),
            Calc.HexToColor("c2c3c7"),
            Calc.HexToColor("fff1e8"),
            Calc.HexToColor("ff004d"),
            Calc.HexToColor("ffa300"),
            Calc.HexToColor("ffec27"),
            Calc.HexToColor("00e436"),
            Calc.HexToColor("29adff"),
            Calc.HexToColor("83769c"),
            Calc.HexToColor("ff77a8"),
            Calc.HexToColor("ffccaa")
        };
        paletteSwap = new Dictionary<Color, int>();
        fontMap = "abcdefghijklmnopqrstuvwxyz0123456789~!@#$%^&*()_+-=?:.";
        //base..ctor();
        bootLevel = new Point(levelX, levelY);
        buffer = VirtualContent.CreateRenderTarget("pico-8", 128, 128);
        MTexture mTexture = GFX.Game["pico8/atlas"];
        sprites = new MTexture[mTexture.Width / 8 * (mTexture.Height / 8)];
        for (int i = 0; i < mTexture.Height / 8; i++)
        {
            for (int j = 0; j < mTexture.Width / 8; j++)
            {
                sprites[j + i * (mTexture.Width / 8)] = mTexture.GetSubtexture(j * 8, i * 8, 8, 8);
            }
        }
        string input = "2331252548252532323232323300002425262425252631323232252628282824252525252525323328382828312525253232323233000000313232323232323232330000002432323233313232322525252525482525252525252526282824252548252525262828282824254825252526282828283132323225482525252525\r\n252331323232332900002829000000242526313232332828002824262a102824254825252526002a2828292810244825282828290000000028282900000000002810000000372829000000002a2831482525252525482525323232332828242525254825323338282a283132252548252628382828282a2a2831323232322525\r\n252523201028380000002a0000003d24252523201028292900282426003a382425252548253300002900002a0031252528382900003a676838280000000000003828393e003a2800000000000028002425253232323232332122222328282425252532332828282900002a283132252526282828282900002a28282838282448\r\n3232332828282900000000003f2020244825262828290000002a243300002a2425322525260000000000000000003125290000000021222328280000000000002a2828343536290000000000002839242526212223202123313232332828242548262b000000000000001c00003b242526282828000000000028282828282425\r\n2340283828293a2839000000343522252548262900000000000030000000002433003125333d3f00000000000000003100001c3a3a31252620283900000000000010282828290000000011113a2828313233242526103133202828282838242525262b000000000000000000003b2425262a2828670016002a28283828282425\r\n263a282828102829000000000000312525323300000000110000370000003e2400000037212223000000000000000000395868282828242628290000000000002a2828290000000000002123283828292828313233282829002a002a2828242525332b0c00000011110000000c3b314826112810000000006828282828282425\r\n252235353628280000000000003a282426003d003a3900270000000000002125001a000024252611111111000000002c28382828283831332800000017170000002a000000001111000024261028290028281b1b1b282800000000002a2125482628390000003b34362b000000002824252328283a67003a28282829002a3132\r\n25333828282900000000000000283824252320201029003039000000005824480000003a31323235353536675800003c282828281028212329000000000000000000000000003436003a2426282800003828390000002a29000000000031323226101000000000282839000000002a2425332828283800282828390000001700\r\n2600002a28000000003a283a2828282425252223283900372858390068283132000000282828282820202828283921222829002a28282426000000000000000000000000000020382828312523000000282828290000000000163a67682828003338280b00000010382800000b00003133282828282868282828280000001700\r\n330000002867580000281028283422252525482628286720282828382828212200003a283828102900002a28382824252a0000002838242600000017170000000000000000002728282a283133390000282900000000000000002a28282829002a2839000000002a282900000000000028282838282828282828290000000000\r\n0000003a2828383e3a2828283828242548252526002a282729002a28283432250000002a282828000000002810282425000000002a282426000000000000000000000000000037280000002a28283900280000003928390000000000282800000028290000002a2828000000000000002a282828281028282828675800000000\r\n0000002838282821232800002a28242532322526003a2830000000002a28282400000000002a281111111128282824480000003a28283133000000000000171700013f0000002029000000003828000028013a28281028580000003a28290000002a280c0000003a380c00000000000c00002a2828282828292828290000003a\r\n00013a2123282a313329001111112425002831263a3829300000000000002a310000000000002834222236292a0024253e013a3828292a00000000000000000035353536000020000000003d2a28671422222328282828283900582838283d00003a290000000028280000000000000000002a28282a29000058100012002a28\r\n22222225262900212311112122222525002a3837282900301111110000003a2800013f0000002a282426290000002425222222232900000000000000171700002a282039003a2000003a003435353535252525222222232828282810282821220b10000000000b28100000000b0000002c00002838000000002a283917000028\r\n2548252526111124252222252525482500012a2828673f242222230000003828222223000012002a24260000001224252525252600000000171700000000000000382028392827080028676820282828254825252525262a28282122222225253a28013d0000006828390000000000003c0168282800171717003a2800003a28\r\n25252525252222252525252525252525222222222222222525482667586828282548260000270000242600000021252525254826171700000000000000000000002a2028102830003a282828202828282525252548252600002a2425252548252821222300000028282800000000000022222223286700000000282839002838\r\n2532330000002432323232323232252525252628282828242532323232254825253232323232323225262828282448252525253300000000000000000000005225253232323233313232323233282900262829286700000000002828313232322525253233282800312525482525254825254826283828313232323232322548\r\n26282800000030402a282828282824252548262838282831333828290031322526280000163a28283133282838242525482526000000000000000000000000522526000016000000002a10282838390026281a3820393d000000002a3828282825252628282829003b2425323232323232323233282828282828102828203125\r\n3328390000003700002a3828002a2425252526282828282028292a0000002a313328111111282828000028002a312525252526000000000000000000000000522526000000001111000000292a28290026283a2820102011111121222328281025252628382800003b24262b002a2a38282828282829002a2800282838282831\r\n28281029000000000000282839002448252526282900282067000000000000003810212223283829003a1029002a242532323367000000000000000000004200252639000000212300000000002122222522222321222321222324482628282832323328282800003b31332b00000028102829000000000029002a2828282900\r\n2828280016000000162a2828280024252525262700002a2029000000000000002834252533292a0000002a00111124252223282800002c46472c00000042535325262800003a242600001600002425252525482631323331323324252620283822222328292867000028290000000000283800111100001200000028292a1600\r\n283828000000000000003a28290024254825263700000029000000000000003a293b2426283900000000003b212225252526382867003c56573c4243435363633233283900282426111111111124252525482526201b1b1b1b1b24252628282825252600002a28143a2900000000000028293b21230000170000112867000000\r\n2828286758000000586828380000313232323320000000000000000000272828003b2426290000000000003b312548252533282828392122222352535364000029002a28382831323535353522254825252525252300000000003132332810284825261111113435361111111100000000003b3133111111111127282900003b\r\n2828282810290000002a28286700002835353536111100000000000011302838003b3133000000000000002a28313225262a282810282425252662636400000000160028282829000000000031322525252525252667580000002000002a28282525323535352222222222353639000000003b34353535353536303800000017\r\n282900002a0000000000382a29003a282828283436200000000000002030282800002a29000011110000000028282831260029002a282448252523000000000039003a282900000000000000002831322525482526382900000017000058682832331028293b2448252526282828000000003b201b1b1b1b1b1b302800000017\r\n283a0000000000000000280000002828283810292a000000000000002a3710281111111111112136000000002a28380b2600000000212525252526001c0000002828281000000000001100002a382829252525252628000000001700002a212228282908003b242525482628282912000000001b00000000000030290000003b\r\n3829000000000000003a102900002838282828000000000000000000002a2828223535353535330000000000002828393300000000313225252533000000000028382829000000003b202b00682828003232323233290000000000000000312528280000003b3132322526382800170000000000000000110000370000000000\r\n290000000000000000002a000000282928292a0000000000000000000000282a332838282829000000000000001028280000000042434424252628390000000028002a0000110000001b002a2010292c1b1b1b1b0000000000000000000010312829160000001b1b1b313328106700000000001100003a2700001b0000000000\r\n00000100000011111100000000002a3a2a0000000000000000000000002a2800282829002a000000000000000028282800000000525354244826282800000000290000003b202b39000000002900003c000000000000000000000000000028282800000000000000001b1b2a2829000001000027390038300000000000000000\r\n1111201111112122230000001212002a00010000000000000000000000002900290000000000000000002a6768282900003f01005253542425262810673a3900013f0000002a3829001100000000002101000000000000003a67000000002a382867586800000100000000682800000021230037282928300000000000000000\r\n22222222222324482611111120201111002739000017170000001717000000000001000000001717000000282838393a0021222352535424253328282838290022232b00000828393b27000000001424230000001200000028290000000000282828102867001717171717282839000031333927101228370000000000000000\r\n254825252526242526212222222222223a303800000000000000000000000000001717000000000000003a28282828280024252652535424262828282828283925262b00003a28103b30000000212225260000002700003a28000000000000282838282828390000005868283828000022233830281728270000000000000000\r\n00000000000000008242525252528452339200001323232352232323232352230000000000000000b302000013232352526200a2828342525223232323232323\r\n00000000000000a20182920013232352363636462535353545550000005525355284525262b20000000000004252525262828282425284525252845252525252\r\n00000000000085868242845252525252b1006100b1b1b1b103b1b1b1b1b103b100000000000000111102000000a282425233000000a213233300009200008392\r\n000000000000110000a2000000a28213000000002636363646550000005525355252528462b2a300000000004252845262828382132323232323232352528452\r\n000000000000a201821323525284525200000000000000007300000000007300000000000000b343536300410000011362b2000000000000000000000000a200\r\n0000000000b302b2002100000000a282000000000000000000560000005526365252522333b28292001111024252525262019200829200000000a28213525252\r\n0000000000000000a2828242525252840000000000000000b10000000000b1000000000000000000b3435363930000b162273737373737373737374711000061\r\n000000110000b100b302b20000006182000000000000000000000000005600005252338282828201a31222225252525262820000a20011111100008283425252\r\n0000000000000093a382824252525252000061000011000000000011000000001100000000000000000000020182001152222222222222222222222232b20000\r\n0000b302b200000000b10000000000a200000000000000009300000000000000846282828283828282132323528452526292000000112434440000a282425284\r\n00000000000000a2828382428452525200000000b302b2936100b302b20061007293a30000000000000000b1a282931252845252525252232323232362b20000\r\n000000b10000001100000000000000000000000093000086820000a3000000005262828201a200a282829200132323236211111111243535450000b312525252\r\n00000000000000008282821323232323820000a300b1a382930000b100000000738283931100000000000011a382821323232323528462829200a20173b20061\r\n000000000000b302b2000061000000000000a385828286828282828293000000526283829200000000a20000000000005222222232263636460000b342525252\r\n00000011111111a3828201b1b1b1b1b182938282930082820000000000000000b100a282721100000000b372828283b122222232132333610000869200000000\r\n00100000000000b1000000000000000086938282828201920000a20182a37686526282829300000000000000000000005252845252328283920000b342845252\r\n00008612222232828382829300000000828282828283829200000000000061001100a382737200000000b373a2829211525284628382a2000000a20000000000\r\n00021111111111111111111111110061828282a28382820000000000828282825262829200000000000000000000000052525252526201a2000000b342525252\r\n00000113235252225353536300000000828300a282828201939300001100000072828292b1039300000000b100a282125223526292000000000000a300000000\r\n0043535353535353535353535363b2008282920082829200061600a3828382a28462000000000000000000000000000052845252526292000011111142525252\r\n0000a28282132362b1b1b1b1000000009200000000a28282828293b372b2000073820100110382a3000000110082821362101333610000000000008293000000\r\n0002828382828202828282828272b20083820000a282d3000717f38282920000526200000000000093000000000000005252525284620000b312223213528452\r\n000000828392b30300000000002100000000000000000082828282b303b20000b1a282837203820193000072a38292b162710000000000009300008382000000\r\n00b1a282820182b1a28283a28273b200828293000082122232122232820000a3233300000000000082920000000000002323232323330000b342525232135252\r\n000000a28200b37300000000a37200000010000000111111118283b373b200a30000828273039200828300738283001162930000000000008200008282920000\r\n0000009261a28200008261008282000001920000000213233342846282243434000000000000000082000085860000008382829200000000b342528452321323\r\n0000100082000082000000a2820300002222321111125353630182829200008300009200b1030000a28200008282001262829200000000a38292008282000000\r\n00858600008282a3828293008292610082001000001222222252525232253535000000f3100000a3820000a2010000008292000000009300b342525252522222\r\n0400122232b200839321008683039300528452222262c000a28282820000a38210000000a3738000008293008292001362820000000000828300a38201000000\r\n00a282828292a2828283828282000000343434344442528452525252622535350000001263000083829300008200c1008210d3e300a38200b342525252845252\r\n1232425262b28682827282820103820052525252846200000082829200008282320000008382930000a28201820000b162839300000000828200828282930000\r\n0000008382000000a28201820000000035353535454252525252528462253535000000032444008282820000829300002222223201828393b342525252525252\r\n525252525262b2b1b1b1132323526200845223232323232352522323233382825252525252525252525284522333b2822323232323526282820000b342525252\r\n52845252525252848452525262838242528452522333828292425223232352520000000000000000000000000000000000000000000000000000000000000000\r\n525252845262b2000000b1b1b142620023338276000000824233b2a282018283525252845252232323235262b1b10083921000a382426283920000b342232323\r\n2323232323232323232323526201821352522333b1b1018241133383828242840000000000000000000000000000000000000000000000000000000000000000\r\n525252525262b20000000000a242627682828392000011a273b200a382729200525252525233b1b1b1b11333000000825353536382426282410000b30382a2a2\r\na1829200a2828382820182426200a2835262b1b10000831232b2000080014252000000000000a300000000000000000000000000000000000000000000000000\r\n528452232333b20000001100824262928201a20000b3720092000000830300002323525262b200000000b3720000a382828283828242522232b200b373928000\r\n000100110092a2829211a2133300a3825262b2000000a21333b20000868242520000000000000100009300000000000000000000000000000000000000000000\r\n525262122232b200a37672b2a24262838292000000b30300000000a3820300002232132333b200000000b303829300a2838292019242845262b2000000000000\r\n00a2b302b2a36182b302b200110000825262b200000000b1b10000a283a2425200000000a30082000083000000000000000000000094a4b4c4d4e4f400000000\r\n525262428462b200a28303b2214262928300000000b3030000000000a203e3415252222232b200000000b30392000000829200000042525262b2000000000000\r\n000000b100a2828200b100b302b211a25262b200000000000000000092b3428400000000827682000001009300000000000000000095a5b5c5d5e5f500000000\r\n232333132362b221008203b2711333008293858693b3031111111111114222225252845262b200001100b303b2000000821111111142528462b2000000000000\r\n000000000000110176851100b1b3026184621111111100000061000000b3135200000000828382670082768200000000000000000096a6b6c6d6e6f600000000\r\n82000000a203117200a203b200010193828283824353235353535353535252845252525262b200b37200b303b2000000824353535323235262b2000011000000\r\n0000000000b30282828372b26100b100525232122232b200000000000000b14200000000a28282123282839200000000000000000097a7b7c7d7e7f700000000\r\n9200110000135362b2001353535353539200a2000001828282829200b34252522323232362b261b30300b3030000000092b1b1b1b1b1b34262b200b372b20000\r\n001100000000b1a2828273b200000000232333132333b200001111000000b342000000868382125252328293a300000000000000000000000000000000000000\r\n00b372b200a28303b2000000a28293b3000000000000a2828382827612525252b1b1b1b173b200b30393b30361000000000000000000b34262b271b303b20000\r\nb302b211000000110092b100000000a3b1b1b1b1b1b10011111232110000b342000000a282125284525232828386000000000000000000000000000000000000\r\n80b303b20000820311111111008283b311111111110000829200928242528452000000a3820000b30382b37300000000000000000000b3426211111103b20000\r\n00b1b302b200b372b200000000000082b21000000000b31222522363b200b3138585868292425252525262018282860000000000000000000000000000000000\r\n00b373b20000a21353535363008292b32222222232111102b20000a21323525200000001839200b3038282820000000011111111930011425222222233b20000\r\n100000b10000b303b200000000858682b27100000000b3425233b1b1000000b182018283001323525284629200a2820000000000000000000000000000000000\r\n9300b100000000b1b1b1b1b100a200b323232323235363b100000000b1b1135200000000820000b30382839200000000222222328283432323232333b2000000\r\n329300000000b373b200000000a20182111111110000b31333b100a30061000000a28293f3123242522333020000820000000000000000000000000000000000\r\n829200001000410000000000000000b39310d30000a28200000000000000824200000086827600b30300a282760000005252526200828200a30182a2006100a3\r\n62820000000000b100000093a382838222222232b20000b1b1000083000000860000122222526213331222328293827600000000000000000000000000000000\r\n017685a31222321111111111002100b322223293000182930000000080a301131000a383829200b373000083920000005284526200a282828283920000000082\r\n62839321000000000000a3828282820152845262b261000093000082a300a3821000135252845222225252523201838200000000000000000000000000000000\r\n828382824252522222222232007100b352526282a38283820000000000838282320001828200000083000082010000005252526271718283820000000000a382\r\n628201729300000000a282828382828252528462b20000a38300a382018283821222324252525252525284525222223200000000000000000000000000000000";
        input = Regex.Replace(input, "\\s+", "");
        tilemap = new byte[input.Length / 2];
        int k = 0;
        int length = input.Length;
        int num = length / 2;
        for (; k < length; k += 2)
        {
            char c = input[k];
            char c2 = input[k + 1];
            string s = ((k < num) ? (c.ToString() + c2) : (c2.ToString() + c));
            tilemap[k / 2] = (byte)int.Parse(s, NumberStyles.HexNumber);
        }
        MTexture mTexture2 = GFX.Game["pico8/font"];
        font = new MTexture[mTexture2.Width / 4 * (mTexture2.Height / 6)];
        for (int l = 0; l < mTexture2.Height / 6; l++)
        {
            for (int m = 0; m < mTexture2.Width / 4; m++)
            {
                font[m + l * (mTexture2.Width / 4)] = mTexture2.GetSubtexture(m * 4, l * 6, 4, 6);
            }
        }
        picoBootLogo = GFX.Game["pico8/logo"];
        ResetScreen();
        Audio.SetMusic(null);
        Audio.SetAmbience(null);
    }
}

public class FuckingAwesomeClassic
{
    private class Cloud
    {
        public float x;

        public float y;

        public float spd;

        public float w;
    }

    private class Particle
    {
        public float x;

        public float y;

        public int s;

        public float spd;

        public float off;

        public int c;
    }

    private class DeadParticle
    {
        public float x;

        public float y;

        public int t;

        public Vector2 spd;
    }

    public class player : ClassicObject
    {
        public bool p_jump;

        public bool p_dash;

        public int grace;

        public int jbuffer;

        public int djump;

        public int dash_time;

        public int dash_effect_time;

        public Vector2 dash_target = new Vector2(0f, 0f);

        public Vector2 dash_accel = new Vector2(0f, 0f);

        public float spr_off;

        public bool was_on_ground;

        public player_hair hair;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            //IL_0024: Unknown result type (might be due to invalid IL or missing references)
            //IL_0029: Unknown result type (might be due to invalid IL or missing references)
            base.init(g, e);
            spr = 1f;
            djump = g.max_djump;
            hitbox = new Rectangle(1, 3, 6, 5);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (G.pause_player)
            {
                return;
            }
            int num = (E.btn(G.k_right) ? 1 : (E.btn(G.k_left) ? (-1) : 0));
            if (G.spikes_at(x + hitbox.X, y + hitbox.Y, hitbox.Width, hitbox.Height, spd.X, spd.Y))
            {
                G.kill_player(this);
            }
            if (y > 128f)
            {
                G.kill_player(this);
            }
            bool flag = is_solid(0, 1);
            bool flag2 = is_ice(0, 1);
            if (flag && !was_on_ground)
            {
                G.init_object(new smoke(), x, y + 4f);
            }
            bool num2 = E.btn(G.k_jump) && !p_jump;
            p_jump = E.btn(G.k_jump);
            if (num2)
            {
                jbuffer = 4;
            }
            else if (jbuffer > 0)
            {
                jbuffer--;
            }
            bool flag3 = E.btn(G.k_dash) && !p_dash;
            p_dash = E.btn(G.k_dash);
            if (flag)
            {
                grace = 6;
                if (djump < G.max_djump)
                {
                    G.psfx(54);
                    djump = G.max_djump;
                }
            }
            else if (grace > 0)
            {
                grace--;
            }
            dash_effect_time--;
            if (dash_time > 0)
            {
                G.init_object(new smoke(), x, y);
                dash_time--;
                spd.X = G.appr(spd.X, dash_target.X, dash_accel.X);
                spd.Y = G.appr(spd.Y, dash_target.Y, dash_accel.Y);
            }
            else
            {
                int num3 = 1;
                float amount = 0.6f;
                float amount2 = 0.15f;
                if (!flag)
                {
                    amount = 0.4f;
                }
                else if (flag2)
                {
                    amount = 0.05f;
                    if (num == ((!flipX) ? 1 : (-1)))
                    {
                        amount = 0.05f;
                    }
                }
                if (E.abs(spd.X) > num3)
                {
                    spd.X = G.appr(spd.X, E.sign(spd.X) * num3, amount2);
                }
                else
                {
                    spd.X = G.appr(spd.X, num * num3, amount);
                }
                if (spd.X != 0f)
                {
                    flipX = spd.X < 0f;
                }
                float target = 2f;
                float num4 = 0.21f;
                if (E.abs(spd.Y) <= 0.15f)
                {
                    num4 *= 0.5f;
                }
                if (num != 0 && is_solid(num, 0) && !is_ice(num, 0))
                {
                    target = 0.4f;
                    if (E.rnd(10f) < 2f)
                    {
                        G.init_object(new smoke(), x + num * 6, y);
                    }
                }
                if (!flag)
                {
                    spd.Y = G.appr(spd.Y, target, num4);
                }
                if (jbuffer > 0)
                {
                    if (grace > 0)
                    {
                        G.psfx(1);
                        jbuffer = 0;
                        grace = 0;
                        spd.Y = -2f;
                        G.init_object(new smoke(), x, y + 4f);
                    }
                    else
                    {
                        int num5 = (is_solid(-3, 0) ? (-1) : (is_solid(3, 0) ? 1 : 0));
                        if (num5 != 0)
                        {
                            G.psfx(2);
                            jbuffer = 0;
                            spd.Y = -2f;
                            spd.X = -num5 * (num3 + 1);
                            if (!is_ice(num5 * 3, 0))
                            {
                                G.init_object(new smoke(), x + num5 * 6, y);
                            }
                        }
                    }
                }
                int num6 = 5;
                float num7 = num6 * 0.70710677f;
                if (djump > 0 && flag3)
                {
                    G.init_object(new smoke(), x, y);
                    djump--;
                    dash_time = 4;
                    G.has_dashed = true;
                    dash_effect_time = 10;
                    int num8 = E.dashDirectionX((!flipX) ? 1 : (-1));
                    int num9 = E.dashDirectionY((!flipX) ? 1 : (-1));
                    if (num8 != 0 && num9 != 0)
                    {
                        spd.X = num8 * num7;
                        spd.Y = num9 * num7;
                    }
                    else if (num8 != 0)
                    {
                        spd.X = num8 * num6;
                        spd.Y = 0f;
                    }
                    else if (num9 != 0)
                    {
                        spd.X = 0f;
                        spd.Y = num9 * num6;
                    }
                    else
                    {
                        spd.X = ((!flipX) ? 1 : (-1));
                        spd.Y = 0f;
                    }
                    G.psfx(3);
                    G.freeze = 2;
                    G.shake = 6;
                    dash_target.X = 2 * E.sign(spd.X);
                    dash_target.Y = 2 * E.sign(spd.Y);
                    dash_accel.X = 1.5f;
                    dash_accel.Y = 1.5f;
                    if (spd.Y < 0f)
                    {
                        dash_target.Y *= 0.75f;
                    }
                    if (spd.Y != 0f)
                    {
                        dash_accel.X *= 0.70710677f;
                    }
                    if (spd.X != 0f)
                    {
                        dash_accel.Y *= 0.70710677f;
                    }
                }
                else if (flag3 && djump <= 0)
                {
                    G.psfx(9);
                    G.init_object(new smoke(), x, y);
                }
            }
            spr_off += 0.25f;
            if (!flag)
            {
                if (is_solid(num, 0))
                {
                    spr = 5f;
                }
                else
                {
                    spr = 3f;
                }
            }
            else if (E.btn(G.k_down))
            {
                spr = 6f;
            }
            else if (E.btn(G.k_up))
            {
                spr = 7f;
            }
            else if (spd.X == 0f || (!E.btn(G.k_left) && !E.btn(G.k_right)))
            {
                spr = 1f;
            }
            else
            {
                spr = 1f + spr_off % 4f;
            }
            if (y < -4f && G.level_index() < 30)
            {
                G.next_room();
            }
            was_on_ground = flag;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            if (x < -1f || x > 121f)
            {
                x = G.clamp(x, -1f, 121f);
                spd.X = 0f;
            }
            hair.draw_hair(this, (!flipX) ? 1 : (-1), djump);
            G.draw_player(this, djump);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public player()
        {
        }//IL_000b: Unknown result type (might be due to invalid IL or missing references)
        //IL_0010: Unknown result type (might be due to invalid IL or missing references)
        //IL_0020: Unknown result type (might be due to invalid IL or missing references)
        //IL_0025: Unknown result type (might be due to invalid IL or missing references)

    }

    public class player_hair
    {
        private class node
        {
            public float x;

            public float y;

            public float size;
        }

        private node[] hair = new node[5];

        private FuckingAwesomeEmulator E;

        private FuckingAwesomeClassic G;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public player_hair(ClassicObject obj)
        {
            E = obj.E;
            G = obj.G;
            for (int i = 0; i <= 4; i++)
            {
                hair[i] = new node
                {
                    x = obj.x,
                    y = obj.y,
                    size = E.max(1f, E.min(2f, 3 - i))
                };
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void draw_hair(ClassicObject obj, int facing, int djump)
        {
            //IL_0086: Unknown result type (might be due to invalid IL or missing references)
            //IL_00a8: Unknown result type (might be due to invalid IL or missing references)
            int num = djump switch
            {
                2 => 7 + E.flr(G.frames / 3 % 2) * 4, 
                1 => 8, 
                _ => 12, 
            };
            Vector2 val = new(obj.x + 4f - facing * 2, obj.y + (E.btn(G.k_down) ? 4 : 3));
            node[] array = hair;
            foreach (node node in array)
            {
                node.x += (val.X - node.x) / 1.5f;
                node.y += (val.Y + 0.5f - node.y) / 1.5f;
                E.circfill(node.x, node.y, node.size, num);
                val = new(node.x, node.y);
            }
        }
    }

    public class player_spawn : ClassicObject
    {
        private Vector2 target;

        private int state;

        private int delay;

        private player_hair hair;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            //IL_0020: Unknown result type (might be due to invalid IL or missing references)
            //IL_0025: Unknown result type (might be due to invalid IL or missing references)
            base.init(g, e);
            spr = 3f;
            target = new Vector2(x, y);
            y = 128f;
            spd.Y = -4f;
            state = 0;
            delay = 0;
            solids = false;
            hair = new player_hair(this);
            E.sfx(4);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            //IL_00d3: Unknown result type (might be due to invalid IL or missing references)
            //IL_00d8: Unknown result type (might be due to invalid IL or missing references)
            if (state == 0)
            {
                if (y < target.Y + 16f)
                {
                    state = 1;
                    delay = 3;
                }
            }
            else if (state == 1)
            {
                spd.Y += 0.5f;
                if (spd.Y > 0f && delay > 0)
                {
                    spd.Y = 0f;
                    delay--;
                }
                if (spd.Y > 0f && y > target.Y)
                {
                    y = target.Y;
                    spd = new Vector2(0f, 0f);
                    state = 2;
                    delay = 5;
                    G.shake = 5;
                    G.init_object(new smoke(), x, y + 4f);
                    E.sfx(5);
                }
            }
            else if (state == 2)
            {
                delay--;
                spr = 6f;
                if (delay < 0)
                {
                    G.destroy_object(this);
                    G.init_object(new player(), x, y).hair = hair;
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            hair.draw_hair(this, 1, G.max_djump);
            G.draw_player(this, G.max_djump);
        }
    }

    public class spring : ClassicObject
    {
        public int hide_in;

        private int hide_for;

        private int delay;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (hide_for > 0)
            {
                hide_for--;
                if (hide_for <= 0)
                {
                    spr = 18f;
                    delay = 0;
                }
            }
            else if (spr == 18f)
            {
                player player = collide<player>(0, 0);
                if (player != null && player.spd.Y >= 0f)
                {
                    spr = 19f;
                    player.y = y - 4f;
                    player.spd.X *= 0.2f;
                    player.spd.Y = -3f;
                    player.djump = G.max_djump;
                    delay = 10;
                    G.init_object(new smoke(), x, y);
                    fall_floor fall_floor = collide<fall_floor>(0, 1);
                    if (fall_floor != null)
                    {
                        G.break_fall_floor(fall_floor);
                    }
                    G.psfx(8);
                }
            }
            else if (delay > 0)
            {
                delay--;
                if (delay <= 0)
                {
                    spr = 18f;
                }
            }
            if (hide_in > 0)
            {
                hide_in--;
                if (hide_in <= 0)
                {
                    hide_for = 60;
                    spr = 0f;
                }
            }
        }
    }

    public class balloon : ClassicObject
    {
        private float offset;

        private float start;

        private float timer;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            //IL_0031: Unknown result type (might be due to invalid IL or missing references)
            //IL_0036: Unknown result type (might be due to invalid IL or missing references)
            base.init(g, e);
            offset = E.rnd(1f);
            start = y;
            hitbox = new Rectangle(-1, -1, 10, 10);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (spr == 22f)
            {
                offset += 0.01f;
                y = start + E.sin(offset) * 2f;
                player player = collide<player>(0, 0);
                if (player != null && player.djump < G.max_djump)
                {
                    G.psfx(6);
                    G.init_object(new smoke(), x, y);
                    player.djump = G.max_djump;
                    spr = 0f;
                    timer = 60f;
                }
            }
            else if (timer > 0f)
            {
                timer -= 1f;
            }
            else
            {
                G.psfx(7);
                G.init_object(new smoke(), x, y);
                spr = 22f;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            if (spr == 22f)
            {
                E.spr(13f + offset * 8f % 3f, x, y + 6f);
                E.spr(spr, x, y);
            }
        }
    }

    public class fall_floor : ClassicObject
    {
        public int state;

        public bool solid = true;

        public int delay;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (state == 0)
            {
                if (check<player>(0, -1) || check<player>(-1, 0) || check<player>(1, 0))
                {
                    G.break_fall_floor(this);
                }
            }
            else if (state == 1)
            {
                delay--;
                if (delay <= 0)
                {
                    state = 2;
                    delay = 60;
                    collideable = false;
                }
            }
            else if (state == 2)
            {
                delay--;
                if (delay <= 0 && !check<player>(0, 0))
                {
                    G.psfx(7);
                    state = 0;
                    collideable = true;
                    G.init_object(new smoke(), x, y);
                }
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            if (state != 2)
            {
                if (state != 1)
                {
                    E.spr(23f, x, y);
                }
                else
                {
                    E.spr(23 + (15 - delay) / 5, x, y);
                }
            }
        }
    }

    public class smoke : ClassicObject
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            spr = 29f;
            spd.Y = -0.1f;
            spd.X = 0.3f + E.rnd(0.2f);
            x += -1f + E.rnd(2f);
            y += -1f + E.rnd(2f);
            flipX = G.maybe();
            flipY = G.maybe();
            solids = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            spr += 0.2f;
            if (spr >= 32f)
            {
                G.destroy_object(this);
            }
        }
    }

    public class fruit : ClassicObject
    {
        private float start;

        private float off;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            spr = 26f;
            start = y;
            off = 0f;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            player player = collide<player>(0, 0);
            if (player != null)
            {
                player.djump = G.max_djump;
                G.sfx_timer = 20;
                E.sfx(13);
                G.got_fruit.Add(1 + G.level_index());
                G.init_object(new lifeup(), x, y);
                G.destroy_object(this);
            }
            off += 1f;
            y = start + E.sin(off / 40f) * 2.5f;
        }
    }

    public class fly_fruit : ClassicObject
    {
        private float start;

        private bool fly;

        private float step = 0.5f;

        private float sfx_delay = 8f;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            start = y;
            solids = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (fly)
            {
                if (sfx_delay > 0f)
                {
                    sfx_delay -= 1f;
                    if (sfx_delay <= 0f)
                    {
                        G.sfx_timer = 20;
                        E.sfx(14);
                    }
                }
                spd.Y = G.appr(spd.Y, -3.5f, 0.25f);
                if (y < -16f)
                {
                    G.destroy_object(this);
                }
            }
            else
            {
                if (G.has_dashed)
                {
                    fly = true;
                }
                step += 0.05f;
                spd.Y = E.sin(step) * 0.5f;
            }
            player player = collide<player>(0, 0);
            if (player != null)
            {
                player.djump = G.max_djump;
                G.sfx_timer = 20;
                E.sfx(13);
                G.got_fruit.Add(1 + G.level_index());
                G.init_object(new lifeup(), x, y);
                G.destroy_object(this);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            float num = 0f;
            if (!fly)
            {
                if (E.sin(step) < 0f)
                {
                    num = 1f + E.max(0f, G.sign(y - start));
                }
            }
            else
            {
                num = (num + 0.25f) % 3f;
            }
            E.spr(45f + num, x - 6f, y - 2f, 1, 1, flipX: true);
            E.spr(spr, x, y);
            E.spr(45f + num, x + 6f, y - 2f);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public fly_fruit()
        {
        }
    }

    public class lifeup : ClassicObject
    {
        private int duration;

        private float flash;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            spd.Y = -0.25f;
            duration = 30;
            x -= 2f;
            y -= 4f;
            flash = 0f;
            solids = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            duration--;
            if (duration <= 0)
            {
                G.destroy_object(this);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            flash += 0.5f;
            E.print("1000", x - 2f, y, 7f + flash % 2f);
        }
    }

    public class fake_wall : ClassicObject
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            //IL_0007: Unknown result type (might be due to invalid IL or missing references)
            //IL_000c: Unknown result type (might be due to invalid IL or missing references)
            //IL_017b: Unknown result type (might be due to invalid IL or missing references)
            //IL_0180: Unknown result type (might be due to invalid IL or missing references)
            hitbox = new Rectangle(-1, -1, 18, 18);
            player player = collide<player>(0, 0);
            if (player != null && player.dash_effect_time > 0)
            {
                player.spd.X = -G.sign(player.spd.X) * 1.5f;
                player.spd.Y = -1.5f;
                player.dash_time = -1;
                G.sfx_timer = 20;
                E.sfx(16);
                G.destroy_object(this);
                G.init_object(new smoke(), x, y);
                G.init_object(new smoke(), x + 8f, y);
                G.init_object(new smoke(), x, y + 8f);
                G.init_object(new smoke(), x + 8f, y + 8f);
                G.init_object(new fruit(), x + 4f, y + 4f);
            }
            hitbox = new Rectangle(0, 0, 16, 16);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            E.spr(64f, x, y);
            E.spr(65f, x + 8f, y);
            E.spr(80f, x, y + 8f);
            E.spr(81f, x + 8f, y + 8f);
        }
    }

    public class key : ClassicObject
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            int num = E.flr(spr);
            spr = 9f + (E.sin(G.frames / 30f) + 0.5f) * 1f;
            int num2 = E.flr(spr);
            if (num2 == 10 && num2 != num)
            {
                flipX = !flipX;
            }
            if (check<player>(0, 0))
            {
                E.sfx(23);
                G.sfx_timer = 20;
                G.destroy_object(this);
                G.has_key = true;
            }
        }
    }

    public class chest : ClassicObject
    {
        private float start;

        private float timer;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            x -= 4f;
            start = x;
            timer = 20f;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            if (G.has_key)
            {
                timer -= 1f;
                x = start - 1f + E.rnd(3f);
                if (timer <= 0f)
                {
                    G.sfx_timer = 20;
                    E.sfx(16);
                    G.init_object(new fruit(), x, y - 4f);
                    G.destroy_object(this);
                }
            }
        }
    }

    public class platform : ClassicObject
    {
        public float dir;

        private float last;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            x -= 4f;
            solids = false;
            hitbox.Width = 16;
            last = x;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void update()
        {
            spd.X = dir * 0.65f;
            if (x < -16f)
            {
                x = 128f;
            }
            if (x > 128f)
            {
                x = -16f;
            }
            if (!check<player>(0, 0))
            {
                collide<player>(0, -1)?.move_x((int)(x - last), 1);
            }
            last = x;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            E.spr(11f, x, y - 1f);
            E.spr(12f, x + 8f, y - 1f);
        }
    }

    public class message : ClassicObject
    {
        private float last;

        private float index;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            //IL_0093: Unknown result type (might be due to invalid IL or missing references)
            //IL_009f: Unknown result type (might be due to invalid IL or missing references)
            //IL_00ab: Unknown result type (might be due to invalid IL or missing references)
            //IL_00b7: Unknown result type (might be due to invalid IL or missing references)
            //IL_00eb: Unknown result type (might be due to invalid IL or missing references)
            //IL_00f1: Unknown result type (might be due to invalid IL or missing references)
            string text = "-- celeste mountain --#this memorial to those# perished on the climb";
            if (check<player>(4, 0))
            {
                if (index < text.Length)
                {
                    index += 0.5f;
                    if (index >= last + 1f)
                    {
                        last += 1f;
                        E.sfx(35);
                    }
                }

                Vector2 val = new(8f, 96f);
                for (int i = 0; i < index; i++)
                {
                    if (text[i] != '#')
                    {
                        E.rectfill(val.X - 2f, val.Y - 2f, val.X + 7f, val.Y + 6f, 7f);
                        E.print(text[i].ToString() ?? "", val.X, val.Y, 0f);
                        val.X += 5f;
                    }
                    else
                    {
                        val.X = 8f;
                        val.Y += 7f;
                    }
                }
            }
            else
            {
                index = 0f;
                last = 0f;
            }
        }
    }

    public class big_chest : ClassicObject
    {
        private class particle
        {
            public float x;

            public float y;

            public float h;

            public float spd;
        }

        private int state;

        private float timer;

        private List<particle> particles;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            hitbox.Width = 16;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            if (state == 0)
            {
                player player = collide<player>(0, 8);
                if (player != null && player.is_solid(0, 1))
                {
                    E.music(-1, 500, 7);
                    E.sfx(37);
                    G.pause_player = true;
                    player.spd.X = 0f;
                    player.spd.Y = 0f;
                    state = 1;
                    G.init_object(new smoke(), x, y);
                    G.init_object(new smoke(), x + 8f, y);
                    timer = 60f;
                    particles = new List<particle>();
                }
                E.spr(96f, x, y);
                E.spr(97f, x + 8f, y);
            }
            else if (state == 1)
            {
                timer -= 1f;
                G.shake = 5;
                G.flash_bg = true;
                if (timer <= 45f && particles.Count < 50)
                {
                    particles.Add(new particle
                    {
                        x = 1f + E.rnd(14f),
                        y = 0f,
                        h = 32f + E.rnd(32f),
                        spd = 8f + E.rnd(8f)
                    });
                }
                if (timer < 0f)
                {
                    state = 2;
                    particles.Clear();
                    G.flash_bg = false;
                    G.new_bg = true;
                    G.init_object(new orb(), x + 4f, y + 4f);
                    G.pause_player = false;
                }
                foreach (particle particle in particles)
                {
                    particle.y += particle.spd;
                    E.rectfill(x + particle.x, y + 8f - particle.y, x + particle.x, E.min(y + 8f - particle.y + particle.h, y + 8f), 7f);
                }
            }
            E.spr(112f, x, y + 8f);
            E.spr(113f, x + 8f, y + 8f);
        }
    }

    public class orb : ClassicObject
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            spd.Y = -4f;
            solids = false;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            spd.Y = G.appr(spd.Y, 0f, 0.5f);
            player player = collide<player>(0, 0);
            if (spd.Y == 0f && player != null)
            {
                G.music_timer = 45;
                E.sfx(51);
                G.freeze = 10;
                G.shake = 10;
                G.destroy_object(this);
                G.max_djump = 2;
                player.djump = 2;
            }
            E.spr(102f, x, y);
            float num = G.frames / 30f;
            for (int i = 0; i <= 7; i++)
            {
                E.circfill(x + 4f + E.cos(num + i / 8f) * 8f, y + 4f + E.sin(num + i / 8f) * 8f, 1f, 7f);
            }
        }
    }

    public class flag : ClassicObject
    {
        private float score;

        private bool show;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            base.init(g, e);
            x += 5f;
            score = G.got_fruit.Count;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            spr = 118f + G.frames / 5f % 3f;
            E.spr(spr, x, y);
            if (show)
            {
                E.rectfill(32f, 2f, 96f, 31f, 0f);
                E.spr(26f, 55f, 6f);
                E.print("x" + score, 64f, 9f, 7f);
                G.draw_time(49, 16);
                E.print("deaths:" + G.deaths, 48f, 24f, 7f);
            }
            else if (check<player>(0, 0))
            {
                E.SceneAs<Level>().Session.SetFlag("game_complete");
                E.sfx(55);
                G.sfx_timer = 30;
                show = true;
            }
        }
    }

    public class room_title : ClassicObject
    {
        private float delay = 5f;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public override void draw()
        {
            delay -= 1f;
            if (delay < -30f)
            {
                G.destroy_object(this);
            }
            else if (delay < 0f)
            {
                E.rectfill(24f, 58f, 104f, 70f, 0f);
                if (G.room.X == 3 && G.room.Y == 1)
                {
                    E.print("old site", 48f, 62f, 7f);
                }
                else if (G.level_index() == 30)
                {
                    E.print("summit", 52f, 62f, 7f);
                }
                else
                {
                    int num = (1 + G.level_index()) * 100;
                    E.print(num + "m", 52 + ((num < 1000) ? 2 : 0), 62f, 7f);
                }
                G.draw_time(4, 4);
            }
        }
    }

    public class ClassicObject
    {
        public FuckingAwesomeClassic G;

        public FuckingAwesomeEmulator E;

        public int type;

        public bool collideable = true;

        public bool solids = true;

        public float spr;

        public bool flipX;

        public bool flipY;

        public float x;

        public float y;

        public Rectangle hitbox = new Rectangle(0, 0, 8, 8);

        public Vector2 spd = new Vector2(0f, 0f);

        public Vector2 rem = new Vector2(0f, 0f);

        public virtual void init(FuckingAwesomeClassic g, FuckingAwesomeEmulator e)
        {
            G = g;
            E = e;
        }

        public virtual void update()
        {
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public virtual void draw()
        {
            if (spr > 0f)
            {
                E.spr(spr, x, y, 1, 1, flipX, flipY);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool is_solid(int ox, int oy)
        {
            if (oy > 0 && !check<platform>(ox, 0) && check<platform>(ox, oy))
            {
                return true;
            }
            if (!G.solid_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height) && !check<fall_floor>(ox, oy))
            {
                return check<fake_wall>(ox, oy);
            }
            return true;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public bool is_ice(int ox, int oy)
        {
            return G.ice_at(x + hitbox.X + ox, y + hitbox.Y + oy, hitbox.Width, hitbox.Height);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public T collide<T>(int ox, int oy) where T : ClassicObject
        {
            Type typeFromHandle = typeof(T);
            foreach (ClassicObject @object in G.objects)
            {
                if (@object != null && @object.GetType() == typeFromHandle && @object != this && @object.collideable && @object.x + @object.hitbox.X + @object.hitbox.Width > x + hitbox.X + ox && @object.y + @object.hitbox.Y + @object.hitbox.Height > y + hitbox.Y + oy && @object.x + @object.hitbox.X < x + hitbox.X + hitbox.Width + ox && @object.y + @object.hitbox.Y < y + hitbox.Y + hitbox.Height + oy)
                {
                    return @object as T;
                }
            }
            return null;
        }

        public bool check<T>(int ox, int oy) where T : ClassicObject
        {
            return collide<T>(ox, oy) != null;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void move(float ox, float oy)
        {
            int num = 0;
            rem.X += ox;
            num = E.flr(rem.X + 0.5f);
            rem.X -= num;
            move_x(num, 0);
            rem.Y += oy;
            num = E.flr(rem.Y + 0.5f);
            rem.Y -= num;
            move_y(num);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void move_x(int amount, int start)
        {
            if (solids)
            {
                int num = G.sign(amount);
                for (int i = start; i <= E.abs(amount); i++)
                {
                    if (!is_solid(num, 0))
                    {
                        x += num;
                        continue;
                    }
                    spd.X = 0f;
                    rem.X = 0f;
                    break;
                }
            }
            else
            {
                x += amount;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public void move_y(int amount)
        {
            if (solids)
            {
                int num = G.sign(amount);
                for (int i = 0; i <= E.abs(amount); i++)
                {
                    if (!is_solid(0, num))
                    {
                        y += num;
                        continue;
                    }
                    spd.Y = 0f;
                    rem.Y = 0f;
                    break;
                }
            }
            else
            {
                y += amount;
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ClassicObject()
        {
        }//IL_0013: Unknown result type (might be due to invalid IL or missing references)
        //IL_0018: Unknown result type (might be due to invalid IL or missing references)
        //IL_0028: Unknown result type (might be due to invalid IL or missing references)
        //IL_002d: Unknown result type (might be due to invalid IL or missing references)
        //IL_003d: Unknown result type (might be due to invalid IL or missing references)
        //IL_0042: Unknown result type (might be due to invalid IL or missing references)

    }

    public FuckingAwesomeEmulator E;

    private Point room;

    private List<ClassicObject> objects;

    public int freeze;

    private int shake;

    private bool will_restart;

    private int delay_restart;

    private HashSet<int> got_fruit;

    private bool has_dashed;

    private int sfx_timer;

    private bool has_key;

    private bool pause_player;

    private bool flash_bg;

    private int music_timer;

    private bool new_bg;

    private int k_left;

    private int k_right = 1;

    private int k_up = 2;

    private int k_down = 3;

    private int k_jump = 4;

    private int k_dash = 5;

    private int frames;

    private int seconds;

    private int minutes;

    private int deaths;

    private int max_djump;

    private bool start_game;

    private int start_game_flash;

    private bool room_just_loaded;

    private List<Cloud> clouds;

    private List<Particle> particles;

    private List<DeadParticle> dead_particles;

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Init(FuckingAwesomeEmulator Emulator)
    {
        //IL_000a: Unknown result type (might be due to invalid IL or missing references)
        //IL_000f: Unknown result type (might be due to invalid IL or missing references)
        E = Emulator;
        room = new Point(0, 0);
        objects = new List<ClassicObject>();
        freeze = 0;
        will_restart = false;
        delay_restart = 0;
        got_fruit = new HashSet<int>();
        has_dashed = false;
        sfx_timer = 0;
        has_key = false;
        pause_player = false;
        flash_bg = false;
        music_timer = 0;
        new_bg = false;
        room_just_loaded = false;
        frames = 0;
        seconds = 0;
        minutes = 0;
        deaths = 0;
        max_djump = 1;
        start_game = false;
        start_game_flash = 0;
        clouds = new List<Cloud>();
        for (int i = 0; i <= 16; i++)
        {
            clouds.Add(new Cloud
            {
                x = E.rnd(128f),
                y = E.rnd(128f),
                spd = 1f + E.rnd(4f),
                w = 32f + E.rnd(32f)
            });
        }
        particles = new List<Particle>();
        for (int j = 0; j <= 32; j++)
        {
            particles.Add(new Particle
            {
                x = E.rnd(128f),
                y = E.rnd(128f),
                s = E.flr(E.rnd(5f) / 4f),
                spd = 0.25f + E.rnd(5f),
                off = E.rnd(1f),
                c = 6 + E.flr(0.5f + E.rnd(1f))
            });
        }
        dead_particles = new List<DeadParticle>();
        title_screen();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void title_screen()
    {
        got_fruit = new HashSet<int>();
        frames = 0;
        deaths = 0;
        max_djump = 1;
        start_game = false;
        start_game_flash = 0;
        E.music(40, 0, 7);
        load_room(7, 3);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void begin_game()
    {
        frames = 0;
        seconds = 0;
        minutes = 0;
        music_timer = 0;
        start_game = false;
        E.music(0, 0, 7);
        load_room(0, 0);
        E.SceneAs<Level>().Session.SetFlag("game_started");
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int level_index()
    {
        return room.X % 8 + room.Y * 8;
    }

    private bool is_title()
    {
        return level_index() == 31;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void psfx(int num)
    {
        if (sfx_timer <= 0)
        {
            E.sfx(num);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void draw_player(ClassicObject obj, int djump)
    {
        int num = 0;
        switch (djump)
        {
            case 2:
                num = ((E.flr(frames / 3 % 2) != 0) ? 144 : 160);
                break;
            case 0:
                num = 128;
                break;
        }
        E.spr(obj.spr + num, obj.x, obj.y, 1, 1, obj.flipX, obj.flipY);
    }

    private void break_spring(spring obj)
    {
        obj.hide_in = 15;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void break_fall_floor(fall_floor obj)
    {
        if (obj.state == 0)
        {
            psfx(15);
            obj.state = 1;
            obj.delay = 15;
            init_object(new smoke(), obj.x, obj.y);
            spring spring = obj.collide<spring>(0, -1);
            if (spring != null)
            {
                break_spring(spring);
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private T init_object<T>(T obj, float x, float y, int? tile = null) where T : ClassicObject
    {
        objects.Add(obj);
        if (tile.HasValue)
        {
            obj.spr = tile.Value;
        }
        obj.x = (int)x;
        obj.y = (int)y;
        obj.init(this, E);
        return obj;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void destroy_object(ClassicObject obj)
    {
        int num = objects.IndexOf(obj);
        if (num >= 0)
        {
            objects[num] = null;
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void kill_player(player obj)
    {
        //IL_00b2: Unknown result type (might be due to invalid IL or missing references)
        //IL_00b7: Unknown result type (might be due to invalid IL or missing references)
        sfx_timer = 12;
        E.sfx(0);
        deaths++;
        shake = 10;
        destroy_object(obj);
        dead_particles.Clear();
        for (int i = 0; i <= 7; i++)
        {
            float num = i / 8f;
            dead_particles.Add(new DeadParticle
            {
                x = obj.x + 4f,
                y = obj.y + 4f,
                t = 10,
                spd = new Vector2(E.cos(num) * 3f, E.sin(num + 0.5f) * 3f)
            });
        }

        Player p = E.Scene.Tracker.GetEntity<Player>();
        if (p != null) p.Die(Vector2.Zero);
        restart_room();
    }

    private void restart_room()
    {
        will_restart = true;
        delay_restart = 15;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void next_room()
    {
        if (room.X == 2 && room.Y == 1)
        {
            E.music(30, 500, 7);
        }
        else if (room.X == 3 && room.Y == 1)
        {
            E.music(20, 500, 7);
        }
        else if (room.X == 4 && room.Y == 2)
        {
            E.music(30, 500, 7);
        }
        else if (room.X == 5 && room.Y == 3)
        {
            E.music(30, 500, 7);
        }
        if (room.X == 7)
        {
            load_room(0, room.Y + 1);
        }
        else
        {
            load_room(room.X + 1, room.Y);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void load_room(int x, int y)
    {
        room_just_loaded = true;
        has_dashed = false;
        has_key = false;
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i] = null;
        }
        room.X = x;
        room.Y = y;
        for (int j = 0; j <= 15; j++)
        {
            for (int k = 0; k <= 15; k++)
            {
                int num = E.mget(room.X * 16 + j, room.Y * 16 + k);
                switch (num)
                {
                    case 11:
                        init_object(new platform(), j * 8, k * 8).dir = -1f;
                        continue;
                    case 12:
                        init_object(new platform(), j * 8, k * 8).dir = 1f;
                        continue;
                }
                ClassicObject classicObject = null;
                switch (num)
                {
                    case 1:
                        classicObject = new player_spawn();
                        break;
                    case 18:
                        classicObject = new spring();
                        break;
                    case 22:
                        classicObject = new balloon();
                        break;
                    case 23:
                        classicObject = new fall_floor();
                        break;
                    case 86:
                        classicObject = new message();
                        break;
                    case 96:
                        classicObject = new big_chest();
                        break;
                    case 118:
                        classicObject = new flag();
                        break;
                    default:
                        if (!got_fruit.Contains(1 + level_index()))
                        {
                            switch (num)
                            {
                                case 26:
                                    classicObject = new fruit();
                                    break;
                                case 28:
                                    classicObject = new fly_fruit();
                                    break;
                                case 64:
                                    classicObject = new fake_wall();
                                    break;
                                case 8:
                                    classicObject = new key();
                                    break;
                                case 20:
                                    classicObject = new chest();
                                    break;
                            }
                        }
                        break;
                }
                if (classicObject != null)
                {
                    init_object(classicObject, j * 8, k * 8, num);
                }
            }
        }
        if (!is_title())
        {
            init_object(new room_title(), 0f, 0f);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Update()
    {
        frames = (frames + 1) % 30;
        if (frames == 0 && level_index() < 30)
        {
            seconds = (seconds + 1) % 60;
            if (seconds == 0)
            {
                minutes++;
            }
        }
        if (music_timer > 0)
        {
            music_timer--;
            if (music_timer <= 0)
            {
                E.music(10, 0, 7);
            }
        }
        if (sfx_timer > 0)
        {
            sfx_timer--;
        }
        if (freeze > 0)
        {
            freeze--;
            return;
        }
        if (shake > 0 && Settings.Instance.ScreenShake != ScreenshakeAmount.Off)
        {
            shake--;
            E.camera();
            if (shake > 0)
            {
                if (Settings.Instance.ScreenShake == ScreenshakeAmount.On)
                {
                    E.camera(-2f + E.rnd(5f), -2f + E.rnd(5f));
                }
                else
                {
                    E.camera(-1f + E.rnd(3f), -1f + E.rnd(3f));
                }
            }
        }
        if (will_restart && delay_restart > 0)
        {
            delay_restart--;
            if (delay_restart <= 0)
            {
                will_restart = true;
                load_room(room.X, room.Y);
            }
        }
        room_just_loaded = false;
        int num = 0;
        while (num != -1)
        {
            int i = num;
            num = -1;
            for (; i < objects.Count; i++)
            {
                ClassicObject classicObject = objects[i];
                if (classicObject != null)
                {
                    classicObject.move(classicObject.spd.X, classicObject.spd.Y);
                    classicObject.update();
                    if (room_just_loaded)
                    {
                        room_just_loaded = false;
                        num = i;
                        break;
                    }
                }
            }
            while (objects.IndexOf(null) >= 0)
            {
                objects.Remove(null);
            }
        }
        if (!is_title())
        {
            return;
        }
        if (!start_game && (E.btn(k_jump) || E.btn(k_dash)))
        {
            E.music(-1, 0, 0);
            start_game_flash = 50;
            start_game = true;
            E.sfx(38);
        }
        if (start_game)
        {
            start_game_flash--;
            if (start_game_flash <= -30)
            {
                begin_game();
            }
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public void Draw()
    {
        E.pal();
        if (start_game)
        {
            int num = 10;
            if (start_game_flash <= 10)
            {
                num = ((start_game_flash > 5) ? 2 : ((start_game_flash > 0) ? 1 : 0));
            }
            else if (frames % 10 < 5)
            {
                num = 7;
            }
            if (num < 10)
            {
                E.pal(6, num);
                E.pal(12, num);
                E.pal(13, num);
                E.pal(5, num);
                E.pal(1, num);
                E.pal(7, num);
            }
        }
        int num2 = 0;
        if (flash_bg)
        {
            num2 = frames / 5;
        }
        else if (new_bg)
        {
            num2 = 2;
        }
        E.rectfill(0f, 0f, 128f, 128f, num2);
        if (!is_title())
        {
            foreach (Cloud cloud in clouds)
            {
                cloud.x += cloud.spd;
                E.rectfill(cloud.x, cloud.y, cloud.x + cloud.w, cloud.y + 4f + (1f - cloud.w / 64f) * 12f, (!new_bg) ? 1 : 14);
                if (cloud.x > 128f)
                {
                    cloud.x = 0f - cloud.w;
                    cloud.y = E.rnd(120f);
                }
            }
        }
        E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 2);
        for (int i = 0; i < objects.Count; i++)
        {
            ClassicObject classicObject = objects[i];
            if (classicObject != null && (classicObject is platform || classicObject is big_chest))
            {
                draw_object(classicObject);
            }
        }
        int tx = (is_title() ? (-4) : 0);
        E.map(room.X * 16, room.Y * 16, tx, 0, 16, 16, 1);
        for (int j = 0; j < objects.Count; j++)
        {
            ClassicObject classicObject2 = objects[j];
            if (classicObject2 != null && !(classicObject2 is platform) && !(classicObject2 is big_chest))
            {
                draw_object(classicObject2);
            }
        }
        E.map(room.X * 16, room.Y * 16, 0, 0, 16, 16, 3);
        foreach (Particle particle in particles)
        {
            particle.x += particle.spd;
            particle.y += E.sin(particle.off);
            particle.off += E.min(0.05f, particle.spd / 32f);
            E.rectfill(particle.x, particle.y, particle.x + particle.s, particle.y + particle.s, particle.c);
            if (particle.x > 132f)
            {
                particle.x = -4f;
                particle.y = E.rnd(128f);
            }
        }
        for (int num3 = dead_particles.Count - 1; num3 >= 0; num3--)
        {
            DeadParticle deadParticle = dead_particles[num3];
            deadParticle.x += deadParticle.spd.X;
            deadParticle.y += deadParticle.spd.Y;
            deadParticle.t--;
            if (deadParticle.t <= 0)
            {
                dead_particles.RemoveAt(num3);
            }
            E.rectfill(deadParticle.x - deadParticle.t / 5, deadParticle.y - deadParticle.t / 5, deadParticle.x + deadParticle.t / 5, deadParticle.y + deadParticle.t / 5, 14 + deadParticle.t % 2);
        }
        E.rectfill(-5f, -5f, -1f, 133f, 0f);
        E.rectfill(-5f, -5f, 133f, -1f, 0f);
        E.rectfill(-5f, 128f, 133f, 133f, 0f);
        E.rectfill(128f, -5f, 133f, 133f, 0f);
        if (is_title())
        {
            E.print("press button", 42f, 96f, 5f);
        }
        if (level_index() != 30)
        {
            return;
        }
        ClassicObject classicObject3 = null;
        foreach (ClassicObject @object in objects)
        {
            if (@object is player)
            {
                classicObject3 = @object;
                break;
            }
        }
        if (classicObject3 != null)
        {
            float num4 = E.min(24f, 40f - E.abs(classicObject3.x + 4f - 64f));
            E.rectfill(0f, 0f, num4, 128f, 0f);
            E.rectfill(128f - num4, 0f, 128f, 128f, 0f);
        }
    }

    private void draw_object(ClassicObject obj)
    {
        obj.draw();
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private void draw_time(int x, int y)
    {
        int num = seconds;
        int num2 = minutes % 60;
        int num3 = E.flr(minutes / 60);
        E.rectfill(x, y, x + 32, y + 6, 0f);
        E.print(((num3 < 10) ? "0" : "") + num3 + ":" + ((num2 < 10) ? "0" : "") + num2 + ":" + ((num < 10) ? "0" : "") + num, x + 1, y + 1, 7f);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private float clamp(float val, float a, float b)
    {
        return E.max(a, E.min(b, val));
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private float appr(float val, float target, float amount)
    {
        if (!(val > target))
        {
            return E.min(val + amount, target);
        }
        return E.max(val - amount, target);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int sign(float v)
    {
        if (!(v > 0f))
        {
            if (!(v < 0f))
            {
                return 0;
            }
            return -1;
        }
        return 1;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool maybe()
    {
        return E.rnd(1f) < 0.5f;
    }

    private bool solid_at(float x, float y, float w, float h)
    {
        return tile_flag_at(x, y, w, h, 0);
    }

    private bool ice_at(float x, float y, float w, float h)
    {
        return tile_flag_at(x, y, w, h, 4);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool tile_flag_at(float x, float y, float w, float h, int flag)
    {
        for (int i = (int)E.max(0f, E.flr(x / 8f)); i <= E.min(15f, (x + w - 1f) / 8f); i++)
        {
            for (int j = (int)E.max(0f, E.flr(y / 8f)); j <= E.min(15f, (y + h - 1f) / 8f); j++)
            {
                if (E.fget(tile_at(i, j), flag))
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private int tile_at(int x, int y)
    {
        return E.mget(room.X * 16 + x, room.Y * 16 + y);
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private bool spikes_at(float x, float y, int w, int h, float xspd, float yspd)
    {
        for (int i = (int)E.max(0f, E.flr(x / 8f)); i <= E.min(15f, (x + w - 1f) / 8f); i++)
        {
            for (int j = (int)E.max(0f, E.flr(y / 8f)); j <= E.min(15f, (y + h - 1f) / 8f); j++)
            {
                int num = tile_at(i, j);
                if (num == 17 && (E.mod(y + h - 1f, 8f) >= 6f || y + h == j * 8 + 8) && yspd >= 0f)
                {
                    return true;
                }
                if (num == 27 && E.mod(y, 8f) <= 2f && yspd <= 0f)
                {
                    return true;
                }
                if (num == 43 && E.mod(x, 8f) <= 2f && xspd <= 0f)
                {
                    return true;
                }
                if (num == 59 && ((x + w - 1f) % 8f >= 6f || x + w == i * 8 + 8) && xspd >= 0f)
                {
                    return true;
                }
            }
        }
        return false;
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    public FuckingAwesomeClassic()
    {
    }
}

