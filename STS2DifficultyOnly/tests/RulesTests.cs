using System;
using System.Collections.Generic;
using System.Text.Json;
using MultiplayerExpanded.Features.Difficulty;
using MultiplayerExpanded.Features.Settings;
class RulesTests {
 static int checks;
 static void Eq<T>(T got,T want,string label) { checks++;if(!EqualityComparer<T>.Default.Equals(got,want))throw new Exception($"{label}: got {got}, expected {want}"); }
 static void Reject(string json) {checks++;try{SettingsPayload.Parse(json);}catch(Exception){return;}throw new Exception("Invalid payload accepted: "+json);}
 static void Main() {
  for(int mask=0;mask<128;mask++)for(int players=1;players<=4;players++)foreach(var mode in Enum.GetValues<MultiplayerDeathMode>()) {
   bool boss=(mask&1)!=0,reworks=(mask&2)!=0,damage=(mask&4)!=0,scaling=(mask&8)!=0;
   bool cards=(mask&16)!=0,potions=(mask&32)!=0,events=(mask&64)!=0;
   Eq(DifficultyRules.CombineBosses(boss,9),false,$"A9 mask {mask}");
   Eq(DifficultyRules.CombineBosses(boss,10),boss,$"A10 mask {mask}");
   Eq(DifficultyRules.CombineBosses(boss,11),boss,$"A11 mask {mask}");
   Eq(DifficultyRules.Scale(1.5m,players,scaling),scaling&&players>=3 ? (players==3?2m:2.5m):1.5m,$"Scaling {players} mask {mask}");
   Eq(DifficultyRules.BaseDamage(10m,players,damage),damage&&players>=3?(players==3?11m:13m):10m,$"Damage {players} mask {mask}");
   var values=new Dictionary<string,object>{{"SurroundedByBosses",boss},{"EnemyReworks",reworks},{"EnemyDamageCompensation",damage},{"IncreasedEnemyScaling",scaling},{"VanillaCardReworks",cards},{"MultiplayerPotionChanges",potions},{"MultiplayerEvents",events},{"DeathMode",mode}};
   var decoded=SettingsPayload.Parse(JsonSerializer.Serialize(values));
   foreach(var name in SettingsPayload.Names)Eq(decoded[name],values[name],"Payload roundtrip "+name);
  }
  var valid=new Dictionary<string,object>{{"SurroundedByBosses",true},{"EnemyReworks",true},{"EnemyDamageCompensation",false},{"IncreasedEnemyScaling",false},{"VanillaCardReworks",true},{"MultiplayerPotionChanges",true},{"MultiplayerEvents",true},{"DeathMode",MultiplayerDeathMode.Vanilla}};
  foreach(var name in new[]{"VanillaCardReworks","MultiplayerPotionChanges","MultiplayerEvents"}) {
   var bad=new Dictionary<string,object>(valid);bad.Remove(name);Reject(JsonSerializer.Serialize(bad));
   bad=new Dictionary<string,object>(valid);bad[name]="true";Reject(JsonSerializer.Serialize(bad));
   var json=JsonSerializer.Serialize(valid);Reject(json[..^1]+",\""+name+"\":false}");
  }
  foreach(var selected in Enum.GetValues<MultiplayerDeathMode>())foreach(var requested in Enum.GetValues<MultiplayerDeathMode>())
  for(int players=1;players<=4;players++)foreach(var multiplayer in new[]{false,true})
   Eq(DeathModeRules.IsActive(selected,requested,players,multiplayer), requested!=MultiplayerDeathMode.Vanilla && selected==requested && players>1 && multiplayer,"Death activation and Vanilla isolation");
  Eq((int)MultiplayerDeathMode.Vanilla,0,"Default enum value is Vanilla");
  Eq(DifficultyRules.BaseDamage(5m,3,true),6m,"3P half rounds away");
  Eq(DifficultyRules.BaseDamage(2m,4,true),3m,"4P half rounds away");
  Eq(DifficultyRules.BaseDamage(0m,4,true),0m,"Zero damage");
  Eq(DifficultyRules.BaseDamage(-5m,4,true),-5m,"Nonpositive damage");
  Eq(DifficultyRules.BaseDamage(10m,3,true)+3m,14m,"Base compensation before Strength");
  Eq(DifficultyRules.Scale(1m,4,true),1m,"No base scaling");
  Reject("{}");Reject("null");Reject("{\"EnemyReworks\":true}");Reject("[]");Reject(new string('x',1025));
  Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":true,\"EnemyDamageCompensation\":true,\"Unexpected\":true,\"DeathMode\":0}");
  Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":\"true\",\"EnemyDamageCompensation\":true,\"IncreasedEnemyScaling\":true,\"DeathMode\":0}");
  Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":true,\"EnemyDamageCompensation\":true,\"IncreasedEnemyScaling\":true,\"EnemyReworks\":false,\"DeathMode\":0}");
  foreach(var invalid in new[]{"-1","4","1.5","\"VanillaPlus\"","null","true"})
   Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":true,\"EnemyDamageCompensation\":true,\"IncreasedEnemyScaling\":true,\"DeathMode\":"+invalid+"}");
  Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":true,\"EnemyDamageCompensation\":true,\"IncreasedEnemyScaling\":true}");
  Reject("{\"SurroundedByBosses\":true,\"EnemyReworks\":true,\"EnemyDamageCompensation\":true,\"IncreasedEnemyScaling\":true,\"DeathMode\":0,\"DeathMode\":1}");
  Console.WriteLine($"PASS: {checks} assertions; all 128 difficulty/content masks at 1–4 players across all 4 death modes; A9/A10/A11; damage/scaling; atomic payload schema; Vanilla/solo isolation.");
 }
}
