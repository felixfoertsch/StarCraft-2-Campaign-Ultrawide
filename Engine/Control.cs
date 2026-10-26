using System;
using System.IO;
using System.Diagnostics;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Web.Script.Serialization;

static class EngineJson {
 public static Dictionary<string,object> Obj(params object[] pairs) { var d=new Dictionary<string,object>();for(int i=0;i<pairs.Length;i+=2)d.Add((string)pairs[i],pairs[i+1]);return d; }
 public static Dictionary<string,object> Read(string path) { return new JavaScriptSerializer().Deserialize<Dictionary<string,object>>(File.ReadAllText(path)); }
 public static void Write(string path,object value) { File.WriteAllText(path,new JavaScriptSerializer().Serialize(value)); }
 public static ulong Hex(object value) { return Convert.ToUInt64(Convert.ToString(value),16); }
 public static int Int(Dictionary<string,object> d,string key,int defaultValue=0) { object v;return d.TryGetValue(key,out v)&&v!=null?Convert.ToInt32(v):defaultValue; }
 public static string Str(Dictionary<string,object> d,string key) { object v;return d.TryGetValue(key,out v)?Convert.ToString(v):""; }
 public static bool Bool(Dictionary<string,object> d,string key) { object v;return d.TryGetValue(key,out v)&&v!=null&&Convert.ToBoolean(v); }
 public static bool Started(Dictionary<string,object> d,Process p) { return DateTime.Parse(Str(d,"Started"),CultureInfo.InvariantCulture,DateTimeStyles.RoundtripKind).ToUniversalTime().Ticks==p.StartTime.ToUniversalTime().Ticks; }
}
sealed class EngineGame : IDisposable {
 public Process Game;public ulong Base;readonly string error;
 public EngineGame(string readError,string processError) {
  error=readError;var games=Process.GetProcessesByName("SC2_x64");if(games.Length!=1){foreach(var p in games)p.Dispose();throw new Exception(processError);}Game=games[0];Base=(ulong)Game.MainModule.BaseAddress.ToInt64();SC2Addresses.Ensure(Game.Id,Base);
 }
 public byte[] Bytes(ulong a,int n) { var b=SC2Memory.Read(Game.Id,a,n);if(b==null||b.Length==0)throw new Exception(error);return b; }
 public ulong Ptr(ulong a) { return BitConverter.ToUInt64(Bytes(a,8),0); }
 public uint U32(ulong a) { return BitConverter.ToUInt32(Bytes(a,4),0); }
 public void Write(ulong a,byte[] before,byte[] after) { SC2HudHook.CheckedWrite(Game.Id,a,before,after); }
 public bool Matches(Dictionary<string,object> d) { return EngineJson.Int(d,"GamePid")==Game.Id&&EngineJson.Started(d,Game); }
 public void Dispose() { Game.Dispose(); }
}
static class EngineControl {
 public static Dictionary<string,object> Hud(string root,string action,int width,int height) { return EngineCenteredHud.Run(root,action,width,height); }
 public static object Resolution(string root,string action,int width,int height) {
  SC2CampaignModeHook.ValidateTarget(width,height);
  using(var g=new EngineGame("Cannot read graphics state","Open exactly one StarCraft II session first")) {
   string path=Path.Combine(root,"engine-reset-session.json");var p=g.Game;ulong b=g.Base,device=g.Ptr(b+SC2Addresses.Rva(0x43D0E08)),original=b+SC2Addresses.Rva(0x2DB8098),resourceTable=b+SC2Addresses.Rva(0x2DB9DA8);Dictionary<string,object> r=null;
   if(File.Exists(path)){var c=EngineJson.Read(path);if(g.Matches(c))r=c;}
   if(action=="Install") {
    SC2DisplayGate.Require(p.Id,b);
    if(r!=null&&EngineJson.Int(r,"GateRevision")!=3)throw new Exception("Close StarCraft II before using the updated campaign restrictions.");
    if(r!=null&&g.Ptr(device)==EngineJson.Hex(r["VTable"]))Console.WriteLine("Campaign ultrawide wrapper is already attached.");
    else {
     if(g.Ptr(device)!=original)throw new Exception("Unexpected graphics device type");
     if(g.Ptr(original+0x28)!=b+SC2Addresses.Rva(0xE60F70))throw new Exception("Mode selection method mismatch");
     ulong resource=g.Ptr(device+0x80);if(g.Ptr(resource)!=resourceTable)throw new Exception("Unexpected display resource type");uint packed=g.U32(resource+0x60);
     if((packed&0x3fff)<1280||((packed>>14)&0x3fff)<720)throw new Exception("Choose a widescreen fullscreen mode in Graphics options first");
     if((g.U32(resource+0x80)&255)!=0)throw new Exception("Use fullscreen display mode");
     var prepared=SC2CampaignModeHook.Prepare(p.Id,original,width,height);
     r=EngineJson.Obj("GateRevision",3,"GamePid",p.Id,"Started",p.StartTime.ToString("o"),"Build",p.MainModule.FileVersionInfo.FileVersion,"TargetWidth",width,"TargetHeight",height,"Device",device.ToString("X"),"OriginalVTable",original.ToString("X"),"VTable",prepared.VTable.ToString("X"),"Code",prepared.Code.ToString("X"),"State",prepared.State.ToString("X"),"Length",prepared.Length);
     EngineJson.Write(path,r);SC2DisplayGate.Require(p.Id,b);SC2CampaignModeHook.SwapPointer(p.Id,device,original,prepared.VTable);
     Console.WriteLine("Resolution enabled. Change the fullscreen resolution in Graphics options once to apply your selection.");
    }
   } else if(action=="Disable") {
    if(r==null){Console.WriteLine("No engine wrapper recorded for this game session.");return null;}
    SC2CampaignModeHook.Disable(p.Id,EngineJson.Hex(r["State"]));
    if(device==EngineJson.Hex(r["Device"])&&g.Ptr(device)==EngineJson.Hex(r["VTable"])) {SC2CampaignModeHook.SwapPointer(p.Id,device,EngineJson.Hex(r["VTable"]),EngineJson.Hex(r["OriginalVTable"]));Console.WriteLine("Resolution override disabled. Choose your preferred resolution in Graphics options, or close the game.");}
    else Console.WriteLine("Wrapper disabled; graphics device has already changed.");return null;
   }
   if(r==null){Console.WriteLine("No engine wrapper recorded for this game session.");return null;}
   byte[] state=g.Bytes(EngineJson.Hex(r["State"]),64);uint size=g.U32(g.Ptr(device+0x80)+0x60);
   return EngineJson.Obj("GamePid",p.Id,"Attached",device==EngineJson.Hex(r["Device"])&&g.Ptr(device)==EngineJson.Hex(r["VTable"]),"Enabled",BitConverter.ToUInt32(state,0)==1,"CurrentWidth",size&0x3fff,"CurrentHeight",(size>>14)&0x3fff,"ModeCalls",BitConverter.ToUInt32(state,32),"ModeOverrides",BitConverter.ToUInt32(state,36),"RequestedWidth",BitConverter.ToUInt32(state,40),"RequestedHeight",BitConverter.ToUInt32(state,44),"CachedWidth",g.U32(device+0x28),"CachedHeight",g.U32(device+0x2c));
  }
 }
 static Dictionary<string,object> ScaleResult(bool active,bool ready,int percent) { return EngineJson.Obj("Active",active,"Ready",ready,"ScalePercent",percent); }
 public static Dictionary<string,object> Scale(string root,string action,int width,int height,int percent) {
  var games=Process.GetProcessesByName("SC2_x64");if(games.Length!=1){foreach(var p in games)p.Dispose();return ScaleResult(false,true,100);}foreach(var p in games)p.Dispose();
  using(var g=new EngineGame("Campaign HUD is not ready.","Open exactly one StarCraft II campaign session first.")) {
   var p=g.Game;ulong b=g.Base,ui=g.Ptr(b+SC2Addresses.Rva(0x4032368));if(ui==0||g.Ptr(ui)!=b+SC2Addresses.Rva(0x2d522a8))return ScaleResult(false,false,100);
   ulong owner=g.Ptr(ui+0xc68),console=g.Ptr(ui+0xc60);string path=Path.Combine(root,"hud-scale-session.json");Dictionary<string,object> r=null;
   if(File.Exists(path)){var c=EngineJson.Read(path);if(EngineJson.Int(c,"Pid")==p.Id&&EngineJson.Started(c,p)&&Convert.ToUInt64(c["Owner"])==owner&&g.Ptr(owner)==Convert.ToUInt64(c["VTable"]))r=c;}
   if(r!=null) {
    ulong state=Convert.ToUInt64(r["State"]);int old=EngineJson.Int(r,"ScalePercent");if(g.U32(state+4)!=0||g.U32(state)!=0)return ScaleResult(true,false,old);
    if(EngineJson.Bool(r,"Restoring")){SC2HudScale.CheckedWrite(p.Id,owner,BitConverter.GetBytes(Convert.ToUInt64(r["VTable"])),BitConverter.GetBytes(Convert.ToUInt64(r["OriginalVTable"])));r=null;}
    else if(action=="Disable"||(action=="Enable"&&old!=percent)) {
     var ids=new List<SC2HudScale.Identity>();foreach(Dictionary<string,object> f in (System.Collections.IEnumerable)r["Frames"])ids.Add(new SC2HudScale.Identity{Frame=Convert.ToUInt64(f["Frame"]),VTable=Convert.ToUInt64(f["VTable"]),Parent=Convert.ToUInt64(f["Parent"])});
     SC2HudScale.GuardRestore(p.Id,b,state,EngineJson.Int(r,"CommandCount"),ids.ToArray());r["Restoring"]=true;EngineJson.Write(path,r);ScaleSignal(g,state,owner,2);return ScaleResult(true,false,old);
    }else return ScaleResult(true,true,old);
   }
   if(action!="Enable")return ScaleResult(false,true,100);
   SC2HudScale.ValidateScale(width,height,percent);SC2CampaignGate.Require(p.Id,b);string baselinePath=Path.Combine(root,"hud-runtime-session.json");if(!File.Exists(baselinePath))throw new Exception("Waiting for the centered campaign HUD.");
   var baseline=EngineJson.Read(baselinePath);ulong active=g.Ptr(owner);if(p.Id!=EngineJson.Int(baseline,"GamePid")||active!=EngineJson.Hex(baseline["VTable"]))throw new Exception("Waiting for the centered campaign HUD.");
   var plan=SC2HudScale.CreatePlan(p.Id,b,width,height,percent);if(plan.Commands.Length==0)return ScaleResult(false,true,percent);
   var prepared=SC2HudScale.Prepare(p.Id,owner,b+SC2Addresses.Rva(0x2d51c58),b+SC2Addresses.Rva(0x16bcfc0),plan.Commands,active,plan.CargoWidths);
   r=EngineJson.Obj("Pid",p.Id,"Started",p.StartTime.ToString("o"),"Ui",ui,"Owner",owner,"Console",console,"OriginalVTable",active,"State",prepared.State,"VTable",prepared.VTable,"Code",prepared.Code,"Length",prepared.Length,"CommandCount",plan.Commands.Length,"ScalePercent",percent,"Restoring",false,"Frames",plan.Frames);
   EngineJson.Write(path,r);SC2CampaignGate.Require(p.Id,b);
   if(g.Ptr(b+SC2Addresses.Rva(0x4032368))!=ui||g.Ptr(ui+0xc68)!=owner||g.Ptr(ui+0xc60)!=console)throw new Exception("Campaign HUD changed before resizing.");
   SC2HudScale.CheckedWrite(p.Id,owner,BitConverter.GetBytes(active),BitConverter.GetBytes(prepared.VTable));ScaleSignal(g,prepared.State,owner,1);return ScaleResult(true,false,percent);
  }
 }
 static void ScaleSignal(EngineGame g,ulong state,ulong owner,int request) { SC2HudScale.CheckedWrite(g.Game.Id,state,BitConverter.GetBytes(0),BitConverter.GetBytes(request));var f=g.Bytes(owner+0x28,1);SC2HudScale.CheckedWrite(g.Game.Id,owner+0x28,f,new[]{(byte)(f[0]|0x10)}); }
 static void Print(object result) { if(result!=null)Console.WriteLine(new JavaScriptSerializer().Serialize(result)); }
 public static int Run(string root,string action) {
  try {
   if(action=="SelfTest") {
    var json=new JavaScriptSerializer();var sample=json.Deserialize<Dictionary<string,object>>(json.Serialize(EngineJson.Obj("State",0x123456789abcdefUL,"Commands",new object[]{EngineJson.Obj("Frame","123ABC")},"Started",DateTime.Now.ToString("o"))));
    if(Convert.ToUInt64(sample["State"])!=0x123456789abcdefUL||!(sample["Commands"] is System.Collections.ArrayList)||EngineJson.Hex(((Dictionary<string,object>)((System.Collections.ArrayList)sample["Commands"])[0])["Frame"])!=0x123abcUL)throw new Exception("Session JSON round trip failed.");
    Console.WriteLine("PASS: session JSON preserves numeric pointers, hex strings and command arrays");
    SC2Addresses.UseTestAddresses();
    SC2CampaignModeHook.ModeSelfTest();Console.WriteLine("PASS: ModeSelfTest");SC2HudHook.SelfTest();Console.WriteLine("PASS: HudHook SelfTest");
    SC2HudScale.SelfTest();SC2HudScale.BatchSelfTest();SC2HudScale.TextSelfTest();SC2HudScale.ScaleSelfTest();SC2HudScale.DynamicSelfTest();SC2HudScale.CargoSelfTest();Console.WriteLine("PASS: HudScale SelfTest, BatchSelfTest, TextSelfTest, ScaleSelfTest, DynamicSelfTest, CargoSelfTest");
    SC2DisplayRefreshSchedule.SelfTest();Console.WriteLine("PASS: DisplayRefreshSchedule SelfTest");
    MissionZoomTests.Run();SC2HudScale.DynamicTextSelfTest();SC2HudScale.TextGateSelfTest();SC2HudScale.TextRestoreSelfTest();AutoStartTests.Run();HubCameraTests.Run();CampaignGateTests.Run();CampaignGateTests.RunQueue();CampaignGateTests.RunCargo();
    foreach(string path in new[]{@"C:\Games\StarCraft II\Versions\Base97563\SC2_x64.exe",@"D:\Another Library\SC2_x64.exe"})foreach(string version in new[]{"5.0.16.97563","5.0.16.99999","6.0.0.100000","",null})SC2AutoStart.ValidateExecutable(path,version);
    foreach(string path in new[]{@"C:\Games\other.exe",@"D:\Games\SC2.exe",""}){bool rejected=false;try{SC2AutoStart.ValidateExecutable(path,"6.0.0.100000");}catch{rejected=true;}if(!rejected)throw new Exception("Executable validation accepted an unsupported executable name.");}
    Console.WriteLine("PASS: native eligibility, zoom, hub, HUD text, installation and executable validation tests");return 0;
   }
   var settings=EngineJson.Read(Path.Combine(root,"resolution.json"));int w=EngineJson.Int(settings,"Width"),h=EngineJson.Int(settings,"Height");
   if(action=="EnableHUD"||action=="RestoreHUD") {DateTime deadline=DateTime.Now.AddMinutes(2);Dictionary<string,object> scaled;do{scaled=Scale(root,"Disable",3440,1440,100);if(EngineJson.Bool(scaled,"Ready"))break;Thread.Sleep(250);}while(DateTime.Now<deadline);if(!EngineJson.Bool(scaled,"Ready"))throw new Exception("Return to the campaign to finish restoring the previous HUD size.");}
   switch(action) {
    case "EnableResolution":Print(Resolution(root,"Install",w,h));break;
    case "DisableResolution":Print(Resolution(root,"Disable",w,h));break;
    case "EnableHUD":Print(Hud(root,"Enable",w,h));break;
    case "Inspect":Print(Resolution(root,"Status",w,h));Print(Hud(root,"Status",w,h));Print(Scale(root,"Status",3440,1440,100));break;
    case "RestoreHUD":
     Print(Hud(root,"Disable",w,h));Console.WriteLine("Return to the mission to allow restoration. Waiting up to two minutes for the layout callback...");DateTime until=DateTime.Now.AddMinutes(2);
     do{Thread.Sleep(2000);var status=Hud(root,"Status",w,h);if(status==null)return 0;if(!EngineJson.Bool(status,"Attached")){Console.WriteLine("Original HUD restored.");return 0;}}while(DateTime.Now<until);
     throw new Exception("Restoration is queued. Return to the mission, then try Restore original HUD again.");
    default:throw new ArgumentException("Unknown control action: "+action);
   }return 0;
  }catch(Exception e){Console.Error.WriteLine(e.Message);return 1;}
 }
}
