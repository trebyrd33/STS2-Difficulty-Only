from pathlib import Path
import re,json
r=Path(__file__).resolve().parents[1];s=r/'src';o=Path(__import__('os').environ.get('MPX_ORIGINAL_SOURCE',str(r.parent/'original-source')));checks=0
def require(x,m):
 global checks
 checks+=1
 if not x:raise AssertionError(m)
def registry(root):
 return {p.stem:p for p in root.glob('MultiplayerExpanded.Cards*/*.cs') if re.search(r'class \w+\s*:\s*CardRework',p.read_text())}
a,b=registry(o),registry(s);require(a.keys()==b.keys(),'Rework inventory differs from original');require(len(b)==52,'Expected 52')
loc=json.loads((r/'assets/STS2DifficultyOnly/localization/eng/cards.json').read_text())
for name,p in b.items():
 t=p.read_text();orig=a[name].read_text();slug=re.sub(r'(?<!^)(?=[A-Z])','_',name).upper();key='MULTIPLAYEREXPANDED-'+slug
 for suff in ['title','description','descriptionUpgraded']:require(key+'.'+suff in loc,'Missing '+key+'.'+suff)
 for attr in ['BetaOnly','StableOnly']:require(('['+attr+']' in t)==('['+attr+']' in orig),'Changed branch '+name)
 require(re.search(r'BaseCard => typeof\((\w+)\)',t)[1]==re.search(r'BaseCard => typeof\((\w+)\)',orig)[1],'Changed base '+name)
require(sum('[BetaOnly]' in p.read_text() for p in b.values())==9,'Beta count')
require(sum('[StableOnly]' in p.read_text() for p in b.values())==1,'Stable count')
for rel in ['MultiplayerExpanded.Cards.Colorless/BurnMpPileChangePatch.cs','MultiplayerExpanded.Cards.Colorless/BurnMpTurnStartPatch.cs','MultiplayerExpanded.Cards.Ironclad/MidnightMpEnteredCombatPatch.cs','MultiplayerExpanded.Cards.Ironclad/MidnightMpExhaustedPatch.cs']:
 require((s/rel).exists(),'Missing lifecycle hook '+rel)
for n in ['Description','Title','Type','TargetType','Rarity','EnergyCost']:
 t=(s/'MultiplayerExpanded.Features.Reworks'/('Rework'+n+'Patch.cs')).read_text();require('MethodType.Getter' in t,'Unresolved getter patch '+n)
for p in (s/'MultiplayerExpanded.Powers.Colorless').glob('*.cs'):
 require('((PowerModel)this).Description' not in p.read_text(),'Recursive virtual description '+str(p))
main=(s/'MainFile.cs').read_text();require('ReworkPatches.Install(harmony)' in main,'Reworks not installed');require('CardPotionOnUsePatch.Apply(harmony)' in main,'Potion patch not applied')
potion=(s/'MultiplayerExpanded.Patches/CardPotionOnUsePatch.cs').read_text();require('val2, (PileType)2, val,' in potion,'Potion recipient mismatch');require('MultiplayerExpandedConfig.MultiplayerPotionChanges && MultiplayerExpanded.Features.Reworks.ReworkPatches.IsMultiplayerRun()' in potion,'Independent potion gate missing')
require(not list((s/'MultiplayerExpanded.Features.ApprovedCurses').glob('Decay*.cs')),'Duplicate Decay patches')
print(f'REWORK COVERAGE: {checks} checks passed; original 52 definitions retained (42 shared, 9 beta-only, 1 stable-only), text complete, lifecycle hooks included, potion ownership/gating checked. Static verification only.')
