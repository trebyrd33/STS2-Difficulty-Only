#!/usr/bin/env python3
"""Offline .NET 9 compiler fallback; normal development can use dotnet build."""
from pathlib import Path
import os, re, shutil, subprocess, sys
root=Path(__file__).resolve().parent
exe=os.environ.get('DOTNET_EXE') or shutil.which('dotnet')
if not exe: raise SystemExit('Install the .NET 9 SDK or set DOTNET_EXE to its dotnet executable.')
installed=subprocess.check_output([exe,'--list-sdks'],text=True)
choices=[]
for line in installed.splitlines():
 m=re.match(r'(9\.\d+\.\d+) \[(.+)\]',line)
 if m:choices.append((tuple(map(int,m[1].split('.'))),Path(m[2])/m[1]))
if not choices:raise SystemExit('A .NET 9 SDK is required.')
sdk=max(choices)[1]
packs=list((sdk.parent.parent/'packs/Microsoft.NETCore.App.Ref').glob('9.*/ref/net9.0'))
if not packs:raise SystemExit('The .NET 9 reference pack is missing.')
pack=max(packs,key=lambda p:tuple(map(int,p.parent.parent.name.split('.'))))
refs=list(pack.glob('*.dll'))
for name in ['sts2.dll','GodotSharp.dll','0Harmony.dll','BaseLib.dll']:
 p=root/'references'/name
 if not p.is_file():raise SystemExit('Missing supplied reference: '+str(p))
 refs.append(p)
out=root/'bin/Release/net9.0';out.mkdir(parents=True,exist_ok=True)
args=['-nologo','-target:library','-langversion:13','-nullable:enable','-deterministic+', '-nowarn:8600,8601,8602,8603,8604,8618,8625,8620,8622','-out:'+str(out/'STS2DifficultyOnly.dll')]+['-r:'+str(p) for p in refs]+[str(p) for p in sorted((root/'src').rglob('*.cs'))]
rsp=root/'bin/compiler.rsp';rsp.write_text('\n'.join('"'+arg+'"' for arg in args))
r=subprocess.run([exe,str(sdk/'Roslyn/bincore/csc.dll'),'@'+str(rsp)],capture_output=True,text=True)
(root/'bin/build.log').write_text(r.stdout+r.stderr)
print(r.stdout+r.stderr,end='')
if r.returncode:raise SystemExit(r.returncode)
print('Compiled:',out/'STS2DifficultyOnly.dll')
