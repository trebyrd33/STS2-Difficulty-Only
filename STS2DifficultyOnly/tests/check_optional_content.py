from pathlib import Path
import re,json
r=Path(__file__).resolve().parents[1];s=r/'src';a=r/'assets/STS2DifficultyOnly';checks=0
def require(value,message):
 global checks
 checks+=1
 if not value:raise AssertionError(message)
def slug(n):return re.sub(r'(?<!^)(?=[A-Z])','_',n).upper()
config=(s/'MultiplayerExpanded.Features.Settings/MultiplayerExpandedConfig.cs').read_text();schema=(s/'MultiplayerExpanded.Features.Settings/SettingsPayload.cs').read_text()
loc=json.loads((a/'localization/eng/settings_ui.json').read_text())
for name in ['VanillaCardReworks','MultiplayerPotionChanges','MultiplayerEvents']:
 require('bool '+name+' { get; set; } = true;' in config,'Default/config missing '+name)
 require(name in schema,'Not synchronized '+name)
 for suffix in ['title','description']:require('STS2DIFFICULTYONLY-'+slug(name)+'.'+suffix in loc,'Missing setting label '+name)
reworks=(s/'MultiplayerExpanded.Features.Reworks/ReworkPatches.cs').read_text()
require('MultiplayerExpandedConfig.VanillaCardReworks && IsMultiplayerRun()' in reworks,'Missing card execution gate')
require('if (!MultiplayerExpandedConfig.VanillaCardReworks) return false;' in reworks,'Missing card preview gate')
for name in ['CardPotionOnUsePatch','CardPotionTargetTypePatch']:
 text=(s/'MultiplayerExpanded.Patches'/(name+'.cs')).read_text();require('MultiplayerExpandedConfig.MultiplayerPotionChanges && MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMultiplayerRun()' in text,'Potion/card gates coupled');require('ReworkPatches.IsMp()' not in text,'Potion depends on card toggle')
require('if (!MultiplayerExpandedConfig.MultiplayerPotionChanges) return;' in (s/'MultiplayerExpanded.Patches/PotionContainerShrinkPatch.cs').read_text(),'Ungated potion UI')
require('MultiplayerExpandedConfig.VanillaCardReworks && (int)multiplayerConstraint == 1' in (s/'MultiplayerExpanded.Patches/DisableVanillaCardsInMultiplayer.cs').read_text(),'Ungated AllForOne exclusion')
require('if (!MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMp()) return null;' in (s/'MultiplayerExpanded.Patches/InterceptMpRedirectDamagePatch.cs').read_text(),'Ungated redirection')
require('MultiplayerExpandedConfig.MultiplayerEvents && runState != null' in (s/'MultiplayerExpanded.Events/MultiplayerExpandedEvent.cs').read_text(),'Ungated events')
expected={'AnEmptyChest','FadingMemories','GuardianAngel','HammeringLesson','LabyrinthOfMirrors'}
found=set()
eloc=json.loads((a/'localization/eng/events.json').read_text())
for p in (s/'MultiplayerExpanded.Events').glob('*.cs'):
 text=p.read_text();m=re.search(r'public sealed class (\w+)',text)
 if not m:continue
 name=m[1];found.add(name);key='MULTIPLAYEREXPANDED-'+slug(name)
 require((a/'images/events'/(slug(name).lower()+'.tres')).exists(),'Missing event portrait')
 require(key+'.title' in eloc,'Missing event title')
 require(key+'.pages.INITIAL.description' in eloc,'Missing initial page')
 require('override bool IsAllowed' not in text or 'base.IsAllowed(runState)' in text,'Bypassed toggle eligibility')
 for page in re.findall(r'PageDescription\("([A-Z_]+)"\)',text):require(key+'.pages.'+page+'.description' in eloc,'Missing event page '+page)
 for method in re.findall(r'Option\(\(Func<Task>\)(\w+)',text):
  for suffix in ['title','description']:require(key+'.pages.INITIAL.options.'+slug(method)+'.'+suffix in eloc,'Missing option '+method)
 require('((EventModel)this).CalculateVars()' not in text and '((EventModel)this).GetAssetPaths(runState)' not in text,'Recursive decompiler virtual call')
require(found==expected,'Unexpected event inventory')
relics=[p.stem for p in (s/'MultiplayerExpanded.Relics').glob('*.cs') if 'public sealed class' in p.read_text()];require(relics==['ALingeringSomething'],'Unexpected extra relics')
for n in ['Immortal','Improvements']:
 t=(s/'MultiplayerExpanded.Cards.Colorless'/(n+'.cs')).read_text();require('Pool(typeof(EventCardPool))' in t and 'CanBeGeneratedInCombat => false' in t,'Event reward escaped event-only eligibility')
for suffix in ['', '_outline']:require((a/'images/relics'/('a_lingering_something'+suffix+'.tres')).exists(),'Missing relic icon')
print(f'OPTIONAL CONTENT: {checks} static checks passed; 3 independent synchronized settings, 5 gated events, 2 event-only cards, 1 event relic, and event text/assets present. Runtime tests pending.')
