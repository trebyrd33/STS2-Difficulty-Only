"""Static coverage between retained C# model names and replacement resources."""
from pathlib import Path
import json,re
root=Path(__file__).resolve().parents[1];src=root/'src';assets=root/'assets/STS2DifficultyOnly';checks=0

def require(condition, message):
 global checks
 checks+=1
 if not condition:raise AssertionError(message)
def slug(name):return re.sub(r'(?<!^)(?=[A-Z])','_',name).upper()
loc={p.stem:json.loads(p.read_text()) for p in (assets/'localization/eng').glob('*.json')}
for folder in ['MultiplayerExpanded.Powers.Monsters','MultiplayerExpanded.Powers.Colorless','MultiplayerExpanded.Powers.Regent','MultiplayerExpanded.Powers.Silent','MultiplayerExpanded.Features.Columns']:
 for p in (src/folder).glob('*.cs'):
  m=re.search(r'public sealed class (\w+)\s*:\s*(?:MultiplayerExpandedPower|ColumnEnemyPower|DelayedSelfDamagePower)',p.read_text())
  if not m:continue
  name=m[1];prefix='MULTIPLAYEREXPANDED-'+slug(name)
  for suffix in ['.title','.description']:require(prefix+suffix in loc['powers'],name+' missing '+suffix)
  require((assets/'images/powers'/(slug(name).lower()+'.tres')).is_file(),name+' missing icon')
for p in (src/'MultiplayerExpanded.Features.Intents').glob('*.cs'):
 m=re.search(r'public sealed class (\w+)\s*:\s*(?:Targeted\w*Intent|DeathBlowIntent)',p.read_text())
 if not m:continue
 prefix='MULTIPLAYEREXPANDED-'+slug(m[1])
 for suffix in ['.title','.description']:require(prefix+suffix in loc['intents'],prefix+' missing '+suffix)
for p in (src/'MultiplayerExpanded.Features.DoubleBoss').glob('*.cs'):
 m=re.search(r'public sealed class (\w+)\s*:\s*DoubleBossEncounter',p.read_text())
 if not m:continue
 prefix='MULTIPLAYEREXPANDED-'+slug(m[1])
 for suffix in ['.title','.loss']:require(prefix+suffix in loc['encounters'],prefix+' missing '+suffix)
 for path in re.findall(r'"res://STS2DifficultyOnly/([^"\n]+)"',p.read_text()):require((assets/path).is_file(),'Missing '+path)
cards=[]
for folder in ['MultiplayerExpanded.Cards.Colorless','MultiplayerExpanded.Cards.Tokens','MultiplayerExpanded.Cards.Curses']:
 for p in (src/folder).glob('*.cs'):
  m=re.search(r'public sealed class (\w+)\s*:\s*(?:TokenCard|CurseCard|ColorlessCard)',p.read_text())
  if not m:continue
  name=m[1];cards.append(name);prefix='MULTIPLAYEREXPANDED-'+slug(name)
  for suffix in ['.title','.description']:require(prefix+suffix in loc['cards'],prefix+' missing '+suffix)
  require((assets/'images/card_portraits'/(slug(name).lower()+'.tres')).is_file(),name+' missing portrait')
  variables=set(re.findall(r'PowerVar<(\w+)>',p.read_text())) | set(re.findall(r'new IntVar\("([^"]+)"',p.read_text()))
  tokens=set(re.findall(r'\{(\w+)\}',loc['cards'][prefix+'.description']))
  require(tokens <= variables,name+' has undefined card description variables: '+str(tokens-variables))
require(set(cards)=={'Haze','Hoarding','Frustration','InnerWorld','ColdFeet','Martyrdom','FranticPull','Sloth','Gluttony','Envy','NeowSClutch','Vogelfrei','Immortal','Improvements'},'Unexpected extra card classes')
for name in ['SurroundedByBosses','EnemyReworks','EnemyDamageCompensation','IncreasedEnemyScaling']:
 require('STS2DIFFICULTYONLY-'+slug(name)+'.title' in loc['settings_ui'],'Missing setting label '+name)
for p in src.rglob('*.cs'):
 text=p.read_text()
 require('res://MultiplayerExpanded' not in text, 'Original pack dependency in '+str(p))
 require(not re.search(r'public\s+(?:sealed\s+)?class\s+\w+\s*:\s*(?:CustomCharacterModel)',text),'Unexpected character class '+str(p))
# Scope and mode isolation are explicit parts of this build.
require(not (src/'MultiplayerExpanded.Cards.Curses/Loneliness.cs').exists(),'Loneliness was restored')
require(not (assets/'images/card_portraits/loneliness.tres').exists(),'Unexpected Loneliness asset')
require('MULTIPLAYEREXPANDED-LONELINESS.title' not in loc['cards'],'Unexpected Loneliness localization')
require((assets/'images/ui/rest_site/option_revive_all.tres').is_file(),'Missing Revive All icon')
for key in ['OPTION_REVIVE_ALL.name','OPTION_REVIVE_ALL.description']:
 require(key in loc['rest_site_ui'],'Missing rest-site label '+key)
for mode in ['Vanilla','VanillaPlus','EveryoneDies','StayDeadUntilMended']:
 require('STS2DIFFICULTYONLY-DEATH_MODE.'+mode in loc['settings_ui'],'Missing death-mode label '+mode)
require('= MultiplayerDeathMode.Vanilla;' in (src/'MultiplayerExpanded.Features.Settings/MultiplayerExpandedConfig.cs').read_text(),'Default death mode changed')
for name in ['DeadSeatDispatchPatch','DeadSeatEventCombatPatch','DeadSeatEventCombatBetaPatch']:
 require('DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended' in (src/'MultiplayerExpanded.Features.DeathMechanic'/(name+'.cs')).read_text(),'Ungated dead-seat patch '+name)
require('DeathMechanicGate.IsActive(MultiplayerDeathMode.StayDeadUntilMended' in (src/'MultiplayerExpanded.Features.DeathMechanic/DeadSeatVotes.cs').read_text(),'Ungated shared event voting')
require('Loneliness' not in ''.join(p.read_text() for p in (src/'MultiplayerExpanded.Cards.Curses').glob('*.cs')),'A curse depends on Loneliness')
# The three formerly recursive getters must call their base implementations.
for f,needle in [('GasBombTargetPower.cs','return base.Title;'),('EnrageContributionPower.cs','return base.Title;'),('CounteredPower.cs','LocString description = base.Description;')]:
 p=next(src.rglob(f));require(needle in p.read_text(),'Virtual getter recursion regression in '+f)
print(f'ASSET COVERAGE: {checks} checks passed; all retained models have resources; 7 approved encounter cards, 5 multiplayer curses excluding Loneliness, and 2 event rewards; no old pack paths.')
